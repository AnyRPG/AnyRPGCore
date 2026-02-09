using UnityEngine;

namespace AnyRPG {
    
    public class SaveGuildRequest {
        public int Id;
        public string SaveData;

        public SaveGuildRequest(int guildId, GuildSaveData guildSaveData) {
            Id = guildId;
            SaveData = JsonUtility.ToJson(guildSaveData);
        }
    }
}
