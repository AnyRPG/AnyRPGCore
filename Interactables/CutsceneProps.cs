using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class CutsceneProps : InteractableOptionProps {

        [Header("Cutscene")]

        [Tooltip("The name of the cutscene to play")]
        [SerializeField]
        private string cutsceneName = string.Empty;

        public override Sprite Icon { get => (SystemConfigurationManager.MyInstance.MyCutSceneInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyCutSceneInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemConfigurationManager.MyInstance.MyCutSceneNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyCutSceneNamePlateImage : base.NamePlateImage); }
        public string CutsceneName { get => cutsceneName; set => cutsceneName = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable) {
            return new CutSceneComponent(interactable, this);
        }
    }

}