using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class QuestGiverProps : InteractableOptionProps {

        [Header("QuestGiver")]

        [SerializeField]
        private List<string> questGiverProfileNames = new List<string>();

        private List<QuestGiverProfile> questGiverProfiles = new List<QuestGiverProfile>();

        private List<QuestNode> quests = new List<QuestNode>();

        public List<QuestNode> Quests { get => quests; set => quests = value; }

        public override Sprite Icon { get => (SystemConfigurationManager.MyInstance.MyQuestGiverInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyQuestGiverInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemConfigurationManager.MyInstance.MyQuestGiverNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyQuestGiverNamePlateImage : base.NamePlateImage); }

        public List<string> QuestGiverProfileNames { get => questGiverProfileNames; set => questGiverProfileNames = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable) {
            return new QuestGiverComponent(interactable, this);
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            if (questGiverProfileNames != null) {
                foreach (string questGiverProfileName in questGiverProfileNames) {
                    QuestGiverProfile tmpQuestGiverProfile = SystemQuestGiverProfileManager.MyInstance.GetResource(questGiverProfileName);
                    if (tmpQuestGiverProfile != null) {
                        questGiverProfiles.Add(tmpQuestGiverProfile);
                    } else {
                        Debug.LogError("QuestgiverComponent.SetupScriptableObjects(): Could not find QuestGiverProfile : " + questGiverProfileName + " while inititalizing a questgiver.  CHECK INSPECTOR");
                    }
                }
            }
        }

    }

}