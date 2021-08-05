using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PressureSwitchComponent : ControlSwitchComponent {

        public PressureSwitchProps PressureSwitchProps { get => interactableOptionProps as PressureSwitchProps; }

        public PressureSwitchComponent(Interactable interactable, PressureSwitchProps interactableOptionProps) : base(interactable, interactableOptionProps) {
        }

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            //Debug.Log(interactable.gameObject.name + ".PressureSwitch.Interact(" + (source == null ? "null" : source.DisplayName) +")");
            float totalWeight = 0f;
            if (interactable.Collider != null) {
                //Debug.Log(gameObject.name + ".PressureSwitch.Interact(" + (source == null ? "null" : source.name) + "): interactable center: " + interactable.transform.position);
                //Collider[] hitColliders = Physics.OverlapBox(interactable.transform.TransformPoint(interactable.Collider.bounds.center), interactable.Collider.bounds.extents, Quaternion.identity);
                Collider[] hitColliders = Physics.OverlapBox(interactable.Collider.bounds.center, interactable.Collider.bounds.extents, Quaternion.identity);
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
            if (totalWeight >= PressureSwitchProps.MinimumWeight && onState == false) {
                //Debug.Log(gameObject.name + "Weight: " + totalWeight);
                base.Interact(source, optionIndex);
            } else if (totalWeight < PressureSwitchProps.MinimumWeight && onState == true) {
                //Debug.Log(gameObject.name + "Weight: " + totalWeight);
                base.Interact(source, optionIndex);
            } else {
                SystemGameManager.Instance.UIManager.interactionWindow.CloseWindow();
            }

            return false;
        }

    }

}