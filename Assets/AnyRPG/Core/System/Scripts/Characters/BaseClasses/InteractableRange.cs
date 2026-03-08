using AnyRPG;
using FishNet;
using FishNet.Managing.Timing;
using FishNet.Object;
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
            
            // colliders should be server side only (or cutscene) because with client side prediction,
            // reconciliation will cause the false exit and enters every frame
            if (systemGameManager.GameMode == GameMode.Network && networkManagerServer.ServerModeActive == false && levelManagerClient.IsCutscene() == false) {
                return;
            }
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
            //Debug.Log($"{gameObject.transform.parent.parent.name}.InteractableRange.EnableCollider() instanceId: {GetInstanceID()}");

            if (systemGameManager.GameMode == GameMode.Network && networkManagerServer.ServerModeActive == false && levelManagerClient.IsCutscene() == false) {
                return;
            }
            rangeCollider.enabled = true;
        }

        public void DisableCollider() {
            //Debug.Log($"{gameObject.transform.parent.parent.name}.InteractableRange.DisableCollider() instanceId: {GetInstanceID()}");

            rangeCollider.enabled = false;
        }

        private void OnTriggerEnter(Collider collider) {
            Debug.Log($"{interactable.gameObject.name}.InteractableRange.OnTriggerEnter({collider.gameObject.name})");
            if (IsReplaying(collider)) return;
            interactable.InteractableTriggerEnter(collider);
        }


        private void OnTriggerExit(Collider collider) {
            Debug.Log($"{interactable.gameObject.name}.InteractableRange.OnTriggerExit({collider.gameObject.name})");
            if (IsReplaying(collider)) return;
            interactable.InteractableTriggerExit(collider);

        }

        private bool IsReplaying(Collider other) {
            // 1. Get the NetworkObject from the thing that entered the trigger (the Player)
            NetworkObject no = other.GetComponentInParent<NetworkObject>();

            // 2. If it's a networked object, check if it's currently replaying a prediction
            if (no != null && InstanceFinder.PredictionManager.ServerReplayTick != TimeManager.UNSET_TICK) {
                Debug.Log($"{gameObject.transform.parent.parent.name}.InteractableRange.IsReplaying({other.gameObject.name}): ignoring trigger event because we are currently replaying a prediction");
                return true;
            }

            Debug.Log($"{gameObject.transform.parent.parent.name}.InteractableRange.IsReplaying({other.gameObject.name}): not ignoring trigger event");
            return false;
        }

        private void Awake() {
            //Debug.Log($"{gameObject.transform.parent.parent.name}.InteractableRange.Awake() instanceId: {GetInstanceID()}");

            if (rangeCollider != null && rangeCollider.enabled == true) {
                colliderWasActive = true;
                DisableCollider();
            }
        }

        public void ResetSettings() {
            Debug.Log($"{gameObject.transform.parent.parent.name}.InteractableRange.ResetSettings() instanceId: {GetInstanceID()}");
            interactable = null;
            DisableCollider();
        }
    }

}