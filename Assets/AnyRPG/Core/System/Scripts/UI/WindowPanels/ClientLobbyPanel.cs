using AnyRPG;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;

namespace AnyRPG {
    public class ClientLobbyPanel : WindowPanel {

        [Header("ClientLobbyPanelController")]

        [SerializeField]
        protected TextMeshProUGUI serverStatusText = null;

        [SerializeField]
        protected GameObject playerConnectionTemplate = null;

        [SerializeField]
        protected Transform playerConnectionContainer = null;

        [SerializeField]
        protected GameObject lobbyGameTemplate = null;

        [SerializeField]
        protected Transform lobbyGameContainer = null;

        [SerializeField]
        protected TMP_InputField chatInput = null;

        [SerializeField]
        protected TextMeshProUGUI chatDisplay = null;

        [SerializeField]
        protected HighlightButton logoutButton = null;

        [SerializeField]
        protected HighlightButton createGameButton = null;

        //[SerializeField]
        //protected HighlightButton joinGameButton = null;

        protected Dictionary<string, List<CreditsNode>> categoriesDictionary = new Dictionary<string, List<CreditsNode>>();

        private string lobbyChatText = string.Empty;
        private int maxLobbyChatTextSize = 64000;

        private Dictionary<int, ClientPlayerLobbyConnectionButton> playerButtons = new Dictionary<int, ClientPlayerLobbyConnectionButton>();
        private Dictionary<int, ClientLobbyGameConnectionButtonController> lobbyGameButtons = new Dictionary<int, ClientLobbyGameConnectionButtonController>();


        // game manager references
        protected UIManager uIManager = null;
        protected ObjectPooler objectPooler = null;
        protected SystemDataFactory systemDataFactory = null;
        protected NetworkManagerServer networkManagerServer = null;
        protected NetworkManagerClient networkManagerClient = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            chatDisplay.text = string.Empty;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            objectPooler = systemGameManager.ObjectPooler;
            systemDataFactory = systemGameManager.SystemDataFactory;
            networkManagerServer = systemGameManager.NetworkManagerServer;
            networkManagerClient = systemGameManager.NetworkManagerClient;
        }


        public void Logout() {
            networkManagerClient.RequestLogout();
            uIManager.clientLobbyWindow.CloseWindow();
        }

        public void CreateGame() {
            //Debug.Log($"ClientLobbyPanelController.CreateGame()");

            uIManager.createLobbyGameWindow.OpenWindow();
        }

        public void SetStatusLabel() {
            serverStatusText.text = $"Logged In As: {networkManagerClient.Username}";
        }

        public void HandleLobbyLogin(int accountId, string userName) {
            //Debug.Log($"ClientLobbyPanel.HandleLobbyLogin({accountId}, {userName})");

            AddPlayerToList(accountId, userName);
        }

        public void HandleLobbyLogout(int accountId) {
            RemovePlayerFromList(accountId);
        }

        public void HandleCreateLobbyGame(LobbyGame lobbyGame) {
            //Debug.Log($"ClientLobbyPanel.HandleCreateLobbyGame()");

            AddLobbyGameToList(lobbyGame.gameId, lobbyGame);
        }

        public void HandleCancelLobbyGame(int gameId) {
            RemoveLobbyGameFromList(gameId);
        }

        public void SendChatMessage() {
            networkManagerClient.SendLobbyChatMessage(chatInput.text);
            chatInput.text = string.Empty;
        }

        public void HandleSendLobbyChatMessage(string messageText) {
            lobbyChatText += messageText;
            while (lobbyChatText.Length > maxLobbyChatTextSize && lobbyChatText.Contains("\n")) {
                lobbyChatText = lobbyChatText.Split("\n", 1)[1];
            }
            chatDisplay.text = lobbyChatText;
        }

        public void RequestLobbyPlayerList() {
            //Debug.Log($"ClientLobbyPanelController.RequestLobbyPlayerList()");

            networkManagerClient.RequestLobbyPlayerList();
        }

        public void HandleSetLobbyPlayerList(Dictionary<int, string> userNames) {
            PopulatePlayerList(userNames);
        }


        public void PopulatePlayerList(Dictionary<int, string> userNames) {
            //Debug.Log($"ClientLobbyPanelController.PopulatePlayerList()");

            foreach (KeyValuePair<int, string> loggedInAccount in userNames) {
                AddPlayerToList(loggedInAccount.Key, loggedInAccount.Value);
            }
        }

