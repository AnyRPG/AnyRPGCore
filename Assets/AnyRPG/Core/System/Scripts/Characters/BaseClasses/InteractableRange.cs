using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class InteractableRange : ConfiguredMonoBehaviour {

        [SerializeField]
        protected Collider rangeCollider = null;

        /*
        [SerializeField]
        protected bool autoSetRadius = true;
        */

        protected Interactable interactable = null;
        protected bool colliderWasActive = false;

        // game manager references
        protected PlayerManagerServer playerManagerServer = null;
        protected NetworkManagerServer networkManagerServer = null;
        protected LevelManagerClient levelManagerClient = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            //Debug.Log($"{gameObject.transform.parent.parent.name}.InteractableRange.SetGameManagerReferences()");

            base.SetGameManagerReferences();
            playerManagerServer = systemGameManager.PlayerManagerServer;
            networkManagerServer = systemGameManager.NetworkManagerServer;
            levelManagerClient = systemGameManager.LevelManagerClient;
        }

        public void SetInteractable(Interactable interactable) {
            //Debug.Log($"InteractableRange.SetInteractable({interactable.gameObject.name}) instanceId: {GetInstanceID()}");

            this.interactable = interactable;
            /*
            if (autoSetRadius == true) {
                //Debug.Log("setting bounds");
                Vector3 extents = rangeCollider.bounds.extents;
                rangeCollider.
                //extents.x = interactable.InteractionMaxRange;
                //extents.y = interactable.InteractionMaxRange;
                //extents.z = interactable.InteractionMaxRange;
                extents = new Vector3(interactable.InteractionMaxRange, interactable.InteractionMaxRange, interactable.InteractionMaxRange);
            }
            */
            //if (colliderWasActive && (networkManagerServer.ServerModeActive == true || systemGameManager.GameMode == GameMode.Local || levelManager.IsCutscene())) {
            if (colliderWasActive) {
                EnableCollider();
                AdjustCollider();
            }
        }

        public void AdjustCollider() {
            if (interactable.OverrideInteractionColliderSize) {
                AdjustCollider(new Vector3(interactable.InteractionMaxRange * 2f, interactable.InteractionMaxRange * 2f, interactable.InteractionMaxRange * 2f));
            }
        }

        public void AdjustCollider(Vector3 newSize) {
            //Debug.Log($"{gameObject.transform.parent.parent.name}.InteractableRange.AdjustCollider({newSize}) instanceId: {GetInstanceID()}");

            switch (rangeCollider) {
                case BoxCollider box:
                    // BoxCollider uses 'size' (full dimensions)
                    box.size = newSize;
                    break;

                case SphereCollider sphere:
                    // SphereCollider uses 'radius' (half of one dimension)
                    sphere.radius = newSize.x * 0.5f;
                    break;

                case CapsuleCollider capsule:
                    // CapsuleCollider uses 'height' and 'radius'
                    capsule.height = newSize.y;
                    capsule.radius = newSize.x * 0.5f;
                    break;

                default:
                    break;
            }
        }


        public void EnableCollider() {
            rangeCollider.enabled = true;
        }

        public void DisableCollider() {
            //Debug.Log($"{gameObject.transform.parent.parent.name}.InteractableRange.DisableCollider() instanceId: {GetInstanceID()}");

            rangeCollider.enabled = false;
        }

        private void OnTriggerEnter(Collider collider) {
            //Debug.Log($"{interactable.gameObject.name}.InteractableRange.OnTriggerEnter({collider.gameObject.name}) count : {inRangeGameObjects.Count}");

            interactable.InteractableTriggerEnter(collider);
        }


        private void OnTriggerExit(Collider collider) {
            //Debug.Log($"{interactable.gameObject.name}.InteractableRange.OnTriggerExit({collider.gameObject.name})");

            interactable.InteractableTriggerExit(collider);

        }

        private void Awake() {
            //Debug.Log($"{gameObject.transform.parent.parent.name}.InteractableRange.Awake() instanceId: {GetInstanceID()}");

            if (rangeCollider != null && rangeCollider.enabled == true) {
                colliderWasActive = true;
                DisableCollider();
            }
        }

        public void ResetSettings() {
            interactable = null;
            DisableCollider();
        }
    }

}