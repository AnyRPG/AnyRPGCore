using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class QuestQuestObjective : QuestObjective {

        public override Type ObjectiveType {
            get {
                return typeof(QuestQuestObjective);
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
                quest.CheckCompletion(true, printMessages);
                //questObjective.CheckCompletion(true, printMessages);
                if (CurrentAmount <= MyAmount && !questObjective.IsAchievement && printMessages == true && CurrentAmount != 0) {
                    messageFeedManager.WriteMessage(string.Format("{0}: {1}/{2}", questObjective.DisplayName, Mathf.Clamp(CurrentAmount, 0, MyAmount), MyAmount));
                }
                if (completeBefore == false && IsComplete && !questObjective.IsAchievement && printMessages == true) {
                    messageFeedManager.WriteMessage(string.Format("Complete {1}: Objective Complete", CurrentAmount, questObjective.DisplayName));
                }
            }
            base.UpdateCompletionCount(printMessages);
        }

        public override void OnAcceptQuest(Quest quest, bool printMessages = true) {
            //Debug.Log("QuestQuestObjective.OnAcceptQuest(" + quest.MyName + ")");
            base.OnAcceptQuest(quest, printMessages);
            questObjective.OnQuestStatusUpdated += HandleQuestStatusUpdated;
            UpdateCompletionCount(printMessages);
        }

        public override void OnAbandonQuest() {
            base.OnAbandonQuest();
            questObjective.OnQuestStatusUpdated -= HandleQuestStatusUpdated;
        }


        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            //Debug.Log("QuestQuestObjective.SetupScriptableObjects()");
            base.SetupScriptableObjects(systemGameManager);
            questObjective = null;
            if (MyType != null && MyType != string.Empty) {
                Quest tmpQuestObjective = systemDataFactory.GetResource<Quest>(MyType);
                if (tmpQuestObjective != null) {
                    questObjective = tmpQuestObjective;
                } else {
                    Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find quest : " + MyType + " while inititalizing a quest quest objective.  CHECK INSPECTOR");
                }
            } else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): MyType was null while inititalizing a quest quest objective.  CHECK INSPECTOR");
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
        }

    }


}