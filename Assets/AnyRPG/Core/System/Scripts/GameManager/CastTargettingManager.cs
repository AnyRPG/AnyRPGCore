using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class CastTargettingManager : ConfiguredMonoBehaviour {

        [SerializeField]
        private CastTargetController castTargetController = null;

        [SerializeField]
        private Vector3 offset = Vector3.zero;

        private Color circleColor;

        protected bool eventSubscriptionsInitialized = false;

        // game manager references
        private PlayerManager playerManager = null;

        public Color CircleColor { get => circleColor; set => circleColor = value; }
        public CastTargetController CastTargetController { get => castTargetController; }

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log("CastTargettingmanager.Configure()");
            base.Configure(systemGameManager);

            castTargetController.Configure(systemGameManager);
            ConfigureDefaultMaterial();
            DisableProjector();
            CreateEventSubscriptions();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
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
            if (castTargetController != null) {
                castTargetController.SetupController();
            }
        }

        public void ProcessLevelUnload() {
            DisableProjector();
        }

        public void DisableProjector() {
            //Debug.Log("CastTargettingmanager.DisableProjector()");
            castTargetController.gameObject.SetActive(false);
        }

        public void EnableProjector(BaseAbilityProperties baseAbility) {
            //Debug.Log("CastTargettingmanager.EnableProjector()");
            castTargetController.gameObject.SetActive(true);
            castTargetController.SetCircleColor((baseAbility.GetTargetOptions(playerManager.MyCharacter) as AbilityTargetProps).GroundTargetColor);
            castTargetController.SetCircleRadius((baseAbility.GetTargetOptions(playerManager.MyCharacter) as AbilityTargetProps).GroundTargetRadius);
        }

        public bool ProjectorIsActive() {
            return castTargetController.gameObject.activeSelf;
        }

    }

}