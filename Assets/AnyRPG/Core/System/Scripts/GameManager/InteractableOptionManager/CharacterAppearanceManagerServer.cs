using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterAppearanceManagerServer : InteractableOptionManager {

        public void UpdatePlayerAppearance(UnitController sourceUnitController, int accountId, Interactable interactable, int componentIndex, string unitProfileName, string appearanceString, List<SwappableMeshSaveData> swappableMeshSaveData) {
            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(sourceUnitController);
            if (currentInteractables[componentIndex] is CharacterCreatorComponent) {
                (currentInteractables[componentIndex] as CharacterCreatorComponent).UpdatePlayerAppearance(sourceUnitController, accountId, unitProfileName, appearanceString, swappableMeshSaveData);
            }
        }
    }

}