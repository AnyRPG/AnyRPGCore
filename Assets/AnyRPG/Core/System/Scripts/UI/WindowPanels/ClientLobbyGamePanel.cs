using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;
using FishNet.Serializing;

namespace AnyRPG {
    public class ClientLobbyGamePanel : WindowPanel {

        [Header("Client Lobby Game Panel")]

        [SerializeField]
        protected TextMeshProUGUI serverStatusText = null;

        [SerializeField]
        protected GameObject playerConnectionTemplate = null;

        [SerializeField]
        protected Transform playerConnectionContainer = null;

        [SerializeField]
        protected TMP_InputField chatInput = null;

        [SerializeField]
        protected TextMeshProUGUI chatDisplay = null;

        [SerializeField]
        protected Image sceneImage = null;

        [SerializeField]
        protected TextMeshProUGUI sceneNameText = null;

        [SerializeField]
        protected TextMeshProUGUI sceneDescriptionText = null;

        [SerializeField]
        protected Image characterImage = null;

        [SerializeField]
        protected TextMeshProUGUI characterNameText = null;

        [SerializeField]
        protected TextMeshProUGUI characterDescriptionText = null;

        [SerializeField]
        protected HighlightButton leaveButton = null;

        [SerializeField]
        protected HighlightButton cancelGameButton = null;

        [SerializeField]
        protected HighlightButton readyButton = null;

        [SerializeField]
        protected TextMeshProUGUI readyButtonText = null;

        [SerializeField]
        protected HighlightButton startGameButton = null;

        [SerializeField]
        protected TextMeshProUGUI startGameButtonText = null;

        private string lobbyGameChatText = string.Empty;
        private int maxLobbyChatTextSize = 64000;
        private string originalCharacterNameText = string.Empty;
        private string originalCharacterDescriptionText = string.Empty;

        private UnitProfile unitProfile = null;

        /// <summary>
        /// accountId, ClientPlayerLobbyGameConnectionButton
        /// </summary>
        private Dictionary<int, ClientPlayerLobbyGameConnectionButton> playerButtons = new Dictionary<int, ClientPlayerLobbyGameConnectionButton>();

        // game manager references
        protected UIManager uIManager = null;
        protected ObjectPooler objectPooler = null;
        protected SystemDataFactory systemDataFactory = null;
        protected NetworkManagerServer networkManagerServer = null;
        protected NetworkManagerClient networkManagerClient = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            chatDisplay.text = string.Empty;
            originalCharacterNameText = characterNameText.text;
            originalCharacterDescriptionText = characterDescriptionText.text;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            objectPooler = systemGameManager.ObjectPooler;
            systemDataFactory = systemGameManager.SystemDataFactory;
            networkManagerServer = systemGameManager.NetworkManagerServer;
            networkManagerClient = systemGameManager.NetworkManagerClient;
        }

        public void ChooseLobbyGameCharacter() {
            uIManager.newGameWindow.OpenWindow();
        }

        public void StartGame() {
            if (networkManagerClient.LobbyGame.inProgress == true) {
                // we are already in a game, so just join it
                networkManagerClient.RequestJoinLobbyGameInProgress(networkManagerClient.LobbyGame.gameId);
                return;
            }
            networkManagerClient.RequestStartLobbyGame(networkManagerClient.LobbyGame.gameId);
        }


        public void CancelLobbyGame() {
            networkManagerClient.CancelLobbyGame(networkManagerClient.LobbyGame.gameId);
        }

        public void ToggleReady() {
            networkManagerClient.ToggleLobbyGameReadyStatus(networkManagerClient.LobbyGame.gameId);
        }

        public void Leave() {
            networkManagerClient.LeaveLobbyGame(networkManagerClient.LobbyGame.gameId);
            uIManager.clientLobbyGameWindow.CloseWindow();
        }

        public void UpdateUIELements() {
            serverStatusText.text = $"Logged In As: {networkManagerClient.Username}";
            SceneNode sceneNode = systemDataFactory.GetResource<SceneNode>(networkManagerClient.LobbyGame.sceneResourceName);
            if (sceneNode == null) {
                Debug.LogWarning($"Could not find scene {networkManagerClient.LobbyGame.sceneResourceName}");
                return;
            }
            if (sceneNode.LoadingScreenImage != null) {
                sceneImage.sprite = sceneNode.LoadingScreenImage;
                sceneImage.color = Color.white;
            } else {
                sceneImage.sprite = null;
                sceneImage.color = Color.black;
            }
            sceneNameText.text = sceneNode.DisplayName;
            sceneDescriptionText.text = sceneNode.Description;

            characterImage.sprite = null;
            characterImage.color = Color.black;
            characterNameText.text = originalCharacterNameText;
            characterDescriptionText.text = originalCharacterDescriptionText;
        }

