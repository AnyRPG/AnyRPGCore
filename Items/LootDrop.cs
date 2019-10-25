using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
public class LootDrop
{
    public Item MyItem { get; set; }

    public LootTable MyLootTable { get; set; }

    public LootDrop(Item item, LootTable lootTable) {
        MyLootTable = lootTable;
        MyItem = item;
    }

    public void Remove() {
        //Debug.Log("LootDrop.Remove()");
        MyLootTable.MyDroppedItems.Remove(this);
    }
}

}