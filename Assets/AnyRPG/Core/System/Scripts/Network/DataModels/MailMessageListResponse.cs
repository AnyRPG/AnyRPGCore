using System;
using System.Collections.Generic;

namespace AnyRPG {

    [Serializable]
    public class MailMessageListResponse {
        // intentionally camelCase for compatibility with API server serializer
        public List<MailMessageSerializedData> mailMessages;

        public MailMessageListResponse() {
            mailMessages = new List<MailMessageSerializedData>();
        }
    }

}