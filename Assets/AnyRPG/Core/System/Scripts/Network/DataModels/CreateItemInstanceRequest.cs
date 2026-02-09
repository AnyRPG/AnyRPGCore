using System;
using UnityEngine;

namespace AnyRPG {
    
    public class CreateItemInstanceRequest {
        public long ItemInstanceId;
        public string SaveData = string.Empty;

        public CreateItemInstanceRequest(ItemInstanceSaveData itemInstanceSaveData) {
            ItemInstanceId = itemInstanceSaveData.ItemInstanceId;
            SaveData = JsonUtility.ToJson(itemInstanceSaveData);
        }
    }
}
