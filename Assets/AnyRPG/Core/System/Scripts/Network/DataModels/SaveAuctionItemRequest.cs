using UnityEngine;

namespace AnyRPG {
    
    public class SaveAuctionItemRequest {
        public int Id;
        public string SaveData;

        public SaveAuctionItemRequest(int guildId, AuctionItem auctionItem) {
            Id = guildId;
            SaveData = JsonUtility.ToJson(auctionItem);
        }
    }
}
