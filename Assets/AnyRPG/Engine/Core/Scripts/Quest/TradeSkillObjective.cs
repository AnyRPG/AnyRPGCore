using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class TradeSkillObjective : QuestObjective {

        public override Type ObjectiveType {
            get {
                return typeof(TradeSkillObjective);
            }
        }
        private Skill skill;

        public virtual bool IsMet() {
            //Debug.Log("TradeSkillObjective.IsMet()");
            if (PlayerManager.MyInstance.MyCharacter.CharacterSkillManager.HasSkill(skill)) {
                return true;
            }
            return false;
        }

        public void UpdateCompletionCount(Skill skill) {
            if (!SystemResourceManager.MatchResource(skill.DisplayName, MyType)) {
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
            if (PlayerManager.MyInstance.MyCharacter.CharacterSkillManager.HasSkill(skill)) {
                CurrentAmount++;
                quest.CheckCompletion(true, printMessages);
            }
            if (CurrentAmount <= MyAmount && !quest.MyIsAchievement && printMessages == true && CurrentAmount != 0) {
                MessageFeedManager.MyInstance.WriteMessage(string.Format("{0}: {1}/{2}", skill.DisplayName, Mathf.Clamp(CurrentAmount, 0, MyAmount), MyAmount));
            }
            if (completeBefore == false && IsComplete && !quest.MyIsAchievement && printMessages == true) {
                MessageFeedManager.MyInstance.WriteMessage(string.Format("Learn {0} {1}: Objective Complete", CurrentAmount, skill.DisplayName));
            }
            base.UpdateCompletionCount(printMessages);
        }

        public override void OnAcceptQuest(Quest quest, bool printMessages = true) {
            base.OnAcceptQuest(quest, printMessages);
            SystemEventManager.MyInstance.OnSkillListChanged += UpdateCompletionCount;
            UpdateCompletionCount(printMessages);
        }

        public override void OnAbandonQuest() {
            base.OnAbandonQuest();
            SystemEventManager.MyInstance.OnSkillListChanged -= UpdateCompletionCount;
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            skill = null;
            if (MyType != null && MyType != string.Empty) {
                skill = SystemSkillManager.MyInstance.GetResource(MyType);
            } else {
                Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability : " + MyType + " while inititalizing an ability objective.  CHECK INSPECTOR");
            }
        }


    }


}