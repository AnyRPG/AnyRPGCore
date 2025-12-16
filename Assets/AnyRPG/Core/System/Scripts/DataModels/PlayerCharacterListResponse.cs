using System;
using System.Collections.Generic;

namespace AnyRPG {

    [Serializable]
    public class PlayerCharacterListResponse {
        public List<PlayerCharacterData> playerCharacters;

        public PlayerCharacterListResponse() {
            playerCharacters = new List<PlayerCharacterData>();
        }
    }

}