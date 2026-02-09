using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;

namespace AnyRPG {
    public class HostServerPanel : WindowPanel {

        [Header("HostGamePanelController")]

        [SerializeField]
        protected TextMeshProUGUI serverStatusText = null;

        [SerializeField]
        protected GameObject gameListingTemplate = null;

        [SerializeField]
        protected Transform gameListingContainer = null;

        [SerializeField]
        private TMP_Dropdown serverModeDropdown = null;

        [SerializeField]
        private WindowPanel optionsPanel = null;

        [SerializeField]
        private HostServerPlayersPanel playersPanel = null;

        [SerializeField]
        private WindowPanel lobbyGamesPanel = null;

        [SerializeField]
        private HostServerScenesPanel scenesPanel = null;

        //[SerializeField]
        //protected HighlightButton returnButton = null;

        [SerializeField]
        protected HighlightButton optionsButton = null;

        [SerializeField]
        protected HighlightButton playersButton = null;

        [SerializeField]
        protected HighlightButton scenesButton = null;

        [SerializeField]
        protected HighlightButton lobbyGamesButton = null;

        [SerializeField]
        protected HighlightButton startServerButton = null;

        [SerializeField]
        protected HighlightButton stopServerButton = null;

        [SerializeField]
        protected UINavigationController panelsNavigationController = null;

        private Dictionary<int, ServerLobbyGameConnectionButtonController> lobbyGameButtons = new Dictionary<int, ServerLobbyGameConnectionButtonController>();

        // game manager references
        protected UIManager uIManager = null;
        protected ObjectPooler objectPooler = null;
        protected SystemDataFactory systemDataFactory = null;
        protected NetworkManagerServer networkManagerServer = null;
        protected LevelManager levelManager = null;
        protected AuthenticationService authenticationService = null;
        protected LevelManagerServer levelManagerServer = null;

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
            levelManager = systemGameManager.LevelManager;
            authenticationService = systemGameManager.AuthenticationService;
            levelManagerServer = systemGameManager.LevelManagerServer;
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
            playersPanel.ClearPlayerList();
        }

        public void HandleAccountLogin(int accountId) {
            //Debug.Log($"HostServerPanelController.HandlePlayerLogin({accountId})");

            playersPanel.AddPlayerToList(accountId, authenticationService.LoggedInAccounts[accountId].username);
        }

        public void HandleAccountLogout(int accountId) {
            //Debug.Log($"HostServerPanelController.HandleLobbyLogout({accountId})");

            playersPanel.RemovePlayerFromList(accountId);
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
            playersPanel.PopulatePlayerList();
            networkManagerServer.OnStartServer += HandleStartServer;
            networkManagerServer.OnStopServer += HandleStopServer;
            authenticationService.OnAccountLogin += HandleAccountLogin;
            authenticationService.OnAccountLogout += HandleAccountLogout;
            networkManagerServer.OnCreateLobbyGame += HandleCreateLobbyGame;
            networkManagerServer.OnCancelLobbyGame += HandleCancelLobbyGame;
            networkManagerServer.OnJoinLobbyGame += HandleJoinLobbyGame;
            networkManagerServer.OnLeaveLobbyGame += HandleLeaveLobbyGame;
            networkManagerServer.OnStartLobbyGame += HandleStartLobbyGame;
            levelManagerServer.OnAddLoadedScene += HandleAddLoadedScene;
            levelManagerServer.OnRemoveLoadedScene += HandleRemoveLoadedScene;
            levelManagerServer.OnSetSceneClientCount += HandleSetSceneClientCount;

            OpenOptionsPanel();
        }

