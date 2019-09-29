using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[System.Serializable]
public class AbilityObjective : QuestObjective {

    // if true, you must use the ability, otherwise, just learning it is good enough
    [SerializeField]
    private bool requireUse = false;

    // for learning
    public void UpdateCompletionCount(string baseAbility) {
        if (!SystemResourceManager.MatchResource(baseAbility, MyType)) {
            // some other ability than this one was used.  no need to check.
            return;
        }
        bool completeBefore = IsComplete;
        if (completeBefore) {
            return;
        }
            MyCurrentAmount++;
            quest.CheckCompletion();
            if (MyCurrentAmount <= MyAmount && !quest.MyIsAchievement) {
                MessageFeedManager.MyInstance.WriteMessage(string.Format("{0}: {1}/{2}", MyType, MyCurrentAmount, MyAmount));
            }
            if (completeBefore == false && IsComplete && !quest.MyIsAchievement) {
                MessageFeedManager.MyInstance.WriteMessage(string.Format("Learn {0} {1}: Objective Complete", MyCurrentAmount, MyType));
            }
    }

    // for casting
    public void UpdateCastCount(BaseAbility baseAbility) {
        bool completeBefore = IsComplete;
        if (SystemResourceManager.MatchResource(baseAbility.MyName, MyType)) {
            MyCurrentAmount++;
            quest.CheckCompletion();
            if (MyCurrentAmount <= MyAmount && !quest.MyIsAchievement) {
                MessageFeedManager.MyInstance.WriteMessage(string.Format("{0}: {1}/{2}", MyType, MyCurrentAmount, MyAmount));
            }
            if (completeBefore == false && IsComplete && !quest.MyIsAchievement) {
                MessageFeedManager.MyInstance.WriteMessage(string.Format("Learn {0} {1}: Objective Complete", MyCurrentAmount, MyType));
            }
        }
    }

    public override void UpdateCompletionCount() {

        base.UpdateCompletionCount();
        bool completeBefore = IsComplete;
        if (completeBefore) {
            return;
        }
        if (PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.HasAbility(MyType)) {
            MyCurrentAmount++;
            quest.CheckCompletion();
            if (MyCurrentAmount <= MyAmount && !quest.MyIsAchievement) {
                MessageFeedManager.MyInstance.WriteMessage(string.Format("{0}: {1}/{2}", MyType, MyCurrentAmount, MyAmount));
            }
            if (completeBefore == false && IsComplete && !quest.MyIsAchievement) {
                MessageFeedManager.MyInstance.WriteMessage(string.Format("Learn {0} {1}: Objective Complete", MyCurrentAmount, MyType));
            }
        }
    }

    public bool MyRequireUse { get => requireUse; set => requireUse = value; }

    public override void OnAcceptQuest(Quest quest) {
        //Debug.Log("AbilityObjective.OnAcceptQuest(): " + MyType);
        base.OnAcceptQuest(quest);
        if (requireUse == true) {
            SystemEventManager.MyInstance.OnAbilityUsed += UpdateCastCount;
        } else {
            SystemEventManager.MyInstance.OnAbilityListChanged += UpdateCompletionCount;
            UpdateCompletionCount();
        }
    }

    public override void OnAbandonQuest() {
        base.OnAbandonQuest();
        SystemEventManager.MyInstance.OnAbilityListChanged -= UpdateCompletionCount;
        if (requireUse == true) {
            SystemEventManager.MyInstance.OnAbilityUsed -= UpdateCastCount;
        }
    }

}

