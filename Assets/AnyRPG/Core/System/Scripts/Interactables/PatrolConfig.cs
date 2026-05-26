using UnityEngine;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Patrol Config", menuName = "AnyRPG/Interactable/Patrol Config")]
    public class PatrolConfig : InteractableOptionConfig {

        [SerializeField]
        private PatrolProps interactableOptionProps = new PatrolProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}