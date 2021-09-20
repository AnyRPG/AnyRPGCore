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

        public override Type ObjectiveType {
            get {
                return typeof(KillObjective);
            }
        }

        public void UpdateKillCount(BaseCharacter character, float creditPercent) {
            //Debug.Log("KillObjective.UpdateKillCount()");

            bool completeBefore = IsComplete;
            if (completeBefore) {
                return;
            }

            // INVESTIGATE IF STRING MATCH CAN BE REPLACED WITH TYPE.GETTYPE DIRECT MATCH
            if (character.GetType() == Type.GetType(targetName) || SystemDataFactory.MatchResource(character.CharacterName, targetName) || SystemDataFactory.MatchResource(character.Faction.DisplayName, targetName)) {
                CurrentAmount++;
                quest.CheckCompletion();
                if (CurrentAmount <= MyAmount && !quest.IsAchievement && CurrentAmount != 0) {
                    messageFeedManager.WriteMessage(string.Format("{0}: {1}/{2}", DisplayName, Mathf.Clamp(CurrentAmount, 0, MyAmount), MyAmount));
                }
                if (completeBefore == false && IsComplete && !quest.IsAchievement) {
                    messageFeedManager.WriteMessage(string.Format("Learn {0} {1}: Objective Complete", CurrentAmount, DisplayName));
                }

            }
        }

        public override void OnAcceptQuest(Quest quest, bool printMessages = true) {
            //Debug.Log("KillObjective.OnAcceptQuest(): MyCurrentAmount: " + MyCurrentAmount);
            base.OnAcceptQuest(quest, printMessages);

            // don't forget to remove these later
            playerManager.MyCharacter.CharacterCombat.OnKillEvent += UpdateKillCount;
        }

        public override void OnAbandonQuest() {
            base.OnAbandonQuest();
            if (playerManager?.MyCharacter?.CharacterCombat != null) {
                playerManager.MyCharacter.CharacterCombat.OnKillEvent -= UpdateKillCount;
            }
        }


    }
}