        public void AddPlayerToList(int accountId, string userName) {
            //Debug.Log($"ClientLobbyPanelController.AddPlayerToList({accountId}, {userName})");

            if (playerButtons.ContainsKey(accountId)) {
                //Debug.Log($"ClientLobbyPanelController.AddPlayerToList({accountId}, {userName}) - already exists, account is reconnecting");
                return;
            }
            GameObject go = objectPooler.GetPooledObject(playerConnectionTemplate, playerConnectionContainer);
            ClientPlayerLobbyConnectionButton clientPlayerLobbyConnectionButtonController = go.GetComponent<ClientPlayerLobbyConnectionButton>();
            clientPlayerLobbyConnectionButtonController.Configure(systemGameManager);
            clientPlayerLobbyConnectionButtonController.SetAccountId(accountId, userName);
            //uINavigationControllers[1].AddActiveButton(clientPlayerLobbyConnectionButtonController.joinbu);
            playerButtons.Add(accountId, clientPlayerLobbyConnectionButtonController);
        }

        public void RemovePlayerFromList(int accountId) {
            //Debug.Log($"ClientLobbyPanelController.RemovePlayerFromList({clientId})");

            if (playerButtons.ContainsKey(accountId)) {
                //uINavigationControllers[1].ClearActiveButton(playerButtons[clientId].KickButton);
                if (playerButtons[accountId].gameObject != null) {
                    playerButtons[accountId].gameObject.transform.SetParent(null);
                    objectPooler.ReturnObjectToPool(playerButtons[accountId].gameObject);
                }
            }
        }

        public void ClearPlayerList() {

            // clear the skill list so any skill left over from a previous time opening the window aren't shown
            foreach (ClientPlayerLobbyConnectionButton clientPlayerLobbyConnectionButtonController in playerButtons.Values) {
                if (clientPlayerLobbyConnectionButtonController.gameObject != null) {
                    clientPlayerLobbyConnectionButtonController.gameObject.transform.SetParent(null);
                    objectPooler.ReturnObjectToPool(clientPlayerLobbyConnectionButtonController.gameObject);
                }
            }
            playerButtons.Clear();
            //uINavigationControllers[1].ClearActiveButtons();
        }


        public void RequestLobbyGameList() {
            //Debug.Log($"ClientLobbyPanelController.RequestLobbyGameList()");

            networkManagerClient.RequestLobbyGameList();
        }

        public void HandleSetLobbyGameList(List<LobbyGame> lobbyGames) {
            //Debug.Log($"ClientLobbyPanelController.HandleSetLobbyGameList()");

            PopulateLobbyGameList(lobbyGames);
            
            // in an account reconnection, we need to rejoin the game we were in
            foreach (LobbyGame lobbyGame in lobbyGames) {
                if (lobbyGame.PlayerList.ContainsKey(networkManagerClient.AccountId) == false) {
                    //Debug.Log($"ClientLobbyPanelController.HandleSetLobbyGameList() - account {networkManagerClient.AccountId} not in game {lobbyGame.gameId}");
                    continue;
                }
                if (lobbyGame.inProgress) {
                    //Debug.Log($"ClientLobbyPanelController.HandleSetLobbyGameList() - rejoining game {lobbyGame.gameId}");
                    if (lobbyGame.PlayerList[networkManagerClient.AccountId].ready) {
                        //Debug.Log($"ClientLobbyPanelController.HandleSetLobbyGameList() - account {networkManagerClient.AccountId} is ready in game {lobbyGame.gameId}");
                        networkManagerClient.SetLobbyGame(lobbyGame);
                        networkManagerClient.RequestJoinLobbyGameInProgress(lobbyGame.gameId);
                    } else {
                        //Debug.Log($"ClientLobbyPanelController.HandleSetLobbyGameList() - account {networkManagerClient.AccountId} is not ready in game {lobbyGame.gameId}");
                        networkManagerClient.SetLobbyGame(lobbyGame);
                        uIManager.clientLobbyGameWindow.OpenWindow();
                    }
                } else {
                    //Debug.Log($"ClientLobbyPanelController.HandleSetLobbyGameList() - starting game {lobbyGame.gameId}");
                    networkManagerClient.SetLobbyGame(lobbyGame);
                    uIManager.clientLobbyGameWindow.OpenWindow();
                }
            }
        }

        public void PopulateLobbyGameList(List<LobbyGame> lobbyGames) {
            //Debug.Log($"ClientLobbyPanelController.PopulateLobbyGameList()");

            foreach (LobbyGame lobbyGame in lobbyGames) {
                AddLobbyGameToList(lobbyGame.gameId, lobbyGame);
            }
        }

        public void AddLobbyGameToList(int gameId, LobbyGame lobbyGame) {
            //Debug.Log($"ClientLobbyPanelController.AddLobbyGameToList(gameId: {gameId})");

            GameObject go = objectPooler.GetPooledObject(lobbyGameTemplate, lobbyGameContainer);
            ClientLobbyGameConnectionButtonController clientLobbyGameConnectionButtonController = go.GetComponent<ClientLobbyGameConnectionButtonController>();
            clientLobbyGameConnectionButtonController.Configure(systemGameManager);
            clientLobbyGameConnectionButtonController.SetGame(lobbyGame);
            uINavigationControllers[1].AddActiveButton(clientLobbyGameConnectionButtonController.JoinButton);
            lobbyGameButtons.Add(gameId, clientLobbyGameConnectionButtonController);
        }

