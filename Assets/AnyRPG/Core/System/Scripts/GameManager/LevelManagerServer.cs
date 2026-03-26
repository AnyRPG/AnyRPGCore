using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    public class LevelManagerServer : ConfiguredClass {

        public event System.Action<int, string> OnRemoveLoadedScene = delegate { };
        public event System.Action<int, SceneData> OnAddLoadedScene = delegate { };
        public event System.Action<int, int> OnSetSceneClientCount = delegate { };
        public event System.Action<int, string> OnBeforeStartUnloadScene = delegate { };

        // dictionary of loaded scenes, where the key is the scene name and the value is a list of scene handles
        private Dictionary<string, Dictionary<int, SceneData>> loadedScenes = new Dictionary<string, Dictionary<int, SceneData>>();

        /// <summary>
        /// hashCode, SceneInstanceType
        /// </summary>
        private Dictionary<int, SceneInstanceType> sceneLoadRequestHashCodes = new Dictionary<int, SceneInstanceType>();

        private Coroutine emptySceneCleanupRoutine = null;

        // game manager references
        private CameraManager cameraManager = null;
        private SceneUtilityService sceneUtilityService = null;
        private SystemEventManager systemEventManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            SceneManager.sceneUnloaded += HandleSceneUnloaded;
            networkManagerServer.OnStartServer += HandleStartServer;
            networkManagerServer.OnStopServer += HandleStopServer;
            systemEventManager.OnPlayerUnitSpawn -= HandlePlayerUnitSpawn;
            systemEventManager.OnReputationChange += HandleReputationChange;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            cameraManager = systemGameManager.CameraManager;
            sceneUtilityService = systemGameManager.SceneUtilityService;
            systemEventManager = systemGameManager.SystemEventManager;
        }

        private void HandlePlayerUnitSpawn(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.Interactable.HandlePlayerUnitSpawn({sourceUnitController?.gameObject.name})");
            
            // the event that triggers this is only called on clients so the next steps are safe

            SceneData sceneData = GetSceneData(sourceUnitController.gameObject.scene);
            if (sceneData == null) {
                return;
            }
            foreach (Interactable interactable in sceneData.Interactables) {
                if (interactable != null) {
                    interactable.ProcessPlayerUnitSpawn(sourceUnitController);
                }
            }
            foreach (UnitController unitController in sceneData.UnitControllers) {
                if (unitController != null && unitController != sourceUnitController) {
                    unitController.ProcessPlayerUnitSpawn(sourceUnitController);
                }
            }
        }

        private void HandleReputationChange(UnitController sourceUnitController) {
            // the event that triggers this is only called on clients so the next steps are safe
            SceneData sceneData = GetSceneData(sourceUnitController.gameObject.scene);
            if (sceneData == null) {
                return;
            }
            foreach (UnitController unitController in sceneData.UnitControllers) {
                if (unitController != null && unitController != sourceUnitController) {
                    unitController.HandleReputationChange(sourceUnitController);
                }
            }

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
                            ProcessBeforeUnloadScene(sceneData.Scene);
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
            cameraManager.DisableCutsceneCameras(scene);
            AddLoadedScene(scene, sceneInstanceType);
        }

        public void AddLoadedScene(Scene scene, SceneInstanceType sceneInstanceType) {
            //Debug.Log($"LevelManagerServer.AddLoadedScene(scene: ({scene.name} {scene.handle}), sceneInstanceType: {sceneInstanceType.ToString()})");

            if (loadedScenes.ContainsKey(scene.name) == false) {
                loadedScenes.Add(scene.name, new Dictionary<int, SceneData>());
            }
            SceneData sceneData = new SceneData(sceneInstanceType, scene, sceneUtilityService.GetSceneNodeBySceneName(scene.name), SceneUtilityService.DoesSceneHaveNavMesh(scene));
            loadedScenes[scene.name].Add(scene.handle, sceneData);

            systemGameManager.AutoConfigureMonoBehaviours(scene);

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

        public void ProcessLevelLoad(Scene loadedScene) {
            //Debug.Log($"LevelManagerServer.ProcessLevelLoad({loadedScene.name}({loadedScene.handle}))");

            cameraManager.ActivateMainCamera();
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

        public SceneData GetSceneData(Scene scene) {
            if (loadedScenes.ContainsKey(scene.name) == false) {
                return null;
            }
            if (loadedScenes[scene.name].ContainsKey(scene.handle) == false) {
                return null;
            }
            return loadedScenes[scene.name][scene.handle];
        }

        public void ProcessBeforeUnloadScene(Scene scene) {
            Debug.Log($"LevelManagerServer.ProcessBeforeUnloadScene({scene.name} ({scene.handle}))");

            SceneData sceneData = GetSceneData(scene);
            if (sceneData != null) {
                List<Interactable> interactables = new List<Interactable>(sceneData.Interactables);
                foreach (Interactable interactable in interactables) {
                    if (interactable != null) {
                        interactable.ResetSettings();
                    }
                }
                List<UnitController> unitControllers = new List<UnitController>(sceneData.UnitControllers);
                foreach (UnitController unitController in unitControllers) {
                    if (unitController != null) {
                        unitController.Despawn(0f, false, true);
                    }
                }
            }

            OnBeforeStartUnloadScene(scene.handle, scene.name);
        }


        public void RegisterInteractable(Interactable interactable) {
            //Debug.Log($"LevelManagerServer.RegisterInteractable({interactable.gameObject.name})");
            Scene scene = interactable.gameObject.scene;
            if (loadedScenes.ContainsKey(scene.name) == false) {
                return;
            }
            if (loadedScenes[scene.name].ContainsKey(scene.handle) == false) {
                return;
            }
            loadedScenes[scene.name][scene.handle].RegisterInteractable(interactable);
        }

        public void UnregisterInteractable(Interactable interactable) {
            //Debug.Log($"LevelManagerServer.UnregisterInteractable({interactable.gameObject.name})");
            Scene scene = interactable.gameObject.scene;
            if (loadedScenes.ContainsKey(scene.name) == false) {
                return;
            }
            if (loadedScenes[scene.name].ContainsKey(scene.handle) == false) {
                return;
            }
            loadedScenes[scene.name][scene.handle].UnregisterInteractable(interactable);
        }

        public void RegisterUnitController(UnitController unitController) {
            //Debug.Log($"LevelManagerServer.RegisterUnitController({unitController.gameObject.name})");
            Scene scene = unitController.gameObject.scene;
            if (loadedScenes.ContainsKey(scene.name) == false) {
                return;
            }
            if (loadedScenes[scene.name].ContainsKey(scene.handle) == false) {
                return;
            }
            loadedScenes[scene.name][scene.handle].RegisterUnitController(unitController);
        }

        public void UnregisterUnitController(UnitController unitController) {
            //Debug.Log($"LevelManagerServer.UnregisterUnitController({unitController.gameObject.name})");
            Scene scene = unitController.gameObject.scene;
            if (loadedScenes.ContainsKey(scene.name) == false) {
                return;
            }
            if (loadedScenes[scene.name].ContainsKey(scene.handle) == false) {
                return;
            }
            loadedScenes[scene.name][scene.handle].UnregisterUnitController(unitController);
        }

        public void ProcessBeforeStopServer() {
                foreach (Dictionary<int, SceneData> sceneDictionary in loadedScenes.Values) {
                    foreach (SceneData sceneData in sceneDictionary.Values) {
                        ProcessBeforeUnloadScene(sceneData.Scene);
                    }
            }
        }
    }

}