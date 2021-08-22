using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        public override Sprite NamePlateImage {
            get {
                if (systemConfigurationManager.BankNamePlateImage != null) {
                    return systemConfigurationManager.BankNamePlateImage;
                }
                return base.NamePlateImage;
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