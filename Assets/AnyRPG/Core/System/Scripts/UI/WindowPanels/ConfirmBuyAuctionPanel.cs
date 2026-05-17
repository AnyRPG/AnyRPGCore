using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class ConfirmBuyAuctionPanel : WindowPanel {

        [Header("Confirm Buy Auction")]

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
            //Debug.Log("ConfirmSendMailPanel.ConfirmAction()");

            //SystemGameManager.Instance.UIManager.HandScript.DeleteItem();
            auctionManagerClient.RequestBuyAuctionItem();
            Close();
        }

        public override void ProcessOpenWindowNotification() {
            base.ProcessOpenWindowNotification();
            InstantiatedItem item = auctionManagerClient.BuyAuctionItem.Items[0];
            currencyBarController.UpdateCurrencyAmount(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, auctionManagerClient.BuyAuctionItem.CurrencyAmount);
            confirmText.text = $"Buying {item.DisplayName} ({auctionManagerClient.BuyAuctionItem.Items.Count}) for";
        }


        public override void ReceiveClosedWindowNotification() {
            base.ReceiveClosedWindowNotification();
            
        }

    }

}