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

        public void UpdateApplyCount(UnitController sourceUnitController, StatusEffectNode statusEffectNode) {
            if (statusEffectNode.StatusEffect != statusEffect) {
                return;
			}
			bool completeBefore = IsComplete(sourceUnitController);
            SetCurrentAmount(sourceUnitController, CurrentAmount(sourceUnitController) + 1);
            if (CurrentAmount(sourceUnitController) <= Amount && questBase.PrintObjectiveCompletionMessages) {
                sourceUnitController.WriteMessageFeedMessage(string.Format("Apply {0}: {1}/{2}", statusEffect.DisplayName, CurrentAmount(sourceUnitController), Amount));
            }
            if (completeBefore == false && IsComplete(sourceUnitController) && questBase.PrintObjectiveCompletionMessages) {
                sourceUnitController.WriteMessageFeedMessage(string.Format("Apply {0}: Objective Complete", statusEffect.DisplayName));
            }
            questBase.CheckCompletion(sourceUnitController);
        }

        public override void UpdateCompletionCount(UnitController sourceUnitController, bool printMessages = true) {

            base.UpdateCompletionCount(sourceUnitController, printMessages);
            bool completeBefore = IsComplete(sourceUnitController);
            if (completeBefore) {
                return;
            }
            if (sourceUnitController.CharacterStats.GetStatusEffectNode(statusEffect) != null) {
                SetCurrentAmount(sourceUnitController, CurrentAmount(sourceUnitController) + 1);
                if (CurrentAmount(sourceUnitController) <= Amount && questBase.PrintObjectiveCompletionMessages && printMessages == true) {
                    sourceUnitController.WriteMessageFeedMessage(string.Format("Apply {0}: {1}/{2}", statusEffect.DisplayName, CurrentAmount(sourceUnitController), Amount));
                }
                if (completeBefore == false && IsComplete(sourceUnitController) && questBase.PrintObjectiveCompletionMessages && printMessages == true) {
                    sourceUnitController.WriteMessageFeedMessage(string.Format("Apply {0}: Objective Complete", statusEffect.DisplayName));
                }
                questBase.CheckCompletion(sourceUnitController, true, printMessages);
            }
        }

        public override void OnAcceptQuest(UnitController sourceUnitController, QuestBase quest, bool printMessages = true) {
            base.OnAcceptQuest(sourceUnitController, quest, printMessages);
            sourceUnitController.UnitEventController.OnStatusEffectAdd += UpdateApplyCount;
            UpdateCompletionCount(sourceUnitController, printMessages);
        }

        public override void OnAbandonQuest(UnitController sourceUnitController) {
            base.OnAbandonQuest(sourceUnitController);
			sourceUnitController.UnitEventController.OnStatusEffectAdd -= UpdateApplyCount;
        }

        public override string GetUnformattedStatus(UnitController sourceUnitController) {
            string beginText = string.Empty;
            //beginText = "Use ";
            //return beginText + DisplayName + ": " + Mathf.Clamp(CurrentAmount, 0, Amount) + "/" + Amount;
            return DisplayName + ": " + Mathf.Clamp(CurrentAmount(sourceUnitController), 0, Amount) + "/" + Amount;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager, QuestBase quest) {
            base.SetupScriptableObjects(systemGameManager, quest);
            
            if (effectName != null && effectName != string.Empty) {
                StatusEffectBase tmpAbility = systemDataFactory.GetResource<AbilityEffect>(effectName) as StatusEffectBase;
                if (tmpAbility != null) {
                    statusEffect = tmpAbility.StatusEffectProperties;
                } else {
                    Debug.LogError($"StatusEffectObjective.SetupScriptableObjects(): Could not find ability : {effectName} while inititalizing an ability objective for {quest.ResourceName}.  CHECK INSPECTOR");
                }
            }
        }

    }

}