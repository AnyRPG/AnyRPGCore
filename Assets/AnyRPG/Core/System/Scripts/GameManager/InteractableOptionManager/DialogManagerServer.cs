using System.Collections.Generic;

namespace AnyRPG {
    public class DialogManagerServer : InteractableOptionManager {

        public void TurnInDialog(UnitController sourceUnitController, Interactable interactable, int componentIndex, Dialog dialog) {
            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(sourceUnitController);
            if (currentInteractables[componentIndex] is DialogComponent) {
                (currentInteractables[componentIndex] as DialogComponent).TurnInDialog(sourceUnitController, dialog);
            }
        }

    }

}