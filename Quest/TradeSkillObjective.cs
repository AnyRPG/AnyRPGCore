using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[System.Serializable]
public class TradeSkillObjective : QuestObjective {

    public virtual bool IsMet() {
        //Debug.Log("TradeSkillObjective.IsMet()");
        if (PlayerManager.MyInstance.MyCharacter.MyCharacterSkillManager.HasSkill(MyType)) {
            return true;
        }
        return false;
    }

    public void UpdateCompletionCount(Skill skill) {
        if (!SystemResourceManager.MatchResource(skill.MyName, MyType)) {
            // some other skill than this one was learned.  no need to check.
            return;
        }
        UpdateCompletionCount();
    }

    public override void UpdateCompletionCount() {
        //Debug.Log("TradeSkillObjective.UpdateCompletionCount()");
        bool completeBefore = IsComplete;
        if (completeBefore) {
            return;
        }
        if (PlayerManager.MyInstance.MyCharacter.MyCharacterSkillManager.HasSkill(MyType)) {
            MyCurrentAmount++;
            quest.CheckCompletion();
        }
        if (MyCurrentAmount <= MyAmount && !quest.MyIsAchievement) {
            MessageFeedManager.MyInstance.WriteMessage(string.Format("{0}: {1}/{2}", MyType, MyCurrentAmount, MyAmount));
        }
        if (completeBefore == false && IsComplete && !quest.MyIsAchievement) {
            MessageFeedManager.MyInstance.WriteMessage(string.Format("Learn {0} {1}: Objective Complete", MyCurrentAmount, MyType));
        }
        base.UpdateCompletionCount();
    }

    public override void OnAcceptQuest(Quest quest) {
        base.OnAcceptQuest(quest);
        SystemEventManager.MyInstance.OnSkillListChanged += UpdateCompletionCount;
        UpdateCompletionCount();
    }

    public override void OnAbandonQuest() {
        base.OnAbandonQuest();
        SystemEventManager.MyInstance.OnSkillListChanged -= UpdateCompletionCount;
    }

}

