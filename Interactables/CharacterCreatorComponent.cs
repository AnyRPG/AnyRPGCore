using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CharacterCreatorComponent : InteractableOptionComponent {

        public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        private CharacterCreatorProps interactableOptionProps = new CharacterCreatorProps();

        public override Sprite Icon { get => interactableOptionProps.Icon; }
        public override Sprite NamePlateImage { get => interactableOptionProps.NamePlateImage; }

        public CharacterCreatorComponent(Interactable interactable, CharacterCreatorProps interactableOptionProps) : base(interactable) {
            this.interactableOptionProps = interactableOptionProps;
        }

        public void CleanupEventSubscriptions(ICloseableWindowContents windowContents) {
            CleanupWindowEventSubscriptions();
        }

        public void CleanupWindowEventSubscriptions() {
            if (SystemWindowManager.MyInstance != null && SystemWindowManager.MyInstance.characterCreatorWindow != null && SystemWindowManager.MyInstance.characterCreatorWindow.MyCloseableWindowContents != null) {
                (SystemWindowManager.MyInstance.characterCreatorWindow.MyCloseableWindowContents as CharacterCreatorPanel).OnConfirmAction -= HandleConfirmAction;
                (SystemWindowManager.MyInstance.characterCreatorWindow.MyCloseableWindowContents as CharacterCreatorPanel).OnCloseWindow -= CleanupEventSubscriptions;
            }
        }

        public override void CleanupEventSubscriptions() {
            base.CleanupEventSubscriptions();
            CleanupWindowEventSubscriptions();
        }

        public override bool Interact(CharacterUnit source) {
            // was there a reason why we didn't have base.Interact here before or just an oversight?
            base.Interact(source);
            SystemWindowManager.MyInstance.characterCreatorWindow.OpenWindow();
            (SystemWindowManager.MyInstance.characterCreatorWindow.MyCloseableWindowContents as CharacterCreatorPanel).OnConfirmAction += HandleConfirmAction;
            (SystemWindowManager.MyInstance.characterCreatorWindow.MyCloseableWindowContents as CharacterCreatorPanel).OnCloseWindow += CleanupEventSubscriptions;
            return true;
        }

        /// <summary>
        /// Pick an item up off the ground and put it in the inventory
        /// </summary>

        public override void StopInteract() {
            base.StopInteract();
            SystemWindowManager.MyInstance.characterCreatorWindow.CloseWindow();
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
            text.fontSize = 50;
            text.color = Color.cyan;
            return true;
        }

        public override int GetCurrentOptionCount() {
            //Debug.Log(gameObject.name + ".CharacterCreatorInteractable.GetCurrentOptionCount()");
            return GetValidOptionCount();
        }

        public override void HandlePrerequisiteUpdates() {
            base.HandlePrerequisiteUpdates();
            MiniMapStatusUpdateHandler(this);
        }

        public override void HandlePlayerUnitSpawn() {
            base.HandlePlayerUnitSpawn();
            MiniMapStatusUpdateHandler(this);
        }
    }
}