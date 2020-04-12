using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ControlSwitch : InteractableOption {

        public override event System.Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        [SerializeField]
        List<InteractableOption> controlObjects = new List<InteractableOption>();

        // all these switches must be in the onState for this switch to activate
        [SerializeField]
        private List<ControlSwitch> switchGroup = new List<ControlSwitch>();

        // can be on or off
        protected bool onState = false;

        public bool MyOnState { get => onState; set => onState = value; }

        /*
        public override Sprite MyIcon { get => (SystemConfigurationManager.MyInstance.MyAnimatedObjectInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyAnimatedObjectInteractionPanelImage : base.MyIcon); }
        public override Sprite MyNamePlateImage { get => (SystemConfigurationManager.MyInstance.MyAnimatedObjectNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyAnimatedObjectNamePlateImage : base.MyNamePlateImage); }
        */

        protected override void Start() {
            base.Start();
            interactionPanelTitle = "Interactable";

        }

        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".AnimatedObject.Interact(" + (source == null ? "null" : source.name) +")");
            PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
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