using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class CollectObjective : QuestObjective {

        public override Type ObjectiveType {
            get {
                return typeof(CollectObjective);
            }
        }

        public void UpdateItemCount(Item item) {

            // change this with check reference to item prefab in the future
            if (SystemDataFactory.MatchResource(MyType, item.DisplayName)) {
                UpdateCompletionCount();
            }
        }

        public override void UpdateCompletionCount(bool printMessages = true) {

            bool completeBefore = IsComplete;
            if (completeBefore) {
                return;
            }
            CurrentAmount = SystemGameManager.Instance.InventoryManager.GetItemCount(MyType);
            CurrentAmount += SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterEquipmentManager.GetEquipmentCount(MyType);

            quest.CheckCompletion(true, printMessages);
            if (CurrentAmount <= MyAmount && !quest.IsAchievement && printMessages == true && CurrentAmount != 0) {
                SystemGameManager.Instance.UIManager.MessageFeedManager.WriteMessage(string.Format("{0}: {1}/{2}", DisplayName, Mathf.Clamp(CurrentAmount, 0, MyAmount), MyAmount));
            }
            if (completeBefore == false && IsComplete && !quest.IsAchievement && printMessages == true) {
                SystemGameManager.Instance.UIManager.MessageFeedManager.WriteMessage(string.Format("Collect {0} {1}: Objective Complete", CurrentAmount, DisplayName));
            }
            //Debug.Log("CollectObjective Updating item count to " + MyCurrentAmount.ToString() + " for type " + MyType);
            base.UpdateCompletionCount(printMessages);
        }

        public void Complete() {
            List<Item> items = SystemGameManager.Instance.InventoryManager.GetItems(MyType, MyAmount);
            foreach (Item item in items) {
                item.Remove();
            }
        }

        public override void OnAcceptQuest(Quest quest, bool printMessages = true) {
            base.OnAcceptQuest(quest, printMessages);
            SystemGameManager.Instance.SystemEventManager.OnItemCountChanged += UpdateItemCount;
            UpdateCompletionCount(printMessages);
        }

        public override void OnAbandonQuest() {
            base.OnAbandonQuest();
            SystemGameManager.Instance.SystemEventManager.OnItemCountChanged -= UpdateItemCount;
        }

    }


}