        public void RemoveLobbyGameFromList(int gameId) {
            //Debug.Log($"ClientLobbyPanelController.RemoveLobbyGameFromList({gameId})");

            if (lobbyGameButtons.ContainsKey(gameId)) {
                uINavigationControllers[1].ClearActiveButton(lobbyGameButtons[gameId].JoinButton);
                if (lobbyGameButtons[gameId].gameObject != null) {
                    lobbyGameButtons[gameId].gameObject.transform.SetParent(null);
                    objectPooler.ReturnObjectToPool(lobbyGameButtons[gameId].gameObject);
                }
            }
        }

        public void ClearLobbyGameList() {

            // clear the list so any button left over from a previous time opening the window aren't shown
            foreach (ClientLobbyGameConnectionButtonController clientLobbyGameConnectionButtonController in lobbyGameButtons.Values) {
                if (clientLobbyGameConnectionButtonController.gameObject != null) {
                    clientLobbyGameConnectionButtonController.gameObject.transform.SetParent(null);
                    objectPooler.ReturnObjectToPool(clientLobbyGameConnectionButtonController.gameObject);
                }
            }
            lobbyGameButtons.Clear();
            uINavigationControllers[1].ClearActiveButtons();
        }

        private void HandleJoinLobbyGame(int gameId, int accountId, string userName) {
            //Debug.Log($"ClientLobbyPanelController.HandleJoinLobbyGame({gameId}, {clientId}, {userName})");
            if (lobbyGameButtons.ContainsKey(gameId)) {
                lobbyGameButtons[gameId].RefreshPlayerCount();
            }
        }

        private void HandleLeaveLobbyGame(int gameId, int accountId) {
            //Debug.Log($"ClientLobbyPanelController.HandleLeaveLobbyGame({gameId}, {clientId})");
            if (lobbyGameButtons.ContainsKey(gameId)) {
                lobbyGameButtons[gameId].RefreshPlayerCount();
            }
        }

        private void HandleStartLobbyGame(int gameId) {
            //Debug.Log($"ClientLobbyPanelController.HandleStartLobbyGame({gameId})");
            if (lobbyGameButtons.ContainsKey(gameId)) {
                lobbyGameButtons[gameId].RefreshStatus();
            }
        }

        public override void ProcessOpenWindowNotification() {
            base.ProcessOpenWindowNotification();
            SetStatusLabel();
            RequestLobbyPlayerList();
            RequestLobbyGameList();
            networkManagerClient.OnSendLobbyChatMessage += HandleSendLobbyChatMessage;
            networkManagerClient.OnLobbyLogin += HandleLobbyLogin;
            networkManagerClient.OnLobbyLogout += HandleLobbyLogout;
            networkManagerClient.OnCreateLobbyGame += HandleCreateLobbyGame;
            networkManagerClient.OnCancelLobbyGame += HandleCancelLobbyGame;
            networkManagerClient.OnSetLobbyGameList += HandleSetLobbyGameList;
            networkManagerClient.OnSetLobbyPlayerList += HandleSetLobbyPlayerList;
            networkManagerClient.OnJoinLobbyGame += HandleJoinLobbyGame;
            networkManagerClient.OnLeaveLobbyGame += HandleLeaveLobbyGame;
            networkManagerClient.OnStartLobbyGame += HandleStartLobbyGame;
        }

        public override void ReceiveClosedWindowNotification() {
            base.ReceiveClosedWindowNotification();
            networkManagerClient.OnSendLobbyChatMessage -= HandleSendLobbyChatMessage;
            networkManagerClient.OnLobbyLogin -= HandleLobbyLogin;
            networkManagerClient.OnLobbyLogout -= HandleLobbyLogout;
            networkManagerClient.OnCreateLobbyGame -= HandleCreateLobbyGame;
            networkManagerClient.OnCancelLobbyGame -= HandleCancelLobbyGame;
            networkManagerClient.OnSetLobbyGameList -= HandleSetLobbyGameList;
            networkManagerClient.OnSetLobbyPlayerList -= HandleSetLobbyPlayerList;
            networkManagerClient.OnJoinLobbyGame -= HandleJoinLobbyGame;
            networkManagerClient.OnLeaveLobbyGame -= HandleLeaveLobbyGame;
            networkManagerClient.OnStartLobbyGame -= HandleStartLobbyGame;

            ClearPlayerList();
            ClearLobbyGameList();
        }
    }
}