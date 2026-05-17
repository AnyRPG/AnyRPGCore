using System;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {

    [Serializable]
    public class LootDropSerializedData {
        public int LootDropId;
        public long ItemInstanceId;

        public LootDropSerializedData() {
        }

        public LootDropSerializedData(int lootDropid, long itemInstanceId) {
            LootDropId = lootDropid;
            ItemInstanceId = itemInstanceId;
        }

    }
}
