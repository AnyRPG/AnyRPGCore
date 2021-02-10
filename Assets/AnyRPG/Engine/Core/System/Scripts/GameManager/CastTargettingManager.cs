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
        private CastTargetController castTargetController = null;

        [SerializeField]
        private Vector3 offset = Vector3.zero;

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
            SystemEventManager.StartListening("OnLevelUnload", HandleLevelUnload);
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("MiniMapIndicatorController.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StopListening("OnLevelUnload", HandleLevelUnload);
            eventSubscriptionsInitialized = false;
        }

        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            CleanupEventSubscriptions();
        }

        public void HandleLevelUnload(string eventName, EventParamProperties eventParamProperties) {
            ProcessLevelUnload();
        }


        public void ConfigureDefaultMaterial() {
            if (SystemConfigurationManager.MyInstance != null) {
                if (castTargetController != null ) {
                    castTargetController.SetupController();
                }
            }
        }

        public void ProcessLevelUnload() {
            DisableProjector();
        }

        public void DisableProjector() {
            //Debug.Log("CastTargettingmanager.DisableProjector()");
            castTargetController.gameObject.SetActive(false);
        }

        public void EnableProjector(BaseAbility baseAbility) {
            //Debug.Log("CastTargettingmanager.EnableProjector()");
            castTargetController.gameObject.SetActive(true);
            castTargetController.SetCircleColor((baseAbility.GetTargetOptions(PlayerManager.MyInstance.MyCharacter) as AbilityTargetProps).GroundTargetColor);
            castTargetController.SetCircleRadius((baseAbility.GetTargetOptions(PlayerManager.MyInstance.MyCharacter) as AbilityTargetProps).GroundTargetRadius);
        }

        public bool ProjectorIsActive() {
            return castTargetController.gameObject.activeSelf;
        }

    }

}