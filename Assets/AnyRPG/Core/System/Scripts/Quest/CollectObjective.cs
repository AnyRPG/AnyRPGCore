using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class CollectObjective : QuestObjective {

        [SerializeField]
        [ResourceSelector(resourceType = typeof(Item))]
        protected string itemName = null;

        [Tooltip("If true, the name can be partially matched")]
        [SerializeField]
        protected bool partialMatch = false;

        // game manager references
        //protected InventoryManager inventoryManager = null;
        

        public override string ObjectiveName { get => itemName; }

        public override Type ObjectiveType {
            get {
                return typeof(CollectObjective);
            }
        }

        public void UpdateItemCount(Item item) {

            // change this with check reference to item prefab in the future
            if (SystemDataUtility.MatchResource(item.ResourceName, itemName, partialMatch)) {
                UpdateCompletionCount();
            }
        }

        public override void UpdateCompletionCount(bool printMessages = true) {

            bool completeBefore = IsComplete;
            if (completeBefore) {
                return;
            }
            CurrentAmount = playerManager.MyCharacter.CharacterInventoryManager.GetItemCount(itemName, partialMatch);
            CurrentAmount += playerManager.MyCharacter.CharacterEquipmentManager.GetEquipmentCount(itemName, partialMatch);

            questBase.CheckCompletion(true, printMessages);
            if (CurrentAmount <= Amount && questBase.PrintObjectiveCompletionMessages && printMessages == true && CurrentAmount != 0) {
                messageFeedManager.WriteMessage(string.Format("{0}: {1}/{2}", DisplayName, Mathf.Clamp(CurrentAmount, 0, Amount), Amount));
            }
            if (completeBefore == false && IsComplete && questBase.PrintObjectiveCompletionMessages && printMessages == true) {
                messageFeedManager.WriteMessage(string.Format("Collect {0} {1}: Objective Complete", CurrentAmount, DisplayName));
            }
            base.UpdateCompletionCount(printMessages);
        }

        public void Complete() {
            List<Item> items = playerManager.MyCharacter.CharacterInventoryManager.GetItems(itemName, Amount);
            foreach (Item item in items) {
                item.Remove();
            }
        }

        public override void OnAcceptQuest(QuestBase quest, bool printMessages = true) {
            base.OnAcceptQuest(quest, printMessages);
            systemEventManager.OnItemCountChanged += UpdateItemCount;
            UpdateCompletionCount(printMessages);
        }

        public override void OnAbandonQuest() {
            base.OnAbandonQuest();
            systemEventManager.OnItemCountChanged -= UpdateItemCount;
        }

        /*
        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            inventoryManager = systemGameManager.InventoryManager;
        }
        */

    }


}