using System;
using System.Collections.Generic;

namespace AnyRPG {

    [Serializable]
    public class PlayerCharacterListResponse {
        public List<PlayerCharacterSerializedData> playerCharacters;

        public PlayerCharacterListResponse() {
            playerCharacters = new List<PlayerCharacterSerializedData>();
        }
    }

}