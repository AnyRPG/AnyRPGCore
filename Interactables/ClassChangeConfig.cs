using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Faction Change Config", menuName = "AnyRPG/Interactable/ClassChangeConfig")]
    [System.Serializable]
    public class ClassChangeConfig : InteractableOptionConfig {

        [Tooltip("the class that this interactable option offers")]
        [SerializeField]
        private string className = string.Empty;

        public override Sprite Icon { get => (SystemConfigurationManager.MyInstance.MyClassChangeInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyClassChangeInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemConfigurationManager.MyInstance.MyClassChangeNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyClassChangeNamePlateImage : base.NamePlateImage); }
        public string ClassName { get => className; set => className = value; }

        public InteractableOption GetInteractableOption(Interactable interactable) {
            return new ClassChangeInteractable(interactable, this);
        }
    }

}