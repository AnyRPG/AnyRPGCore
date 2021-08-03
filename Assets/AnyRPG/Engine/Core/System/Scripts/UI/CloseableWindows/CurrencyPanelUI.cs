using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CurrencyPanelUI : PagedWindowContents {

        [SerializeField]
        private List<CurrencyButton> currencyButtons = new List<CurrencyButton>();

        private PlayerManager playerManager = null;

        public override void Init(SystemGameManager systemGameManager) {
            base.Init(systemGameManager);
            playerManager = systemGameManager.PlayerManager;
        }

        protected override void PopulatePages() {
            //Debug.Log("ReputationBookUI.CreatePages()");
            CurrencyNodeContentList page = new CurrencyNodeContentList();
            foreach (CurrencyNode currencySaveData in playerManager.MyCharacter.CharacterCurrencyManager.MyCurrencyList.Values) {
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
            //Debug.Log("ReputationBookUI.AddAbilities()");
            if (pages.Count > 0) {
                for (int i = 0; i < pageSize; i++) {
                    //for (int i = 0; i < pages[pageIndex].Count - 1; i++) {
                    //Debug.Log("ReputationBookUI.AddAbilities(): i: " + i);
                    if (i < (pages[pageIndex] as CurrencyNodeContentList).currencyNodes.Count) {
                        //Debug.Log("adding ability");
                        currencyButtons[i].gameObject.SetActive(true);
                        currencyButtons[i].AddCurrency((pages[pageIndex] as CurrencyNodeContentList).currencyNodes[i].currency);
                    } else {
                        //Debug.Log("clearing ability");
                        currencyButtons[i].ClearCurrency();
                        currencyButtons[i].gameObject.SetActive(false);
                    }
                }
            }
        }

        public override void LoadPage(int pageIndex) {
            base.LoadPage(pageIndex);
            AddCurrencies();
        }

        public override void ClearButtons() {
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