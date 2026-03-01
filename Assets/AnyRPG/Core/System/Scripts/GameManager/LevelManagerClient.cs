using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    public class LevelManagerClient : ConfiguredClass {

        public event Action<float> OnSetLoadingProgress = delegate { };
        public event Action<string> OnBeginLoadingLevel = delegate { };
        public event Action OnExitToMainMenu = delegate { };
        public event Action OnLevelLoad = delegate { };
        public event Action<int, string> OnLevelUnload = delegate { };

        private SceneNode returnScene = null;
        private Bounds sceneBounds;

        private Coroutine loadCutSceneCoroutine = null;
        private bool loadingLevel = false;
        private float loadingProgress = 0f;

        private bool levelManagerInitialized = false;

        private SceneNode activeSceneNode = null;
        private string activeSceneName = string.Empty;
        private SceneNode loadingSceneNode = null;

        private TerrainDetector terrainDetector = null;

        // game manager references
        private UIManager uIManager = null;
        private AudioManager audioManager = null;
        private CameraManager cameraManager = null;
        private PlayerManagerClient playerManagerClient = null;
        private MapManager mapManager = null;
        private SceneUtilityService sceneUtilityService = null;
        private LevelManagerServer levelManagerServer = null;

        //public Vector3 SpawnRotationOverride { get => spawnRotationOverride; set => spawnRotationOverride = value; }
        //public Vector3 SpawnLocationOverride { get => spawnLocationOverride; set => spawnLocationOverride = value; }
        //public SceneNode ReturnSceneName { get => returnScene; set => returnScene = value; }
        public bool LoadingLevel { get => loadingLevel; set => loadingLevel = value; }
        public string ActiveSceneName { get => activeSceneName; set => activeSceneName = value; }
        public Bounds SceneBounds { get => sceneBounds; }
        public SceneNode LoadingSceneNode { get => loadingSceneNode; set => loadingSceneNode = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            mapManager = uIManager.MapManager;
            audioManager = systemGameManager.AudioManager;
            cameraManager = systemGameManager.CameraManager;
            playerManagerClient = systemGameManager.PlayerManagerClient;
            sceneUtilityService = systemGameManager.SceneUtilityService;
            levelManagerServer = systemGameManager.LevelManagerServer;
        }

        private void HandleStopServer() {
            if (IsMainMenu() == false) {
                LoadMainMenu(false);
            }
        }

        public void PerformSetupActivities() {
            InitializeLevelManager();
            PerformLevelLoadActivities(SceneManager.GetActiveScene());
        }

        private void InitializeLevelManager() {
            //Debug.Log($"LevelManagerClient.InitializeLevelManager()");
            if (levelManagerInitialized == true) {
                return;
            }
            //DontDestroyOnLoad(this.gameObject);
            //Debug.Log($"LevelManagerClient.InitializeLevelManager(): setting scenemanager onloadlevel");
            SceneManager.sceneLoaded += HandleLoadLevel;
            SceneManager.sceneUnloaded += HandleSceneUnloaded;
            //SceneManager.activeSceneChanged += HandleActiveSceneChanged;
            
            networkManagerServer.OnStopServer += HandleStopServer;
            terrainDetector = new TerrainDetector();
            NavMesh.pathfindingIterationsPerFrame = 500;
            levelManagerInitialized = true;
        }


        public static Bounds GetSceneBounds() {
            Renderer[] renderers;
            TerrainCollider[] terrainColliders;
            Bounds sceneBounds = new Bounds();

            // only grab mesh renderers because skinned mesh renderers get strange angles when their bones are rotated
            renderers = GameObject.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
            terrainColliders = GameObject.FindObjectsByType<TerrainCollider>(FindObjectsSortMode.None);

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
            //Debug.Log($"LevelManagerClient.GetActiveSceneNode(): return " + SceneManager.GetActiveScene().name);
            return activeSceneNode;
        }

        /// <summary>
        /// automatically called by the scene manager
        /// </summary>
        /// <param name="newScene"></param>
        /// <param name="loadSceneMode"></param>
        public void HandleLoadLevel(Scene newScene, LoadSceneMode loadSceneMode) {
            //Debug.Log($"Levelmanager.HandleLoadLevel({newScene.name}) SceneManager.GetActiveScene().name: {SceneManager.GetActiveScene().name}");

            if (!levelManagerInitialized) {
                //Debug.Log("Levelmanager.OnLoadLevel(): Start has not run yet, returning!");
                return;
            }
            SetActiveSceneNode();
            if (systemGameManager.GameMode == GameMode.Local || IsCutscene() == true) {
                // in local mode or cutscene mode (which is also local), the scene is fully loaded after this event,
                // so we can perform the level load activities immediately.  In network mode the scene is not fully loaded until the
                // FishNetNetworkController lets us know all network objects have been spawned, so this will be called from there instead of here.
                PerformLevelLoadActivities(newScene);
            }
        }

        private void HandleSceneUnloaded(Scene scene) {
            Debug.Log($"LevelmanagerClient.HandleSceneUnloaded({scene.name})");

            levelManagerServer.RemoveLoadedScene(scene.handle, scene.name);
        }

        /*
        private void HandleActiveSceneChanged(Scene oldScene, Scene newScene) {
            //Debug.Log($"Levelmanager.HandleActiveSceneChanged({oldScene.name}, {newScene.name})");
        }
        */

        public void SetActiveSceneNode() {
            //Debug.Log("Levelmanager.SetActiveSceneNode()");

            activeSceneName = SceneManager.GetActiveScene().name;
            //Debug.Log($"Levelmanager.SetActiveSceneNode(): {activeSceneName}");
            activeSceneNode = sceneUtilityService.GetSceneNodeBySceneName(activeSceneName);
        }

        public bool IsMainMenu() {
            return IsMainMenu(activeSceneName);
        }

        public bool IsMainMenu(string matchSceneName) {
            if (matchSceneName == systemConfigurationManager.MainMenuSceneNode?.SceneFile) {
                return true;
            }
            if (matchSceneName == systemConfigurationManager.MainMenuScene?.Replace(" ", "")) {
                return true;
            }
            return false;
        }

        public bool IsInitializationScene() {
            return IsInitializationScene(activeSceneName);
        }

        public bool IsInitializationScene(string matchSceneName) {
            if (matchSceneName == systemConfigurationManager.InitializationSceneNode?.SceneFile) {
                return true;
            }
            if (matchSceneName == systemConfigurationManager.InitializationScene.Replace(" ", "")) {
                return true;
            }
            return false;
        }

        public void PerformNetworkLevelLoadActivities(Scene newScene) {
            Debug.Log($"Levelmanager.PerformNetworkLevelLoadActivities({newScene.name})");
            // this method is only called on network client
            SetActiveSceneNode();
            PerformLevelLoadActivities(newScene);
        }

        public void PerformLevelLoadActivities(Scene newScene) {
            Debug.Log($"Levelmanager.PerformLevelLoadActivities({newScene.name})");

            loadingLevel = false;

            // determine if this is the game manager loading scene
            if (IsInitializationScene(newScene.name)) {
                LoadMainMenu(true);
                return;
            }

            if (IsMainMenu(newScene.name) == true) {
                systemGameManager.AutoConfigureMonoBehaviours(newScene);
            } else {
                // just defaulting to world for now because this is a single player game
                // might add type "single" or something if this is an issue in the future
                levelManagerServer.AddLoadedScene(newScene, SceneInstanceType.World);
            }

            uIManager.ProcessLevelLoad();

            // ensure this is done after addloadedscene so that any initialization of active cameras has completed.
            cameraManager.CheckForCutsceneCamera(newScene);

            if (networkManagerServer.ServerModeActive == false) {
                // client only
                // client only for now
                if (activeSceneNode != null) {
                    activeSceneNode.PreloadFootStepAudio();
                    //activeSceneNode.Visit();
                }
                terrainDetector.LoadSceneSettings();
                if (IsMainMenu() == false) {
                    sceneBounds = GetSceneBounds();
                    if (systemGameManager.GameMode == GameMode.Local) {
                        mapManager.ProcessLevelLoad();
                    }
                }
                PlayLevelSounds();
                // activate the correct camera
                ActivateSceneCamera();
            } else {
                // Monitor for this message and delete the if block if it never happens.
                Debug.LogWarning("LevelManager.PerformLevelLoadActivities(): This method should never be called if the server is active. ");
            }

            OnLevelLoad();
        }

        public AudioProfile GetTerrainFootStepProfile(Vector3 transformPosition) {
            if (activeSceneNode != null && activeSceneNode.FootStepProfilesCount > 0) {
                return activeSceneNode.GetFootStepAudioProfile(terrainDetector.GetActiveTerrainTextureIdx(transformPosition));
            }

            return null;
        }

        public void PlayLevelSounds() {
            //Debug.Log("Levelmanager.PlayLevelSounds()");
            if (activeSceneNode == null) {
                return;
            }
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
            //Debug.Log($"Levelmanager.LoadCutSceneWithDelay({(cutscene == null ? null : cutscene.ResourceName)})");
            // doing this so that methods that needs to do something on successful interaction have time before the level unloads

            if (loadingLevel == false) {
                loadingLevel = true;
                loadCutSceneCoroutine = systemGameManager.StartCoroutine(LoadCutSceneDelay(cutscene));
            }
        }

        private IEnumerator LoadCutSceneDelay(Cutscene cutscene) {
            //Debug.Log($"LevelManagerClient.LoadCutSceneDelay({cutscene.ResourceName})");
            //yield return new WaitForSeconds(1);
            yield return null;
            loadCutSceneCoroutine = null;
            LoadCutScene(cutscene);
        }

        public void LoadCutScene(Cutscene cutscene) {
            //Debug.Log($"LevelManagerClient.LoadCutScene({cutscene.ResourceName})");

            returnScene = activeSceneNode;
            uIManager.CutSceneBarController.AssignCutScene(cutscene);
            LoadLevel(cutscene.LoadScene);
        }

        public void EndCutscene(Cutscene cutscene) {
            //Debug.Log($"LevelManagerClient.EndCutscene()");

            if (cutscene != null && cutscene.UnloadSceneOnEnd == true) {
                //Debug.Log("Levelmanager.ActivateSceneCamera(): activating cutscene bars");
                // we must do this here because the cutscene camera will be unloaded with the scene
                // so the main camera must be active or Unity will display that "no cameras rendering" message
                cameraManager.DisableCutsceneCamera();
                cameraManager.ActivateMainCamera();
                if (systemGameManager.GameMode == GameMode.Local) {
                    LoadLevel(returnScene);
                } else {
                    ProcessBeforeLevelUnload();
                    networkManagerClient.RequestReturnFromCutscene();
                }
            } else {

                //cameraManager.ActivateMainCamera();
                uIManager.PlayerInterfaceCanvas.SetActive(true);
                uIManager.PopupWindowContainer.SetActive(true);
                uIManager.PopupPanelContainer.SetActive(true);
                uIManager.CombatTextCanvas.SetActive(true);

                if (playerManagerClient.PlayerUnitSpawned == false) {
                    playerManagerClient.RequestSpawnPlayerUnit();
                }

                // test moving down here
                cameraManager.DisableCutsceneCamera();
                cameraManager.ActivateMainCamera();
            }
        }

        /// <summary>
        /// load a new level
        /// </summary>
        /// <param name="levelName"></param>
        public void LoadLevel(SceneNode loadSceneNode) {
            //Debug.Log($"LevelManagerClient.LoadLevel({loadSceneNode.ResourceName})");

            ProcessBeforeLevelUnload();

            StartLoadAsync(loadSceneNode);
        }

        /// <summary>
        /// load a new level
        /// </summary>
        /// <param name="levelName"></param>
        public void LoadLevel(string levelName) {
            //Debug.Log($"LevelManagerClient.LoadLevel({levelName})");

            if (levelName == null || levelName == string.Empty) {
                return;
            }

            ProcessBeforeLevelUnload();

            loadingSceneNode = systemDataFactory.GetResource<SceneNode>(levelName);

            if (loadingSceneNode != null && IsInitializationScene() == false && IsMainMenu(loadingSceneNode.SceneFile) == true) {
                // since this isn't the initialization scene, we need to process the exit to main menu functionality
                playerManagerClient.ProcessExitToMainMenu();
                OnExitToMainMenu();
            }

            if (loadingSceneNode != null) {
                StartLoadAsync(loadingSceneNode);
            } else {
                StartLoadAsync(levelName);
                //Debug.LogError("LevelManager.LoadLevel(" + levelName + "): could not find scene node with that name!");
            }
        }

        public void ProcessBeforeLevelUnload() {
            Debug.Log("LevelManagerClient.ProcessBeforeLevelUnload()");
            mapManager.ProcessLevelUnload();
            OnLevelUnload(SceneManager.GetActiveScene().handle, SceneManager.GetActiveScene().name);

            uIManager.DeactivatePlayerUI();
            uIManager.DeactivateInGameUI();
            uIManager.DeactivateSystemMenuUI();
        }

        private void StartLoadAsync(SceneNode loadSceneNode) {
            //Debug.Log($"LevelManagerClient.StartLoadAsync({loadSceneNode.ResourceName}) cutscene: {loadSceneNode.IsCutScene}");

            if (systemGameManager.GameMode == GameMode.Local || loadSceneNode.IsCutScene) {
                systemGameManager.StartCoroutine(LoadAsynchronously(loadSceneNode.SceneFile));
                return;
            }
            StartLoadAsync(loadSceneNode.SceneFile);
        }

        private void StartLoadAsync(string sceneName) {
            //Debug.Log($"LevelManagerClient.StartLoadAsync({sceneName})");

            if (systemGameManager.GameMode == GameMode.Local) {
                systemGameManager.StartCoroutine(LoadAsynchronously(sceneName));
                return;
            }
            Debug.LogWarning("LevelManager.StartLoadAsync(): should not be loading a scene directly in networked mode!");
        }

        public bool IsCutscene() {
            if (activeSceneNode != null) {
                return activeSceneNode.IsCutScene;
            }
            return false;
        }

        public void LoadDefaultStartingZone() {
            if (systemConfigurationManager.DefaultStartingZone != string.Empty) {
                LoadLevel(systemConfigurationManager.DefaultStartingZone);
            }
        }

        public void LoadMainMenu(bool isInitializationScene) {
            //Debug.Log($"LevelManagerClient.LoadMainMenu({isInitializationScene})");

            /*
            if (isInitializationScene == false) {
                // since this isn't the initialization scene, we need to process the exit to main menu functionality
                playerManager.ProcessExitToMainMenu();
            }
            */
            if (systemConfigurationManager.MainMenuSceneNode != null) {
                LoadLevel(systemConfigurationManager.MainMenuSceneNode.ResourceName);
            } else {
                LoadLevel(SystemDataUtility.PrepareStringForMatch(systemConfigurationManager.MainMenuScene));
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

        public void SetLoadingProgress(float newProgress) {
            loadingProgress = newProgress;
            OnSetLoadingProgress(newProgress);
        }

        // scene name is just the name of the current scene being loaded
        IEnumerator LoadAsynchronously(string sceneName) {
            //Debug.Log($"LevelManagerClient.LoadAsynchronously({sceneName})");

            NotifyOnBeginLoadingLevel(sceneName);

            // try initial value
            SetLoadingProgress(0.1f);
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            if (operation == null) {
                //Debug.Log($"LevelManagerClient.LoadAsynchronously(" + sceneName + "): Could not create load operation!");
                yield return null;
            } else {
                //operation.allowSceneActivation = false;
                operation.allowSceneActivation = true;

                while (!operation.isDone) {
                    float progress = Mathf.Clamp01(operation.progress / .9f);
                    SetLoadingProgress(progress);

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

        public void NotifyOnBeginLoadingLevel(string sceneName) {
            OnBeginLoadingLevel(sceneName);
        }


    }

}