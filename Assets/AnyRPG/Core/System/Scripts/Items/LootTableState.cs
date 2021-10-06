using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    public class LootTableState {

        // keep track of remaining drops if there is a drop limit
        private int lootTableRemainingDrops = 0;

        private List<LootDrop> droppedItems = new List<LootDrop>();

        private bool rolled = false;

        public List<LootDrop> DroppedItems { get => droppedItems; set => droppedItems = value; }
        public bool Rolled { get => rolled; set => rolled = value; }
        public int LootTableRemainingDrops { get => lootTableRemainingDrops; set => lootTableRemainingDrops = value; }

        public void ResetLootTableState() {
            DroppedItems.Clear();
            Reset();
        }

        public void Reset() {
            Rolled = false;
        }

    }
}