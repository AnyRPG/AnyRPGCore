using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class AuctionManagerClient : InteractableOptionManager {

        public event Action OnSetAuctionItems = delegate { };
        public event Action OnItemListed = delegate { };
        public event Action OnCancelAuction = delegate { };
        public event Action OnBuyAuctionItem = delegate { };

        private ListAuctionItemRequest listAuctionItemRequest = null;

        private AuctionComponent auctionComponent = null;

        private AuctionItemSearchResult cancelAuctionItem = null;
        private AuctionItemSearchResult buyAuctionItem = null;

        private Dictionary<int, AuctionItemSearchResult> auctionItems = new Dictionary<int, AuctionItemSearchResult>();

        // game manager references
        MessageFeedManager messageFeedManager = null;

        public AuctionComponent AuctionComponent { get => auctionComponent; set => auctionComponent = value; }
        public ListAuctionItemRequest ListAuctionItemRequest { get => listAuctionItemRequest; }
        public Dictionary<int, AuctionItemSearchResult> AuctionItems { get => auctionItems; }
        public AuctionItemSearchResult CancelAuctionItem { get => cancelAuctionItem; set => cancelAuctionItem = value; }
        public AuctionItemSearchResult BuyAuctionItem { get => buyAuctionItem; set => buyAuctionItem = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            //networkManagerClient.OnClientConnectionStarted += HandleClientConnectionStarted;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            messageFeedManager = systemGameManager.UIManager.MessageFeedManager;
        }

        public void SetProps(AuctionComponent auctionComponent, int componentIndex, int choiceIndex) {
            this.auctionComponent = auctionComponent;

            BeginInteraction(auctionComponent, componentIndex, choiceIndex);
        }

        public override void EndInteraction() {
            base.EndInteraction();
            auctionComponent = null;
        }

        public void ListAuctionItems() {
            //Debug.Log($"AuctionManagerClient.ListAuctionItems()");

            networkManagerClient.RequestListAuctionItems(auctionComponent.Interactable, componentIndex, listAuctionItemRequest);
        }

        public void SetAuctionItems(AuctionItemSearchListResult auctionItemListResponse) {
            //Debug.Log($"AuctionManagerClient.SetAuctionItems(count: {auctionItemListResponse.AuctionItems.Count})");

            auctionItems.Clear();
            systemItemManager.LoadItemInstanceListSaveData(auctionItemListResponse.ItemInstanceListSaveData);
            foreach (AuctionItemSerializedSearchResult auctionItem in auctionItemListResponse.AuctionItems) {
                auctionItems.Add(auctionItem.AuctionItemId, new AuctionItemSearchResult(auctionItem, systemItemManager));
            }
            OnSetAuctionItems();
        }

        public void SetCancelAuctionItem(AuctionItemSearchResult auctionItem) {
            //Debug.Log($"AuctionManagerClient.SetCancelAuctionItem(auctionItemId: {auctionItem.AuctionItemId})");

            cancelAuctionItem = auctionItem;
        }

        public void SetBuyAuctionItem(AuctionItemSearchResult auctionItem) {
            //Debug.Log($"AuctionManagerClient.SetBuyAuctionItem(auctionItemId: {auctionItem.AuctionItemId})");

            buyAuctionItem = auctionItem;
        }

        public void RequestCancelAuction() {
            //Debug.Log($"AuctionManagerClient.RequestCancelAuction()");

            networkManagerClient.RequestCancelAuction(cancelAuctionItem.AuctionItemId);
        }

        public void RequestBuyAuctionItem() {
            //Debug.Log($"AuctionManagerClient.RequestBuyAuctionItem()");

            networkManagerClient.RequestBuyAuctionItem(buyAuctionItem.AuctionItemId);
        }

        public void AdvertiseCancelAuction(int auctionItemId) {
            //Debug.Log($"AuctionManagerClient.AdvertiseCancelAuction(auctionItemId: {auctionItemId})");

            if (auctionItems.ContainsKey (auctionItemId) == false) {
                return;
            }
            auctionItems.Remove(auctionItemId);
            messageFeedManager.WriteMessage("Auction cancelled");
            OnCancelAuction();
        }

        public void AdvertiseListItem() {
            //Debug.Log($"AuctionManagerClient.AdvertiseListItem()");

            OnItemListed();
            messageFeedManager.WriteMessage("Your auction was listed");
        }

        public void SetListItem(List<long> itemInstanceIdList, int amount) {
            //Debug.Log($"AuctionManagerClient.SetListItem(itemIdList.Count: {itemIdList.Count}, amount: {amount})");

            listAuctionItemRequest = new ListAuctionItemRequest() {
                ItemInstanceIds = itemInstanceIdList,
                CurrencyAmount = amount
            };
        }

        public void AdvertiseBuyAuctionItem(int auctionItemId) {
            //Debug.Log($"AuctionManagerClient.AdvertiseBuyAuctionItem(auctionItemId: {auctionItemId})");

            if (auctionItems.ContainsKey(auctionItemId) == false) {
                return;
            }
            auctionItems.Remove(auctionItemId);
            messageFeedManager.WriteMessage("Auction item purchased");
            OnBuyAuctionItem();
        }

        public void RequestSearchAuctions(string searchText, bool onlyShowOwnAuctions) {
            //Debug.Log($"AuctionManagerClient.RequestSearchAuctions(searchText: {searchText}, onlyShowOwnAuctions: {onlyShowOwnAuctions})");

            networkManagerClient.RequestSearchAuctions(auctionComponent.Interactable, componentIndex, searchText, onlyShowOwnAuctions);
        }
    }

}