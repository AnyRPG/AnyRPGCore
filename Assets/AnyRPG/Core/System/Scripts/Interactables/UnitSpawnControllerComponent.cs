using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class UnitSpawnControllerComponent : InteractableOptionComponent {

        // game manager references
        private UnitSpawnManager unitSpawnManager = null;

        public UnitSpawnControllerProps Props { get => interactableOptionProps as UnitSpawnControllerProps; }

        public UnitSpawnControllerComponent(Interactable interactable, UnitSpawnControllerProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            if (interactionPanelTitle == string.Empty) {
                interactionPanelTitle = "Spawn Characters";
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            unitSpawnManager = systemGameManager.UnitSpawnManager;
        }

        public override bool ProcessInteract(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            base.ProcessInteract(sourceUnitController, componentIndex, choiceIndex);
            return true;
        }

        public override void ClientInteraction(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            base.ClientInteraction(sourceUnitController, componentIndex, choiceIndex);
            unitSpawnManager.SetProps(Props, this, componentIndex, choiceIndex);
            uIManager.unitSpawnWindow.OpenWindow();
        }

        public override void StopInteract() {
            base.StopInteract();
            uIManager.unitSpawnWindow.CloseWindow();
        }

        public override int GetCurrentOptionCount(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.CharacterCreatorInteractable.GetCurrentOptionCount()");
            return GetValidOptionCount(sourceUnitController);
        }

        public void SpawnUnit(UnitController sourceUnitController, int unitLevel, int extraLevels, bool useDynamicLevel, UnitProfile unitProfile, UnitToughness unitToughness) {
            //Debug.Log($"{interactable.gameObject.name}UnitSpawnManager.SpawnUnit({sourceUnitController.gameObject.name}, {unitLevel}, {extraLevels}, {useDynamicLevel}, {unitProfile.ResourceName}, {(unitToughness == null ? string.Empty : unitToughness.ResourceName)})");

            foreach (UnitSpawnNode unitSpawnNode in Props.UnitSpawnNodeList) {
                if (unitSpawnNode != null) {
                    unitSpawnNode.ManualSpawn(unitLevel, extraLevels, useDynamicLevel, unitProfile, unitToughness, sourceUnitController);
                }
            }
            NotifyOnConfirmAction(sourceUnitController);
        }


    }

}