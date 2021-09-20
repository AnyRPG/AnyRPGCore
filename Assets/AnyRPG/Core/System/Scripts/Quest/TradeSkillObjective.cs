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

        public virtual bool IsMet() {
            //Debug.Log("TradeSkillObjective.IsMet()");
            if (playerManager.MyCharacter.CharacterSkillManager.HasSkill(skill)) {
                return true;
            }
            return false;
        }

        public void UpdateCompletionCount(Skill skill) {
            if (!SystemDataFactory.MatchResource(skill.DisplayName, skillName)) {
                // some other skill than this one was learned.  no need to check.
                return;
            }
            UpdateCompletionCount();
        }

        public override void UpdateCompletionCount(bool printMessages = true) {
            //Debug.Log("TradeSkillObjective.UpdateCompletionCount()");
            bool completeBefore = IsComplete;
            if (completeBefore) {
                return;
            }
            if (playerManager.MyCharacter.CharacterSkillManager.HasSkill(skill)) {
                CurrentAmount++;
                quest.CheckCompletion(true, printMessages);
            }
            if (CurrentAmount <= MyAmount && !quest.IsAchievement && printMessages == true && CurrentAmount != 0) {
                messageFeedManager.WriteMessage(string.Format("{0}: {1}/{2}", skill.DisplayName, Mathf.Clamp(CurrentAmount, 0, MyAmount), MyAmount));
            }
            if (completeBefore == false && IsComplete && !quest.IsAchievement && printMessages == true) {
                messageFeedManager.WriteMessage(string.Format("Learn {0} {1}: Objective Complete", CurrentAmount, skill.DisplayName));
            }
            base.UpdateCompletionCount(printMessages);
        }

        public override void OnAcceptQuest(Quest quest, bool printMessages = true) {
            base.OnAcceptQuest(quest, printMessages);
            systemEventManager.OnSkillListChanged += UpdateCompletionCount;
            UpdateCompletionCount(printMessages);
        }

        public override void OnAbandonQuest() {
            base.OnAbandonQuest();
            systemEventManager.OnSkillListChanged -= UpdateCompletionCount;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);
            skill = null;
            if (skillName != null && skillName != string.Empty) {
                skill = systemDataFactory.GetResource<Skill>(skillName);
            } else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability : " + skillName + " while inititalizing an ability objective.  CHECK INSPECTOR");
            }
        }


    }


}