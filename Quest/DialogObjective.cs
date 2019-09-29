using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


[System.Serializable]
public class DialogObjective : QuestObjective {

    // NEW (HOPEFULLY) SAFE COMPLETION CHECK CODE THAT SHOULDN'T RESULT IN RUNAWAY STACK OVERFLOW ETC
    public void CheckCompletionCount(Dialog dialog) {
        bool completeBefore = IsComplete;
        if (completeBefore) {
            return;
        }

        if (SystemResourceManager.MatchResource(MyType, dialog.MyName)) {
            MyCurrentAmount++;
            quest.CheckCompletion();
            if (MyCurrentAmount <= MyAmount && !quest.MyIsAchievement) {
                MessageFeedManager.MyInstance.WriteMessage(string.Format("{0}: {1}/{2}", MyType, MyCurrentAmount, MyAmount));
            }
            if (completeBefore == false && IsComplete && !quest.MyIsAchievement) {
                MessageFeedManager.MyInstance.WriteMessage(string.Format("{0}: Objective Complete", MyType));
            }
        }
    }

    public override void OnAcceptQuest(Quest quest) {
        //Debug.Log("UseInteractableObjective.OnAcceptQuest()");
        base.OnAcceptQuest(quest);

        // don't forget to remove these later
        SystemEventManager.MyInstance.OnDialogCompleted += CheckCompletionCount;
    }

    public override void OnAbandonQuest() {
        //Debug.Log("UseInteractableObjective.OnAbandonQuest()");
        base.OnAbandonQuest();
        SystemEventManager.MyInstance.OnDialogCompleted -= CheckCompletionCount;
    }

}