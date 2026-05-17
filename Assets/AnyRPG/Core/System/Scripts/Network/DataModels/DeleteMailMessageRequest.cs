using UnityEngine;

namespace AnyRPG {

    public class DeleteMailMessageRequest {
        public int Id;

        public DeleteMailMessageRequest(int mailMessageId) {
            Id = mailMessageId;
        }
    }
}
