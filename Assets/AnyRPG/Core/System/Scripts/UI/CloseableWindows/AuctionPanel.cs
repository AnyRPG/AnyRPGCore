using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class AuctionPanel : PagedWindowContents {

        [Header("Auction Panel")]

        [SerializeField]
        private TMP_InputField searchInput = null;

        [SerializeField]
        private Toggle toggle = null;

        [SerializeField]
        private List<AuctionItemButton> auctionItemButtons = new List<AuctionItemButton>();

        [SerializeField]
        private UINavigationController auctionButtonsNavigationController = null;

        [SerializeField]
        AuctionListAttachmentButton auctionListAttachmentButton = null;

        [SerializeField]
        CurrencyEntryBarController currencyEntryBarController = null;

        [SerializeField]
        private CurrencyBarController currencyBarController = null;

        private AuctionSortField currentSortType = AuctionSortField.Name;
        private bool reverseSort = false;

        private bool windowSubscriptionsInitialized = false;

        // game manager references
        private AuctionManagerClient auctionManagerClient = null;
        private UIManager uiManager = null;
        private SystemItemManager systemItemManager = null;
        private PlayerManager playerManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            //auctionListAttachmentButton.Configure(systemGameManager);
            currencyEntryBarController.Configure(systemGameManager);
            currencyBarController.Configure(systemGameManager);
            auctionListAttachmentButton.OnAddAttachment += HandleAddAttachment;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            auctionManagerClient = systemGameManager.AuctionManagerClient;
            uiManager = systemGameManager.UIManager;
            systemItemManager = systemGameManager.SystemItemManager;
            playerManager = systemGameManager.PlayerManager;
        }

        public override void ProcessOpenWindowNotification() {
            base.ProcessOpenWindowNotification();
            if (windowSubscriptionsInitialized == true) {
                return;
            }
            auctionManagerClient.OnSetAuctionItems += HandleSetAuctionItems;
            auctionManagerClient.OnCancelAuction += HandleCancelAuction;
            auctionManagerClient.OnBuyAuctionItem += HandleBuyAuctionItem;
            auctionManagerClient.OnItemListed += HandleItemListed;
            windowSubscriptionsInitialized = true;
            currencyEntryBarController.ResetCurrencyAmounts();
            auctionListAttachmentButton.ClearItems();
            currencyBarController.UpdateCurrencyAmount(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, systemConfigurationManager.AuctionDepositAmount, "Deposit:");
            SearchAuctions();
        }

        public override void ReceiveClosedWindowNotification() {
            base.ReceiveClosedWindowNotification();
            if (windowSubscriptionsInitialized == false) {
                return;
            }
            auctionManagerClient.OnSetAuctionItems -= HandleSetAuctionItems;
            auctionManagerClient.OnCancelAuction -= HandleCancelAuction;
            auctionManagerClient.OnBuyAuctionItem -= HandleBuyAuctionItem;
            auctionManagerClient.OnItemListed -= HandleItemListed;
            windowSubscriptionsInitialized = false;
        }

        private void HandleAddAttachment() {
            // calculate the currency entry amount as the total vendor price of all attached items
            int totalVendorPrice = 0;
            foreach (long itemInstanceId in auctionListAttachmentButton.GetItemInstanceIds()) {
                InstantiatedItem instantiatedItem = systemItemManager.GetExistingInstantiatedItem(itemInstanceId);
                if (instantiatedItem == null) {
                    continue;
                }
                KeyValuePair<Currency, int> sellAmount = instantiatedItem.Item.GetSellPrice(instantiatedItem, playerManager.UnitController);
                if (sellAmount.Value == 0 || sellAmount.Key == null) {
                    // don't print a sell price on things that cannot be sold
                    continue;
                }
                if (sellAmount.Key != systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency) {
                    // only count base currency toward auction price
                    continue;
                }
                totalVendorPrice += sellAmount.Value;
            }
            currencyEntryBarController.UpdateCurrencyAmount(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, totalVendorPrice);
        }


        private void HandleItemListed() {
            //Debug.Log($"AuctionPanel.HandleItemListed()");

            currencyEntryBarController.ResetCurrencyAmounts();
            auctionListAttachmentButton.ClearItems();
            
            // if we are looking at our own auctions, refresh the search results to show the new auction
            if (toggle.isOn == true) {
                SearchAuctions();
            }
        }

        private void HandleBuyAuctionItem() {
            //Debug.Log($"AuctionPanel.HandleTakeMailAttachment({messageId})");

            CreatePages();
        }

        private void HandleSetAuctionItems() {
            //PopulatePages();
            CreatePages();
        }

        private void HandleCancelAuction() {
            //PopulatePages();
            CreatePages();
        }

        private void SortAuctionItemDictionary(Dictionary<int, AuctionItemSearchResult> sortedDictionary, bool reverseSort) {
            List<KeyValuePair<int, AuctionItemSearchResult>> sortedList = new List<KeyValuePair<int, AuctionItemSearchResult>>(sortedDictionary);
            if (reverseSort) {
                sortedList.Sort(
                    delegate (KeyValuePair<int, AuctionItemSearchResult> pair1,
                              KeyValuePair<int, AuctionItemSearchResult> pair2) {
                                  if (currentSortType == AuctionSortField.Price) {
                                      return pair2.Value.CurrencyAmount.CompareTo(pair1.Value.CurrencyAmount);
                                  } else if (currentSortType == AuctionSortField.Amount) {
                                      return pair2.Value.Items.Count.CompareTo(pair1.Value.Items.Count);
                                  } else if (currentSortType == AuctionSortField.Seller) {
                                      return pair2.Value.SellerName.CompareTo(pair1.Value.SellerName);
                                  }
                                  return pair2.Value.Items[0].DisplayName.CompareTo(pair1.Value.Items[0].DisplayName);
                              }
                );
            } else {
                sortedList.Sort(
                    delegate (KeyValuePair<int, AuctionItemSearchResult> pair1,
                              KeyValuePair<int, AuctionItemSearchResult> pair2) {
                                  if (currentSortType == AuctionSortField.Price) {
                                      return pair1.Value.CurrencyAmount.CompareTo(pair2.Value.CurrencyAmount);
                                  } else if (currentSortType == AuctionSortField.Amount) {
                                      return pair1.Value.Items.Count.CompareTo(pair2.Value.Items.Count);
                                  } else if (currentSortType == AuctionSortField.Seller) {
                                      return pair1.Value.SellerName.CompareTo(pair2.Value.SellerName);
                                  }
                                  return pair1.Value.Items[0].DisplayName.CompareTo(pair2.Value.Items[0].DisplayName);
                              }
                );
            }
            // Clear the original dictionary and repopulate it with the sorted values
            sortedDictionary.Clear();
            int sortOrder = 0;
            foreach (KeyValuePair<int, AuctionItemSearchResult> pair in sortedList) {
                sortedDictionary.Add(sortOrder, pair.Value);
                sortOrder++;
            }

        }

        protected override void PopulatePages() {
            //Debug.Log("AuctionPanel.PopulatePages()");

            pages.Clear();
            AuctionContentList page = new AuctionContentList();

            Dictionary<int, AuctionItemSearchResult> sortedDictionary = new Dictionary<int, AuctionItemSearchResult>();
            int sortOrder = 0;
            foreach (AuctionItemSearchResult auctionItem in auctionManagerClient.AuctionItems.Values) {
                sortedDictionary.Add(sortOrder, auctionItem);
                sortOrder++;
            }
            SortAuctionItemDictionary(sortedDictionary, reverseSort);

            for (int i = 0; i < sortedDictionary.Count; i++) {
                AuctionItemSearchResult auctionItem = sortedDictionary[i];
                page.auctionItems.Add(auctionItem);
                if (page.auctionItems.Count == pageSize) {
                    pages.Add(page);
                    page = new AuctionContentList();
                }
            }
            if (page.auctionItems.Count > 0) {
                pages.Add(page);
            }
            AddAuctionItems();
        }



        public void AddAuctionItems() {
            //Debug.Log("AuctionPanel.AddMessages()");

            bool foundButton = false;
            if (pages.Count > 0) {
                if (pageIndex >= pages.Count) {
                    pageIndex = pages.Count - 1;
                }
                for (int i = 0; i < pageSize; i++) {
                    //for (int i = 0; i < pages[pageIndex].Count - 1; i++) {
                    //Debug.Log($"CurrencyPanelUI.AddCurrencies() i: {i} pageIndex: {pageIndex}");
                    if (i < (pages[pageIndex] as AuctionContentList).auctionItems.Count) {
                        //Debug.Log($"CurrencyPanelUI.AddCurrencies() i: {i} pageIndex: {pageIndex} adding button");
                        auctionItemButtons[i].gameObject.SetActive(true);
                        auctionItemButtons[i].AddAuctionItem((pages[pageIndex] as AuctionContentList).auctionItems[i]);
                        foundButton = true;
                    } else {
                        //Debug.Log($"CurrencyPanelUI.AddCurrencies() i: {i} pageIndex: {pageIndex} clearing button");
                        auctionItemButtons[i].ClearButton();
                        auctionItemButtons[i].gameObject.SetActive(false);
                    }
                }
            }

            if (foundButton) {
                auctionButtonsNavigationController.FocusFirstButton();
                SetNavigationController(auctionButtonsNavigationController);
            }
        }

        public override void AddPageContent() {
            //Debug.Log("AuctionPanel.AddPageContent()");

            base.AddPageContent();
            AddAuctionItems();
        }

        public override void ClearButtons() {
            //Debug.Log("AuctionPanel.ClearButtons()");

            base.ClearButtons();
            foreach (AuctionItemButton btn in auctionItemButtons) {
                btn.gameObject.SetActive(false);
            }
        }

        public void ListItem() {
            //Debug.Log("AuctionPanel.ListItem()");

            if (auctionListAttachmentButton.GetItemInstanceIds().Count == 0) {
                uiManager.AdvertiseConfirmationPopup("You must attach at least one item to list an auction.");
                return;
            }

            if (currencyEntryBarController.CurrencyNode.Amount <= 0) {
                uiManager.AdvertiseConfirmationPopup("You must enter a valid listing price to list an auction.");
                return;
            }


            auctionManagerClient.SetListItem(auctionListAttachmentButton.GetItemInstanceIds(), currencyEntryBarController.CurrencyNode.Amount);
            uiManager.confirmListAuctionWindow.OpenWindow();
        }

        public void SearchAuctions() {
            //Debug.Log("AuctionPanel.SearchAuctions()");

            string searchText = searchInput.text;
            bool onlyShowOwnAuctions = toggle.isOn;
            auctionManagerClient.RequestSearchAuctions(searchText, onlyShowOwnAuctions);
        }

        public void SortByAmount() {
            reverseSort = !reverseSort;
            currentSortType = AuctionSortField.Amount;
            CreatePages();
        }

        public void SortByName() {
            reverseSort = !reverseSort;
            currentSortType = AuctionSortField.Name;
            CreatePages();
        }

        public void SortBySeller() {
            reverseSort = !reverseSort;
            currentSortType= AuctionSortField.Seller;
            CreatePages();
        }

        public void SortByPrice() {
            reverseSort = !reverseSort;
            currentSortType = AuctionSortField.Price;
            CreatePages();
        }

    }

    public class AuctionContentList : PagedContentList {
        public List<AuctionItemSearchResult> auctionItems = new List<AuctionItemSearchResult>();
    }

    public enum AuctionSortField {
        Name,
        Amount,
        Seller,
        Price
    }
}