using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatherLootTable : LootTable
{
    private void Start() {
        //RollLoot();
    }

    protected override void RollLoot() {
        //Debug.Log("GatherLootTable.RollLoot()");
        MyDroppedItems = new List<LootDrop>();

        foreach (Loot _loot in loot) {
            int roll = Random.Range(0, 100);
            if (roll <= _loot.MyDropChance) {
                int itemCount = Random.Range(1, 6);
                //Debug.Log("GatherLootTable.RollLoot(): itemCount: " + itemCount);
                for (int i = 0; i < itemCount; i++) {
                    MyDroppedItems.Add(new LootDrop(Instantiate(_loot.MyItem), this));
                }
            }
        }

        rolled = true;
    }
}
