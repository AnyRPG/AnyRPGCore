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
        protected LayoutElement graphicLayoutElement = null;

        [SerializeField]
        protected RawImage mapRawImage = null;

        [SerializeField]
        protected RectTransform mainMapBackground = null;

        [SerializeField]
        protected GameObject mapGraphic = null;

        protected string loadedMapName = string.Empty;

        // the number of pixels per meter of level based on the total map pixels
        protected float levelScaleFactor = 1f;

        //private bool activeEventSubscriptionsInitialized = false;

        // system component references
        protected CameraManager cameraManager = null;
        protected PlayerManager playerManager = null;
        protected MapManager mapManager = null;
        protected MainMapManager mainMapManager = null;
        protected LevelManager levelManager = null;
        protected UIManager uIManager = null;
        protected ObjectPooler objectPooler = null;

        protected Dictionary<Interactable, MainMapIndicatorController> mapIndicatorControllers = new Dictionary<Interactable, MainMapIndicatorController>();

        public Dictionary<Interactable, MainMapIndicatorController> MapIndicatorControllers { get => mapIndicatorControllers; }

        public GameObject MapGraphic { get => mapGraphic; }

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log("MainMapController.Init()");
            base.Configure(systemGameManager);

            //cameraManager.MainMapCamera.enabled = false;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            cameraManager = systemGameManager.CameraManager;
            playerManager = systemGameManager.PlayerManager;
            uIManager = systemGameManager.UIManager;
            mapManager = uIManager.MapManager;
            mainMapManager = uIManager.MainMapManager;
            levelManager = systemGameManager.LevelManager;
            objectPooler = systemGameManager.ObjectPooler;
        }

        public void HandleInteractableStatusUpdate(Interactable interactable, InteractableOptionComponent interactableOptionComponent) {
            if (mapIndicatorControllers.ContainsKey(interactable)) {
                mapIndicatorControllers[interactable].HandleMainMapStatusUpdate(interactableOptionComponent);
            }
        }

        public void HandleAddIndicator(Interactable interactable) {
            //Debug.Log("MainMapController.AddIndicator(" + interactable.gameObject.name + ")");
            if (mapIndicatorControllers.ContainsKey(interactable) == false) {
                GameObject mainMapIndicator = objectPooler.GetPooledObject(mainMapManager.MapIndicatorPrefab, (mapGraphic.transform));
                if (mainMapIndicator != null) {
                    MainMapIndicatorController mapIndicatorController = mainMapIndicator.GetComponent<MainMapIndicatorController>();
                    if (mapIndicatorController != null) {
                        mapIndicatorControllers.Add(interactable, mapIndicatorController);
                        mapIndicatorController.Configure(systemGameManager);
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
                objectPooler.ReturnObjectToPool(mapIndicatorControllers[interactable].gameObject);
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
            if (uIManager.mainMapWindow.IsOpen == false) {
                return;
            }

            UpdateIndicatorPositions();
        }

        public void LateUpdate() {
            if (uIManager.mainMapWindow.IsOpen == false) {
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

        protected override void ProcessCreateEventSubscriptions() {
            //Debug.Log("MainMapController.CreateEventSubscriptions()");
            base.ProcessCreateEventSubscriptions();
            mainMapManager.OnAddIndicator += HandleAddIndicator;
            mainMapManager.OnRemoveIndicator += HandleRemoveIndicator;
            mainMapManager.OnUpdateIndicatorRotation += HandleIndicatorRotation;
            mainMapManager.OnInteractableStatusUpdate += HandleInteractableStatusUpdate;
            SystemEventManager.StartListening("AfterCameraUpdate", HandleAfterCameraUpdate);
        }

        protected override void ProcessCleanupEventSubscriptions() {
            //Debug.Log("MainMapController.CleanupEventSubscriptions()");
            base.ProcessCleanupEventSubscriptions();
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
            if (mapManager.SceneTextureFound == true) {
                mapRawImage.texture = mapManager.MapTexture;
            } else {
                // if a map image could not be found, take a picture
                
            }
            loadedMapName = SceneManager.GetActiveScene().name;

            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

            // set the width and height of the image to a square the size of the smallest side
            graphicLayoutElement.preferredWidth = Mathf.Min(mainMapBackground.rect.width, mainMapBackground.rect.height);
            graphicLayoutElement.preferredHeight = graphicLayoutElement.preferredWidth;

            // the image will be scaled to the largest dimension
            levelScaleFactor = graphicLayoutElement.preferredWidth / (levelManager.SceneBounds.size.x > levelManager.SceneBounds.size.z ? levelManager.SceneBounds.size.x : levelManager.SceneBounds.size.z);
        }
       

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("MainMapController.OnOpenWindow()");
            base.ReceiveOpenWindowNotification();
            SetBackGroundColor(new Color32(0, 0, 0, (byte)(int)(PlayerPrefs.GetFloat("PopupWindowOpacity") * 255)));

            // take snapshot of map or load from file
            InitializeMap();

            // update indicators so their positions are correct before first update
            UpdateMainMap();
        }
    }

}