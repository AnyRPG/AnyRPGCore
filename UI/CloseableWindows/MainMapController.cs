using AnyRPG;
using System;
using System.Collections;
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

        // objects in the player stats window

        /*
    [SerializeField]
    private Transform cameraTransform;
    */

        //[SerializeField]
        //private float cameraOffsetYDefault = 25f;

        //[SerializeField]
        //private float cameraSizeDefault = 500f;

        //[SerializeField]
        //private float minZoom = 5f;

        //[SerializeField]
        //private float maxZoom = 25f;

        //[SerializeField]
        //private float zoomSpeed = 4f;

        [SerializeField]
        private LayoutElement panelLayoutElement = null;

        [SerializeField]
        private LayoutElement graphicLayoutElement = null;

        [SerializeField]
        private RectTransform mainMapBackground = null;

        //private float cameraOffsetY = 0f;
        private float cameraSize = 0f;

        //private bool initialized = false;

        private Renderer[] renderers;
        private Bounds sceneBounds;
        private Vector3[] worldCorners = new Vector3[4];

        protected bool eventSubscriptionsInitialized = false;

        //public override event Action<ICloseableWindowContents> OnOpenWindow;

        public override void Awake() {
            //Debug.Log(gameObject.name + ": MiniMapController.Awake()");
            base.Awake();
            //instantiate singleton
            MainMapController tempcontroller = MyInstance;
            CameraManager.MyInstance.MyMainMapCamera.enabled = false;
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
            SetSceneBounds();
            UpdateCameraSize();
            UpdateCameraPosition();
            CameraManager.MyInstance.MyMainMapCamera.Render();
        }

        private void SetSceneBounds() {
            //Debug.Log("MainMapController.SetSceneBounds()");
            renderers = GameObject.FindObjectsOfType<Renderer>();
            if (renderers.Length == 0) {
                //Debug.Log("MainMapController.SetSceneBounds(). No Renderers Available!");
                return; // nothing to see here, go on
            }

            sceneBounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) {
                sceneBounds.Encapsulate(renderers[i].bounds);
                //Debug.Log("MainMapController.SetSceneBounds(). Encapsulating: " + renderers[i].bounds);
            }
            //Debug.Log("MainMapController.SetSceneBounds(). New Bounds: " + sceneBounds);
        }

        void OnDrawGizmosSelected() {
            // A sphere that fully encloses the bounding box.
            Vector3 center = sceneBounds.center;
            float radius = sceneBounds.extents.magnitude;

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(center, radius);
        }

        /*
        void Update() {
            if (!initialized) {
                //Debug.Log("MiniMapController.Update(): not initialized yet.  Exiting!");
                return;
            }
        }
        */

        private void UpdateCameraSize() {
            //Debug.Log("MainMapController.UpdateCameraSize()");
            //float newCameraSize = cameraSizeDefault;
            cameraSize = Mathf.Max(sceneBounds.extents.x, sceneBounds.extents.z);
            CameraManager.MyInstance.MyMainMapCamera.orthographicSize = cameraSize;
        }

        private void UpdateCameraPosition() {
            //Debug.Log("MainMapController.UpdateCameraPosition()");
            Vector3 wantedPosition = new Vector3(sceneBounds.center.x, sceneBounds.center.y + sceneBounds.extents.y, sceneBounds.center.z);
            //Debug.Log("MainMapController.UpdateCameraPosition() wantedposition: " + wantedPosition);
            Vector3 wantedLookPosition = new Vector3(sceneBounds.center.x, sceneBounds.center.y, sceneBounds.center.z);
            //Debug.Log("MainMapController.UpdateCameraPosition() wantedLookPosition: " + wantedLookPosition);
            CameraManager.MyInstance.MyMainMapCamera.transform.position = wantedPosition;
            CameraManager.MyInstance.MyMainMapCamera.transform.LookAt(wantedLookPosition);
        }

        private void CommonInitialization() {
            //Debug.Log("MainMapController.CommonInitialization()");
            //zoneNameText.text = SceneManager.GetActiveScene().name;
            this.gameObject.SetActive(true);
        }

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("MainMapController.OnCloseWindow()");
            base.RecieveClosedWindowNotification();
            CameraManager.MyInstance.MyMainMapCamera.enabled = false;
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("MainMapController.OnOpenWindow()");
            // TESTING DID PUTTING THE EVENT CLEANUP IN ONDESTROY FIX THIS SO THE NEXT LINE ISN'T NEEDED?
            // re-adding this back here.  Not sure why, but possible the level objects aren't rendered by the time this gets called in onlevelload.  Trying on every open window
            InitializeMap();
            //CameraManager.MyInstance.MyMainMapCamera.enabled = true;
            panelLayoutElement.preferredWidth = Screen.width - 50;
            panelLayoutElement.preferredHeight = Screen.height - 50;
            //Debug.Log("MainMapController.OnOpenWindow(); panelLayoutElement.preferredWidth: " + panelLayoutElement.preferredWidth);
            //Debug.Log("MainMapController.OnOpenWindow(); panelLayoutElement.preferredHeight: " + panelLayoutElement.preferredHeight);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            float graphicScale = Mathf.Min(mainMapBackground.rect.width, mainMapBackground.rect.height);
            //Debug.Log("MainMapController.OnOpenWindow(); graphicScale: " + graphicScale);
            float extentsRatio = sceneBounds.extents.x / sceneBounds.extents.z;
            //Debug.Log("MainMapController.OnOpenWindow(); sceneBounds.extents.x: " + sceneBounds.extents.x + "; extentsRatio: " + extentsRatio + ";sceneBounds.extents.z: " + sceneBounds.extents.z);
            //Debug.Log("MainMapController.OnOpenWindow(); mainMapBackground.rect.width: " + mainMapBackground.rect.width);
            //Debug.Log("MainMapController.OnOpenWindow(); mainMapBackground.rect.height: " + mainMapBackground.rect.height);

            graphicLayoutElement.preferredWidth = (graphicScale / (sceneBounds.extents.x * 2)) * (sceneBounds.extents.x * 2) * extentsRatio;
            graphicLayoutElement.preferredHeight = (graphicScale / (sceneBounds.extents.z * 2)) * (sceneBounds.extents.z * 2);
            //Debug.Log("MainMapController.OnOpenWindow(); graphicLayoutElement.preferredWidth: " + graphicLayoutElement.preferredWidth);
            //Debug.Log("MainMapController.OnOpenWindow(); graphicLayoutElement.preferredHeight: " + graphicLayoutElement.preferredHeight);
        }
    }

}