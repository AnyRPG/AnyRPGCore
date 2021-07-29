using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class LootManager {

        private List<List<LootDrop>> pages = new List<List<LootDrop>>();

        private List<LootDrop> droppedLoot = new List<LootDrop>();

        public List<List<LootDrop>> Pages { get => pages; set => pages = value; }
        public List<LootDrop> DroppedLoot { get => droppedLoot; set => droppedLoot = value; }

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

            (SystemGameManager.Instance.UIManager.PopupWindowManager.lootWindow.CloseableWindowContents as LootUI).AddLoot();

            (SystemGameManager.Instance.UIManager.PopupWindowManager.lootWindow.CloseableWindowContents as LootUI).BroadcastPageCountUpdate();
        }

        public void TakeAllLoot() {
            //Debug.Log("LootUI.TakeAllLoot()");

            // added emptyslotcount to prevent game from freezup when no bag space left and takeall button pressed
            int maximumLoopCount = droppedLoot.Count;
            int currentLoopCount = 0;
            while (pages.Count > 0 && InventoryManager.Instance.EmptySlotCount() > 0 && currentLoopCount < maximumLoopCount && (SystemGameManager.Instance.UIManager.PopupWindowManager.lootWindow.CloseableWindowContents as LootUI).LootButtons.Count > 0) {
                foreach (LootButton lootButton in (SystemGameManager.Instance.UIManager.PopupWindowManager.lootWindow.CloseableWindowContents as LootUI).LootButtons) {
                    //Debug.Log("LootUI.TakeAllLoot(): droppedItems.Count: " + droppedLoot.Count);
                    if (lootButton.gameObject.activeSelf == true) {
                        lootButton.TakeLoot();
                    }
                    currentLoopCount++;
                }
            }

            if (pages.Count > 0 && InventoryManager.Instance.EmptySlotCount() == 0) {
                if (InventoryManager.Instance.EmptySlotCount() == 0) {
                    //Debug.Log("No space left in inventory");
                }
                SystemGameManager.Instance.UIManager.MessageFeedManager.WriteMessage("Inventory is full!");
            }
        }

        public void RemoveFromDroppedItems(LootDrop lootDrop) {

            if (droppedLoot.Contains(lootDrop)) {
                droppedLoot.Remove(lootDrop);
            }
        }

        public void TakeLoot(LootDrop lootDrop) {
            //Debug.Log("LootUI.TakeLoot(" + loot.MyName + ")");

            (SystemGameManager.Instance.UIManager.PopupWindowManager.lootWindow.CloseableWindowContents as LootUI).TakeLoot(lootDrop);
        }


        public void ClearPages() {
            pages.Clear();
            (SystemGameManager.Instance.UIManager.PopupWindowManager.lootWindow.CloseableWindowContents as LootUI).ClearPages();
        }

    }

}