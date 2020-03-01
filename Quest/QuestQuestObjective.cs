using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class QuestQuestObjective : QuestObjective {

        private Quest questObjective;

        public override void UpdateCompletionCount(bool printMessages = true) {
            //Debug.Log("QuestQuestObjective.UpdateCompletionCount()");
            bool completeBefore = IsComplete;
            if (completeBefore) {
                //Debug.Log("QuestQuestObjective.UpdateCompletionCount() : COMPLETEBEFORE = TRUE");
                return;
            }
            if (SystemQuestManager.MyInstance != null) {
                if (questObjective == null) {
                    //Debug.Log("QuestQuestObjective.UpdateCompletionCount(): questObjective is null");
                    return;
                }
                if (questObjective.GetStatus() == "completed") {
                    MyCurrentAmount++;
                    questObjective.CheckCompletion(true, printMessages);
                    if (MyCurrentAmount <= MyAmount && !questObjective.MyIsAchievement && printMessages == true && MyCurrentAmount != 0) {
                        MessageFeedManager.MyInstance.WriteMessage(string.Format("{0}: {1}/{2}", questObjective.MyName, Mathf.Clamp(MyCurrentAmount, 0, MyAmount), MyAmount));
                    }
                    if (completeBefore == false && IsComplete && !questObjective.MyIsAchievement && printMessages == true) {
                        MessageFeedManager.MyInstance.WriteMessage(string.Format("Complete {1}: Objective Complete", MyCurrentAmount, questObjective.MyName));
                    }
                }
            }
            base.UpdateCompletionCount(printMessages);
        }

        public override void OnAcceptQuest(Quest quest, bool printMessages = true) {
            //Debug.Log("QuestQuestObjective.OnAcceptQuest(" + quest.MyName + ")");
            base.OnAcceptQuest(quest, printMessages);
            SystemEventManager.MyInstance.OnQuestStatusUpdated += HandleQuestStatusUpdated;
            UpdateCompletionCount(printMessages);
        }

        public override void OnAbandonQuest() {
            base.OnAbandonQuest();
            SystemEventManager.MyInstance.OnQuestStatusUpdated -= HandleQuestStatusUpdated;
        }


        public override void SetupScriptableObjects() {
            //Debug.Log("QuestQuestObjective.SetupScriptableObjects()");
            base.SetupScriptableObjects();
            questObjective = null;
            if (MyType != null && MyType != string.Empty) {
                Quest tmpQuestObjective = SystemQuestManager.MyInstance.GetResource(MyType);
                if (tmpQuestObjective != null) {
                    questObjective = tmpQuestObjective;
                } else {
                    Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find quest : " + MyType + " while inititalizing a quest quest objective.  CHECK INSPECTOR");
                }
            } else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): MyType was null while inititalizing a quest quest objective.  CHECK INSPECTOR");
            }
        }
    }


}