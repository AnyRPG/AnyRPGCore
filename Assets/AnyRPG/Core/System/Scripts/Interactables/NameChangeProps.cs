using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class NameChangeProps : InteractableOptionProps {

        public override Sprite Icon { get => (systemConfigurationManager.NameChangeInteractionPanelImage != null ? systemConfigurationManager.NameChangeInteractionPanelImage : base.Icon); }
        public override Sprite NameplateImage { get => (systemConfigurationManager.NameChangeNameplateImage != null ? systemConfigurationManager.NameChangeNameplateImage : base.NameplateImage); }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new NameChangeComponent(interactable, this, systemGameManager);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }

    }

}