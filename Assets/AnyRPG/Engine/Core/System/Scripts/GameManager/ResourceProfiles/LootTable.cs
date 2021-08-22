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

        [SerializeField]
        protected List<LootGroup> lootGroups = new List<LootGroup>();


        public List<LootDrop> GetLoot(LootTableState lootTableState, bool rollLoot = true) {
            //Debug.Log("LootTable.GetLoot().");
            if (!lootTableState.Rolled && rollLoot == true) {
                //Debug.Log("LootTable.GetLoot() !rolled. rolling...");
                RollLoot(lootTableState);
            }
            //Debug.Log("LootTable.GetLoot(). MyDroppedItems.Length: " + MyDroppedItems.Count);
            return lootTableState.DroppedItems;
        }

        /// <summary>
        /// used to prevent multiple copies of a unique item from dropping since the other check requires it to be in your bag, so multiple can still drop if you haven't looted a mob yet 
        /// </summary>
        /// <param name="itemName"></param>
        /// <returns></returns>
        public bool droppedItemsContains(LootTableState lootTableState, string itemName) {
            foreach (LootDrop lootDrop in lootTableState.DroppedItems) {
                if ((lootDrop as ItemLootDrop) is ItemLootDrop) {

                }
                if (SystemDataFactory.MatchResource((lootDrop as ItemLootDrop).Item.DisplayName, itemName)) {
                    return true;
                }
            }
            return false;
        }

        protected virtual void RollLoot(LootTableState lootTableState) {
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

                    // get list of loot that is currenly valid to be rolled so that weights can be properly calculated based on only valid loot
                    List<Loot> validLoot = new List<Loot>();
                    foreach (Loot loot in lootGroup.Loot) {
                        if (loot.MyPrerequisitesMet == true &&
                            (loot.MyItem.MyUniqueItem == false ||
                            (SystemGameManager.Instance.InventoryManager.GetItemCount(loot.MyItem.DisplayName) == 0 &&
                            SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterEquipmentManager.HasEquipment(loot.MyItem.DisplayName) == false))) {
                            validLoot.Add(loot);
                        }/* else {
                            Debug.Log(MyName + ".LootTable.RollLoot() item: " + loot.MyItem);
                        }*/
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
                                sum_of_weight += (int)validLoot[i].MyDropChance;
                            }
                            //Debug.Log(MyName + ".Item.InitilizeNewItem(): sum_of_weight: " + sum_of_weight);
                            int rnd = UnityEngine.Random.Range(0, sum_of_weight);
                            //Debug.Log(MyName + ".Item.InitilizeNewItem(): sum_of_weight: " + sum_of_weight + "; rnd: " + rnd);
                            for (int i = 0; i < validLoot.Count; i++) {
                                //Debug.Log(MyName + ".Item.InitilizeNewItem(): weightCompare: " + validItemQualities[i].RandomWeight + "; rnd: " + rnd);
                                accumulatedWeight += (int)validLoot[i].MyDropChance;
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
                            lootTableState.DroppedItems.AddRange(GetLootDrop(lootTableState, validLoot[randomItemIndex], lootGroupUnlimitedDrops, ignoreDropLimit, lootTableUnlimitedDrops, ref lootGroupRemainingDrops));
                        }
                    } else {
                        foreach (Loot item in validLoot) {
                            //Debug.Log("LootTable.RollLoot(): " + item.MyItem.MyName + " rolling");
                            int roll = Random.Range(0, 100);
                            if (roll <= item.MyDropChance) {
                                lootTableState.DroppedItems.AddRange(GetLootDrop(lootTableState, item, lootGroupUnlimitedDrops, ignoreDropLimit, lootTableUnlimitedDrops, ref lootGroupRemainingDrops));
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

            lootTableState.Rolled = true;
        }


        public List<ItemLootDrop> GetLootDrop(LootTableState lootTableState, Loot loot, bool lootGroupUnlimitedDrops, bool ignoreDropLimit, bool lootTableUnlimitedDrops, ref int lootGroupRemainingDrops) {
            List<ItemLootDrop> returnValue = new List<ItemLootDrop>();
            int itemCount = Random.Range(loot.MyMinDrops, loot.MyMaxDrops + 1);
            //Debug.Log("GatherLootTable.RollLoot(): itemCount: " + itemCount);
            for (int i = 0; i < itemCount; i++) {
                ItemLootDrop droppedItem = new ItemLootDrop(SystemGameManager.Instance.SystemItemManager.GetNewResource(loot.MyItem.DisplayName), lootTableState);
                droppedItem.Item.DropLevel = SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.Level;
                lootTableState.DroppedItems.Add(droppedItem);
                if (lootGroupUnlimitedDrops == false && ignoreDropLimit == false) {
                    lootGroupRemainingDrops = lootGroupRemainingDrops - 1;
                    if (lootGroupRemainingDrops <= 0) {
                        break;
                    }
                }
                if (lootTableUnlimitedDrops == false && ignoreDropLimit == false) {
                    lootTableState.LootTableRemainingDrops -= 1;
                    if (lootTableState.LootTableRemainingDrops <= 0) {
                        break;
                    }
                }
            }
            //droppedItems.Add(new LootDrop(SystemGameManager.Instance.SystemItemManager.GetNewResource(item.MyItem.MyName), this));

            return returnValue;
        }

        public void HandleRevive(LootTableState lootTableState) {
            lootTableState.DroppedItems.Clear();
            Reset(lootTableState);
        }

        public void Reset(LootTableState lootTableState) {
            lootTableState.Rolled = false;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            if (lootGroups != null) {
                foreach (LootGroup lootGroup in lootGroups) {
                    foreach (Loot tmpLoot in lootGroup.Loot) {
                        tmpLoot.SetupScriptableObjects(systemDataFactory);
                    }
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