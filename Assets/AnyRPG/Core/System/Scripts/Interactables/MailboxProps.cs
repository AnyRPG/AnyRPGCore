using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class MailboxProps : InteractableOptionProps {

        public override Sprite Icon { get => (systemConfigurationManager.MailboxInteractionPanelImage != null ? systemConfigurationManager.MailboxInteractionPanelImage : base.Icon); }
        public override Sprite NameplateImage { get => (systemConfigurationManager.MailboxNameplateImage != null ? systemConfigurationManager.MailboxNameplateImage : base.NameplateImage); }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new MailboxComponent(interactable, this, systemGameManager);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }

    }

}