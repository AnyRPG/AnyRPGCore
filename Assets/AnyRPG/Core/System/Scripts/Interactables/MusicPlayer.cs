using UnityEngine;

namespace AnyRPG {
    public class MusicPlayer : InteractableOption {

        [SerializeField]
        private MusicPlayerProps musicPlayerProps = new MusicPlayerProps();

        public override InteractableOptionProps InteractableOptionProps { get => musicPlayerProps; }
    }

}