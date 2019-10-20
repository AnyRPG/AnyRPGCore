using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootTable : MonoBehaviour {

    [SerializeField]
    protected Loot[] loot;

    private List<LootDrop> droppedItems = new List<LootDrop>();

    protected bool rolled = false;

    public List<LootDrop> MyDroppedItems { get => droppedItems; set => droppedItems = value; }

    public Loot[] MyLoot { get => loot; set => loot = value; }

    public List<LootDrop> GetLoot() {
        //Debug.Log("LootTable.GetLoot().");
        if (!rolled) {
            //Debug.Log("LootTable.GetLoot() !rolled. rolling...");
            RollLoot();
        }
        //Debug.Log("LootTable.GetLoot(). MyDroppedItems.Length: " + MyDroppedItems.Count);
        return MyDroppedItems;
    }

    // used to prevent multiple copies of a unique item from dropping since the other check requires it to be in your bag, so multiple can still drop if you haven't looted a mob yet
    public bool droppedItemsContains(string itemName) {
        foreach (LootDrop lootDrop in droppedItems) {
            if (SystemResourceManager.MatchResource(lootDrop.MyItem.MyName, itemName)) {
                return true;
            }
        }
        return false;
    }

    protected virtual void RollLoot() {
        //Debug.Log(gameObject.name + ".LootTable.RollLoot()");
        foreach (Loot item in loot) {
            if (item.MyItem.MyUniqueItem == true && InventoryManager.MyInstance.GetItemCount(item.MyItem.MyName) > 0) {
                //Debug.Log("LootTable.RollLoot(): " + item.MyItem.MyName + " skipping due to uniqueness");
            }
            if (item.MyPrerequisitesMet == true && (item.MyItem.MyUniqueItem == false || (InventoryManager.MyInstance.GetItemCount(item.MyItem.MyName) == 0 && PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager.HasEquipment(item.MyItem.MyName) == false) )) {
                int roll = Random.Range(0, 100);
                if (roll <= item.MyDropChance) {
                    droppedItems.Add(new LootDrop(SystemItemManager.MyInstance.GetNewResource(item.MyItem.MyName), this));
                }
            } else {
                //Debug.Log("LootTable.RollLoot(): " + item.MyItem.MyName + " prereqs not met");
            }
        }

        rolled = true;
    }

    public void HandleRevive() {
        MyDroppedItems.Clear();
        Reset();
    }

    public void Reset() {
        rolled = false;
    }
}
