using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    public class AuctionItemSearchResult {
        public int SellerPlayerCharacterId = 0;
        public string SellerName = string.Empty;
        public int AuctionItemId = 0;
        public List<InstantiatedItem> Items = new List<InstantiatedItem>();
        public int CurrencyAmount = 0;

        public AuctionItemSearchResult() { }

        public AuctionItemSearchResult(AuctionItemSerializedSearchResult auctionItemSerializedSearchResult, SystemItemManager systemItemManager) {
            this.SellerPlayerCharacterId = auctionItemSerializedSearchResult.SellerPlayerCharacterId;
            this.SellerName = auctionItemSerializedSearchResult.SellerName;
            this.AuctionItemId = auctionItemSerializedSearchResult.AuctionItemId;
            this.CurrencyAmount = auctionItemSerializedSearchResult.CurrencyAmount;
            foreach (int itemId in auctionItemSerializedSearchResult.ItemIds) {
                InstantiatedItem item = systemItemManager.GetExistingInstantiatedItem(itemId);
                if (item != null) {
                    Items.Add(item);
                } else {
                    Debug.LogWarning("AuctionItemSearchResult.AuctionItemSearchResult(): Could not find item with id " + itemId + " for auction item " + AuctionItemId);
                }
            }
        }

    }

}
