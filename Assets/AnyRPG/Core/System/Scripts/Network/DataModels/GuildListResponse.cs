using System;
using System.Collections.Generic;

namespace AnyRPG {

    [Serializable]
    public class GuildListResponse {
        // intentionally camelCase for compatibility with API server serializer
        public List<GuildSerializedData> guilds;

        public GuildListResponse() {
            guilds = new List<GuildSerializedData>();
        }
    }

}