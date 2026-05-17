using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class TradeSkillObjective : QuestObjective {

        [SerializeField]
        [ResourceSelector(resourceType = typeof(Skill))]
        protected string skillName = null;

        public override string ObjectiveName { get => skillName; }

        public override Type ObjectiveType {
            get {
                return typeof(TradeSkillObjective);
            }
        }
        private Skill skill;

        public virtual bool IsMet(UnitController sourceUnitController) {
            //Debug.Log("TradeSkillObjective.IsMet()");
            if (sourceUnitController.CharacterSkillManager.HasSkill(skill)) {
                return true;
            }
            return false;
        }

        public void UpdateCompletionCount(UnitController sourceUnitController, Skill skill) {
            if (!SystemDataUtility.MatchResource(skill.ResourceName, skillName)) {
                // some other skill than this one was learned.  no need to check.
                return;
            }
            UpdateCompletionCount(sourceUnitController);
        }

        public override void UpdateCompletionCount(UnitController sourceUnitController, bool printMessages = true) {
            //Debug.Log("TradeSkillObjective.UpdateCompletionCount()");
            bool completeBefore = IsComplete(sourceUnitController);
            if (completeBefore) {
                return;
            }
            if (sourceUnitController.CharacterSkillManager.HasSkill(skill)) {
                SetCurrentAmount(sourceUnitController, CurrentAmount(sourceUnitController) + 1);
            }
            if (CurrentAmount(sourceUnitController) <= Amount && questBase.PrintObjectiveCompletionMessages && printMessages == true && CurrentAmount(sourceUnitController) != 0) {
                sourceUnitController.WriteMessageFeedMessage(string.Format("Learn {0}: {1}/{2}", skill.DisplayName, Mathf.Clamp(CurrentAmount(sourceUnitController), 0, Amount), Amount));
            }
            if (completeBefore == false && IsComplete(sourceUnitController) && questBase.PrintObjectiveCompletionMessages && printMessages == true) {
                sourceUnitController.WriteMessageFeedMessage(string.Format("Learn {0}: Objective Complete", skill.DisplayName));
            }
            if (sourceUnitController.CharacterSkillManager.HasSkill(skill)) {
                questBase.CheckCompletion(sourceUnitController, true, printMessages);
            }
            base.UpdateCompletionCount(sourceUnitController, printMessages);
        }

        public override void OnAcceptQuest(UnitController sourceUnitController, QuestBase quest, bool printMessages = true) {
            base.OnAcceptQuest(sourceUnitController, quest, printMessages);
            sourceUnitController.UnitEventController.OnLearnSkill += UpdateCompletionCount;
            sourceUnitController.UnitEventController.OnUnLearnSkill += UpdateCompletionCount;
            UpdateCompletionCount(sourceUnitController, printMessages);
        }

        public override void OnAbandonQuest(UnitController sourceUnitController) {
            base.OnAbandonQuest(sourceUnitController);
            sourceUnitController.UnitEventController.OnLearnSkill -= UpdateCompletionCount;
            sourceUnitController.UnitEventController.OnUnLearnSkill -= UpdateCompletionCount;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager, QuestBase quest) {
            base.SetupScriptableObjects(systemGameManager, quest);
            skill = null;
            if (skillName != null && skillName != string.Empty) {
                skill = systemDataFactory.GetResource<Skill>(skillName);
                if (skill == null) {
                    Debug.LogError("TradeSkillObjective.SetupScriptableObjects(): Could not find skill : " + skillName + " while inititalizing a trade skill objective for " + quest.ResourceName + ".  CHECK INSPECTOR");
                }
            } else {
                Debug.LogError("TradeSkillObjective.SetupScriptableObjects(): Skill name was null while inititalizing a trade skill objective for " + quest.ResourceName + ".  CHECK INSPECTOR");
            }
        }


    }


}