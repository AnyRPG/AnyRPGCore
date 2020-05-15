using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class AbilityObjective : QuestObjective {

        // if true, you must use the ability, otherwise, just learning it is good enough
        [SerializeField]
        private bool requireUse = false;

        private BaseAbility baseAbility;

        // for learning
        public void UpdateCompletionCount() {
            Debug.Log("AbilityObjective.UpdateCompletionCount(" + (baseAbility == null ? "null" : baseAbility.MyName) + ")");
            bool completeBefore = IsComplete;
            if (completeBefore) {
                return;
            }
            CurrentAmount++;
            quest.CheckCompletion();
            if (CurrentAmount <= MyAmount && !quest.MyIsAchievement && CurrentAmount != 0) {
                MessageFeedManager.MyInstance.WriteMessage(string.Format("{0}: {1}/{2}", DisplayName, Mathf.Clamp(CurrentAmount, 0, MyAmount), MyAmount));
            }
            if (completeBefore == false && IsComplete && !quest.MyIsAchievement) {
                MessageFeedManager.MyInstance.WriteMessage(string.Format("Learn {0} {1}: Objective Complete", CurrentAmount, DisplayName));
            }
        }

        // for casting
        public void UpdateCastCount() {
            bool completeBefore = IsComplete;
                CurrentAmount++;
                quest.CheckCompletion();
                if (CurrentAmount <= MyAmount && !quest.MyIsAchievement) {
                    MessageFeedManager.MyInstance.WriteMessage(string.Format("{0}: {1}/{2}", baseAbility.MyName, CurrentAmount, MyAmount));
                }
                if (completeBefore == false && IsComplete && !quest.MyIsAchievement) {
                    MessageFeedManager.MyInstance.WriteMessage(string.Format("Learn {0} {1}: Objective Complete", CurrentAmount, baseAbility.MyName));
                }
        }

        public override void UpdateCompletionCount(bool printMessages = true) {

            base.UpdateCompletionCount(printMessages);
            bool completeBefore = IsComplete;
            if (completeBefore) {
                return;
            }
            if (PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.HasAbility(baseAbility)) {
                CurrentAmount++;
                quest.CheckCompletion(true, printMessages);
                if (CurrentAmount <= MyAmount && !quest.MyIsAchievement && printMessages == true) {
                    MessageFeedManager.MyInstance.WriteMessage(string.Format("{0}: {1}/{2}", baseAbility.MyName, CurrentAmount, MyAmount));
                }
                if (completeBefore == false && IsComplete && !quest.MyIsAchievement && printMessages == true) {
                    MessageFeedManager.MyInstance.WriteMessage(string.Format("Learn {0} {1}: Objective Complete", CurrentAmount, baseAbility.MyName));
                }
            }
        }

        public bool MyRequireUse { get => requireUse; set => requireUse = value; }

        public override void OnAcceptQuest(Quest quest, bool printMessages = true) {
            //Debug.Log("AbilityObjective.OnAcceptQuest(): " + MyType);
            base.OnAcceptQuest(quest, printMessages);
            if (requireUse == true) {
                baseAbility.OnAbilityUsed += UpdateCastCount;
            } else {
                baseAbility.OnAbilityLearn += UpdateCompletionCount;
                UpdateCompletionCount(printMessages);
            }
        }

        public override void OnAbandonQuest() {
            base.OnAbandonQuest();
            baseAbility.OnAbilityLearn -= UpdateCompletionCount;
            if (requireUse == true) {
                baseAbility.OnAbilityUsed -= UpdateCastCount;
            }
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            baseAbility = null;
            if (MyType != null && MyType != string.Empty) {
                baseAbility = SystemAbilityManager.MyInstance.GetResource(MyType);
            } else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability : " + MyType + " while inititalizing an ability objective.  CHECK INSPECTOR");
            }
        }

    }


}