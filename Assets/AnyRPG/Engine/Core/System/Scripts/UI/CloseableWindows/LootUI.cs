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
            if (LootManager.Instance.Pages.Count > 0) {
                //Debug.Log("LootUI.AddLoot() pages.count: " + pages.Count);

                for (int i = 0; i < LootManager.Instance.Pages[pageIndex].Count; i++) {
                    if (LootManager.Instance.Pages[pageIndex][i] != null) {
                        // set the loot drop
                        lootButtons[i].LootDrop = LootManager.Instance.Pages[pageIndex][i];

                        // make sure the loot button is visible
                        lootButtons[i].gameObject.SetActive(true);

                        string colorString = "white";
                        if (LootManager.Instance.Pages[pageIndex][i].MyItemQuality != null) {
                            colorString = "#" + ColorUtility.ToHtmlStringRGB(LootManager.Instance.Pages[pageIndex][i].MyItemQuality.MyQualityColor);
                        }
                        string title = string.Format("<color={0}>{1}</color>", colorString, LootManager.Instance.Pages[pageIndex][i].DisplayName);
                        // set the title
                        lootButtons[i].MyTitle.text = title;
                    }
                }
            } else {
                //Debug.Log("LootUI.AddLoot() pages.count: " + pages.Count);
            }
        }

        public void TakeAllLoot() {
            LootManager.Instance.TakeAllLoot();
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

            LootManager.Instance.Pages[pageIndex].Remove(lootDrop);
            LootManager.Instance.RemoveFromDroppedItems(lootDrop);
            lootDrop.Remove();
            SystemEventManager.TriggerEvent("OnTakeLoot", new EventParamProperties());

            if (LootManager.Instance.Pages[pageIndex].Count == 0) {

                // removes the empty page
                LootManager.Instance.Pages.Remove(LootManager.Instance.Pages[pageIndex]);

                if (pageIndex == LootManager.Instance.Pages.Count && pageIndex > 0) {
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
            LootManager.Instance.ClearPages();
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

            return LootManager.Instance.Pages.Count;
        }


    }

}