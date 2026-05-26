using System;
using System.Collections.Generic;


namespace AnyRPG {

    [Serializable]
    public class MailMessageRequest {
        public string Sender = string.Empty;
        public string Recipient = string.Empty;
        public string Subject = string.Empty;
        public string Body = string.Empty;
        public List<MailAttachmentSlot> AttachmentSlots = new List<MailAttachmentSlot>();
        public int CurrencyAmount = 0;
        
        public MailMessageRequest() { }
    }

}
