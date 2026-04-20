using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class ContainerLootTable {

        [Header("Loot Table")]

        /*
        [Tooltip("If set to true, the items on this list will ignore any parent drop limits.")]
        [SerializeField]
        private bool ignoreGlobalDropLimit = false;
        */

        [Tooltip("The maximum amount of items that can drop. 0 is unlimited")]
        [SerializeField]
        private int dropLimit = 0;

        [SerializeField]
        protected List<ContainerLootGroup> lootGroups = new List<ContainerLootGroup>();

        public List<ContainerLootGroup> LootGroups { get => lootGroups; set => lootGroups = value; }
        public int DropLimit { get => dropLimit; }
        //public bool IgnoreGlobalDropLimit { get => ignoreGlobalDropLimit; }

        public void SetupScriptableObjects(SystemGameManager systemGameManager) {

            if (lootGroups != null) {
                //Debug.Log($"LootTable.SetupScriptableObjects(): setting up loot groups for {ResourceName}");
                foreach (ContainerLootGroup lootGroup in lootGroups) {
                    foreach (ContainerLoot tmpLoot in lootGroup.Loot) {
                        tmpLoot.SetupScriptableObjects(systemGameManager);
                    }
                }
            }

        }
    }

}