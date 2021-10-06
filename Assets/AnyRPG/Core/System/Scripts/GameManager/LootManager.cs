using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class LootManager : ConfiguredMonoBehaviour {

        private List<List<LootDrop>> pages = new List<List<LootDrop>>();

        private List<LootDrop> droppedLoot = new List<LootDrop>();

        private List<LootTableState> lootTableStates = new List<LootTableState>();

        // game manager references
        private UIManager uIManager = null;
        private InventoryManager inventoryManager = null;
        private MessageFeedManager messageFeedManager = null;
        private PlayerManager playerManager = null;

        public List<List<LootDrop>> Pages { get => pages; set => pages = value; }
        public List<LootDrop> DroppedLoot { get => droppedLoot; set => droppedLoot = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            inventoryManager = systemGameManager.InventoryManager;
            messageFeedManager = systemGameManager.UIManager.MessageFeedManager;
            playerManager = systemGameManager.PlayerManager;
        }

        public void CreatePages(List<LootDrop> items) {
            //Debug.Log("LootUI.CreatePages()");
            ClearPages();
            //Debug.Log("LootUI.CreatePages(): done clearing pages");
            List<LootDrop> page = new List<LootDrop>();
            droppedLoot = items;
            for (int i = 0; i < items.Count; i++) {
                page.Add(items[i]);
                if (page.Count == 4 || i == items.Count - 1) {
                    pages.Add(page);
                    page = new List<LootDrop>();
                }
            }
            //Debug.Log("LootUI.CreatePages(): pages.count: " + pages.Count);

            (uIManager.lootWindow.CloseableWindowContents as LootUI).AddLoot();

            (uIManager.lootWindow.CloseableWindowContents as LootUI).BroadcastPageCountUpdate();
        }

        public void TakeAllLoot() {
            //Debug.Log("LootUI.TakeAllLoot()");

            // added emptyslotcount to prevent game from freezup when no bag space left and takeall button pressed
            int maximumLoopCount = droppedLoot.Count;
            int currentLoopCount = 0;
            while (pages.Count > 0 && inventoryManager.EmptySlotCount() > 0 && currentLoopCount < maximumLoopCount && (uIManager.lootWindow.CloseableWindowContents as LootUI).LootButtons.Count > 0) {
                foreach (LootButton lootButton in (uIManager.lootWindow.CloseableWindowContents as LootUI).LootButtons) {
                    //Debug.Log("LootUI.TakeAllLoot(): droppedItems.Count: " + droppedLoot.Count);
                    if (lootButton.gameObject.activeSelf == true) {
                        lootButton.TakeLoot();
                    }
                    currentLoopCount++;
                }
            }

            if (pages.Count > 0 && inventoryManager.EmptySlotCount() == 0) {
                if (inventoryManager.EmptySlotCount() == 0) {
                    //Debug.Log("No space left in inventory");
                }
                messageFeedManager.WriteMessage("Inventory is full!");
            }
        }

        public void RemoveFromDroppedItems(LootDrop lootDrop) {

            if (droppedLoot.Contains(lootDrop)) {
                droppedLoot.Remove(lootDrop);
            }
        }

        public void TakeLoot(LootDrop lootDrop) {
            (uIManager.lootWindow.CloseableWindowContents as LootUI).TakeLoot(lootDrop);
        }


        public void ClearPages() {
            pages.Clear();
            (uIManager.lootWindow.CloseableWindowContents as LootUI).ClearPages();
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
            if (inventoryManager.GetItemCount(item.DisplayName) > 0) {
                return false;
            }
            if (playerManager.MyCharacter.CharacterEquipmentManager.HasEquipment(item.DisplayName) == true) {
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