using UnityEngine;

namespace AnyRPG {
    public class ClassChangeInteractable : InteractableOption {

        [SerializeField]
        private ClassChangeProps classChangeProps = new ClassChangeProps();

        public override InteractableOptionProps InteractableOptionProps { get => classChangeProps; }
    }

}