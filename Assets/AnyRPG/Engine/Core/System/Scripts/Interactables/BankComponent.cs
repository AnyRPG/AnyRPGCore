using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class BankComponent : InteractableOptionComponent {

        public BankProps Props { get => interactableOptionProps as BankProps; }

        public BankComponent(Interactable interactable, BankProps interactableOptionProps) : base(interactable, interactableOptionProps) {
            interactableOptionProps.InteractionPanelTitle = "Bank";
        }

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            //Debug.Log(gameObject.name + ".Bank.Interact(" + (source == null ? "null" : source.name) +")");
            base.Interact(source, optionIndex);
            PopupWindowManager.Instance.interactionWindow.CloseWindow();
            if (!PopupWindowManager.Instance.bankWindow.IsOpen) {
                PopupWindowManager.Instance.bankWindow.OpenWindow();
                return true;
            }
            return false;
        }

        public override void StopInteract() {
            base.StopInteract();
            PopupWindowManager.Instance.bankWindow.CloseWindow();
        }

    }

}