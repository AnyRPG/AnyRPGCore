using UnityEngine;

namespace AnyRPG {
    public class AnimatedObject : InteractableOption {

        [SerializeField]
        private AnimatedObjectProps animatedObjectProps = new AnimatedObjectProps();

        public override InteractableOptionProps InteractableOptionProps { get => animatedObjectProps; }
    }

}