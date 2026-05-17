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

        [Tooltip("List of Unit Spawn Nodes to control")]
        [SerializeField]
        private List<string> unitSpawnNodeTagList = new List<string>();

        private List<UnitSpawnNode> completeUnitSpawnNodeList = new List<UnitSpawnNode>();
        private List<UnitProfile> unitProfileList = new List<UnitProfile>();

        // game manager references
        private PlayerManagerServer playerManagerServer = null;

        public override Sprite Icon { get => (systemConfigurationManager.UnitSpawnControllerInteractionPanelImage != null ? systemConfigurationManager.UnitSpawnControllerInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (systemConfigurationManager.UnitSpawnControllerNamePlateImage != null ? systemConfigurationManager.UnitSpawnControllerNamePlateImage : base.NamePlateImage); }

        public List<UnitSpawnNode> UnitSpawnNodeList { get => completeUnitSpawnNodeList; }
        public List<UnitProfile> UnitProfileList { get => unitProfileList; set => unitProfileList = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManagerServer = systemGameManager.PlayerManagerServer;
        }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {

            foreach (string unitSpawnNodeTag in unitSpawnNodeTagList) {
                GameObject spawnLocation = playerManagerServer.GetSceneObjectByTag(unitSpawnNodeTag, interactable.gameObject.scene);
                //Debug.Log($"UnitSpawnControllerComponent.SetupScriptableObjects(): searching for tag {unitSpawnNodeTag}");
                if (spawnLocation != null) {
                    //Debug.Log($"UnitSpawnControllerComponent.SetupScriptableObjects(): found spawn location with tag {unitSpawnNodeTag}: {spawnLocation.gameObject.name}");
                    UnitSpawnNode unitSpawnNode = spawnLocation.GetComponent<UnitSpawnNode>();
                    if (unitSpawnNode != null) {
                        completeUnitSpawnNodeList.Add(unitSpawnNode);
                    }
                }
            }

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
                        Debug.LogError($"UnitSpawnControllerComponent.SetupScriptableObjects(): COULD NOT FIND UNIT PROFILE: {unitProfileName} while initializing");
                    }
                }
            }

            completeUnitSpawnNodeList.AddRange(unitSpawnNodeList);

            

        }

    }

}