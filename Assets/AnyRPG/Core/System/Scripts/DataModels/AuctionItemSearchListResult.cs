using System;
using System.Collections.Generic;

namespace AnyRPG {
    
    [Serializable]
    public class AuctionItemSearchListResult {
        public List<AuctionItemSerializedSearchResult> AuctionItems = new List<AuctionItemSerializedSearchResult>();
        public ItemInstanceListSaveData ItemInstanceListSaveData = new ItemInstanceListSaveData();

        public AuctionItemSearchListResult() { }

        public void BundleItems(SystemItemManager systemItemManager) {
            foreach (AuctionItemSerializedSearchResult auctionItem in AuctionItems) {
                foreach (long itemInstanceId in auctionItem.ItemInstanceIds) {
                    InstantiatedItem instantiatedItem = systemItemManager.GetExistingInstantiatedItem(itemInstanceId);
                    ItemInstanceListSaveData.ItemInstances.Add(instantiatedItem.GetItemSaveData());
                }
            }
        }

    }
}