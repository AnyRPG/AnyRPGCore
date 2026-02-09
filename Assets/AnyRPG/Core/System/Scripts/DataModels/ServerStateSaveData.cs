using System;

namespace AnyRPG {
    [Serializable]
    public class ServerStateSaveData {
        public int accountIdCounter = 1;
        public int auctionIdCounter = 1;
        public int guildIdCounter = 1;
        public int playerCharacterIdCounter = 1;
        public int mailIdCounter = 1;
    }
}