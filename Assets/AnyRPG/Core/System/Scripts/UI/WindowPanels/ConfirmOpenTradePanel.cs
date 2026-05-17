using System;
using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class ConfirmOpenTradePanel : WindowPanel {

        [Header("Confirm Open Trade Panel")]

        [SerializeField]
        private TextMeshProUGUI messageText = null;

        // game manager references
        private TradeServiceClient tradeServiceClient = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            //noButton.Configure(systemGameManager);
            //yesButton.Configure(systemGameManager);
            tradeServiceClient.OnRequestBeginTrade += HandleRequestBeginTrade;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            tradeServiceClient = systemGameManager.TradeServiceClient;
        }

        public void HandleRequestBeginTrade() {
            Open();
        }

        public override void ProcessOpenWindowNotification() {
            base.ProcessOpenWindowNotification();
            if (messageText != null) {
                messageText.text = $"{tradeServiceClient.TargetUnitController?.DisplayName} would like to trade with you";
            }
        }

        public void CancelAction() {
            //Debug.Log("NewGameMenuController.CancelAction()");

            Close();
            tradeServiceClient.RequestDeclineTrade();
        }

        public void ConfirmAction() {
            //Debug.Log("ConfirmOpenTradePanel.ConfirmAction()");

            Close();
            tradeServiceClient.RequestAcceptTrade();
        }

    }

}