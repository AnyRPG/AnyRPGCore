using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class AuctionProps : InteractableOptionProps {

        public override Sprite Icon { get => (systemConfigurationManager.AuctionInteractionPanelImage != null ? systemConfigurationManager.AuctionInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (systemConfigurationManager.AuctionNamePlateImage != null ? systemConfigurationManager.AuctionNamePlateImage : base.NamePlateImage); }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new AuctionComponent(interactable, this, systemGameManager);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }

    }

}