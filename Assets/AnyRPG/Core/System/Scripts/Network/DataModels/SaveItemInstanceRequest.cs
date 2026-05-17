using UnityEngine;

namespace AnyRPG {
    
    public class SaveItemInstanceRequest {
        public long ItemInstanceId;
        public string SaveData;

        public SaveItemInstanceRequest(long itemInstanceId, ItemInstanceSaveData itemInstanceSaveData) {
            ItemInstanceId = itemInstanceId;
            SaveData = JsonUtility.ToJson(itemInstanceSaveData);
        }
    }
}
