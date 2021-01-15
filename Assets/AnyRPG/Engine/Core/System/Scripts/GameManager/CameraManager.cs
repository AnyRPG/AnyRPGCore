using AnyRPG;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    public class CameraManager : MonoBehaviour {

        #region Singleton
        private static CameraManager instance;

        public static CameraManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<CameraManager>();
                }

                return instance;
            }
        }

        #endregion

        [SerializeField]
        private Camera mainCamera = null;

        [SerializeField]
        private GameObject mainCameraGameObject = null;

        [SerializeField]
        private Camera miniMapCamera = null;

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

        //[SerializeField]
        private GameObject thirdPartyCameraGameObject = null;

        //[SerializeField]
        private Camera thirdPartyCamera = null;



        private AnyRPGCameraController mainCameraController;

        protected bool eventSubscriptionsInitialized = false;

        public Camera MainCamera { get => mainCamera; set => mainCamera = value; }
        public GameObject MainCameraGameObject { get => mainCameraGameObject; }
        public Camera MiniMapCamera { get => miniMapCamera; set => miniMapCamera = value; }
        public Camera MainMapCamera { get => mainMapCamera; set => mainMapCamera = value; }
        public Camera CharacterPortraitCamera { get => characterPortraitCamera; set => characterPortraitCamera = value; }
        public Camera FocusPortraitCamera { get => focusPortraitCamera; set => focusPortraitCamera = value; }
        public Camera CharacterCreatorCamera { get => characterCreatorCamera; set => characterCreatorCamera = value; }
        public Camera CharacterPreviewCamera { get => characterPreviewCamera; set => characterPreviewCamera = value; }
        public AnyRPGCameraController MainCameraController { get => mainCameraController; set => mainCameraController = value; }
        public Camera UnitPreviewCamera { get => unitPreviewCamera; set => unitPreviewCamera = value; }
        public Camera PetPreviewCamera { get => petPreviewCamera; set => petPreviewCamera = value; }
        public GameObject ThirdPartyCamera { get => thirdPartyCameraGameObject; set => thirdPartyCameraGameObject = value; }

        public Camera MyActiveMainCamera {
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

        private void Awake() {
            //Debug.Log("CameraManager.Awake()");
            CheckConfiguration();

            // attach camera to player
            mainCameraController = mainCameraGameObject.GetComponent<AnyRPGCameraController>();

            if (thirdPartyCameraGameObject == null && SystemConfigurationManager.MyInstance.ThirdPartyCamera != null) {
                thirdPartyCameraGameObject = Instantiate(SystemConfigurationManager.MyInstance.ThirdPartyCamera, transform);
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
        }

        private void Start() {
            //Debug.Log("CameraManager.Start()");
            CreateEventSubscriptions();
        }

        private void CheckConfiguration() {
            if (SystemConfigurationManager.MyInstance.UseThirdPartyCameraControl == true && SystemConfigurationManager.MyInstance.ThirdPartyCamera == null && (thirdPartyCamera == null || thirdPartyCameraGameObject == null)) {
                Debug.LogError("CameraManager.CheckConfiguration(): The system configuration option 'Use Third Party Camera' is true, but no third party camera is configured in the Camera Manager. Check inspector!");
            }
        }

        public void ActivateMainCamera(bool prePositionCamera = false) {
            //Debug.Log("CameraManager.ActivateMainCamera()");
            if (SystemConfigurationManager.MyInstance == null) {
                // can't get camera settings, so just return
                return;
            }
            SceneNode activeScene = LevelManager.MyInstance.GetActiveSceneNode();
            if (activeScene == SystemConfigurationManager.MyInstance?.MainMenuSceneNode
                || activeScene == SystemConfigurationManager.MyInstance?.InitializationSceneNode
                || SystemConfigurationManager.MyInstance.UseThirdPartyCameraControl == false) {
                MainCameraGameObject.SetActive(true);
                return;
            }
            if (SystemConfigurationManager.MyInstance.UseThirdPartyCameraControl == true) {
                EnableThirdPartyCamera(prePositionCamera);
                return;
            }

            // fallback in case no camera found
            MainCameraGameObject.SetActive(true);
        }

        public void SwitchToMainCamera() {
            //Debug.Log("CameraManager.SwitchToMainCamera()");
            if (SystemConfigurationManager.MyInstance.UseThirdPartyCameraControl == true) {
                DisableThirdPartyCamera();
            }
            MainCameraGameObject.SetActive(true);
        }

        public void DeactivateMainCamera() {
            //Debug.Log("CameraManager.DeactivateMainCamera()");
            MainCameraGameObject.SetActive(false);
            if (SystemConfigurationManager.MyInstance.UseThirdPartyCameraControl == true) {
                DisableThirdPartyCamera();
            }
        }

        public void EnableCutsceneCamera() {
            //Debug.Log("CameraManager.EnableCutsceneCamera()");
            if (CutsceneCameraController.MyInstance != null) {
                //Debug.Log("CameraManager.EnableCutsceneCamera(): enabling");
                CutsceneCameraController.MyInstance.gameObject.SetActive(true);
            }
        }

        public void DisableCutsceneCamera() {
            //Debug.Log("CameraManager.DisableCutsceneCamera()");
            if (CutsceneCameraController.MyInstance != null) {
                //Debug.Log("CameraManager.DisableCutsceneCamera(): disabling");
                CutsceneCameraController.MyInstance.gameObject.SetActive(false);
            }
        }

        public void EnableThirdPartyCamera(bool prePositionCamera = false) {
            //Debug.Log("CameraManager.EnableThirdPartyCamera()");
            if (thirdPartyCameraGameObject != null) {
                if (mainCameraGameObject != null) {
                    MainCameraGameObject.SetActive(false);
                }
                if (PlayerManager.MyInstance.ActiveUnitController != null) {
                    thirdPartyCameraGameObject.transform.position = PlayerManager.MyInstance.ActiveUnitController.transform.TransformPoint(new Vector3(0f, 2.5f, -3f));
                    thirdPartyCameraGameObject.transform.forward = PlayerManager.MyInstance.ActiveUnitController.transform.forward;
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
            SystemEventManager.MyInstance.OnPlayerUnitDespawn += HandlePlayerUnitDespawn;
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StopListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            SystemEventManager.MyInstance.OnPlayerUnitDespawn -= HandlePlayerUnitDespawn;
            eventSubscriptionsInitialized = false;
        }

        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            CleanupEventSubscriptions();
        }

        public void HandlePlayerUnitSpawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log(gameObject.name + ".InanimateUnit.HandlePlayerUnitSpawn()");
            ProcessPlayerUnitSpawn();
        }


        public void ProcessPlayerUnitSpawn() {
            //Debug.Log("CameraManager.HandlePlayerUnitSpawn()");
            if (LevelManager.MyInstance.GetActiveSceneNode() == null) {
                //Debug.Log("CameraManager.HandlePlayerUnitSpawn(): ACTIVE SCENE NODE WAS NULL");
                return;
            }

            if (LevelManager.MyInstance.GetActiveSceneNode().SuppressMainCamera != true) {
                //Debug.Log("CameraManager.HandlePlayerUnitSpawn(): suppressed by level = false, spawning camera");
                mainCamera.GetComponent<AnyRPGCameraController>().InitializeCamera(PlayerManager.MyInstance.ActiveUnitController.transform);
            }
        }

        public void HandlePlayerUnitDespawn() {
            mainCamera.GetComponent<AnyRPGCameraController>().ClearTarget();
        }
    }

}