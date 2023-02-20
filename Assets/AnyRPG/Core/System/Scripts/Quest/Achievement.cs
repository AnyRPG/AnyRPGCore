using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    
    [CreateAssetMenu(fileName = "New Achievement", menuName = "AnyRPG/Achievement")]
    public class Achievement : QuestBase {

        // game manager references
        protected AchievementLog achievementLog = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            achievementLog = systemGameManager.AchievementLog;
        }

        protected override void ProcessMarkComplete(bool printMessages) {
            base.ProcessMarkComplete(printMessages);
            if (printMessages == true) {
                messageFeedManager.WriteMessage(string.Format("Achievement: {0} Complete!", DisplayName));
            }
            playerManager.PlayLevelUpEffects(0);

            TurnedIn = true;
        }

        protected override void ResetObjectiveSaveData() {
            saveManager.ResetAchievementObjectiveSaveData(DisplayName);
        }

        protected override QuestSaveData GetSaveData() {
            return saveManager.GetAchievementSaveData(this);
        }

        protected override void SetSaveData(string QuestName, QuestSaveData questSaveData) {
            saveManager.SetAchievementSaveData(DisplayName, questSaveData);
        }

        protected override bool HasQuest() {
            return achievementLog.HasAchievement(DisplayName);
        }

    }

}