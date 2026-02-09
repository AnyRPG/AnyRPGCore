using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class MailViewPanel : WindowPanel {

        [Header("Mail View Panel")]

        [SerializeField]
        private TextMeshProUGUI mailHeaderText = null;

        [SerializeField]
        private TextMeshProUGUI mailBodyText = null;

        /*
        [SerializeField]
        private HighlightButton closeButton = null;
        */

        [SerializeField]
        private List<MailViewAttachmentButton> attachmentButtons = new List<MailViewAttachmentButton>();

        [SerializeField]
        private HighlightButton takeAttachmentsButton = null;

        [SerializeField]
        private HighlightButton deleteButton = null;

        /*
        [SerializeField]
        private int dialogFontSize = 30;
        */

        private bool windowSubscriptionsInitialized = false;

        // game manager references
        protected PlayerManager playerManager = null;
        protected MailboxManagerClient mailboxManagerClient = null;
        protected SystemItemManager systemItemManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            mailboxManagerClient = systemGameManager.MailboxManagerClient;
            systemItemManager = systemGameManager.SystemItemManager;
        }

        public void DeleteMail() {
            mailboxManagerClient.RequestDeleteMessage();
            Close();
        }

        public void TakeAttachments() {
            mailboxManagerClient.RequestTakeAttachments();
        }

        public void DisplayMessage() {
            //Debug.Log("MailViewPanel.DisplayMessage()");

            MailMessage mailMessage = mailboxManagerClient.GetCurrentMessage();

            mailHeaderText.text = $"From: {mailMessage.Sender}\nSubject: {mailMessage.Subject}";
            mailBodyText.text = $"{mailMessage.Body}";
            int i = 0;
            bool hasAttachment = false;
            foreach (MailViewAttachmentButton attachmentButton in attachmentButtons) {
                List<InstantiatedItem> items = new List<InstantiatedItem>();
                if (mailMessage.AttachmentSlots.Count > i) {
                    foreach (long itemInstanceId in mailMessage.AttachmentSlots[i].ItemInstanceIds) {
                        InstantiatedItem instantiatedItem = systemItemManager.GetExistingInstantiatedItem(itemInstanceId);
                        if (instantiatedItem != null) {
                            hasAttachment = true;
                            items.Add(instantiatedItem);
                        }
                    }
                }
                attachmentButton.AddItemsFromMailMessage(i, items, mailMessage);
                i++;
            }
            if (hasAttachment == false ) {
                takeAttachmentsButton.Button.interactable = false;
                deleteButton.Button.interactable = true;
            } else {
                takeAttachmentsButton.Button.interactable= true;
                deleteButton.Button.interactable = false;
            }
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("MailViewPanel.OnOpenWindow()");

            base.ProcessOpenWindowNotification();
            DisplayMessage();
            if (windowSubscriptionsInitialized == true) {
                return;
            }
            mailboxManagerClient.OnTakeMailAttachment += HandleTakeMailAttachment;
            windowSubscriptionsInitialized = true;
        }

        public override void ReceiveClosedWindowNotification() {
            base.ReceiveClosedWindowNotification();
            if (windowSubscriptionsInitialized == false) {
                return;
            }
            mailboxManagerClient.OnTakeMailAttachment -= HandleTakeMailAttachment;
            windowSubscriptionsInitialized = false;
        }

        private void HandleTakeMailAttachment(int messageId) {
            DisplayMessage();
        }


    }

}