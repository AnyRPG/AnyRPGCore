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
            if (SystemQuestManager.Instance != null) {
                if (questObjective == null) {
                    //Debug.Log("QuestQuestObjective.UpdateCompletionCount(): questObjective is null");
                    return;
                }
                if (questObjective.GetStatus() == "completed") {
                    CurrentAmount++;
                    // i think that is supposed to be this instead to ask the quest that we are an objective for to check completion
                    quest.CheckCompletion(true, printMessages);
                    //questObjective.CheckCompletion(true, printMessages);
                    if (CurrentAmount <= MyAmount && !questObjective.MyIsAchievement && printMessages == true && CurrentAmount != 0) {
                        MessageFeedManager.Instance.WriteMessage(string.Format("{0}: {1}/{2}", questObjective.DisplayName, Mathf.Clamp(CurrentAmount, 0, MyAmount), MyAmount));
                    }
                    if (completeBefore == false && IsComplete && !questObjective.MyIsAchievement && printMessages == true) {
                        MessageFeedManager.Instance.WriteMessage(string.Format("Complete {1}: Objective Complete", CurrentAmount, questObjective.DisplayName));
                    }
                }
            }
            base.UpdateCompletionCount(printMessages);
        }

        public override void OnAcceptQuest(Quest quest, bool printMessages = true) {
            //Debug.Log("QuestQuestObjective.OnAcceptQuest(" + quest.MyName + ")");
            base.OnAcceptQuest(quest, printMessages);
            // not needed anymore ?
            //SystemGameManager.Instance.EventManager.OnQuestStatusUpdated += HandleQuestStatusUpdated;
            questObjective.OnQuestStatusUpdated += HandleQuestStatusUpdated;
            UpdateCompletionCount(printMessages);
        }

        public override void OnAbandonQuest() {
            base.OnAbandonQuest();
            // not needed anymore ?
            //SystemGameManager.Instance.EventManager.OnQuestStatusUpdated -= HandleQuestStatusUpdated;
            questObjective.OnQuestStatusUpdated -= HandleQuestStatusUpdated;
        }


        public override void SetupScriptableObjects() {
            //Debug.Log("QuestQuestObjective.SetupScriptableObjects()");
            base.SetupScriptableObjects();
            questObjective = null;
            if (MyType != null && MyType != string.Empty) {
                Quest tmpQuestObjective = SystemQuestManager.Instance.GetResource(MyType);
                if (tmpQuestObjective != null) {
                    questObjective = tmpQuestObjective;
                } else {
                    Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find quest : " + MyType + " while inititalizing a quest quest objective.  CHECK INSPECTOR");
                }
            } else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): MyType was null while inititalizing a quest quest objective.  CHECK INSPECTOR");
            }
        }

        public void CleanupScriptableObjects () {

        }
    }


}