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
                if (SystemConfigurationManager.Instance.BankInteractionPanelImage != null) {
                    return SystemConfigurationManager.Instance.BankInteractionPanelImage;
                }
                return base.Icon;
            }
        }

        public override Sprite NamePlateImage {
            get {
                if (SystemConfigurationManager.Instance.BankNamePlateImage != null) {
                    return SystemConfigurationManager.Instance.BankNamePlateImage;
                }
                return base.NamePlateImage;
            }
        }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new BankComponent(interactable, this);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }
    }

}