using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class CastTargettingManager : MonoBehaviour {

        [SerializeField]
        private CastTargetController castTargetController = null;

        [SerializeField]
        private Vector3 offset = Vector3.zero;

        private Color circleColor;

        protected bool eventSubscriptionsInitialized = false;

        public Color MyCircleColor { get => circleColor; set => circleColor = value; }

        public void Init() {
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
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            CleanupEventSubscriptions();
        }

        public void HandleLevelUnload(string eventName, EventParamProperties eventParamProperties) {
            ProcessLevelUnload();
        }


        public void ConfigureDefaultMaterial() {
            if (SystemGameManager.Instance.SystemConfigurationManager != null) {
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
            castTargetController.SetCircleColor((baseAbility.GetTargetOptions(SystemGameManager.Instance.PlayerManager.MyCharacter) as AbilityTargetProps).GroundTargetColor);
            castTargetController.SetCircleRadius((baseAbility.GetTargetOptions(SystemGameManager.Instance.PlayerManager.MyCharacter) as AbilityTargetProps).GroundTargetRadius);
        }

        public bool ProjectorIsActive() {
            return castTargetController.gameObject.activeSelf;
        }

    }

}