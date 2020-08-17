using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

        [Header("Level Manager")]

        [Tooltip("When a new game is started, the character will initially spawn in this scene")]
        [SerializeField]
        private string defaultStartingZone = string.Empty;

        [Tooltip("The name of the scene that loads the game manager into memory, and then proceeds to the main menu")]
        [SerializeField]
        private string initializationScene = "Load GameManager";

        private SceneNode initializationSceneNode = null;

        [Tooltip("The name of the main menu scene")]
        [SerializeField]
        private string mainMenuScene = "Main Menu";

        // reference to the main menu scene node
        private SceneNode mainMenuSceneNode = null;

        [Tooltip("The name of the character creator scene")]
        [SerializeField]
        private string characterCreatorScene = "Character Creator";

        private SceneNode characterCreatorSceneNode = null;

        [Tooltip("whether to start the game with the character creator or just go straight into the game as a soul")]
        [SerializeField]
        private bool loadCharacterCreator = true;

        [Header("LOADING SCREEN")]
        public Slider loadBar;
        public TextMeshProUGUI finishedLoadingText;

        private bool navMeshAvailable;
        private Vector3 spawnLocationOverride = Vector3.zero;
        private Vector3 spawnRotationOverride = Vector3.zero;
        private string returnSceneName = string.Empty;

        private Coroutine loadCutSceneCoroutine = null;

        private bool levelManagerInitialized = false;

        private SceneNode activeSceneNode = null;

        private string defaultSpawnLocationTag = "DefaultSpawnLocation";

        private string overrideSpawnLocationTag = string.Empty;

        // dictionary of scene file names to scene nodes for quick lookup at runtime
        private Dictionary<string, SceneNode> sceneDictionary = new Dictionary<string, SceneNode>();

        public bool MyNavMeshAvailable { get => navMeshAvailable; set => navMeshAvailable = value; }
        public string DefaultStartingZone { get => defaultStartingZone; set => defaultStartingZone = value; }
        public Vector3 SpawnRotationOverride { get => spawnRotationOverride; set => spawnRotationOverride = value; }
        public Vector3 SpawnLocationOverride { get => spawnLocationOverride; set => spawnLocationOverride = value; }
        public string ReturnSceneName { get => returnSceneName; set => returnSceneName = value; }
        public string OverrideSpawnLocationTag { get => overrideSpawnLocationTag; set => overrideSpawnLocationTag = value; }
        public SceneNode InitializationSceneNode { get => initializationSceneNode; set => initializationSceneNode = value; }
        public SceneNode MainMenuSceneNode { get => mainMenuSceneNode; set => mainMenuSceneNode = value; }
        public SceneNode CharacterCreatorSceneNode { get => characterCreatorSceneNode; set => characterCreatorSceneNode = value; }

        public void PerformSetupActivities() {
            InitializeLevelManager();
            PerformLevelLoadActivities();
        }

        private void InitializeLevelManager() {
            //Debug.Log("LevelManager.InitializeLevelManager()");
            if (levelManagerInitialized == true) {
                return;
            }
            //DontDestroyOnLoad(this.gameObject);
            //Debug.Log("LevelManager.InitializeLevelManager(): setting scenemanager onloadlevel");
            SceneManager.sceneLoaded += OnLoadLevel;
            levelManagerInitialized = true;

            // initialize the scene dictionary
            foreach (SceneNode sceneNode in SystemSceneNodeManager.MyInstance.GetResourceList()) {
                if (sceneNode.SceneFile != null && sceneNode.SceneFile != string.Empty) {
                    sceneDictionary.Add(sceneNode.SceneFile, sceneNode);
                }
            }

            SetupScriptableObjects();
        }

        public SceneNode GetActiveSceneNode() {
            //Debug.Log("LevelManager.GetActiveSceneNode(): return " + SceneManager.GetActiveScene().name);
            //return SystemSceneNodeManager.MyInstance.GetResource(SceneManager.GetActiveScene().name);
            return activeSceneNode;
        }

        public Vector3 GetSpawnLocation() {
            //Debug.Log("LevelManager.GetSpawnLocation(): scene is: " + SceneManager.GetActiveScene().name);
            if (activeSceneNode != null) {
                if (spawnLocationOverride != Vector3.zero) {
                    //Debug.Log("Levelmanager.GetSpawnLocation(). SpawnLocationOverride is set.  returning " + spawnLocationOverride);
                    Vector3 returnValue = spawnLocationOverride;

                    // reset to default so the next level loaded will not attempt to use this spawn location override
                    spawnLocationOverride = Vector3.zero;

                    // return original value
                    return returnValue;
                } else {
                    string usedTag = defaultSpawnLocationTag;
                    if (overrideSpawnLocationTag != null && overrideSpawnLocationTag != string.Empty) {
                        usedTag = overrideSpawnLocationTag;
                    }
                    //Debug.Log("Levelmanager.GetSpawnLocation(). usedTag: " + usedTag);
                    GameObject defaultspawnLocationMarker = GameObject.FindWithTag(usedTag);
                    overrideSpawnLocationTag = string.Empty;
                    if (defaultspawnLocationMarker != null) {
                        //Debug.Log("Levelmanager.GetSpawnLocation(). Found an object tagged " + usedTag + ". returning " + defaultspawnLocationMarker.transform.position);
                        return defaultspawnLocationMarker.transform.position;
                    } else {
                        defaultspawnLocationMarker = GameObject.FindWithTag(defaultSpawnLocationTag);
                        if (defaultspawnLocationMarker != null) {
                            //Debug.Log("Levelmanager.GetSpawnLocation(). Found an object tagged " + defaultSpawnLocationTag + ". returning " + defaultspawnLocationMarker.transform.position);
                            return defaultspawnLocationMarker.transform.position;
                        }
                    }
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
            NavMesh.pathfindingIterationsPerFrame = 500;
        }

        public void SetActiveSceneNode() {

            string activeSceneName = SceneManager.GetActiveScene().name;

            if (sceneDictionary.ContainsKey(activeSceneName)) {
                activeSceneNode = sceneDictionary[activeSceneName];
            } else {
                activeSceneNode = null;
            }
        }

        public void PerformLevelLoadActivities() {
            // determine if this is the game manager loading scene
            //Debug.Log("Levelmanager.PerformLevelLoadActivities(): Finding Scene Settings. SceneManager.GetActiveScene().name: " + SceneManager.GetActiveScene().name + " == " + initializationScene);
            SetActiveSceneNode();
            if (activeSceneNode != null) {
                activeSceneNode.Visit();
            }
            string activeSceneName = SceneManager.GetActiveScene().name;
            if (activeSceneName == initializationSceneNode.SceneFile) {
                //Debug.Log("Levelmanager.OnLoadLevel(): Loading Main Menu");
                LoadLevel(mainMenuScene);
                return;
            }

            // determine if this is the main menu
            UIManager.MyInstance.ActivateInGameUI();
            UIManager.MyInstance.DeactivatePlayerUI();
            UIManager.MyInstance.ActivateSystemMenuUI();
            if (activeSceneName == mainMenuSceneNode.SceneFile) {
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
            EventParamProperties eventParamProperties = new EventParamProperties();
            eventParamProperties.simpleParams.StringParam = (activeSceneNode == null ? activeSceneName : activeSceneNode.DisplayName);
            SystemEventManager.TriggerEvent("OnLevelLoad", eventParamProperties);

            // activate the correct camera
            ActivateSceneCamera();
        }

        public void PlayLevelSounds() {
            //Debug.Log("Levelmanager.PlayLevelSounds()");
            if (activeSceneNode != null) {
                if (activeSceneNode.AmbientMusicProfile != null && activeSceneNode.AmbientMusicProfile.AudioClip != null) {
                    AudioManager.MyInstance.PlayAmbientSound(activeSceneNode.AmbientMusicProfile.AudioClip);
                } else {
                    AudioManager.MyInstance.StopAmbientSound();
                }
                if (activeSceneNode.BackgroundMusicProfile != null && activeSceneNode.BackgroundMusicProfile.AudioClip != null) {
                    //Debug.Log("Levelmanager.PlayLevelSounds(): PLAYING MUSIC");
                    AudioManager.MyInstance.PlayMusic(activeSceneNode.BackgroundMusicProfile.AudioClip);
                } else {
                    //Debug.Log("Levelmanager.PlayLevelSounds(): STOPPING MUSIC");
                    AudioManager.MyInstance.StopMusic();
                }
            }
        }

        private void ActivateSceneCamera() {
            //Debug.Log("Levelmanager.ActivateSceneCamera()");

            if (activeSceneNode != null) {
                //Debug.Log("Levelmanager.ActivateSceneCamera(): GetActiveSceneNode is not null");
                if (activeSceneNode.AutoPlayCutscene != null) {
                    if (activeSceneNode.AutoPlayCutscene.Viewed == true && activeSceneNode.AutoPlayCutscene.Repeatable == false) {
                        // this is just an intro scene, not a full cutscene, and we have already viewed it, just go straight to main camera
                        CameraManager.MyInstance.ActivateMainCamera();
                        return;
                    }
                    //Debug.Log("Levelmanager.ActivateSceneCamera(): activating cutscene camera");
                    //if (GetActiveSceneNode().MyIsCutScene == true || GetActiveSceneNode().MySuppressMainCamera == true) {
                        //Debug.Log("Levelmanager.ActivateSceneCamera(): activating cutscene bars");
                        UIManager.MyInstance.MyCutSceneBarController.StartCutScene(activeSceneNode.AutoPlayCutscene);
                    //}
                } else {
                    CameraManager.MyInstance.ActivateMainCamera();
                }
            }
        }

        public void LoadCutSceneWithDelay(Cutscene cutscene) {
            // doing this so that methods that needs to do something on successful interaction have time before the level unloads
            if (loadCutSceneCoroutine == null) {
                loadCutSceneCoroutine = StartCoroutine(LoadCutSceneDelay(cutscene));
            }
        }

        private IEnumerator LoadCutSceneDelay(Cutscene cutscene) {
            yield return new WaitForSeconds(1);
            LoadCutScene(cutscene);
            loadCutSceneCoroutine = null;
        }

        public void LoadCutScene(Cutscene cutscene) {
            //Debug.Log("LevelManager.LoadCutScene(" + sceneName + ")");
            if (PlayerManager.MyInstance.MyPlayerUnitObject != null) {
                spawnRotationOverride = PlayerManager.MyInstance.MyPlayerUnitObject.transform.forward;
                spawnLocationOverride = PlayerManager.MyInstance.MyPlayerUnitObject.transform.position;
            }
            returnSceneName = activeSceneNode.ResourceName;
            UIManager.MyInstance.MyCutSceneBarController.AssignCutScene(cutscene);
            LoadLevel(cutscene.MyLoadScene.ResourceName);
        }

        public void EndCutscene(Cutscene cutscene) {
            //Debug.Log("LevelManager.EndCutscene()");
            if (cutscene != null && cutscene.MyUnloadSceneOnEnd == true) {
                //Debug.Log("Levelmanager.ActivateSceneCamera(): activating cutscene bars");
                LoadLevel(returnSceneName);
            } else {

                //CameraManager.MyInstance.ActivateMainCamera();
                UIManager.MyInstance.MyPlayerInterfaceCanvas.SetActive(true);
                UIManager.MyInstance.MyPopupWindowContainer.SetActive(true);
                UIManager.MyInstance.MyPopupPanelContainer.SetActive(true);
                UIManager.MyInstance.MyCombatTextCanvas.SetActive(true);

                if (PlayerManager.MyInstance.MyPlayerUnitSpawned == false) {
                    PlayerManager.MyInstance.SpawnPlayerUnit();
                }

                // test moving down here
                CameraManager.MyInstance.DisableCutsceneCamera();
                CameraManager.MyInstance.ActivateMainCamera();
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
            SystemEventManager.TriggerEvent("OnLevelUnload", new EventParamProperties());

            UIManager.MyInstance.DeactivateInGameUI();
            UIManager.MyInstance.DeactivateSystemMenuUI();

            //SceneManager.LoadScene(levelName);
            //StartCoroutine(LoadAsynchronously(levelName.Replace(" ", string.Empty)));
            SceneNode sceneNode = SystemSceneNodeManager.MyInstance.GetResource(levelName);
            if (sceneNode != null) {
                StartCoroutine(LoadAsynchronously(sceneNode.SceneFile));
            } else {
                Debug.LogError("LevelManager.LoadLevel(" + levelName + "): could not find scene node with that name!");
            }
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

        private void SetupScriptableObjects() {

            if (initializationScene != null && initializationScene != string.Empty) {
                SceneNode tmpSceneNode = SystemSceneNodeManager.MyInstance.GetResource(initializationScene);
                if (tmpSceneNode != null) {
                    initializationSceneNode = tmpSceneNode;
                } else {
                    Debug.LogError("LevelManager.SetupScriptableObjects: could not find scene node " + initializationScene + ". Check inspector.");
                }
            }

            if (mainMenuScene != null && mainMenuScene != string.Empty) {
                SceneNode tmpSceneNode = SystemSceneNodeManager.MyInstance.GetResource(mainMenuScene);
                if (tmpSceneNode != null) {
                    mainMenuSceneNode = tmpSceneNode;
                } else {
                    Debug.LogError("LevelManager.SetupScriptableObjects: could not find scene node " + mainMenuScene + ". Check inspector.");
                }
            }

            if (characterCreatorScene != null && characterCreatorScene != string.Empty) {
                SceneNode tmpSceneNode = SystemSceneNodeManager.MyInstance.GetResource(characterCreatorScene);
                if (tmpSceneNode != null) {
                    characterCreatorSceneNode = tmpSceneNode;
                } else {
                    Debug.LogError("LevelManager.SetupScriptableObjects: could not find scene node " + characterCreatorScene + ". Check inspector.");
                }
            }


        }


    }

}