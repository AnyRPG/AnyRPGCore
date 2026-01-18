using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public Dictionary<LootTable, Dictionary<int, LootTableState>> LootTableStates { get => lootTableStates; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManagerServer = systemGameManager.PlayerManagerServer;
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
            //Debug.Log($"LootHolder.GetLoot({sourceUnitController?.gameObject.name}, {rollLoot})");

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
    }
}