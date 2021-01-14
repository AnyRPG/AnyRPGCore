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

        private Cutscene cutscene = null;


        public override Sprite Icon { get => (SystemConfigurationManager.MyInstance.MyCutSceneInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyCutSceneInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemConfigurationManager.MyInstance.MyCutSceneNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyCutSceneNamePlateImage : base.NamePlateImage); }
        public Cutscene Cutscene { get => cutscene; set => cutscene = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new CutSceneComponent(interactable, this);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            if (cutsceneName != null && cutsceneName != string.Empty) {
                Cutscene tmpCutscene = SystemCutsceneManager.MyInstance.GetResource(cutsceneName);
                if (tmpCutscene != null) {
                    cutscene = tmpCutscene;
                } else {
                    Debug.LogError("CutsceneProps.SetupScriptableObjects(): Could not find cutscene : " + cutsceneName + " while inititalizing.  CHECK INSPECTOR");
                }
            }
        }
    }

}