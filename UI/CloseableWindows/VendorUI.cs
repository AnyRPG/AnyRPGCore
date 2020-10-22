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
        public override event System.Action<ICloseableWindowContents> OnOpenWindow = delegate { };

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

        protected bool eventSubscriptionsInitialized = false;

        VendorCollection buyBackCollection = null;


        private List<CurrencyAmountController> currencyAmountControllers = new List<CurrencyAmountController>();

        public override void Awake() {
            base.Awake();
            //vendorUI.CreatePages(items);
            CreateEventSubscriptions();
            //InitializeBuyBackList();
            //buyBackCollection = new VendorCollection();
            buyBackCollection = ScriptableObject.CreateInstance(typeof(VendorCollection)) as VendorCollection;
        }

        private void CreateEventSubscriptions() {
            //Debug.Log("VendorUI.CreateEventSubscriptions()");
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
            //Debug.Log("VendorUI.ReceiveOpenWindowNotification()");
            ClearButtons();
            ClearPages();
            base.ReceiveOpenWindowNotification();
            OnOpenWindow(this);
            LoadPage(0);
            OnPageCountUpdate(false);
        }

        public void UpdateCurrencyAmount() {
            if (PopupWindowManager.MyInstance.vendorWindow.IsOpen == false) {
                return;
            }
            Dictionary<Currency, int> playerBaseCurrency = PlayerManager.MyInstance.MyCharacter.CharacterCurrencyManager.GetRedistributedCurrency();
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

        
        public void AddToBuyBackCollection(Item newItem) {
            VendorItem newVendorItem = new VendorItem();
            newVendorItem.MyQuantity = 1;
            newVendorItem.MyItem = newItem;
            buyBackCollection.MyVendorItems.Add(newVendorItem);
        }
        

        public bool SellItem(Item item) {
            if (item.BuyPrice <= 0 || item.MySellPrice.Key == null) {
                MessageFeedManager.MyInstance.WriteMessage("The vendor does not want to buy the " + item.DisplayName);
                return false;
            }
            KeyValuePair<Currency, int> sellAmount = item.MySellPrice;

            PlayerManager.MyInstance.MyCharacter.CharacterCurrencyManager.AddCurrency(sellAmount.Key, sellAmount.Value);
            AddToBuyBackCollection(item);
            //InventoryManager.MyInstance.RemoveItem(item);
            item.MySlot.RemoveItem(item);

            if (SystemConfigurationManager.MyInstance.VendorAudioProfile != null && SystemConfigurationManager.MyInstance.VendorAudioProfile.AudioClip != null) {
                AudioManager.MyInstance.PlayEffect(SystemConfigurationManager.MyInstance.VendorAudioProfile.AudioClip);
            }
            string priceString = CurrencyConverter.GetCombinedPriceSring(sellAmount.Key, sellAmount.Value);
            MessageFeedManager.MyInstance.WriteMessage("Sold " + item.DisplayName + " for " + priceString);


            if (dropDownIndex == 0) {
                CreatePages(vendorCollections[dropDownIndex].MyVendorItems);
                LoadPage(pageIndex);
                OnPageCountUpdate(false);
            }
            return true;
        }
    }
}