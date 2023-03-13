using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    /// <summary>
    /// Maintains a list of all achievements
    /// </summary>
    public class AchievementLog : ConfiguredMonoBehaviour {

        private Dictionary<string, Achievement> achievements = new Dictionary<string, Achievement>();

        // game manager references
        SystemDataFactory systemDataFactory = null;

        public Dictionary<string, Achievement> Achievements { get => achievements; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemDataFactory = systemGameManager.SystemDataFactory;
        }

        public void AcceptAchievement(QuestSaveData questSaveData) {

            Achievement achievement = systemDataFactory.GetResource<Achievement>(questSaveData.QuestName);
            if (achievement == null) {
                return;
            }
            if (!questSaveData.inLog) {
                return;
            }

            // change to new subscription method in quest to avoid duplicated out of date code not tracking newer objective types
            achievement.AcceptQuest(false, false);
            // gotta check here because kills and ability use are not automatically checked on accept because under normal circumstances those amounts must start at 0
            achievement.CheckCompletion(true, false);
            string keyName = SystemDataFactory.PrepareStringForMatch(achievement.ResourceName);
            achievements[keyName] = achievement;

            // just in case one quest was complete but not turned in
            //CheckCompletion();
        }

        public bool HasAchievement(string achievementName) {
            //Debug.Log("QuestLog.HasAchievement(" + questName + ")");
            string keyName = SystemDataFactory.PrepareStringForMatch(achievementName);
            if (achievements.ContainsKey(keyName)) {
                return true;
            }
            return false;
        }

        public void ClearLog() {
            List<Achievement> removeList = new List<Achievement>();
            foreach (Achievement achievement in achievements.Values) {
                removeList.Add(achievement);
            }
            foreach (Achievement oldAchievement in removeList) {
                oldAchievement.RemoveQuest(false);
            }
            achievements.Clear();
        }

    }

}