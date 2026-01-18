using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    public class LevelManager : ConfiguredClass {

        public event System.Action<float> OnSetLoadingProgress = delegate { };
        public event System.Action<string> OnBeginLoadingLevel = delegate { };
        public event System.Action OnExitToMainMenu = delegate { };

        private bool navMeshAvailable;
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


        // dictionary of scene file names to scene nodes for quick lookup at runtime
        private Dictionary<string, SceneNode> sceneDictionary = new Dictionary<string, SceneNode>();

        // game manager references
        private UIManager uIManager = null;
        private AudioManager audioManager = null;
        private CameraManager cameraManager = null;
        private PlayerManager playerManager = null;
        private MapManager mapManager = null;
        private SystemEventManager systemEventManager = null;

        public bool NavMeshAvailable { get => navMeshAvailable; set => navMeshAvailable = value; }
        //public Vector3 SpawnRotationOverride { get => spawnRotationOverride; set => spawnRotationOverride = value; }
        //public Vector3 SpawnLocationOverride { get => spawnLocationOverride; set => spawnLocationOverride = value; }
        //public SceneNode ReturnSceneName { get => returnScene; set => returnScene = value; }
        public bool LoadingLevel { get => loadingLevel; set => loadingLevel = value; }
        public string ActiveSceneName { get => activeSceneName; set => activeSceneName = value; }
        public Bounds SceneBounds { get => sceneBounds; }
        public SceneNode LoadingSceneNode { get => loadingSceneNode; set => loadingSceneNode = value; }
        public Dictionary<string, SceneNode> SceneDictionary { get => sceneDictionary; set => sceneDictionary = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            networkManagerServer.OnStopServer += HandleStopServer;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            mapManager = uIManager.MapManager;
            audioManager = systemGameManager.AudioManager;
            cameraManager = systemGameManager.CameraManager;
            playerManager = systemGameManager.PlayerManager;
            systemEventManager = systemGameManager.SystemEventManager;
        }

        private void HandleStopServer() {
            if (IsMainMenu() == false) {
                LoadMainMenu(false);
            }
        }

        public void PerformSetupActivities() {
            InitializeLevelManager();
            PerformLevelLoadActivities(true);
        }

        private void InitializeLevelManager() {
            //Debug.Log("LevelManager.InitializeLevelManager()");
            if (levelManagerInitialized == true) {
                return;
            }
            //DontDestroyOnLoad(this.gameObject);
            //Debug.Log("LevelManager.InitializeLevelManager(): setting scenemanager onloadlevel");
            SceneManager.sceneLoaded += HandleLoadLevel;
            SceneManager.sceneUnloaded += HandleSceneUnloaded;
            SceneManager.activeSceneChanged += HandleActiveSceneChanged;
            terrainDetector = new TerrainDetector();
            levelManagerInitialized = true;

            // initialize the scene dictionary
            foreach (SceneNode sceneNode in systemDataFactory.GetResourceList<SceneNode>()) {
                if (sceneNode.SceneFile != null && sceneNode.SceneFile != string.Empty) {
                    //Debug.Log($"LevelManager.InitializeLevelManager(): adding scene {sceneNode.SceneFile} to scene dictionary from {sceneNode.ResourceName}");
                    sceneDictionary.Add(sceneNode.SceneFile, sceneNode);
                }
            }
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
            //Debug.Log("LevelManager.GetActiveSceneNode(): return " + SceneManager.GetActiveScene().name);
            return activeSceneNode;
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
        public void HandleLoadLevel(Scene newScene, LoadSceneMode loadSceneMode) {
            //Debug.Log($"Levelmanager.HandleLoadLevel({newScene.name}) SceneManager.GetActiveScene().name: {SceneManager.GetActiveScene().name}");

            if (!levelManagerInitialized) {
                //Debug.Log("Levelmanager.OnLoadLevel(): Start has not run yet, returning!");
                return;
            }
            SetActiveSceneNode();
            if (systemGameManager.GameMode == GameMode.Local || IsCutscene()) {
                ProcessLevelLoad(false);
            }
        }

        public void ProcessLevelLoad(bool setActiveSceneNode) {
            //Debug.Log($"Levelmanager.ProcessLevelLoad(): {SceneManager.GetActiveScene().name}");

            PerformLevelLoadActivities(setActiveSceneNode);
            NavMesh.pathfindingIterationsPerFrame = 500;
        }

        private void HandleSceneUnloaded(Scene scene) {
            //Debug.Log($"Levelmanager.HandleSceneUnloaded({scene.name}): Finding Scene Settings. SceneManager.GetActiveScene().name: {SceneManager.GetActiveScene().name}");
        }

        private void HandleActiveSceneChanged(Scene oldScene, Scene newScene) {
            //Debug.Log($"Levelmanager.HandleActiveSceneChanged({oldScene.name}, {newScene.name})");
        }

        public void SetActiveSceneNode() {
            //Debug.Log("Levelmanager.SetActiveSceneNode()");

            activeSceneName = SceneManager.GetActiveScene().name;
            //Debug.Log($"Levelmanager.SetActiveSceneNode(): {activeSceneName}");

            if (sceneDictionary.ContainsKey(activeSceneName)) {
                activeSceneNode = sceneDictionary[activeSceneName];
                //Debug.Log($"Levelmanager.SetActiveSceneNode(): setting active scene node");
            } else {
                activeSceneNode = null;
                //Debug.Log($"Levelmanager.SetActiveSceneNode(): NULLING active scene node");
            }
        }

        public SceneNode GetSceneNodeBySceneName(string sceneName) {
            if (sceneDictionary.ContainsKey(sceneName)) {
                return sceneDictionary[sceneName];
            }
            return null;
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

        public void PerformLevelLoadActivities(bool setActiveSceneNode) {
            //Debug.Log($"Levelmanager.PerformLevelLoadActivities() SceneManager.GetActiveScene().name: {SceneManager.GetActiveScene().name}");

            loadingLevel = false;
            if (setActiveSceneNode) {
                SetActiveSceneNode();
            }
            systemGameManager.AutoConfigureMonoBehaviours(SceneManager.GetActiveScene());

            // determine if this is the game manager loading scene
            if (IsInitializationScene()) {
                LoadMainMenu(true);
                return;
            }

            uIManager.ProcessLevelLoad();

            // determine if a navmesh is available
            DetectNavMesh();
            cameraManager.CheckForCutsceneCamera();

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
            }

            // send messages to subscribers
            EventParamProperties eventParamProperties = new EventParamProperties();
            eventParamProperties.simpleParams.StringParam = (activeSceneNode == null ? activeSceneName : activeSceneNode.ResourceName);
            systemEventManager.NotifyOnLevelLoad();
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
            //Debug.Log($"LevelManager.LoadCutSceneDelay({cutscene.ResourceName})");
            //yield return new WaitForSeconds(1);
            yield return null;
            loadCutSceneCoroutine = null;
            LoadCutScene(cutscene);
        }

        public void LoadCutScene(Cutscene cutscene) {
            //Debug.Log($"LevelManager.LoadCutScene({cutscene.ResourceName})");

            returnScene = activeSceneNode;
            uIManager.CutSceneBarController.AssignCutScene(cutscene);
            LoadLevel(cutscene.LoadScene);
        }

        public void EndCutscene(Cutscene cutscene) {
            //Debug.Log("LevelManager.EndCutscene()");

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

                if (playerManager.PlayerUnitSpawned == false) {
                    playerManager.RequestSpawnPlayerUnit();
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
            //Debug.Log($"LevelManager.LoadLevel({loadSceneNode.ResourceName})");

            ProcessBeforeLevelUnload();

            StartLoadAsync(loadSceneNode);
        }

        /// <summary>
        /// load a new level
        /// </summary>
        /// <param name="levelName"></param>
        public void LoadLevel(string levelName) {
            //Debug.Log($"LevelManager.LoadLevel({levelName})");

            if (levelName == null || levelName == string.Empty) {
                return;
            }

            ProcessBeforeLevelUnload();

            loadingSceneNode = systemDataFactory.GetResource<SceneNode>(levelName);

            if (loadingSceneNode != null && IsInitializationScene() == false && IsMainMenu(loadingSceneNode.SceneFile) == true) {
                // since this isn't the initialization scene, we need to process the exit to main menu functionality
                playerManager.ProcessExitToMainMenu();
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
            mapManager.ProcessLevelUnload();
            systemEventManager.NotifyOnLevelUnloadClient(SceneManager.GetActiveScene().handle, SceneManager.GetActiveScene().name);

            uIManager.DeactivatePlayerUI();
            uIManager.DeactivateInGameUI();
            uIManager.DeactivateSystemMenuUI();
        }

        private void StartLoadAsync(SceneNode loadSceneNode) {
            //Debug.Log($"LevelManager.StartLoadAsync({loadSceneNode.ResourceName}) cutscene: {loadSceneNode.IsCutScene}");

            if (systemGameManager.GameMode == GameMode.Local || loadSceneNode.IsCutScene) {
                systemGameManager.StartCoroutine(LoadAsynchronously(loadSceneNode.SceneFile));
                return;
            }
            StartLoadAsync(loadSceneNode.SceneFile);
        }

        private void StartLoadAsync(string sceneName) {
            //Debug.Log($"LevelManager.StartLoadAsync({sceneName})");

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
            //Debug.Log($"LevelManager.LoadMainMenu({isInitializationScene})");

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
            //Debug.Log($"LevelManager.LoadAsynchronously({sceneName})");

            NotifyOnBeginLoadingLevel(sceneName);

            // try initial value
            SetLoadingProgress(0.1f);
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            if (operation == null) {
                //Debug.Log("LevelManager.LoadAsynchronously(" + sceneName + "): Could not create load operation!");
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