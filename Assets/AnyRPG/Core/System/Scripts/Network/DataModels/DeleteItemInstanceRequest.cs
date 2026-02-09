using UnityEngine;

namespace AnyRPG {

    public class DeleteItemInstanceRequest {
        public long ItemInstanceId;

        public DeleteItemInstanceRequest(long itemInstanceId) {
            ItemInstanceId = itemInstanceId;
        }
    }
}
