using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[System.Serializable]
public class CollectObjective : QuestObjective {

    public void UpdateItemCount(Item item) {

        // change this with check reference to item prefab in the future
        if (SystemResourceManager.MatchResource(MyType, item.MyName)) {
            UpdateCompletionCount();
        }
    }

    public override void UpdateCompletionCount() {

        bool completeBefore = IsComplete;
        if (completeBefore) {
            return;
        }
        MyCurrentAmount = InventoryManager.MyInstance.GetItemCount(MyType);
        quest.CheckCompletion();
        if (MyCurrentAmount <= MyAmount && !quest.MyIsAchievement) {
            MessageFeedManager.MyInstance.WriteMessage(string.Format("{0}: {1}/{2}", MyType, MyCurrentAmount, MyAmount));
        }
        if (completeBefore == false && IsComplete && !quest.MyIsAchievement) {
            MessageFeedManager.MyInstance.WriteMessage(string.Format("Collect {0} {1}: Objective Complete", MyCurrentAmount, MyType));
        }
        //Debug.Log("CollectObjective Updating item count to " + MyCurrentAmount.ToString() + " for type " + MyType);
        base.UpdateCompletionCount();
    }

    public void Complete() {
        List<Item> items = InventoryManager.MyInstance.GetItems(MyType, MyAmount);
        foreach (Item item in items) {
            item.Remove();
        }
    }

    public override void OnAcceptQuest(Quest quest) {
        base.OnAcceptQuest(quest);
        SystemEventManager.MyInstance.OnItemCountChanged += UpdateItemCount;
        UpdateCompletionCount();
    }

    public override void OnAbandonQuest() {
        base.OnAbandonQuest();
        SystemEventManager.MyInstance.OnItemCountChanged -= UpdateItemCount;
    }

}

