using UnityEngine;

namespace AnyRPG {

    public class DeletePlayerCharacterRequest {
        public int Id;

        public DeletePlayerCharacterRequest(int playerCharacterId) {
            Id = playerCharacterId;
        }
    }
}
