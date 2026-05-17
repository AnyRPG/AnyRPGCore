using System;
using UnityEngine;

namespace AnyRPG {
    
    public class CreateGuildRequest {
        public string SaveData = string.Empty;

        public CreateGuildRequest(GuildSaveData guildSaveData) {
            SaveData = JsonUtility.ToJson(guildSaveData);
        }
    }
}
