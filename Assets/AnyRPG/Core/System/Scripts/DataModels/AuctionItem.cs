using System;
using System.Collections.Generic;


namespace AnyRPG {

    [Serializable]
    public class AuctionItem {
        public int SellerPlayerCharacterId = 0;
        public int AuctionItemId = 0;
        public List<int> ItemIds = new List<int>();
        public int CurrencyAmount = 0;

        public AuctionItem() { }

        public AuctionItem(ListAuctionItemRequest listAuctionItemRequest, int playerCharacterId) {
            this.SellerPlayerCharacterId = playerCharacterId;
            this.CurrencyAmount = listAuctionItemRequest.CurrencyAmount;
            this.ItemIds = listAuctionItemRequest.ItemIds;
        }
    }

}
