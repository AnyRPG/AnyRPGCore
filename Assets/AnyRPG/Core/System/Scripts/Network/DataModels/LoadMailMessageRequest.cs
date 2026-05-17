using UnityEngine;

namespace AnyRPG {

    public class LoadMailMessageRequest {
        public int MessageId;

        public LoadMailMessageRequest(int messageId) {
            MessageId = messageId;
        }
    }
}
