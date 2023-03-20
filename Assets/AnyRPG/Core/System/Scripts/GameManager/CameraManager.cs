using AnyRPG;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    public class CameraManager : ConfiguredMonoBehaviour {

        [SerializeField]
        private Camera mainCamera = null;

        [SerializeField]
        private GameObject mainCameraGameObject = null;

        [SerializeField]
        private Camera mainMapCamera = null;

        [SerializeField]
        private Camera characterPortraitCamera = null;

        [SerializeField]
        private Camera focusPortraitCamera = null;

        [SerializeField]
        private Camera characterCreatorCamera = null;

        [SerializeField]
        private Camera characterPreviewCamera = null;

        [SerializeField]
        private Camera characterPanelCamera = null;

        [SerializeField]
        private Camera unitPreviewCamera = null;

        [SerializeField]
        private Camera petPreviewCamera = null;

        private GameObject thirdPartyCameraGameObject = null;

        private Camera thirdPartyCamera = null;

        private AnyRPGCameraController mainCameraController;

        private CutsceneCameraController currentCutsceneCameraController = null;

        private int playerLayer;
        private int equipmentLayer;
        private int hideLayers;

        protected bool eventSubscriptionsInitialized = false;

        // game manager references
        LevelManager levelManager = null;
        PlayerManager playerManager = null;

        public Camera MainCamera { get => mainCamera; set => mainCamera = value; }
        public GameObject MainCameraGameObject { get => mainCameraGameObject; }
        public Camera MainMapCamera { get => mainMapCamera; set => mainMapCamera = value; }
        public Camera CharacterPortraitCamera { get => characterPortraitCamera; set => characterPortraitCamera = value; }
        public Camera FocusPortraitCamera { get => focusPortraitCamera; set => focusPortraitCamera = value; }
        public Camera CharacterCreatorCamera { get => characterCreatorCamera; set => characterCreatorCamera = value; }
        public Camera CharacterPanelCamera { get => characterPanelCamera; set => characterPanelCamera = value; }
        public Camera CharacterPreviewCamera { get => characterPreviewCamera; set => characterPreviewCamera = value; }
        public AnyRPGCameraController MainCameraController { get => mainCameraController; set => mainCameraController = value; }
        public Camera UnitPreviewCamera { get => unitPreviewCamera; set => unitPreviewCamera = value; }
        public Camera PetPreviewCamera { get => petPreviewCamera; set => petPreviewCamera = value; }
        public GameObject ThirdPartyCamera { get => thirdPartyCameraGameObject; set => thirdPartyCameraGameObject = value; }

        public Camera ActiveMainCamera {
            get {
                if (MainCameraGameObject != null && MainCameraGameObject.activeSelf == true && MainCamera != null) {
                    return MainCamera;
                }
                if (ThirdPartyCamera != null && ThirdPartyCamera.activeSelf == true && thirdPartyCamera != null) {
                    return thirdPartyCamera;
                }
                return null;
            }
        }

        public CutsceneCameraController CurrentCutsceneCameraController { get => currentCutsceneCameraController; set => currentCutsceneCameraController = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log("CameraManager.Awake()");
            base.Configure(systemGameManager);
            levelManager = systemGameManager.LevelManager;
            playerManager = systemGameManager.PlayerManager;

            CheckConfiguration();

            // attach camera to player
            mainCameraController = mainCameraGameObject.GetComponent<AnyRPGCameraController>();
            mainCameraController.Configure(systemGameManager);

            if (thirdPartyCameraGameObject == null && systemConfigurationManager.ThirdPartyCamera != null) {
                thirdPartyCameraGameObject = Instantiate(systemConfigurationManager.ThirdPartyCamera, transform);
                if (thirdPartyCameraGameObject != null) {
                    thirdPartyCamera = thirdPartyCameraGameObject.GetComponentInChildren<Camera>();
                    if (thirdPartyCamera == null) {
                        Debug.LogWarning("No camera was found on the third party camera GameObject");
                    }
                } else {
                    Debug.LogWarning("Unable to instantiate third party camera GameObject");
                }
            }

            DisablePreviewCameras();
            DisableThirdPartyCamera();
            DisableFocusCamera();

            CreateEventSubscriptions();

            playerLayer = LayerMask.NameToLayer("Player");
            equipmentLayer = LayerMask.NameToLayer("Equipment");
            hideLayers = (1 << playerLayer | 1 << equipmentLayer);
        }

        public void HidePlayers() {
            //Debug.Log("CameraManager.HidePlayers()");
            mainCamera.cullingMask = mainCamera.cullingMask & ~hideLayers;
        }

        public void ShowPlayers() {
            //Debug.Log("CameraManager.ShowPlayers()");
            mainCamera.cullingMask = mainCamera.cullingMask | hideLayers;
        }

        public void CheckForCutsceneCamera() {
            //currentCutsceneCameraController = null;
            currentCutsceneCameraController = FindObjectOfType<CutsceneCameraController>();
        }

        private void CheckConfiguration() {
            if (systemConfigurationManager.UseThirdPartyCameraControl == true && systemConfigurationManager.ThirdPartyCamera == null && (thirdPartyCamera == null || thirdPartyCameraGameObject == null)) {
                Debug.LogError("CameraManager.CheckConfiguration(): The system configuration option 'Use Third Party Camera' is true, but no third party camera is configured in the Camera Manager. Check inspector!");
            }
        }

        public void ActivateMainCamera(bool prePositionCamera = false) {
            //Debug.Log("CameraManager.ActivateMainCamera()");
            if (systemConfigurationManager == null) {
                // can't get camera settings, so just return
                return;
            }
            if (levelManager.IsMainMenu()
                || levelManager.IsInitializationScene()) {
                MainCameraGameObject.SetActive(true);
                return;
            }
            if (systemConfigurationManager.UseThirdPartyCameraControl == false) {
                MainCameraGameObject.SetActive(true);
                return;
            }

            if (systemConfigurationManager.UseThirdPartyCameraControl == true) {
                EnableThirdPartyCamera(prePositionCamera);
                return;
            }

            // fallback in case no camera found
            MainCameraGameObject.SetActive(true);
        }

        public void SwitchToMainCamera() {
            //Debug.Log("CameraManager.SwitchToMainCamera()");
            if (systemConfigurationManager.UseThirdPartyCameraControl == true) {
                DisableThirdPartyCamera();
            }
            MainCameraGameObject.SetActive(true);
        }

        public void DeactivateMainCamera() {
            //Debug.Log("CameraManager.DeactivateMainCamera()");
            MainCameraGameObject.SetActive(false);
            if (systemConfigurationManager.UseThirdPartyCameraControl == true) {
                DisableThirdPartyCamera();
            }
        }

        public void EnableCutsceneCamera() {
            //Debug.Log("CameraManager.EnableCutsceneCamera()");
            if (currentCutsceneCameraController != null) {
                //Debug.Log("CameraManager.EnableCutsceneCamera(): enabling");
                currentCutsceneCameraController.gameObject.SetActive(true);
            }
        }

        public void DisableCutsceneCamera() {
            //Debug.Log("CameraManager.DisableCutsceneCamera()");
            if (currentCutsceneCameraController != null) {
                //Debug.Log("CameraManager.DisableCutsceneCamera(): disabling");
                currentCutsceneCameraController.gameObject.SetActive(false);
            }
        }

        public void EnableThirdPartyCamera(bool prePositionCamera = false) {
            //Debug.Log("CameraManager.EnableThirdPartyCamera()");
            if (thirdPartyCameraGameObject != null) {
                if (mainCameraGameObject != null) {
                    MainCameraGameObject.SetActive(false);
                }
                if (playerManager.ActiveUnitController != null) {
                    thirdPartyCameraGameObject.transform.position = playerManager.ActiveUnitController.transform.TransformPoint(new Vector3(0f, 2.5f, -3f));
                    thirdPartyCameraGameObject.transform.forward = playerManager.ActiveUnitController.transform.forward;
                    //Debug.Log("Setting camera location : " + thirdPartyCameraGameObject.transform.position + "; forward: " + thirdPartyCameraGameObject.transform.forward);
                }
                thirdPartyCameraGameObject.SetActive(true);
            }
        }

        public void DisableThirdPartyCamera() {
            //Debug.Log("CameraManager.DisableThirdPartyCamera()");
            if (thirdPartyCameraGameObject != null) {
                thirdPartyCameraGameObject.SetActive(false);
            }
        }

        private void DisablePreviewCameras() {
            if (characterPanelCamera != null) {
                characterPanelCamera.enabled = false;
            }
            if (characterPreviewCamera != null) {
                characterPreviewCamera.enabled = false;
            }
            if (unitPreviewCamera != null) {
                unitPreviewCamera.enabled = false;
            }
            if (petPreviewCamera != null) {
                petPreviewCamera.enabled = false;
            }
        }

        private void DisableFocusCamera() {
            focusPortraitCamera.enabled = false;
        }

        private void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StartListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            SystemEventManager.StartListening("OnPlayerUnitDespawn", HandlePlayerUnitDespawn);
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StopListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            SystemEventManager.StopListening("OnPlayerUnitDespawn", HandlePlayerUnitDespawn);
            eventSubscriptionsInitialized = false;
        }

        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            CleanupEventSubscriptions();
        }

        public void HandlePlayerUnitSpawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log($"{gameObject.name}.CameraManager.HandlePlayerUnitSpawn()");
            ProcessPlayerUnitSpawn();
        }


        public void ProcessPlayerUnitSpawn() {
            //Debug.Log("CameraManager.ProcessPlayerUnitSpawn()");
            /*
             // disabled this next condition because it was causing EmptyGame to fail
            if (levelManager.GetActiveSceneNode() == null) {
                //Debug.Log("CameraManager.ProcessPlayerUnitSpawn(): ACTIVE SCENE NODE WAS NULL");
                return;
            }
            */

            // disabled this condition because there is no situation where the player would spawn automatically in a camera suppressed level anyway
            // if there is a cutscene in the level, the player will spawn after the cutscene so the need to initialize the camera remains
            //if (levelManager.GetActiveSceneNode().SuppressMainCamera != true) {
                //Debug.Log("CameraManager.ProcessPlayerUnitSpawn(): suppressed by level = false, spawning camera");
                mainCameraController.InitializeCamera(playerManager.ActiveUnitController.transform);
            //}
        }

        public void HandlePlayerUnitDespawn(string eventName, EventParamProperties eventParamProperties) {
            mainCameraController.ClearTarget();
        }
    }

}