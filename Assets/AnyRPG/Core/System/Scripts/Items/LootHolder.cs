using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;

namespace AnyRPG {
    public class LootHolder : ConfiguredClass {

        public event System.Action<LootDrop, int> OnRemoveDroppedItem = delegate { };
        public event System.Action<InstantiatedItem> OnInitializeItem = delegate { };

        /// <summary>
        /// lootTable, accountId, lootTableState
        /// </summary>
        private Dictionary<LootTable, Dictionary<int, LootTableState>> lootTableStates = new Dictionary<LootTable, Dictionary<int, LootTableState>>();

        // game manager references
        private PlayerManagerServer playerManagerServer = null;
        private LootManager lootManager = null;

        public Dictionary<LootTable, Dictionary<int, LootTableState>> LootTableStates { get => lootTableStates; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManagerServer = systemGameManager.PlayerManagerServer;
            lootManager = systemGameManager.LootManager;
        }
        
        public void InitializeLootTableStates() {
            lootTableStates = new Dictionary<LootTable, Dictionary<int, LootTableState>>();
        }
        

        public void AddLootTableState(LootTable lootTable) {
            lootTableStates.Add(lootTable, new Dictionary<int, LootTableState>());
        }

        public void ClearLootTableStates() {
            lootTableStates.Clear();
        }

        public List<LootDrop> GetLoot(UnitController sourceUnitController, LootTable lootTable, bool rollLoot) {
            Debug.Log($"LootHolder.GetLoot({sourceUnitController?.gameObject.name}, {rollLoot})");

            if (playerManagerServer.ActiveUnitControllerLookup.ContainsKey(sourceUnitController) == false) {
                return new List<LootDrop>();
            }
            int accountId = playerManagerServer.ActiveUnitControllerLookup[sourceUnitController];
            if (lootTableStates.ContainsKey(lootTable) == false) {
                Debug.LogWarning($"LootHolder.GetLoot(): lootTableStates does not contain lootTable");
                return new List<LootDrop>();
            }

            // add account if it does not exist
            if (lootTableStates[lootTable].ContainsKey(accountId) == false) {
                LootTableState lootTableState = new LootTableState(systemGameManager, accountId);
                lootTableState.OnRemoveDroppedItem += HandleRemoveDroppedItem;
                lootTableState.OnInitializeItem += HandleInitializeItem;
                lootTableStates[lootTable].Add(accountId, lootTableState);
            }
            return lootTableStates[lootTable][accountId].GetLoot(sourceUnitController, lootTable, rollLoot);
        }

        private void HandleInitializeItem(InstantiatedItem item) {
            //Debug.Log($"LootHolder.HandleInitializeItem({item.ResourceName})");

            OnInitializeItem(item);
        }

        public void HandleRemoveDroppedItem(LootDrop lootDrop, int accountId) {
            //Debug.Log($"LootHolder.HandleRemoveDroppedItem({lootDrop.InstantiatedItem.ResourceName}, {accountId})");

            OnRemoveDroppedItem(lootDrop, accountId);
        }

        public LootHolderSerializedData GetSerializedData() {
            LootHolderSerializedData returnValue = new LootHolderSerializedData();
            foreach (KeyValuePair<LootTable, Dictionary<int, LootTableState>> lootTableEntry in lootTableStates) {
                foreach (KeyValuePair<int, LootTableState> accountEntry in lootTableEntry.Value) {
                    LootTableStateSerializedData lootTableStateSerializedData = new LootTableStateSerializedData();
                    lootTableStateSerializedData.LootTableName = lootTableEntry.Key.ResourceName;
                    lootTableStateSerializedData.AccountId = accountEntry.Key;
                    foreach (LootDrop lootDrop in accountEntry.Value.DroppedItems) {
                        lootTableStateSerializedData.LootDropSerializedDataList.Add(new LootDropSerializedData(lootDrop.LootDropId, lootDrop.InstantiatedItem.InstanceId));
                    }
                    returnValue.LootTableStateSerializedDataList.Add(lootTableStateSerializedData);
                }
            }
            return returnValue;

        }

        public void LoadFromSerializedData(LootHolderSerializedData lootHolderSerializedData) {
            lootTableStates = new Dictionary<LootTable, Dictionary<int, LootTableState>>();
            foreach (LootTableStateSerializedData lootTableStateSerializedData in lootHolderSerializedData.LootTableStateSerializedDataList) {
                LootTable lootTable = systemDataFactory.GetResource<LootTable>(lootTableStateSerializedData.LootTableName);
                if (lootTable == null) {
                    Debug.LogWarning($"LootHolder.LoadFromSerializedData(): loot table {lootTableStateSerializedData.LootTableName} not found");
                    continue;
                }
                if (lootTableStates.ContainsKey(lootTable) == false) {
                    AddLootTableState(lootTable);
                }
                LootTableState lootTableState = new LootTableState(systemGameManager, lootTableStateSerializedData.AccountId);
                lootTableState.OnRemoveDroppedItem += HandleRemoveDroppedItem;
                lootTableState.OnInitializeItem += HandleInitializeItem;
                foreach (LootDropSerializedData lootDropSerializedData in lootTableStateSerializedData.LootDropSerializedDataList) {
                    InstantiatedItem instantiatedItem = systemItemManager.GetExistingInstantiatedItem(lootDropSerializedData.ItemInstanceId);
                    if (instantiatedItem == null) {
                        Debug.LogWarning($"LootHolder.LoadFromSerializedData(): item with instance id {lootDropSerializedData.ItemInstanceId} not found");
                        continue;
                    }
                    LootDrop lootDrop = lootManager.AddNetworkLootDrop(lootDropSerializedData);
                    lootTableState.DroppedItems.Add(lootDrop);
                }
                lootTableStates[lootTable].Add(lootTableStateSerializedData.AccountId, lootTableState);
            }
        }
    }
}