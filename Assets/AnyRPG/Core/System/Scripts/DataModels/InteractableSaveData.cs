using System;
using System.Collections.Generic;
using System.Net.Mail;
using UnityEngine;


namespace AnyRPG {

    [Serializable]
    public class InteractableSaveData {
        public LootableCharacterSaveData LootableCharacterSaveData = new LootableCharacterSaveData();
        public ItemInstanceListSaveData ItemInstanceListSaveData = new ItemInstanceListSaveData();

        public void BundleItems(SystemItemManager systemItemManager) {
            foreach (LootDropSerializedData lootDropSerializedData in LootableCharacterSaveData.LootDropSerializedDataList) {
                InstantiatedItem instantiatedItem = systemItemManager.GetExistingInstantiatedItem(lootDropSerializedData.ItemInstanceId);
                if (instantiatedItem == null) {
                    Debug.LogWarning($"InteractableSaveData.BundleItems() Item with instanceId {lootDropSerializedData.ItemInstanceId} not found!");
                    continue;
                }
                ItemInstanceListSaveData.ItemInstances.Add(instantiatedItem.GetItemSaveData());
            }
        }
    }

    [Serializable]
    public class  LootableCharacterSaveData {
        public List<LootDropIdList> LootDropIds = new List<LootDropIdList>();
        public List<LootDropSerializedData> LootDropSerializedDataList = new List<LootDropSerializedData>();
    }
}
