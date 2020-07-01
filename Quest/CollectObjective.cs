using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class CollectObjective : QuestObjective {

        public void UpdateItemCount(Item item) {

            // change this with check reference to item prefab in the future
            if (SystemResourceManager.MatchResource(MyType, item.DisplayName)) {
                UpdateCompletionCount();
            }
        }

        public override void UpdateCompletionCount(bool printMessages = true) {

            bool completeBefore = IsComplete;
            if (completeBefore) {
                return;
            }
            CurrentAmount = InventoryManager.MyInstance.GetItemCount(MyType);
            if (PlayerManager.MyInstance.MyCharacter.CharacterEquipmentManager.HasEquipment(MyType)) {
                CurrentAmount++;
            }

            quest.CheckCompletion(true, printMessages);
            if (CurrentAmount <= MyAmount && !quest.MyIsAchievement && printMessages == true && CurrentAmount != 0) {
                MessageFeedManager.MyInstance.WriteMessage(string.Format("{0}: {1}/{2}", DisplayName, Mathf.Clamp(CurrentAmount, 0, MyAmount), MyAmount));
            }
            if (completeBefore == false && IsComplete && !quest.MyIsAchievement && printMessages == true) {
                MessageFeedManager.MyInstance.WriteMessage(string.Format("Collect {0} {1}: Objective Complete", CurrentAmount, DisplayName));
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


}