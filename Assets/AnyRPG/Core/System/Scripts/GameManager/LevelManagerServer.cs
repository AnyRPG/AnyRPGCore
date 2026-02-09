using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static SceneLoader;

namespace AnyRPG {
    public class LevelManagerServer : ConfiguredClass {

        public event System.Action<int, string> OnRemoveLoadedScene = delegate { };
        public event System.Action<int, SceneData> OnAddLoadedScene = delegate { };
        public event System.Action<int, int> OnSetSceneClientCount = delegate { };

        // dictionary of loaded scenes, where the key is the scene name and the value is a list of scene handles
        private Dictionary<string, Dictionary<int, SceneData>> loadedScenes = new Dictionary<string, Dictionary<int, SceneData>>();

        /// <summary>
        /// hashCode, SceneInstanceType
        /// </summary>
        private Dictionary<int, SceneInstanceType> sceneLoadRequestHashCodes = new Dictionary<int, SceneInstanceType>();

        private Coroutine emptySceneCleanupRoutine = null;

        // game manager references
        private CameraManager cameraManager = null;
        private LevelManager levelManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            SceneManager.sceneUnloaded += HandleSceneUnloaded;
            networkManagerServer.OnStartServer += HandleStartServer;
            networkManagerServer.OnStopServer += HandleStopServer;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            cameraManager = systemGameManager.CameraManager;
            levelManager = systemGameManager.LevelManager;
        }

        private void HandleStartServer() {
            if (emptySceneCleanupRoutine == null) {
                emptySceneCleanupRoutine = systemGameManager.StartCoroutine(CleanupEmptyScenes());
            }
        }

        private void HandleStopServer() {
            if (emptySceneCleanupRoutine != null) {
                systemGameManager.StopCoroutine(emptySceneCleanupRoutine);
            }
        }

        private IEnumerator CleanupEmptyScenes() {
            //Debug.Log($"LevelManagerServer.CleanupEmptyScenes()");

            float timeoutMinutes = 0f;
            while (networkManagerServer.ServerModeActive == true) {
                //Debug.Log($"LevelManagerServer.CleanupEmptyScenes()");
                foreach (Dictionary<int, SceneData> sceneDictionary in loadedScenes.Values) {
                    foreach (SceneData sceneData in sceneDictionary.Values) {
                        if (sceneData.ClientCount > 0) {
                            continue;
                        }
                        if (sceneData.SceneInstanceType == SceneInstanceType.Personal) {
                            timeoutMinutes = systemConfigurationManager.PersonalInstanceTimeout;
                        } else if (sceneData.SceneInstanceType == SceneInstanceType.LobbyGame) {
                            timeoutMinutes = systemConfigurationManager.LobbyGameInstanceTimeout;
                        } else if (sceneData.SceneInstanceType == SceneInstanceType.Group) {
                            timeoutMinutes = systemConfigurationManager.GroupInstanceTimeout;
                        } else {
                            timeoutMinutes = systemConfigurationManager.WorldInstanceTimeout;
                        }
                        if (timeoutMinutes == -1f) {
                            continue;
                        }
                        if (IsTimeExpired(sceneData.EmptyTime, timeoutMinutes) == true) {
                            networkManagerServer.UnloadScene(sceneData.Scene.handle);
                        }
                    }
                }
                yield return new WaitForSecondsRealtime(10);
            }
        }

        public bool IsTimeExpired(DateTime timeToCheck, float timeoutMinutes) {
            // Subtract the timeToCheck from the current time
            TimeSpan difference = DateTime.Now - timeToCheck;

            // Return true if the total minutes elapsed is greater than our limit
            return difference.TotalMinutes >= timeoutMinutes;
        }

