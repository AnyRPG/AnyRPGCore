using AnyRPG;
using UnityEngine;

namespace AnyRPG {
    public class BankComponent : InteractableOptionComponent {

        // game manager references
        private UIManager uIManager = null;

        public BankProps Props { get => interactableOptionProps as BankProps; }

        public BankComponent(Interactable interactable, BankProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            interactableOptionProps.InteractionPanelTitle = "Bank";
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            uIManager = systemGameManager.UIManager;
        }

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            //Debug.Log(gameObject.name + ".Bank.Interact(" + (source == null ? "null" : source.name) +")");
            base.Interact(source, optionIndex);
            uIManager.interactionWindow.CloseWindow();
            if (!uIManager.bankWindow.IsOpen) {
                uIManager.bankWindow.OpenWindow();
                return true;
            }
            return false;
        }

        public override void StopInteract() {
            base.StopInteract();
            uIManager.bankWindow.CloseWindow();
        }

    }

}