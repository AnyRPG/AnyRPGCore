using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class DialogObjective : QuestObjective {

        [SerializeField]
        [ResourceSelector(resourceType = typeof(Dialog))]
        protected string dialogName = null;

        public override string ObjectiveName { get => dialogName; }

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

            if (SystemDataFactory.MatchResource(dialogName, dialog.DisplayName)) {
                CurrentAmount++;
                quest.CheckCompletion();
                if (CurrentAmount <= MyAmount && !quest.IsAchievement && CurrentAmount != 0) {
                    messageFeedManager.WriteMessage(string.Format("{0}: {1}/{2}", DisplayName, Mathf.Clamp(CurrentAmount, 0, MyAmount), MyAmount));
                }
                if (completeBefore == false && IsComplete && !quest.IsAchievement) {
                    messageFeedManager.WriteMessage(string.Format("{0}: Objective Complete", DisplayName));
                }
            }
        }

        public override void UpdateCompletionCount(bool printMessages = true) {
            base.UpdateCompletionCount(printMessages);
            Dialog dialog = systemDataFactory.GetResource<Dialog>(dialogName);
            if (dialog != null && dialog.TurnedIn == true) {
                CurrentAmount++;
            }
        }

        public override void OnAcceptQuest(Quest quest, bool printMessages = true) {
            //Debug.Log("UseInteractableObjective.OnAcceptQuest()");
            base.OnAcceptQuest(quest, printMessages);

            UpdateCompletionCount(printMessages);

            // don't forget to remove these later
            systemEventManager.OnDialogCompleted += CheckCompletionCount;
        }

        public override void OnAbandonQuest() {
            //Debug.Log("UseInteractableObjective.OnAbandonQuest()");
            base.OnAbandonQuest();
            systemEventManager.OnDialogCompleted -= CheckCompletionCount;
        }

    }
}