using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class ConfirmSendMailPanel : WindowPanel {

        [Header("Confirm Send Mail")]

        [SerializeField]
        private CurrencyBarController currencyBarController = null;

        [SerializeField]
        private TextMeshProUGUI confirmNameText = null;

        /*
        [SerializeField]
        private HighlightButton noButton = null;

        [SerializeField]
        private HighlightButton yesButton = null;
        */

        // game manager references
        private UIManager uIManager = null;
        private MailboxManagerClient mailboxManagerClient = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            currencyBarController.Configure(systemGameManager);
            /*
            noButton.Configure(systemGameManager);
            yesButton.Configure(systemGameManager);
            */
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            mailboxManagerClient = systemGameManager.MailboxManagerClient;
        }

        public void CancelAction() {
            //Debug.Log("ConfirmSendMailPanel.CancelAction()");
            Close();
        }

        public void ConfirmAction() {
            //Debug.Log("ConfirmSendMailPanel.ConfirmAction()");

            //SystemGameManager.Instance.UIManager.HandScript.DeleteItem();
            mailboxManagerClient.SendSavedMessage();
            Close();
        }

        public override void ProcessOpenWindowNotification() {
            base.ProcessOpenWindowNotification();
            currencyBarController.UpdateCurrencyAmount(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, mailboxManagerClient.SavedMessageRequest.CurrencyAmount);
            confirmNameText.text = $"To {mailboxManagerClient.SavedMessageRequest.Recipient}.\nAre you sure?";
        }


        public override void ReceiveClosedWindowNotification() {
            base.ReceiveClosedWindowNotification();
            
        }

    }

}