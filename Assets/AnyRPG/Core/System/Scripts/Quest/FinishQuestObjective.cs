using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class FinishQuestObjective : QuestObjective {

        [SerializeField]
        [ResourceSelector(resourceType = typeof(Quest))]
        protected string questName = null;

        public override string ObjectiveName { get => questName; }

        public override Type ObjectiveType {
            get {
                return typeof(FinishQuestObjective);
            }
        }

        private Quest questObjective;

        public override void UpdateCompletionCount(bool printMessages = true) {
            //Debug.Log("QuestQuestObjective.UpdateCompletionCount()");
            bool completeBefore = IsComplete;
            if (completeBefore) {
                //Debug.Log("QuestQuestObjective.UpdateCompletionCount() : COMPLETEBEFORE = TRUE");
                return;
            }
            if (questObjective == null) {
                //Debug.Log("QuestQuestObjective.UpdateCompletionCount(): questObjective is null");
                return;
            }
            if (questObjective.GetStatus() == "completed") {
                CurrentAmount++;
                // i think that is supposed to be this instead to ask the quest that we are an objective for to check completion
                questBase.CheckCompletion(true, printMessages);
                //questObjective.CheckCompletion(true, printMessages);
                if (CurrentAmount <= Amount && questBase.PrintObjectiveCompletionMessages && printMessages == true && CurrentAmount != 0) {
                    messageFeedManager.WriteMessage(string.Format("{0}: {1}/{2}", questObjective.DisplayName, Mathf.Clamp(CurrentAmount, 0, Amount), Amount));
                }
                if (completeBefore == false && IsComplete && questBase.PrintObjectiveCompletionMessages && printMessages == true) {
                    messageFeedManager.WriteMessage(string.Format("Complete {1}: Objective Complete", CurrentAmount, questObjective.DisplayName));
                }
            }
            base.UpdateCompletionCount(printMessages);
        }

        public override void OnAcceptQuest(QuestBase quest, bool printMessages = true) {
            base.OnAcceptQuest(quest, printMessages);
            questObjective.OnQuestStatusUpdated += HandleQuestStatusUpdated;
            UpdateCompletionCount(printMessages);
        }

        public override void OnAbandonQuest() {
            base.OnAbandonQuest();
            questObjective.OnQuestStatusUpdated -= HandleQuestStatusUpdated;
        }


        public override void SetupScriptableObjects(SystemGameManager systemGameManager, QuestBase quest) {
            //Debug.Log("QuestQuestObjective.SetupScriptableObjects()");
            base.SetupScriptableObjects(systemGameManager, quest);

            if (questName != null && questName != string.Empty) {
                questObjective = systemDataFactory.GetResource<Quest>(questName);
                if (questObjective == null) {
                    Debug.LogError("QuestQuestObjective.SetupScriptableObjects(): Could not find quest : " + questName + " while inititalizing a quest quest objective for " + quest.ResourceName + ".  CHECK INSPECTOR");
                }
            } else {
                Debug.LogError("QuestQuestObjective.SetupScriptableObjects(): questName was null while inititalizing a quest quest objective for " + quest.ResourceName + ".  CHECK INSPECTOR");
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
        }

    }


}