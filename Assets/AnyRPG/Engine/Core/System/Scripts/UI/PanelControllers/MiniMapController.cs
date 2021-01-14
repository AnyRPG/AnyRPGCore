using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    public class MiniMapController : DraggableWindow {

        #region Singleton
        private static MiniMapController instance;

        public static MiniMapController MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<MiniMapController>();
                }

                return instance;
            }
        }

        #endregion

        // objects in the player stats window

        [SerializeField]
        private TextMeshProUGUI zoneNameText = null;

        [SerializeField]
        private Transform cameraTransform = null;

        [SerializeField]
        private GameObject miniMapIndicatorPrefab = null;

        [SerializeField]
        private float cameraOffsetYDefault = 5f;

        [SerializeField]
        private float cameraSizeDefault = 10f;

        [SerializeField]
        private float minZoom = 5f;

        [SerializeField]
        private float maxZoom = 25f;

        [SerializeField]
        private float zoomSpeed = 4f;

        [SerializeField]
        private GameObject followGameObject = null;

        //private float cameraOffsetZ = 0f;
        private float cameraOffsetY = 0f;
        private float cameraSize = 0f;

        private Transform followTransform = null;

        private bool initialized = false;

        private RectTransform rectTransform = null;
        private Vector3[] worldCorners = new Vector3[4];

        public GameObject MyMiniMapIndicatorPrefab { get => miniMapIndicatorPrefab; set => miniMapIndicatorPrefab = value; }

        public override void Awake() {
            base.Awake();
            //Debug.Log(gameObject.name + ": MiniMapController.Awake()");
            //instantiate singleton
            MiniMapController tempcontroller = MyInstance;

            rectTransform = gameObject.GetComponent<RectTransform>();

            // set initial camera size
            if (PlayerPrefs.HasKey("MiniMapZoomLevel")) {
                cameraSize = PlayerPrefs.GetFloat("MiniMapZoomLevel");
            } else {
                cameraSize = cameraSizeDefault;
            }
            UpdateCameraSize();
        }

        // Start is called before the first frame update
        void Start() {
            //Debug.Log(gameObject.name + ": MiniMapController.Start()");
            if (followGameObject != null) {
                CommonInitialization();
            } else {
                this.gameObject.SetActive(false);
            }
            // get bounds for comparing mouseposition
        }

        void Update() {
            if (!initialized) {
                //Debug.Log("MiniMapController.Update(): not initialized yet.  Exiting!");
                return;
            }
            if (followTransform == null) {
                //Debug.Log("MiniMapController.Update(): followTransform is null.  Exiting!");
                return;
            }
            UpdateCameraSize();
            HandleCameraZoom();
            UpdateCameraPosition();
        }

        private void HandleCameraZoom() {
            if (InputManager.MyInstance.mouseScrolled) {
                rectTransform.GetWorldCorners(worldCorners);
                Vector3 mousePosition = Input.mousePosition;
                //Debug.Log("mouse position: " + mousePosition);
                //Debug.Log("World Corners");
                for (var i = 0; i < 4; i++) {
                    //Debug.Log("World Corner " + i + " : " + worldCorners[i]);
                }
                if (mousePosition.x < worldCorners[0].x || mousePosition.x > worldCorners[2].x || mousePosition.y < worldCorners[0].y || mousePosition.y > worldCorners[2].y) {
                    //Debug.Log("mouse scroll was outside of onscreen bounds.  ignoring!");
                    return;
                }
                //Debug.Log("gameobject position: " + gameObject.GetComponent<RectTransform>().GetWorldCorners());
                //Debug.Log("Mouse Scrollwheel: " + Input.GetAxis("Mouse ScrollWheel"));
                /*
                cameraOffsetY += (Input.GetAxis("Mouse ScrollWheel") * zoomSpeed * -1);
                cameraOffsetY = Mathf.Clamp(cameraOffsetY, minZoom, maxZoom);
                */
                cameraSize += (Input.GetAxis("Mouse ScrollWheel") * zoomSpeed * -1);
                cameraSize = Mathf.Clamp(cameraSize, minZoom, maxZoom);
                PlayerPrefs.SetFloat("MiniMapZoomLevel", cameraSize);
            }

        }
        private void UpdateCameraSize() {
            CameraManager.MyInstance.MiniMapCamera.orthographicSize = cameraSize;
        }

        private void UpdateCameraPosition() {
            //Vector3 wantedPosition = followTransform.TransformPoint(0, cameraOffsetY, cameraOffsetZ);
            Vector3 wantedPosition = new Vector3(followTransform.position.x, followTransform.position.y + cameraOffsetY, followTransform.position.z);
            Vector3 wantedLookPosition = new Vector3(followTransform.position.x, followTransform.position.y, followTransform.position.z);
            cameraTransform.position = wantedPosition;
            cameraTransform.LookAt(wantedLookPosition);
        }

        private void CommonInitialization() {
            SceneNode sceneNode = LevelManager.MyInstance.GetActiveSceneNode();
            if (sceneNode != null) {
                zoneNameText.text = sceneNode.DisplayName;
            } else {
                zoneNameText.text = SceneManager.GetActiveScene().name;
            }
            this.gameObject.SetActive(true);
            StartCoroutine(WaitForFollowTarget());
        }

        public void SetTarget(GameObject target) {
            //Debug.Log("MiniMapController setting target: " + target.name);
            followGameObject = target;
            CommonInitialization();
        }

        public void ClearTarget() {

            followGameObject = null;
            this.gameObject.SetActive(false);
        }


        private IEnumerator WaitForFollowTarget() {
            Transform targetBone = followGameObject.transform;
            cameraOffsetY = cameraOffsetYDefault;

            this.followTransform = targetBone;
            initialized = true;
            yield return null;
        }


    }

}