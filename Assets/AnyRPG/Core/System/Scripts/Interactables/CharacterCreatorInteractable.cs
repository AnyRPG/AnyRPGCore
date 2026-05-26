using UnityEngine;

namespace AnyRPG {
    public class CharacterCreatorInteractable : InteractableOption {

        [SerializeField]
        private CharacterCreatorProps characterCreatorProps = new CharacterCreatorProps();

        public override InteractableOptionProps InteractableOptionProps { get => characterCreatorProps; }
    }
}