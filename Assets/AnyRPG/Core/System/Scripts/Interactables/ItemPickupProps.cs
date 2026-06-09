namespace AnyRPG {

    [System.Serializable]
    public class ItemPickupProps : LootableNodeProps {

        public override InteractableOptionComponent GetInteractableOption(InteractableBase interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new ItemPickupComponent(interactable, this, systemGameManager);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }
    }

}