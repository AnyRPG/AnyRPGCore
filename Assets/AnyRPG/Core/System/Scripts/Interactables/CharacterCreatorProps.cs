using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class CharacterCreatorProps : InteractableOptionProps {

        [SerializeField]
        private bool allowGenderChange = false;

        [Tooltip("If this list is empty, the current model will be edited.  If the list has items, model will be forced to be one of the models below.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(UnitProfile))]
        private List<string> unitProfiles = new List<string>();

        private List<UnitProfile> unitProfileList = new List<UnitProfile>();

        public override Sprite Icon { get => (systemConfigurationManager.CharacterCreatorInteractionPanelImage != null ? systemConfigurationManager.CharacterCreatorInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (systemConfigurationManager.CharacterCreatorNamePlateImage != null ? systemConfigurationManager.CharacterCreatorNamePlateImage : base.NamePlateImage); }
        public bool AllowGenderChange { get => allowGenderChange; }
        public List<UnitProfile> UnitProfileList { get => unitProfileList; set => unitProfileList = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new CharacterCreatorComponent(interactable, this, systemGameManager);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            if (unitProfiles.Count > 0) {
                foreach (string unitProfileName in unitProfiles) {
                    UnitProfile unitProfile = systemDataFactory.GetResource<UnitProfile>(unitProfileName);
                    if (unitProfile != null) {
                        unitProfileList.Add(unitProfile);
                    } else {
                        Debug.LogError("CharacterCreatorProps.SetupScriptableObjects(): Could not find unitProfile : " + unitProfileName + " while inititalizing.  CHECK INSPECTOR");
                    }
                }
            }
        }
    }

}