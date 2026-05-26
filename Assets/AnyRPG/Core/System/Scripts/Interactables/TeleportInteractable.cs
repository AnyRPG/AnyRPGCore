using UnityEngine;

namespace AnyRPG {
    public class TeleportInteractable : PortalInteractable {

        [SerializeField]
        private TeleportProps teleportProps = new TeleportProps();

        public override InteractableOptionProps InteractableOptionProps { get => teleportProps; }
    }
}