using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class NotificationsPanel : WindowPanel {

        [Header("Notifications Panel")]

        [SerializeField]
        private MailNotificationIcon mailNotificationIcon = null;

        // game manager references
        private MailboxManagerClient mailboxManagerClient = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            mailNotificationIcon.Configure(systemGameManager);

            mailboxManagerClient.OnNewUnreadMail += HandleNewUnreadMail;
            mailboxManagerClient.OnAllMailIsRead += HandleAllMailIsRead;

            // turn off by default
            HandleAllMailIsRead();
        }

        private void HandleAllMailIsRead() {
            gameObject.SetActive(false);
        }

        private void HandleNewUnreadMail() {
            gameObject.SetActive(true);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            mailboxManagerClient = systemGameManager.MailboxManagerClient;
        }


    }

}