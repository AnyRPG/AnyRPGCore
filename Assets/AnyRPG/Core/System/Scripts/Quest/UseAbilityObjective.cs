using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class UseAbilityObjective : QuestObjective {

        [SerializeField]
        [ResourceSelector(resourceType = typeof(BaseAbility))]
        protected string abilityName = null;

        public override string ObjectiveName { get => abilityName; }

        public override Type ObjectiveType {
            get {
                return typeof(UseAbilityObjective);
            }
        }

        private BaseAbilityProperties baseAbility;

        public void UpdateCastCount() {
            bool completeBefore = IsComplete;
                CurrentAmount++;
                quest.CheckCompletion();
                if (CurrentAmount <= Amount && !quest.IsAchievement) {
                    messageFeedManager.WriteMessage(string.Format("{0}: {1}/{2}", baseAbility.DisplayName, CurrentAmount, Amount));
                }
                if (completeBefore == false && IsComplete && !quest.IsAchievement) {
                    messageFeedManager.WriteMessage(string.Format("Learn {0} {1}: Objective Complete", CurrentAmount, baseAbility.DisplayName));
                }
        }

        public override void OnAcceptQuest(Quest quest, bool printMessages = true) {
            base.OnAcceptQuest(quest, printMessages);
            baseAbility.OnAbilityUsed += UpdateCastCount;
        }

        public override void OnAbandonQuest() {
            base.OnAbandonQuest();
            baseAbility.OnAbilityUsed -= UpdateCastCount;
        }

        public override string GetUnformattedStatus() {
            return "Use " + DisplayName + ": " + Mathf.Clamp(CurrentAmount, 0, Amount) + "/" + Amount;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager, Quest quest) {
            base.SetupScriptableObjects(systemGameManager, quest);
            
            if (abilityName != null && abilityName != string.Empty) {
                BaseAbility tmpAbility = systemDataFactory.GetResource<BaseAbility>(abilityName);
                if (tmpAbility != null) {
                    baseAbility = tmpAbility.AbilityProperties;
                } else {
                    Debug.LogError("AbilityObjective.SetupScriptableObjects(): Could not find ability : " + abilityName + " while inititalizing an ability objective for " + quest.DisplayName + ".  CHECK INSPECTOR");
                }
            }
        }

    }

}