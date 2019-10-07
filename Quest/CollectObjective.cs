﻿using System;
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

    public override void UpdateCompletionCount(bool printMessages = true) {

        bool completeBefore = IsComplete;
        if (completeBefore) {
            return;
        }
        MyCurrentAmount = InventoryManager.MyInstance.GetItemCount(MyType);
        quest.CheckCompletion(true, printMessages);
        if (MyCurrentAmount <= MyAmount && !quest.MyIsAchievement && printMessages == true && MyCurrentAmount != 0) {
            MessageFeedManager.MyInstance.WriteMessage(string.Format("{0}: {1}/{2}", MyType, Mathf.Clamp(MyCurrentAmount, 0, MyAmount), MyAmount));
        }
        if (completeBefore == false && IsComplete && !quest.MyIsAchievement && printMessages == true) {
            MessageFeedManager.MyInstance.WriteMessage(string.Format("Collect {0} {1}: Objective Complete", MyCurrentAmount, MyType));
        }
        //Debug.Log("CollectObjective Updating item count to " + MyCurrentAmount.ToString() + " for type " + MyType);
        base.UpdateCompletionCount(printMessages);
    }

    public void Complete() {
        List<Item> items = InventoryManager.MyInstance.GetItems(MyType, MyAmount);
        foreach (Item item in items) {
            item.Remove();
        }
    }

    public override void OnAcceptQuest(Quest quest, bool printMessages = true) {
        base.OnAcceptQuest(quest, printMessages);
        SystemEventManager.MyInstance.OnItemCountChanged += UpdateItemCount;
        UpdateCompletionCount(printMessages);
    }

    public override void OnAbandonQuest() {
        base.OnAbandonQuest();
        SystemEventManager.MyInstance.OnItemCountChanged -= UpdateItemCount;
    }

}

