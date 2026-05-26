using UnityEngine;

namespace AnyRPG {
    public class DialogInteractable : InteractableOption {

        [SerializeField]
        private DialogProps dialogProps = new DialogProps();

        public override InteractableOptionProps InteractableOptionProps { get => dialogProps; }
    }

}