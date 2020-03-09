using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ControlSwitch : InteractableOption {

        public override event System.Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        [SerializeField]
        InteractableOption controlObject = null;


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
            if (controlObject == null) {
                //Debug.Log(gameObject.name + ".AnimatedObject.Interact(): coroutine is not null, exiting");
                return false;
            }
            base.Interact(source);
            PopupWindowManager.MyInstance.interactionWindow.CloseWindow();

            controlObject.Interact(source);

            return false;
        }

        public override void HandlePrerequisiteUpdates() {
            //Debug.Log(gameObject.name + ".AnimatedObject.HandldePrerequisiteUpdates()");
            base.HandlePrerequisiteUpdates();
            MiniMapStatusUpdateHandler(this);
        }

        public override void OnDisable() {
            base.OnDisable();
        }
    }

}