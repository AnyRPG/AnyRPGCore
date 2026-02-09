using System;
using UnityEngine;

namespace AnyRPG {
    
    public class CreatePlayerCharacterRequest {
        public int AccountId;
        public string Name = string.Empty;
        public string SaveData = string.Empty;

        public CreatePlayerCharacterRequest(int accountId, CharacterSaveData characterSaveData) {
            AccountId = accountId;
            Name = characterSaveData.CharacterName;
            SaveData = JsonUtility.ToJson(characterSaveData);
        }
    }
}
