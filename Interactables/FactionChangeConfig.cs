using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Faction Change Config", menuName = "AnyRPG/Interactable/FactionChangeConfig")]
    [System.Serializable]
    public class FactionChangeConfig : InteractableOptionConfig {

        [SerializeField]
        private FactionChangeProps interactableOptionProps = new FactionChangeProps();

        [Tooltip("the faction that this interactable option offers")]
        [SerializeField]
        private string factionName = string.Empty;

        public override Sprite Icon { get => (SystemConfigurationManager.MyInstance.MyFactionChangeInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyFactionChangeInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemConfigurationManager.MyInstance.MyFactionChangeNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyFactionChangeNamePlateImage : base.NamePlateImage); }
        public string FactionName { get => factionName; set => factionName = value; }
        public FactionChangeProps InteractableOptionProps { get => interactableOptionProps; set => interactableOptionProps = value; }
    }

}