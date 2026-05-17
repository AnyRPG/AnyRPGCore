using System;
using System.Collections;
using System.Collections.Generic;
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

        public void HandleItemCountChanged(UnitController sourceUnitController, Item item) {

            // change this with check reference to item prefab in the future
            if (SystemDataUtility.MatchResource(item.ResourceName, itemName, partialMatch)) {
                UpdateCompletionCount(sourceUnitController);
            }
        }

        public override void UpdateCompletionCount(UnitController sourceUnitController, bool printMessages = true) {

            bool completeBefore = IsComplete(sourceUnitController);
            if (completeBefore) {
                return;
            }
            SetCurrentAmount(sourceUnitController,
                sourceUnitController.CharacterInventoryManager.GetItemCount(itemName, partialMatch)
                + sourceUnitController.CharacterEquipmentManager.GetEquipmentCount(itemName, partialMatch));

            if (CurrentAmount(sourceUnitController) <= Amount && questBase.PrintObjectiveCompletionMessages && printMessages == true && CurrentAmount(sourceUnitController) != 0) {
                sourceUnitController.WriteMessageFeedMessage(string.Format("{0}: {1}/{2}", DisplayName, Mathf.Clamp(CurrentAmount(sourceUnitController), 0, Amount), Amount));
            }
            if (completeBefore == false && IsComplete(sourceUnitController) && questBase.PrintObjectiveCompletionMessages && printMessages == true) {
                sourceUnitController.WriteMessageFeedMessage(string.Format("Collect {0} {1}: Objective Complete", CurrentAmount(sourceUnitController), DisplayName));
            }
            questBase.CheckCompletion(sourceUnitController, true, printMessages);
            base.UpdateCompletionCount(sourceUnitController, printMessages);
        }

        public void Complete(UnitController sourceUnitController) {
            List<InstantiatedItem> items = sourceUnitController.CharacterInventoryManager.GetItems(itemName, Amount);
            foreach (InstantiatedItem instantiatedItem in items) {
                instantiatedItem.Remove();
            }
        }

        public override void OnAcceptQuest(UnitController sourceUnitController, QuestBase quest, bool printMessages = true) {
            base.OnAcceptQuest(sourceUnitController, quest, printMessages);
            sourceUnitController.UnitEventController.OnItemCountChanged += HandleItemCountChanged;
            UpdateCompletionCount(sourceUnitController, printMessages);
        }

        public override void OnAbandonQuest(UnitController sourceUnitController) {
            base.OnAbandonQuest(sourceUnitController);
            sourceUnitController.UnitEventController.OnItemCountChanged -= HandleItemCountChanged;
        }

        /*
        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            inventoryManager = systemGameManager.InventoryManager;
        }
        */

    }


}