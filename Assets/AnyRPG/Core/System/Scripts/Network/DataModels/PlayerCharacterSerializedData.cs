using System;

namespace AnyRPG {
    
    [Serializable]
    public class PlayerCharacterSerializedData {
        // intentionally camelCase for compatibility with API server serializer
        public int id;
        public int accountId;
        public string name;
        public string saveData;

        public PlayerCharacterSerializedData() {
            name = string.Empty;
            saveData = string.Empty;
        }
    }
}
