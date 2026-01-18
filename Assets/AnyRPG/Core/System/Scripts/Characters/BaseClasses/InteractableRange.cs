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
        protected bool colliderWasActive = false;

        // game manager references
        protected PlayerManagerServer playerManagerServer = null;
        protected NetworkManagerServer networkManagerServer = null;
        protected LevelManager levelManager = null;

        protected Dictionary<GameObject, UnitController> inRangeGameObjects = new Dictionary<GameObject, UnitController>();

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            //Debug.Log($"{gameObject.transform.parent.parent.name}.InteractableRange.SetGameManagerReferences()");

            base.SetGameManagerReferences();
            playerManagerServer = systemGameManager.PlayerManagerServer;
            networkManagerServer = systemGameManager.NetworkManagerServer;
            levelManager = systemGameManager.LevelManager;
        }

        public void SetInteractable(Interactable interactable) {
            //Debug.Log($"InteractableRange.SetInteractable({interactable.gameObject.name})");
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
            if (colliderWasActive && (networkManagerServer.ServerModeActive == true || systemGameManager.GameMode == GameMode.Local || levelManager.IsCutscene())) {
                EnableCollider();
            }
        }

        public void EnableCollider() {
            rangeCollider.enabled = true;
        }

        public void DisableCollider() {
            rangeCollider.enabled = false;
        }

        private void OnTriggerEnter(Collider collider) {
            //Debug.Log($"{gameObject.transform.parent.parent.name}.InteractableRange.OnTriggerEnter({collider.gameObject.name}) count : " + inRangeGameObjects.Count);

            if (systemGameManager.GameMode == GameMode.Network && networkManagerServer.ServerModeActive == false) {
                // triggers are server authoritative
                return;
            }

            if (playerManagerServer.ActivePlayerGameObjects.ContainsKey(collider.gameObject) == false) {
                return;
            }

            if (inRangeGameObjects.ContainsKey(collider.gameObject) == false) {
                UnitController unitController = collider.gameObject.GetComponent<UnitController>();
                if (unitController != null) {
                    return;
                }
                inRangeGameObjects.Add(collider.gameObject, unitController);
                if (interactable.GetCurrentInteractables(unitController).Count == 0) {
                    return;
                }
                unitController.UnitEventController.NotifyOnEnterInteractableRange(interactable);
            }
        }

        private void OnTriggerExit(Collider collider) {
            //Debug.Log(interactable.gameObject.name + ".InteractableRange.OnTriggerExit(" + collider.gameObject.name + ") count: " + inRangeColliders.Count);

            if (inRangeGameObjects.ContainsKey(collider.gameObject) == false) {
                return;
            }

            inRangeGameObjects[collider.gameObject].UnitEventController.NotifyOnExitInteractableRange(interactable);
            RemoveInRangeCollider(collider.gameObject);
        }

        private void RemoveInRangeCollider(GameObject go) {
            //Debug.Log("InteractableRange.RemoveInRangeCollider(" + go.name + ") count: " + inRangeColliders.Count);
            if (inRangeGameObjects.ContainsKey(go)) {
                inRangeGameObjects.Remove(go);
            }
        }

        public void UpdateStatus() {
            //Debug.Log("InteractableRange.UpdateStatus()");

            foreach (UnitController inRangeUnitController in inRangeGameObjects.Values) {
                if (interactable.GetCurrentInteractables(inRangeUnitController).Count == 0) {
                    inRangeUnitController.UnitEventController.NotifyOnExitInteractableRange(interactable);
                } else {
                    inRangeUnitController.UnitEventController.NotifyOnEnterInteractableRange(interactable);
                }

            }
        }

        public void RegisterDespawn(GameObject go) {
            RemoveInRangeCollider(go);
        }

        public void OnSendObjectToPool() {
            foreach (UnitController inRangeUnitController in inRangeGameObjects.Values) {
                inRangeUnitController.UnitEventController.NotifyOnExitInteractableRange(interactable);
            }
            inRangeGameObjects.Clear();
        }

        private void Awake() {
            if (rangeCollider != null && rangeCollider.enabled == true) {
                colliderWasActive = true;
                DisableCollider();
            }
        }


    }

}