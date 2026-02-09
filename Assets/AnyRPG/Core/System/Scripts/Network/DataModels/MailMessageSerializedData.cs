using System;

namespace AnyRPG {
    
    [Serializable]
    public class MailMessageSerializedData {
        // intentionally camelCase for compatibility with API server serializer
        public int id;
        public string saveData;

        public MailMessageSerializedData() {
            saveData = string.Empty;
        }
    }
}
