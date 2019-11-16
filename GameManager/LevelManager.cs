using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace AnyRPG {
    public class LevelManager : MonoBehaviour {

        #region Singleton
        private static LevelManager instance;

        public static LevelManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<LevelManager>();
                }
                return instance;
            }
        }

        #endregion

        [SerializeField]
        private string defaultStartingZone = string.Empty;

        [SerializeField]
        private string initializationScene = "LoadGameManager";

        [SerializeField]
        private string mainMenuScene = "MainMenu";

        [SerializeField]
        private string characterCreatorScene = "CharacterCreator";

        // whether to start the game with the character creator or just go straight into the game as a soul
        [SerializeField]
        private bool loadCharacterCreator = true;

        private bool navMeshAvailable;
        private Vector3 spawnLocationOverride = Vector3.zero;
        private Vector3 spawnRotationOverride = Vector3.zero;
        private string returnSceneName = string.Empty;

        private bool levelManagerInitialized = false;

        [Header("LOADING SCREEN")]
        public Slider loadBar;
        public Text finishedLoadingText;

        public bool MyNavMeshAvailable { get => navMeshAvailable; set => navMeshAvailable = value; }
        public string MyDefaultStartingZone { get => defaultStartingZone; set => defaultStartingZone = value; }
        public Vector3 MySpawnRotationOverride { get => spawnRotationOverride; set => spawnRotationOverride = value; }
        public Vector3 MySpawnLocationOverride { get => spawnLocationOverride; set => spawnLocationOverride = value; }
        public string MyReturnSceneName { get => returnSceneName; set => returnSceneName = value; }

        public void PerformSetupActivities() {
            InitializeLevelManager();
            PerformLevelLoadActivities();
        }

        private void InitializeLevelManager() {
            //Debug.Log("LevelManager.InitializeLevelManager()");
            if (levelManagerInitialized == true) {
                return;
            }
            DontDestroyOnLoad(this.gameObject);
            //Debug.Log("LevelManager.InitializeLevelManager(): setting scenemanager onloadlevel");
            SceneManager.sceneLoaded += OnLoadLevel;
            levelManagerInitialized = true;
        }

        public SceneNode GetActiveSceneNode() {
            //Debug.Log("LevelManager.GetActiveSceneNode()");
            return SystemSceneNodeManager.MyInstance.GetResource(SceneManager.GetActiveScene().name);
        }

        public Vector3 GetSpawnLocation() {
            //Debug.Log("LevelManager.GetSpawnLocation(): scene is: " + SceneManager.GetActiveScene().name);
            SceneNode activeSceneNode = GetActiveSceneNode();
            if (activeSceneNode != null) {
                if (spawnLocationOverride != Vector3.zero) {
                    //Debug.Log("Levelmanager.GetSpawnLocation(). SpawnLocationOverride is set.  returning " + spawnLocationOverride);
                    Vector3 returnValue = spawnLocationOverride;

                    // reset to default so the next level loaded will not attempt to use this spawn location override
                    spawnLocationOverride = Vector3.zero;

                    // return original value
                    return returnValue;
                } else {
                    //Debug.Log("Levelmanager.GetSpawnLocation(). SpawnLocationOverride is not set.");
                    GameObject defaultspawnLocationMarker = GameObject.FindWithTag("DefaultSpawnLocation");
                    if (defaultspawnLocationMarker != null) {
                        //Debug.Log("Levelmanager.GetSpawnLocation(). Found an object tagged DefaultSpawnLocation. returning " + defaultspawnLocationMarker.transform.position);
                        return defaultspawnLocationMarker.transform.position;
                    }
                    //Debug.Log("Levelmanager.GetSpawnLocation(). Could Not Find a tagged DefaultSpawnLocation.  returning scene node default" + activeSceneNode.MyDefaultSpawnPosition);
                    return activeSceneNode.MyDefaultSpawnPosition;
                }
            }

            //Debug.Log("LevelManager.GetSpawnLocation(): Could not find level in configured list.  Return default(0,0,0)");
            return Vector3.zero;
        }

        public Vector3 GetSpawnRotation() {
            //Debug.Log("Levelmanager.GetSpawnLocation(). SpawnLocationOverride is set.  returning " + spawnLocationOverride);
            Vector3 returnValue = spawnRotationOverride;

            // reset to default so the next level loaded will not attempt to use this spawn location override
            spawnRotationOverride = Vector3.zero;

            // return original value
            return returnValue;
        }

        private void DetectNavMesh() {
            //Debug.Log("LevelManager.DetectNavMesh()");
            NavMeshHit hit;
            if (NavMesh.SamplePosition(Vector3.zero, out hit, 1000.0f, NavMesh.AllAreas)) {
                Vector3 result = hit.position;
                navMeshAvailable = true;
                //Debug.Log("LevelManager.DetectNavMesh(): NavMesh detected!");
            } else {
                navMeshAvailable = false;
                //Debug.Log("LevelManager.DetectNavMesh(): Could not detect a Navmesh");
            }
        }

        /// <summary>
        /// automatically called by the scene manager
        /// </summary>
        /// <param name="newScene"></param>
        /// <param name="loadSceneMode"></param>
        public void OnLoadLevel(Scene newScene, LoadSceneMode loadSceneMode) {
            //Debug.Log("Levelmanager.OnLoadLevel(): Finding Scene Settings. SceneManager.GetActiveScene().name: " + SceneManager.GetActiveScene().name);
            if (!levelManagerInitialized) {
                //Debug.Log("Levelmanager.OnLoadLevel(): Start has not run yet, returning!");
                return;
            }
            PerformLevelLoadActivities();
        }

        public void PerformLevelLoadActivities() {
            // determine if this is the game manager loading scene
            //Debug.Log("Levelmanager.PerformLevelLoadActivities(): Finding Scene Settings. SceneManager.GetActiveScene().name: " + SceneManager.GetActiveScene().name + " == " + initializationScene);
            if (SceneManager.GetActiveScene().name == initializationScene) {
                //Debug.Log("Levelmanager.OnLoadLevel(): Loading Main Menu");
                LoadLevel(mainMenuScene);
                return;
            }

            // determine if this is the main menu
            UIManager.MyInstance.ActivateInGameUI();
            UIManager.MyInstance.DeactivatePlayerUI();
            UIManager.MyInstance.ActivateSystemMenuUI();
            if (SceneManager.GetActiveScene().name == mainMenuScene) {
                //Debug.Log("Levelmanager.OnLoadLevel(): This is the main menu scene.  Activating Main Menu");
                SystemWindowManager.MyInstance.OpenMainMenu();
            } else {
                // just in case
                SystemWindowManager.MyInstance.CloseMainMenu();
            }

            // determine if a navmesh is available
            DetectNavMesh();

            // do things that can only be done if we have information about this scene in the scene nodes database
            PlayLevelSounds();

            // send messages to subscribers
            SystemEventManager.MyInstance.NotifyOnLevelLoad();

            // activate the correct camera
            ActivateSceneCamera();
        }

        private void PlayLevelSounds() {
            SceneNode activeSceneNode = GetActiveSceneNode();
            if (activeSceneNode != null) {
                if (activeSceneNode.MyAmbientMusicProfile != null && activeSceneNode.MyAmbientMusicProfile != string.Empty) {
                    MusicProfile musicProfile = SystemMusicProfileManager.MyInstance.GetResource(activeSceneNode.MyAmbientMusicProfile);
                    if (musicProfile != null && musicProfile.MyAudioClip != null) {
                        AudioManager.MyInstance.PlayAmbientSound(musicProfile.MyAudioClip);
                    }
                } else {
                    AudioManager.MyInstance.StopAmbientSound();
                }
                if (activeSceneNode.MyBackgroundMusicProfile != null && activeSceneNode.MyBackgroundMusicProfile != string.Empty) {
                    MusicProfile backgroundMusicProfile = SystemMusicProfileManager.MyInstance.GetResource(activeSceneNode.MyBackgroundMusicProfile);
                    if (backgroundMusicProfile != null && backgroundMusicProfile.MyAudioClip != null) {
                        AudioManager.MyInstance.PlayMusic(backgroundMusicProfile.MyAudioClip);
                    }
                } else {
                    AudioManager.MyInstance.StopMusic();
                }
            }
        }

        private void ActivateSceneCamera() {
            //Debug.Log("Levelmanager.ActivateSceneCamera()");

            if (GetActiveSceneNode() != null) {
                //Debug.Log("Levelmanager.ActivateSceneCamera(): GetActiveSceneNode is not null");
                if (GetActiveSceneNode().MySuppressMainCamera == true || GetActiveSceneNode().MyIsCutScene == true) {
                    if (GetActiveSceneNode().MyIsCutScene == false && GetActiveSceneNode().MyCutsceneViewed) {
                        // this is just an intro scene, not a full cutscene, and we have already viewed it, just go straight to main camera
                        CameraManager.MyInstance.MyMainCameraGameObject.SetActive(true);
                        return;
                    }
                    //Debug.Log("Levelmanager.ActivateSceneCamera(): activating cutscene camera");
                    CameraManager.MyInstance.MyMainCameraGameObject.SetActive(false);
                    if (AnyRPGCutsceneCameraController.MyInstance != null) {
                        AnyRPGCutsceneCameraController.MyInstance.gameObject.SetActive(true);
                    }
                    if (GetActiveSceneNode().MyIsCutScene == true || GetActiveSceneNode().MySuppressMainCamera == true) {
                        //Debug.Log("Levelmanager.ActivateSceneCamera(): activating cutscene bars");
                        UIManager.MyInstance.MyCutSceneBarController.StartCutScene(GetActiveSceneNode().MyDescription);
                    }
                } else {
                    CameraManager.MyInstance.MyMainCameraGameObject.SetActive(true);
                }
            }
        }

        public void LoadCutSceneWithDelay(string sceneName) {
            // doing this so that methods that needs to do something on successful interaction have time before the level unloads
            StartCoroutine(LoadCutSceneDelay(sceneName));
        }

        private IEnumerator LoadCutSceneDelay(string sceneName) {
            yield return new WaitForSeconds(1);
            LoadCutScene(sceneName);
        }

        public void LoadCutScene(string sceneName) {
            //Debug.Log("LevelManager.LoadCutScene(" + sceneName + ")");
            spawnRotationOverride = PlayerManager.MyInstance.MyPlayerUnitObject.transform.forward;
            spawnLocationOverride = PlayerManager.MyInstance.MyPlayerUnitObject.transform.position;
            returnSceneName = GetActiveSceneNode().MyName;
            LoadLevel(sceneName);
        }

        public void ReturnFromCutScene() {
            //Debug.Log("LevelManager.ReturnFromCutScene()");
            if (GetActiveSceneNode().MyIsCutScene == true) {
                //Debug.Log("Levelmanager.ActivateSceneCamera(): activating cutscene bars");
                LoadLevel(returnSceneName);
            } else {
                if (AnyRPGCutsceneCameraController.MyInstance != null) {
                    AnyRPGCutsceneCameraController.MyInstance.gameObject.SetActive(false);
                }
                CameraManager.MyInstance.MyMainCameraGameObject.SetActive(true);
                UIManager.MyInstance.MyPlayerInterfaceCanvas.SetActive(true);
                UIManager.MyInstance.MyPopupWindowContainer.SetActive(true);
                UIManager.MyInstance.MyPopupPanelContainer.SetActive(true);
                UIManager.MyInstance.MyCombatTextCanvas.SetActive(true);

                PlayerManager.MyInstance.SpawnPlayerUnit();
            }

        }

        public void LoadLevel(string levelName, Vector3 spawnLocationOverride, Vector3 spawnRotationOverride) {
            //Debug.Log("LevelManager.LoadLevel(" + levelName + ")");
            this.spawnRotationOverride = spawnRotationOverride;
            LoadLevel(levelName, spawnLocationOverride);
        }

        public void LoadLevel(string levelName, Vector3 spawnLocationOverride) {
            //Debug.Log("LevelManager.LoadLevel(" + levelName + ")");
            this.spawnLocationOverride = spawnLocationOverride;
            LoadLevel(levelName);
        }

        /// <summary>
        /// load a new level
        /// </summary>
        /// <param name="levelName"></param>
        public void LoadLevel(string levelName) {
            //Debug.Log("LevelManager.LoadLevel(" + levelName + ")");
            SystemEventManager.MyInstance.NotifyOnLevelUnload();

            UIManager.MyInstance.DeactivateInGameUI();
            UIManager.MyInstance.DeactivateSystemMenuUI();

            //SceneManager.LoadScene(levelName);
            StartCoroutine(LoadAsynchronously(levelName.Replace(" ", string.Empty)));
        }

        public void LoadDefaultStartingZone() {
            if (defaultStartingZone != string.Empty) {
                LoadLevel(defaultStartingZone);
            }
        }

        public void LoadMainMenu() {
            SystemEventManager.MyInstance.NotifyOnExitGame();
            LoadLevel(mainMenuScene);
        }

        public void LoadFirstScene() {
            if (loadCharacterCreator == true) {
                LoadLevel(characterCreatorScene);
            } else {
                LoadDefaultStartingZone();
            }
        }


        IEnumerator LoadAsynchronously(string sceneName) { // scene name is just the name of the current scene being loaded
            UIManager.MyInstance.ActivateLoadingUI();
            // try initial value
            loadBar.value = 0.1f;
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            if (operation == null) {
                //Debug.Log("LevelManager.LoadAsynchronously(" + sceneName + "): Could not create load operation!");
                yield return null;
            } else {
                //operation.allowSceneActivation = false;
                operation.allowSceneActivation = true;

                while (!operation.isDone) {
                    float progress = Mathf.Clamp01(operation.progress / .9f);
                    loadBar.value = progress;

                    /*
                    if (operation.progress >= 0.9f) {
                        //finishedLoadingText.gameObject.SetActive(true);

                        if (Input.anyKeyDown) {
                            operation.allowSceneActivation = true;
                        }
                    }
                    */

                    yield return null;
                }
                // deactivate loading menu
                //UIManager.MyInstance.DeactivateLoadingUI();
            }
        }


    }

}