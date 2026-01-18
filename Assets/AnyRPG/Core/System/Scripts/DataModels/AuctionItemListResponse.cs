using System;
using System.Collections.Generic;

namespace AnyRPG {
    
    [Serializable]
    public class AuctionItemListResponse {
        public List<AuctionItemSerializedSearchResult> AuctionItems = new List<AuctionItemSerializedSearchResult>();
        public ItemInstanceListSaveData ItemInstanceListSaveData = new ItemInstanceListSaveData();

        public AuctionItemListResponse() { }

        public void BundleItems(SystemItemManager systemItemManager) {
            foreach (AuctionItemSerializedSearchResult auctionItem in AuctionItems) {
                foreach (int itemInstanceId in auctionItem.ItemIds) {
                    InstantiatedItem instantiatedItem = systemItemManager.GetExistingInstantiatedItem(itemInstanceId);
                    ItemInstanceListSaveData.ItemInstances.Add(instantiatedItem.GetItemSaveData());
                }
            }
        }

    }
}