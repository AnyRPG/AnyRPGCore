using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class KillObjective : QuestObjective {

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
            if (character.GetType() == Type.GetType(MyType) || SystemResourceManager.MatchResource(character.CharacterName, MyType) || SystemResourceManager.MatchResource(character.Faction.DisplayName, MyType)) {
                CurrentAmount++;
                quest.CheckCompletion();
                if (CurrentAmount <= MyAmount && !quest.MyIsAchievement && CurrentAmount != 0) {
                    MessageFeedManager.MyInstance.WriteMessage(string.Format("{0}: {1}/{2}", DisplayName, Mathf.Clamp(CurrentAmount, 0, MyAmount), MyAmount));
                }
                if (completeBefore == false && IsComplete && !quest.MyIsAchievement) {
                    MessageFeedManager.MyInstance.WriteMessage(string.Format("Learn {0} {1}: Objective Complete", CurrentAmount, DisplayName));
                }

            }
        }

        public override void OnAcceptQuest(Quest quest, bool printMessages = true) {
            //Debug.Log("KillObjective.OnAcceptQuest(): MyCurrentAmount: " + MyCurrentAmount);
            base.OnAcceptQuest(quest, printMessages);

            // don't forget to remove these later
            PlayerManager.MyInstance.MyCharacter.CharacterCombat.OnKillEvent += UpdateKillCount;
        }

        public override void OnAbandonQuest() {
            base.OnAbandonQuest();
            if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyCharacter != null && PlayerManager.MyInstance.MyCharacter.CharacterCombat != null) {
                PlayerManager.MyInstance.MyCharacter.CharacterCombat.OnKillEvent -= UpdateKillCount;
            }
        }

    }
}