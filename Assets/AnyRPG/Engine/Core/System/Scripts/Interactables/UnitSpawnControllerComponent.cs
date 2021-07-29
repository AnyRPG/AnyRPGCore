using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class UnitSpawnControllerComponent : InteractableOptionComponent {

        public UnitSpawnControllerProps Props { get => interactableOptionProps as UnitSpawnControllerProps; }

        public UnitSpawnControllerComponent(Interactable interactable, UnitSpawnControllerProps interactableOptionProps) : base(interactable, interactableOptionProps) {
            if (interactableOptionProps.GetInteractionPanelTitle() == string.Empty) {
                interactableOptionProps.InteractionPanelTitle = "Spawn Characters";
            }
        }

        public void CleanupEventSubscriptions(ICloseableWindowContents windowContents) {
            CleanupWindowEventSubscriptions();
        }

        public void CleanupWindowEventSubscriptions() {
            if (SystemGameManager.Instance.UIManager.SystemWindowManager != null && SystemGameManager.Instance.UIManager.SystemWindowManager.unitSpawnWindow != null && SystemGameManager.Instance.UIManager.SystemWindowManager.unitSpawnWindow.CloseableWindowContents != null) {
                (SystemGameManager.Instance.UIManager.SystemWindowManager.unitSpawnWindow.CloseableWindowContents as UnitSpawnControlPanel).OnConfirmAction -= HandleConfirmAction;
                (SystemGameManager.Instance.UIManager.SystemWindowManager.unitSpawnWindow.CloseableWindowContents as UnitSpawnControlPanel).OnCloseWindow -= CleanupEventSubscriptions;
            }
        }

        public override void CleanupEventSubscriptions() {
            base.CleanupEventSubscriptions();
            CleanupWindowEventSubscriptions();
        }

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            base.Interact(source, optionIndex);
            (SystemGameManager.Instance.UIManager.SystemWindowManager.unitSpawnWindow.CloseableWindowContents as UnitSpawnControlPanel).MyUnitProfileList = Props.UnitProfileList;
            (SystemGameManager.Instance.UIManager.SystemWindowManager.unitSpawnWindow.CloseableWindowContents as UnitSpawnControlPanel).MyUnitSpawnNodeList = Props.UnitSpawnNodeList;
            SystemGameManager.Instance.UIManager.SystemWindowManager.unitSpawnWindow.OpenWindow();
            (SystemGameManager.Instance.UIManager.SystemWindowManager.unitSpawnWindow.CloseableWindowContents as UnitSpawnControlPanel).OnConfirmAction += HandleConfirmAction;
            (SystemGameManager.Instance.UIManager.SystemWindowManager.unitSpawnWindow.CloseableWindowContents as UnitSpawnControlPanel).OnCloseWindow += CleanupEventSubscriptions;
            return true;
        }

        /// <summary>
        /// Pick an item up off the ground and put it in the inventory
        /// </summary>

        public override void StopInteract() {
            base.StopInteract();
            SystemGameManager.Instance.UIManager.SystemWindowManager.unitSpawnWindow.CloseWindow();
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