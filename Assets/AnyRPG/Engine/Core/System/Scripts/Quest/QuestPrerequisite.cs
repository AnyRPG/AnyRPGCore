using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class QuestPrerequisite : IPrerequisite {

        public event System.Action OnStatusUpdated = delegate { };


        [SerializeField]
        [ResourceSelector(resourceType = typeof(Quest))]
        private string prerequisiteName = string.Empty;

        private bool prerequisiteMet = false;


        private Quest prerequisiteQuest = null;

        // does the quest need to be complete, or just in progress for this prerequisite to be met
        [SerializeField]
        private bool requireComplete = true;

        [SerializeField]
        private bool requireTurnedIn = true;

        public void HandleQuestStatusUpdated() {
            //Debug.Log("QuestPrerequisite.HandleQuestStatusUpdated()" + prerequisiteQuest.DisplayName);
            UpdateStatus();
        }

        public void UpdateStatus(bool notify = true) {
            //Debug.Log("QuestPrerequisite.UpdateStatus(" + notify + "): " + prerequisiteQuest.DisplayName);
            bool originalResult = prerequisiteMet;
            if (prerequisiteQuest == null) {
                Debug.LogError("QuestPrerequisite.IsMet(): prerequisiteQuest IS NULL FOR " + prerequisiteName + "!  FIX THIS!  DO NOT COMMENT THIS LINE");
                return;
            }
            if (requireTurnedIn && prerequisiteQuest.TurnedIn == true) {
                //Debug.Log("QuestPrerequisite.UpdateStatus(): " + prerequisiteQuest.MyName + ";requireTurnedIn = true and prerequisiteQuest.TurnedIn == true; originalresult: " + originalResult);
                prerequisiteMet = true;
            } else if (!requireTurnedIn && requireComplete && prerequisiteQuest.IsComplete && SystemGameManager.Instance.QuestLog.HasQuest(prerequisiteQuest.DisplayName)) {
                prerequisiteMet = true;
            } else if (!requireTurnedIn && !requireComplete && SystemGameManager.Instance.QuestLog.HasQuest(prerequisiteQuest.DisplayName)) {
                prerequisiteMet = true;
            } else {
                prerequisiteMet = false;
            }
            if (prerequisiteMet != originalResult && notify == true) {
                //Debug.Log("QuestPrerequisite.UpdateStatus(): " + prerequisiteQuest.DisplayName + "; calling OnStatusUpated; originalresult: " + originalResult + "; notify: " + notify);
                OnStatusUpdated();
            } else {
                //Debug.Log("QuestPrerequisite.UpdateStatus(): " + prerequisiteQuest.MyName + "; STATUS DID NOT CHANGE; originalresult: " + originalResult + "; notify: " + notify);
            }
        }

        public virtual bool IsMet(BaseCharacter baseCharacter) {
            //Debug.Log("QuestPrerequisite.IsMet(): " + prerequisiteQuest.MyName + " returning " + prerequisiteMet);
            return prerequisiteMet;
        }

        public void SetupScriptableObjects() {
            prerequisiteQuest = null;
            if (prerequisiteName != null && prerequisiteName != string.Empty) {
                Quest tmpPrerequisiteQuest = SystemQuestManager.Instance.GetResource(prerequisiteName);
                if (tmpPrerequisiteQuest != null) {
                    //Debug.Log("QuestPrerequisite.SetupScriptableObjects(): setting: " + prerequisiteName + " while inititalizing a quest prerequisite.");
                    prerequisiteQuest = tmpPrerequisiteQuest;
                } else {
                    Debug.LogError("QuestPrerequisite.SetupScriptableObjects(): Could not find quest : " + prerequisiteName + " while inititalizing a quest prerequisite.  CHECK INSPECTOR");
                }
            } else {
                Debug.LogError("QuestPrerequisite.SetupScriptableObjects(): prerequisiteName was empty while inititalizing a quest prerequisite.  CHECK INSPECTOR");
            }
            if (prerequisiteQuest != null) {
                prerequisiteQuest.OnQuestStatusUpdated += HandleQuestStatusUpdated;
            }
        }

        public void CleanupScriptableObjects() {
            if (prerequisiteQuest != null) {
                prerequisiteQuest.OnQuestStatusUpdated -= HandleQuestStatusUpdated;
            }
        }

    }

}