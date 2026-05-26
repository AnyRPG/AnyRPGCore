using UnityEngine;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Music Player Config", menuName = "AnyRPG/Interactable/MusicPlayerConfig")]
    public class MusicPlayerConfig : InteractableOptionConfig {

        [SerializeField]
        private MusicPlayerProps interactableOptionProps = new MusicPlayerProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}