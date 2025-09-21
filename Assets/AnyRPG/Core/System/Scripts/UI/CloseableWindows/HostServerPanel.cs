using AnyRPG;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;

namespace AnyRPG {
    public class HostServerPanel : WindowContentController {

        [Header("HostGamePanelController")]

        [SerializeField]
        protected TextMeshProUGUI serverStatusText = null;

        [SerializeField]
        protected GameObject playerConnectionTemplate = null;

        [SerializeField]
        protected Transform playerConnectionContainer = null;

        [SerializeField]
        protected GameObject gameListingTemplate = null;

        [SerializeField]
        protected Transform gameListingContainer = null;


        //[SerializeField]
        //protected HighlightButton returnButton = null;

        [SerializeField]
        protected HighlightButton startServerButton = null;

        [SerializeField]
        protected HighlightButton stopServerButton = null;

        /// <summary>
        /// accountId, PlayerConnectionButtonController
        /// </summary>
        private Dictionary<int, PlayerConnectionButtonController> playerButtons = new Dictionary<int, PlayerConnectionButtonController>();

        private Dictionary<int, ServerLobbyGameConnectionButtonController> lobbyGameButtons = new Dictionary<int, ServerLobbyGameConnectionButtonController>();

        // game manager references
        protected UIManager uIManager = null;
        protected ObjectPooler objectPooler = null;
        protected SystemDataFactory systemDataFactory = null;
        protected NetworkManagerServer networkManagerServer = null;
        protected SystemEventManager systemEventManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            stopServerButton.Button.interactable = false;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            objectPooler = systemGameManager.ObjectPooler;
            systemDataFactory = systemGameManager.SystemDataFactory;
            networkManagerServer = systemGameManager.NetworkManagerServer;
            systemEventManager = systemGameManager.SystemEventManager;
        }


        public void PopulatePlayerList() {
            //Debug.Log($"HostServerPanelController.PopulatePlayerList()");

            foreach (KeyValuePair<int, LoggedInAccount> loggedInAccount in networkManagerServer.LoggedInAccounts) {
                AddPlayerToList(loggedInAccount.Value.accountId, loggedInAccount.Value.username);
            }
        }

        public void AddPlayerToList(int accountId, string userName) {
            //Debug.Log($"HostServerPanelController.AddPlayerToList({accountId}, {userName})");

            if (playerButtons.ContainsKey(accountId)) {
                //Debug.Warning($"HostServerPanelController.AddPlayerToList() - player was already connected, and is reconnecting");
                playerButtons[accountId].UpdateIPAddress(networkManagerServer.LoggedInAccounts[accountId].ipAddress);
                return;
            }
            GameObject go = objectPooler.GetPooledObject(playerConnectionTemplate, playerConnectionContainer);
            PlayerConnectionButtonController playerConnectionButtonController = go.GetComponent<PlayerConnectionButtonController>();
            playerConnectionButtonController.Configure(systemGameManager);
            playerConnectionButtonController.SetAccountId(accountId, userName, networkManagerServer.LoggedInAccounts[accountId].ipAddress);
            uINavigationControllers[1].AddActiveButton(playerConnectionButtonController.KickButton);
            playerButtons.Add(accountId, playerConnectionButtonController);
        }

        public void RemovePlayerFromList(int accountId) {
            //Debug.Log($"HostServerPanelController.RemovePlayerFromList({accountId})");

            if (playerButtons.ContainsKey(accountId)) {
                uINavigationControllers[1].ClearActiveButton(playerButtons[accountId].KickButton);
                if (playerButtons[accountId].gameObject != null) {
                    playerButtons[accountId].gameObject.transform.SetParent(null);
                    objectPooler.ReturnObjectToPool(playerButtons[accountId].gameObject);
                }
                playerButtons.Remove(accountId);
            }
        }

        public void ClearPlayerList() {

            // clear the skill list so any skill left over from a previous time opening the window aren't shown
            foreach (PlayerConnectionButtonController playerConnectionButtonController in playerButtons.Values) {
                if (playerConnectionButtonController.gameObject != null) {
                    playerConnectionButtonController.gameObject.transform.SetParent(null);
                    objectPooler.ReturnObjectToPool(playerConnectionButtonController.gameObject);
                }
            }
            playerButtons.Clear();
            uINavigationControllers[1].ClearActiveButtons();
        }

        public void CloseMenu() {
            if (networkManagerServer.ServerModeActive == true) {
                uIManager.confirmStopServerWindow.OpenWindow();
                return;
            }

            uIManager.hostServerWindow.CloseWindow();
        }

        public void StartServer() {
            //Debug.Log($"HostServerPanelController.StartServer()");

            networkManagerServer.ClientMode = NetworkClientMode.Lobby;
            networkManagerServer.StartServer();
        }

        public void StopServer() {
            //Debug.Log($"HostServerPanelController.StopServer()");
            
            uIManager.confirmStopServerWindow.OpenWindow();
            //networkManagerServer.StopServer();
        }

