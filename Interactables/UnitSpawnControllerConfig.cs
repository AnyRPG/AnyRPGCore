using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Unit Spawn Controller Config", menuName = "AnyRPG/Interactable/UnitSpawnControllerConfig")]
    [System.Serializable]
    public class UnitSpawnControllerConfig : InteractableOptionConfig {

        [SerializeField]
        private UnitSpawnControllerProps interactableOptionProps = new UnitSpawnControllerProps();

        [Header("Unit Spawn Controller")]

        [Tooltip("The names of the unit profiles that will be available to spawn with this controller")]
        [SerializeField]
        private List<string> unitProfileNames = new List<string>();

        public override Sprite Icon { get => (SystemConfigurationManager.MyInstance.MyUnitSpawnControllerInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyUnitSpawnControllerInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemConfigurationManager.MyInstance.MyUnitSpawnControllerNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyUnitSpawnControllerNamePlateImage : base.NamePlateImage); }
        public List<string> UnitProfileNames { get => unitProfileNames; set => unitProfileNames = value; }
        public UnitSpawnControllerProps InteractableOptionProps { get => interactableOptionProps; set => interactableOptionProps = value; }
    }

}