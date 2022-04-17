using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {

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