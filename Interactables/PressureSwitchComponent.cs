using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PressureSwitchComponent : ControlSwitchComponent {

        public override event System.Action<InteractableOptionComponent> MiniMapStatusUpdateHandler = delegate { };

        public PressureSwitchComponent(Interactable interactable, PressureSwitchProps interactableOptionProps) : base(interactable, interactableOptionProps) {
            // done in base
            //this.interactableOptionProps = interactableOptionProps;
        }

        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".PressureSwitch.Interact(" + (source == null ? "null" : source.name) +")");
            float totalWeight = 0f;
            if (interactable.Collider != null) {
                //Debug.Log(gameObject.name + ".PressureSwitch.Interact(" + (source == null ? "null" : source.name) + "): interactable center: " + interactable.transform.position);
                Collider[] hitColliders = Physics.OverlapBox(interactable.transform.TransformPoint(interactable.Collider.bounds.center), interactable.Collider.bounds.extents, Quaternion.identity);
                int i = 0;
                //Check when there is a new collider coming into contact with the box
                //Debug.Log(gameObject.name + ".PressureSwitch.Interact(" + (source == null ? "null" : source.name) + "): hitcolliders: " + hitColliders.Length);
                while (i < hitColliders.Length) {
                    //Debug.Log(gameObject.name + ".Overlap Box Hit : " + hitColliders[i].gameObject.name + "[" + i + "]");
                    Rigidbody rigidbody = hitColliders[i].gameObject.GetComponent<Rigidbody>();
                    if (rigidbody != null) {
                        //Debug.Log(gameObject.name + ".Overlap Box Hit : " + hitColliders[i].gameObject.name + "[" + i + "] MATCH!!");
                        totalWeight += rigidbody.mass;
                    }
                    i++;
                }
            }
            //Debug.Log(gameObject.name + " totalWeight: " + totalWeight + "; minimumWeight: " + minimumWeight);
            if (totalWeight >= (interactableOptionProps as PressureSwitchProps).MinimumWeight && onState == false) {
                //Debug.Log(gameObject.name + "Weight: " + totalWeight);
                base.Interact(source);
            } else if (totalWeight < (interactableOptionProps as PressureSwitchProps).MinimumWeight && onState == true) {
                //Debug.Log(gameObject.name + "Weight: " + totalWeight);
                base.Interact(source);
            } else {
                PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
            }

            return false;
        }

    }

}