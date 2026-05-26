using UnityEngine;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Load Scene Config", menuName = "AnyRPG/Interactable/LoadSceneConfig")]
    public class LoadSceneConfig : InteractableOptionConfig {

        [SerializeField]
        private LoadSceneProps interactableOptionProps = new LoadSceneProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}