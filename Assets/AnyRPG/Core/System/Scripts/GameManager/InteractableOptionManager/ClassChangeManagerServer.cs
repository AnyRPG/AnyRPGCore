using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ClassChangeManagerServer : InteractableOptionManager {

        public void ChangeCharacterClass(UnitController sourceUnitController, Interactable interactable, int componentIndex) {
            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(sourceUnitController);
            if (currentInteractables[componentIndex] is ClassChangeComponent) {
                (currentInteractables[componentIndex] as ClassChangeComponent).ChangeCharacterClass(sourceUnitController);
            }
        }

    }

}