using System;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {

    [Serializable]
    public class InteractableSaveData {
        public LootableNodeSaveData LootableNodeSaveData = new LootableNodeSaveData();
        public LootableCharacterSaveData LootableCharacterSaveData = new LootableCharacterSaveData();
        public ItemInstanceListSaveData ItemInstanceListSaveData = new ItemInstanceListSaveData();

        public void BundleItems(SystemItemManager systemItemManager) {
            // bundle items from lootable character and lootable node into one list to be saved with the interactable
            foreach (LootDropSerializedData lootDropSerializedData in LootableCharacterSaveData.LootDropSerializedDataList) {
                InstantiatedItem instantiatedItem = systemItemManager.GetExistingInstantiatedItem(lootDropSerializedData.ItemInstanceId);
                if (instantiatedItem == null) {
                    Debug.LogWarning($"InteractableSaveData.BundleItems() Item with instanceId {lootDropSerializedData.ItemInstanceId} not found!");
                    continue;
                }
                ItemInstanceListSaveData.ItemInstances.Add(instantiatedItem.GetItemSaveData());
            }
            foreach (LootTableStateSerializedData lootTableStateSerializedData in LootableNodeSaveData.LootHolderSerializedData.LootTableStateSerializedDataList) {
                foreach (LootDropSerializedData lootDropSerializedData in lootTableStateSerializedData.LootDropSerializedDataList) {
                    InstantiatedItem instantiatedItem = systemItemManager.GetExistingInstantiatedItem(lootDropSerializedData.ItemInstanceId);
                    if (instantiatedItem == null) {
                        Debug.LogWarning($"InteractableSaveData.BundleItems() Item with instanceId {lootDropSerializedData.ItemInstanceId} not found!");
                        continue;
                    }
                    ItemInstanceListSaveData.ItemInstances.Add(instantiatedItem.GetItemSaveData());
                }
            }
        }
    }

    [Serializable]
    public class  LootableCharacterSaveData {
        public List<LootDropIdList> LootDropIds = new List<LootDropIdList>();
        public List<LootDropSerializedData> LootDropSerializedDataList = new List<LootDropSerializedData>();
    }

    [Serializable]
    public class LootableNodeSaveData {
        public LootHolderSerializedData LootHolderSerializedData = new LootHolderSerializedData();
        public bool SpawnObjectActive = false;
        public bool LootDropped = false;
        public int PickupCount = 0;
    }

    [Serializable]
    public class LootHolderSerializedData {
        public List<LootTableStateSerializedData> LootTableStateSerializedDataList = new List<LootTableStateSerializedData>();
    }

    [Serializable]
    public class LootTableStateSerializedData {
        public string LootTableName;
        public int AccountId;
        public List<LootDropSerializedData> LootDropSerializedDataList = new List<LootDropSerializedData>();
    }

}
