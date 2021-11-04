using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class LootUI : PagedWindowContents {

        public override event System.Action<bool> OnPageCountUpdate = delegate { };
        public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        [Header("Loot UI")]

        [SerializeField]
        protected List<LootButton> lootButtons = new List<LootButton>();

        [SerializeField]
        protected HighlightButton takeAllButton = null;

        // game manager references
        private LootManager lootManager = null;

        public List<LootButton> LootButtons { get => lootButtons; set => lootButtons = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            foreach (LootButton lootButton in lootButtons) {
                lootButton.Configure(systemGameManager);
                //lootButton.SetLootUI(this);
            }
            takeAllButton.Configure(systemGameManager);
            pageSize = 4;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            lootManager = systemGameManager.LootManager;
        }

        protected override void PopulatePages() {
            Debug.Log("LootUI.PopulatePages()");
            base.PopulatePages();

            pages.Clear();
            LootDropContentList page = new LootDropContentList();
            foreach (LootDrop lootDrop in lootManager.DroppedLoot) {
                page.lootDrops.Add(lootDrop);
                if (page.lootDrops.Count == pageSize) {
                    pages.Add(page);
                    page = new LootDropContentList();
                }
            }
            if (page.lootDrops.Count > 0) {
                pages.Add(page);
            }
            AddLoot();
        }

        public void AddLoot() {
            Debug.Log("LootUI.AddLoot()");
            if (pages.Count > 0) {
                if (pageIndex >= pages.Count) {
                    pageIndex = pages.Count - 1;
                }
                for (int i = 0; i < pageSize; i++) {
                    //for (int i = 0; i < pages[pageIndex].Count - 1; i++) {
                    //Debug.Log("SkillBookUI.AddSkills(): i: " + i);
                    if (i < (pages[pageIndex] as LootDropContentList).lootDrops.Count) {
                        //Debug.Log("adding skill");
                        lootButtons[i].gameObject.SetActive(true);
                        lootButtons[i].SetLootDrop((pages[pageIndex] as LootDropContentList).lootDrops[i]);
                        uINavigationControllers[0].AddActiveButton(lootButtons[i]);

                    } else {
                        //Debug.Log("clearing skill");
                        lootButtons[i].ClearLootDrop();
                        lootButtons[i].gameObject.SetActive(false);
                    }
                }
                currentNavigationController.FocusCurrentButton();
            }
        }

        public void TakeAllLoot() {
            Debug.Log("LootUI.TakeAllLoot()");
            lootManager.TakeAllLoot();
            BroadcastPageCountUpdate();
        }

        public void BroadcastPageCountUpdate() {
            Debug.Log("LootUI.BroadcastPageCountUpdate()");
            OnPageCountUpdate(true);
        }

        public override void ClearButtons() {
            Debug.Log("LootUI.ClearButtons()");
            foreach (LootButton button in lootButtons) {
                button.DeSelect();
                button.gameObject.SetActive(false);
            }
            uINavigationControllers[0].ClearActiveButtons();
        }

        public override void ReceiveClosedWindowNotification() {
            Debug.Log("LootUI.ReceiveClosedWindowNotification(): clearing pages");
            base.ReceiveClosedWindowNotification();
            foreach (LootButton lootButton in lootButtons) {
                lootButton.CheckMouse();
            }
            lootManager.ClearDroppedLoot();
            OnCloseWindow(this);
        }

        public override void ReceiveOpenWindowNotification() {
            Debug.Log("LootUI.ReceiveOpenWindowNotification()");
            base.ReceiveOpenWindowNotification();
            SetBackGroundColor(new Color32(0, 0, 0, (byte)(int)(PlayerPrefs.GetFloat("PopupWindowOpacity") * 255)));
            BroadcastPageCountUpdate();
        }

        public override void AddPageContent() {
            Debug.Log("LootUI.AddPageContent()");
            base.AddPageContent();
            AddLoot();
        }

        /*
        public override int GetPageCount() {
            //Debug.Log("LootUI.GetPageCount()");

            return lootManager.Pages.Count;
        }
        */

        public void HandleTakeLoot() {
            Debug.Log("LootUI.HandleTakeLoot()");

            ClearButtons();
            PopulatePages();
            uINavigationControllers[0].FocusCurrentButton();
            BroadcastPageCountUpdate();
        }

        protected override void ProcessCreateEventSubscriptions() {
            base.ProcessCreateEventSubscriptions();
            lootManager.OnTakeLoot += HandleTakeLoot;
        }

        protected override void ProcessCleanupEventSubscriptions() {
            base.ProcessCleanupEventSubscriptions();
            lootManager.OnTakeLoot -= HandleTakeLoot;
        }

    }

    public class LootDropContentList : PagedContentList {
        public List<LootDrop> lootDrops = new List<LootDrop>();
    }

}