        public void HandleJoinLobbyGame(int gameId, int accountId, string userName) {
            if (gameId != networkManagerClient.LobbyGame.gameId) {
                return;
            }
            AddPlayerToList(accountId, userName, string.Empty);
        }

        public void HandleLeaveLobbyGame(int accountId, int gameId) {
            if (gameId != networkManagerClient.LobbyGame.gameId) {
                return;
            }
            RemovePlayerFromList(accountId);
        }


        public void HandleLobbyLogout(int accountId) {
            RemovePlayerFromList(accountId);
        }

        public void HandleCancelLobbyGame(int gameId) {
            if (gameId != networkManagerClient.LobbyGame.gameId) {
                return;
            }
            Close();
        }

        public void SendChatMessage() {
            networkManagerClient.SendLobbyGameChatMessage(chatInput.text, networkManagerClient.LobbyGame.gameId);
            chatInput.text = string.Empty;
        }

        public void HandleSendLobbyGameChatMessage(string messageText, int gameId) {
            if (gameId != networkManagerClient.LobbyGame.gameId) {
                // this message is meant for a different lobby game and we will ignore it
                return;
            }
            lobbyGameChatText += messageText;
            lobbyGameChatText = NetworkManagerServer.ShortenStringOnNewline(lobbyGameChatText, maxLobbyChatTextSize);
            chatDisplay.text = lobbyGameChatText;
        }

        public void RequestLobbyGamePlayerList() {
            //Debug.Log($"ClientLobbyPanelController.RequestLobbyPlayerList()");

            networkManagerClient.RequestLobbyPlayerList();
        }

        public void PopulatePlayerList(Dictionary<int, LobbyGamePlayerInfo> userNames) {
            //Debug.Log($"ClientLobbyGamePanel.PopulatePlayerList({userNames.Count})");

            foreach (KeyValuePair<int, LobbyGamePlayerInfo> loggedInAccount in userNames) {
                AddPlayerToList(loggedInAccount.Key, loggedInAccount.Value.userName, loggedInAccount.Value.unitProfileName);
                if (loggedInAccount.Value.ready == true) {
                    HandleSetLobbyGameReadyStatus(networkManagerClient.LobbyGame.gameId, loggedInAccount.Key, true);
                }
            }
        }

        public void AddPlayerToList(int accountId, string userName, string unitProfileName) {
            //Debug.Log($"ClientLobbyGamePanel.AddPlayerToList({accountId}, {userName}, {unitProfileName})");

            GameObject go = objectPooler.GetPooledObject(playerConnectionTemplate, playerConnectionContainer);
            ClientPlayerLobbyGameConnectionButton clientPlayerLobbyGameConnectionButton = go.GetComponent<ClientPlayerLobbyGameConnectionButton>();
            clientPlayerLobbyGameConnectionButton.Configure(systemGameManager);
            if (networkManagerClient.LobbyGame.leaderAccountId == accountId) {
                clientPlayerLobbyGameConnectionButton.SetClientId(accountId, $"{userName} (leader)", unitProfileName);
            } else {
                clientPlayerLobbyGameConnectionButton.SetClientId(accountId, userName, unitProfileName);
            }
            //uINavigationControllers[1].AddActiveButton(clientPlayerLobbyGameConnectionButton.joinbu);
            playerButtons.Add(accountId, clientPlayerLobbyGameConnectionButton);

            if (accountId == networkManagerClient.AccountId) {
                HandleChooseLobbyGameCharacter(networkManagerClient.LobbyGame.gameId, accountId, unitProfileName);
            }
        }

        public void RemovePlayerFromList(int accountId) {
            //Debug.Log($"ClientLobbyGamePanel.RemovePlayerFromList({accountId})");

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
            foreach (ClientPlayerLobbyGameConnectionButton clientPlayerLobbyGameConnectionButton in playerButtons.Values) {
                if (clientPlayerLobbyGameConnectionButton.gameObject != null) {
                    clientPlayerLobbyGameConnectionButton.gameObject.transform.SetParent(null);
                    objectPooler.ReturnObjectToPool(clientPlayerLobbyGameConnectionButton.gameObject);
                }
            }
            playerButtons.Clear();
            //uINavigationControllers[1].ClearActiveButtons();
        }

