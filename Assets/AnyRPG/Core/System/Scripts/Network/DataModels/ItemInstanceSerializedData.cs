using System;

namespace AnyRPG {
    
    [Serializable]
    public class ItemInstanceSerializedData {
        // intentionally camelCase for compatibility with API server serializer
        public int id;
        public long itemInstanceId;
        public string saveData;

        public ItemInstanceSerializedData() {
            saveData = string.Empty;
        }
    }
}
