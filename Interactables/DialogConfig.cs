using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Dialog Config", menuName = "AnyRPG/Interactable/DialogConfig")]
    [System.Serializable]
    public class DialogConfig : InteractableOptionConfig {

        [Header("Dialog")]

        [Tooltip("The names of the dialogs available to this interactable")]
        [SerializeField]
        private List<string> dialogNames = new List<string>();

        public override Sprite Icon { get => (SystemConfigurationManager.MyInstance.MyDialogInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyDialogInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemConfigurationManager.MyInstance.MyDialogNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyDialogNamePlateImage : base.NamePlateImage); }
        public List<string> DialogNames { get => dialogNames; set => dialogNames = value; }

        public InteractableOption GetInteractableOption(Interactable interactable) {
            return new DialogInteractable(interactable, this);
        }
    }

}