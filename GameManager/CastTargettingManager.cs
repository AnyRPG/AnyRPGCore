using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class CastTargettingManager : MonoBehaviour {

        #region Singleton
        private static CastTargettingManager instance;

        public static CastTargettingManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<CastTargettingManager>();
                }
                return instance;
            }
        }

        #endregion

        [SerializeField]
        private CastTargettingController castTargettingController;

        [SerializeField]
        private Vector3 offset;

        private Color circleColor;

        protected bool startHasRun = false;
        protected bool eventReferencesInitialized = false;

        public Color MyCircleColor { get => circleColor; set => circleColor = value; }

        void Start() {
            //Debug.Log("CastTargettingmanager.Start()");
            ConfigureDefaultMaterial();
            DisableProjector();
            CreateEventReferences();
        }

        private void CreateEventReferences() {
            //Debug.Log("PlayerManager.CreateEventReferences()");
            if (eventReferencesInitialized || !startHasRun) {
                return;
            }
            SystemEventManager.MyInstance.OnLevelUnload += HandleLevelUnload;
            eventReferencesInitialized = true;
        }

        private void CleanupEventReferences() {
            //Debug.Log("MiniMapIndicatorController.CleanupEventReferences()");
            if (!eventReferencesInitialized) {
                return;
            }
            SystemEventManager.MyInstance.OnLevelUnload -= HandleLevelUnload;
            eventReferencesInitialized = false;
        }

        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            CleanupEventReferences();
        }

        public void ConfigureDefaultMaterial() {
            if (SystemConfigurationManager.MyInstance != null) {
                if (castTargettingController != null ) {
                    castTargettingController.SetupController();
                }
            }
        }

        public void HandleLevelUnload() {
            DisableProjector();
        }

        public void DisableProjector() {
            //Debug.Log("CastTargettingmanager.DisableProjector()");
            castTargettingController.gameObject.SetActive(false);
        }

        public void EnableProjector(Color groundTargetColor) {
            //Debug.Log("CastTargettingmanager.EnableProjector()");
            castTargettingController.gameObject.SetActive(true);
            castTargettingController.SetCircleColor(groundTargetColor);
        }

        /*
        void Update() {
            Debug.Log("CastTargettingmanager);
            icon.transform.position = Input.mousePosition+offset;

        }
        */
    }

}