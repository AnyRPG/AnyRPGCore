using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class MailComposePanel : WindowPanel {

        [Header("Mail Compose Panel")]

        [SerializeField]
        private CurrencyBarController currencyBarController = null;

        [SerializeField]
        private TMP_InputField recipientInput = null;

        [SerializeField]
        private TMP_InputField subjectInput = null;

        [SerializeField]
        private TMP_InputField bodyInput = null;

        [SerializeField]
        List<MailComposeAttachmentButton> attachmentButtons = new List<MailComposeAttachmentButton>();

        [SerializeField]
        CurrencyEntryBarController currencyEntryBarController = null;

        private int postageCurrencyAmount = 0;

        /*
        [SerializeField]
        private HighlightButton cancelButton = null;

        [SerializeField]
        private HighlightButton sendButton = null;
        */

        /*
        [SerializeField]
        private int dialogFontSize = 30;
        */

        // game manager references
        protected UIManager uIManager = null;
        protected MessageLogClient messageLogClient = null;
        protected DialogManagerClient dialogManagerClient = null;
        protected PlayerManager playerManager = null;
        protected MailboxManagerClient mailboxManagerClient = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            currencyBarController.Configure(systemGameManager);
            currencyEntryBarController.Configure(systemGameManager);
            foreach (MailComposeAttachmentButton button in attachmentButtons) {
                if (button != null) {
                    button.OnAddAttachment += HandleAddAttachment;
                    button.OnRemoveAttachment += HandleRemoveAttachment;
                    button.SetMailComposePanel(this);
                }
            }
            mailboxManagerClient.OnMailSend += HandleMailSend;
        }

        public bool HasItemInstanceId(long itemInstanceId) {
            foreach (MailComposeAttachmentButton button in attachmentButtons) {
                if (button != null && button.HasItemInstanceId(itemInstanceId)) {
                    return true;
                }
            }

            return false;
        }

        private void HandleMailSend() {
            Close();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            messageLogClient = systemGameManager.MessageLogClient;
            dialogManagerClient = systemGameManager.DialogManagerClient;
            playerManager = systemGameManager.PlayerManager;
            mailboxManagerClient = systemGameManager.MailboxManagerClient;
        }

        private void HandleRemoveAttachment() {
            RecalculatePostage();
        }

        private void HandleAddAttachment() {
            RecalculatePostage();
        }

        private void RecalculatePostage() {
            if (systemConfigurationManager.BasePostageCurrencyAmount == 0 && systemConfigurationManager.PostageCurrencyAmountPerAttachment == 0) {
                currencyBarController.ClearCurrencyAmounts();
                return;
            }
            postageCurrencyAmount = systemConfigurationManager.BasePostageCurrencyAmount;
            foreach (MailComposeAttachmentButton button in attachmentButtons) {
                if (button != null && button.GetItemInstanceIds().Count > 0) {
                    postageCurrencyAmount += systemConfigurationManager.PostageCurrencyAmountPerAttachment;
                }
            }
            currencyBarController.UpdateCurrencyAmount(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, postageCurrencyAmount, "Postage:");
        }

        public void CancelAction() {
            Close();
        }

        public void SendMail() {
            //Debug.Log($"MailComposePanel.SendMail()");

            List<MailAttachmentSlot> allAttachmentSlots = new List<MailAttachmentSlot>();
            foreach (MailComposeAttachmentButton composeMailAttachmentButton in attachmentButtons) {
                List<long> attachmentIds = composeMailAttachmentButton.GetItemInstanceIds();
                MailAttachmentSlot mailAttachmentSlot = new MailAttachmentSlot();
                mailAttachmentSlot.ItemInstanceIds = attachmentIds;
                allAttachmentSlots.Add(mailAttachmentSlot);
            }
            MailMessageRequest sendMailRequest = new MailMessageRequest() {
                Recipient = recipientInput.text,
                Subject = subjectInput.text,
                Body = bodyInput.text,
                AttachmentSlots = allAttachmentSlots,
                CurrencyAmount = currencyEntryBarController.CurrencyNode.Amount
            };
            if (currencyEntryBarController.CurrencyNode.Amount > 0) {
                mailboxManagerClient.SaveMessageRequest(sendMailRequest);
                uIManager.confirmSendMailWindow.OpenWindow();
            } else {
                mailboxManagerClient.RequestSendMail(sendMailRequest);
            }
        }

        private void ClearInputFields() {
            recipientInput.text = string.Empty;
            subjectInput.text = string.Empty;
            bodyInput.text = string.Empty;
            currencyEntryBarController.ResetCurrencyAmounts();
            foreach (MailComposeAttachmentButton mailComposeAttachmentButton in attachmentButtons) {
                mailComposeAttachmentButton.Clearitems();
            }
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("DialogPanelController.OnOpenWindow()");
            base.ProcessOpenWindowNotification();
            ClearInputFields();
            RecalculatePostage();
        }

        public override void ReceiveClosedWindowNotification() {
            base.ReceiveClosedWindowNotification();
        }

    }

}