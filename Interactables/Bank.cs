using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class Bank : InteractableOption {

        public override event System.Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        [SerializeField]
        private BankConfig bankConfig = new BankConfig();

        public override Sprite Icon {
            get {
                return bankConfig.Icon;
            } 
        }
        public override Sprite NamePlateImage {
            get {
                return bankConfig.NamePlateImage;
            }
        }

        public Bank(Interactable interactable, BankConfig interactableConfig) : base(interactable) {
            this.bankConfig = interactableConfig;
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