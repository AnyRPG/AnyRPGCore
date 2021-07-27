using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [RequireComponent(typeof(SphereCollider))]
    public class InteractableRange : MonoBehaviour {

        // THIS SHOULD ONLY BE ON THE PLAYER OR ANYTHING LEAVING ANYTHINGS RANGE WILL REMOVE THEMSELVES FROM THE PLAYERS RANGE TABLE!!!

        private SphereCollider rangeCollider;

        //private CharacterUnit playerUnit;

        private void OnEnable() {
            rangeCollider = GetComponent<SphereCollider>();
            if (PlayerManager.Instance.UnitController == null) {
                // player unit not spawned yet, so this can't be the player.  Disable collider
                rangeCollider.enabled = false;
                return;
            }
            Interactable _interactable = GetComponentInParent<Interactable>();
            //if (_interactable.gameObject != PlayerManager.Instance.ActiveUnitController.gameObject) {
                // player unit is spawned, but this is not the player unit.  Disable collider
                rangeCollider.enabled = false;
            //}
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
                if (PlayerManager.Instance.MyCharacter == null) {
                    //Debug.Log("PlayerManager.Instance.MyCharacter == null: true");
                    return;
                }
                if (PlayerManager.Instance.PlayerController.MyInteractables.Count != 0) {
                    // loop through the table and see if the target is already in it.
                    foreach (Interactable interactable in PlayerManager.Instance.PlayerController.MyInteractables) {
                        if (_interactable == interactable) {
                            isAlreadyInRangeTable = true;
                            //Debug.Log(gameObject.name + " adding " + aggroAmount.ToString() + " aggro to entry: " + target.name + "; total: " + aggroNode.aggroValue.ToString());
                        }
                    }
                }

                if (!isAlreadyInRangeTable) {
                    //Debug.Log(gameObject.name + " adding new entry " + target.name + " to aggro table");
                    PlayerManager.Instance.PlayerController.MyInteractables.Add(_interactable);
                }
                //Debug.Log("OnTriggerEnter(): Rangetable size: " + PlayerManager.Instance.MyCharacter.MyCharacterController.MyInteractables.Count);
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
                for (int i = 0; i < PlayerManager.Instance.PlayerController.MyInteractables.Count; i++) {
                    if (PlayerManager.Instance.PlayerController.MyInteractables[i] == _interactable) {
                        if (_interactable.IsInteracting == true) {
                            _interactable.StopInteract();
                        }
                        PlayerManager.Instance.PlayerController.MyInteractables.Remove(_interactable);
                        return;
                    }
                }
            }
            //Debug.Log("OnTriggerExit(): Rangetable size: " + PlayerManager.Instance.MyCharacter.MyCharacterController.MyInteractables.Count);
        }



    }

}