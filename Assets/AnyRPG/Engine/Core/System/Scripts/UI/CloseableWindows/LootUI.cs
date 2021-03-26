using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class LootUI : WindowContentController, IPagedWindowContents {

        [SerializeField]
        private List<LootButton> lootButtons = new List<LootButton>();

        private int pageIndex = 0;

        public List<LootButton> LootButtons { get => lootButtons; set => lootButtons = value; }

        public event System.Action<bool> OnPageCountUpdate = delegate { };
        public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        public void AddLoot() {
            //Debug.Log("LootUI.AddLoot()");
            if (LootManager.MyInstance.Pages.Count > 0) {
                //Debug.Log("LootUI.AddLoot() pages.count: " + pages.Count);

                for (int i = 0; i < LootManager.MyInstance.Pages[pageIndex].Count; i++) {
                    if (LootManager.MyInstance.Pages[pageIndex][i] != null) {
                        // set the loot drop
                        lootButtons[i].LootDrop = LootManager.MyInstance.Pages[pageIndex][i];

                        // make sure the loot button is visible
                        lootButtons[i].gameObject.SetActive(true);

                        string colorString = "white";
                        if (LootManager.MyInstance.Pages[pageIndex][i].MyItemQuality != null) {
                            colorString = "#" + ColorUtility.ToHtmlStringRGB(LootManager.MyInstance.Pages[pageIndex][i].MyItemQuality.MyQualityColor);
                        }
                        string title = string.Format("<color={0}>{1}</color>", colorString, LootManager.MyInstance.Pages[pageIndex][i].DisplayName);
                        // set the title
                        lootButtons[i].MyTitle.text = title;
                    }
                }
            } else {
                //Debug.Log("LootUI.AddLoot() pages.count: " + pages.Count);
            }
        }

        public void TakeAllLoot() {
            LootManager.MyInstance.TakeAllLoot();
        }

        public void BroadcastPageCountUpdate() {
            OnPageCountUpdate(true);
        }

        public void ClearButtons() {
            //Debug.Log("LootUI.ClearButtons()");
            foreach (LootButton button in lootButtons) {
                button.gameObject.SetActive(false);
            }
        }

        public void TakeLoot(LootDrop lootDrop) {
            //Debug.Log("LootUI.TakeLoot(" + loot.MyName + ")");

            LootManager.MyInstance.Pages[pageIndex].Remove(lootDrop);
            LootManager.MyInstance.RemoveFromDroppedItems(lootDrop);
            lootDrop.Remove();
            SystemEventManager.MyInstance.NotifyOnTakeLoot();

            if (LootManager.MyInstance.Pages[pageIndex].Count == 0) {

                // removes the empty page
                LootManager.MyInstance.Pages.Remove(LootManager.MyInstance.Pages[pageIndex]);

                if (pageIndex == LootManager.MyInstance.Pages.Count && pageIndex > 0) {
                    pageIndex--;
                }
                AddLoot();
                OnPageCountUpdate(true);
            }
        }

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("LootUI.OnCloseWindow(): clearing pages");
            base.RecieveClosedWindowNotification();
            foreach (LootButton lootButton in lootButtons) {
                lootButton.CheckMouse();
            }
            LootManager.MyInstance.ClearPages();
            OnCloseWindow(this);
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("LootUI.OnOpenWindow()");
            base.ReceiveOpenWindowNotification();
            OnPageCountUpdate(true);
        }

        public void LoadPage(int pageIndex) {
            this.pageIndex = pageIndex;
            ClearButtons();
            AddLoot();
        }

        public void ClearPages() {
            ClearButtons();
            pageIndex = 0;
        }

        public int GetPageCount() {
            //Debug.Log("LootUI.GetPageCount()");

            return LootManager.MyInstance.Pages.Count;
        }


    }

}