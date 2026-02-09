using System;

namespace AnyRPG {
    
    [Serializable]
    public class GuildSerializedData {
        // intentionally camelCase for compatibility with API server serializer
        public int id;
        public string saveData;

        public GuildSerializedData() {
            saveData = string.Empty;
        }
    }
}
