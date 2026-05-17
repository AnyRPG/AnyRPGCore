using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class KillObjective : QuestObjective {

        [SerializeField]
        protected string targetName = null;

        public override string ObjectiveName { get => targetName; }

        public override Type ObjectiveType {
            get {
                return typeof(KillObjective);
            }
        }

        public void UpdateKillCount(UnitController sourceUnitController, UnitController killedUnitController, float creditPercent) {
            //Debug.Log("KillObjective.UpdateKillCount()");

            bool completeBefore = IsComplete(sourceUnitController);
            if (completeBefore) {
                return;
            }

            // INVESTIGATE IF STRING MATCH CAN BE REPLACED WITH TYPE.GETTYPE DIRECT MATCH
            if (killedUnitController.GetType() == Type.GetType(targetName)
                || SystemDataUtility.MatchResource(killedUnitController.BaseCharacter.CharacterName, targetName)
                || SystemDataUtility.MatchResource(killedUnitController.BaseCharacter.Faction.ResourceName, targetName)) {
                SetCurrentAmount(sourceUnitController, CurrentAmount(sourceUnitController) + 1);
                if (CurrentAmount(sourceUnitController) <= Amount && questBase.PrintObjectiveCompletionMessages && CurrentAmount(sourceUnitController) != 0) {
                    sourceUnitController.WriteMessageFeedMessage(string.Format("Kill {0}: {1}/{2}", DisplayName, Mathf.Clamp(CurrentAmount(sourceUnitController), 0, Amount), Amount));
                }
                if (completeBefore == false && IsComplete(sourceUnitController) && questBase.PrintObjectiveCompletionMessages) {
                    sourceUnitController.WriteMessageFeedMessage(string.Format("Kill {0}: Objective Complete", CurrentAmount(sourceUnitController), DisplayName));
                }
                questBase.CheckCompletion(sourceUnitController);
            }
        }

        public override void OnAcceptQuest(UnitController sourceUnitController, QuestBase quest, bool printMessages = true) {
            base.OnAcceptQuest(sourceUnitController, quest, printMessages);

            // don't forget to remove these later
            sourceUnitController.UnitEventController.OnKillEvent += UpdateKillCount;
        }

        public override void OnAbandonQuest(UnitController sourceUnitController) {
            base.OnAbandonQuest(sourceUnitController);
            if (sourceUnitController.CharacterCombat != null) {
                sourceUnitController.UnitEventController.OnKillEvent -= UpdateKillCount;
            }
        }

    }
}