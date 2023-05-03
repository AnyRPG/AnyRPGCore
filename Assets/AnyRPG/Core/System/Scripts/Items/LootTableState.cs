using AnyRPG;
//using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    public class LootTableState : ConfiguredClass {

        // keep track of remaining drops if there is a drop limit
        private int lootTableRemainingDrops = 0;

        private List<LootDrop> droppedItems = new List<LootDrop>();

        private bool rolled = false;

        // game manager references
        private PlayerManager playerManager = null;
        private SystemItemManager systemItemManager = null;
        private LootManager lootManager = null;

        public List<LootDrop> DroppedItems { get => droppedItems; set => droppedItems = value; }
        public bool Rolled { get => rolled; set => rolled = value; }
        public int LootTableRemainingDrops { get => lootTableRemainingDrops; set => lootTableRemainingDrops = value; }

        public LootTableState(SystemGameManager systemGameManager) {
            Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            systemItemManager = systemGameManager.SystemItemManager;
            lootManager = systemGameManager.LootManager;
        }

        public void RemoveDroppedItem(ItemLootDrop itemLootDrop) {
            droppedItems.Remove(itemLootDrop);
        }

        private void AddDroppedItem(ItemLootDrop droppedItem) {
            droppedItems.Add(droppedItem);
        }

        private void AddDroppedItems(List<ItemLootDrop> itemLootDrops) {
            droppedItems.AddRange(itemLootDrops);
        }

        private void RollLoot(LootTable lootTable) {
            //Debug.Log($"{gameObject.name}.LootTable.RollLoot()");
            int lootTableRemainingDrops = lootTable.DropLimit;
            bool lootTableUnlimitedDrops = (lootTable.DropLimit == 0);

            foreach (LootGroup lootGroup in lootTable.LootGroups) {

                // check if this group can drop an item
                float randomInt = UnityEngine.Random.Range(0, 100);
                if (lootGroup.GroupChance > randomInt) {
                    // unlimited drops settins for this loot group
                    int lootGroupRemainingDrops = lootGroup.DropLimit;
                    bool lootGroupUnlimitedDrops = (lootGroup.DropLimit == 0);

                    // ignore drop limit settings for this loot group
                    bool ignoreDropLimit = lootTable.IgnoreGlobalDropLimit;
                    if (lootGroup.IgnoreGlobalDropLimit == true) {
                        ignoreDropLimit = true;
                    }

                    // get list of loot that is currenly valid to be rolled so that weights can be properly calculated based on only valid loot
                    List<Loot> validLoot = new List<Loot>();
                    foreach (Loot loot in lootGroup.Loot) {
                        if (loot.PrerequisitesMet == true &&
                            (loot.Item.UniqueItem == false || lootManager.CanDropUniqueItem(loot.Item) == true)) {
                            validLoot.Add(loot);
                        }
                    }

                    if (lootGroup.GuaranteedDrop == true) {
                        List<int> randomItemIndexes = new List<int>();
                        // guaranteed drops can never have a 0 drop limit, but shouldn't be unlimited because the chance is not random per item like non guaranteed drops
                        int maxCount = (int)Mathf.Min(Mathf.Clamp(lootGroup.DropLimit, 1, Mathf.Infinity), validLoot.Count);
                        while (randomItemIndexes.Count < maxCount) {

                            // pure random
                            //int randomNumber = UnityEngine.Random.Range(0, lootGroup.Loot.Count);

                            // weighted
                            int usedIndex = 0;
                            int sum_of_weight = 0;
                            int accumulatedWeight = 0;

                            for (int i = 0; i < validLoot.Count; i++) {
                                sum_of_weight += (int)validLoot[i].DropChance;
                            }
                            //Debug.Log(DisplayName + ".Item.InitilizeNewItem(): sum_of_weight: " + sum_of_weight);
                            int rnd = UnityEngine.Random.Range(0, sum_of_weight);
                            //Debug.Log(DisplayName + ".Item.InitilizeNewItem(): sum_of_weight: " + sum_of_weight + "; rnd: " + rnd);
                            for (int i = 0; i < validLoot.Count; i++) {
                                //Debug.Log(DisplayName + ".Item.InitilizeNewItem(): weightCompare: " + validItemQualities[i].RandomWeight + "; rnd: " + rnd);
                                accumulatedWeight += (int)validLoot[i].DropChance;
                                if (rnd < accumulatedWeight) {
                                    usedIndex = i;
                                    //Debug.Log(DisplayName + ".Item.InitilizeNewItem(): break");
                                    break;
                                }
                                //rnd -= validItemQualities[i].RandomWeight;
                            }

                            if (lootGroup.UniqueLimit > 0) {
                                int foundCount = randomItemIndexes.Where(x => x.Equals(usedIndex)).Count();
                                if (foundCount < lootGroup.UniqueLimit) {
                                    randomItemIndexes.Add(usedIndex);
                                }

                            } else {
                                randomItemIndexes.Add(usedIndex);
                            }
                        }
                        foreach (int randomItemIndex in randomItemIndexes) {
                            AddDroppedItems(GetLootDrop(validLoot[randomItemIndex], lootGroupUnlimitedDrops, ignoreDropLimit, lootTableUnlimitedDrops, ref lootGroupRemainingDrops));
                        }
                    } else {
                        foreach (Loot item in validLoot) {
                            //Debug.Log("LootTable.RollLoot(): " + item.MyItem.DisplayName + " rolling");
                            int roll = Random.Range(0, 100);
                            if (roll <= item.DropChance) {
                                AddDroppedItems(GetLootDrop(item, lootGroupUnlimitedDrops, ignoreDropLimit, lootTableUnlimitedDrops, ref lootGroupRemainingDrops));
                            }
                            if ((lootGroupUnlimitedDrops == false && lootGroupRemainingDrops <= 0) || (lootTableUnlimitedDrops == false && lootTableRemainingDrops <= 0)) {
                                break;
                            }
                        }
                    }

                    if (lootTableUnlimitedDrops == false && lootTableRemainingDrops <= 0) {
                        break;
                    }
                }
            }

            rolled = true;
            lootManager.AddLootTableState(this);
        }

        private List<ItemLootDrop> GetLootDrop(Loot loot, bool lootGroupUnlimitedDrops, bool ignoreDropLimit, bool lootTableUnlimitedDrops, ref int lootGroupRemainingDrops) {
            List<ItemLootDrop> returnValue = new List<ItemLootDrop>();
            int itemCount = Random.Range(loot.MinDrops, loot.MaxDrops + 1);
            //Debug.Log("GatherLootTable.RollLoot(): itemCount: " + itemCount);
            for (int i = 0; i < itemCount; i++) {
                ItemLootDrop droppedItem = new ItemLootDrop(systemItemManager.GetNewResource(loot.Item.ResourceName), this, systemGameManager);
                droppedItem.Item.DropLevel = playerManager.MyCharacter.CharacterStats.Level;
                AddDroppedItem(droppedItem);
                if (lootGroupUnlimitedDrops == false && ignoreDropLimit == false) {
                    lootGroupRemainingDrops = lootGroupRemainingDrops - 1;
                    if (lootGroupRemainingDrops <= 0) {
                        break;
                    }
                }
                if (lootTableUnlimitedDrops == false && ignoreDropLimit == false) {
                    lootTableRemainingDrops -= 1;
                    if (lootTableRemainingDrops <= 0) {
                        break;
                    }
                }
            }
            //droppedItems.Add(new LootDrop(systemItemManager.GetNewResource(item.MyItem.DisplayName), this));

            return returnValue;
        }

        public List<LootDrop> GetLoot(LootTable lootTable, bool rollLoot = true) {
            //Debug.Log("LootTable.GetLoot().");
            if (!rolled && rollLoot == true) {
                //Debug.Log("LootTable.GetLoot() !rolled. rolling...");
                RollLoot(lootTable);
            }
            //Debug.Log("LootTable.GetLoot(). MyDroppedItems.Length: " + MyDroppedItems.Count);
            return droppedItems;
        }

        /*
        public void ResetLootTableState() {
            // why would we need to do this.  Looting should only happen once in the lifetime of any mob
            droppedItems.Clear();
            Reset();
        }
        */

        /*
        public void Reset() {
        // why would we need to do this?  Looting should only happen once in the lifetime of any mob
            rolled = false;
        }
        */

    }
}