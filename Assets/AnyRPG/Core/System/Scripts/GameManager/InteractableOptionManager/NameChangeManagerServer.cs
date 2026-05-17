using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class NameChangeManagerServer : InteractableOptionManager {

        public void SetPlayerName(UnitController sourceUnitController, Interactable interactable, int componentIndex, string newName) {
            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(sourceUnitController);
            if (currentInteractables[componentIndex] is NameChangeComponent) {
                (currentInteractables[componentIndex] as NameChangeComponent).SetPlayerName(sourceUnitController, newName);
            }
        }

    }

}