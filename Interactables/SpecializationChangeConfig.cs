using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Specialization Change Config", menuName = "AnyRPG/Interactable/SpecializationChangeConfig")]
    [System.Serializable]
    public class SpecializationChangeConfig : InteractableOptionConfig {

        [Tooltip("the class Specialization that this interactable option offers")]
        [SerializeField]
        private string specializationName = string.Empty;

        public override Sprite Icon { get => (SystemConfigurationManager.MyInstance.MyClassChangeInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyClassChangeInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemConfigurationManager.MyInstance.MyClassChangeNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyClassChangeNamePlateImage : base.NamePlateImage); }
        public string SpecializationName { get => specializationName; set => specializationName = value; }

        public InteractableOption GetInteractableOption(Interactable interactable) {
            return new SpecializationChangeInteractable(interactable, this);
        }
    }

}