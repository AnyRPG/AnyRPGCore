using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class VendorUI : WindowContentController, IPagedWindowContents {

        public event System.Action<bool> OnPageCountUpdate = delegate { };

        [SerializeField]
        private List<VendorButton> vendorButtons = new List<VendorButton>();

        [SerializeField]
        private TMP_Dropdown dropdown = null;

        [SerializeField]
        private CurrencyBarController currencyBarController = null;

        private List<List<VendorItem>> pages = new List<List<VendorItem>>();

        private List<VendorCollection> vendorCollections = new List<VendorCollection>();

        private int pageIndex = 0;

        private int dropDownIndex = 0;

        VendorCollection buyBackCollection = null;

        // game manager references
        PlayerManager playerManager = null;
        UIManager uIManager = null;
        MessageFeedManager messageFeedManager = null;
        private CurrencyConverter currencyConverter = null;

        private List<CurrencyAmountController> currencyAmountControllers = new List<CurrencyAmountController>();

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            playerManager = systemGameManager.PlayerManager;
            uIManager = systemGameManager.UIManager;
            messageFeedManager = uIManager.MessageFeedManager;
            currencyConverter = systemGameManager.CurrencyConverter;

            //vendorUI.CreatePages(items);
            CreateEventSubscriptions();
            //InitializeBuyBackList();
            //buyBackCollection = new VendorCollection();
            buyBackCollection = ScriptableObject.CreateInstance(typeof(VendorCollection)) as VendorCollection;

            currencyBarController.Configure(systemGameManager);
            foreach (VendorButton vendorButton in vendorButtons) {
                vendorButton.Configure(systemGameManager);
            }

            foreach (CurrencyAmountController currencyAmountController in currencyAmountControllers) {
                currencyAmountController.Configure(systemGameManager);
            }
        }

        protected override void CreateEventSubscriptions() {
            //Debug.Log("VendorUI.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StartListening("OnCurrencyChange", HandleCurrencyChange);
            eventSubscriptionsInitialized = true;
        }

        protected override void CleanupEventSubscriptions() {
            //Debug.Log("UnitSpawnNode.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StopListening("OnCurrencyChange", HandleCurrencyChange);
            eventSubscriptionsInitialized = false;
        }

        public void HandleCurrencyChange(string eventName, EventParamProperties eventParamProperties) {
            UpdateCurrencyAmount();
        }

        public int GetPageCount() {
            return pages.Count;
        }

        public void CreatePages(List<VendorItem> items, bool resetPageIndex = true) {
            //Debug.Log("VendorUI.CreatePages(" + items.Count + ")");
            ClearPages(resetPageIndex);

            // remove all items with a quanity of 0 from the list
            items.RemoveAll(item => (item.Unlimited == false && item.Quantity == 0));

            List<VendorItem> page = new List<VendorItem>();
            for (int i = 0; i < items.Count; i++) {
                page.Add(items[i]);
                if (page.Count == 10 || i == items.Count - 1) {
                    pages.Add(page);
                    page = new List<VendorItem>();
                }
            }
            if (pages.Count <= pageIndex) {
                // set the page index to the last page
                pageIndex = Mathf.Clamp(pages.Count - 1, 0, int.MaxValue);
                //Debug.Log("VendorUI.CreatePages(" + items.Count + ") pageIndex: " + pageIndex);
            }
            AddItems();
            OnPageCountUpdate(false);
        }

        public void AddItems() {
            //Debug.Log("VendorUI.AddItems()");
            if (pages.Count > 0) {
                for (int i = 0; i < pages[pageIndex].Count; i++) {
                    if (pages[pageIndex][i] != null) {
                        vendorButtons[i].AddItem(pages[pageIndex][i], (dropDownIndex == 0 ? true : false));
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
            //Debug.Log("VendorUI.LoadPage(" + pageIndex + ")");
            ClearButtons();
            this.pageIndex = pageIndex;
            AddItems();
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("VendorUI.OnCloseWindow()");
            base.ReceiveClosedWindowNotification();
            ClearButtons();
            ClearPages();
            ClearVendorCollections();
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("VendorUI.ReceiveOpenWindowNotification()");
            SetBackGroundColor(new Color32(0, 0, 0, (byte)(int)(PlayerPrefs.GetFloat("PopupWindowOpacity") * 255)));
            ClearButtons();
            ClearPages();
            base.ReceiveOpenWindowNotification();
            LoadPage(0);
            OnPageCountUpdate(false);
        }

        public void UpdateCurrencyAmount() {
            if (uIManager.vendorWindow.IsOpen == false) {
                return;
            }
            Dictionary<Currency, int> playerBaseCurrency = playerManager.MyCharacter.CharacterCurrencyManager.GetRedistributedCurrency();
            if (playerBaseCurrency != null) {
                //Debug.Log("VendorUI.UpdateCurrencyAmount(): " + playerBaseCurrency.Count);
                KeyValuePair<Currency, int> keyValuePair = playerBaseCurrency.First();
                currencyBarController.UpdateCurrencyAmount(keyValuePair.Key, keyValuePair.Value);
            }
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
                vendorCollectionNames.Add(vendorCollection.DisplayName);
            }
            dropdown.AddOptions(vendorCollectionNames);
            dropdown.value = dropDownIndex;
            CreatePages(this.vendorCollections[dropDownIndex].MyVendorItems);
        }

        private void ClearVendorCollections() {
            vendorCollections.Clear();
        }

        private void ClearPages(bool resetPageIndex = true) {
            ClearButtons();
            pages.Clear();
            if (resetPageIndex == true) {
                pageIndex = 0;
            }
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

        
        public void AddToBuyBackCollection(Item newItem) {
            VendorItem newVendorItem = new VendorItem();
            newVendorItem.Quantity = 1;
            newVendorItem.Item = newItem;
            buyBackCollection.MyVendorItems.Add(newVendorItem);
        }
        

        public bool SellItem(Item item) {
            if (item.BuyPrice() <= 0 || item.GetSellPrice().Key == null) {
                messageFeedManager.WriteMessage("The vendor does not want to buy the " + item.DisplayName);
                return false;
            }
            KeyValuePair<Currency, int> sellAmount = item.GetSellPrice();

            playerManager.MyCharacter.CharacterCurrencyManager.AddCurrency(sellAmount.Key, sellAmount.Value);
            AddToBuyBackCollection(item);
            item.Slot.RemoveItem(item);

            if (systemConfigurationManager.VendorAudioProfile?.AudioClip != null) {
                audioManager.PlayEffect(systemConfigurationManager.VendorAudioProfile.AudioClip);
            }
            string priceString = currencyConverter.GetCombinedPriceString(sellAmount.Key, sellAmount.Value);
            messageFeedManager.WriteMessage("Sold " + item.DisplayName + " for " + priceString);


            if (dropDownIndex == 0) {
                /*
                CreatePages(vendorCollections[dropDownIndex].MyVendorItems);
                LoadPage(pageIndex);
                OnPageCountUpdate(false);
                */
                RefreshPage();
            }
            return true;
        }

        public void RefreshPage() {
            //Debug.Log("VendorUI.RefreshPage()");
            CreatePages(vendorCollections[dropDownIndex].MyVendorItems, false);
            //Debug.Log("VendorUI.RefreshPage() count: " + pages.Count + "; index: " + pageIndex);
            LoadPage(pageIndex);
            OnPageCountUpdate(false);
        }

    }
}