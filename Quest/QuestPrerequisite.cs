using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class QuestPrerequisite : IPrerequisite {

        [SerializeField]
        private string prerequisiteName;

        private Quest prerequisiteQuest;

        // does the quest need to be complete, or just in progress for this prerequisite to be met
        [SerializeField]
        private bool requireComplete = true;

        [SerializeField]
        private bool requireTurnedIn = true;

        public virtual bool IsMet(BaseCharacter baseCharacter) {
            //Debug.Log("QuestPrerequisite.IsMet()");
            if (prerequisiteQuest == null) {
                Debug.Log("QuestPrerequisite.IsMet(): PREREQUISITE IS NULL FOR " + prerequisiteName + "!  FIX THIS!  DO NOT COMMENT THIS LINE");
                return false;
            }
            if (requireTurnedIn && prerequisiteQuest.TurnedIn == true) {
                return true;
            }
            if (!requireTurnedIn && requireComplete && prerequisiteQuest.IsComplete && QuestLog.MyInstance.HasQuest(prerequisiteQuest.MyName)) {
                return true;
            }
            if (!requireTurnedIn && !requireComplete && QuestLog.MyInstance.HasQuest(prerequisiteQuest.MyName)) {
                return true;
            }
            return false;
        }

        public void SetupScriptableObjects() {
            prerequisiteQuest = null;
            if (prerequisiteName != null && prerequisiteName != string.Empty) {
                Quest tmpPrerequisiteQuest = SystemQuestManager.MyInstance.GetResource(prerequisiteName);
                if (tmpPrerequisiteQuest != null) {
                    //Debug.Log("QuestPrerequisite.SetupScriptableObjects(): setting: " + prerequisiteName + " while inititalizing a quest prerequisite.");
                    prerequisiteQuest = tmpPrerequisiteQuest;
                } else {
                    Debug.LogError("QuestPrerequisite.SetupScriptableObjects(): Could not find quest : " + prerequisiteName + " while inititalizing a quest prerequisite.  CHECK INSPECTOR");
                }
            } else {
                Debug.LogError("QuestPrerequisite.SetupScriptableObjects(): Could not find quest : " + prerequisiteName + " while inititalizing a quest prerequisite.  CHECK INSPECTOR");
            }
        }

    }

}