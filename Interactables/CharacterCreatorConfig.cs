using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Character Creator Config", menuName = "AnyRPG/Interactable/CharacterCreatorConfig")]
    public class CharacterCreatorConfig : InteractableOptionConfig {

        [SerializeField]
        private CharacterCreatorProps interactableOptionProps = new CharacterCreatorProps();

        [Header("Dialog")]

        [Tooltip("The names of the dialogs available to this interactable")]
        [SerializeField]
        private List<string> dialogNames = new List<string>();

        public override Sprite Icon { get => (SystemConfigurationManager.MyInstance.MyCharacterCreatorInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyCharacterCreatorInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemConfigurationManager.MyInstance.MyCharacterCreatorNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyCharacterCreatorNamePlateImage : base.NamePlateImage); }

    }

}