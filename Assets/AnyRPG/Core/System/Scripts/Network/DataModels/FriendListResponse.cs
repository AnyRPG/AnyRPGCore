using System;
using System.Collections.Generic;

namespace AnyRPG {

    [Serializable]
    public class FriendListResponse {
        // intentionally camelCase for compatibility with API server serializer
        public List<FriendListSerializedData> friendLists;

        public FriendListResponse() {
            friendLists = new List<FriendListSerializedData>();
        }
    }

}