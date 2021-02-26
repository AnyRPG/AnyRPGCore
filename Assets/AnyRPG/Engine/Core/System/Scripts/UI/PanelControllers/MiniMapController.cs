using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
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

        [Header("MiniMap")]

        [SerializeField]
        private TextMeshProUGUI zoneNameText = null;

        [SerializeField]
        private GameObject mapButtonText = null;

        [SerializeField]
        private Image mapButtonImage = null;

        [SerializeField]
        private Transform cameraTransform = null;

        [SerializeField]
        private GameObject miniMapGraphic = null;

        [SerializeField]
        private RawImage miniMapGraphicRawImage = null;

        [SerializeField]
        private RectTransform miniMapGraphicRect = null;

        [SerializeField]
        private GameObject miniMapIndicatorPrefab = null;

        [SerializeField]
        private float cameraOffsetYDefault = 5f;

        [Tooltip("The default number of meters to display on the minimap")]
        [SerializeField]
        private float cameraSizeDefault = 20f;

        [Tooltip("The smallest number of meters to display on the minimap (full zoomed in)")]
        [SerializeField]
        private float minZoom = 20f;

        [Tooltip("The largest number of meters to display on the minimap (full zoomed out)")]
        [SerializeField]
        private float maxZoom = 50f;

        [SerializeField]
        private float zoomSpeed = 10f;

        [SerializeField]
        private GameObject followGameObject = null;

        //private float cameraOffsetZ = 0f;
        private float cameraOffsetY = 0f;
        private float cameraSize = 0f;

        private Transform followTransform = null;

        //private float zoomMultiplier = 1f;

        private bool initialized = false;
        //private bool sceneTextureFound = false;

        // keep track of the width in pixels of the map window
        private float windowSize = 150f;

        // the number of pixels per meter of level based on the total map pixels
        private float levelScaleFactor = 1f;

        // the center of the scene bounds
        private Vector3 levelOffset = Vector3.zero;

        // a multiplier to determine what the image scale will need to be to fit the requested 'real pixels' into the space available in the minimap window
        private float imageScaleFactor = 1f;

        // rect transform of the object this script is on
        private RectTransform rectTransform = null;

        private Vector3[] worldCorners = new Vector3[4];

        private Dictionary<Interactable, MiniMapIndicatorController> miniMapIndicatorControllers = new Dictionary<Interactable, MiniMapIndicatorController>();

        // track current minimap status and state
        private bool miniMapEnabled = false;
        private MiniMapSource miniMapSource = MiniMapSource.Disk;

        public override void Awake() {
            base.Awake();
            if (SystemConfigurationManager.MyInstance.SystemBarMap != null) {
                mapButtonImage.sprite = SystemConfigurationManager.MyInstance.SystemBarMap;
                mapButtonImage.color = Color.white;
                mapButtonText.SetActive(false);
            } else {
                mapButtonImage.sprite = null;
                mapButtonImage.color = Color.black;
                mapButtonText.SetActive(true);
            }

            minimapTextureFolder = minimapTextureFolderBase + SystemConfigurationManager.MyInstance.GameName.Replace(" ", "") + "/Images/MiniMap/";
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

            SystemEventManager.StartListening("AfterCameraUpdate", HandleAfterCameraUpdate);
            SystemEventManager.StartListening("OnLevelUnload", HandleLevelUnload);
        }

        public void HandleAfterCameraUpdate(string eventName, EventParamProperties eventParamProperties) {
            UpdateMiniMap();
        }

        public void HandleLevelUnload(string eventName, EventParamProperties eventParamProperties) {
            List<Interactable> removeList = new List<Interactable>();
            removeList.AddRange(miniMapIndicatorControllers.Keys);
            foreach (Interactable interactable in removeList) {
                RemoveIndicator(interactable);
            }

        }

        // Start is called before the first frame update
        void Start() {
            //Debug.Log(gameObject.name + ": MiniMapController.Start()");
            if (followGameObject != null) {
                CommonInitialization();
            } else {
                this.gameObject.SetActive(false);
            }
        }

        public void LateUpdate() {
            if (SystemConfigurationManager.MyInstance.UseThirdPartyCameraControl == true
                && CameraManager.MyInstance.ThirdPartyCamera.activeInHierarchy == true
                && PlayerManager.MyInstance.PlayerUnitSpawned == true) {
                UpdateMiniMap();
            }
        }

        void UpdateMiniMap() {
            if (initialized == false || miniMapEnabled == false) {
                //Debug.Log("MiniMapController.Update(): not initialized yet.  Exiting!");
                return;
            }
            if (followTransform == null) {
                //Debug.Log("MiniMapController.Update(): followTransform is null.  Exiting!");
                return;
            }
            /*
            if (sceneTextureFound == false) {
                return;
            }
            */

            UpdateCameraSize();
            HandleCameraZoom();
            UpdateMapPosition();
            UpdateIndicatorPositions();
        }

        private void EnableIndicators() {
            foreach (MiniMapIndicatorController miniMapIndicatorController in miniMapIndicatorControllers.Values) {
                miniMapIndicatorController.gameObject.SetActive(true);
            }
        }

        private void DisableIndicators() {
            foreach (MiniMapIndicatorController miniMapIndicatorController in miniMapIndicatorControllers.Values) {
                miniMapIndicatorController.gameObject.SetActive(true);
            }
        }

        private void UpdateIndicatorPositions() {
            foreach (Interactable interactable in miniMapIndicatorControllers.Keys) {
                if (miniMapIndicatorControllers[interactable].gameObject.activeSelf == true) {
                    miniMapIndicatorControllers[interactable].transform.localPosition = new Vector3((interactable.transform.position.x - levelOffset.x) * levelScaleFactor, (interactable.transform.position.z - levelOffset.z) * levelScaleFactor, 0);
                    miniMapIndicatorControllers[interactable].transform.localScale = new Vector3(1f / miniMapGraphic.transform.localScale.x, 1f / miniMapGraphic.transform.localScale.y, 1f / miniMapGraphic.transform.localScale.z);
                    interactable.UpdateMiniMapIndicator();
                }
            }
        }

        public GameObject AddIndicator(Interactable interactable) {
            //Debug.Log("MinimapController.AddIndicator(" + interactable.gameObject.name + ")");
            if (miniMapIndicatorControllers.ContainsKey(interactable) == false) {
                GameObject miniMapIndicator = Instantiate(miniMapIndicatorPrefab, miniMapGraphic.transform);
                MiniMapIndicatorController miniMapIndicatorController = miniMapIndicator.GetComponent<MiniMapIndicatorController>();
                miniMapIndicatorControllers.Add(interactable, miniMapIndicatorController);
                miniMapIndicatorController.SetInteractable(interactable);
                if (miniMapEnabled == false) {
                    miniMapIndicatorController.gameObject.SetActive(false);
                }
            }

            return miniMapIndicatorControllers[interactable].gameObject;
        }

        public void RemoveIndicator(Interactable interactable) {
            if (miniMapIndicatorControllers.ContainsKey(interactable)) {
                Destroy(miniMapIndicatorControllers[interactable].gameObject);
                miniMapIndicatorControllers.Remove(interactable);
            }
        }

        private void HandleCameraZoom(bool force = false) {
            if (InputManager.MyInstance.mouseScrolled || force) {

                rectTransform.GetWorldCorners(worldCorners);
                Vector3 mousePosition = Input.mousePosition;
                //Debug.Log("mouse position: " + mousePosition);
                //Debug.Log("World Corners");
                for (var i = 0; i < 4; i++) {
                    //Debug.Log("World Corner " + i + " : " + worldCorners[i]);
                }
                if (force != true && (mousePosition.x < worldCorners[0].x || mousePosition.x > worldCorners[2].x || mousePosition.y < worldCorners[0].y || mousePosition.y > worldCorners[2].y)) {
                    //Debug.Log("mouse scroll was outside of onscreen bounds.  ignoring!");
                    return;
                }

                cameraSize += (Input.GetAxis("Mouse ScrollWheel") * zoomSpeed * -1);
                cameraSize = Mathf.Clamp(cameraSize, minZoom, maxZoom);
                if (miniMapSource == MiniMapSource.Camera) {
                    // nothing here yet
                } else if (miniMapSource == MiniMapSource.Disk) {

                    // camera size is the number of meters we want to show in the minimap window
                    // requiredPixels is the number of pixels we need to display to show the requested number of meters
                    float requiredPixels = levelScaleFactor * cameraSize;

                    // a multiplier to determine what the image scale will need to be to fit the requested 'real pixels' into the space available in the minimap window
                    imageScaleFactor = windowSize / requiredPixels;

                    miniMapGraphic.transform.localScale = new Vector3(imageScaleFactor, imageScaleFactor, 1);

                }
                PlayerPrefs.SetFloat("MiniMapZoomLevel", cameraSize);
            }

        }
        private void UpdateCameraSize() {
            CameraManager.MyInstance.MiniMapCamera.orthographicSize = cameraSize;
        }

        private void UpdateMapPosition() {

            if (miniMapSource == MiniMapSource.Camera) {
                //Vector3 wantedPosition = followTransform.TransformPoint(0, cameraOffsetY, cameraOffsetZ);
                // position the camera over top of the player
                Vector3 wantedPosition = new Vector3(followTransform.position.x, followTransform.position.y + cameraOffsetY, followTransform.position.z);
                Vector3 wantedLookPosition = new Vector3(followTransform.position.x, followTransform.position.y, followTransform.position.z);
                cameraTransform.position = wantedPosition;
                cameraTransform.LookAt(wantedLookPosition);
            } else if (miniMapSource == MiniMapSource.Disk) {
                // Position the texture such that the position desired is in the middle of the viewable rectangle

                // My coordinates on the image are = my world coordinates
                float playerX = followTransform.position.x;
                float playerZ = followTransform.position.z;

                // the image center is the player actual position modified by the offset (center of map compared to vector3.zero)
                // this value is then multiplied by the level scale factor (number of pixels per meter) and the image scale factor (current zoom level)
                // and made negative to account for the fact that we are moving the image center
                float imageCenterX = -1f * ((playerX - levelOffset.x) * levelScaleFactor) * imageScaleFactor;
                float imageCenterY = -1f * ((playerZ - levelOffset.z) * levelScaleFactor) * imageScaleFactor;

                //Debug.Log("scaleFactor: " + levelScaleFactor + "; player: " + followTransform.position + "; imageScaleFactor: " + imageScaleFactor + "; rawWidth: " + miniMapGraphicRawImage.texture.width + "; cameraSize: " + cameraSize + "; sceneBounds: " + LevelManager.MyInstance.SceneBounds.size);

                miniMapGraphicRect.anchoredPosition = new Vector2(imageCenterX, imageCenterY);
            }
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

            if (SystemConfigurationManager.MyInstance.RealTimeMiniMap == false) {
                InitRenderedMinimap();
                HandleCameraZoom(true); // Force the image to be zoomed correctly for the first rendering
            }
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

        /// <summary>
        /// Loads the pre-rendered minimap texture into the appropriate component.  Logs error and does nothing if 
        /// No pre-rendered minimap is found.
        /// </summary>
        void InitRenderedMinimap() {
            //sceneTextureFound = false;

            // First, try to find the minimap
            Texture2D mapTexture = new Texture2D((int)LevelManager.MyInstance.SceneBounds.size.x, (int)LevelManager.MyInstance.SceneBounds.size.z);
            string textureFilePath = minimapTextureFolder + GetScreenshotFilename();
            if (System.IO.File.Exists(textureFilePath)) {
                //sceneTextureFound = true;
                miniMapEnabled = true;
                byte[] fileData = System.IO.File.ReadAllBytes(textureFilePath);
                mapTexture.LoadImage(fileData);
                // Normalize to the width/height of the image
            } else {
                Debug.Log("No minimap texture exists at " + textureFilePath + ".  Please run \"Minimap Wizard\" from the Tools menu under AnyRPG.");
                if (SystemConfigurationManager.MyInstance.MiniMapFallBackMode == MiniMapFallBackMode.None) {
                    DisableIndicators();
                    return;
                }
                miniMapEnabled = true;
                //miniMapGraphicRect.sizeDelta = new Vector2(mapTexture.width, mapTexture.height);
                //return;
            }
            miniMapGraphicRawImage.texture = mapTexture;
            miniMapGraphicRect.sizeDelta = new Vector2(mapTexture.width, mapTexture.height);

            GameObject parentObject = miniMapGraphic.transform.parent.gameObject;
            if (parentObject == null) {
                Debug.LogError("Could not find parent object of minimap raw image.  Unable to set rectangle mask on pre-rendered minimap image.");
            }
            windowSize = parentObject.GetComponent<RectTransform>().rect.width;

            // scale factor gives the number of pixels per meter for this image
            levelScaleFactor = miniMapGraphicRawImage.texture.width / LevelManager.MyInstance.SceneBounds.size.x;
            levelOffset = LevelManager.MyInstance.SceneBounds.center;

            EnableIndicators();
        }

        /// <summary>
        /// Return the standardized name of the minimap image file
        /// </summary>
        /// <returns></returns>
        public string GetScreenshotFilename() {
            return SceneManager.GetActiveScene().name + ".png";
        }

        public void OpenMainMap() {
            PopupWindowManager.MyInstance.mainMapWindow.ToggleOpenClose();
        }

        public void CleanupEventSubscriptions() {
            SystemEventManager.StopListening("AfterCameraUpdate", HandleAfterCameraUpdate);
            SystemEventManager.StopListening("OnLevelUnload", HandleLevelUnload);
        }

        public void OnDestroy() {
            CleanupEventSubscriptions();
        }

    }

    public enum MiniMapSource { Disk, Camera };
    public enum MiniMapFallBackMode { None, Empty, Render, RealTime };
}