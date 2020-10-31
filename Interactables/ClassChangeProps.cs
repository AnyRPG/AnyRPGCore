using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class ClassChangeProps : InteractableOptionProps {

        [Tooltip("the class that this interactable option offers")]
        [SerializeField]
        private string className = string.Empty;

        public override Sprite Icon { get => (SystemConfigurationManager.MyInstance.MyClassChangeInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyClassChangeInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemConfigurationManager.MyInstance.MyClassChangeNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyClassChangeNamePlateImage : base.NamePlateImage); }
        public string ClassName { get => className; set => className = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable) {
            return new ClassChangeComponent(interactable, this);
        }
    }

}