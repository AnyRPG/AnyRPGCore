using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class ContainerLoot : ConfiguredClass {

        [SerializeField]
        [ResourceSelector(resourceType = typeof(Item))]
        private string itemName = string.Empty;

        //[SerializeField]
        private Item item;

        [SerializeField]
        private float dropChance = 100f;

        [SerializeField]
        private int minDrops = 1;

        [SerializeField]
        private int maxDrops = 1;

        public string ItemName { get => itemName; set => itemName = value; }
        public string DisplayName { get => ItemName; }
        public Item Item { get => item; set => item = value; }
        public float DropChance { get => dropChance; set => dropChance = value; }
        public int MinDrops { get => minDrops; set => minDrops = value; }
        public int MaxDrops { get => maxDrops; set => maxDrops = value; }

        public void SetupScriptableObjects(SystemGameManager systemGameManager) {
            Configure(systemGameManager);
            if (itemName != string.Empty) {
                Item tmpItem = systemDataFactory.GetResource<Item>(itemName);
                if (tmpItem != null) {
                    item = tmpItem;
                } else {
                    Debug.LogError($"Loot.SetupScriptableObjects(): Could not find item : {itemName} while inititalizing a loot.  CHECK INSPECTOR");
                }
            }
        }

    }

}