using UnityEngine;

namespace AnyRPG {
    public class GatheringNode : LootableNode {

        [SerializeField]
        private GatheringNodeProps gatheringNodeProps = new GatheringNodeProps();

        public override InteractableOptionProps InteractableOptionProps { get => gatheringNodeProps; }
    }

}