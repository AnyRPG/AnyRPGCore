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

    public override void UpdateCompletionCount(bool printMessages = true) {
        //Debug.Log("TradeSkillObjective.UpdateCompletionCount()");
        bool completeBefore = IsComplete;
        if (completeBefore) {
            return;
        }
        if (PlayerManager.MyInstance.MyCharacter.MyCharacterSkillManager.HasSkill(MyType)) {
            MyCurrentAmount++;
            quest.CheckCompletion(true, printMessages);
        }
        if (MyCurrentAmount <= MyAmount && !quest.MyIsAchievement && printMessages == true && MyCurrentAmount != 0) {
            MessageFeedManager.MyInstance.WriteMessage(string.Format("{0}: {1}/{2}", MyType, Mathf.Clamp(MyCurrentAmount, 0, MyAmount), MyAmount));
        }
        if (completeBefore == false && IsComplete && !quest.MyIsAchievement && printMessages == true) {
            MessageFeedManager.MyInstance.WriteMessage(string.Format("Learn {0} {1}: Objective Complete", MyCurrentAmount, MyType));
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

}

