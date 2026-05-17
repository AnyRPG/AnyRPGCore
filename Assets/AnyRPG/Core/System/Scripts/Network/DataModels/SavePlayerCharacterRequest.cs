using UnityEngine;

namespace AnyRPG {
    
    public class SavePlayerCharacterRequest {
        public int Id;
        public string Name;
        public string SaveData;

        public SavePlayerCharacterRequest(int playerCharacterId, string name, CharacterSaveData characterSaveData) {
            Id = playerCharacterId;
            Name = name;
            SaveData = JsonUtility.ToJson(characterSaveData);
        }
    }
}
