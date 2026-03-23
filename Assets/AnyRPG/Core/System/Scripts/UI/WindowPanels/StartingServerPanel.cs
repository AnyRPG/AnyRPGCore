using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class StartingServerPanel : WindowPanel {

        [Header("Starting Server Panel")]

        [SerializeField]
        private Toggle loadingItemsToggle = null;

        [SerializeField]
        private TextMeshProUGUI loadingItemsText = null;

        [SerializeField]
        private Toggle loadingPlayersToggle = null;

        [SerializeField]
        private TextMeshProUGUI loadingPlayersText = null;

        [SerializeField]
        private Toggle loadingGuildsToggle = null;

        [SerializeField]
        private TextMeshProUGUI loadingGuildsText = null;

        [SerializeField]
        private Toggle loadingFriendsToggle = null;

        [SerializeField]
        private TextMeshProUGUI loadingFriendsText = null;

        [SerializeField]
        private Toggle loadingAuctionItemsToggle = null;

        [SerializeField]
        private TextMeshProUGUI loadingAuctionItemsText = null;

        [SerializeField]
        private HighlightButton confirmButton = null;


        // game manager references
        private NetworkManagerServer networkManagerServer = null;
        private ServerDataService serverDataService = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            networkManagerServer.OnBeforeStartServer += HandleBeforeStartServer;
            serverDataService.OnBeforeLoadAuctionItems += HandleBeforeLoadAuctionItems;
            serverDataService.OnLoadAuctionItem += HandleLoadAuctionItem;
            serverDataService.OnLoadAuctionItems += HandleLoadAuctionItems;
            serverDataService.OnBeforeLoadItems += HandleBeforeLoadItems;
            serverDataService.OnLoadItem += HandleLoadItem;
            serverDataService.OnLoadItems += HandleLoadItems;
            serverDataService.OnBeforeLoadPlayerNameMap += HandleBeforeLoadPlayerNameMap;
            serverDataService.OnLoadPlayerName += HandleLoadPlayerName;
            serverDataService.OnLoadPlayerNameMap += HandleLoadPlayerNameMap;
            serverDataService.OnBeforeLoadGuilds += HandleBeforeLoadGuilds;
            serverDataService.OnLoadGuild += HandleLoadGuild;
            serverDataService.OnLoadGuilds += HandleLoadGuilds;
            serverDataService.OnBeforeLoadFriends += HandleBeforeLoadFriends;
            serverDataService.OnLoadFriend += HandleLoadFriend;
            serverDataService.OnLoadFriends += HandleLoadFriends;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            networkManagerServer = systemGameManager.NetworkManagerServer;
            serverDataService = systemGameManager.ServerDataService;
        }

        private void HandleBeforeStartServer() {
            //Debug.Log("StartingServerPanel.HandleBeforeStartServer()");

            confirmButton.Button.interactable = false;
            loadingItemsText.text = "Items: waiting to load";
            loadingPlayersText.text = "Player names: waiting to load";
            loadingGuildsText.text = "Guilds: waiting to load";
            loadingFriendsText.text = "Friends: waiting to load";
            loadingAuctionItemsText.text = "Auction items: waiting to load";
            loadingAuctionItemsToggle.isOn = false;
            loadingItemsToggle.isOn = false;
            loadingPlayersToggle.isOn = false;
            loadingGuildsToggle.isOn = false;
            loadingFriendsToggle.isOn = false;
            Open();
        }

        private void CheckConfirmButtonStatus() {
            if (serverDataService.IsServerDataLoaded()) {
                confirmButton.Button.interactable = true;
            }
        }

        private void HandleLoadFriend(int count) {
            //Debug.Log($"StartingServerPanel.HandleLoadFriend({count})");

            loadingFriendsText.text = $"Friends: {count} loaded...";
        }

        private void HandleLoadFriends(int count) {
            //Debug.Log($"StartingServerPanel.HandleLoadFriends({count})");

            loadingFriendsText.text = $"Friends: {count} loaded.";
            loadingFriendsToggle.isOn = true;
            CheckConfirmButtonStatus();
        }

        private void HandleBeforeLoadFriends() {
            //Debug.Log($"StartingServerPanel.HandleBeforeLoadFriends()");

            loadingFriendsText.text = "Friends: loading...";
        }

        private void HandleLoadGuild(int count) {
            //Debug.Log($"StartingServerPanel.HandleLoadGuild({count})");

            loadingItemsText.text = $"Guilds: {count} loaded...";
        }

        private void HandleLoadGuilds(int count) {
            //Debug.Log($"StartingServerPanel.HandleLoadGuilds({count})");

            loadingGuildsText.text = $"Guilds: {count} loaded.";
            loadingGuildsToggle.isOn = true;
            CheckConfirmButtonStatus();
        }

        private void HandleBeforeLoadGuilds() {
            //Debug.Log($"StartingServerPanel.HandleBeforeLoadGuilds()");

            loadingGuildsText.text = "Guilds: loading...";
        }

        private void HandleLoadPlayerName(int count) {
            //Debug.Log($"StartingServerPanel.HandleLoadPlayerName({count})");

            loadingPlayersText.text = $"Player names: {count} loaded...";
        }

        private void HandleLoadPlayerNameMap(int count) {
            //Debug.Log($"StartingServerPanel.HandleLoadPlayerNameMap({count})");

            loadingPlayersText.text = $"Player names: {count} loaded.";
            loadingPlayersToggle.isOn = true;
            CheckConfirmButtonStatus();
        }

        private void HandleBeforeLoadPlayerNameMap() {
            //Debug.Log($"StartingServerPanel.HandleBeforeLoadPlayerNameMap()");

            loadingPlayersText.text = "Player names: loading...";
        }

        private void HandleLoadItem(int count) {
            //Debug.Log($"StartingServerPanel.HandleLoadItem({count})");

            loadingItemsText.text = $"Items: {count} loaded...";
        }

        private void HandleLoadItems(int count) {
            //Debug.Log($"StartingServerPanel.HandleLoadItems({count})");

            loadingItemsText.text = $"Items: {count} loaded.";
            loadingItemsToggle.isOn = true;
            CheckConfirmButtonStatus();
        }

        private void HandleBeforeLoadItems() {
            //Debug.Log($"StartingServerPanel.HandleBeforeLoadItems()");

            loadingItemsText.text = "Items: loading...";
        }

        private void HandleLoadAuctionItem(int count) {
            //Debug.Log($"StartingServerPanel.HandleLoadAuctionItem({count})");

            loadingAuctionItemsText.text = $"Auction items: {count} loaded...";
        }

        private void HandleLoadAuctionItems(int count) {
            //Debug.Log($"StartingServerPanel.HandleLoadAuctionItems({count})");

            loadingAuctionItemsText.text = $"Auction items: {count} loaded.";
            loadingAuctionItemsToggle.isOn = true;
            CheckConfirmButtonStatus();
        }

        private void HandleBeforeLoadAuctionItems() {
            //Debug.Log($"StartingServerPanel.HandleBeforeLoadAuctionItems()");

            loadingAuctionItemsText.text = "Auction items: loading...";
        }

        public void HandleRequestBeginTrade() {
            Open();
        }

        /*
        public override void ProcessOpenWindowNotification() {
            base.ProcessOpenWindowNotification();
        }
        */

        /*
        public void CancelAction() {
            //Debug.Log("NewGameMenuController.CancelAction()");

            Close();
        }
        */

        public void ConfirmAction() {
            //Debug.Log("ConfirmOpenTradePanel.ConfirmAction()");

            Close();
        }

    }

}