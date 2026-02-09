using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.PostProcessing.SubpixelMorphologicalAntialiasing;

namespace AnyRPG {
    public class VendorPanel : PagedWindowContents {

        public override event System.Action<bool> OnPageCountUpdate = delegate { };

        [SerializeField]
        protected List<VendorButton> vendorButtons = new List<VendorButton>();

        [SerializeField]
        protected TMP_Dropdown dropdown = null;

        [SerializeField]
        protected CurrencyBarController currencyBarController = null;

        //protected List<List<VendorItem>> pages = new List<List<VendorItem>>();

        protected List<VendorCollection> vendorCollections = new List<VendorCollection>();

        protected int dropDownIndex = 0;

        // track the interactable to send a message back when the window closes
        //InteractableOptionComponent interactableOptionComponent = null;

        // game manager references
        protected PlayerManager playerManager = null;
        protected UIManager uIManager = null;
        protected MessageFeedManager messageFeedManager = null;
        protected CurrencyConverter currencyConverter = null;
        protected VendorManagerClient vendorManager = null;
        protected NetworkManagerClient NetworkManagerClient = null;

        //protected List<CurrencyAmountController> currencyAmountControllers = new List<CurrencyAmountController>();

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            CreateEventSubscriptions();

            currencyBarController.Configure(systemGameManager);
            currencyBarController.SetToolTipTransform(rectTransform);

            foreach (VendorButton vendorButton in vendorButtons) {
                vendorButton.Configure(systemGameManager);
            }

            /*
            foreach (CurrencyAmountController currencyAmountController in currencyAmountControllers) {
                currencyAmountController.Configure(systemGameManager);
            }
            */
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            playerManager = systemGameManager.PlayerManager;
            uIManager = systemGameManager.UIManager;
            messageFeedManager = uIManager.MessageFeedManager;
            currencyConverter = systemGameManager.CurrencyConverter;
            vendorManager = systemGameManager.VendorManagerClient;
            NetworkManagerClient = systemGameManager.NetworkManagerClient;
        }

        protected override void ProcessCreateEventSubscriptions() {
            //Debug.Log("VendorUI.CreateEventSubscriptions()");
            base.ProcessCreateEventSubscriptions();
            systemEventManager.OnCurrencyChange += HandleCurrencyChange;
        }

        protected override void ProcessCleanupEventSubscriptions() {
            //Debug.Log("UnitSpawnNode.CleanupEventSubscriptions()");
            base.ProcessCleanupEventSubscriptions();
            systemEventManager.OnCurrencyChange -= HandleCurrencyChange;
        }

        public void HandleCurrencyChange() {
            UpdateCurrencyAmount();
        }

