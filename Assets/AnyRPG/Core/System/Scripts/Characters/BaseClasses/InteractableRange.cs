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

        protected List<GameObject> inRangeColliders = new List<GameObject>();

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
            //Debug.Log(interactable.gameObject.name + ".InteractableRange.OnTriggerEnter(" + collider.gameObject.name + ") count : " + inRangeColliders.Count);

            if (collider.gameObject == playerManager.ActiveUnitController.gameObject && inRangeColliders.Contains(collider.gameObject) == false) {
                inRangeColliders.Add(collider.gameObject);

                if (interactable.PrerequisitesMet == false || interactable.GetCurrentInteractables().Count == 0) {
                    return;
                }

                playerManager.PlayerController.AddInteractable(interactable);
            }
        }

        private void OnTriggerExit(Collider collider) {
            //Debug.Log(interactable.gameObject.name + ".InteractableRange.OnTriggerExit(" + collider.gameObject.name + ") count: " + inRangeColliders.Count);

            if (collider.gameObject == playerManager.ActiveUnitController.gameObject) {
                playerManager.PlayerController.RemoveInteractable(interactable);
                RemoveInRangeCollider(collider.gameObject);
            }
        }

        private void RemoveInRangeCollider(GameObject go) {
            //Debug.Log("InteractableRange.RemoveInRangeCollider(" + go.name + ") count: " + inRangeColliders.Count);
            if (inRangeColliders.Contains(go)) {
                inRangeColliders.Remove(go);
            }
        }

        public void UpdateStatus() {
            //Debug.Log("InteractableRange.UpdateStatus()");

            foreach (GameObject go in inRangeColliders) {
                if (interactable.PrerequisitesMet == false || interactable.GetCurrentInteractables().Count == 0) {
                    playerManager.PlayerController.RemoveInteractable(interactable);
                } else {
                    playerManager.PlayerController.AddInteractable(interactable);
                }

            }
        }

        public void RegisterDespawn(GameObject go) {
            RemoveInRangeCollider(go);
        }

        public void OnSendObjectToPool() {
            foreach (GameObject go in inRangeColliders) {
                if (go == playerManager.ActiveUnitController.gameObject) {
                    playerManager.PlayerController.RemoveInteractable(interactable);
                }
            }
            inRangeColliders.Clear();
        }



    }

}