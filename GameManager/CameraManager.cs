using AnyRPG;
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

        [SerializeField]
        private GameObject thirdPartyCameraGameObject = null;

        [SerializeField]
        private Camera thirdPartyCamera = null;



        private AnyRPGCameraController mainCameraController;

        protected bool eventSubscriptionsInitialized = false;

        public Camera MyMainCamera { get => mainCamera; set => mainCamera = value; }
        public GameObject MyMainCameraGameObject { get => mainCameraGameObject; }
        public Camera MyMiniMapCamera { get => miniMapCamera; set => miniMapCamera = value; }
        public Camera MyMainMapCamera { get => mainMapCamera; set => mainMapCamera = value; }
        public Camera MyCharacterPortraitCamera { get => characterPortraitCamera; set => characterPortraitCamera = value; }
        public Camera MyFocusPortraitCamera { get => focusPortraitCamera; set => focusPortraitCamera = value; }
        public Camera MyCharacterCreatorCamera { get => characterCreatorCamera; set => characterCreatorCamera = value; }
        public Camera MyCharacterPreviewCamera { get => characterPreviewCamera; set => characterPreviewCamera = value; }
        public AnyRPGCameraController MyMainCameraController { get => mainCameraController; set => mainCameraController = value; }
        public Camera MyUnitPreviewCamera { get => unitPreviewCamera; set => unitPreviewCamera = value; }
        public Camera MyPetPreviewCamera { get => petPreviewCamera; set => petPreviewCamera = value; }
        public GameObject MyThirdPartyCamera { get => thirdPartyCameraGameObject; set => thirdPartyCameraGameObject = value; }

        public Camera MyActiveMainCamera {
            get {
                if (MyMainCameraGameObject != null && MyMainCameraGameObject.activeSelf == true && MyMainCamera != null) {
                    return MyMainCamera;
                }
                if (MyThirdPartyCamera != null && MyThirdPartyCamera.activeSelf == true && thirdPartyCamera != null) {
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

            DisablePreviewCameras();
            DisableThirdPartyCamera();
            DisableFocusCamera();
        }

        private void Start() {
            //Debug.Log("CameraManager.Start()");
            CreateEventSubscriptions();
        }

        private void CheckConfiguration() {
            if (SystemConfigurationManager.MyInstance.MyUseThirdPartyCameraControl == true && (thirdPartyCamera == null || thirdPartyCameraGameObject == null)) {
                Debug.LogError("CameraManager.CheckConfiguration(): The system configuration option 'Use Third Party Camera' is true, but no third party camera is configured in the Camera Manager. Check inspector!");
            }
        }

        public void ActivateMainCamera() {
            //Debug.Log("CameraManager.ActivateMainCamera()");
            if (SystemConfigurationManager.MyInstance == null) {
                // can't get camera settings, so just return
                return;
            }
            SceneNode activeScene = LevelManager.MyInstance.GetActiveSceneNode();
            if (activeScene == LevelManager.MyInstance.MainMenuSceneNode || activeScene == LevelManager.MyInstance.InitializationSceneNode || activeScene == LevelManager.MyInstance.CharacterCreatorSceneNode || SystemConfigurationManager.MyInstance.MyUseThirdPartyCameraControl == false) {
                MyMainCameraGameObject.SetActive(true);
                return;
            }
            if (SystemConfigurationManager.MyInstance.MyUseThirdPartyCameraControl == true) {
                EnableThirdPartyCamera();
                return;
            }

            // fallback in case no camera found
            MyMainCameraGameObject.SetActive(true);
        }

        public void SwitchToMainCamera() {
            //Debug.Log("CameraManager.SwitchToMainCamera()");
            if (SystemConfigurationManager.MyInstance.MyUseThirdPartyCameraControl == true) {
                DisableThirdPartyCamera();
            }
            MyMainCameraGameObject.SetActive(true);
        }

        public void DeactivateMainCamera() {
            //Debug.Log("CameraManager.DeactivateMainCamera()");
            MyMainCameraGameObject.SetActive(false);
            if (SystemConfigurationManager.MyInstance.MyUseThirdPartyCameraControl == true) {
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

        public void EnableThirdPartyCamera() {
            //Debug.Log("CameraManager.EnableThirdPartyCamera()");
            if (thirdPartyCameraGameObject != null) {
                if (mainCameraGameObject != null) {
                    MyMainCameraGameObject.SetActive(false);
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

            if (LevelManager.MyInstance.GetActiveSceneNode().MySuppressMainCamera != true) {
                //Debug.Log("CameraManager.HandlePlayerUnitSpawn(): suppressed by level = false, spawning camera");
                mainCamera.GetComponent<AnyRPGCameraController>().InitializeCamera(PlayerManager.MyInstance.MyPlayerUnitObject.transform);
            }
        }

        public void HandlePlayerUnitDespawn() {
            mainCamera.GetComponent<AnyRPGCameraController>().ClearTarget();
        }
    }

}