using UnityEngine;

namespace AnyRPG {
    public class ActivatableObject : InteractableOption {
        
        [SerializeField]
        private ActivatableObjectProps activatableObjectProps = new ActivatableObjectProps();

        public override InteractableOptionProps InteractableOptionProps { get => activatableObjectProps; }

    }

}