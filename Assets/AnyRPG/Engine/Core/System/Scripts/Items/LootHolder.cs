using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    public class LootHolder {

        private Dictionary<LootTable, LootTableState> lootTableStates = new Dictionary<LootTable, LootTableState>();

        public Dictionary<LootTable, LootTableState> LootTableStates { get => lootTableStates; set => lootTableStates = value; }
    }
}