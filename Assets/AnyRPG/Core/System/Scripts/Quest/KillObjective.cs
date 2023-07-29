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

        public void UpdateKillCount(UnitController unitController, float creditPercent) {
            //Debug.Log("KillObjective.UpdateKillCount()");

            bool completeBefore = IsComplete;
            if (completeBefore) {
                return;
            }

            // INVESTIGATE IF STRING MATCH CAN BE REPLACED WITH TYPE.GETTYPE DIRECT MATCH
            if (unitController.GetType() == Type.GetType(targetName) || SystemDataUtility.MatchResource(unitController.BaseCharacter.CharacterName, targetName) || SystemDataUtility.MatchResource(unitController.BaseCharacter.Faction.ResourceName, targetName)) {
                CurrentAmount++;
                questBase.CheckCompletion();
                if (CurrentAmount <= Amount && questBase.PrintObjectiveCompletionMessages && CurrentAmount != 0) {
                    messageFeedManager.WriteMessage(string.Format("{0}: {1}/{2}", DisplayName, Mathf.Clamp(CurrentAmount, 0, Amount), Amount));
                }
                if (completeBefore == false && IsComplete && questBase.PrintObjectiveCompletionMessages) {
                    messageFeedManager.WriteMessage(string.Format("Learn {0} {1}: Objective Complete", CurrentAmount, DisplayName));
                }

            }
        }

        public override void OnAcceptQuest(QuestBase quest, bool printMessages = true) {
            base.OnAcceptQuest(quest, printMessages);

            // don't forget to remove these later
            playerManager.UnitController.UnitEventController.OnKillEvent += UpdateKillCount;
        }

        public override void OnAbandonQuest() {
            base.OnAbandonQuest();
            if (playerManager?.UnitController?.CharacterCombat != null) {
                playerManager.UnitController.UnitEventController.OnKillEvent -= UpdateKillCount;
            }
        }

    }
}