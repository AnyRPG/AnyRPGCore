using System;
using UnityEngine;

namespace AnyRPG {
    
    public class CreateAuctionItemRequest {
        public string SaveData = string.Empty;

        public CreateAuctionItemRequest(AuctionItem auctionItem) {
            SaveData = JsonUtility.ToJson(auctionItem);
        }
    }
}
