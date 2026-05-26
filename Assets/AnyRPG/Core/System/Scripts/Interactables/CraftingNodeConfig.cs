using UnityEngine;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Crafting Node Config", menuName = "AnyRPG/Interactable/CraftingNodeConfig")]
    [System.Serializable]
    public class CraftingNodeConfig : InteractableOptionConfig {

        [SerializeField]
        private CraftingNodeProps interactableOptionProps = new CraftingNodeProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}