using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Loot Table", menuName = "AnyRPG/LootTable")]
    [System.Serializable]
    public class LootTable : DescribableResource {

        [Header("Loot Table")]

        [Tooltip("If set to true, the items on this list will ignore any parent drop limits.")]
        [SerializeField]
        private bool ignoreGlobalDropLimit = false;

        [Tooltip("The maximum amount of items that can drop. 0 is unlimited")]
        [SerializeField]
        protected int dropLimit = 0;

        // keep track of remaining drops if there is a drop limit
        protected int lootTableRemainingDrops = 0;

        [SerializeField]
        protected List<LootGroup> lootGroups = new List<LootGroup>();

        private List<LootDrop> droppedItems = new List<LootDrop>();

        protected bool rolled = false;

        public List<LootDrop> MyDroppedItems { get => droppedItems; set => droppedItems = value; }

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
                if ((lootDrop as ItemLootDrop) is ItemLootDrop) {

                }
                if (SystemResourceManager.MatchResource((lootDrop as ItemLootDrop).MyItem.MyDisplayName, itemName)) {
                    return true;
                }
            }
            return false;
        }

        protected virtual void RollLoot() {
            //Debug.Log(gameObject.name + ".LootTable.RollLoot()");
            int lootTableRemainingDrops = dropLimit;
            bool lootTableUnlimitedDrops = (dropLimit == 0);

            foreach (LootGroup lootGroup in lootGroups) {

                // check if this group can drop an item
                float randomInt = UnityEngine.Random.Range(0, 100);
                if (lootGroup.GroupChance > randomInt) {
                    // unlimited drops settins for this loot group
                    int lootGroupRemainingDrops = lootGroup.DropLimit;
                    bool lootGroupUnlimitedDrops = (lootGroup.DropLimit == 0);

                    // ignore drop limit settings for this loot group
                    bool ignoreDropLimit = ignoreGlobalDropLimit;
                    if (lootGroup.IgnoreGlobalDropLimit == true) {
                        ignoreDropLimit = true;
                    }
                    if (lootGroup.GuaranteedDrop == true) {
                        List<int> randomItemIndexes = new List<int>();
                        int maxCount = Mathf.Min(lootGroup.DropLimit, lootGroup.Loot.Count);
                        while (randomItemIndexes.Count < maxCount) {

                            // pure random
                            //int randomNumber = UnityEngine.Random.Range(0, lootGroup.Loot.Count);

                            // weighted
                            int usedIndex = 0;
                            int sum_of_weight = 0;
                            int accumulatedWeight = 0;

                            for (int i = 0; i < lootGroup.Loot.Count; i++) {
                                sum_of_weight += (int)lootGroup.Loot[i].MyDropChance;
                            }
                            //Debug.Log(MyName + ".Item.InitilizeNewItem(): sum_of_weight: " + sum_of_weight);
                            int rnd = UnityEngine.Random.Range(0, sum_of_weight);
                            //Debug.Log(MyName + ".Item.InitilizeNewItem(): sum_of_weight: " + sum_of_weight + "; rnd: " + rnd);
                            for (int i = 0; i < lootGroup.Loot.Count; i++) {
                                //Debug.Log(MyName + ".Item.InitilizeNewItem(): weightCompare: " + validItemQualities[i].RandomWeight + "; rnd: " + rnd);
                                accumulatedWeight += (int)lootGroup.Loot[i].MyDropChance;
                                if (rnd < accumulatedWeight) {
                                    usedIndex = i;
                                    //Debug.Log(MyName + ".Item.InitilizeNewItem(): break");
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
                            droppedItems.AddRange(GetLootDrop(lootGroup.Loot[randomItemIndex], lootGroupUnlimitedDrops, ignoreDropLimit, lootTableUnlimitedDrops, ref lootGroupRemainingDrops));
                        }
                    } else {
                        foreach (Loot item in lootGroup.Loot) {
                            if (item.MyItem.MyUniqueItem == true && InventoryManager.MyInstance.GetItemCount(item.MyItem.MyDisplayName) > 0) {
                                //Debug.Log("LootTable.RollLoot(): " + item.MyItem.MyName + " skipping due to uniqueness");
                            }
                            if (item.MyPrerequisitesMet == true && (item.MyItem.MyUniqueItem == false || (InventoryManager.MyInstance.GetItemCount(item.MyItem.MyDisplayName) == 0 && PlayerManager.MyInstance.MyCharacter.CharacterEquipmentManager.HasEquipment(item.MyItem.MyDisplayName) == false))) {
                                int roll = Random.Range(0, 100);
                                if (roll <= item.MyDropChance) {
                                    droppedItems.AddRange(GetLootDrop(item, lootGroupUnlimitedDrops, ignoreDropLimit, lootTableUnlimitedDrops, ref lootGroupRemainingDrops));
                                }
                            } else {
                                //Debug.Log("LootTable.RollLoot(): " + item.MyItem.MyName + " prereqs not met");
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
        }


        public List<ItemLootDrop> GetLootDrop(Loot loot, bool lootGroupUnlimitedDrops, bool ignoreDropLimit, bool lootTableUnlimitedDrops, ref int lootGroupRemainingDrops) {
            List<ItemLootDrop> returnValue = new List<ItemLootDrop>();
            int itemCount = Random.Range(loot.MyMinDrops, loot.MyMaxDrops + 1);
            //Debug.Log("GatherLootTable.RollLoot(): itemCount: " + itemCount);
            for (int i = 0; i < itemCount; i++) {
                //MyDroppedItems.Add(new LootDrop(Instantiate(_loot.MyItem), this));
                ItemLootDrop droppedItem = new ItemLootDrop(SystemItemManager.MyInstance.GetNewResource(loot.MyItem.MyDisplayName), this);
                droppedItem.MyItem.DropLevel = PlayerManager.MyInstance.MyCharacter.CharacterStats.Level;
                droppedItems.Add(droppedItem);
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
            //droppedItems.Add(new LootDrop(SystemItemManager.MyInstance.GetNewResource(item.MyItem.MyName), this));

            return returnValue;
        }

        public void HandleRevive() {
            MyDroppedItems.Clear();
            Reset();
        }

        public void Reset() {
            rolled = false;
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            if (lootGroups != null) {
                foreach (LootGroup lootGroup in lootGroups) {
                    foreach (Loot tmpLoot in lootGroup.Loot) {
                        tmpLoot.SetupScriptableObjects();
                    }
                }
            }
        }

        [System.Serializable]
        public class LootGroup {

            [Tooltip("If true, <dropLimit> items will be randomly chosen from the list, using their drop chances as weights")]
            [SerializeField]
            private bool guaranteedDrop = false;

            [Tooltip("The chance that this group can attempt to drop items")]
            [SerializeField]
            private float groupChance = 100f;

            [Tooltip("The amount of items that can drop from this list.  0 is unlimited.")]
            [SerializeField]
            private int dropLimit = 0;

            [Tooltip("The limit to the number of times the same item can drop.  0 is unlimited.")]
            [SerializeField]
            private int uniqueLimit = 0;

            [Tooltip("If set to true, the items on this list will ignore any parent drop limits.")]
            [SerializeField]
            private bool ignoreGlobalDropLimit = false;

            [SerializeField]
            private List<Loot> loot = new List<Loot>();

            public List<Loot> Loot { get => loot; set => loot = value; }
            public int DropLimit { get => dropLimit; set => dropLimit = value; }
            public bool IgnoreGlobalDropLimit { get => ignoreGlobalDropLimit; set => ignoreGlobalDropLimit = value; }
            public bool GuaranteedDrop { get => guaranteedDrop; set => guaranteedDrop = value; }
            public float GroupChance { get => groupChance; set => groupChance = value; }
            public int UniqueLimit { get => uniqueLimit; set => uniqueLimit = value; }
        }

    }

}