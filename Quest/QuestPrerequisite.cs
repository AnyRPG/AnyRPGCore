using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class QuestPrerequisite : IPrerequisite {

        public event System.Action OnStatusUpdated = delegate { };


        [SerializeField]
        private string prerequisiteName = string.Empty;

        private bool prerequisiteMet = false;


        private Quest prerequisiteQuest = null;

        // does the quest need to be complete, or just in progress for this prerequisite to be met
        [SerializeField]
        private bool requireComplete = true;

        [SerializeField]
        private bool requireTurnedIn = true;

        public void HandleQuestStatusUpdated() {
            UpdateStatus();
        }

        public void UpdateStatus() {
            bool originalResult = prerequisiteMet;
            if (prerequisiteQuest == null) {
                Debug.Log("QuestPrerequisite.IsMet(): prerequisiteQuest IS NULL FOR " + prerequisiteName + "!  FIX THIS!  DO NOT COMMENT THIS LINE");
                return;
            }
            if (requireTurnedIn && prerequisiteQuest.TurnedIn == true) {
                prerequisiteMet = true;
            } else if (!requireTurnedIn && requireComplete && prerequisiteQuest.IsComplete && QuestLog.MyInstance.HasQuest(prerequisiteQuest.MyName)) {
                prerequisiteMet = true;
            } else if (!requireTurnedIn && !requireComplete && QuestLog.MyInstance.HasQuest(prerequisiteQuest.MyName)) {
                prerequisiteMet = true;
            } else {
                prerequisiteMet = false;
            }
            if (prerequisiteMet != originalResult) {
                OnStatusUpdated();
            }
        }



        public virtual bool IsMet(BaseCharacter baseCharacter) {
            //Debug.Log("QuestPrerequisite.IsMet()");
            return prerequisiteMet;
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
                Debug.LogError("QuestPrerequisite.SetupScriptableObjects(): prerequisiteName was empty while inititalizing a quest prerequisite.  CHECK INSPECTOR");
            }
            prerequisiteQuest.OnQuestStatusUpdated += HandleQuestStatusUpdated;
        }

        public void CleanupScriptableObjects() {
            prerequisiteQuest.OnQuestStatusUpdated -= HandleQuestStatusUpdated;
        }

    }

}