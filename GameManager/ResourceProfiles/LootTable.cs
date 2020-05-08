using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Loot Table", menuName = "AnyRPG/LootTable")]
    [System.Serializable]
    public class LootTable : DescribableResource {

        [Tooltip("If set to true, the items on this list will ignore any parent drop limits.")]
        [SerializeField]
        private bool ignoreGlobalDropLimit = false;

        [Tooltip("The maximum amount of items that can drop")]
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
                if (SystemResourceManager.MatchResource(lootDrop.MyItem.MyName, itemName)) {
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
                // unlimited drops settins for this loot group
                int lootGroupRemainingDrops = lootGroup.DropLimit;
                bool lootGroupUnlimitedDrops = (lootGroup.DropLimit == 0);

                // ignore drop limit settings for this loot group
                bool ignoreDropLimit = ignoreGlobalDropLimit;
                if (lootGroup.IgnoreGlobalDropLimit == true) {
                    ignoreDropLimit = true;
                }
                foreach (Loot item in lootGroup.Loot) {
                    if (item.MyItem.MyUniqueItem == true && InventoryManager.MyInstance.GetItemCount(item.MyItem.MyName) > 0) {
                        //Debug.Log("LootTable.RollLoot(): " + item.MyItem.MyName + " skipping due to uniqueness");
                    }
                    if (item.MyPrerequisitesMet == true && (item.MyItem.MyUniqueItem == false || (InventoryManager.MyInstance.GetItemCount(item.MyItem.MyName) == 0 && PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager.HasEquipment(item.MyItem.MyName) == false))) {
                        int roll = Random.Range(0, 100);
                        if (roll <= item.MyDropChance) {
                            int itemCount = Random.Range(item.MyMinDrops, item.MyMaxDrops + 1);
                            //Debug.Log("GatherLootTable.RollLoot(): itemCount: " + itemCount);
                            for (int i = 0; i < itemCount; i++) {
                                //MyDroppedItems.Add(new LootDrop(Instantiate(_loot.MyItem), this));
                                droppedItems.Add(new LootDrop(SystemItemManager.MyInstance.GetNewResource(item.MyItem.MyName), this));
                                if (lootGroupUnlimitedDrops == false && ignoreDropLimit == false) {
                                    lootGroupRemainingDrops -= 1;
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
                        }
                    } else {
                        //Debug.Log("LootTable.RollLoot(): " + item.MyItem.MyName + " prereqs not met");
                    }
                    if ((lootGroupUnlimitedDrops == false && lootGroupRemainingDrops <= 0) || (lootTableUnlimitedDrops == false && lootTableRemainingDrops <= 0)) {
                        break;
                    }
                }
                if (lootTableUnlimitedDrops == false && lootTableRemainingDrops <= 0) {
                    break;
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

            [Tooltip("The amount of items that can drop from this list.  0 is unlimited.")]
            [SerializeField]
            private int dropLimit = 0;

            [Tooltip("If set to true, the items on this list will ignore any parent drop limits.")]
            [SerializeField]
            private bool ignoreGlobalDropLimit = false;

            [SerializeField]
            private List<Loot> loot = new List<Loot>();

            public List<Loot> Loot { get => loot; set => loot = value; }
            public int DropLimit { get => dropLimit; set => dropLimit = value; }
            public bool IgnoreGlobalDropLimit { get => ignoreGlobalDropLimit; set => ignoreGlobalDropLimit = value; }
        }

    }

}