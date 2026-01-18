using System;
using System.Collections.Generic;

namespace AnyRPG {

    [Serializable]
    public class AuctionItemSerializedSearchResult {
        public int SellerPlayerCharacterId = 0;
        public string SellerName = string.Empty;
        public int AuctionItemId = 0;
        public List<int> ItemIds = new List<int>();
        public int CurrencyAmount = 0;

        public AuctionItemSerializedSearchResult() { }

        public AuctionItemSerializedSearchResult(AuctionItem auctionItem, string sellerName) {
            this.SellerPlayerCharacterId = auctionItem.SellerPlayerCharacterId;
            this.SellerName = sellerName;
            this.AuctionItemId = auctionItem.AuctionItemId;
            this.CurrencyAmount = auctionItem.CurrencyAmount;
            this.ItemIds = auctionItem.ItemIds;
        }

        public AuctionItemSerializedSearchResult(ListAuctionItemRequest listAuctionItemRequest, int playerCharacterId) {
            this.SellerPlayerCharacterId = playerCharacterId;
            this.CurrencyAmount = listAuctionItemRequest.CurrencyAmount;
            this.ItemIds = listAuctionItemRequest.ItemIds;
        }
    }

}
