using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class MailboxManagerClient : InteractableOptionManager {

        public event Action OnSetMailMessages = delegate { };
        public event Action OnDeleteMailMessage = delegate { };
        public event Action<int> OnTakeMailAttachment = delegate { };
        public event Action OnMailSend = delegate { };
        public event Action OnNewUnreadMail = delegate { };
        public event Action OnAllMailIsRead = delegate { };
        public event Action<int> OnMarkMessageAsRead = delegate { };

        private MailMessageRequest savedMessageRequest = null;

        private int currentMessageId = 0;

        private MailboxComponent mailboxComponent = null;

        private Dictionary<int, MailMessage> mailMessages = new Dictionary<int, MailMessage>();

        public MailboxComponent MailboxComponent { get => mailboxComponent; set => mailboxComponent = value; }
        public MailMessageRequest SavedMessageRequest { get => savedMessageRequest; }
        public Dictionary<int, MailMessage> MailMessages { get => mailMessages; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            //networkManagerClient.OnClientConnectionStarted += HandleClientConnectionStarted;
        }

        public void SetProps(MailboxComponent mailboxComponent, int componentIndex, int choiceIndex) {
            this.mailboxComponent = mailboxComponent;

            BeginInteraction(mailboxComponent, componentIndex, choiceIndex);
        }

        public override void EndInteraction() {
            base.EndInteraction();
            mailboxComponent = null;
        }

        public void SaveMessageRequest(MailMessageRequest sendMailRequest) {
            //Debug.Log($"mailboxManagerClient.SaveMessageRequest()");

            this.savedMessageRequest = sendMailRequest;
        }

        public void SendSavedMessage() {
            //Debug.Log("MailboxManagerClient.SendSavedMessage()");

            networkManagerClient.RequestSendMail(mailboxComponent.Interactable, componentIndex, savedMessageRequest);
        }


        public void RequestSendMail(MailMessageRequest sendMailRequest) {
            networkManagerClient.RequestSendMail(mailboxComponent.Interactable, componentIndex, sendMailRequest);
        }

        public void RequestMailMessages() {
            //networkManagerClient.RequestMailMessages
        }

        public void SetMailMessages(MailMessageListBundle mailMessageListResponse) {
            //Debug.Log($"MailboxManagerClient.SetMailMessages(count: {mailMessageListResponse.MailMessages.Count})");

            mailMessages.Clear();
            systemItemManager.LoadItemInstanceListSaveData(mailMessageListResponse.ItemInstanceListSaveData);
            foreach (MailMessage mailMessage in mailMessageListResponse.MailMessages) {
                mailMessages.Add(mailMessage.MessageId, mailMessage);
            }
            OnSetMailMessages();
            CheckForUnreadMail();
        }

        public void SetCurrentMailMessageId(int messageId) {
            //Debug.Log($"MailboxManagerClient.SetCurrentMailMessageId({messageId})");

            this.currentMessageId = messageId;
        }

        public MailMessage GetCurrentMessage() {
            //Debug.Log($"MailboxManagerClient.GetCurrentMessage()");

            if (currentMessageId == -1) {
                return null;
            }
            if (mailMessages.ContainsKey(currentMessageId) == false) {
                return null;
            }
            networkManagerClient.RequestMarkMailAsRead(currentMessageId);
            mailMessages[currentMessageId].IsRead = true;
            OnMarkMessageAsRead(currentMessageId);

            CheckForUnreadMail();
            return mailMessages[currentMessageId];
        }

        public void CheckForUnreadMail() {
            //Debug.Log($"MailboxManagerClient.CheckForUnreadMail()");

            foreach (MailMessage mailMessage in mailMessages.Values) {
                if (mailMessage.IsRead == false) {
                    OnNewUnreadMail();
                    return;
                }
            }
            OnAllMailIsRead();
        }

        public void RequestDeleteMessage() {
            //Debug.Log($"MailboxManagerClient.RequestDeleteMessage()");

            networkManagerClient.RequestDeleteMailMessage(currentMessageId);
        }

        public void RequestTakeAttachments() {
            //Debug.Log($"MailboxManagerClient.RequestTakeAttachments()");

            networkManagerClient.RequestTakeMailAttachments(currentMessageId);
        }

        public void RequestTakeAttachment(int messageId, int attachmentSlotId) {
            //Debug.Log($"MailboxManagerClient.RequestTakeAttachment({messageId}, {attachmentSlotId})");

            networkManagerClient.RequestTakeMailAttachment(messageId, attachmentSlotId);
        }

        public void AdvertiseDeleteMailMessage(int messageId) {
            if (mailMessages.ContainsKey (messageId) == false) {
                return;
            }
            mailMessages.Remove(messageId);
            OnDeleteMailMessage();
        }

        public void AdvertiseTakeMailAttachment(int messageId, int attachmentSlotId) {
            mailMessages[messageId].AttachmentSlots[attachmentSlotId].ItemInstanceIds.Clear();
            OnTakeMailAttachment(messageId);
        }

        public void AdvertiseTakeMailAttachments(int messageId) {
            foreach (MailAttachmentSlot attachmentSlot in mailMessages[messageId].AttachmentSlots) {
                attachmentSlot.ItemInstanceIds.Clear();
            }
            OnTakeMailAttachment(messageId);
        }

        public void AdvertiseMailSend() {
            OnMailSend();
        }
    }

}