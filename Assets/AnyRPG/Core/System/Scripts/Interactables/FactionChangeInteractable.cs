using UnityEngine;

namespace AnyRPG {
    public class FactionChangeInteractable : InteractableOption {

        [SerializeField]
        private FactionChangeProps factionChangeProps = new FactionChangeProps();

        public override InteractableOptionProps InteractableOptionProps { get => factionChangeProps; }
    }

}