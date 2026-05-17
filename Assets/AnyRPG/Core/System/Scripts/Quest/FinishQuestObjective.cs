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

        public override void UpdateCompletionCount(UnitController sourceUnitController, bool printMessages = true) {
            //Debug.Log("QuestQuestObjective.UpdateCompletionCount()");
            bool completeBefore = IsComplete(sourceUnitController);
            if (completeBefore) {
                //Debug.Log("QuestQuestObjective.UpdateCompletionCount() : COMPLETEBEFORE = TRUE");
                return;
            }
            if (questObjective == null) {
                //Debug.Log("QuestQuestObjective.UpdateCompletionCount(): questObjective is null");
                return;
            }
            if (questObjective.GetStatus(sourceUnitController) == "completed") {
                SetCurrentAmount(sourceUnitController, CurrentAmount(sourceUnitController) + 1);
                // i think that is supposed to be this instead to ask the quest that we are an objective for to check completion
                //questObjective.CheckCompletion(true, printMessages);
                if (CurrentAmount(sourceUnitController) <= Amount && questBase.PrintObjectiveCompletionMessages && printMessages == true && CurrentAmount(sourceUnitController) != 0) {
                    sourceUnitController.WriteMessageFeedMessage(string.Format("{0}: {1}/{2}", questObjective.DisplayName, Mathf.Clamp(CurrentAmount(sourceUnitController), 0, Amount), Amount));
                }
                if (completeBefore == false && IsComplete(sourceUnitController) && questBase.PrintObjectiveCompletionMessages && printMessages == true) {
                    sourceUnitController.WriteMessageFeedMessage(string.Format("Complete {1}: Objective Complete", CurrentAmount(sourceUnitController), questObjective.DisplayName));
                }
                questBase.CheckCompletion(sourceUnitController, true, printMessages);
            }
            base.UpdateCompletionCount(sourceUnitController, printMessages);
        }

        public override void OnAcceptQuest(UnitController sourceUnitController, QuestBase quest, bool printMessages = true) {
            base.OnAcceptQuest(sourceUnitController, quest, printMessages);
            sourceUnitController.UnitEventController.OnTurnInQuest += HandleTurnInQuest;
            UpdateCompletionCount(sourceUnitController, printMessages);
        }

        public override void OnAbandonQuest(UnitController sourceUnitController) {
            base.OnAbandonQuest(sourceUnitController);
            sourceUnitController.UnitEventController.OnTurnInQuest -= HandleTurnInQuest;
        }

        private void HandleTurnInQuest(UnitController sourceUnitController, Quest quest) {
            if (quest == questObjective) {
                UpdateCompletionCount(sourceUnitController, true);
            }
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