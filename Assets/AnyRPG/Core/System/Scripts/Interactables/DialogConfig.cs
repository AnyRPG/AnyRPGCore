using UnityEngine;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Dialog Config", menuName = "AnyRPG/Interactable/DialogConfig")]
    public class DialogConfig : InteractableOptionConfig {

        [SerializeField]
        private DialogProps interactableOptionProps = new DialogProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}