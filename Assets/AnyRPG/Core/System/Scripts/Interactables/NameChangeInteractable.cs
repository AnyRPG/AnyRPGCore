using UnityEngine;

namespace AnyRPG {
    public class NameChangeInteractable : InteractableOption {

        [SerializeField]
        private NameChangeProps nameChangeProps = new NameChangeProps();

        public override InteractableOptionProps InteractableOptionProps { get => nameChangeProps; }
    }

}