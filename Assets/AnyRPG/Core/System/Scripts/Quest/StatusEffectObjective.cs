using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class StatusEffectObjective : QuestObjective {

        [SerializeField]
        [ResourceSelector(resourceType = typeof(AbilityEffect))]
        protected string effectName = null;

        public override string ObjectiveName { get => effectName; }

        public override Type ObjectiveType {
            get {
                return typeof(StatusEffectObjective);
            }
        }

        private StatusEffectProperties statusEffect;

        public void UpdateApplyCount() {
            bool completeBefore = IsComplete;
                CurrentAmount++;
                quest.CheckCompletion();
                if (CurrentAmount <= Amount && !quest.IsAchievement) {
                    messageFeedManager.WriteMessage(string.Format("{0}: {1}/{2}", statusEffect.DisplayName, CurrentAmount, Amount));
                }
                if (completeBefore == false && IsComplete && !quest.IsAchievement) {
                    messageFeedManager.WriteMessage(string.Format("Learn {0} {1}: Objective Complete", CurrentAmount, statusEffect.DisplayName));
                }
        }

        public override void UpdateCompletionCount(bool printMessages = true) {

            base.UpdateCompletionCount(printMessages);
            bool completeBefore = IsComplete;
            if (completeBefore) {
                return;
            }
            if (playerManager.MyCharacter.CharacterStats.GetStatusEffectNode(statusEffect) != null) {
                CurrentAmount++;
                quest.CheckCompletion(true, printMessages);
                if (CurrentAmount <= Amount && !quest.IsAchievement && printMessages == true) {
                    messageFeedManager.WriteMessage(string.Format("{0}: {1}/{2}", statusEffect.DisplayName, CurrentAmount, Amount));
                }
                if (completeBefore == false && IsComplete && !quest.IsAchievement && printMessages == true) {
                    messageFeedManager.WriteMessage(string.Format("Learn {0} {1}: Objective Complete", CurrentAmount, statusEffect.DisplayName));
                }
            }
        }

        public override void OnAcceptQuest(Quest quest, bool printMessages = true) {
            base.OnAcceptQuest(quest, printMessages);
            statusEffect.OnApply += UpdateApplyCount;
            UpdateCompletionCount(printMessages);
        }

        public override void OnAbandonQuest() {
            base.OnAbandonQuest();
            statusEffect.OnApply -= UpdateApplyCount;
        }

        public override string GetUnformattedStatus() {
            string beginText = string.Empty;
            //beginText = "Use ";
            //return beginText + DisplayName + ": " + Mathf.Clamp(CurrentAmount, 0, Amount) + "/" + Amount;
            return DisplayName + ": " + Mathf.Clamp(CurrentAmount, 0, Amount) + "/" + Amount;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager, Quest quest) {
            base.SetupScriptableObjects(systemGameManager, quest);
            
            if (effectName != null && effectName != string.Empty) {
                StatusEffect tmpAbility = systemDataFactory.GetResource<AbilityEffect>(effectName) as StatusEffect;
                if (tmpAbility != null) {
                    statusEffect = tmpAbility.StatusEffectProperties;
                } else {
                    Debug.LogError("StatusEffectObjective.SetupScriptableObjects(): Could not find ability : " + effectName + " while inititalizing an ability objective for " + quest.DisplayName + ".  CHECK INSPECTOR");
                }
            }
        }

    }

}