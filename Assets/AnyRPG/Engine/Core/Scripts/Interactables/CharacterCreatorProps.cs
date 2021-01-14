using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class CharacterCreatorProps : InteractableOptionProps {

        public override Sprite Icon { get => (SystemConfigurationManager.MyInstance.MyCharacterCreatorInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyCharacterCreatorInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemConfigurationManager.MyInstance.MyCharacterCreatorNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyCharacterCreatorNamePlateImage : base.NamePlateImage); }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new CharacterCreatorComponent(interactable, this);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }
    }

}