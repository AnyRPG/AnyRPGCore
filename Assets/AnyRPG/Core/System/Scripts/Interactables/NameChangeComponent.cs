using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class NameChangeComponent : InteractableOptionComponent {

        public NameChangeProps Props { get => interactableOptionProps as NameChangeProps; }

        private bool windowEventSubscriptionsInitialized = false;

        public NameChangeComponent(Interactable interactable, NameChangeProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
        }

        public void CleanupEventSubscriptions(ICloseableWindowContents windowContents) {
            //Debug.Log(gameObject.name + ".NameChangeInteractable.CleanupEventSubscriptions(ICloseableWindowContents)");
            CleanupWindowEventSubscriptions();
        }

        public void CleanupWindowEventSubscriptions() {
            if (uIManager.nameChangeWindow.CloseableWindowContents != null) {
                (uIManager.nameChangeWindow.CloseableWindowContents as NameChangePanelController).OnConfirmAction -= HandleConfirmAction;
                (uIManager.nameChangeWindow.CloseableWindowContents as NameChangePanelController).OnCloseWindow -= CleanupEventSubscriptions;
            }
            windowEventSubscriptionsInitialized = false;
        }

        public override void ProcessCleanupEventSubscriptions() {
            //Debug.Log(gameObject.name + ".NameChangeInteractable.CleanupEventSubscriptions()");
            base.ProcessCleanupEventSubscriptions();
            CleanupWindowEventSubscriptions();
        }

        public override void HandleConfirmAction() {
            //Debug.Log(gameObject.name + ".NameChangeInteractable.HandleConfirmAction()");
            base.HandleConfirmAction();

            // just to be safe
            CleanupWindowEventSubscriptions();
        }

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            //Debug.Log(gameObject.name + ".NameChangeInteractable.Interact()");
            if (windowEventSubscriptionsInitialized == true) {
                //Debug.Log(gameObject.name + ".NameChangeInteractable.Interact(): EVENT SUBSCRIPTIONS WERE ALREADY INITIALIZED!!! RETURNING");
                return false;
            }
            base.Interact(source, optionIndex);

            uIManager.nameChangeWindow.OpenWindow();
            (uIManager.nameChangeWindow.CloseableWindowContents as NameChangePanelController).OnConfirmAction += HandleConfirmAction;
            (uIManager.nameChangeWindow.CloseableWindowContents as NameChangePanelController).OnCloseWindow += CleanupEventSubscriptions;
            windowEventSubscriptionsInitialized = true;
            return true;
        }



        /// <summary>
        /// Pick an item up off the ground and put it in the inventory
        /// </summary>

        public override void StopInteract() {
            base.StopInteract();
            uIManager.nameChangeWindow.CloseWindow();
        }

        public override bool HasMiniMapText() {
            return true;
        }

        public override bool SetMiniMapText(TextMeshProUGUI text) {
            //Debug.Log(gameObject.name + ".NameChangeInteractable.SetMiniMapText(" + text + ")");
            if (!base.SetMiniMapText(text)) {
                text.text = "";
                text.color = new Color32(0, 0, 0, 0);
                return false;
            }
            text.text = "o";
            text.color = Color.cyan;
            return true;
        }

        public override int GetCurrentOptionCount() {
            //Debug.Log(interactable.gameObject.name + ".NameChangeInteractable.GetCurrentOptionCount(): returning " + GetValidOptionCount());
            return GetValidOptionCount();
        }

    }

}