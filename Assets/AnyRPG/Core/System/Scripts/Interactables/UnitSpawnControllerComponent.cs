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

        public UnitSpawnControllerComponent(Interactable interactable, UnitSpawnControllerProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            if (interactableOptionProps.GetInteractionPanelTitle() == string.Empty) {
                interactableOptionProps.InteractionPanelTitle = "Spawn Characters";
            }
        }

        //public void CleanupEventSubscriptions(ICloseableWindowContents windowContents) {
        public void CleanupEventSubscriptions(CloseableWindowContents windowContents) {
            CleanupWindowEventSubscriptions();
        }

        public void CleanupWindowEventSubscriptions() {
            if (uIManager.unitSpawnWindow.CloseableWindowContents != null) {
                (uIManager.unitSpawnWindow.CloseableWindowContents as UnitSpawnControlPanel).OnConfirmAction -= HandleConfirmAction;
                (uIManager.unitSpawnWindow.CloseableWindowContents as UnitSpawnControlPanel).OnCloseWindow -= CleanupEventSubscriptions;
            }
        }

        public override void ProcessCleanupEventSubscriptions() {
            base.ProcessCleanupEventSubscriptions();
            CleanupWindowEventSubscriptions();
        }

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            base.Interact(source, optionIndex);
            (uIManager.unitSpawnWindow.CloseableWindowContents as UnitSpawnControlPanel).UnitProfileList = Props.UnitProfileList;
            (uIManager.unitSpawnWindow.CloseableWindowContents as UnitSpawnControlPanel).UnitSpawnNodeList = Props.UnitSpawnNodeList;
            uIManager.unitSpawnWindow.OpenWindow();
            (uIManager.unitSpawnWindow.CloseableWindowContents as UnitSpawnControlPanel).OnConfirmAction += HandleConfirmAction;
            (uIManager.unitSpawnWindow.CloseableWindowContents as UnitSpawnControlPanel).OnCloseWindow += CleanupEventSubscriptions;
            return true;
        }

        /// <summary>
        /// Pick an item up off the ground and put it in the inventory
        /// </summary>

        public override void StopInteract() {
            base.StopInteract();
            uIManager.unitSpawnWindow.CloseWindow();
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