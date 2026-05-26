using System.Collections.Generic;

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