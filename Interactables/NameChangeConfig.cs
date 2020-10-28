using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Name Change Config", menuName = "AnyRPG/Interactable/NameChangeConfig")]
    [System.Serializable]
    public class NameChangeConfig : InteractableOptionConfig {

        [SerializeField]
        private NameChangeProps interactableOptionProps = new NameChangeProps();

        public override Sprite Icon { get => (SystemConfigurationManager.MyInstance.MyNameChangeInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyNameChangeInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemConfigurationManager.MyInstance.MyNameChangeNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyNameChangeNamePlateImage : base.NamePlateImage); }

    }

}