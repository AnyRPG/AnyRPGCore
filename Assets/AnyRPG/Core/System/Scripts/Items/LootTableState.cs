//using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    public class LootTableState : ConfiguredClass, IInstantiatedItemRequestor {

        public event System.Action<LootDrop, int> OnRemoveDroppedItem = delegate { };
        public event System.Action<InstantiatedItem> OnInitializeItem = delegate { };

        private int accountId = 0;

        // keep track of remaining drops if there is a drop limit
        private int lootTableRemainingDrops = 0;

        private List<LootDrop> droppedItems = new List<LootDrop>();

        private bool rolled = false;

        // game manager references
        private LootManager lootManager = null;

        public List<LootDrop> DroppedItems { get => droppedItems; set => droppedItems = value; }
        public bool Rolled { get => rolled; set => rolled = value; }
        public int LootTableRemainingDrops { get => lootTableRemainingDrops; set => lootTableRemainingDrops = value; }

        public LootTableState(SystemGameManager systemGameManager, int accountId) {
            Configure(systemGameManager);
            this.accountId = accountId;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            lootManager = systemGameManager.LootManager;
        }

        public void RemoveDroppedItem(LootDrop lootDrop) {
            droppedItems.Remove(lootDrop);
            OnRemoveDroppedItem(lootDrop, accountId);
        }

        private void AddDroppedItem(LootDrop droppedItem) {
            droppedItems.Add(droppedItem);
        }

        private void AddDroppedItems(List<LootDrop> lootDrops) {
            droppedItems.AddRange(lootDrops);
        }

        private void RollLoot(UnitController sourceUnitController, LootTable lootTable) {
            //Debug.Log($"LootTableState.RollLoot({sourceUnitController.gameObject.name})");

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
                        if (loot.PrerequisitesMet(sourceUnitController) == true &&
                            (loot.Item.UniqueItem == false || lootManager.CanDropUniqueItem(sourceUnitController, loot.Item) == true)) {
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
                            AddDroppedItems(GetLootDrop(sourceUnitController, validLoot[randomItemIndex], lootGroupUnlimitedDrops, ignoreDropLimit, lootTableUnlimitedDrops, ref lootGroupRemainingDrops));
                        }
                    } else {
                        foreach (Loot item in validLoot) {
                            //Debug.Log("LootTable.RollLoot(): " + item.MyItem.DisplayName + " rolling");
                            int roll = Random.Range(0, 100);
                            if (roll <= item.DropChance) {
                                AddDroppedItems(GetLootDrop(sourceUnitController, item, lootGroupUnlimitedDrops, ignoreDropLimit, lootTableUnlimitedDrops, ref lootGroupRemainingDrops));
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

        private List<LootDrop> GetLootDrop(UnitController sourceUnitController, Loot loot, bool lootGroupUnlimitedDrops, bool ignoreDropLimit, bool lootTableUnlimitedDrops, ref int lootGroupRemainingDrops) {
            //Debug.Log($"LootTableState.GetLootDrop({sourceUnitController.gameObject.name}, {loot.Item.ResourceName}, {lootGroupUnlimitedDrops}, {ignoreDropLimit}, {lootTableRemainingDrops}, {lootGroupRemainingDrops})");
            
            List<LootDrop> returnValue = new List<LootDrop>();
            int itemCount = Random.Range(loot.MinDrops, loot.MaxDrops + 1);
            //Debug.Log("GatherLootTable.RollLoot(): itemCount: " + itemCount);
            for (int i = 0; i < itemCount; i++) {
                int lootDropId = systemGameManager.LootManager.GetLootDropId();
                LootDrop droppedItem = new LootDrop(lootDropId, sourceUnitController.CharacterInventoryManager.GetNewInstantiatedItem(loot.Item, null, this), systemGameManager);
                lootManager.AddLootDropToIndex(sourceUnitController, droppedItem);
                lootManager.AddLootTableStateIndex(droppedItem.LootDropId, this);
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

        public List<LootDrop> GetLoot(UnitController sourceUnitController, LootTable lootTable, bool rollLoot) {
            //Debug.Log($"LootTableState.GetLoot({sourceUnitController.gameObject.name}, {rollLoot})");

            if (!rolled && rollLoot == true) {
                //Debug.Log("LootTable.GetLoot() !rolled. rolling...");
                RollLoot(sourceUnitController, lootTable);
            }
            //Debug.Log("LootTable.GetLoot(). MyDroppedItems.Length: " + MyDroppedItems.Count);
            return droppedItems;
        }

        public void InitializeItem(InstantiatedItem instantiatedItem) {
            //Debug.Log($"LootTableState.InitializeItem({instantiatedItem.Item.ResourceName})");

            OnInitializeItem(instantiatedItem);

            // loot table is the only implementation of IInstantiatedItemRequestor, but only currency loot needs to be created(saved to database for first time)
            if (networkManagerServer.ServerModeActive == true && instantiatedItem.Item == lootManager.CurrencyLootItem) {
                systemItemManager.CreateItemInstance(instantiatedItem);
            }
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