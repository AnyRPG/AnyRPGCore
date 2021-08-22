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
        [ResourceSelector(resourceType = typeof(Cutscene))]
        private string cutsceneName = string.Empty;

        private Cutscene cutscene = null;


        public override Sprite Icon { get => (systemConfigurationManager.CutSceneInteractionPanelImage != null ? systemConfigurationManager.CutSceneInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (systemConfigurationManager.CutSceneNamePlateImage != null ? systemConfigurationManager.CutSceneNamePlateImage : base.NamePlateImage); }
        public Cutscene Cutscene { get => cutscene; set => cutscene = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new CutSceneComponent(interactable, this, systemGameManager);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);
            if (cutsceneName != null && cutsceneName != string.Empty) {
                Cutscene tmpCutscene = systemDataFactory.GetResource<Cutscene>(cutsceneName);
                if (tmpCutscene != null) {
                    cutscene = tmpCutscene;
                } else {
                    Debug.LogError("CutsceneProps.SetupScriptableObjects(): Could not find cutscene : " + cutsceneName + " while inititalizing.  CHECK INSPECTOR");
                }
            }
        }
    }

}