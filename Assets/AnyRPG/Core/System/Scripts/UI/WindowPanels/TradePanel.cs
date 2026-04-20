using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class TradePanel : WindowPanel {

        [Header("Trade")]

        [SerializeField]
        private TextMeshProUGUI playerNameText = null;

        [SerializeField]
        private Image playerImage = null;

        [SerializeField]
        private TextMeshProUGUI targetNameText = null;

        [SerializeField]
        private Image targetImage = null;

        [SerializeField]
        private CurrencyEntryBarController currencyEntryBarController = null;

        [SerializeField]
        private CurrencyBarController currencyBarController = null;

        [SerializeField]
        private UINavigationController actionButtonsNavigationController = null;

        //[SerializeField]
        //private HighlightButton cancelButton = null;

        [SerializeField]
        private HighlightButton confirmButton = null;

        [SerializeField]
        private List<TradeButton> playerTradeButtons = new List<TradeButton>();

        [SerializeField]
        private List<TradeButton> targetTradeButtons = new List<TradeButton>();


        // game manager references
        private TradeServiceClient tradeServiceClient = null;
        private PlayerManagerClient playerManagerClient = null;
        private MessageFeedManager messageFeedManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            currencyBarController.Configure(systemGameManager);
            currencyEntryBarController.Configure(systemGameManager);
            tradeServiceClient.OnStartTradeSession += HandleStartTradeSession;
            tradeServiceClient.OnRequestAddItemsToTargetTradeSlot += HandleRequestAddItemsToTargetTradeSlot;
            tradeServiceClient.OnAddItemsToTargetTradeSlot += HandleAddItemsToTargetTradeSlot;
            tradeServiceClient.OnAddCurrencyToTrade += HandleAddCurrencyToTrade;
            tradeServiceClient.OnCompleteTrade += HandleCompleteTrade;
            tradeServiceClient.OnCancelTrade += HandleCancelTrade;
            currencyEntryBarController.OnRecalculateBaseCurrency += HandleRecalculateBaseCurrency;
            int i = 0;
            foreach (TradeButton button in playerTradeButtons) {
                button.SetIsInteractable(true);
                button.SetButtonIndex(i);
                i++;
            }
            i = 0;
            foreach (TradeButton button in targetTradeButtons) {
                button.SetIsInteractable(false);
                button.SetButtonIndex(i);
                i++;
            }
        }

        private void HandleRequestAddItemsToTargetTradeSlot() {
            if (tradeServiceClient.TradeConfirmed == true) {
                tradeServiceClient.UnconfirmTrade();
                confirmButton.Button.interactable = true;
                actionButtonsNavigationController.UpdateNavigationList();
            }
        }

        private void HandleCancelTrade() {
            Close();
        }

        private void HandleCompleteTrade() {
            Close();
        }

        private void HandleAddCurrencyToTrade(int amount) {
            //Debug.Log($"TradePanel.HandleAddCurrencyToTrade({amount})");

            currencyBarController.UpdateCurrencyAmount(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, amount);
            confirmButton.Button.interactable = true;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            tradeServiceClient = systemGameManager.TradeServiceClient;
            playerManagerClient = systemGameManager.PlayerManagerClient;
            messageFeedManager = systemGameManager.UIManager.MessageFeedManager;
        }

        private void HandleRecalculateBaseCurrency() {
            //Debug.Log($"TradePanel.HandleRecalculateBaseCurrency()");

            if (tradeServiceClient.TradeConfirmed == true) {
                tradeServiceClient.UnconfirmTrade();
                confirmButton.Button.interactable = true;
                actionButtonsNavigationController.UpdateNavigationList();
                return;
            }
            tradeServiceClient.AddCurrency(currencyEntryBarController.CurrencyNode);
        }

        private void HandleAddItemsToTargetTradeSlot(int buttonIndex, List<InstantiatedItem> itemList) {
            //Debug.Log($"TradePanel.HandleAddItemsToTargetTradeSlot({buttonIndex})");

            if (targetTradeButtons.Count <= buttonIndex) {
                return;
            }
            targetTradeButtons[buttonIndex].AddItems(itemList);
            if (tradeServiceClient.TradeConfirmed == true) {
                tradeServiceClient.UnconfirmTrade();
                confirmButton.Button.interactable = true;
                actionButtonsNavigationController.UpdateNavigationList();
                return;
            }
        }

        private void HandleStartTradeSession() {
            //Debug.Log($"TradePanel.HandleStartTradeSession()");

            playerNameText.text = playerManagerClient.UnitController.DisplayName;
            targetNameText.text = tradeServiceClient.TargetUnitController.DisplayName;
            playerImage.sprite = playerManagerClient.UnitController.UnitProfile.Icon;
            targetImage.sprite = tradeServiceClient.TargetUnitController.UnitProfile.Icon;
            foreach (TradeButton button in playerTradeButtons) {
                button.RemoveItem();
            }
            foreach (TradeButton button in targetTradeButtons) {
                button.RemoveItem();
            }
            Open();
        }

        public override void ProcessOpenWindowNotification() {
            base.ProcessOpenWindowNotification();
            currencyBarController.UpdateCurrencyAmount(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, 0);
            tradeServiceClient.OnRequestAddItemsToTrade += HandleRequestAddItemsToTrade;
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log($"TradePanel.ReceiveClosedWindowNotification()");

            base.ReceiveClosedWindowNotification();
            tradeServiceClient.RequestCancelTrade();
            currencyEntryBarController.ResetCurrencyAmounts();
            confirmButton.Button.interactable = true;
            tradeServiceClient.OnRequestAddItemsToTrade -= HandleRequestAddItemsToTrade;
        }

        private void HandleRequestAddItemsToTrade(InventorySlot inventorySlot) {
            //Debug.Log($"TradePanel.HandleRequestAddItemsToTrade({inventorySlot})");

            // iterate attachment buttons and find on that is empty, then add the item to that button
            foreach (TradeButton button in playerTradeButtons) {
                if (button.Items.Count == 0) {
                    button.AddItemFromInventorySlot(inventorySlot);
                    return;
                }
            }
            messageFeedManager.WriteMessage("No more trade slots available");
        }

        public void ConfirmAction() {
            confirmButton.Button.interactable = false;
            actionButtonsNavigationController.UpdateNavigationList();
            tradeServiceClient.RequestConfirmTrade();
        }

        public void CancelAction() {
            //Debug.Log($"TradePanel.CancelAction()");

            if (tradeServiceClient.TradeConfirmed == true) {
                tradeServiceClient.UnconfirmTrade();
                confirmButton.Button.interactable = true;
                actionButtonsNavigationController.UpdateNavigationList();
                return;
            }
            tradeServiceClient.RequestCancelTrade();
            Close();
        }



    }

}