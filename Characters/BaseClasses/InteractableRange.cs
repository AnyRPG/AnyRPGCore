using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [RequireComponent(typeof(SphereCollider))]
    public class InteractableRange : MonoBehaviour {

        // THIS SHOULD ONLY BE ON THE PLAYER OR ANYTHING LEAVING ANYTHINGS RANGE WILL REMOVE THEMSELVES FROM THE PLAYERS RANGE TABLE!!!

        private SphereCollider rangeCollider;

        private PlayerUnit playerUnit;

        private void Awake() {
            rangeCollider = GetComponent<SphereCollider>();
            playerUnit = GetComponentInParent<PlayerUnit>();
            if (playerUnit == null) {
                Debug.Log("Found an interactable range on a non player unit!!! DESTROYING IT!!! " + transform.parent.gameObject.name);
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider collider) {
            //Debug.Log("InteractableRange.OnTriggerEnter()");
            if (playerUnit == null) {
                return;
            }
            bool isAlreadyInRangeTable = false;

            Interactable _interactable = collider.GetComponent<Interactable>();
            if (_interactable != null) {
                if (PlayerManager.MyInstance.MyCharacter == null) {
                    //Debug.Log("PlayerManager.MyInstance.MyCharacter == null: true");
                    return;
                }
                if ((PlayerManager.MyInstance.MyCharacter.MyCharacterController as PlayerController).MyInteractables.Count != 0) {
                    // loop through the table and see if the target is already in it.
                    foreach (Interactable interactable in (PlayerManager.MyInstance.MyCharacter.MyCharacterController as PlayerController).MyInteractables) {
                        if (_interactable == interactable) {
                            isAlreadyInRangeTable = true;
                            //Debug.Log(gameObject.name + " adding " + aggroAmount.ToString() + " aggro to entry: " + target.name + "; total: " + aggroNode.aggroValue.ToString());
                        }
                    }
                }

                if (!isAlreadyInRangeTable) {
                    //Debug.Log(gameObject.name + " adding new entry " + target.name + " to aggro table");
                    (PlayerManager.MyInstance.MyCharacter.MyCharacterController as PlayerController).MyInteractables.Add(_interactable);
                }
                //Debug.Log("OnTriggerEnter(): Rangetable size: " + PlayerManager.MyInstance.MyCharacter.MyCharacterController.MyInteractables.Count);
            }
        }

        private void OnTriggerExit(Collider collider) {
            //Debug.Log(gameObject.name + ".InteractableRange.OnTriggerExit()");
            if (playerUnit == null) {
                return;
            }
            Interactable _interactable = collider.GetComponent<Interactable>();
            //Debug.Log(gameObject.name + " at " + transform.position + ".InteractableRange.OnTriggerExit(): " + collider.gameObject.name + " at " + collider.gameObject.transform.position);
            if (_interactable != null) {
                for (int i = 0; i < (PlayerManager.MyInstance.MyCharacter.MyCharacterController as PlayerController).MyInteractables.Count; i++) {
                    if ((PlayerManager.MyInstance.MyCharacter.MyCharacterController as PlayerController).MyInteractables[i] == _interactable) {
                        if (_interactable.IsInteracting == true) {
                            _interactable.StopInteract();
                        }
                        (PlayerManager.MyInstance.MyCharacter.MyCharacterController as PlayerController).MyInteractables.Remove(_interactable);
                        return;
                    }
                }
            }
            //Debug.Log("OnTriggerExit(): Rangetable size: " + PlayerManager.MyInstance.MyCharacter.MyCharacterController.MyInteractables.Count);
        }



    }

}