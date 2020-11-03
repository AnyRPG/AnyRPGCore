using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class UnitSpawnControllerComponent : InteractableOptionComponent {

        public override event Action<InteractableOptionComponent> MiniMapStatusUpdateHandler = delegate { };

        private UnitSpawnControllerProps interactableOptionProps = null;

        public override Sprite Icon { get => interactableOptionProps.Icon; }
        public override Sprite NamePlateImage { get => interactableOptionProps.Icon; }

        private List<UnitProfile> unitProfileList = new List<UnitProfile>();

        [Tooltip("List of Unit Spawn Nodes to control")]
        [SerializeField]
        private List<UnitSpawnNode> unitSpawnNodeList = new List<UnitSpawnNode>();

        public UnitSpawnControllerComponent(Interactable interactable, UnitSpawnControllerProps interactableOptionProps) : base(interactable) {
            this.interactableOptionProps = interactableOptionProps;
        }

        public void CleanupEventSubscriptions(ICloseableWindowContents windowContents) {
            CleanupWindowEventSubscriptions();
        }

        public void CleanupWindowEventSubscriptions() {
            if (SystemWindowManager.MyInstance != null && SystemWindowManager.MyInstance.unitSpawnWindow != null && SystemWindowManager.MyInstance.unitSpawnWindow.MyCloseableWindowContents != null) {
                (SystemWindowManager.MyInstance.unitSpawnWindow.MyCloseableWindowContents as UnitSpawnControlPanel).OnConfirmAction -= HandleConfirmAction;
                (SystemWindowManager.MyInstance.unitSpawnWindow.MyCloseableWindowContents as UnitSpawnControlPanel).OnCloseWindow -= CleanupEventSubscriptions;
            }
        }

        public override void CleanupEventSubscriptions() {
            base.CleanupEventSubscriptions();
            CleanupWindowEventSubscriptions();
        }

        public override bool Interact(CharacterUnit source) {
            base.Interact(source);
            (SystemWindowManager.MyInstance.unitSpawnWindow.MyCloseableWindowContents as UnitSpawnControlPanel).MyUnitProfileList = unitProfileList;
            (SystemWindowManager.MyInstance.unitSpawnWindow.MyCloseableWindowContents as UnitSpawnControlPanel).MyUnitSpawnNodeList = unitSpawnNodeList;
            SystemWindowManager.MyInstance.unitSpawnWindow.OpenWindow();
            (SystemWindowManager.MyInstance.unitSpawnWindow.MyCloseableWindowContents as UnitSpawnControlPanel).OnConfirmAction += HandleConfirmAction;
            (SystemWindowManager.MyInstance.unitSpawnWindow.MyCloseableWindowContents as UnitSpawnControlPanel).OnCloseWindow += CleanupEventSubscriptions;
            return true;
        }

        /// <summary>
        /// Pick an item up off the ground and put it in the inventory
        /// </summary>

        public override void StopInteract() {
            base.StopInteract();
            SystemWindowManager.MyInstance.unitSpawnWindow.CloseWindow();
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


        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            unitProfileList = new List<UnitProfile>();
            if (interactableOptionProps.UnitProfileNames != null) {
                foreach (string unitProfileName in interactableOptionProps.UnitProfileNames) {
                    UnitProfile tmpUnitProfile = SystemUnitProfileManager.MyInstance.GetResource(unitProfileName);
                    if (tmpUnitProfile != null) {
                        unitProfileList.Add(tmpUnitProfile);
                    } else {
                        Debug.LogError("UnitSpawnControllerComponent.SetupScriptableObjects(): COULD NOT FIND UNIT PROFILE: " + unitProfileName + " while initializing");
                    }
                }
            }
        }
    }
}