using UnityEngine;

namespace AnyRPG {
    public class UnitSpawnControllerInteractable : InteractableOption {

        [SerializeField]
        private UnitSpawnControllerProps unitSpawnControllerProps = new UnitSpawnControllerProps();

        public override InteractableOptionProps InteractableOptionProps { get => unitSpawnControllerProps; }

    }
}