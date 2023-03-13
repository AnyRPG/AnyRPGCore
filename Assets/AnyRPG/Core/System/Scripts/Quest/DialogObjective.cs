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

            if (SystemDataFactory.MatchResource(dialogName, dialog.ResourceName)) {
                CurrentAmount++;
                questBase.CheckCompletion();
                if (CurrentAmount <= Amount && questBase.PrintObjectiveCompletionMessages && CurrentAmount != 0) {
                    messageFeedManager.WriteMessage(string.Format("{0}: {1}/{2}", DisplayName, Mathf.Clamp(CurrentAmount, 0, Amount), Amount));
                }
                if (completeBefore == false && IsComplete && questBase.PrintObjectiveCompletionMessages) {
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

        public override void OnAcceptQuest(QuestBase quest, bool printMessages = true) {
            //Debug.Log("UseInteractableObjective.OnAcceptQuest()");
            base.OnAcceptQuest(quest, printMessages);

            // don't forget to remove these later
            systemEventManager.OnDialogCompleted += CheckCompletionCount;
            UpdateCompletionCount(printMessages);
        }

        public override void OnAbandonQuest() {
            //Debug.Log("UseInteractableObjective.OnAbandonQuest()");
            base.OnAbandonQuest();
            systemEventManager.OnDialogCompleted -= CheckCompletionCount;
        }

    }
}