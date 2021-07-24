using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    public class MainMapController : WindowContentController {

        [Header("Map")]

        [SerializeField]
        private LayoutElement graphicLayoutElement = null;

        [SerializeField]
        private RawImage mapRawImage = null;

        [SerializeField]
        private RectTransform mainMapBackground = null;

        [SerializeField]
        private GameObject mapGraphic = null;

        private const string mainmapTextureFolderBase = "Assets/Games/";
        private string mainmapTextureFolder = string.Empty;

        private string loadedMapName = string.Empty;

        private float cameraSize = 0f;

        // the number of pixels per meter of level based on the total map pixels
        private float levelScaleFactor = 1f;

        private bool activeEventSubscriptionsInitialized = false;

        public GameObject MapGraphic { get => mapGraphic; }

        public override void Init() {
            //Debug.Log("MainMapController.Awake()");
            base.Init();
            //instantiate singleton
            CameraManager.MyInstance.MainMapCamera.enabled = false;

            mainmapTextureFolder = mainmapTextureFolderBase + SystemConfigurationManager.Instance.GameName.Replace(" ", "") + "/Images/MiniMap/";

        }

        public void HandleAfterCameraUpdate(string eventName, EventParamProperties eventParamProperties) {
            UpdateMainMap();
        }

        void UpdateMainMap() {
            if (PopupWindowManager.Instance.mainMapWindow.IsOpen == false) {
                return;
            }

            UpdateIndicatorPositions();
        }

        public void LateUpdate() {
            if (PopupWindowManager.Instance.mainMapWindow.IsOpen == false) {
                return;
            }
            if (SystemConfigurationManager.Instance.UseThirdPartyCameraControl == true
                && CameraManager.MyInstance.ThirdPartyCamera.activeInHierarchy == true
                && PlayerManager.MyInstance.PlayerUnitSpawned == true) {
                UpdateMainMap();
            }
        }

        private void UpdateIndicatorPositions() {
            foreach (Interactable interactable in MainMapManager.MyInstance.MapIndicatorControllers.Keys) {
                if (MainMapManager.MyInstance.MapIndicatorControllers[interactable].gameObject.activeSelf == true) {
                    MainMapManager.MyInstance.MapIndicatorControllers[interactable].transform.localPosition = new Vector3((interactable.transform.position.x - LevelManager.MyInstance.SceneBounds.center.x) * levelScaleFactor, (interactable.transform.position.z - LevelManager.MyInstance.SceneBounds.center.z) * levelScaleFactor, 0);
                    MainMapManager.MyInstance.MapIndicatorControllers[interactable].transform.localScale = new Vector3(1f / mapGraphic.transform.localScale.x, 1f / mapGraphic.transform.localScale.y, 1f / mapGraphic.transform.localScale.z);
                    interactable.UpdateMainMapIndicator();
                }
            }
        }

        private void OnEnable() {
            //Debug.Log("MainMapController.OnEnable()");
            CreateActiveEventSubscriptions();
        }

        private void CreateActiveEventSubscriptions() {
            //Debug.Log("MainMapController.CreateEventSubscriptions()");
            if (activeEventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StartListening("AfterCameraUpdate", HandleAfterCameraUpdate);
            activeEventSubscriptionsInitialized = true;
        }

        private void CleanupActiveEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!activeEventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StopListening("AfterCameraUpdate", HandleAfterCameraUpdate);
            activeEventSubscriptionsInitialized = false;
        }

        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            CleanupActiveEventSubscriptions();
        }

        /*
        public void HandleLevelLoad(string eventName, EventParamProperties eventParamProperties) {
            InitializeMap();
        }
        */

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

            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

            // set the width and height of the image to a square the size of the smallest side
            graphicLayoutElement.preferredWidth = Mathf.Min(mainMapBackground.rect.width, mainMapBackground.rect.height);
            graphicLayoutElement.preferredHeight = graphicLayoutElement.preferredWidth;

            // the image will be scaled to the largest dimension
            levelScaleFactor = graphicLayoutElement.preferredWidth / (LevelManager.MyInstance.SceneBounds.size.x > LevelManager.MyInstance.SceneBounds.size.z ? LevelManager.MyInstance.SceneBounds.size.x : LevelManager.MyInstance.SceneBounds.size.z);
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

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("MainMapController.OnOpenWindow()");
            SetBackGroundColor(new Color32(0, 0, 0, (byte)(int)(PlayerPrefs.GetFloat("PopupWindowOpacity") * 255)));

            // take snapshot of map or load from file
            InitializeMap();

            // update indicators so their positions are correct before first update
            UpdateMainMap();
        }
    }

}