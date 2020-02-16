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

        protected bool eventSubscriptionsInitialized = false;

        public Color MyCircleColor { get => circleColor; set => circleColor = value; }

        void Start() {
            //Debug.Log("CastTargettingmanager.Start()");
            ConfigureDefaultMaterial();
            DisableProjector();
            CreateEventSubscriptions();
        }

        private void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.MyInstance.OnLevelUnload += HandleLevelUnload;
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("MiniMapIndicatorController.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.MyInstance.OnLevelUnload -= HandleLevelUnload;
            eventSubscriptionsInitialized = false;
        }

        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            CleanupEventSubscriptions();
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

        public void EnableProjector(BaseAbility baseAbility) {
            //Debug.Log("CastTargettingmanager.EnableProjector()");
            castTargettingController.gameObject.SetActive(true);
            castTargettingController.SetCircleColor(baseAbility.MyGroundTargetColor);
            castTargettingController.SetCircleRadius(baseAbility.MyGroundTargetRadius);
        }

        public bool ProjectorIsActive() {
            return castTargettingController.gameObject.activeSelf;
        }

    }

}