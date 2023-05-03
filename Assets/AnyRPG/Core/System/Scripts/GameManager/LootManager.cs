using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class LootManager : ConfiguredMonoBehaviour {

        public event System.Action OnTakeLoot = delegate { };

        // a list that is reset every time the loot window opens or closes to give the proper list depending on what was looted
        private List<LootDrop> droppedLoot = new List<LootDrop>();

        // this list is solely for the purpose of tracking dropped loot to ensure that unique items cannot be dropped twice
        // if one drops and is left on a body unlooted and another enemy is killed
        private List<LootTableState> lootTableStates = new List<LootTableState>();

        // game manager references
        private MessageFeedManager messageFeedManager = null;
        private PlayerManager playerManager = null;

        public List<LootDrop> DroppedLoot { get => droppedLoot; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            messageFeedManager = systemGameManager.UIManager.MessageFeedManager;
            playerManager = systemGameManager.PlayerManager;
        }

        public void AddLoot(List<LootDrop> items) {
            //Debug.Log("LootManager.AddLoot()");

            droppedLoot = items;
        }

        public void ClearDroppedLoot() {
            //Debug.Log("LootManager.ClearDroppedLoot()");

            droppedLoot.Clear();
        }

        public void TakeLoot(LootDrop lootDrop) {
            //Debug.Log("LootManager.TakeLoot()");

            RemoveFromDroppedItems(lootDrop);

            SystemEventManager.TriggerEvent("OnTakeLoot", new EventParamProperties());
            OnTakeLoot();
        }

        public void RemoveFromDroppedItems(LootDrop lootDrop) {
            //Debug.Log("LootManager.RemoveFromDroppedItems()");

            if (droppedLoot.Contains(lootDrop)) {
                droppedLoot.Remove(lootDrop);
            }
        }

        public void TakeAllLoot() {
            //Debug.Log("LootManager.TakeAllLoot()");

            // added emptyslotcount to prevent game from freezup when no bag space left and takeall button pressed
            int maximumLoopCount = droppedLoot.Count;
            int currentLoopCount = 0;
            while (droppedLoot.Count > 0 && playerManager.MyCharacter.CharacterInventoryManager.EmptySlotCount() > 0 && currentLoopCount < maximumLoopCount) {
                droppedLoot[0].TakeLoot();
                currentLoopCount++;
            }

            if (droppedLoot.Count > 0 && playerManager.MyCharacter.CharacterInventoryManager.EmptySlotCount() == 0) {
                if (playerManager.MyCharacter.CharacterInventoryManager.EmptySlotCount() == 0) {
                    //Debug.Log("No space left in inventory");
                }
                messageFeedManager.WriteMessage("Inventory is full!");
            }
        }

        public void AddLootTableState(LootTableState lootTableState) {
            //Debug.Log("LootManager.AddLootTableState()");

            if (lootTableStates.Contains(lootTableState) == false) {
                lootTableStates.Add(lootTableState);
            }
        }

        public void RemoveLootTableState(LootTableState lootTableState) {
            //Debug.Log("LootManager.RemoveLootTableState()");

            if (lootTableStates.Contains(lootTableState)) {
                lootTableStates.Remove(lootTableState);
            }
        }

        public bool CanDropUniqueItem(Item item) {
            //Debug.Log("LootManager.CanDropUniqueItem(" + item.DisplayName + ")");
            if (playerManager.MyCharacter.CharacterInventoryManager.GetItemCount(item.ResourceName) > 0) {
                return false;
            }
            if (playerManager.MyCharacter.CharacterEquipmentManager.HasEquipment(item.ResourceName) == true) {
                return false;
            }
            foreach (LootTableState lootTableState in lootTableStates) {
                foreach (LootDrop lootDrop in lootTableState.DroppedItems) {
                    if (lootDrop.HasItem(item)) {
                        return false;
                    }
                }
            }
            return true;
        }

    }

}