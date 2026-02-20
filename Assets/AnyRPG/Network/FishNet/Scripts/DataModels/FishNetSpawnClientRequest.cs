using UnityEngine;
using System.Collections.Generic;
using FishNet.Object;

namespace AnyRPG {
    public class FishNetSpawnClientRequest {
        public PlayerCharacterSaveData PlayerCharacterSaveData;
        public int CharacterGroupId;
        public int GuildId;
        public string GuildName = string.Empty;
        public List<NetworkObject> InteractionPoints = new List<NetworkObject>();
    }

}
