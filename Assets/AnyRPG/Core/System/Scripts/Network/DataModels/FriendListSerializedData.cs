using System;

namespace AnyRPG {
    
    [Serializable]
    public class FriendListSerializedData {
        // intentionally camelCase for compatibility with API server serializer
        public int id;
        public int playerCharacterId;
        public string saveData;

        public FriendListSerializedData() {
            saveData = string.Empty;
        }
    }
}
