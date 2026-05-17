using UnityEngine;

namespace AnyRPG {
    public class DroppedItem : InteractableOption {

        [SerializeField]
        private DroppedItemProps droppedItemProps = new DroppedItemProps();

        public override InteractableOptionProps InteractableOptionProps { get => droppedItemProps; }
    }

}