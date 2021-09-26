using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class LootUI : WindowContentController, IPagedWindowContents {

        public event System.Action<bool> OnPageCountUpdate = delegate { };
        public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        [SerializeField]
        private List<LootButton> lootButtons = new List<LootButton>();

        [SerializeField]
        private HighlightButton takeAllButton = null;

        private int pageIndex = 0;

        // game manager references
        private LootManager lootManager = null;

        public List<LootButton> LootButtons { get => lootButtons; set => lootButtons = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            foreach (LootButton lootButton in lootButtons) {
                lootButton.Configure(systemGameManager);
                lootButton.SetLootUI(this);
            }
            takeAllButton.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            lootManager = systemGameManager.LootManager;
        }

        public void AddLoot() {
            //Debug.Log("LootUI.AddLoot()");
            if (lootManager.Pages.Count > 0) {
                //Debug.Log("LootUI.AddLoot() pages.count: " + pages.Count);

                for (int i = 0; i < lootManager.Pages[pageIndex].Count; i++) {
                    if (lootManager.Pages[pageIndex][i] != null) {
                        // set the loot drop
                        lootButtons[i].LootDrop = lootManager.Pages[pageIndex][i];

                        // make sure the loot button is visible
                        lootButtons[i].gameObject.SetActive(true);

                        string colorString = "white";
                        if (lootManager.Pages[pageIndex][i].ItemQuality != null) {
                            colorString = "#" + ColorUtility.ToHtmlStringRGB(lootManager.Pages[pageIndex][i].ItemQuality.QualityColor);
                        }
                        string title = string.Format("<color={0}>{1}</color>", colorString, lootManager.Pages[pageIndex][i].DisplayName);
                        // set the title
                        lootButtons[i].MyTitle.text = title;
                    }
                }
            } else {
                //Debug.Log("LootUI.AddLoot() pages.count: " + pages.Count);
            }
        }

        public void TakeAllLoot() {
            lootManager.TakeAllLoot();
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

            lootManager.Pages[pageIndex].Remove(lootDrop);
            lootManager.RemoveFromDroppedItems(lootDrop);
            lootDrop.Remove();
            SystemEventManager.TriggerEvent("OnTakeLoot", new EventParamProperties());

            if (lootManager.Pages[pageIndex].Count == 0) {

                // removes the empty page
                lootManager.Pages.Remove(lootManager.Pages[pageIndex]);

                if (pageIndex == lootManager.Pages.Count && pageIndex > 0) {
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
            lootManager.ClearPages();
            OnCloseWindow(this);
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("LootUI.OnOpenWindow()");
            base.ReceiveOpenWindowNotification();
            SetBackGroundColor(new Color32(0, 0, 0, (byte)(int)(PlayerPrefs.GetFloat("PopupWindowOpacity") * 255)));
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

            return lootManager.Pages.Count;
        }


    }

}