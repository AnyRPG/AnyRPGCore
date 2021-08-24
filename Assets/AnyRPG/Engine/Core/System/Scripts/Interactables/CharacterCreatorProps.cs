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

        public override Sprite Icon { get => (systemConfigurationManager.CharacterCreatorInteractionPanelImage != null ? systemConfigurationManager.CharacterCreatorInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (systemConfigurationManager.CharacterCreatorNamePlateImage != null ? systemConfigurationManager.CharacterCreatorNamePlateImage : base.NamePlateImage); }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new CharacterCreatorComponent(interactable, this, systemGameManager);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }
    }

}