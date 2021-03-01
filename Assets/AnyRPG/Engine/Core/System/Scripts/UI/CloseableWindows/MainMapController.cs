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
        private RectTransform mainMapBackground = null;

        [SerializeField]
        private GameObject mapGraphic = null;

        [SerializeField]
        private GameObject mapIndicatorPrefab = null;

        private float cameraSize = 0f;

        // the number of pixels per meter of level based on the total map pixels
        private float levelScaleFactor = 1f;

        protected bool eventSubscriptionsInitialized = false;

        private Dictionary<Interactable, MiniMapIndicatorController> mapIndicatorControllers = new Dictionary<Interactable, MiniMapIndicatorController>();


        public override void Awake() {
            //Debug.Log(gameObject.name + ": MiniMapController.Awake()");
            base.Awake();
            //instantiate singleton
            MainMapController tempcontroller = MyInstance;
            CameraManager.MyInstance.MainMapCamera.enabled = false;

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

        public GameObject AddIndicator(Interactable interactable) {
            //Debug.Log("MinimapController.AddIndicator(" + interactable.gameObject.name + ")");
            if (mapIndicatorControllers.ContainsKey(interactable) == false) {
                GameObject miniMapIndicator = ObjectPooler.MyInstance.GetPooledObject(mapIndicatorPrefab, mapGraphic.transform);
                MiniMapIndicatorController mapIndicatorController = miniMapIndicator.GetComponent<MiniMapIndicatorController>();
                mapIndicatorControllers.Add(interactable, mapIndicatorController);
                mapIndicatorController.SetInteractable(interactable);
                /*
                if (miniMapEnabled == false) {
                    miniMapIndicatorController.gameObject.SetActive(false);
                }
                */
            }

            return mapIndicatorControllers[interactable].gameObject;
        }

        public void RemoveIndicator(Interactable interactable) {
            if (mapIndicatorControllers.ContainsKey(interactable)) {
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
            // scale factor gives the number of pixels per meter for the image
            UpdateCameraSize();
            UpdateCameraPosition();
            CameraManager.MyInstance.MainMapCamera.Render();
            //levelScaleFactor = mapGraphicRawImage.texture.width / LevelManager.MyInstance.SceneBounds.size.x;
            //levelScaleFactor = graphicLayoutElement.preferredWidth / LevelManager.MyInstance.SceneBounds.size.x;

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
            // TESTING DID PUTTING THE EVENT CLEANUP IN ONDESTROY FIX THIS SO THE NEXT LINE ISN'T NEEDED?
            // re-adding this back here.  Not sure why, but possible the level objects aren't rendered by the time this gets called in onlevelload.  Trying on every open window
            InitializeMap();
            //CameraManager.MyInstance.MyMainMapCamera.enabled = true;

            //panelLayoutElement.preferredWidth = Screen.width - 50;
            //panelLayoutElement.preferredHeight = Screen.height - 50;

            //Debug.Log("MainMapController.OnOpenWindow(); panelLayoutElement.preferredWidth: " + panelLayoutElement.preferredWidth);
            //Debug.Log("MainMapController.OnOpenWindow(); panelLayoutElement.preferredHeight: " + panelLayoutElement.preferredHeight);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            float graphicScale = Mathf.Min(mainMapBackground.rect.width, mainMapBackground.rect.height);
            //Debug.Log("MainMapController.OnOpenWindow(); graphicScale: " + graphicScale);
            float extentsRatio = LevelManager.MyInstance.SceneBounds.extents.x / LevelManager.MyInstance.SceneBounds.extents.z;
            //Debug.Log("MainMapController.OnOpenWindow(); sceneBounds.extents.x: " + sceneBounds.extents.x + "; extentsRatio: " + extentsRatio + ";sceneBounds.extents.z: " + sceneBounds.extents.z);
            //Debug.Log("MainMapController.OnOpenWindow(); mainMapBackground.rect.width: " + mainMapBackground.rect.width);
            //Debug.Log("MainMapController.OnOpenWindow(); mainMapBackground.rect.height: " + mainMapBackground.rect.height);

            graphicLayoutElement.preferredWidth = (graphicScale / (LevelManager.MyInstance.SceneBounds.extents.x * 2)) * (LevelManager.MyInstance.SceneBounds.extents.x * 2) * extentsRatio;
            graphicLayoutElement.preferredHeight = (graphicScale / (LevelManager.MyInstance.SceneBounds.extents.z * 2)) * (LevelManager.MyInstance.SceneBounds.extents.z * 2);
            //Debug.Log("MainMapController.OnOpenWindow(); graphicLayoutElement.preferredWidth: " + graphicLayoutElement.preferredWidth);
            //Debug.Log("MainMapController.OnOpenWindow(); graphicLayoutElement.preferredHeight: " + graphicLayoutElement.preferredHeight);
            levelScaleFactor = graphicLayoutElement.preferredWidth / LevelManager.MyInstance.SceneBounds.size.x;
        }
    }

}