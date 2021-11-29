using AnyRPG;
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

        // game manager references
        protected PlayerManager playerManager = null;

        protected List<Collider> inRangeColliders = new List<Collider>();

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
        }

        public void SetInteractable(Interactable interactable) {
            //Debug.Log("InteractableRange.SetInteractable(" + interactable.gameObject.name + ")");
            this.interactable = interactable;
            /*
            if (autoSetRadius == true) {
                Debug.Log("setting bounds");
                Vector3 extents = rangeCollider.bounds.extents;
                rangeCollider.
                //extents.x = interactable.InteractionMaxRange;
                //extents.y = interactable.InteractionMaxRange;
                //extents.z = interactable.InteractionMaxRange;
                extents = new Vector3(interactable.InteractionMaxRange, interactable.InteractionMaxRange, interactable.InteractionMaxRange);
            }
            */
        }

        public void EnableCollider() {
            rangeCollider.enabled = true;
        }

        public void DisableCollider() {
            rangeCollider.enabled = false;
        }

        private void OnTriggerEnter(Collider collider) {
            //Debug.Log(interactable.gameObject.name + ".InteractableRange.OnTriggerEnter(" + collider.gameObject.name + ")");

            if (collider.gameObject == playerManager.ActiveUnitController.gameObject) {
                inRangeColliders.Add(collider);

                if (interactable.PrerequisitesMet == false || interactable.GetCurrentInteractables().Count == 0) {
                    return;
                }

                playerManager.PlayerController.AddInteractable(interactable);
            }
        }

        private void OnTriggerExit(Collider collider) {
            //Debug.Log(interactable.gameObject.name + ".InteractableRange.OnTriggerExit(" + collider.gameObject.name + ")");

            if (collider.gameObject == playerManager.ActiveUnitController.gameObject) {
                playerManager.PlayerController.RemoveInteractable(interactable);
                inRangeColliders.Remove(collider);
            }
        }

        public void UpdateStatus() {
            //Debug.Log("InteractableRange.UpdateStatus()");

            foreach (Collider collider in inRangeColliders) {
                if (interactable.PrerequisitesMet == false || interactable.GetCurrentInteractables().Count == 0) {
                    playerManager.PlayerController.RemoveInteractable(interactable);
                } else {
                    playerManager.PlayerController.AddInteractable(interactable);
                }

            }
        }

        public void OnSendObjectToPool() {
            foreach (Collider collider in inRangeColliders) {
                if (collider.gameObject == playerManager.ActiveUnitController.gameObject) {
                    playerManager.PlayerController.RemoveInteractable(interactable);
                }
            }
            inRangeColliders.Clear();
        }



    }

}