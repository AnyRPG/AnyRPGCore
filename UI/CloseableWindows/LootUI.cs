using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class LootUI : WindowContentController, IPagedWindowContents {

        #region Singleton
        private static LootUI instance;

        public static LootUI MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<LootUI>();
                }

                return instance;
            }
        }

        #endregion

        [SerializeField]
        private LootButton[] lootButtons;

        private List<List<LootDrop>> pages = new List<List<LootDrop>>();

        private List<LootDrop> droppedLoot = new List<LootDrop>();

        private int pageIndex = 0;

        public event System.Action<bool> OnPageCountUpdate = delegate { };
        public override event Action<ICloseableWindowContents> OnOpenWindow = delegate { };
        public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };

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

            AddLoot();

            OnPageCountUpdate(true);
        }

        private void AddLoot() {
            //Debug.Log("LootUI.AddLoot()");
            if (pages.Count > 0) {
                //Debug.Log("LootUI.AddLoot() pages.count: " + pages.Count);

                for (int i = 0; i < pages[pageIndex].Count; i++) {
                    if (pages[pageIndex][i] != null) {
                        // set the loot button icon
                        lootButtons[i].MyIcon.sprite = pages[pageIndex][i].MyItem.MyIcon;

                        lootButtons[i].MyLoot = pages[pageIndex][i].MyItem;

                        // make sure the loot button is visible
                        lootButtons[i].gameObject.SetActive(true);

                        string colorString = "white";
                        if (pages[pageIndex][i].MyItem.MyItemQuality != null && pages[pageIndex][i].MyItem.MyItemQuality != string.Empty) {
                            ItemQuality itemQuality = SystemItemQualityManager.MyInstance.GetResource(pages[pageIndex][i].MyItem.MyItemQuality);
                            if (itemQuality != null) {
                                colorString = "#" + ColorUtility.ToHtmlStringRGB(itemQuality.MyQualityColor);
                            }
                        }
                        string title = string.Format("<color={0}>{1}</color>", colorString, pages[pageIndex][i].MyItem.MyName);
                        // set the title
                        lootButtons[i].MyTitle.text = title;
                    }
                }
            } else {
                //Debug.Log("LootUI.AddLoot() pages.count: " + pages.Count);
            }
        }

        public void ClearButtons() {
            //Debug.Log("LootUI.ClearButtons()");
            foreach (LootButton button in lootButtons) {
                button.gameObject.SetActive(false);
            }
        }

        public void TakeAllLoot() {
            //Debug.Log("LootUI.TakeAllLoot()");

            // added emptyslotcount to prevent game from freezup when no bag space left and takeall button pressed
            int maximumLoopCount = droppedLoot.Count;
            int currentLoopCount = 0;
            while (pages.Count > 0 && InventoryManager.MyInstance.MyEmptySlotCount() > 0 && currentLoopCount < maximumLoopCount && lootButtons.Length > 0) {
                foreach (LootButton lootButton in lootButtons) {
                    //Debug.Log("LootUI.TakeAllLoot(): droppedItems.Count: " + droppedLoot.Count);
                    if (lootButton.gameObject.activeSelf == true) {
                        lootButton.TakeLoot();
                    }
                    currentLoopCount++;
                }
            }

            if (pages.Count > 0 && InventoryManager.MyInstance.MyEmptySlotCount() == 0) {
                if (InventoryManager.MyInstance.MyEmptySlotCount() == 0) {
                    Debug.Log("No space left in inventory");
                }
                MessageFeedManager.MyInstance.WriteMessage("Inventory is full!");
            }
        }

        private void RemoveFromDroppedItems(Item removeItem) {

            foreach (LootDrop lootDrop in droppedLoot) {
                if (lootDrop.MyItem == removeItem) {
                    droppedLoot.Remove(lootDrop);
                    return;
                }
            }
        }

        public void TakeLoot(Item loot) {
            //Debug.Log("LootUI.TakeLoot(" + loot.MyName + ")");

            LootDrop lootDrop = pages[pageIndex].Find(x => x.MyItem.MyName == loot.MyName);

            pages[pageIndex].Remove(lootDrop);
            RemoveFromDroppedItems(loot);
            lootDrop.Remove();
            SystemEventManager.MyInstance.NotifyOnTakeLoot();

            if (pages[pageIndex].Count == 0) {

                // removes the empty page
                pages.Remove(pages[pageIndex]);

                if (pageIndex == pages.Count && pageIndex > 0) {
                    pageIndex--;
                }
                AddLoot();
                OnPageCountUpdate(true);
            }
        }

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("LootUI.OnCloseWindow(): clearing pages");
            base.RecieveClosedWindowNotification();
            ClearPages();
            OnCloseWindow(this);
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("LootUI.OnOpenWindow()");
            base.ReceiveOpenWindowNotification();
            OnOpenWindow(this);
            OnPageCountUpdate(true);
        }

        public int GetPageCount() {
            //Debug.Log("LootUI.GetPageCount()");

            return pages.Count;
        }

        public void LoadPage(int pageIndex) {
            this.pageIndex = pageIndex;
            ClearButtons();
            AddLoot();
        }

        private void ClearPages() {
            ClearButtons();
            pages.Clear();
            pageIndex = 0;
        }

    }

}