using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class FactionChangeProps : InteractableOptionProps {

        [Tooltip("the faction that this interactable option offers")]
        [SerializeField]
        private string factionName = string.Empty;

        public override Sprite Icon { get => (SystemConfigurationManager.MyInstance.MyFactionChangeInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyFactionChangeInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemConfigurationManager.MyInstance.MyFactionChangeNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyFactionChangeNamePlateImage : base.NamePlateImage); }
        public string FactionName { get => factionName; set => factionName = value; }

        public InteractableOption GetInteractableOption(Interactable interactable) {
            return new FactionChangeInteractable(interactable, this);
        }
    }

}