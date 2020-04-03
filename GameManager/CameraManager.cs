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

        private void Awake() {
            //Debug.Log("CameraManager.Awake()");
            // attach camera to player
            mainCameraController = mainCameraGameObject.GetComponent<AnyRPGCameraController>();
            DisablePreviewCameras();
            DisableFocusCamera();
        }

        private void Start() {
            //Debug.Log("CameraManager.Start()");
            CreateEventSubscriptions();
        }

        private void DisablePreviewCameras() {
            characterPreviewCamera.enabled = false;
            unitPreviewCamera.enabled = false;
            petPreviewCamera.enabled = false;
        }

        private void DisableFocusCamera() {
            focusPortraitCamera.enabled = false;
        }

        private void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.MyInstance.OnPlayerUnitSpawn += HandlePlayerUnitSpawn;
            SystemEventManager.MyInstance.OnPlayerUnitDespawn += HandlePlayerUnitDespawn;
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.MyInstance.OnPlayerUnitSpawn -= HandlePlayerUnitSpawn;
            SystemEventManager.MyInstance.OnPlayerUnitDespawn -= HandlePlayerUnitDespawn;
            eventSubscriptionsInitialized = false;
        }

        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            CleanupEventSubscriptions();
        }

        public void HandlePlayerUnitSpawn() {
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