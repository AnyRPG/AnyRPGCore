using AnyRPG;
﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AnyRPG {

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
            if (MyCurrentAmount <= MyAmount && !quest.MyIsAchievement && MyCurrentAmount != 0) {
                MessageFeedManager.MyInstance.WriteMessage(string.Format("{0}: {1}/{2}", MyType, Mathf.Clamp(MyCurrentAmount, 0, MyAmount), MyAmount));
            }
            if (completeBefore == false && IsComplete && !quest.MyIsAchievement) {
                MessageFeedManager.MyInstance.WriteMessage(string.Format("{0}: Objective Complete", MyType));
            }
        }
    }

    public override void OnAcceptQuest(Quest quest, bool printMessages = true) {
        //Debug.Log("UseInteractableObjective.OnAcceptQuest()");
        base.OnAcceptQuest(quest, printMessages);

        // don't forget to remove these later
        SystemEventManager.MyInstance.OnDialogCompleted += CheckCompletionCount;
    }

    public override void OnAbandonQuest() {
        //Debug.Log("UseInteractableObjective.OnAbandonQuest()");
        base.OnAbandonQuest();
        SystemEventManager.MyInstance.OnDialogCompleted -= CheckCompletionCount;
    }

}
}