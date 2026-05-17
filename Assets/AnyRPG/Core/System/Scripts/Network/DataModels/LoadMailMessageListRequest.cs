using UnityEngine;

namespace AnyRPG {

    public class LoadMailMessageListRequest {
        public int PlayerCharacterId;

        public LoadMailMessageListRequest(int playerCharacterId) {
            PlayerCharacterId = playerCharacterId;
        }
    }
}
