using UnityEngine;

namespace AnyRPG {
    public class SpecializationChangeInteractable : InteractableOption {

        [SerializeField]
        private SpecializationChangeProps specializationChangeProps = new SpecializationChangeProps();

        public override InteractableOptionProps InteractableOptionProps { get => specializationChangeProps; }
    }

}