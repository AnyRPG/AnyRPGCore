using System;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {

    [Serializable]
    public class LootDropIdList {
        public int AccountId;
        public List<int> LootDropIds = new List<int>();

        public LootDropIdList() {
        }

        public LootDropIdList(int accountId, List<int> lootDropIds) {
            AccountId = accountId;
            LootDropIds = lootDropIds;
        }

    }
}