        public void HandleStartServer() {
            serverStatusText.text = $"Server Status: Online\nListening on Port: {networkManagerServer.GetServerPort()}";
            startServerButton.Button.interactable = false;
            stopServerButton.Button.interactable = true;
        }

        public void HandleStopServer() {
            serverStatusText.text = "Server Status: Offline";
            startServerButton.Button.interactable = true;
            stopServerButton.Button.interactable = false;
            ClearPlayerList();
        }

        public void HandleLobbyLogin(int accountId) {
            //Debug.Log($"HostServerPanelController.HandleLobbyLogin({accountId})");

            AddPlayerToList(accountId, networkManagerServer.LoggedInAccounts[accountId].username);
        }

        public void HandleLobbyLogout(int accountId) {
            //Debug.Log($"HostServerPanelController.HandleLobbyLogout({accountId})");

            RemovePlayerFromList(accountId);
        }

        public void HandleCreateLobbyGame(LobbyGame lobbyGame) {
            AddLobbyGameToList(lobbyGame.gameId, lobbyGame);
        }

        public void AddLobbyGameToList(int gameId, LobbyGame lobbyGame) {
            //Debug.Log($"ClientLobbyPanelController.AddLobbyGameToList({gameId})");

            GameObject go = objectPooler.GetPooledObject(gameListingTemplate, gameListingContainer);
            ServerLobbyGameConnectionButtonController serverLobbyGameButtonController = go.GetComponent<ServerLobbyGameConnectionButtonController>();
            serverLobbyGameButtonController.Configure(systemGameManager);
            serverLobbyGameButtonController.SetGame(lobbyGame);
            //uINavigationControllers[1].AddActiveButton(serverLobbyGameButtonController.JoinButton);
            lobbyGameButtons.Add(gameId, serverLobbyGameButtonController);
        }

        public void HandleJoinLobbyGame(int gameId, int accountId, string userName) {
            if (lobbyGameButtons.ContainsKey(gameId)) {
                lobbyGameButtons[gameId].RefreshPlayerCount();
            }
        }

        public void HandleLeaveLobbyGame(int gameId, int accountId) {
            if (lobbyGameButtons.ContainsKey(gameId)) {
                lobbyGameButtons[gameId].RefreshPlayerCount();
            }
        }

        public void HandleStartLobbyGame(int gameId) {
            if (lobbyGameButtons.ContainsKey(gameId)) {
                lobbyGameButtons[gameId].RefreshStatus();
            }
        }

        public void HandleCancelLobbyGame(int gameId) {
            //Debug.Log($"HostServerPanelController.HandleCancelLobbyGame({gameId})");
            RemoveLobbyGameFromList(gameId);
        }

        public void RemoveLobbyGameFromList(int gameId) {
            if (lobbyGameButtons.ContainsKey(gameId)) {
                objectPooler.ReturnObjectToPool(lobbyGameButtons[gameId].gameObject);
                lobbyGameButtons.Remove(gameId);
            }
        }

        public override void ProcessOpenWindowNotification() {
            base.ProcessOpenWindowNotification();
            PopulatePlayerList();
            systemEventManager.OnStartServer += HandleStartServer;
            systemEventManager.OnStopServer += HandleStopServer;
            networkManagerServer.OnLobbyLogin += HandleLobbyLogin;
            networkManagerServer.OnLobbyLogout += HandleLobbyLogout;
            networkManagerServer.OnCreateLobbyGame += HandleCreateLobbyGame;
            networkManagerServer.OnCancelLobbyGame += HandleCancelLobbyGame;
            networkManagerServer.OnJoinLobbyGame += HandleJoinLobbyGame;
            networkManagerServer.OnLeaveLobbyGame += HandleLeaveLobbyGame;
            networkManagerServer.OnStartLobbyGame += HandleStartLobbyGame;
        }

        public override void ReceiveClosedWindowNotification() {
            base.ReceiveClosedWindowNotification();
            systemEventManager.OnStartServer -= HandleStartServer;
            systemEventManager.OnStopServer -= HandleStopServer;
            networkManagerServer.OnLobbyLogin -= HandleLobbyLogin;
            networkManagerServer.OnLobbyLogout -= HandleLobbyLogout;
            networkManagerServer.OnCreateLobbyGame -= HandleCreateLobbyGame;
            networkManagerServer.OnCancelLobbyGame -= HandleCancelLobbyGame;
            networkManagerServer.OnJoinLobbyGame -= HandleJoinLobbyGame;
            networkManagerServer.OnLeaveLobbyGame -= HandleLeaveLobbyGame;
            networkManagerServer.OnStartLobbyGame -= HandleStartLobbyGame;

            ClearPlayerList();

            // ensure you can't accidentally try to run the server and client in the same window
            if (networkManagerServer.ServerModeActive == true) {
                StopServer();
            }
        }
    }
}