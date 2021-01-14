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

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new QuestGiverComponent(interactable, this);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
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

            foreach (QuestGiverProfile questGiverProfile in questGiverProfiles) {
                if (questGiverProfile != null && questGiverProfile.MyQuests != null) {
                    foreach (QuestNode questNode in questGiverProfile.MyQuests) {
                        //Debug.Log(gameObject.name + ".SetupScriptableObjects(): Adding quest: " + questNode.MyQuest.MyName);
                        quests.Add(questNode);
                    }
                }
            }

        }

    }

}