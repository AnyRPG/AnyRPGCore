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
            if (!SystemDataUtility.MatchResource(skill.ResourceName, skillName)) {
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
                questBase.CheckCompletion(true, printMessages);
            }
            if (CurrentAmount <= Amount && questBase.PrintObjectiveCompletionMessages && printMessages == true && CurrentAmount != 0) {
                messageFeedManager.WriteMessage(string.Format("{0}: {1}/{2}", skill.DisplayName, Mathf.Clamp(CurrentAmount, 0, Amount), Amount));
            }
            if (completeBefore == false && IsComplete && questBase.PrintObjectiveCompletionMessages && printMessages == true) {
                messageFeedManager.WriteMessage(string.Format("Learn {0} {1}: Objective Complete", CurrentAmount, skill.DisplayName));
            }
            base.UpdateCompletionCount(printMessages);
        }

        public override void OnAcceptQuest(QuestBase quest, bool printMessages = true) {
            base.OnAcceptQuest(quest, printMessages);
            systemEventManager.OnSkillListChanged += UpdateCompletionCount;
            UpdateCompletionCount(printMessages);
        }

        public override void OnAbandonQuest() {
            base.OnAbandonQuest();
            systemEventManager.OnSkillListChanged -= UpdateCompletionCount;
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