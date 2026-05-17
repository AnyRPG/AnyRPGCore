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
        [ResourceSelector(resourceType = typeof(Ability))]
        protected string abilityName = null;

        public override string ObjectiveName { get => abilityName; }

        public override Type ObjectiveType {
            get {
                return typeof(UseAbilityObjective);
            }
        }

        private AbilityProperties baseAbility;



        public void UpdateCastCount(UnitController sourceUnitController) {
            bool completeBefore = IsComplete(sourceUnitController);
            SetCurrentAmount(sourceUnitController, CurrentAmount(sourceUnitController) + 1);
            if (CurrentAmount(sourceUnitController) <= Amount && questBase.PrintObjectiveCompletionMessages) {
                sourceUnitController.WriteMessageFeedMessage(string.Format("Use {0}: {1}/{2}", baseAbility.DisplayName, CurrentAmount(sourceUnitController), Amount));
            }
            if (completeBefore == false && IsComplete(sourceUnitController) && questBase.PrintObjectiveCompletionMessages) {
                sourceUnitController.WriteMessageFeedMessage(string.Format("Use {0}: Objective Complete", baseAbility.DisplayName));
            }
            questBase.CheckCompletion(sourceUnitController);
        }

        private void HandlePerformAbility(UnitController sourceUnitController, AbilityProperties properties) {
            if (properties == baseAbility) {
                UpdateCastCount(sourceUnitController);
            }
        }

        public override void OnAcceptQuest(UnitController sourceUnitController, QuestBase quest, bool printMessages = true) {
            base.OnAcceptQuest(sourceUnitController, quest, printMessages);
            sourceUnitController.UnitEventController.OnPerformAbility += HandlePerformAbility;
        }

        public override void OnAbandonQuest(UnitController sourceUnitController) {
            base.OnAbandonQuest(sourceUnitController);
            sourceUnitController.UnitEventController.OnPerformAbility -= HandlePerformAbility;
        }

        public override string GetUnformattedStatus(UnitController sourceUnitController) {
            return "Use " + DisplayName + ": " + Mathf.Clamp(CurrentAmount(sourceUnitController), 0, Amount) + "/" + Amount;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager, QuestBase quest) {
            base.SetupScriptableObjects(systemGameManager, quest);
            
            if (abilityName != null && abilityName != string.Empty) {
                Ability tmpAbility = systemDataFactory.GetResource<Ability>(abilityName);
                if (tmpAbility != null) {
                    baseAbility = tmpAbility.AbilityProperties;
                } else {
                    Debug.LogError("AbilityObjective.SetupScriptableObjects(): Could not find ability : " + abilityName + " while inititalizing an ability objective for " + quest.ResourceName + ".  CHECK INSPECTOR");
                }
            }
        }

    }

}