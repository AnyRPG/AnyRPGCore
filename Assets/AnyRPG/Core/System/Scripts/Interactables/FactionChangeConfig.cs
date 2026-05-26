using UnityEngine;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Faction Change Config", menuName = "AnyRPG/Interactable/FactionChangeConfig")]
    public class FactionChangeConfig : InteractableOptionConfig {

        [SerializeField]
        private FactionChangeProps interactableOptionProps = new FactionChangeProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}