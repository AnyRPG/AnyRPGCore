using UnityEngine;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Cutscene Config", menuName = "AnyRPG/Interactable/CutsceneConfig")]
    public class CutsceneConfig : InteractableOptionConfig {

        [SerializeField]
        private CutsceneProps interactableOptionProps = new CutsceneProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}