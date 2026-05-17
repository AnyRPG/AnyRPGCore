using AnyRPG;
using UnityEngine;

namespace AnyRPG {
    public class BankComponent : InteractableOptionComponent {

        public BankProps Props { get => interactableOptionProps as BankProps; }

        public BankComponent(Interactable interactable, BankProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            interactionPanelTitle = "Bank";
        }

        public override bool ProcessInteract(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            //Debug.Log($"{gameObject.name}.Bank.Interact(" + (source == null ? "null" : source.name) +")");
            base.ProcessInteract(sourceUnitController, componentIndex, choiceIndex);
            return true;
        }

        public override void ClientInteraction(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            base.ClientInteraction(sourceUnitController, componentIndex, choiceIndex);
            uIManager.interactionWindow.CloseWindow();
            if (!uIManager.bankWindow.IsOpen) {
                uIManager.bankWindow.OpenWindow();
            }
        }

        public override void StopInteract() {
            base.StopInteract();
            uIManager.bankWindow.CloseWindow();
        }

        public override bool PlayInteractionSound() {
            return true;
        }

    }

}