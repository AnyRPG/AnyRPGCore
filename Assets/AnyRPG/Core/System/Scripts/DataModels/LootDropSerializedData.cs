using System;


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
