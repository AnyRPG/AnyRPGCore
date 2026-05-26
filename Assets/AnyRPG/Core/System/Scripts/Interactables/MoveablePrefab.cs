using UnityEngine;

namespace AnyRPG {
    public class MoveablePrefab : InteractableOption {

        [SerializeField]
        private MoveablePrefabProps animatedObjectProps = new MoveablePrefabProps();

        public override InteractableOptionProps InteractableOptionProps { get => animatedObjectProps; }
    }

}