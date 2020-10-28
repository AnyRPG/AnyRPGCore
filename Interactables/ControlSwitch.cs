using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ControlSwitch : InteractableOption {

        public override event System.Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        [SerializeField]
        private ControlSwitchProps interactableOptionProps = new ControlSwitchProps();

        [Header("Control Switch")]

        [Tooltip("When successfully activated, this switch will call Interact() on the following interactables")]
        [SerializeField]
        List<InteractableOption> controlObjects = new List<InteractableOption>();

        [Tooltip("all these switches must be in the onState for this switch to activate")]
        [SerializeField]
        private List<ControlSwitch> switchGroup = new List<ControlSwitch>();

        [Tooltip("The number of times this object can be activated.  0 is unlimited")]
        [SerializeField]
        private int activationLimit = 0;

        // keep track of the number of times this switch has been activated
        private int activationCount = 0;

        // can be on or off
        protected bool onState = false;

        public bool MyOnState { get => onState; set => onState = value; }

        public ControlSwitch(Interactable interactable, ControlSwitchProps interactableOptionProps) : base(interactable) {
            this.interactableOptionProps = interactableOptionProps;
            interactionPanelTitle = "Interactable";
        }

        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".AnimatedObject.Interact(" + (source == null ? "null" : source.name) +")");
            PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
            if (activationLimit > 0 && activationCount >= activationLimit) {
                // this has already been activated the number of allowed times
                return false;
            }
            activationCount++;
            if (switchGroup != null && switchGroup.Count > 0) {
                int activeSwitches = 0;
                foreach (ControlSwitch controlSwitch in switchGroup) {
                    if (controlSwitch.MyOnState) {
                        activeSwitches++;
                    }
                }
                if (onState == false && activeSwitches < switchGroup.Count) {
                    return false;
                } else if (onState == true && activeSwitches >= switchGroup.Count) {
                    return false;
                }

            }
            onState = !onState;
            base.Interact(source);

            if (controlObjects != null) {
                //Debug.Log(gameObject.name + ".AnimatedObject.Interact(): coroutine is not null, exiting");
                foreach (InteractableOption interactableOption in controlObjects) {
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


        public override void OnDisable() {
            base.OnDisable();
        }
    }

}