        public void CreatePages(List<VendorItem> items, bool resetPageIndex = true) {
            //Debug.Log("VendorUI.CreatePages(" + items.Count + ", " + resetPageIndex + ")");
            ClearPages(resetPageIndex);

            // assign an item id based on absolute position (before removing limited quantity items) for sending across the network when using vendor buttons in the UI
            for (int i = 0; i < items.Count; i++) {
                items[i].itemIndex = i;
            }

            // remove all items with a quanity of 0 from the list
            items.RemoveAll(item => (item.Unlimited == false && item.Quantity == 0));

            VendorItemContentList page = new VendorItemContentList();

            for (int i = 0; i < items.Count; i++) {
                page.vendorItems.Add(items[i]);
                if (page.vendorItems.Count == 10 || i == items.Count - 1) {
                    pages.Add(page);
                    page = new VendorItemContentList();
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
                //for (int i = 0; i < (pages[pageIndex] as VendorItemContentList).vendorItems.Count; i++) {
                for (int i = 0; i < (pages[pageIndex] as VendorItemContentList).vendorItems.Count; i++) {
                    if ((pages[pageIndex] as VendorItemContentList).vendorItems[i] != null) {
                        vendorButtons[i].AddItem((pages[pageIndex] as VendorItemContentList).vendorItems[i], dropDownIndex, (pages[pageIndex] as VendorItemContentList).vendorItems[i].itemIndex, (dropDownIndex == 0 ? true : false));
                    }
                }
            }
            //if (currentNavigationController != null) {
                uINavigationControllers[1].UpdateNavigationList();
            //}
            //FocusCurrentButton();
        }   

        public override void ClearButtons() {
            //Debug.Log("VendorUI.ClearButtons()");
            foreach (VendorButton btn in vendorButtons) {
                //Debug.Log("VendorUI.ClearButtons() setting a button to not active");
                btn.gameObject.SetActive(false);
            }
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("VendorUI.OnCloseWindow()");
            base.ReceiveClosedWindowNotification();
            if (vendorManager.VendorComponent?.Interactable != null) {
                // if we got kicked off the server the window can be closed after the vendor despawns
                vendorManager.VendorComponent.Interactable.InteractableEventController.OnAddToBuyBackCollection -= HandleAddtoBuyBackCollection;
                vendorManager.VendorComponent.Interactable.InteractableEventController.OnSellItemToPlayer -= HandleBuyItemFromVendor;
            }
            ClearButtons();
            ClearPages();
            ClearVendorCollections();
            vendorManager.EndInteraction();
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("VendorUI.ProcessOpenWindowNotification()");
            //SetBackGroundColor(new Color32(0, 0, 0, (byte)(int)(PlayerPrefs.GetFloat("PopupWindowOpacity") * 255)));
            ClearButtons();
            ClearPages();
            SetNavigationController(uINavigationControllers[0]);
            base.ProcessOpenWindowNotification();
            LoadPage(0);
            OnPageCountUpdate(false);

            PopulateDropDownList(vendorManager.VendorProps.VendorCollections);
            vendorManager.VendorComponent.Interactable.InteractableEventController.OnAddToBuyBackCollection += HandleAddtoBuyBackCollection;
            vendorManager.VendorComponent.Interactable.InteractableEventController.OnSellItemToPlayer += HandleBuyItemFromVendor;
        }

        public void UpdateCurrencyAmount() {
            if (uIManager.vendorWindow.IsOpen == false) {
                return;
            }
            Dictionary<Currency, int> playerBaseCurrency = playerManager.UnitController.CharacterCurrencyManager.GetRedistributedCurrency();
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
            this.vendorCollections.Add(vendorManager.VendorComponent.GetBuyBackCollection(NetworkManagerClient.AccountId));
            this.vendorCollections.AddRange(vendorCollections);
            dropdown.ClearOptions();
            List<string> vendorCollectionNames = new List<string>();
            vendorCollectionNames.Add("Buy Back Items");
            foreach (VendorCollection vendorCollection in vendorCollections) {
                vendorCollectionNames.Add(vendorCollection.DisplayName);
            }
            dropdown.AddOptions(vendorCollectionNames);
            dropdown.value = dropDownIndex;
            // testing - does this get done automatically when the dropdown value is set?
            //CreatePages(this.vendorCollections[dropDownIndex].MyVendorItems);
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
            CreatePages(vendorCollections[dropDownIndex].VendorItems);
            LoadPage(0);
            OnPageCountUpdate(false);
        }

        public bool SellItem(InstantiatedItem instantiatedItem) {
            vendorManager.RequestSellItemToVendor(playerManager.UnitController, instantiatedItem);

            /*
            if (systemConfigurationManager.VendorAudioClip != null) {
                audioManager.PlayEffect(systemConfigurationManager.VendorAudioClip);
            }

            if (dropDownIndex == 0) {
                RefreshPage();
            }
            */
            return true;
        }

        public void HandleAddtoBuyBackCollection(UnitController controller, InstantiatedItem item) {
            if (systemConfigurationManager.VendorAudioClip != null) {
                audioManager.PlayEffect(systemConfigurationManager.VendorAudioClip);
            }

            if (dropDownIndex == 0) {
                RefreshPage();
            }
        }

        private void HandleBuyItemFromVendor(VendorItem vendorItem, int componentIndex, int collectionIndex, int itemIndex) {

            if (systemConfigurationManager.VendorAudioClip != null) {
                audioManager.PlayEffect(systemConfigurationManager.VendorAudioClip);
            }

            if (!vendorItem.Unlimited) {
                //quantity.text = vendorItem.Quantity.ToString();
                if (vendorItem.Quantity == 0) {
                    gameObject.SetActive(false);
                    uIManager.HideToolTip();
                    // if there is no longer anything in this slot, re-create the pages so there is no empty slot in the middle of the page
                    RefreshPage();
                }
            }
        }

        public void RefreshPage() {
            //Debug.Log("VendorUI.RefreshPage()");

            CreatePages(vendorCollections[dropDownIndex].VendorItems, false);
            //Debug.Log("VendorUI.RefreshPage() count: " + pages.Count + "; index: " + pageIndex);
            LoadPage(pageIndex);
            OnPageCountUpdate(false);
        }

        public override void AddPageContent() {
            base.AddPageContent();
            AddItems();
        }

    }

    public class VendorItemContentList : PagedContentList {
        public List<VendorItem> vendorItems = new List<VendorItem>();
    }
}