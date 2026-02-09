using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class TradeServiceClient : ConfiguredClass {

        public event Action OnStartTradeSession = delegate { };
        public event Action OnRequestBeginTrade = delegate { };
        public event Action OnRequestAddItemsToTargetTradeSlot = delegate { };
        public event Action<int, List<InstantiatedItem>> OnAddItemsToTargetTradeSlot = delegate { };
        public event Action<int> OnAddCurrencyToTrade = delegate { };
        public event Action OnCompleteTrade = delegate { };
        public event Action OnCancelTrade = delegate { };

        private int targetCharacterId = 0;
        private UnitController targetUnitController = null;
        private bool tradeConfirmed = false;
        private int currencyAmount = 0;

        // game manager references
        private CharacterManager characterManager = null;
        private MessageLogClient messageLogClient = null;

        public int TargetCharacterId { get => targetCharacterId; set => targetCharacterId = value; }
        public UnitController TargetUnitController { get => targetUnitController; set => targetUnitController = value; }
        public bool TradeConfirmed { get => tradeConfirmed; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            characterManager = systemGameManager.CharacterManager;
            messageLogClient = systemGameManager.MessageLogClient;
        }

        public void AcceptTradeInvite(int characterId) {
            //Debug.Log($"TradeServiceClient.AcceptTradeInvite({characterId})");

            this.targetCharacterId = characterId;
            targetUnitController = characterManager.GetUnitController(UnitControllerMode.Player, characterId);
            messageLogClient.WriteSystemMessage($"You are now trading with {targetUnitController.DisplayName}");
            OnStartTradeSession();
        }

        public void RequestBeginTrade(int characterId) {
            //Debug.Log($"TradeServiceClient.RequestBeginTrade({characterId})");

            networkManagerClient.RequestBeginTrade(characterId);
        }

        public void RequestDeclineTrade() {
            //Debug.Log($"TradeServiceClient.RequestDeclineTrade()");

            ResetTradeSettings();
            networkManagerClient.RequestDeclineTrade();
        }

        public void RequestAcceptTrade() {
            //Debug.Log($"TradeServiceClient.RequestAcceptTrade()");

            networkManagerClient.RequestAcceptTrade();
        }

        public void DeclineTradeInvite() {
            //Debug.Log($"TradeServiceClient.DeclineTradeInvite()");

            ResetTradeSettings();
            messageLogClient.WriteSystemMessage("Your trade invitation was declined");
        }

        public void AdvertiseRequestBeginTrade(int sourceCharacterId) {
            //Debug.Log($"TradeServiceClient.AdvertiseRequestBeginTrade({sourceCharacterId})");

            this.targetCharacterId = sourceCharacterId;
            targetUnitController = characterManager.GetUnitController(UnitControllerMode.Player, sourceCharacterId);
            OnRequestBeginTrade();
        }

        public void RequestAddItemsToTradeSlot(int buttonIndex, List<long> itemInstanceIdList) {
            //Debug.Log($"TradeServiceClient.RequestAddItemsToTradeSlot({buttonIndex})");

            networkManagerClient.RequestAddItemsToTradeSlot(buttonIndex, itemInstanceIdList);
            OnRequestAddItemsToTargetTradeSlot();
        }

        public void AddItemsToTargetTradeSlot(int buttonIndex, List<long> itemInstanceIdList) {
            //Debug.Log($"TradeServiceClient.AddItemsToTargetTradeSlot({buttonIndex})");

            List<InstantiatedItem> itemList = new List<InstantiatedItem>();
            foreach (long itemInstanceId in itemInstanceIdList) {
                InstantiatedItem item = systemItemManager.GetExistingInstantiatedItem(itemInstanceId);
                if (item != null) {
                    itemList.Add(item);
                }
            }

            OnAddItemsToTargetTradeSlot(buttonIndex, itemList);
        }

        public void AddCurrency(CurrencyNode currencyNode) {
            //Debug.Log($"TradeServiceClient.AddCurrency({currencyNode})");

            if (targetCharacterId == -1) {
                return;
            }

            if (currencyNode.Amount == currencyAmount) {
                return;
            }

            currencyAmount = currencyNode.Amount;

            networkManagerClient.RequestAddCurrencyToTrade(currencyNode);
        }

        public void AdvertiseAddCurrencyToTrade(int amount) {
            //Debug.Log($"TradeServiceClient.AdvertiseAddCurrencyToTrade({amount})");

            OnAddCurrencyToTrade(amount);
        }

        public void RequestConfirmTrade() {
            //Debug.Log($"TradeServiceClient.RequestConfirmTrade()");

            tradeConfirmed = true;
            networkManagerClient.RequestConfirmTrade();
        }

        public void UnconfirmTrade() {
            //Debug.Log($"TradeServiceClient.UnconfirmTrade()");

            tradeConfirmed = false;
            networkManagerClient.RequestUnconfirmTrade();
        }

        public void ResetTradeSettings() {
            //Debug.Log($"TradeServiceClient.ResetTradeSettings()");

            targetCharacterId = 0;
            targetUnitController = null;
            tradeConfirmed = false;
            currencyAmount = 0;
        }

        public void RequestCancelTrade() {
            //Debug.Log($"TradeServiceClient.RequestCancelTrade()");

            if (targetCharacterId == -1) {
                // trade is not active or has already been cancelled
                return;
            }

            ResetTradeSettings();

            networkManagerClient.RequestCancelTrade();
        }

        public void AdvertiseCancelTrade() {
            //Debug.Log($"TradeServiceClient.AdvertiseCancelTrade()");

            ResetTradeSettings();
            messageLogClient.WriteSystemMessage("The trade was cancelled");
            OnCancelTrade();
        }

        public void AdvertiseTradeComplete() {
            //Debug.Log($"TradeServiceClient.AdvertiseTradeComplete()");

            ResetTradeSettings();
            messageLogClient.WriteSystemMessage("The trade was completed");
            OnCompleteTrade();
        }
    }

}