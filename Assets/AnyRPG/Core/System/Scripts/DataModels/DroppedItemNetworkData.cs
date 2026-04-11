using System;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {

    [Serializable]
    public class DroppedItemNetworkData {
        public List<long> itemInstanceIds = new List<long>();
        public ItemInstanceListSaveData ItemInstanceListSaveData = new ItemInstanceListSaveData();

        public void BundleItems(SystemItemManager systemItemManager) {
            // bundle items from dropped items into one list to be sent across the network
            foreach (long itemInstanceId in itemInstanceIds) {
                InstantiatedItem instantiatedItem = systemItemManager.GetExistingInstantiatedItem(itemInstanceId);
                if (instantiatedItem == null) {
                    Debug.LogWarning($"InteractableSaveData.BundleItems() Item with instanceId {itemInstanceId} not found!");
                    continue;
                }
                ItemInstanceListSaveData.ItemInstances.Add(instantiatedItem.GetItemSaveData());
            }
        }
    }

}
