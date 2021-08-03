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

        //private bool activeEventSubscriptionsInitialized = false;

        // system component references
        private SystemConfigurationManager systemConfigurationManager = null;
        private CameraManager cameraManager = null;
        private PlayerManager playerManager = null;
        private MainMapManager mainMapManager = null;
        private LevelManager levelManager = null;
        private PopupWindowManager popupWindowManager = null;

        private Dictionary<Interactable, MainMapIndicatorController> mapIndicatorControllers = new Dictionary<Interactable, MainMapIndicatorController>();

        public Dictionary<Interactable, MainMapIndicatorController> MapIndicatorControllers { get => mapIndicatorControllers; }

        public GameObject MapGraphic { get => mapGraphic; }

        public override void Init(SystemGameManager systemGameManager) {
            //Debug.Log("MainMapController.Init()");
            systemConfigurationManager = systemGameManager.SystemConfigurationManager;
            cameraManager = systemGameManager.CameraManager;
            playerManager = systemGameManager.PlayerManager;
            mainMapManager = systemGameManager.UIManager.MainMapManager;
            levelManager = systemGameManager.LevelManager;
            popupWindowManager = systemGameManager.UIManager.PopupWindowManager;

            cameraManager.MainMapCamera.enabled = false;

            mainmapTextureFolder = mainmapTextureFolderBase + systemConfigurationManager.GameName.Replace(" ", "") + "/Images/MiniMap/";

            // calling base.Init() last because it will trigger event subscriptions, which need the above references initialized
            base.Init(systemGameManager);
        }

        public void HandleInteractableStatusUpdate(Interactable interactable, InteractableOptionComponent interactableOptionComponent) {
            if (mapIndicatorControllers.ContainsKey(interactable)) {
                mapIndicatorControllers[interactable].HandleMainMapStatusUpdate(interactableOptionComponent);
            }
        }

        public void HandleAddIndicator(Interactable interactable) {
            //Debug.Log("MainMapController.AddIndicator(" + interactable.gameObject.name + ")");
            if (mapIndicatorControllers.ContainsKey(interactable) == false) {
                GameObject mainMapIndicator = ObjectPooler.Instance.GetPooledObject(mainMapManager.MapIndicatorPrefab, (mapGraphic.transform));
                if (mainMapIndicator != null) {
                    MainMapIndicatorController mapIndicatorController = mainMapIndicator.GetComponent<MainMapIndicatorController>();
                    if (mapIndicatorController != null) {
                        mapIndicatorControllers.Add(interactable, mapIndicatorController);
                        mapIndicatorController.SetInteractable(interactable);
                        /*
                        if (miniMapEnabled == false) {
                            mapIndicatorController.gameObject.SetActive(false);
                        }
                        */
                    }
                }
            }

            //return mapIndicatorControllers[interactable];
        }

        public void HandleRemoveIndicator(Interactable interactable) {
            if (mapIndicatorControllers.ContainsKey(interactable)) {
                mapIndicatorControllers[interactable].ResetSettings();
                ObjectPooler.Instance.ReturnObjectToPool(mapIndicatorControllers[interactable].gameObject);
                mapIndicatorControllers.Remove(interactable);
            }
        }

        public void HandleIndicatorRotation(Interactable interactable) {
            mapIndicatorControllers[interactable].transform.rotation = Quaternion.Euler(0, 0, (interactable.transform.eulerAngles.y - systemConfigurationManager.PlayerMiniMapIconRotation) * -1f);
        }

        public void HandleAfterCameraUpdate(string eventName, EventParamProperties eventParamProperties) {
            UpdateMainMap();
        }

        void UpdateMainMap() {
            if (popupWindowManager.mainMapWindow.IsOpen == false) {
                return;
            }

            UpdateIndicatorPositions();
        }

        public void LateUpdate() {
            if (popupWindowManager.mainMapWindow.IsOpen == false) {
                return;
            }
            if (systemConfigurationManager.UseThirdPartyCameraControl == true
                && cameraManager.ThirdPartyCamera.activeInHierarchy == true
                && playerManager.PlayerUnitSpawned == true) {
                UpdateMainMap();
            }
        }

        private void UpdateIndicatorPositions() {
            foreach (Interactable interactable in mapIndicatorControllers.Keys) {
                if (mapIndicatorControllers[interactable].gameObject.activeSelf == true) {
                    mapIndicatorControllers[interactable].transform.localPosition = new Vector3((interactable.transform.position.x - levelManager.SceneBounds.center.x) * levelScaleFactor, (interactable.transform.position.z - levelManager.SceneBounds.center.z) * levelScaleFactor, 0);
                    mapIndicatorControllers[interactable].transform.localScale = new Vector3(1f / mapGraphic.transform.localScale.x, 1f / mapGraphic.transform.localScale.y, 1f / mapGraphic.transform.localScale.z);
                    interactable.UpdateMainMapIndicator();
                }
            }
        }

        /*
        private void OnEnable() {
            Debug.Log("MainMapController.OnEnable()");
            //CreateActiveEventSubscriptions();
        }
        */

        protected override void CreateEventSubscriptions() {
            //Debug.Log("MainMapController.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            base.CreateEventSubscriptions();
            mainMapManager.OnAddIndicator += HandleAddIndicator;
            mainMapManager.OnRemoveIndicator += HandleRemoveIndicator;
            mainMapManager.OnUpdateIndicatorRotation += HandleIndicatorRotation;
            mainMapManager.OnInteractableStatusUpdate += HandleInteractableStatusUpdate;
            SystemEventManager.StartListening("AfterCameraUpdate", HandleAfterCameraUpdate);
        }

        protected override void CleanupEventSubscriptions() {
            //Debug.Log("MainMapController.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            base.CleanupEventSubscriptions();
            mainMapManager.OnAddIndicator += HandleAddIndicator;
            mainMapManager.OnRemoveIndicator += HandleRemoveIndicator;
            mainMapManager.OnUpdateIndicatorRotation += HandleIndicatorRotation;
            mainMapManager.OnInteractableStatusUpdate += HandleInteractableStatusUpdate;
            SystemEventManager.StopListening("AfterCameraUpdate", HandleAfterCameraUpdate);
        }

        /*
        public void OnDisable() {
            //Debug.Log("MainMapController.OnDisable()");
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            //CleanupActiveEventSubscriptions();
        }
        */

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
            Texture2D mapTexture = new Texture2D((int)levelManager.SceneBounds.size.x, (int)levelManager.SceneBounds.size.z);
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
                cameraManager.MainMapCamera.Render();
            }
            loadedMapName = SceneManager.GetActiveScene().name;

            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

            // set the width and height of the image to a square the size of the smallest side
            graphicLayoutElement.preferredWidth = Mathf.Min(mainMapBackground.rect.width, mainMapBackground.rect.height);
            graphicLayoutElement.preferredHeight = graphicLayoutElement.preferredWidth;

            // the image will be scaled to the largest dimension
            levelScaleFactor = graphicLayoutElement.preferredWidth / (levelManager.SceneBounds.size.x > levelManager.SceneBounds.size.z ? levelManager.SceneBounds.size.x : levelManager.SceneBounds.size.z);
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
            cameraSize = Mathf.Max(levelManager.SceneBounds.extents.x, levelManager.SceneBounds.extents.z);
            cameraManager.MainMapCamera.orthographicSize = cameraSize;
        }

        private void UpdateCameraPosition() {
            //Debug.Log("MainMapController.UpdateCameraPosition()");
            Vector3 wantedPosition = new Vector3(levelManager.SceneBounds.center.x, levelManager.SceneBounds.center.y + levelManager.SceneBounds.extents.y + 1f, levelManager.SceneBounds.center.z);
            //Debug.Log("MainMapController.UpdateCameraPosition() wantedposition: " + wantedPosition);
            Vector3 wantedLookPosition = new Vector3(levelManager.SceneBounds.center.x, levelManager.SceneBounds.center.y, levelManager.SceneBounds.center.z);
            //Debug.Log("MainMapController.UpdateCameraPosition() wantedLookPosition: " + wantedLookPosition);
            cameraManager.MainMapCamera.transform.position = wantedPosition;
            cameraManager.MainMapCamera.transform.LookAt(wantedLookPosition);
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