using System.Collections.Generic;

namespace AnyRPG {
    public class FactionChangeManagerServer : InteractableOptionManager {

        public void ChangeCharacterFaction(UnitController sourceUnitController, Interactable interactable, int componentIndex) {
            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(sourceUnitController);
            if (currentInteractables[componentIndex] is FactionChangeComponent) {
                (currentInteractables[componentIndex] as FactionChangeComponent).ChangeCharacterFaction(sourceUnitController);
            }
        }

    }

}