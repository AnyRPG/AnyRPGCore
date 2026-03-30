using System;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {

    [Serializable]
    public class InteractableSaveData {
        public List<LootableNodeSaveData> LootableNodeSaveData = new List<LootableNodeSaveData>();
        public List<LootableCharacterSaveData> LootableCharacterSaveData = new List<LootableCharacterSaveData>();
        public ItemInstanceListSaveData ItemInstanceListSaveData = new ItemInstanceListSaveData();
        public List<MoveableObjectSaveData> MoveableObjectSaveData = new List<MoveableObjectSaveData>();
        public List<AnimatedObjectSaveData> AnimatedObjectSaveData = new List<AnimatedObjectSaveData>();

        public void BundleItems(SystemItemManager systemItemManager) {
            // bundle items from lootable character and lootable node into one list to be saved with the interactable
            if (LootableCharacterSaveData.Count > 0) {
                foreach (LootDropSerializedData lootDropSerializedData in LootableCharacterSaveData[0].LootDropSerializedDataList) {
                    InstantiatedItem instantiatedItem = systemItemManager.GetExistingInstantiatedItem(lootDropSerializedData.ItemInstanceId);
                    if (instantiatedItem == null) {
                        Debug.LogWarning($"InteractableSaveData.BundleItems() Item with instanceId {lootDropSerializedData.ItemInstanceId} not found!");
                        continue;
                    }
                    ItemInstanceListSaveData.ItemInstances.Add(instantiatedItem.GetItemSaveData());
                }
            }
            if (LootableNodeSaveData.Count > 0) {
                foreach (LootTableStateSerializedData lootTableStateSerializedData in LootableNodeSaveData[0].LootHolderSerializedData.LootTableStateSerializedDataList) {
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

    [Serializable]
    public class MoveableObjectSaveData {
        public bool ObjectOpen;
        public float ObjectPositionX;
        public float ObjectPositionY;
        public float ObjectPositionZ;
        public float ObjectRotationX;
        public float ObjectRotationY;
        public float ObjectRotationZ;
        public float ObjectRotationW;
    }

    [Serializable]
    public class AnimatedObjectSaveData { 
        public bool ObjectOpen;
    }

}
