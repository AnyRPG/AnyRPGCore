using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AnyRPG {
    public class LevelManager : ConfiguredMonoBehaviour {

        [Header("Loading Screen")]
        public Slider loadBar;
        public TextMeshProUGUI finishedLoadingText;

        private bool navMeshAvailable;
        private bool overrideSpawnLocation = false;
        private Vector3 spawnLocationOverride = Vector3.zero;
        private bool overrideSpawnRotation = false;
        private Vector3 spawnRotationOverride = Vector3.zero;
        private string returnSceneName = string.Empty;
        private Bounds sceneBounds;

        private Coroutine loadCutSceneCoroutine = null;
        private bool loadingLevel = false;

        private bool levelManagerInitialized = false;

        private SceneNode activeSceneNode = null;
        private string activeSceneName = string.Empty;

        private TerrainDetector terrainDetector = null;

        private string defaultSpawnLocationTag = "DefaultSpawnLocation";

        private string overrideSpawnLocationTag = string.Empty;

        // dictionary of scene file names to scene nodes for quick lookup at runtime
        private Dictionary<string, SceneNode> sceneDictionary = new Dictionary<string, SceneNode>();

        // game manager references
        private SystemDataFactory systemDataFactory = null;
        private UIManager uIManager = null;
        private AudioManager audioManager = null;
        private CameraManager cameraManager = null;
        private PlayerManager playerManager = null;
        private MapManager mapManager = null;

        public bool NavMeshAvailable { get => navMeshAvailable; set => navMeshAvailable = value; }
        //public Vector3 SpawnRotationOverride { get => spawnRotationOverride; set => spawnRotationOverride = value; }
        //public Vector3 SpawnLocationOverride { get => spawnLocationOverride; set => spawnLocationOverride = value; }
        public string ReturnSceneName { get => returnSceneName; set => returnSceneName = value; }
        public string OverrideSpawnLocationTag { get => overrideSpawnLocationTag; set => overrideSpawnLocationTag = value; }
        public bool LoadingLevel { get => loadingLevel; set => loadingLevel = value; }
        public string ActiveSceneName { get => activeSceneName; set => activeSceneName = value; }
        public Bounds SceneBounds { get => sceneBounds; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemDataFactory = systemGameManager.SystemDataFactory;
            uIManager = systemGameManager.UIManager;
            mapManager = uIManager.MapManager;
            audioManager = systemGameManager.AudioManager;
            cameraManager = systemGameManager.CameraManager;
            playerManager = systemGameManager.PlayerManager;
        }

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
            terrainDetector = new TerrainDetector();
            levelManagerInitialized = true;

            // initialize the scene dictionary
            foreach (SceneNode sceneNode in systemDataFactory.GetResourceList<SceneNode>()) {
                if (sceneNode.SceneFile != null && sceneNode.SceneFile != string.Empty) {
                    sceneDictionary.Add(sceneNode.SceneFile.ToLower(), sceneNode);
                }
            }
        }

        public static Bounds GetSceneBounds() {
            Renderer[] renderers;
            TerrainCollider[] terrainColliders;
            Bounds sceneBounds = new Bounds();

            // only grab mesh renderers because skinned mesh renderers get strange angles when their bones are rotated
            renderers = GameObject.FindObjectsOfType<MeshRenderer>();
            terrainColliders = GameObject.FindObjectsOfType<TerrainCollider>();

            // add bounds of renderers in case there are structures higher or lower than terrain bounds
            if (renderers.Length != 0) {
                for (int i = 0; i < renderers.Length; i++) {
                    if (renderers[i].enabled == true && renderers[i].gameObject.layer == LayerMask.NameToLayer("Default")) {
                        sceneBounds.Encapsulate(renderers[i].bounds);
                        //Debug.Log("MainMapController.SetSceneBounds(). Encapsulating gameobject: " + renderers[i].gameObject.name + " with bounds " + renderers[i].bounds);
                    }
                }
            }

            // add bounds of terrain colliders to get 'main' bounds
            if (terrainColliders.Length != 0) {
                for (int i = 0; i < terrainColliders.Length; i++) {
                    if (terrainColliders[i].enabled == true) {
                        sceneBounds.Encapsulate(terrainColliders[i].bounds);
                        //Debug.Log("MiniMapGeneratorController.GetSceneBounds(). Encapsulating terrain bounds: " + terrainColliders[i].bounds);
                    }
                }
            }

            return sceneBounds;
        }

        public SceneNode GetActiveSceneNode() {
            //Debug.Log("LevelManager.GetActiveSceneNode(): return " + SceneManager.GetActiveScene().name);
            return activeSceneNode;
        }

        public void SetSpawnRotationOverride(Vector3 spawnRotation) {
            //Debug.Log("LevelManager.SetSpawnRotationOverride(" + spawnRotation + ")");
            overrideSpawnRotation = true;
            spawnRotationOverride = spawnRotation;
        }

        public void SetSpawnLocationOverride(Vector3 spawnLocation) {
            //Debug.Log("LevelManager.SetSpawnLocationOverride(" + spawnLocation + ")");
            overrideSpawnLocation = true;
            spawnLocationOverride = spawnLocation;
        }

        public Vector3 GetSpawnLocation() {
            //Debug.Log("LevelManager.GetSpawnLocation(): scene is: " + SceneManager.GetActiveScene().name);
            // test : disable this, it was preventing games with no scene nodes from recalling save position
            //if (activeSceneNode != null) {
            if (overrideSpawnLocation == true) {
                //Debug.Log("Levelmanager.GetSpawnLocation(). SpawnLocationOverride is set.  returning " + spawnLocationOverride);
                Vector3 returnValue = spawnLocationOverride;

                // reset to default so the next level loaded will not attempt to use this spawn location override
                overrideSpawnLocation = false;
                spawnLocationOverride = Vector3.zero;

                // return original value
                return returnValue;
            } else {
                string usedTag = defaultSpawnLocationTag;
                if (overrideSpawnLocationTag != null && overrideSpawnLocationTag != string.Empty) {
                    usedTag = overrideSpawnLocationTag;
                }
                //Debug.Log("Levelmanager.GetSpawnLocation(). usedTag: " + usedTag);
                GameObject spawnLocationMarker = GameObject.FindWithTag(usedTag);
                overrideSpawnLocationTag = string.Empty;

                // if the prefered tag was found, us it, otherwise fall back to the default tag
                if (spawnLocationMarker != null) {
                    //Debug.Log("Levelmanager.GetSpawnLocation(). Found an object tagged " + usedTag + ". returning " + defaultspawnLocationMarker.transform.position);
                    if (overrideSpawnRotation == false) {
                        SetSpawnRotationOverride(spawnLocationMarker.transform.forward);
                    }
                    return spawnLocationMarker.transform.position;
                }
            }
            //}

            // no override was set.  fall back to default
            GameObject defaultspawnLocationMarker = GameObject.FindWithTag(defaultSpawnLocationTag);
            if (defaultspawnLocationMarker != null) {
                //Debug.Log("Levelmanager.GetSpawnLocation(). Found an object tagged " + defaultSpawnLocationTag + ". returning " + defaultspawnLocationMarker.transform.position);
                if (overrideSpawnRotation == false) {
                    SetSpawnRotationOverride(defaultspawnLocationMarker.transform.forward);
                }
                return defaultspawnLocationMarker.transform.position;
            }

            //Debug.Log("LevelManager.GetSpawnLocation(): Could not find level in configured list.  Return default(0,0,0)");
            return Vector3.zero;
        }

        public Vector3 GetSpawnRotation() {
            //Debug.Log("Levelmanager.GetSpawnRotation() " + spawnRotationOverride);
            Vector3 returnValue = spawnRotationOverride;

            // reset to default so the next level loaded will not attempt to use this spawn location override
            overrideSpawnRotation = false;
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

            activeSceneName = SceneManager.GetActiveScene().name;

            if (sceneDictionary.ContainsKey(activeSceneName.ToLower())) {
                activeSceneNode = sceneDictionary[activeSceneName.ToLower()];
            } else {
                activeSceneNode = null;
            }
        }

        public bool IsMainMenu() {
            if (activeSceneName == systemConfigurationManager.MainMenuSceneNode?.SceneFile) {
                return true;
            }
            if (activeSceneName == systemConfigurationManager.MainMenuScene?.Replace(" ", "")) {
                return true;
            }
            return false;
        }

        public bool IsInitializationScene() {
            if (activeSceneName == systemConfigurationManager.InitializationSceneNode?.SceneFile) {
                return true;
            }
            if (activeSceneName == systemConfigurationManager.InitializationScene.Replace(" ", "")) {
                return true;
            }
            return false;
        }

        public void PerformLevelLoadActivities() {
            // determine if this is the game manager loading scene
            //Debug.Log("Levelmanager.PerformLevelLoadActivities(): Finding Scene Settings. SceneManager.GetActiveScene().name: " + SceneManager.GetActiveScene().name + " == " + initializationScene);
            loadingLevel = false;
            SetActiveSceneNode();
            if (activeSceneNode != null) {
                activeSceneNode.Visit();
            }
            terrainDetector.LoadSceneSettings();
            systemGameManager.AutoConfigureMonoBehaviours();
            cameraManager.CheckForCutsceneCamera();
            if (IsInitializationScene()) {
                LoadMainMenu();
                return;
            }

            // determine if this is the main menu
            uIManager.ActivateInGameUI();
            uIManager.DeactivatePlayerUI();
            uIManager.ActivateSystemMenuUI();
            if (IsMainMenu()) {
                //Debug.Log("Levelmanager.OnLoadLevel(): This is the main menu scene.  Activating Main Menu");
                uIManager.mainMenuWindow.OpenWindow();
            } else {
                // just in case
                uIManager.mainMenuWindow.CloseWindow();
                sceneBounds = GetSceneBounds();
                mapManager.ProcessLevelLoad();
            }

            // get level boundaries
            // testing - move this to not main menu or initialization scene
            //sceneBounds = GetSceneBounds();

            // determine if a navmesh is available
            DetectNavMesh();

            // bug fix for unity making shadows too dark on realtime lighting with no lightmaps baked and using skybox as environment lightning source
            // doesn't work too well :(
            //DynamicGI.UpdateEnvironment();

            // do things that can only be done if we have information about this scene in the scene nodes database
            PlayLevelSounds();

            // send messages to subscribers
            // testing moving this to after activating scene camera
            /*
            EventParamProperties eventParamProperties = new EventParamProperties();
            eventParamProperties.simpleParams.StringParam = (activeSceneNode == null ? activeSceneName : activeSceneNode.DisplayName);
            SystemEventManager.TriggerEvent("OnLevelLoad", eventParamProperties);
            */

            // activate the correct camera
            ActivateSceneCamera();

            // send messages to subscribers
            EventParamProperties eventParamProperties = new EventParamProperties();
            eventParamProperties.simpleParams.StringParam = (activeSceneNode == null ? activeSceneName : activeSceneNode.DisplayName);
            SystemEventManager.TriggerEvent("OnLevelLoad", eventParamProperties);
        }

        public AudioProfile GetTerrainFootStepProfile(Vector3 transformPosition) {
            if (activeSceneNode != null && activeSceneNode.FootStepProfilesCount > 0) {
                return activeSceneNode.GetFootStepAudioProfile(terrainDetector.GetActiveTerrainTextureIdx(transformPosition));
            }

            return null;
        }

        public void PlayLevelSounds() {
            //Debug.Log("Levelmanager.PlayLevelSounds()");
            if (activeSceneNode != null) {
                /*
                if (activeSceneNode.AmbientMusicProfile != null && activeSceneNode.AmbientMusicProfile.AudioClip != null) {
                    audioManager.PlayAmbient(activeSceneNode.AmbientMusicProfile.AudioClip);
                } else {
                    audioManager.StopAmbient();
                }
                */
                if (activeSceneNode.BackgroundMusicAudio != null) {
                    //Debug.Log("Levelmanager.PlayLevelSounds(): PLAYING MUSIC");
                    audioManager.PlayMusic(activeSceneNode.BackgroundMusicAudio);
                } else {
                    //Debug.Log("Levelmanager.PlayLevelSounds(): STOPPING MUSIC");
                    audioManager.StopMusic();
                }
            }
        }

        private void ActivateSceneCamera() {
            //Debug.Log("Levelmanager.ActivateSceneCamera()");

            if (activeSceneNode?.AutoPlayCutscene != null
                && (activeSceneNode.AutoPlayCutscene.Repeatable == true || activeSceneNode.AutoPlayCutscene.Viewed == false)) {
                // a scene that is only a cutscene, or a cutscene that has not been viewed yet is active
                // load the cutscene
                uIManager.CutSceneBarController.StartCutScene(activeSceneNode.AutoPlayCutscene);
                return;
            }

            // no cutscene to be played, activate the main camera
            cameraManager.ActivateMainCamera();
        }

        public void LoadCutSceneWithDelay(Cutscene cutscene) {
            //Debug.Log("Levelmanager.LoadCutSceneWithDelay(" + (cutscene == null ? null : cutscene.DisplayName) + ")");
            // doing this so that methods that needs to do something on successful interaction have time before the level unloads

            if (loadingLevel == false) {
                loadingLevel = true;
                loadCutSceneCoroutine = StartCoroutine(LoadCutSceneDelay(cutscene));
            }
        }

        private IEnumerator LoadCutSceneDelay(Cutscene cutscene) {
            //yield return new WaitForSeconds(1);
            yield return null;
            loadCutSceneCoroutine = null;
            LoadCutScene(cutscene);
        }

        public void LoadCutScene(Cutscene cutscene) {
            //Debug.Log("LevelManager.LoadCutScene(" + sceneName + ")");
            if (playerManager.ActiveUnitController != null) {
                SetSpawnRotationOverride(playerManager.ActiveUnitController.transform.forward);
                SetSpawnLocationOverride(playerManager.ActiveUnitController.transform.position);
            }
            returnSceneName = activeSceneNode.ResourceName;
            uIManager.CutSceneBarController.AssignCutScene(cutscene);
            LoadLevel(cutscene.LoadScene.ResourceName);
        }

        public void EndCutscene(Cutscene cutscene) {
            //Debug.Log("LevelManager.EndCutscene()");
            if (cutscene != null && cutscene.UnloadSceneOnEnd == true) {
                //Debug.Log("Levelmanager.ActivateSceneCamera(): activating cutscene bars");
                LoadLevel(returnSceneName);
            } else {

                //cameraManager.ActivateMainCamera();
                uIManager.PlayerInterfaceCanvas.SetActive(true);
                uIManager.PopupWindowContainer.SetActive(true);
                uIManager.PopupPanelContainer.SetActive(true);
                uIManager.CombatTextCanvas.SetActive(true);

                if (playerManager.PlayerUnitSpawned == false) {
                    playerManager.SpawnPlayerUnit();
                }

                // test moving down here
                cameraManager.DisableCutsceneCamera();
                cameraManager.ActivateMainCamera();
            }
        }

        public void LoadLevel(string levelName, Vector3 spawnLocationOverride, Vector3 spawnRotationOverride) {
            //Debug.Log("LevelManager.LoadLevel(" + levelName + ")");
            SetSpawnRotationOverride(spawnRotationOverride);
            LoadLevel(levelName, spawnLocationOverride);
        }

        public void LoadLevel(string levelName, Vector3 spawnLocation) {
            //Debug.Log("LevelManager.LoadLevel(" + levelName + ")");
            SetSpawnLocationOverride(spawnLocation);
            LoadLevel(levelName);
        }

        /// <summary>
        /// load a new level
        /// </summary>
        /// <param name="levelName"></param>
        public void LoadLevel(string levelName) {
            //Debug.Log("LevelManager.LoadLevel(" + levelName + ")");

            if (levelName == null || levelName == string.Empty) {
                return;
            }

            mapManager.ProcessLevelUnload();
            SystemEventManager.TriggerEvent("OnLevelUnload", new EventParamProperties());

            // playerManager needs to do this last so other objects can respond before we despawn the character
            // testing - let playerController handle passing on player despawn event
            //playerManager.ProcessLevelUnload();

            uIManager.DeactivatePlayerUI();
            uIManager.DeactivateInGameUI();
            uIManager.DeactivateSystemMenuUI();

            //SceneManager.LoadScene(levelName);
            //StartCoroutine(LoadAsynchronously(levelName.Replace(" ", string.Empty)));
            SceneNode sceneNode = systemDataFactory.GetResource<SceneNode>(levelName);
            if (sceneNode != null) {
                StartCoroutine(LoadAsynchronously(sceneNode.SceneFile));
            } else {
                StartCoroutine(LoadAsynchronously(levelName));
                //Debug.LogError("LevelManager.LoadLevel(" + levelName + "): could not find scene node with that name!");
            }
        }

        public void LoadDefaultStartingZone() {
            if (systemConfigurationManager.DefaultStartingZone != string.Empty) {
                LoadLevel(systemConfigurationManager.DefaultStartingZone);
            }
        }

        public void LoadMainMenu() {
            SystemEventManager.TriggerEvent("OnExitGame", new EventParamProperties());
            playerManager.ProcessExitToMainMenu();
            if (systemConfigurationManager.MainMenuSceneNode != null) {
                LoadLevel(systemConfigurationManager.MainMenuSceneNode.DisplayName);
            } else {
                LoadLevel(SystemDataFactory.PrepareStringForMatch(systemConfigurationManager.MainMenuScene));
            }
        }

        public void LoadFirstScene() {
            /*
            if (loadCharacterCreator == true) {
                LoadLevel(characterCreatorScene);
            } else {
            */
            LoadDefaultStartingZone();
            //}
        }


        IEnumerator LoadAsynchronously(string sceneName) { // scene name is just the name of the current scene being loaded
            //Debug.Log("LevelManager.LoadAsynchronously(" + sceneName + ")");
            uIManager.ActivateLoadingUI();
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
                //uIManager.DeactivateLoadingUI();
            }
        }


    }

}