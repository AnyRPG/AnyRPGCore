using UnityEngine;

namespace AnyRPG {
    
    public class SaveMailMessageRequest {
        public int Id;
        public string SaveData;

        public SaveMailMessageRequest(int mailMessageId, MailMessage mailMessage) {
            Id = mailMessageId;
            SaveData = JsonUtility.ToJson(mailMessage);
        }
    }
}
