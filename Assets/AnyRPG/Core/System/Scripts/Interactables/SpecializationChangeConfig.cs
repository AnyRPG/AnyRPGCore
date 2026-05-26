using UnityEngine;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Specialization Change Config", menuName = "AnyRPG/Interactable/SpecializationChangeConfig")]
    public class SpecializationChangeConfig : InteractableOptionConfig {

        [SerializeField]
        private SpecializationChangeProps interactableOptionProps = new SpecializationChangeProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}