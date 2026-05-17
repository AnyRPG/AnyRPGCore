using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace AnyRPG {
    public class MailboxButton : HighlightButton, IPointerClickHandler {

        [Header("Mailbox Button")]

        [SerializeField]
        protected Image attachmentBackgroundImage = null;

        [SerializeField]
        protected Image attachmentImage = null;

        [SerializeField]
        protected TextMeshProUGUI subjectText = null;

        [SerializeField]
        protected TextMeshProUGUI descriptionText = null;

        private MailMessage mailMessage = null;

        // game manager references
        private MailboxManagerClient mailboxManagerClient = null;
        private SystemItemManager systemItemManager = null;

        public MailMessage MailMessage { get => mailMessage; set => mailMessage = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            mailboxManagerClient = systemGameManager.MailboxManagerClient;
            systemItemManager = systemGameManager.SystemItemManager;
        }

        public void AddMessage(MailMessage mailMessage) {
            //Debug.Log($"{gameObject.name}.MailboxButton.AddMessage({mailMessage.MessageId})");

            this.mailMessage = mailMessage;
            InstantiatedItem instantiatedItem = GetFirstMessageItem();
            if (instantiatedItem != null) {
                attachmentImage.color = Color.white;
                attachmentImage.sprite = instantiatedItem.Icon;
                attachmentBackgroundImage.color = Color.white;
                uIManager.SetItemBackground(instantiatedItem.Item, attachmentBackgroundImage, new Color32(0, 0, 0, 255), instantiatedItem.ItemQuality);
            } else {
                attachmentImage.sprite = null;
                attachmentImage.color = new Color32(0, 0, 0, 0);
                attachmentBackgroundImage.sprite = null;
                attachmentBackgroundImage.color = new Color32(0, 0, 0, 0);
            }
            string subjectColorString = mailMessage.IsRead ? "#888888" : "#E5E22E";
            string descriptionColorString = mailMessage.IsRead ? "#888888" : "#FFFFFF";
            subjectText.text = $"<color={subjectColorString}>{mailMessage.Subject}</color>";
            descriptionText.text = $"<color={descriptionColorString}>From: {mailMessage.Sender}</color>";
        }

        public InstantiatedItem GetFirstMessageItem() {
            foreach (MailAttachmentSlot mailAttachmentSlot in mailMessage.AttachmentSlots) {
                foreach (long itemId in mailAttachmentSlot.ItemInstanceIds) {
                    InstantiatedItem instantiatedItem = systemItemManager.GetExistingInstantiatedItem(itemId);
                    return instantiatedItem;
                }
            }
            return null;
        }

        public void ClearMessage() {
            attachmentImage.sprite = null;
            attachmentImage.color = new Color32(0, 0, 0, 0);
            subjectText.text = string.Empty;
            descriptionText.text = string.Empty;
        }

        protected override void HandleLeftClick() {
            base.HandleLeftClick();
            mailboxManagerClient.SetCurrentMailMessageId(mailMessage.MessageId);
            uIManager.mailViewWindow.OpenWindow();
        }
    }

}