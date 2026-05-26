using UnityEngine;

namespace AnyRPG {
    public class CraftingNode : InteractableOption {

        [SerializeField]
        private CraftingNodeProps craftingNodeProps = new CraftingNodeProps();

        public override InteractableOptionProps InteractableOptionProps { get => craftingNodeProps; }
    }

}