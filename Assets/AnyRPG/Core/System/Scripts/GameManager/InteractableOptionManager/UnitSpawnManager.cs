using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class UnitSpawnManager : InteractableOptionManager {

        private UnitSpawnControllerProps unitSpawnControllerProps = null;
        private UnitSpawnControllerComponent unitSpawnControllerComponent = null;

        public UnitSpawnControllerProps UnitSpawnControllerProps { get => unitSpawnControllerProps; set => unitSpawnControllerProps = value; }

        // game manager references
        PlayerManager playerManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            playerManager = systemGameManager.PlayerManager;
        }

        public void SetProps(UnitSpawnControllerProps unitSpawnControllerProps, UnitSpawnControllerComponent unitSpawnControllerComponent, int componentIndex, int choiceIndex) {
            //Debug.Log("UnitSpawnManager.SetProps()");

            this.unitSpawnControllerProps = unitSpawnControllerProps;
            this.unitSpawnControllerComponent = unitSpawnControllerComponent;
            BeginInteraction(unitSpawnControllerComponent, componentIndex, choiceIndex);
        }

        public void RequestSpawnUnit(int unitLevel, int extraLevels, bool useDynamicLevel, UnitProfile unitProfile, UnitToughness unitToughness) {
            //Debug.Log("UnitSpawnManager.RequestSpawnUnit()");

            if (systemGameManager.GameMode == GameMode.Local) {
                unitSpawnControllerComponent.SpawnUnit(playerManager.UnitController, unitLevel, extraLevels, useDynamicLevel, unitProfile, unitToughness);
            } else {
                networkManagerClient.RequestSpawnUnit(unitSpawnControllerComponent.Interactable, componentIndex, unitLevel, extraLevels, useDynamicLevel, unitProfile.ResourceName, (unitToughness == null ? string.Empty : unitToughness.ResourceName));
            }
        }

        public void SpawnUnit(UnitController sourceUnitController, Interactable interactable, int componentIndex, int unitLevel, int extraLevels, bool useDynamicLevel, UnitProfile unitProfile, UnitToughness unitToughness) {
            //Debug.Log($"UnitSpawnManager.SpawnUnit({sourceUnitController.gameObject.name}, {interactable.gameObject.name}, {componentIndex}, {unitLevel}, {extraLevels}, {useDynamicLevel}, {unitProfile.ResourceName}, {(unitToughness == null ? string.Empty : unitToughness.ResourceName)})");

            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(sourceUnitController);
            if (currentInteractables[componentIndex] is UnitSpawnControllerComponent) {
                (currentInteractables[componentIndex] as UnitSpawnControllerComponent).SpawnUnit(sourceUnitController, unitLevel, extraLevels, useDynamicLevel, unitProfile, unitToughness);
            }
        }

        public override void EndInteraction() {
            base.EndInteraction();

            unitSpawnControllerProps = null;
            unitSpawnControllerComponent = null;
        }


    }

}