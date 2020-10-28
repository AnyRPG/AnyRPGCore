using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Faction Change Config", menuName = "AnyRPG/Interactable/ClassChangeConfig")]
    public class ClassChangeConfig : InteractableOptionConfig {

        [SerializeField]
        private ClassChangeProps interactableOptionProps = new ClassChangeProps();

        [Tooltip("the class that this interactable option offers")]
        [SerializeField]
        private string className = string.Empty;

        public override Sprite Icon { get => (SystemConfigurationManager.MyInstance.MyClassChangeInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyClassChangeInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemConfigurationManager.MyInstance.MyClassChangeNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyClassChangeNamePlateImage : base.NamePlateImage); }
        public string ClassName { get => className; set => className = value; }

    }

}