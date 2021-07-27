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

        public static MiniMapController Instance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<MiniMapController>();
                }

                return instance;
            }
        }

        #endregion

        [Header("MiniMap")]

        [SerializeField]
        private TextMeshProUGUI zoneNameText = null;

        [SerializeField]
        private GameObject mapButtonText = null;

        [SerializeField]
        private Image mapButtonImage = null;

        [SerializeField]
        private GameObject miniMapGraphic = null;

        [SerializeField]
        private RawImage miniMapGraphicRawImage = null;

        [SerializeField]
        private RectTransform miniMapGraphicRect = null;

        [SerializeField]
        private GameObject miniMapIndicatorPrefab = null;

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

        // the diameter in game meters that should be shown on the map
        private float cameraSize = 0f;

        // the object the map should center over
        private Transform followTransform = null;

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

        // keep track if mouse is in window bounds
        private Vector3[] worldCorners = new Vector3[4];

        // track all active indicators
        private Dictionary<Interactable, MiniMapIndicatorController> miniMapIndicatorControllers = new Dictionary<Interactable, MiniMapIndicatorController>();

        // track current minimap status and state
        private bool miniMapEnabled = false;

        private const string minimapTextureFolderBase = "Assets/Games/";
        private string minimapTextureFolder = string.Empty;

        private string loadedMapName = string.Empty;


        public override void Awake() {
            //Debug.Log(gameObject.name + ": MiniMapController.Awake()");
            base.Awake();
            if (SystemConfigurationManager.Instance.SystemBarMap != null) {
                mapButtonImage.sprite = SystemConfigurationManager.Instance.SystemBarMap;
                mapButtonImage.color = Color.white;
                mapButtonText.SetActive(false);
            } else {
                mapButtonImage.sprite = null;
                mapButtonImage.color = Color.black;
                mapButtonText.SetActive(true);
            }

            minimapTextureFolder = minimapTextureFolderBase + SystemConfigurationManager.Instance.GameName.Replace(" ", "") + "/Images/MiniMap/";
            
            //instantiate singleton
            MiniMapController tempcontroller = Instance;

            rectTransform = gameObject.GetComponent<RectTransform>();

            // set initial camera size
            if (PlayerPrefs.HasKey("MiniMapZoomLevel")) {
                cameraSize = PlayerPrefs.GetFloat("MiniMapZoomLevel");
            } else {
                cameraSize = cameraSizeDefault;
            }

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

        void Start() {
            //Debug.Log(gameObject.name + ": MiniMapController.Start()");
            if (followGameObject == null) {
                this.gameObject.SetActive(false);
            }
        }

        public void LateUpdate() {
            if (SystemConfigurationManager.Instance.UseThirdPartyCameraControl == true
                && CameraManager.Instance.ThirdPartyCamera.activeInHierarchy == true
                && PlayerManager.Instance.PlayerUnitSpawned == true) {
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
                miniMapIndicatorController.gameObject.SetActive(false);
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

        public MiniMapIndicatorController AddIndicator(Interactable interactable) {
            //Debug.Log("MinimapController.AddIndicator(" + interactable.gameObject.name + ")");
            if (miniMapIndicatorControllers.ContainsKey(interactable) == false) {
                GameObject miniMapIndicator = ObjectPooler.Instance.GetPooledObject(miniMapIndicatorPrefab, miniMapGraphic.transform);
                if (miniMapIndicator != null) {
                    MiniMapIndicatorController miniMapIndicatorController = miniMapIndicator.GetComponent<MiniMapIndicatorController>();
                    miniMapIndicatorControllers.Add(interactable, miniMapIndicatorController);
                    miniMapIndicatorController.SetInteractable(interactable);
                    if (miniMapEnabled == false) {
                        miniMapIndicatorController.gameObject.SetActive(false);
                    }
                }
            }

            return miniMapIndicatorControllers[interactable];
        }

        public void RemoveIndicator(Interactable interactable) {
            if (miniMapIndicatorControllers.ContainsKey(interactable)) {
                miniMapIndicatorControllers[interactable].ResetSettings();
                ObjectPooler.Instance.ReturnObjectToPool(miniMapIndicatorControllers[interactable].gameObject);
                miniMapIndicatorControllers.Remove(interactable);
            }
        }

        private void HandleCameraZoom(bool force = false) {
            if (InputManager.Instance.mouseScrolled || force) {

                // determine if mouse is inside this object
                rectTransform.GetWorldCorners(worldCorners);
                Vector3 mousePosition = Input.mousePosition;
                if (force != true && (mousePosition.x < worldCorners[0].x || mousePosition.x > worldCorners[2].x || mousePosition.y < worldCorners[0].y || mousePosition.y > worldCorners[2].y)) {
                    //Debug.Log("mouse scroll was outside of onscreen bounds.  ignoring!");
                    return;
                }

                cameraSize += (Input.GetAxis("Mouse ScrollWheel") * zoomSpeed * -1);
                cameraSize = Mathf.Clamp(cameraSize, minZoom, maxZoom);

                // camera size is the number of meters we want to show in the minimap window
                // requiredPixels is the number of pixels we need to display to show the requested number of meters
                float requiredPixels = levelScaleFactor * cameraSize;

                // a multiplier to determine what the image scale will need to be to fit the requested 'real pixels' into the space available in the minimap window
                imageScaleFactor = windowSize / requiredPixels;

                miniMapGraphic.transform.localScale = new Vector3(imageScaleFactor, imageScaleFactor, 1);

                PlayerPrefs.SetFloat("MiniMapZoomLevel", cameraSize);
            }

        }

        private void UpdateMapPosition() {
            // Position the texture such that the position desired is in the middle of the viewable rectangle
            // My coordinates on the image are = my world coordinates
            float playerX = followTransform.position.x;
            float playerZ = followTransform.position.z;

            // the image center is the player actual position modified by the offset (center of map compared to vector3.zero)
            // this value is then multiplied by the level scale factor (number of pixels per meter) and the image scale factor (current zoom level)
            // and made negative to account for the fact that we are moving the image center
            float imageCenterX = -1f * ((playerX - levelOffset.x) * levelScaleFactor) * imageScaleFactor;
            float imageCenterY = -1f * ((playerZ - levelOffset.z) * levelScaleFactor) * imageScaleFactor;

            //Debug.Log("scaleFactor: " + levelScaleFactor + "; player: " + followTransform.position + "; imageScaleFactor: " + imageScaleFactor + "; rawWidth: " + miniMapGraphicRawImage.texture.width + "; cameraSize: " + cameraSize + "; sceneBounds: " + LevelManager.Instance.SceneBounds.size);

            miniMapGraphicRect.anchoredPosition = new Vector2(imageCenterX, imageCenterY);
        }

        private void CommonInitialization() {
            SceneNode sceneNode = LevelManager.Instance.GetActiveSceneNode();
            if (sceneNode != null) {
                zoneNameText.text = sceneNode.DisplayName;
            } else {
                zoneNameText.text = SceneManager.GetActiveScene().name;
            }
            this.gameObject.SetActive(true);
            StartCoroutine(WaitForFollowTarget());

            InitRenderedMinimap();
            HandleCameraZoom(true); // Force the image to be zoomed correctly for the first rendering
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

            this.followTransform = targetBone;
            initialized = true;
            yield return null;
        }

        /// <summary>
        /// Load the pre-rendered minimap texture into the appropriate component
        /// </summary>
        void InitRenderedMinimap() {

            if (loadedMapName == SceneManager.GetActiveScene().name) {
                // current map is already loaded or rendered
                return;
            }

            // First, try to find the minimap
            Texture2D mapTexture = new Texture2D((int)LevelManager.Instance.SceneBounds.size.x, (int)LevelManager.Instance.SceneBounds.size.z);
            string textureFilePath = minimapTextureFolder + GetScreenshotFilename();
            if (System.IO.File.Exists(textureFilePath)) {
                //sceneTextureFound = true;
                miniMapEnabled = true;
                byte[] fileData = System.IO.File.ReadAllBytes(textureFilePath);
                mapTexture.LoadImage(fileData);
            } else {
                //Debug.Log("No minimap texture exists at " + textureFilePath + ".  Please run \"Minimap Wizard\" from the Tools menu under AnyRPG.");
                if (SystemConfigurationManager.Instance.MiniMapFallBackMode == MiniMapFallBackMode.None) {
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
            // it assumes a square image whose factor is based on the largest scene dimension
            levelScaleFactor = miniMapGraphicRawImage.texture.width / (LevelManager.Instance.SceneBounds.size.x > LevelManager.Instance.SceneBounds.size.z ? LevelManager.Instance.SceneBounds.size.x : LevelManager.Instance.SceneBounds.size.z);
            levelOffset = LevelManager.Instance.SceneBounds.center;

            EnableIndicators();

            loadedMapName = SceneManager.GetActiveScene().name;
        }

        /// <summary>
        /// Return the standardized name of the minimap image file
        /// </summary>
        /// <returns></returns>
        public string GetScreenshotFilename() {
            return SceneManager.GetActiveScene().name + ".png";
        }

        public void OpenMainMap() {
            PopupWindowManager.Instance.mainMapWindow.ToggleOpenClose();
        }

        public void CleanupEventSubscriptions() {
            SystemEventManager.StopListening("AfterCameraUpdate", HandleAfterCameraUpdate);
            SystemEventManager.StopListening("OnLevelUnload", HandleLevelUnload);
        }

        public void OnDestroy() {
            CleanupEventSubscriptions();
        }

    }

    public enum MiniMapFallBackMode { None, Empty };
}