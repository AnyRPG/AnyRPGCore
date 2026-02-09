using System;
using System.Collections.Generic;

namespace AnyRPG {

    [Serializable]
    public class ItemInstanceListResponse {
        // intentionally camelCase for compatibility with API server serializer
        public List<ItemInstanceSerializedData> itemInstances;

        public ItemInstanceListResponse() {
            itemInstances = new List<ItemInstanceSerializedData>();
        }
    }

}