        public void HandleChooseLobbyGameCharacter(int gameId, int accountId, string unitProfileName) {
            //Debug.Log($"ClientLobbyGamePanel.HandleChooseLobbyGameCharacter({gameId}, {accountId}, {unitProfileName})");

            if (accountId == networkManagerClient.AccountId) {
                unitProfile = systemDataFactory.GetResource<UnitProfile>(unitProfileName);
                if (unitProfile != null) {
                    characterImage.sprite = unitProfile.Icon;
                    characterImage.color = Color.white;
                    characterNameText.text = unitProfile.DisplayName;
                    characterDescriptionText.text = unitProfile.Description;
                    readyButton.Button.interactable = true;
                } else {
                    characterImage.sprite = null;
                    characterImage.color = Color.black;
                }
            }
            playerButtons[accountId].SetUnitProfileName(unitProfileName);
        }

        public void HandleSetLobbyGameReadyStatus(int gameId, int accountId, bool ready) {
            //Debug.Log($"ClientLobbyGamePanel.HandleSetLobbyGameReadyStatus({gameId}, {clientId}, {ready})");

            if (networkManagerClient.LobbyGame.gameId != gameId) {
                return;
            }

            if (accountId == networkManagerClient.AccountId) {
                if (ready) {
                    readyButtonText.text = "Not Ready";
                    readyButton.Button.interactable = true;
                    if (networkManagerClient.LobbyGame.inProgress == true && networkManagerClient.LobbyGame.allowLateJoin == true) {
                        startGameButton.Button.interactable = true;
                    }
                } else {
                    readyButtonText.text = "Ready";
                }
            }
            
            playerButtons[accountId].SetReadyStatus(ready);

            if (networkManagerClient.AccountId == networkManagerClient.LobbyGame.leaderAccountId) {
                // check if the start button can be made interactable
                if (AllPlayersReady()) {
                    startGameButton.Button.interactable = true;
                } else {
                    startGameButton.Button.interactable = false;

                }
            }
        }

        private bool AllPlayersReady() {
            foreach (LobbyGamePlayerInfo lobbyGamePlayerInfo in networkManagerClient.LobbyGame.PlayerList.Values) {
                if (lobbyGamePlayerInfo.ready == false) {
                    return false;
                }
            }
            return true;
        }

        public void UpdateNavigationButtons() {

            startGameButtonText.text = "Start Game";
            readyButtonText.text = "Ready";

            // hide the cancel game button for anyone other than the leader
            if (networkManagerClient.AccountId == networkManagerClient.LobbyGame.leaderAccountId) {
                leaveButton.Button.interactable = false;
                cancelGameButton.gameObject.SetActive(true);
                startGameButton.gameObject.SetActive(true);
                startGameButton.Button.interactable = false;
            } else {
                leaveButton.Button.interactable = true;
                cancelGameButton.gameObject.SetActive(false);
                if (networkManagerClient.LobbyGame.inProgress == true && networkManagerClient.LobbyGame.allowLateJoin == true) {
                    startGameButton.gameObject.SetActive(true);
                    startGameButtonText.text = "Join Game";
                    startGameButton.Button.interactable = false;
                } else {
                    startGameButton.gameObject.SetActive(false);
                }
            }
            readyButton.Button.interactable = false;

            uINavigationControllers[0].UpdateNavigationList();
        }


        public override void ProcessOpenWindowNotification() {
            base.ProcessOpenWindowNotification();
            UpdateUIELements();
            UpdateNavigationButtons();
            PopulatePlayerList(networkManagerClient.LobbyGame.PlayerList);
            networkManagerClient.OnSendLobbyGameChatMessage += HandleSendLobbyGameChatMessage;
            networkManagerClient.OnJoinLobbyGame += HandleJoinLobbyGame;
            networkManagerClient.OnLeaveLobbyGame += HandleLeaveLobbyGame;
            networkManagerClient.OnCancelLobbyGame += HandleCancelLobbyGame;
            networkManagerClient.OnChooseLobbyGameCharacter += HandleChooseLobbyGameCharacter;
            networkManagerClient.OnSetLobbyGameReadyStatus += HandleSetLobbyGameReadyStatus;
        }

        public override void ReceiveClosedWindowNotification() {
            base.ReceiveClosedWindowNotification();
            networkManagerClient.OnSendLobbyGameChatMessage -= HandleSendLobbyGameChatMessage;
            networkManagerClient.OnJoinLobbyGame -= HandleJoinLobbyGame;
            networkManagerClient.OnLeaveLobbyGame -= HandleLeaveLobbyGame;
            networkManagerClient.OnCancelLobbyGame -= HandleCancelLobbyGame;
            networkManagerClient.OnChooseLobbyGameCharacter -= HandleChooseLobbyGameCharacter;
            networkManagerClient.OnSetLobbyGameReadyStatus -= HandleSetLobbyGameReadyStatus;

            ClearPlayerList();
        }
    }
}