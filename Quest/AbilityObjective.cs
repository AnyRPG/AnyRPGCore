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
            MyCurrentAmount++;
            quest.CheckCompletion();
            if (MyCurrentAmount <= MyAmount && !quest.MyIsAchievement && MyCurrentAmount != 0) {
                MessageFeedManager.MyInstance.WriteMessage(string.Format("{0}: {1}/{2}", MyType, Mathf.Clamp(MyCurrentAmount, 0, MyAmount), MyAmount));
            }
            if (completeBefore == false && IsComplete && !quest.MyIsAchievement) {
                MessageFeedManager.MyInstance.WriteMessage(string.Format("Learn {0} {1}: Objective Complete", MyCurrentAmount, MyType));
            }
        }

        // for casting
        public void UpdateCastCount() {
            bool completeBefore = IsComplete;
                MyCurrentAmount++;
                quest.CheckCompletion();
                if (MyCurrentAmount <= MyAmount && !quest.MyIsAchievement) {
                    MessageFeedManager.MyInstance.WriteMessage(string.Format("{0}: {1}/{2}", baseAbility.MyName, MyCurrentAmount, MyAmount));
                }
                if (completeBefore == false && IsComplete && !quest.MyIsAchievement) {
                    MessageFeedManager.MyInstance.WriteMessage(string.Format("Learn {0} {1}: Objective Complete", MyCurrentAmount, baseAbility.MyName));
                }
        }

        public override void UpdateCompletionCount(bool printMessages = true) {

            base.UpdateCompletionCount(printMessages);
            bool completeBefore = IsComplete;
            if (completeBefore) {
                return;
            }
            if (PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.HasAbility(baseAbility)) {
                MyCurrentAmount++;
                quest.CheckCompletion(true, printMessages);
                if (MyCurrentAmount <= MyAmount && !quest.MyIsAchievement && printMessages == true) {
                    MessageFeedManager.MyInstance.WriteMessage(string.Format("{0}: {1}/{2}", baseAbility.MyName, MyCurrentAmount, MyAmount));
                }
                if (completeBefore == false && IsComplete && !quest.MyIsAchievement && printMessages == true) {
                    MessageFeedManager.MyInstance.WriteMessage(string.Format("Learn {0} {1}: Objective Complete", MyCurrentAmount, baseAbility.MyName));
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