using UnityEngine;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Gathering Node Config", menuName = "AnyRPG/Interactable/GatheringNodeConfig")]
    public class GatheringNodeConfig : InteractableOptionConfig {

        [SerializeField]
        private GatheringNodeProps interactableOptionProps = new GatheringNodeProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}