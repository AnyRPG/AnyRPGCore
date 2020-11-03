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
        private List<string> unitProfileNames = new List<string>();

        [Tooltip("List of Unit Spawn Nodes to control")]
        [SerializeField]
        private List<UnitSpawnNode> unitSpawnNodeList = new List<UnitSpawnNode>();

        public override Sprite Icon { get => (SystemConfigurationManager.MyInstance.UnitSpawnControllerInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.UnitSpawnControllerInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemConfigurationManager.MyInstance.UnitSpawnControllerNamePlateImage != null ? SystemConfigurationManager.MyInstance.UnitSpawnControllerNamePlateImage : base.NamePlateImage); }
        public List<string> UnitProfileNames { get => unitProfileNames; set => unitProfileNames = value; }
        public List<UnitSpawnNode> UnitSpawnNodeList { get => unitSpawnNodeList; set => unitSpawnNodeList = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable) {
            return new UnitSpawnControllerComponent(interactable, this);
        }
    }

}