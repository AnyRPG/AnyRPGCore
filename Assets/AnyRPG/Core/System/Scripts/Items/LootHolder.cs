using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    public class LootHolder {

        private Dictionary<LootTable, LootTableState> lootTableStates = null;

        public Dictionary<LootTable, LootTableState> LootTableStates { get => lootTableStates; }

        public LootHolder() {
            InitializeLootTableStates();
        }

        public void InitializeLootTableStates() {
            lootTableStates = new Dictionary<LootTable, LootTableState>();
        }

        public void AddLootTableState(LootTable lootTable, LootTableState lootTableState) {
            lootTableStates.Add(lootTable, lootTableState);
        }

        public void ClearLootTableStates() {
            lootTableStates.Clear();
        }
    }
}