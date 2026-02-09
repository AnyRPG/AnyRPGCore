using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class ConfirmListAuctionPanel : WindowPanel {

        [Header("Confirm List Auction")]

        [SerializeField]
        private CurrencyBarController currencyBarController = null;

        [SerializeField]
        private TextMeshProUGUI confirmText = null;

        /*
        [SerializeField]
        private HighlightButton noButton = null;

        [SerializeField]
        private HighlightButton yesButton = null;
        */

        // game manager references
        private UIManager uIManager = null;
        private AuctionManagerClient auctionManagerClient = null;
        private SystemItemManager systemItemManager = null;

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
            auctionManagerClient = systemGameManager.AuctionManagerClient;
            systemItemManager = systemGameManager.SystemItemManager;
        }

        public void CancelAction() {
            //Debug.Log("ConfirmSendMailPanel.CancelAction()");
            Close();
        }

        public void ConfirmAction() {
            //Debug.Log("ConfirmListAuctionPanel.ConfirmAction()");

            //SystemGameManager.Instance.UIManager.HandScript.DeleteItem();
            auctionManagerClient.ListAuctionItems();
            Close();
        }

        public override void ProcessOpenWindowNotification() {
            base.ProcessOpenWindowNotification();
            currencyBarController.UpdateCurrencyAmount(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, auctionManagerClient.ListAuctionItemRequest.CurrencyAmount);
            confirmText.text = $"List {systemItemManager.GetExistingInstantiatedItem(auctionManagerClient.ListAuctionItemRequest.ItemInstanceIds[0]).DisplayName} for";
        }


        public override void ReceiveClosedWindowNotification() {
            base.ReceiveClosedWindowNotification();
            
        }

    }

}