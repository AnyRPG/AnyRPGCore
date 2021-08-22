using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CharacterCreatorComponent : InteractableOptionComponent {

        public CharacterCreatorProps Props { get => interactableOptionProps as CharacterCreatorProps; }

        public CharacterCreatorComponent(Interactable interactable, CharacterCreatorProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
        }

        public void CleanupEventSubscriptions(ICloseableWindowContents windowContents) {
            CleanupWindowEventSubscriptions();
        }

        public void CleanupWindowEventSubscriptions() {
            if (uIManager != null && uIManager.characterCreatorWindow != null && uIManager.characterCreatorWindow.CloseableWindowContents != null) {
                (uIManager.characterCreatorWindow.CloseableWindowContents as CharacterCreatorWindowPanel).OnConfirmAction -= HandleConfirmAction;
                (uIManager.characterCreatorWindow.CloseableWindowContents as CharacterCreatorWindowPanel).OnCloseWindow -= CleanupEventSubscriptions;
            }
        }

        public override void CleanupEventSubscriptions() {
            base.CleanupEventSubscriptions();
            CleanupWindowEventSubscriptions();
        }

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            // was there a reason why we didn't have base.Interact here before or just an oversight?
            base.Interact(source, optionIndex);
            uIManager.characterCreatorWindow.OpenWindow();
            (uIManager.characterCreatorWindow.CloseableWindowContents as CharacterCreatorWindowPanel).OnConfirmAction += HandleConfirmAction;
            (uIManager.characterCreatorWindow.CloseableWindowContents as CharacterCreatorWindowPanel).OnCloseWindow += CleanupEventSubscriptions;
            return true;
        }

        /// <summary>
        /// Pick an item up off the ground and put it in the inventory
        /// </summary>

        public override void StopInteract() {
            base.StopInteract();
            uIManager.characterCreatorWindow.CloseWindow();
        }

        public override bool HasMiniMapText() {
            return true;
        }

        public override bool SetMiniMapText(TextMeshProUGUI text) {
            //Debug.Log(gameObject.name + ".CharacterCreatorInteractable.SetMiniMapText(" + text + ")");
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
            //Debug.Log(gameObject.name + ".CharacterCreatorInteractable.GetCurrentOptionCount()");
            return GetValidOptionCount();
        }

    }

}