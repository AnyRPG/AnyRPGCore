using System;

namespace AnyRPG {
    
    [Serializable]
    public class AuctionItemSerializedData {
        // intentionally camelCase for compatibility with API server serializer
        public int id;
        public string saveData;

        public AuctionItemSerializedData() {
            saveData = string.Empty;
        }
    }
}
