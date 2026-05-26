using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class BankProps : InteractableOptionProps {


        public override Sprite Icon {
            get {
                if (systemConfigurationManager.BankInteractionPanelImage != null) {
                    return systemConfigurationManager.BankInteractionPanelImage;
                }
                return base.Icon;
            }
        }

        public override Sprite NameplateImage {
            get {
                if (systemConfigurationManager.BankNameplateImage != null) {
                    return systemConfigurationManager.BankNameplateImage;
                }
                return base.NameplateImage;
            }
        }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new BankComponent(interactable, this, systemGameManager);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }
    }

}