using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class MailboxPanel : PagedWindowContents {

        [Header("Mailbox Panel")]

        [SerializeField]
        private List<MailboxButton> mailboxButtons = new List<MailboxButton>();

        [SerializeField]
        private UINavigationController mailboxButtonsNavigationController = null;

        private bool windowSubscriptionsInitialized = false;

        // game manager references
        private PlayerManager playerManager = null;
        private MailboxManagerClient mailboxManagerClient = null;
        private UIManager uiManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            mailboxManagerClient = systemGameManager.MailboxManagerClient;
            uiManager = systemGameManager.UIManager;
        }

        public override void ProcessOpenWindowNotification() {
            base.ProcessOpenWindowNotification();
            if (windowSubscriptionsInitialized == true) {
                return;
            }
            mailboxManagerClient.OnSetMailMessages += HandleSetMailMessages;
            mailboxManagerClient.OnDeleteMailMessage += HandleDeleteMailMessage;
            mailboxManagerClient.OnTakeMailAttachment += HandleTakeMailAttachment;
            mailboxManagerClient.OnMarkMessageAsRead += HandleMarkMessageAsRead;
            windowSubscriptionsInitialized = true;
            if (playerManager.UnitController == null) {
                return;
            }
        }

        public override void ReceiveClosedWindowNotification() {
            base.ReceiveClosedWindowNotification();
            if (windowSubscriptionsInitialized == false) {
                return;
            }
            mailboxManagerClient.OnSetMailMessages -= HandleSetMailMessages;
            mailboxManagerClient.OnDeleteMailMessage -= HandleDeleteMailMessage;
            mailboxManagerClient.OnTakeMailAttachment -= HandleTakeMailAttachment;
            mailboxManagerClient.OnMarkMessageAsRead -= HandleMarkMessageAsRead;
            windowSubscriptionsInitialized = false;
        }

        private void HandleMarkMessageAsRead(int messageId) {
            //Debug.Log($"MailboxPanel.HandleMarkMessageAsRead({messageId})");

            RefreshMessageDisplay(messageId);
        }

        private void HandleTakeMailAttachment(int messageId) {
            //Debug.Log($"MailboxPanel.HandleTakeMailAttachment({messageId})");

            RefreshMessageDisplay(messageId);
        }

        private void RefreshMessageDisplay(int messageId) {
            //Debug.Log($"MailboxPanel.RefreshMessageDisplay({messageId})");

            foreach (MailboxButton mailboxButton in mailboxButtons) {
                if (mailboxButton.MailMessage != null && mailboxButton.MailMessage.MessageId == messageId) {
                    mailboxButton.AddMessage(mailboxButton.MailMessage);
                    break;
                }
            }
        }

        private void HandleSetMailMessages() {
            //PopulatePages();
            CreatePages();
        }

        private void HandleDeleteMailMessage() {
            //PopulatePages();
            CreatePages();
        }

        protected override void PopulatePages() {
            //Debug.Log("MailboxPanel.PopulatePages()");

            pages.Clear();
            MailboxContentList page = new MailboxContentList();
            foreach (MailMessage message in mailboxManagerClient.MailMessages.Values) {
                page.messages.Add(message);
                if (page.messages.Count == pageSize) {
                    pages.Add(page);
                    page = new MailboxContentList();
                }
            }
            if (page.messages.Count > 0) {
                pages.Add(page);
            }
            AddMessages();
        }

        public void AddMessages() {
            //Debug.Log("MailboxPanel.AddMessages()");

            bool foundButton = false;
            if (pages.Count > 0) {
                if (pageIndex >= pages.Count) {
                    pageIndex = pages.Count - 1;
                }
                for (int i = 0; i < pageSize; i++) {
                    //for (int i = 0; i < pages[pageIndex].Count - 1; i++) {
                    //Debug.Log($"CurrencyPanelUI.AddCurrencies() i: {i} pageIndex: {pageIndex}");
                    if (i < (pages[pageIndex] as MailboxContentList).messages.Count) {
                        //Debug.Log($"CurrencyPanelUI.AddCurrencies() i: {i} pageIndex: {pageIndex} adding button");
                        mailboxButtons[i].gameObject.SetActive(true);
                        mailboxButtons[i].AddMessage((pages[pageIndex] as MailboxContentList).messages[i]);
                        foundButton = true;
                    } else {
                        //Debug.Log($"CurrencyPanelUI.AddCurrencies() i: {i} pageIndex: {pageIndex} clearing button");
                        mailboxButtons[i].ClearMessage();
                        mailboxButtons[i].gameObject.SetActive(false);
                    }
                }
            }

            if (foundButton) {
                mailboxButtonsNavigationController.FocusFirstButton();
                SetNavigationController(mailboxButtonsNavigationController);
            }
        }

        public override void AddPageContent() {
            //Debug.Log("MailboxPanel.AddPageContent()");

            base.AddPageContent();
            AddMessages();
        }

        public override void ClearButtons() {
            //Debug.Log("MailboxPanel.ClearButtons()");

            base.ClearButtons();
            foreach (MailboxButton btn in mailboxButtons) {
                btn.gameObject.SetActive(false);
            }
        }

        public void NewMessage() {
            uiManager.mailComposeWindow.OpenWindow();
        }

    }

    public class MailboxContentList : PagedContentList {
        public List<MailMessage> messages = new List<MailMessage>();
    }
}