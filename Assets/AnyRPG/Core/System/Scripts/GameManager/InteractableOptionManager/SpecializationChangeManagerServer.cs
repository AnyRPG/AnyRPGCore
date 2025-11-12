using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SpecializationChangeManagerServer : InteractableOptionManager {

        public void ChangeCharacterSpecialization(UnitController sourceUnitController, Interactable interactable, int componentIndex) {
            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(sourceUnitController);
            if (currentInteractables[componentIndex] is SpecializationChangeComponent) {
                (currentInteractables[componentIndex] as SpecializationChangeComponent).ChangeCharacterSpecialization(sourceUnitController);
            }
        }

    }

}