        public override void ReceiveClosedWindowNotification() {
            base.ReceiveClosedWindowNotification();
            networkManagerServer.OnStartServer -= HandleStartServer;
            networkManagerServer.OnStopServer -= HandleStopServer;
            authenticationService.OnAccountLogin -= HandleAccountLogin;
            authenticationService.OnAccountLogout -= HandleAccountLogout;
            networkManagerServer.OnCreateLobbyGame -= HandleCreateLobbyGame;
            networkManagerServer.OnCancelLobbyGame -= HandleCancelLobbyGame;
            networkManagerServer.OnJoinLobbyGame -= HandleJoinLobbyGame;
            networkManagerServer.OnLeaveLobbyGame -= HandleLeaveLobbyGame;
            networkManagerServer.OnStartLobbyGame -= HandleStartLobbyGame;
            levelManagerServer.OnAddLoadedScene -= HandleAddLoadedScene;
            levelManagerServer.OnRemoveLoadedScene -= HandleRemoveLoadedScene;
            levelManagerServer.OnSetSceneClientCount -= HandleSetSceneClientCount;

            playersPanel.ClearPlayerList();

            // ensure you can't accidentally try to run the server and client in the same window
            if (networkManagerServer.ServerModeActive == true) {
                StopServer();
            }
        }

        private void HandleSetSceneClientCount(int sceneHandle, int clientCount) {
            scenesPanel.SetSceneClientCount(sceneHandle, clientCount);
        }

        private void HandleRemoveLoadedScene(int sceneHandle, string sceneName) {
            scenesPanel.RemoveSceneFromList(sceneHandle);
        }

        private void HandleAddLoadedScene(int sceneHandle, SceneData data) {
            scenesPanel.AddSceneToList(sceneHandle, data);
        }

        public void SetServerMode(int dropdownIndex) {
            //Debug.Log($"HostServerPanel.SetServerMode({dropdownIndex})");

            if (serverModeDropdown.options[serverModeDropdown.value].text == "Lobby") {
                networkManagerServer.SetServerMode(NetworkServerMode.Lobby);
            } else {
                networkManagerServer.SetServerMode(NetworkServerMode.MMO);
            }
        }

        public void SetServerPort(string newServerPort) {
            /*
            if (isResettingInputText == true) {
                return;
            }
            */
            ushort port = 7770;
            if (ushort.TryParse(newServerPort, out port) == false) {
                return;
            }
            networkManagerServer.SetServerPort(port);
        }

        private void ClosePanels(CloseableWindowContents skipPanel) {
            if (skipPanel != optionsPanel) {
                optionsPanel.HidePanel();
            }
            if (skipPanel != playersPanel) {
                playersPanel.HidePanel();
            }
            if (skipPanel != lobbyGamesPanel) {
                lobbyGamesPanel.HidePanel();
            }
            if (skipPanel != scenesPanel) {
                scenesPanel.HidePanel();
            }
        }

        public void OpenOptionsPanel() {
            if (openSubPanel != optionsPanel) {
                ClosePanels(optionsPanel);
                optionsPanel.ShowPanel();
                SetOpenSubPanel(optionsPanel, false);
            }

            optionsButton.HighlightBackground();
            panelsNavigationController.UnHightlightButtonBackgrounds(optionsButton);
        }

        public void OpenPlayersPanel() {
            if (openSubPanel != playersPanel) {
                ClosePanels(playersPanel);
                playersPanel.ShowPanel();
                SetOpenSubPanel(playersPanel, false);
            }
            playersButton.HighlightBackground();
            panelsNavigationController.UnHightlightButtonBackgrounds(playersButton);
        }

        public void OpenLobbyGamesPanel() {
            if (openSubPanel != lobbyGamesPanel) {
                ClosePanels(lobbyGamesPanel);
                lobbyGamesPanel.ShowPanel();
                SetOpenSubPanel(lobbyGamesPanel, false);
            }
            lobbyGamesButton.HighlightBackground();
            panelsNavigationController.UnHightlightButtonBackgrounds(lobbyGamesButton);
        }

        public void OpenScenesPanel() {
            if (openSubPanel != scenesPanel) {
                ClosePanels(scenesPanel);
                scenesPanel.ShowPanel();
                SetOpenSubPanel(scenesPanel, false);
            }
            scenesButton.HighlightBackground();
            panelsNavigationController.UnHightlightButtonBackgrounds(scenesButton);
        }

    }
}