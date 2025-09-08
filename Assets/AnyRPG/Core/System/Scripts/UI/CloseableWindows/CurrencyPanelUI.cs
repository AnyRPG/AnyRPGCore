using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CurrencyPanelUI : PagedWindowContents {

        [SerializeField]
        private List<CurrencyButton> currencyButtons = new List<CurrencyButton>();

        private bool windowSubscriptionsInitialized = false;

        // game manager references
        private PlayerManager playerManager = null;
        private SystemEventManager systemEventManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            foreach (CurrencyButton currencyButton in currencyButtons) {
                currencyButton.Configure(systemGameManager);
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            systemEventManager = systemGameManager.SystemEventManager;
        }

        public override void ProcessOpenWindowNotification() {
            base.ProcessOpenWindowNotification();
            if (windowSubscriptionsInitialized == true) {
                return;
            }
            systemEventManager.OnCurrencyChange += HandleCurrencyChange;
            windowSubscriptionsInitialized = true;
        }

        public override void ReceiveClosedWindowNotification() {
            base.ReceiveClosedWindowNotification();
            if (windowSubscriptionsInitialized == false) {
                return;
            }
            systemEventManager.OnCurrencyChange -= HandleCurrencyChange;
            windowSubscriptionsInitialized = false;
        }

        private void HandleCurrencyChange() {
            //Debug.Log("CurrencyPanelUI.HandleCurrencyChange()");

            ClearPages();
            PopulatePages();
        }

        protected override void PopulatePages() {
            //Debug.Log("CurrencyPanelUI.PopulatePages()");

            CurrencyNodeContentList page = new CurrencyNodeContentList();
            foreach (CurrencyNode currencySaveData in playerManager.UnitController.CharacterCurrencyManager.CurrencyList.Values) {
                //Debug.Log($"CurrencyPanelUI.PopulatePages() adding {currencySaveData.currency.ResourceName} {currencySaveData.Amount}");
                page.currencyNodes.Add(currencySaveData);
                if (page.currencyNodes.Count == pageSize) {
                    pages.Add(page);
                    page = new CurrencyNodeContentList();
                }
            }
            if (page.currencyNodes.Count > 0) {
                pages.Add(page);
            }
            AddCurrencies();
        }

        public void AddCurrencies() {
            //Debug.Log("CurrencyPanelUI.AddCurrencies()");

            if (pages.Count > 0) {
                for (int i = 0; i < pageSize; i++) {
                    //for (int i = 0; i < pages[pageIndex].Count - 1; i++) {
                    //Debug.Log($"CurrencyPanelUI.AddCurrencies() i: {i} pageIndex: {pageIndex}");
                    if (i < (pages[pageIndex] as CurrencyNodeContentList).currencyNodes.Count) {
                        //Debug.Log($"CurrencyPanelUI.AddCurrencies() i: {i} pageIndex: {pageIndex} adding button");
                        currencyButtons[i].gameObject.SetActive(true);
                        currencyButtons[i].AddCurrency((pages[pageIndex] as CurrencyNodeContentList).currencyNodes[i].currency);
                    } else {
                        //Debug.Log($"CurrencyPanelUI.AddCurrencies() i: {i} pageIndex: {pageIndex} clearing button");
                        currencyButtons[i].ClearCurrency();
                        currencyButtons[i].gameObject.SetActive(false);
                    }
                }
            }
        }

        /*
        public override void LoadPage(int pageIndex) {
            base.LoadPage(pageIndex);
            AddCurrencies();
        }
        */

        public override void AddPageContent() {
            //Debug.Log("CurrencyPanelUI.AddPageContent()");

            base.AddPageContent();
            AddCurrencies();
        }


        public override void ClearButtons() {
            //Debug.Log("CurrencyPanelUI.ClearButtons()");

            base.ClearButtons();
            foreach (CurrencyButton btn in currencyButtons) {
                btn.gameObject.SetActive(false);
            }
        }

    }

    public class CurrencyNodeContentList : PagedContentList {
        public List<CurrencyNode> currencyNodes = new List<CurrencyNode>();
    }
}