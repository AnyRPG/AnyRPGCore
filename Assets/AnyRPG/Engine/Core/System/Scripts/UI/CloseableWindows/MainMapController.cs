using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    public class MainMapController : WindowContentController {

        #region Singleton
        private static MainMapController instance;

        public static MainMapController MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<MainMapController>();
                }

                return instance;
            }
        }

        #endregion


        [Header("Map")]

        [SerializeField]
        private LayoutElement graphicLayoutElement = null;

        [SerializeField]
        private RawImage mapRawImage = null;

        [SerializeField]
        private RectTransform mainMapBackground = null;

        [SerializeField]
        private GameObject mapGraphic = null;

        [SerializeField]
        private GameObject mapIndicatorPrefab = null;

        private const string mainmapTextureFolderBase = "Assets/Games/";
        private string mainmapTextureFolder = string.Empty;

        private string loadedMapName = string.Empty;

        private float cameraSize = 0f;

        // the number of pixels per meter of level based on the total map pixels
        private float levelScaleFactor = 1f;

        protected bool eventSubscriptionsInitialized = false;

        private Dictionary<Interactable, MainMapIndicatorController> mapIndicatorControllers = new Dictionary<Interactable, MainMapIndicatorController>();


        public override void Awake() {
            //Debug.Log("MainMapController.Awake()");
            base.Awake();
            //instantiate singleton
            MainMapController tempcontroller = MyInstance;
            CameraManager.MyInstance.MainMapCamera.enabled = false;

            mainmapTextureFolder = mainmapTextureFolderBase + SystemConfigurationManager.MyInstance.GameName.Replace(" ", "") + "/Images/MiniMap/";

            SystemEventManager.StartListening("AfterCameraUpdate", HandleAfterCameraUpdate);
            SystemEventManager.StartListening("OnLevelUnload", HandleLevelUnload);
        }

        public void HandleAfterCameraUpdate(string eventName, EventParamProperties eventParamProperties) {
            UpdateMainMap();
        }

        void UpdateMainMap() {
            if (PopupWindowManager.MyInstance.mainMapWindow.IsOpen == false) {
                return;
            }

            UpdateIndicatorPositions();
        }

        public void LateUpdate() {
            if (PopupWindowManager.MyInstance.mainMapWindow.IsOpen == false) {
                return;
            }
            if (SystemConfigurationManager.MyInstance.UseThirdPartyCameraControl == true
                && CameraManager.MyInstance.ThirdPartyCamera.activeInHierarchy == true
                && PlayerManager.MyInstance.PlayerUnitSpawned == true) {
                UpdateMainMap();
            }
        }

        private void UpdateIndicatorPositions() {
            foreach (Interactable interactable in mapIndicatorControllers.Keys) {
                if (mapIndicatorControllers[interactable].gameObject.activeSelf == true) {
                    mapIndicatorControllers[interactable].transform.localPosition = new Vector3((interactable.transform.position.x - LevelManager.MyInstance.SceneBounds.center.x) * levelScaleFactor, (interactable.transform.position.z - LevelManager.MyInstance.SceneBounds.center.z) * levelScaleFactor, 0);
                    mapIndicatorControllers[interactable].transform.localScale = new Vector3(1f / mapGraphic.transform.localScale.x, 1f / mapGraphic.transform.localScale.y, 1f / mapGraphic.transform.localScale.z);
                    interactable.UpdateMainMapIndicator();
                }
            }
        }

        public void HandleLevelUnload(string eventName, EventParamProperties eventParamProperties) {
            List<Interactable> removeList = new List<Interactable>();
            removeList.AddRange(mapIndicatorControllers.Keys);
            foreach (Interactable interactable in removeList) {
                RemoveIndicator(interactable);
            }
        }

        public MainMapIndicatorController AddIndicator(Interactable interactable) {
            //Debug.Log("MainMapController.AddIndicator(" + interactable.gameObject.name + ")");
            if (mapIndicatorControllers.ContainsKey(interactable) == false) {
                GameObject mainMapIndicator = ObjectPooler.MyInstance.GetPooledObject(mapIndicatorPrefab, mapGraphic.transform);
                if (mainMapIndicator != null) {
                    MainMapIndicatorController mapIndicatorController = mainMapIndicator.GetComponent<MainMapIndicatorController>();
                    if (mapIndicatorController != null) {
                        mapIndicatorControllers.Add(interactable, mapIndicatorController);
                        mapIndicatorController.SetInteractable(interactable);
                    }
                }
            }

            return mapIndicatorControllers[interactable];
        }

        public void RemoveIndicator(Interactable interactable) {
            if (mapIndicatorControllers.ContainsKey(interactable)) {
                mapIndicatorControllers[interactable].ResetSettings();
                ObjectPooler.MyInstance.ReturnObjectToPool(mapIndicatorControllers[interactable].gameObject);
                mapIndicatorControllers.Remove(interactable);
            }
        }

        protected void Start() {
            //Debug.Log(gameObject.name + ".MainMapController.Start()");
            CreateEventSubscriptions();
        }

        private void OnEnable() {
            //Debug.Log("MainMapController.OnEnable()");
            CreateEventSubscriptions();
        }

        private void CreateEventSubscriptions() {
            //Debug.Log("MainMapController.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StartListening("OnLevelLoad", HandleLevelLoad);
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StopListening("OnLevelLoad", HandleLevelLoad);
            eventSubscriptionsInitialized = false;
        }

        // onDestroy instead of OnDisable so not accidentally removing these since the map is always disabled until the window is opened
        public void OnDestroy() {
            //Debug.Log("PlayerManager.OnDisable()");
            CleanupEventSubscriptions();
        }

        public void HandleLevelLoad(string eventName, EventParamProperties eventParamProperties) {
            InitializeMap();
        }

        public void InitializeMap() {
            //Debug.Log(gameObject.name + ": MainMapController.InitializeMap()");

            if (loadedMapName == SceneManager.GetActiveScene().name) {
                // current map is already loaded or rendered
                return;
            }

            // First, try to find the the map image
            Texture2D mapTexture = new Texture2D((int)LevelManager.MyInstance.SceneBounds.size.x, (int)LevelManager.MyInstance.SceneBounds.size.z);
            string textureFilePath = mainmapTextureFolder + GetScreenshotFilename();
            if (System.IO.File.Exists(textureFilePath)) {
                //sceneTextureFound = true;
                byte[] fileData = System.IO.File.ReadAllBytes(textureFilePath);
                mapTexture.LoadImage(fileData);
                mapRawImage.texture = mapTexture;
            } else {
                // if a map image could not be found, take a picture
                UpdateCameraSize();
                UpdateCameraPosition();
                CameraManager.MyInstance.MainMapCamera.Render();
            }
            loadedMapName = SceneManager.GetActiveScene().name;
        }

        /// <summary>
        /// Return the standardized name of the map image file
        /// </summary>
        /// <returns></returns>
        public string GetScreenshotFilename() {
            return SceneManager.GetActiveScene().name + ".png";
        }

        private void UpdateCameraSize() {
            //Debug.Log("MainMapController.UpdateCameraSize()");
            //float newCameraSize = cameraSizeDefault;
            cameraSize = Mathf.Max(LevelManager.MyInstance.SceneBounds.extents.x, LevelManager.MyInstance.SceneBounds.extents.z);
            CameraManager.MyInstance.MainMapCamera.orthographicSize = cameraSize;
        }

        private void UpdateCameraPosition() {
            //Debug.Log("MainMapController.UpdateCameraPosition()");
            Vector3 wantedPosition = new Vector3(LevelManager.MyInstance.SceneBounds.center.x, LevelManager.MyInstance.SceneBounds.center.y + LevelManager.MyInstance.SceneBounds.extents.y + 1f, LevelManager.MyInstance.SceneBounds.center.z);
            //Debug.Log("MainMapController.UpdateCameraPosition() wantedposition: " + wantedPosition);
            Vector3 wantedLookPosition = new Vector3(LevelManager.MyInstance.SceneBounds.center.x, LevelManager.MyInstance.SceneBounds.center.y, LevelManager.MyInstance.SceneBounds.center.z);
            //Debug.Log("MainMapController.UpdateCameraPosition() wantedLookPosition: " + wantedLookPosition);
            CameraManager.MyInstance.MainMapCamera.transform.position = wantedPosition;
            CameraManager.MyInstance.MainMapCamera.transform.LookAt(wantedLookPosition);
        }

        private void CommonInitialization() {
            //Debug.Log("MainMapController.CommonInitialization()");
            //zoneNameText.text = SceneManager.GetActiveScene().name;
            this.gameObject.SetActive(true);
        }

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("MainMapController.OnCloseWindow()");
            base.RecieveClosedWindowNotification();
            CameraManager.MyInstance.MainMapCamera.enabled = false;
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("MainMapController.OnOpenWindow()");
            // re-adding this back here.  Not sure why, but possible the level objects aren't rendered by the time this gets called in onlevelload.  Trying on every open window
            InitializeMap();
            loadedMapName = SceneManager.GetActiveScene().name;

            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

            // set the width and height of the image to a square the size of the smallest side
            graphicLayoutElement.preferredWidth = Mathf.Min(mainMapBackground.rect.width, mainMapBackground.rect.height);
            graphicLayoutElement.preferredHeight = graphicLayoutElement.preferredWidth;

            // the image will be scaled to the largest dimension
            levelScaleFactor = graphicLayoutElement.preferredWidth / (LevelManager.MyInstance.SceneBounds.size.x > LevelManager.MyInstance.SceneBounds.size.z ? LevelManager.MyInstance.SceneBounds.size.x : LevelManager.MyInstance.SceneBounds.size.z);
        }
    }

}