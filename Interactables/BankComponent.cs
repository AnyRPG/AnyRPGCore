using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class BankComponent : InteractableOptionComponent {

        public override event System.Action<InteractableOptionComponent> MiniMapStatusUpdateHandler = delegate { };

        private BankProps interactableOptionProps = null;

        public override Sprite Icon {
            get {
                return interactableOptionProps.Icon;
            } 
        }
        public override Sprite NamePlateImage {
            get {
                return interactableOptionProps.NamePlateImage;
            }
        }

        public BankComponent(Interactable interactable, BankProps interactableOptionProps) : base(interactable) {
            this.interactableOptionProps = interactableOptionProps;
            interactionPanelTitle = "Bank";
        }

        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".Bank.Interact(" + (source == null ? "null" : source.name) +")");
            base.Interact(source);
            PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
            if (!PopupWindowManager.MyInstance.bankWindow.IsOpen) {
                PopupWindowManager.MyInstance.bankWindow.OpenWindow();
                return true;
            }
            return false;
        }

        public override void StopInteract() {
            base.StopInteract();
            PopupWindowManager.MyInstance.bankWindow.CloseWindow();
        }

        public override void HandlePrerequisiteUpdates() {
            //Debug.Log(gameObject.name + ".Bank.HandldePrerequisiteUpdates()");
            base.HandlePrerequisiteUpdates();
            MiniMapStatusUpdateHandler(this);
        }

        public override void HandlePlayerUnitSpawn() {
            base.HandlePlayerUnitSpawn();
            MiniMapStatusUpdateHandler(this);
        }

    }

}