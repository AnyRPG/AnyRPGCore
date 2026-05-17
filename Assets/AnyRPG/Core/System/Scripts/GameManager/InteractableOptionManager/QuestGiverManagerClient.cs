using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class QuestGiverManagerClient : InteractableOptionManager {

        private QuestGiverComponent questGiverComponent = null;

        public QuestGiverComponent QuestGiver { get => questGiverComponent; set => questGiverComponent = value; }

        public List<Quest> GetAvailableQuestList(UnitController sourceUnitController) {
            List<Quest> returnList = new List<Quest>();

            foreach (QuestNode questNode in questGiverComponent.QuestGiverProps.Quests) {
                if (!sourceUnitController.CharacterQuestLog.HasQuest(questNode.Quest.ResourceName)) {
                    returnList.Add(questNode.Quest);
                }
            }

            return returnList;
        }


        public void SetQuestGiver(QuestGiverComponent questGiverComponent, int componentIndex, int choiceIndex, bool notify) {
            //Debug.Log($"QuestGiverManagerClient.SetQuestGiver({questGiverComponent.Interactable.gameObject.name}, {componentIndex}, {choiceIndex})");

            this.questGiverComponent = questGiverComponent;
            
            BeginInteraction(questGiverComponent, componentIndex, choiceIndex, notify);
        }

        public void RequestAcceptQuest(UnitController sourceUnitController, Quest quest) {
            //Debug.Log($"QuestGiverManagerClient.AcceptQuestClient({sourceUnitController.gameObject.name}, {quest.ResourceName})");

            if (systemGameManager.GameMode == GameMode.Local) {
                questGiverComponent.AcceptQuest(sourceUnitController, quest);
            } else {
                networkManagerClient.RequestAcceptQuest(questGiverComponent.Interactable, componentIndex, quest);
            }
        }

        public void RequestCompleteQuest(UnitController sourceUnitController, Quest quest, QuestRewardChoices questRewardChoices) {
            if (systemGameManager.GameMode == GameMode.Local) {
                questGiverComponent.CompleteQuest(sourceUnitController, quest, questRewardChoices);
            } else {
                networkManagerClient.RequestCompleteQuest(questGiverComponent.Interactable, componentIndex, quest, questRewardChoices);
            }
        }

        public override void EndInteraction() {
            base.EndInteraction();

            questGiverComponent = null;
        }

    }

    public class QuestRewardChoices {
        public List<int> itemRewardIndexes = new List<int>();
        public List<int> factionRewardIndexes = new List<int>();
        public List<int> abilityRewardIndexes = new List<int>();
        public List<int> skillRewardIndexes = new List<int>();

        public QuestRewardChoices() {
        }
    }

}