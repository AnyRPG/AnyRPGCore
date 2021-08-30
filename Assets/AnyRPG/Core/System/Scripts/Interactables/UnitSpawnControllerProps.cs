using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class UnitSpawnControllerProps : InteractableOptionProps {

        [Header("Unit Spawn Controller")]

        [Tooltip("The names of the unit profiles that will be available to spawn with this controller")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(UnitProfile))]
        private List<string> unitProfileNames = new List<string>();

        [Tooltip("List of Unit Spawn Nodes to control")]
        [SerializeField]
        private List<UnitSpawnNode> unitSpawnNodeList = new List<UnitSpawnNode>();

        private List<UnitProfile> unitProfileList = new List<UnitProfile>();

        public override Sprite Icon { get => (systemConfigurationManager.UnitSpawnControllerInteractionPanelImage != null ? systemConfigurationManager.UnitSpawnControllerInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (systemConfigurationManager.UnitSpawnControllerNamePlateImage != null ? systemConfigurationManager.UnitSpawnControllerNamePlateImage : base.NamePlateImage); }

        public List<UnitSpawnNode> UnitSpawnNodeList { get => unitSpawnNodeList; set => unitSpawnNodeList = value; }
        public List<UnitProfile> UnitProfileList { get => unitProfileList; set => unitProfileList = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new UnitSpawnControllerComponent(interactable, this, systemGameManager);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);
            if (unitProfileNames != null) {
                foreach (string unitProfileName in unitProfileNames) {
                    UnitProfile tmpUnitProfile = systemDataFactory.GetResource<UnitProfile>(unitProfileName);
                    if (tmpUnitProfile != null) {
                        unitProfileList.Add(tmpUnitProfile);
                    } else {
                        Debug.LogError("UnitSpawnControllerComponent.SetupScriptableObjects(): COULD NOT FIND UNIT PROFILE: " + unitProfileName + " while initializing");
                    }
                }
            }

        }

    }

}