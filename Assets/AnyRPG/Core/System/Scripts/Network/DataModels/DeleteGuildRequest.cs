using UnityEngine;

namespace AnyRPG {

    public class DeleteGuildRequest {
        public int Id;

        public DeleteGuildRequest(int guildId) {
            Id = guildId;
        }
    }
}
