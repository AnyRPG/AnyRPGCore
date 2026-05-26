using System.Collections.Generic;

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