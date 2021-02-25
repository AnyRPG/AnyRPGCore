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

        private const string minimapTextureFolderBase = "Assets/Games/";
        private string minimapTextureFolder = string.Empty;

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

        private GameObject miniMapGraphic = null;
        private RawImage miniMapGraphicRawImage = null;
        private float zoomMultiplier = 1f;

        private bool initialized = false;

        private RectTransform rectTransform = null;
        private Vector3[] worldCorners = new Vector3[4];

        public GameObject MyMiniMapIndicatorPrefab { get => miniMapIndicatorPrefab; set => miniMapIndicatorPrefab = value; }

        public override void Awake() {
            base.Awake();
            minimapTextureFolder = minimapTextureFolderBase + SystemConfigurationManager.MyInstance.GameName.Replace(" ", "") + "/Images/MiniMap/";
            //Debug.Log(gameObject.name + ": MiniMapController.Awake()");
            //instantiate singleton
            MiniMapController tempcontroller = MyInstance;

            rectTransform = gameObject.GetComponent<RectTransform>();

            // NOTE: Not sure about grabbing this by name
            miniMapGraphic = GameObject.Find("MiniMapGraphic");

            miniMapGraphicRawImage = miniMapGraphic.GetComponent<RawImage>();

            // set initial camera size
            if (PlayerPrefs.HasKey("MiniMapZoomLevel")) {
                cameraSize = PlayerPrefs.GetFloat("MiniMapZoomLevel");
            } else {
                cameraSize = cameraSizeDefault;
            }
            UpdateCameraSize();
            InitRenderedMinimap();
            HandleCameraZoom(true); // Force the image to be zoomed correctly for the first rendering
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

        private void HandleCameraZoom(bool force = false) {
            if (InputManager.MyInstance.mouseScrolled || force) {

                // NOTE: A litle "Translation" between the zoom range and the scale factor of the raw image
                // I didn't just change the range, because these are stored in user preferences and I wanted
                // to disturb as little as possible.  You may choose to just change the constants.
                cameraSize = Mathf.Clamp(cameraSize, minZoom, maxZoom);
                float imageZoomMax = 1;
                float imageZoomMin = 0;
                zoomMultiplier = (imageZoomMax - imageZoomMin) / (maxZoom - minZoom);

                miniMapGraphic.transform.localScale = new Vector3(cameraSize * zoomMultiplier, cameraSize * zoomMultiplier, 1);

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

            // Position the texture such that the position desired is in the middle of the viewable rectangle
            // Assume a square minimap here
            float scaleFactor = miniMapGraphicRawImage.texture.width / MainMapController.MyInstance.GetSceneBounds().size.x;

            // My coordinates on the image are = my world coordinates
            float playerX = followTransform.position.x;
            float playerZ = followTransform.position.z;

            // NOTE:  Not sure why I am dividing by two here, but it works.  Possibly because the pivot for the image is in the center?
            float imageCenterX = -1 * (playerX * scaleFactor) / 2 * zoomMultiplier * cameraSize;
            float imageCenterY = -1 * (playerZ * scaleFactor) / 2 * zoomMultiplier * cameraSize;

            miniMapGraphic.GetComponent<RectTransform>().anchoredPosition = new Vector2(imageCenterX, imageCenterY);

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

        /*
         * Loads the pre-rendered minimap texture into the appropriate component.  Logs error and does nothing if 
         * No pre-rendered minimap is found.
         */
        void InitRenderedMinimap() {
            // First, try to find the minimap
            string textureFilePath = minimapTextureFolder + "/" + GetScreenshotFilename();
            if (!System.IO.File.Exists(textureFilePath))
            {
                Debug.LogError("No minimap texture exists at " + textureFilePath + ".  Please run \"Generate Minimap\" from the Tools menu.");
                return;
            }
            byte[] fileData = System.IO.File.ReadAllBytes(textureFilePath);
            Texture2D mapTexture = new Texture2D(2, 2);
            mapTexture.LoadImage(fileData);

            miniMapGraphicRawImage.texture = mapTexture;
            // Normalize to the width/height of the image
            miniMapGraphic.GetComponent<RectTransform>().sizeDelta = new Vector2(mapTexture.width, mapTexture.height);

            GameObject parentObject = miniMapGraphic.transform.parent.gameObject;
            if (parentObject == null)
            {
                Debug.LogError("Could not find parent object of minimap raw image.  Unable to set rectangle mask on pre-rendered minimap image.");
            }
            RectMask2D rectMask = parentObject.GetComponent<RectMask2D>();
            if (rectMask == null)
            {
                parentObject.AddComponent<RectMask2D>();
            }
        }

        /*
         * Returns the standardized name of the minimap image file
         */
        public string GetScreenshotFilename() {
            return SceneManager.GetActiveScene().name + "_minimap.png";
        }
    }
}