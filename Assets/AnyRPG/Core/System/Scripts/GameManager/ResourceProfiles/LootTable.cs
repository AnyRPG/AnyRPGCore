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
        private int dropLimit = 0;

        [SerializeField]
        protected List<LootGroup> lootGroups = new List<LootGroup>();

        public List<LootGroup> LootGroups { get => lootGroups; set => lootGroups = value; }
        public int DropLimit { get => dropLimit; }
        public bool IgnoreGlobalDropLimit { get => ignoreGlobalDropLimit; }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            if (lootGroups != null) {
                foreach (LootGroup lootGroup in lootGroups) {
                    foreach (Loot tmpLoot in lootGroup.Loot) {
                        tmpLoot.SetupScriptableObjects(systemGameManager);
                    }
                }
            }
        }

    }

}