using AnyRPG;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    public class CameraManager : MonoBehaviour {

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
        private Camera unitPreviewCamera = null;

        [SerializeField]
        private Camera petPreviewCamera = null;

        private GameObject thirdPartyCameraGameObject = null;

        private Camera thirdPartyCamera = null;

        private AnyRPGCameraController mainCameraController;

        protected bool eventSubscriptionsInitialized = false;

        public Camera MainCamera { get => mainCamera; set => mainCamera = value; }
        public GameObject MainCameraGameObject { get => mainCameraGameObject; }
        public Camera MainMapCamera { get => mainMapCamera; set => mainMapCamera = value; }
        public Camera CharacterPortraitCamera { get => characterPortraitCamera; set => characterPortraitCamera = value; }
        public Camera FocusPortraitCamera { get => focusPortraitCamera; set => focusPortraitCamera = value; }
        public Camera CharacterCreatorCamera { get => characterCreatorCamera; set => characterCreatorCamera = value; }
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

        public void Init() {
            //Debug.Log("CameraManager.Awake()");
            CheckConfiguration();

            // attach camera to player
            mainCameraController = mainCameraGameObject.GetComponent<AnyRPGCameraController>();

            if (thirdPartyCameraGameObject == null && SystemConfigurationManager.Instance.ThirdPartyCamera != null) {
                thirdPartyCameraGameObject = Instantiate(SystemConfigurationManager.Instance.ThirdPartyCamera, transform);
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
        }

        private void CheckConfiguration() {
            if (SystemConfigurationManager.Instance.UseThirdPartyCameraControl == true && SystemConfigurationManager.Instance.ThirdPartyCamera == null && (thirdPartyCamera == null || thirdPartyCameraGameObject == null)) {
                Debug.LogError("CameraManager.CheckConfiguration(): The system configuration option 'Use Third Party Camera' is true, but no third party camera is configured in the Camera Manager. Check inspector!");
            }
        }

        public void ActivateMainCamera(bool prePositionCamera = false) {
            //Debug.Log("CameraManager.ActivateMainCamera()");
            if (SystemConfigurationManager.Instance == null) {
                // can't get camera settings, so just return
                return;
            }
            if (LevelManager.Instance.IsMainMenu()
                || LevelManager.Instance.IsInitializationScene()
                || SystemConfigurationManager.Instance.UseThirdPartyCameraControl == false) {
                MainCameraGameObject.SetActive(true);
                return;
            }
            if (SystemConfigurationManager.Instance.UseThirdPartyCameraControl == true) {
                EnableThirdPartyCamera(prePositionCamera);
                return;
            }

            // fallback in case no camera found
            MainCameraGameObject.SetActive(true);
        }

        public void SwitchToMainCamera() {
            //Debug.Log("CameraManager.SwitchToMainCamera()");
            if (SystemConfigurationManager.Instance.UseThirdPartyCameraControl == true) {
                DisableThirdPartyCamera();
            }
            MainCameraGameObject.SetActive(true);
        }

        public void DeactivateMainCamera() {
            //Debug.Log("CameraManager.DeactivateMainCamera()");
            MainCameraGameObject.SetActive(false);
            if (SystemConfigurationManager.Instance.UseThirdPartyCameraControl == true) {
                DisableThirdPartyCamera();
            }
        }

        public void EnableCutsceneCamera() {
            //Debug.Log("CameraManager.EnableCutsceneCamera()");
            if (CutsceneCameraController.Instance != null) {
                //Debug.Log("CameraManager.EnableCutsceneCamera(): enabling");
                CutsceneCameraController.Instance.gameObject.SetActive(true);
            }
        }

        public void DisableCutsceneCamera() {
            //Debug.Log("CameraManager.DisableCutsceneCamera()");
            if (CutsceneCameraController.Instance != null) {
                //Debug.Log("CameraManager.DisableCutsceneCamera(): disabling");
                CutsceneCameraController.Instance.gameObject.SetActive(false);
            }
        }

        public void EnableThirdPartyCamera(bool prePositionCamera = false) {
            //Debug.Log("CameraManager.EnableThirdPartyCamera()");
            if (thirdPartyCameraGameObject != null) {
                if (mainCameraGameObject != null) {
                    MainCameraGameObject.SetActive(false);
                }
                if (PlayerManager.Instance.ActiveUnitController != null) {
                    thirdPartyCameraGameObject.transform.position = PlayerManager.Instance.ActiveUnitController.transform.TransformPoint(new Vector3(0f, 2.5f, -3f));
                    thirdPartyCameraGameObject.transform.forward = PlayerManager.Instance.ActiveUnitController.transform.forward;
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
            if (characterPreviewCamera != null) {
                unitPreviewCamera.enabled = false;
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
            //Debug.Log(gameObject.name + ".CameraManager.HandlePlayerUnitSpawn()");
            ProcessPlayerUnitSpawn();
        }


        public void ProcessPlayerUnitSpawn() {
            //Debug.Log("CameraManager.ProcessPlayerUnitSpawn()");
            if (LevelManager.Instance.GetActiveSceneNode() == null) {
                //Debug.Log("CameraManager.ProcessPlayerUnitSpawn(): ACTIVE SCENE NODE WAS NULL");
                return;
            }

            if (LevelManager.Instance.GetActiveSceneNode().SuppressMainCamera != true) {
                //Debug.Log("CameraManager.ProcessPlayerUnitSpawn(): suppressed by level = false, spawning camera");
                mainCamera.GetComponent<AnyRPGCameraController>().InitializeCamera(PlayerManager.Instance.ActiveUnitController.transform);
            }
        }

        public void HandlePlayerUnitDespawn(string eventName, EventParamProperties eventParamProperties) {
            mainCamera.GetComponent<AnyRPGCameraController>().ClearTarget();
        }
    }

}