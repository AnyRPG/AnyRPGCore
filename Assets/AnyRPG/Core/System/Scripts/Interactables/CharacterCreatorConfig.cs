using UnityEngine;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Character Creator Config", menuName = "AnyRPG/Interactable/CharacterCreatorConfig")]
    public class CharacterCreatorConfig : InteractableOptionConfig {

        [SerializeField]
        private CharacterCreatorProps interactableOptionProps = new CharacterCreatorProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}