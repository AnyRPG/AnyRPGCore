using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class LootTableProps {

        [Header("Loot Table")]

        [Tooltip("If set to true, the items on this list will ignore any parent drop limits.")]
        [SerializeField]
        private bool ignoreGlobalDropLimit = false;

        [Tooltip("The maximum amount of items that can drop. 0 is unlimited")]
        [SerializeField]
        protected int dropLimit = 0;

        [SerializeField]
        protected List<LootGroup> lootGroups = new List<LootGroup>();

    }

}