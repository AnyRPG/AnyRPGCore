using System;
using UnityEngine;

namespace AnyRPG {
    
    public class CreateMailMessageRequest {
        public int PlayerCharacterId;
        public string SaveData = string.Empty;

        public CreateMailMessageRequest(MailMessage mailMessage, int playerCharacterId) {
            SaveData = JsonUtility.ToJson(mailMessage);
            PlayerCharacterId = playerCharacterId;
        }
    }
}
