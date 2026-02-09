using UnityEngine;

namespace AnyRPG {
    
    public class SaveFriendListRequest {
        public int PlayerCharacterId;
        public string SaveData;

        public SaveFriendListRequest(int playerCharacterId, FriendListSaveData friendListSaveData) {
            PlayerCharacterId = playerCharacterId;
            SaveData = JsonUtility.ToJson(friendListSaveData);
        }
    }
}
