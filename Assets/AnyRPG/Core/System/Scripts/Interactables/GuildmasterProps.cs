using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class GuildmasterProps : InteractableOptionProps {

        public override Sprite Icon { get => (systemConfigurationManager.GuildmasterInteractionPanelImage != null ? systemConfigurationManager.GuildmasterInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (systemConfigurationManager.GuildmasterNamePlateImage != null ? systemConfigurationManager.GuildmasterNamePlateImage : base.NamePlateImage); }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new GuildmasterComponent(interactable, this, systemGameManager);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }

    }

}