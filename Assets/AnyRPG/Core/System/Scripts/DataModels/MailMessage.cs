using System;
using System.Collections.Generic;


namespace AnyRPG {

    [Serializable]
    public class MailMessage {
        public int MessageId = 0;
        public string Sender = string.Empty;
        public string Recipient = string.Empty;
        public string Subject = string.Empty;
        public string Body = string.Empty;
        public List<MailAttachmentSlot> AttachmentSlots = new List<MailAttachmentSlot>();
        public int CurrencyAmount = 0;
        public bool IsRead = false;

        public MailMessage() { }

        public MailMessage(MailMessageRequest mailMessageRequest) {
            Recipient = mailMessageRequest.Recipient;
            Subject = mailMessageRequest.Subject;
            Body = mailMessageRequest.Body;
            AttachmentSlots = mailMessageRequest.AttachmentSlots;
        }
    }

    [Serializable]
    public class  MailAttachmentSlot {
        public List<long> ItemInstanceIds = new List<long>();
    }

}
