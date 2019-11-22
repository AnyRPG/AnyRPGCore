using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class VendorUI : WindowContentController, IPagedWindowContents {

        public event System.Action<bool> OnPageCountUpdate = delegate { };
        public override event System.Action<ICloseableWindowContents> OnOpenWindow = delegate { };

        [SerializeField]
        private VendorButton[] vendorButtons;

        [SerializeField]
        private Dropdown dropdown;

        [SerializeField]
        private GameObject currencyAmountPrefab;

        [SerializeField]
        private GameObject currencyAmountParent;

        private List<List<VendorItem>> pages = new List<List<VendorItem>>();

        private List<VendorCollection> vendorCollections = new List<VendorCollection>();

        private int pageIndex;

        private int dropDownIndex;

        protected bool eventSubscriptionsInitialized = false;

        VendorCollection buyBackCollection;


        private List<CurrencyAmountController> currencyAmountControllers = new List<CurrencyAmountController>();

        public override void Awake() {
            base.Awake();
            //vendorUI.CreatePages(items);
            CreateEventSubscriptions();
            //InitializeBuyBackList();
            buyBackCollection = new VendorCollection();
        }

        private void CreateEventSubscriptions() {
            Debug.Log("VendorUI.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.MyInstance.OnCurrencyChange += UpdateCurrencyAmount;
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("UnitSpawnNode.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnCurrencyChange -= UpdateCurrencyAmount;
            }
            eventSubscriptionsInitialized = false;
        }

        public void OnDestroy() {
            //Debug.Log("UnitSpawnNode.OnDisable(): stopping any outstanding coroutines");
            CleanupEventSubscriptions();
        }

        public int GetPageCount() {
            return pages.Count;
        }

        public void CreatePages(List<VendorItem> items) {
            //Debug.Log("VendorUI.CreatePages()");
            ClearPages();
            List<VendorItem> page = new List<VendorItem>();
            for (int i = 0; i < items.Count; i++) {
                page.Add(items[i]);
                if (page.Count == 10 || i == items.Count - 1) {
                    pages.Add(page);
                    page = new List<VendorItem>();
                }
            }
            AddItems();
            OnPageCountUpdate(false);
        }

        public void AddItems() {
            //Debug.Log("VendorUI.AddItems()");
            if (pages.Count > 0) {
                for (int i = 0; i < pages[pageIndex].Count; i++) {
                    if (pages[pageIndex][i] != null) {
                        vendorButtons[i].AddItem(pages[pageIndex][i]);
                    }
                }
            }
        }

        public void ClearButtons() {
            //Debug.Log("VendorUI.ClearButtons()");
            foreach (VendorButton btn in vendorButtons) {
                //Debug.Log("VendorUI.ClearButtons() setting a button to not active");
                btn.gameObject.SetActive(false);
            }
        }

        public void LoadPage(int pageIndex) {
            //Debug.Log("VendorUI.LoadPage()");
            ClearButtons();
            this.pageIndex = pageIndex;
            AddItems();
        }

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("VendorUI.OnCloseWindow()");
            base.RecieveClosedWindowNotification();
            ClearButtons();
            ClearPages();
            ClearVendorCollections();
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("VendorUI.OnOpenWindow()");
            ClearButtons();
            ClearPages();
            base.ReceiveOpenWindowNotification();
            OnOpenWindow(this);
            LoadPage(0);
            OnPageCountUpdate(false);
        }

        public void PopulateDropDownList(List<VendorCollection> vendorCollections) {
            UpdateCurrencyAmount();
            dropDownIndex = 1;
            this.vendorCollections = new List<VendorCollection>(1 + vendorCollections.Count);
            this.vendorCollections.Add(buyBackCollection);
            this.vendorCollections.AddRange(vendorCollections);
            dropdown.ClearOptions();
            List<string> vendorCollectionNames = new List<string>();
            vendorCollectionNames.Add("Buy Back Items");
            foreach (VendorCollection vendorCollection in vendorCollections) {
                vendorCollectionNames.Add(vendorCollection.MyName);
            }
            dropdown.AddOptions(vendorCollectionNames);
            dropdown.value = dropDownIndex;
            CreatePages(this.vendorCollections[dropDownIndex].MyVendorItems);
        }

        private void ClearVendorCollections() {
            vendorCollections.Clear();
        }

        private void ClearPages() {
            ClearButtons();
            pages.Clear();
            pageIndex = 0;
        }

        public void SetCollection(int dropDownIndex) {
            //Debug.Log("CharacterCreatorPanel.SetHair(" + dropdownIndex + "): " + hairAppearanceDropdown.options[hairAppearanceDropdown.value].text);
            ClearButtons();
            ClearPages();
            this.dropDownIndex = dropDownIndex;
            CreatePages(vendorCollections[dropDownIndex].MyVendorItems);
            LoadPage(0);
            OnPageCountUpdate(false);
        }

        public void UpdateCurrencyAmount() {

            // get list of currencies
            List<Currency> currencyList = new List<Currency>(SystemConfigurationManager.MyInstance.MyDefaultCurrencyGroup.MyCurrencyGroupRates.Count + 1);
            currencyList.Add(SystemConfigurationManager.MyInstance.MyDefaultCurrencyGroup.MyBaseCurrency);
            if (SystemConfigurationManager.MyInstance.MyDefaultCurrencyGroup.MyCurrencyGroupRates.Count > 0) {
                foreach (CurrencyGroupRate currencyGroupRate in SystemConfigurationManager.MyInstance.MyDefaultCurrencyGroup.MyCurrencyGroupRates) {
                    currencyList.Add(currencyGroupRate.MyCurrency);
                }
            }

            // despawn old ones
            foreach (CurrencyAmountController currencyAmountController in currencyAmountControllers) {
                Destroy(currencyAmountController.gameObject);
            }
            currencyAmountControllers.Clear();

            // spawn new ones
            foreach (Currency currency in currencyList) {
                GameObject go = Instantiate(currencyAmountPrefab, currencyAmountParent.transform);
                go.transform.SetAsFirstSibling();
                CurrencyAmountController currencyAmountController = go.GetComponent<CurrencyAmountController>();
                currencyAmountControllers.Add(currencyAmountController);
                if (currencyAmountController.MyCurrencyIcon != null) {
                    currencyAmountController.MyCurrencyIcon.SetDescribable(currency);
                }
                if (currencyAmountController.MyAmountText != null) {
                    currencyAmountController.MyAmountText.text = PlayerManager.MyInstance.MyCharacter.MyPlayerCurrencyManager.GetCurrencyAmount(currency).ToString();
                }
            }
        }

        public void AddToBuyBackCollection(Item newItem) {
            VendorItem newVendorItem = new VendorItem();
            newVendorItem.MyQuantity = 1;
            newVendorItem.MyItem = newItem;
            buyBackCollection.MyVendorItems.Add(newVendorItem);
        }

        

        public bool SellItem(Item item) {
            if (item.MyPrice <= 0) {
                return false;
            }
            int sellAmount = item.MyPrice;
            Currency currency = item.MyCurrency;
            CurrencyGroup currencyGroup = (PlayerManager.MyInstance.MyCharacter as PlayerCharacter).MyPlayerCurrencyManager.FindCurrencyGroup(currency);
            if (currencyGroup != null) {
                int convertedSellAmount = (PlayerManager.MyInstance.MyCharacter as PlayerCharacter).MyPlayerCurrencyManager.GetConvertedValue(currency, sellAmount);
                currency = currencyGroup.MyBaseCurrency;
                sellAmount = (int)Mathf.Ceil((float)convertedSellAmount * SystemConfigurationManager.MyInstance.MyVendorPriceMultiplier);
            } else {
                sellAmount = (int)Mathf.Ceil((float)sellAmount * SystemConfigurationManager.MyInstance.MyVendorPriceMultiplier);
            }

            (PlayerManager.MyInstance.MyCharacter as PlayerCharacter).MyPlayerCurrencyManager.AddCurrency(currency, sellAmount);
            AddToBuyBackCollection(item);
            //InventoryManager.MyInstance.RemoveItem(item);
            item.MySlot.RemoveItem(item);
            if (dropDownIndex == 0) {
                CreatePages(vendorCollections[dropDownIndex].MyVendorItems);
                LoadPage(pageIndex);
                OnPageCountUpdate(false);
            }
            return true;
        }
    }
}