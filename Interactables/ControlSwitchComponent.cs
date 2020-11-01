using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ControlSwitchComponent : InteractableOptionComponent {

        public override event System.Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        protected ControlSwitchProps interactableOptionProps = null;

        // keep track of the number of times this switch has been activated
        protected int activationCount = 0;

        // can be on or off
        protected bool onState = false;

        public bool MyOnState { get => onState; set => onState = value; }

        public ControlSwitchComponent(Interactable interactable, ControlSwitchProps interactableOptionProps) : base(interactable) {
            this.interactableOptionProps = interactableOptionProps;
            interactionPanelTitle = "Interactable";
        }

        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".AnimatedObject.Interact(" + (source == null ? "null" : source.name) +")");
            PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
            if (interactableOptionProps.ActivationLimit > 0 && activationCount >= interactableOptionProps.ActivationLimit) {
                // this has already been activated the number of allowed times
                return false;
            }
            activationCount++;
            if (interactableOptionProps.SwitchGroup != null && interactableOptionProps.SwitchGroup.Count > 0) {
                int activeSwitches = 0;
                foreach (ControlSwitchComponent controlSwitch in interactableOptionProps.SwitchGroup) {
                    if (controlSwitch.MyOnState) {
                        activeSwitches++;
                    }
                }
                if (onState == false && activeSwitches < interactableOptionProps.SwitchGroup.Count) {
                    return false;
                } else if (onState == true && activeSwitches >= interactableOptionProps.SwitchGroup.Count) {
                    return false;
                }

            }
            onState = !onState;
            base.Interact(source);

            if (interactableOptionProps.ControlObjects != null) {
                //Debug.Log(gameObject.name + ".AnimatedObject.Interact(): coroutine is not null, exiting");
                foreach (InteractableOptionComponent interactableOption in interactableOptionProps.ControlObjects) {
                    interactableOption.Interact(source);
                }
            }
            

            return false;
        }

        public override void HandlePrerequisiteUpdates() {
            //Debug.Log(gameObject.name + ".AnimatedObject.HandldePrerequisiteUpdates()");
            base.HandlePrerequisiteUpdates();
            MiniMapStatusUpdateHandler(this);
        }

        public override void HandlePlayerUnitSpawn() {
            base.HandlePlayerUnitSpawn();
            MiniMapStatusUpdateHandler(this);
        }

    }

}