using UnityEngine;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Item Pickup Config", menuName = "AnyRPG/Interactable/ItemPickupConfig")]
    public class ItemPickupConfig : InteractableOptionConfig {

        [SerializeField]
        private ItemPickupProps interactableOptionProps = new ItemPickupProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}