        public void HandleSceneUnloaded(Scene scene) {
            //Debug.Log($"LevelManagerServer.HandleSceneUnloaded({scene.name})");

            // if the scene is not in the loaded scenes, then we don't need to do anything
            if (loadedScenes.ContainsKey(scene.name) == false) {
                //Debug.LogWarning($"LevelManagerServer.HandleSceneUnloaded() - scene {scene.name} not found in loaded scenes");
                return;
            }
            // remove the scene from the loaded scenes
            RemoveLoadedScene(scene.handle, scene.name);

            // if there are no more handles for this scene, then remove it from the dictionary
            if (loadedScenes[scene.name].Count == 0) {
                loadedScenes.Remove(scene.name);
            }

            networkManagerServer.HandleSceneUnloadEnd(scene.handle, scene.name);
        }

        public void AddLoadedScene(int hashCode, Scene scene) {
            //Debug.Log($"LevelManagerServer.AddLoadedScene(hashCode: {hashCode}, scene: ({scene.name} {scene.handle}))");

            SceneInstanceType sceneInstanceType = SceneInstanceType.World;
            if (sceneLoadRequestHashCodes.ContainsKey(hashCode) == true) {
                //Debug.Log($"NetworkManagerServer.HandleSceneLoadEnd({scene.name}, {loadRequestHashCode}) - character group load request");
                sceneInstanceType = sceneLoadRequestHashCodes[hashCode];
                sceneLoadRequestHashCodes.Remove(hashCode);
            }

            if (loadedScenes.ContainsKey(scene.name) == false) {
                loadedScenes.Add(scene.name, new Dictionary<int, SceneData>());
            }
            SceneData sceneData = new SceneData(sceneInstanceType, scene, levelManager.GetSceneNodeBySceneName(scene.name));
            loadedScenes[scene.name].Add(scene.handle, sceneData);
            OnAddLoadedScene(scene.handle, sceneData);
        }

        public void RemoveLoadedScene(int sceneHandle, string sceneName) {
            //Debug.Log($"LevelManagerServer.RemoveLoadedScene({sceneHandle}, {sceneName})");

            if (loadedScenes.ContainsKey(sceneName) == false) {
                //Debug.LogError($"LevelManagerServer.RemoveLoadedScene() - scene {sceneName} not found in loaded scenes");
                return;
            }

            loadedScenes[sceneName].Remove(sceneHandle);
            OnRemoveLoadedScene(sceneHandle, sceneName);
        }

        public void ProcessLevelLoad(Scene loadedScene)  {
            //Debug.Log($"LevelManagerServer.ProcessLevelLoad({loadedScene.name}({loadedScene.handle}))");

            cameraManager.ActivateMainCamera();
            systemGameManager.AutoConfigureMonoBehaviours(loadedScene);

        }

        public void SetSceneLoadRequestHashCode(SceneInstanceType sceneInstanceType, int hashCode) {
            //Debug.Log($"LevelManagerServer.SetSceneLoadRequestHashCode(sceneInstanceType: {sceneInstanceType.ToString()}, hashCode: {hashCode})");

            if (sceneLoadRequestHashCodes.ContainsKey(hashCode) == false) {
                sceneLoadRequestHashCodes.Add(hashCode, sceneInstanceType);
            }
        }

        public void SetSceneClientCount(string sceneName, int handle, int clientCount) {
            //Debug.Log($"LevelManagerServer.SetSceneClientCount(sceneName: {sceneName}, handle: {handle}, clientCount: {clientCount})");

            if (loadedScenes.ContainsKey(sceneName) == false) {
                return;
            }
            if (loadedScenes[sceneName].ContainsKey(handle) == false) {
                return;
            }
            loadedScenes[sceneName][handle].EmptyTime = DateTime.Now;
            loadedScenes[sceneName][handle].ClientCount = clientCount;
            OnSetSceneClientCount(handle, clientCount);
        }

        public Dictionary<int, SceneData> GetLoadedSceneData() { 
            Dictionary<int, SceneData> returnValue = new Dictionary<int, SceneData>();
            foreach (Dictionary<int, SceneData> sceneDataDictionary in loadedScenes.Values) {
                foreach (KeyValuePair<int, SceneData> kvp in sceneDataDictionary) {
                    if (returnValue.ContainsKey(kvp.Key) == false) {
                        returnValue.Add(kvp.Key, kvp.Value);
                    }
                }
            }
            return returnValue;
        }
    }

}