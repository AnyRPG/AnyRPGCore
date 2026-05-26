using UnityEngine;

namespace AnyRPG {
    public class CutsceneInteractable : InteractableOption {

        [SerializeField]
        private CutsceneProps cutsceneProps = new CutsceneProps();

        public override InteractableOptionProps InteractableOptionProps { get => cutsceneProps; }
    }

}