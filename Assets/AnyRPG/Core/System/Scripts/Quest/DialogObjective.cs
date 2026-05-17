using System;
using System.Collections;
using System.Collections.Generic;
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
        public void CheckCompletionCount(UnitController sourceUnitController, Dialog dialog) {
            bool completeBefore = IsComplete(sourceUnitController);
            if (completeBefore) {
                return;
            }

            if (SystemDataUtility.MatchResource(dialogName, dialog.ResourceName)) {
                SetCurrentAmount(sourceUnitController, CurrentAmount(sourceUnitController) + 1);
                if (CurrentAmount(sourceUnitController) <= Amount && questBase.PrintObjectiveCompletionMessages && CurrentAmount(sourceUnitController) != 0) {
                    sourceUnitController.WriteMessageFeedMessage(string.Format("{0}: {1}/{2}", DisplayName, Mathf.Clamp(CurrentAmount(sourceUnitController), 0, Amount), Amount));
                }
                if (completeBefore == false && IsComplete(sourceUnitController) && questBase.PrintObjectiveCompletionMessages) {
                    sourceUnitController.WriteMessageFeedMessage(string.Format("{0}: Objective Complete", DisplayName));
                }
                questBase.CheckCompletion(sourceUnitController);
            }
        }

        public override void UpdateCompletionCount(UnitController sourceUnitController, bool printMessages = true) {
            base.UpdateCompletionCount(sourceUnitController, printMessages);
            Dialog dialog = systemDataFactory.GetResource<Dialog>(dialogName);
            if (dialog != null && dialog.TurnedIn(sourceUnitController) == true) {
                SetCurrentAmount(sourceUnitController, CurrentAmount(sourceUnitController) + 1);
            }
        }

        public override void OnAcceptQuest(UnitController sourceUnitController, QuestBase quest, bool printMessages = true) {
            //Debug.Log("UseInteractableObjective.OnAcceptQuest()");
            base.OnAcceptQuest(sourceUnitController, quest, printMessages);

            // don't forget to remove these later
            sourceUnitController.UnitEventController.OnDialogCompleted += CheckCompletionCount;
            UpdateCompletionCount(sourceUnitController, printMessages);
        }

        public override void OnAbandonQuest(UnitController sourceUnitController) {
            //Debug.Log("UseInteractableObjective.OnAbandonQuest()");
            base.OnAbandonQuest(sourceUnitController);
            sourceUnitController.UnitEventController.OnDialogCompleted -= CheckCompletionCount;
        }

    }
}