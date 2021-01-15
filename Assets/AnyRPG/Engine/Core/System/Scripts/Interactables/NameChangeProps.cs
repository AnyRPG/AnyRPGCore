using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class NameChangeProps : InteractableOptionProps {

        public override Sprite Icon { get => (SystemConfigurationManager.MyInstance.NameChangeInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.NameChangeInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemConfigurationManager.MyInstance.MyNameChangeNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyNameChangeNamePlateImage : base.NamePlateImage); }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new NameChangeComponent(interactable, this);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }

    }

}