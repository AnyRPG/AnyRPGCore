using UnityEngine;

namespace AnyRPG {
    public class BehaviorInteractable : InteractableOption {

        [SerializeField]
        private BehaviorProps behaviorProps = new BehaviorProps();

        public override InteractableOptionProps InteractableOptionProps { get => behaviorProps; }
    }

}