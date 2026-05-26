using UnityEngine;

namespace AnyRPG {
    public class LoadSceneInteractable : PortalInteractable {

        [SerializeField]
        private LoadSceneProps loadSceneProps = new LoadSceneProps();

        public override InteractableOptionProps InteractableOptionProps { get => loadSceneProps; }
        public LoadSceneProps LoadSceneProps { get => loadSceneProps; }
    }
}