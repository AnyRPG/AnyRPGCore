using UnityEngine;

namespace AnyRPG {
    public class ItemPickup : LootableNode {

        [SerializeField]
        private ItemPickupProps itemPickupProps = new ItemPickupProps();

        public override InteractableOptionProps InteractableOptionProps { get => itemPickupProps; }
    }

}