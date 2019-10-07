using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[System.Serializable]
public class QuestQuestObjective : QuestObjective {

    public override void UpdateCompletionCount(bool printMessages = true) {
        bool completeBefore = IsComplete;
        if (completeBefore) {
            return;
        }
        if (SystemQuestManager.MyInstance != null) {
            if (MyType != null && MyType == string.Empty) {
                Debug.LogError("MyType is null or empty on QuestQuestObjective");
                return;
            }
            Quest _quest = SystemQuestManager.MyInstance.GetResource(MyType);
            if (_quest == null) {
                Debug.LogError("No quest returned for: " + MyType);
                return;
            }
            if (_quest.GetStatus() == "completed") {
                MyCurrentAmount++;
                quest.CheckCompletion(true, printMessages);
                if (MyCurrentAmount <= MyAmount && !quest.MyIsAchievement && printMessages == true && MyCurrentAmount != 0) {
                    MessageFeedManager.MyInstance.WriteMessage(string.Format("{0}: {1}/{2}", MyType, Mathf.Clamp(MyCurrentAmount, 0, MyAmount), MyAmount));
                }
                if (completeBefore == false && IsComplete && !quest.MyIsAchievement && printMessages == true) {
                    MessageFeedManager.MyInstance.WriteMessage(string.Format("Complete {1}: Objective Complete", MyCurrentAmount, MyType));
                }
            }
        }
        base.UpdateCompletionCount(printMessages);
    }

    public override void OnAcceptQuest(Quest quest, bool printMessages = true) {
        base.OnAcceptQuest(quest, printMessages);
        SystemEventManager.MyInstance.OnQuestStatusUpdated += HandleQuestStatusUpdated;
        UpdateCompletionCount(printMessages);
    }

    public override void OnAbandonQuest() {
        base.OnAbandonQuest();
        SystemEventManager.MyInstance.OnQuestStatusUpdated -= HandleQuestStatusUpdated;
    }

}

