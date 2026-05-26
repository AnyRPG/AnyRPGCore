namespace AnyRPG {
    public class ItemPickupComponent : LootableNodeComponent {

        public ItemPickupProps ItemPickupProps { get => interactableOptionProps as ItemPickupProps; }

        public ItemPickupComponent(Interactable interactable, ItemPickupProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
        }

        public override int GetValidOptionCount(UnitController sourceUnitController) {
            //Debug.Log(interactable.gameObject.name + ".ItemPickupComponent.GetValidOptionCount()");
            int returnValue = base.GetValidOptionCount(sourceUnitController);
            if (returnValue == 0) {
                return returnValue;
            }
            if ((ItemPickupProps.SpawnTimer == -1  && pickupCount > 0) || spawnCoroutine != null) {
                return 0;
            }
            return returnValue;
        }

    }

}