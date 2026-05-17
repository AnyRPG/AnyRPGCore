using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class LearnAbilityObjective : QuestObjective {

        [SerializeField]
        [ResourceSelector(resourceType = typeof(Ability))]
        protected string abilityName = null;

        public override string ObjectiveName { get => abilityName; }

        public override Type ObjectiveType {
            get {
                return typeof(LearnAbilityObjective);
            }
        }

        private AbilityProperties baseAbility;

        // for learning
        public void UpdateLearnedCompletionCount(UnitController sourceUnitController) {
            //Debug.Log("AbilityObjective.UpdateCompletionCount(" + (baseAbility == null ? "null" : baseAbility.DisplayName) + ")");
            bool completeBefore = IsComplete(sourceUnitController);
            if (completeBefore) {
                return;
            }
            SetCurrentAmount(sourceUnitController, CurrentAmount(sourceUnitController) + 1);
            if (CurrentAmount(sourceUnitController) <= Amount && questBase.PrintObjectiveCompletionMessages && CurrentAmount(sourceUnitController) != 0) {
                sourceUnitController.WriteMessageFeedMessage(string.Format("Learn {0}: {1}/{2}", DisplayName, Mathf.Clamp(CurrentAmount(sourceUnitController), 0, Amount), Amount));
            }
            if (completeBefore == false && IsComplete(sourceUnitController) && questBase.PrintObjectiveCompletionMessages) {
                sourceUnitController.WriteMessageFeedMessage(string.Format("Learn {1}: Objective Complete", CurrentAmount(sourceUnitController), DisplayName));
            }
            questBase.CheckCompletion(sourceUnitController);
        }

        public override void UpdateCompletionCount(UnitController sourceUnitController, bool printMessages = true) {

            base.UpdateCompletionCount(sourceUnitController, printMessages);
            bool completeBefore = IsComplete(sourceUnitController);
            if (completeBefore) {
                return;
            }
            if (sourceUnitController.CharacterAbilityManager.HasAbility(baseAbility)) {
                SetCurrentAmount(sourceUnitController, CurrentAmount(sourceUnitController)+1);
                questBase.CheckCompletion(sourceUnitController, true, printMessages);
                if (CurrentAmount(sourceUnitController) <= Amount && questBase.PrintObjectiveCompletionMessages && printMessages == true) {
                    sourceUnitController.WriteMessageFeedMessage(string.Format("Learn {0}: {1}/{2}", baseAbility.DisplayName, CurrentAmount(sourceUnitController), Amount));
                }
                if (completeBefore == false && IsComplete(sourceUnitController) && questBase.PrintObjectiveCompletionMessages && printMessages == true) {
                    sourceUnitController.WriteMessageFeedMessage(string.Format("Learn {1}: Objective Complete", CurrentAmount(sourceUnitController), baseAbility.DisplayName));
                }
            }
        }

        public override void OnAcceptQuest(UnitController sourceUnitController, QuestBase questBase, bool printMessages = true) {
            base.OnAcceptQuest(sourceUnitController, questBase, printMessages);
            sourceUnitController.UnitEventController.OnLearnAbility += HandleLearnAbility;
            UpdateCompletionCount(sourceUnitController, printMessages);
        }

        public override void OnAbandonQuest(UnitController sourceUnitController) {
            base.OnAbandonQuest(sourceUnitController);
            sourceUnitController.UnitEventController.OnLearnAbility -= HandleLearnAbility;
        }

        private void HandleLearnAbility(UnitController sourceUnitController, AbilityProperties properties) {
            if (properties == baseAbility) {
                UpdateLearnedCompletionCount(sourceUnitController);
            }
        }

        public override string GetUnformattedStatus(UnitController sourceUnitController) {
            return "Learn " + DisplayName + ": " + Mathf.Clamp(CurrentAmount(sourceUnitController), 0, Amount) + "/" + Amount;
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