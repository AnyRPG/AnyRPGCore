using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [RequireComponent(typeof(SphereCollider))]
    public class InteractableRangeOld : ConfiguredMonoBehaviour {

        // THIS SHOULD ONLY BE ON THE PLAYER OR ANYTHING LEAVING ANYTHINGS RANGE WILL REMOVE THEMSELVES FROM THE PLAYERS RANGE TABLE!!!

        private SphereCollider rangeCollider;

        //private CharacterUnit playerUnit;

        // game manager references
        protected PlayerManager playerManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            rangeCollider = GetComponent<SphereCollider>();
            if (playerManager.UnitController == null) {
                // player unit not spawned yet, so this can't be the player.  Disable collider
                DisableCollider();
                return;
            }
            Interactable _interactable = GetComponentInParent<Interactable>();
            //if (_interactable.gameObject != playerManager.ActiveUnitController.gameObject) {
            // player unit is spawned, but this is not the player unit.  Disable collider
            DisableCollider();
            //}

        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
        }

        public void EnableCollider() {
            rangeCollider.enabled = true;
        }

        public void DisableCollider() {
            rangeCollider.enabled = false;
        }

        private void OnTriggerEnter(Collider collider) {
            //Debug.Log("InteractableRange.OnTriggerEnter()");
            /*
            if (playerUnit == null) {
                return;
            }
            */
            bool isAlreadyInRangeTable = false;

            Interactable _interactable = collider.GetComponent<Interactable>();
            if (_interactable != null) {
                if (playerManager.MyCharacter == null) {
                    //Debug.Log("playerManager.MyCharacter == null: true");
                    return;
                }
                if (playerManager.PlayerController.Interactables.Count != 0) {
                    // loop through the table and see if the target is already in it.
                    foreach (Interactable interactable in playerManager.PlayerController.Interactables) {
                        if (_interactable == interactable) {
                            isAlreadyInRangeTable = true;
                            //Debug.Log(gameObject.name + " adding " + aggroAmount.ToString() + " aggro to entry: " + target.name + "; total: " + aggroNode.aggroValue.ToString());
                        }
                    }
                }

                if (!isAlreadyInRangeTable) {
                    //Debug.Log(gameObject.name + " adding new entry " + target.name + " to aggro table");
                    playerManager.PlayerController.Interactables.Add(_interactable);
                }
                //Debug.Log("OnTriggerEnter(): Rangetable size: " + playerManager.MyCharacter.MyCharacterController.MyInteractables.Count);
            }
        }

        private void OnTriggerExit(Collider collider) {
            //Debug.Log(gameObject.name + ".InteractableRange.OnTriggerExit()");
            /*
            if (playerUnit == null) {
                return;
            }
            */
            Interactable _interactable = collider.GetComponent<Interactable>();
            //Debug.Log(gameObject.name + " at " + transform.position + ".InteractableRange.OnTriggerExit(): " + collider.gameObject.name + " at " + collider.gameObject.transform.position);
            if (_interactable != null) {
                for (int i = 0; i < playerManager.PlayerController.Interactables.Count; i++) {
                    if (playerManager.PlayerController.Interactables[i] == _interactable) {
                        if (_interactable.IsInteracting == true) {
                            _interactable.StopInteract();
                        }
                        playerManager.PlayerController.Interactables.Remove(_interactable);
                        return;
                    }
                }
            }
            //Debug.Log("OnTriggerExit(): Rangetable size: " + playerManager.MyCharacter.MyCharacterController.MyInteractables.Count);
        }



    }

}