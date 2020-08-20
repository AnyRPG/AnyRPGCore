using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class DialogObjective : QuestObjective {

        public override Type ObjectiveType {
            get {
                return typeof(DialogObjective);
            }
        }

        // NEW (HOPEFULLY) SAFE COMPLETION CHECK CODE THAT SHOULDN'T RESULT IN RUNAWAY STACK OVERFLOW ETC
        public void CheckCompletionCount(Dialog dialog) {
            bool completeBefore = IsComplete;
            if (completeBefore) {
                return;
            }

            if (SystemResourceManager.MatchResource(MyType, dialog.DisplayName)) {
                CurrentAmount++;
                quest.CheckCompletion();
                if (CurrentAmount <= MyAmount && !quest.MyIsAchievement && CurrentAmount != 0) {
                    MessageFeedManager.MyInstance.WriteMessage(string.Format("{0}: {1}/{2}", DisplayName, Mathf.Clamp(CurrentAmount, 0, MyAmount), MyAmount));
                }
                if (completeBefore == false && IsComplete && !quest.MyIsAchievement) {
                    MessageFeedManager.MyInstance.WriteMessage(string.Format("{0}: Objective Complete", DisplayName));
                }
            }
        }

        public override void UpdateCompletionCount(bool printMessages = true) {
            base.UpdateCompletionCount(printMessages);
            Dialog dialog = SystemDialogManager.MyInstance.GetResource(MyType);
            if (dialog != null && dialog.TurnedIn == true) {
                CurrentAmount++;
            }
        }

        public override void OnAcceptQuest(Quest quest, bool printMessages = true) {
            //Debug.Log("UseInteractableObjective.OnAcceptQuest()");
            base.OnAcceptQuest(quest, printMessages);

            UpdateCompletionCount(printMessages);

            // don't forget to remove these later
            SystemEventManager.MyInstance.OnDialogCompleted += CheckCompletionCount;
        }

        public override void OnAbandonQuest() {
            //Debug.Log("UseInteractableObjective.OnAbandonQuest()");
            base.OnAbandonQuest();
            SystemEventManager.MyInstance.OnDialogCompleted -= CheckCompletionCount;
        }

    }
}