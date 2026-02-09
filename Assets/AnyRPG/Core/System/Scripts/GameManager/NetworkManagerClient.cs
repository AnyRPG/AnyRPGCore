using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    public class NetworkManagerClient : ConfiguredMonoBehaviour {

        public event Action<string> OnClientVersionFailure = delegate { };
        public event Action<LobbyGame> OnCreateLobbyGame = delegate { };
        public event Action<int> OnCancelLobbyGame = delegate { };
        public event Action<int, int, string> OnJoinLobbyGame = delegate { };
        public event Action<int, int> OnLeaveLobbyGame = delegate { };
        public event Action<string> OnSendLobbyChatMessage = delegate { };
        public event Action<string, int> OnSendLobbyGameChatMessage = delegate { };
        public event Action<string, int> OnSendSceneChatMessage = delegate { };
        public event Action<int, string> OnLobbyLogin = delegate { };
        public event Action<int> OnLobbyLogout = delegate { };
        public event Action<List<LobbyGame>> OnSetLobbyGameList = delegate { };
        public event Action<Dictionary<int, string>> OnSetLobbyPlayerList = delegate { };
        public event Action<int, int, string> OnChooseLobbyGameCharacter = delegate { };
        public event Action<int, int, bool> OnSetLobbyGameReadyStatus = delegate { };
        public event Action<int> OnStartLobbyGame = delegate { };
        public event Action OnClientConnectionStopped = delegate { };
        public event Action OnClientConnectionStarted = delegate { };

        [SerializeField]
        private NetworkController networkController = null;

        private string username = string.Empty;
        private string password = string.Empty;

        private bool isLoggingInOrOut = false;

        //private INetworkConnector networkConnector = null;
        private NetworkServerMode clientMode = NetworkServerMode.Lobby;
        private int accountId;
        private LobbyGame lobbyGame;


        private Dictionary<int, LoggedInAccount> lobbyGamePlayerList = new Dictionary<int, LoggedInAccount>();
        
        private Dictionary<int, LobbyGame> lobbyGames = new Dictionary<int, LobbyGame>();
        private Dictionary<int, string> lobbyPlayers = new Dictionary<int, string>();

        // game manager references
        private PlayerManager playerManager = null;
        private PlayerManagerServer playerManagerServer = null;
        private CharacterManager characterManager = null;
        private UIManager uIManager = null;
        private LevelManager levelManager = null;
        private MessageLogClient messageLogClient = null;
        private MessageFeedManager messageFeedManager = null;
        private SystemItemManager systemItemManager = null;
        private LootManager lootManager = null;
        private CraftingManager craftingManager = null;
        private TimeOfDayManagerServer timeOfDayManagerServer = null;
        private WeatherManagerClient weatherManagerClient = null;
        private MapManager mapManager = null;
        private CharacterGroupServiceClient characterGroupServiceClient = null;
        private LoadGameManager loadGameManager = null;
        private TradeServiceClient tradeServiceClient = null;
        private MailboxManagerClient mailboxManagerClient = null;
        private AuctionManagerClient auctionManagerClient = null;
        private GuildServiceClient guildServiceClient = null;
        private FriendServiceClient friendServiceClient = null;

        public string Username { get => username; }
        public string Password { get => password; }
        public NetworkServerMode ClientMode { get => clientMode; }
        public Dictionary<int, LoggedInAccount> LobbyGamePlayerList { get => lobbyGamePlayerList; }
        public LobbyGame LobbyGame { get => lobbyGame; }
        public int AccountId { get => accountId; }
        public NetworkController NetworkController { get => networkController; set => networkController = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            characterManager = systemGameManager.CharacterManager;
            levelManager = systemGameManager.LevelManager;
            uIManager = systemGameManager.UIManager;
            messageLogClient = systemGameManager.MessageLogClient;
            messageFeedManager = uIManager.MessageFeedManager;
            systemItemManager = systemGameManager.SystemItemManager;
            lootManager = systemGameManager.LootManager;
            craftingManager = systemGameManager.CraftingManager;
            playerManagerServer = systemGameManager.PlayerManagerServer;
            timeOfDayManagerServer = systemGameManager.TimeOfDayManagerServer;
            weatherManagerClient = systemGameManager.WeatherManagerClient;
            mapManager = uIManager.MapManager;
            characterGroupServiceClient = systemGameManager.CharacterGroupServiceClient;
            loadGameManager = systemGameManager.LoadGameManager;
            tradeServiceClient = systemGameManager.TradeServiceClient;
            mailboxManagerClient = systemGameManager.MailboxManagerClient;
            auctionManagerClient = systemGameManager.AuctionManagerClient;
            guildServiceClient = systemGameManager.GuildServiceClient;
            friendServiceClient = systemGameManager.FriendServiceClient;
        }

        public bool Login(string username, string password, string server) {
            //Debug.Log($"NetworkManagerClient.Login({username}, {password})");
            
            isLoggingInOrOut = true;

            this.username = username;
            this.password = password;
            return networkController.Login(username, password, server);
        }

        public void RequestLogout() {
            //Debug.Log("NetworkManagerClient.RequestLogout()");

            isLoggingInOrOut = true;
            networkController.RequestLogout();
        }

        public void RequestDisconnect() {
            isLoggingInOrOut = true;
            networkController.Disconnect();
        }

        public void RequestReturnFromCutscene() {
            //Debug.Log($"NetworkManagerClient.ReturnFromCutscene()");

            networkController.RequestReturnFromCutscene();
        }

        public void RequestSpawnPlayerUnit(string sceneName) {
            //Debug.Log($"NetworkManagerClient.RequestSpawnPlayerUnit({sceneName})");

            networkController.RequestSpawnPlayerUnit(sceneName);
        }

        public void RequestRespawnPlayerUnit() {
            networkController.RequestRespawnPlayerUnit();
        }

        public void RequestRevivePlayerUnit() {
            networkController.RequestRevivePlayerUnit();
        }

        public GameObject RequestSpawnModelPrefab(GameObject prefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            //Debug.Log($"NetworkManagerClient.RequestSpawnModelPrefab({prefab.name}, {parentTransform.gameObject.name}, {position}, {forward})");

            return networkController.RequestSpawnModelPrefab(prefab, parentTransform, position, forward);
        }

        public void SendLobbyChatMessage(string messageText) {
            networkController.SendLobbyChatMessage(messageText);
        }

        public void SendLobbyGameChatMessage(string messageText, int gameId) {
            networkController.SendLobbyGameChatMessage(messageText, gameId);
        }

        public void SendSceneChatMessage(string chatMessage) {
            networkController.SendSceneChatMessage(chatMessage);
        }

        public void RequestLobbyPlayerList() {
            networkController.RequestLobbyPlayerList();
        }

        public void ToggleLobbyGameReadyStatus(int gameId) {
            networkController.ToggleLobbyGameReadyStatus(gameId);
        }

        public bool CanSpawnPlayerOverNetwork() {
            return networkController.CanSpawnCharacterOverNetwork();
        }

        public void ProcessStopNetworkUnitClient(UnitController unitController) {
            //if (playerManager.UnitController == unitController) {
                //playerManager.ProcessStopClient();
            //} else {
                characterManager.ProcessStopNetworkUnit(unitController);
            //}
            /*
            if (unitController.UnitControllerMode == UnitControllerMode.Player) {
                playerManagerServer.StopMonitoringPlayerUnit(unitController);
            }
            */
        }

        public void ProcessStopConnection() {
            //Debug.Log($"NetworkManagerClient.ProcessStopConnection()");

            if (systemGameManager.DisconnectingNetworkForShutdown == true) {
                systemGameManager.ExitGame();
                return;
            }

            systemGameManager.SetGameMode(GameMode.Local);
            OnClientConnectionStopped();
            if (levelManager.GetActiveSceneNode() != systemConfigurationManager.MainMenuSceneNode) {
                if (isLoggingInOrOut == false) {
                    uIManager.AddPopupWindowToQueue(uIManager.disconnectedWindow);
                }
                isLoggingInOrOut = false;
                levelManager.LoadMainMenu(false);
                return;
            }

            // don't open disconnected window if this was an expected logout;
            if (isLoggingInOrOut == true) {
                isLoggingInOrOut = false;
                return;
            }
            
            // main menu, close main menu windows and open the disconnected window
            uIManager.newGameWindow.CloseWindow();
            uIManager.loadGameWindow.CloseWindow();
            uIManager.clientLobbyWindow.CloseWindow();
            uIManager.clientLobbyGameWindow.CloseWindow();
            uIManager.createLobbyGameWindow.CloseWindow();
            uIManager.disconnectedWindow.OpenWindow();
        }

        public void ProcessClientVersionFailure(string requiredClientVersion) {
            //Debug.Log($"NetworkManagerClient.ProcessClientVersionFailure()");

            uIManager.loginInProgressWindow.CloseWindow();
            uIManager.wrongClientVersionWindow.OpenWindow();
            OnClientVersionFailure(requiredClientVersion);
        }

        public void ProcessAuthenticationFailure() {
            //Debug.Log($"NetworkManagerClient.ProcessAuthenticationFailure()");

            uIManager.loginInProgressWindow.CloseWindow();
            uIManager.loginFailedWindow.OpenWindow();
        }

        public void ProcessLoginSuccess(int accountId, NetworkServerMode clientMode) {
            //Debug.Log($"NetworkManagerClient.ProcessLoginSuccess({accountId}, {clientMode})");

            // not doing this here because the connector has not spawned yet.
            //uIManager.ProcessLoginSuccess();
            this.clientMode = clientMode;
            this.accountId = accountId;
            isLoggingInOrOut = false;
        }

        public void RequestCreatePlayerCharacter(CharacterSaveData saveData) {
            //Debug.Log($"NetworkManagerClient.CreatePlayerCharacterClient(AnyRPGSaveData)");

            networkController.RequestCreatePlayerCharacter(saveData);
        }

        public void RequestLobbyGameList() {
            //Debug.Log($"NetworkManagerClient.RequestLobbyGameList()");

            networkController.RequestLobbyGameList();
        }

        public void LoadCharacterList() {
            //Debug.Log($"NetworkManagerClient.LoadCharacterList()");

            networkController.LoadCharacterList();
        }

        public void DeletePlayerCharacter(int playerCharacterId) {
            //Debug.Log($"NetworkManagerClient.DeletePlayerCharacter({playerCharacterId})");

            networkController.DeletePlayerCharacter(playerCharacterId);
        }

        public void RequestCreateLobbyGame(string sceneResourceName, bool allowLateJoin) {
            networkController.RequestCreateLobbyGame(sceneResourceName, allowLateJoin);
        }

        public void AdvertiseCreateLobbyGame(LobbyGame lobbyGame) {
            //Debug.Log($"NetworkManagerClient.AdvertiseCreateLobbyGame({lobbyGame.leaderAccountId}) accountId: {accountId}");

            lobbyGames.Add(lobbyGame.gameId, lobbyGame);
            if (lobbyGame.leaderAccountId == accountId) {
                this.lobbyGame = lobbyGame;
                uIManager.clientLobbyGameWindow.OpenWindow();
            }
            OnCreateLobbyGame(lobbyGame);
        }

        public void SetLobbyGame(LobbyGame lobbyGame) {
            //Debug.Log($"NetworkManagerClient.SetLobbyGame({lobbyGame.gameId}) accountId: {accountId}");

            this.lobbyGame = lobbyGame;
        }

        public void CancelLobbyGame(int gameId) {
            networkController.CancelLobbyGame(gameId);
        }

        public void AdvertiseCancelLobbyGame(int gameId) {
            OnCancelLobbyGame(gameId);
        }

        public void JoinLobbyGame(int gameId) {
            networkController.JoinLobbyGame(gameId);
        }

        public void LeaveLobbyGame(int gameId) {
            networkController.LeaveLobbyGame(gameId);
        }

        public void AdvertiseAccountJoinLobbyGame(int gameId, int accountId, string userName) {
            //Debug.Log($"NetworkManagerClient.AdvertiseAccountJoinLobbyGame({gameId}, {accountId}, {userName})");

            lobbyGames[gameId].AddPlayer(accountId, userName);
            OnJoinLobbyGame(gameId, accountId, userName);
            if (accountId == this.accountId) {
                // this client just joined a game
                lobbyGame = lobbyGames[gameId];
                uIManager.clientLobbyGameWindow.OpenWindow();
            }
        }

        public void AdvertiseAccountLeaveLobbyGame(int gameId, int accountId) {
            lobbyGames[gameId].RemovePlayer(accountId);
            OnLeaveLobbyGame(gameId, accountId);
        }

        public void AdvertiseSendLobbyChatMessage(string messageText) {
            OnSendLobbyChatMessage(messageText);
        }

        public void AdvertiseSendLobbyGameChatMessage(string messageText, int gameId) {
            OnSendLobbyGameChatMessage(messageText, gameId);
        }

        public void AdvertiseSendSceneChatMessage(string messageText, int accountId) {
            OnSendSceneChatMessage(messageText, accountId);
            messageLogClient.WriteGeneralMessage(messageText);
        }

        public void AdvertiseLobbyLogin(int accountId, string userName) {
            OnLobbyLogin(accountId, userName);
        }

        public void AdvertiseLobbyLogout(int accountId) {
            OnLobbyLogout(accountId);
        }

        public void SetLobbyGameList(List<LobbyGame> lobbyGames) {
            //Debug.Log($"NetworkManagerClient.SetLobbyGameList({lobbyGames.Count})");

            this.lobbyGames.Clear();
            foreach (LobbyGame lobbyGame in lobbyGames) {
                this.lobbyGames.Add(lobbyGame.gameId, lobbyGame);
            }
            OnSetLobbyGameList(this.lobbyGames.Values.ToList<LobbyGame>());
        }

        public void SetLobbyPlayerList(Dictionary<int, string> lobbyPlayers) {
            this.lobbyPlayers.Clear();
            foreach (int loggedInClientId in lobbyPlayers.Keys) {
                this.lobbyPlayers.Add(loggedInClientId, lobbyPlayers[loggedInClientId]);
            }
            OnSetLobbyPlayerList(lobbyPlayers);
        }

        public void ChooseLobbyGameCharacter(string unitProfileName, string appearanceString, List<SwappableMeshSaveData> swappableMeshSaveData) {
            //Debug.Log($"NetworkManagerClient.ChooseLobbyGameCharacter({unitProfileName})");

            networkController.ChooseLobbyGameCharacter(unitProfileName, lobbyGame.gameId, appearanceString, swappableMeshSaveData);
        }

        public void AdvertiseChooseLobbyGameCharacter(int gameId, int accountId, string unitProfileName) {
            //Debug.Log($"NetworkManagerClient.AdvertiseChooseLobbyGameCharacter({gameId}, {accountId}, {unitProfileName})");

            if (lobbyGames.ContainsKey(gameId) == false) {
                Debug.LogWarning($"NetworkManagerClient.AdvertiseChooseLobbyGameCharacter: gameId {gameId} does not exist");
                return;
            }
            lobbyGames[gameId].PlayerList[accountId].unitProfileName = unitProfileName;
            
            OnChooseLobbyGameCharacter(gameId, accountId, unitProfileName);

            if (lobbyGame != null && gameId == lobbyGame.gameId && accountId == this.accountId) {
                // the character was chosen for this client so close the new game window
                uIManager.newGameWindow.CloseWindow();
            }
        }

        public void RequestStartLobbyGame(int gameId) {
            networkController.RequestStartLobbyGame(gameId);
        }

        public void RequestJoinLobbyGameInProgress(int gameId) {
            networkController.RequestJoinLobbyGameInProgress(gameId);
        }

        public void AdvertiseJoinLobbyGameInProgress(int gameId) {
            //Debug.Log($"NetworkManagerClient.AdvertiseJoinLobbyGameInProgress({gameId})");

            if (lobbyGames.ContainsKey(gameId) == false) {
                // lobby game does not exist
                return;
            }

            LaunchLobbyGame(gameId);
        }

        public void AdvertiseJoinMMOGameInProgress() {
            //Debug.Log($"NetworkManagerClient.AdvertiseJoinMMOGameInProgress()");

            uIManager.loadGameWindow.CloseWindow();
            LaunchNetworkGame();
        }


        public void AdvertiseStartLobbyGame(int gameId) {
            if (lobbyGames.ContainsKey(gameId) == false) {
                // lobby game does not exist
                return;
            }
            lobbyGames[gameId].inProgress = true;
            OnStartLobbyGame(gameId);

            LaunchLobbyGame(gameId);
        }

        public void LaunchNetworkGame() {
            //Debug.Log($"NetworkManagerClient.LaunchNetworkGame()");

            //systemItemManager.ClearInstantiatedItems();
            playerManager.SpawnPlayerConnection();
            levelManager.ProcessBeforeLevelUnload();
        }

        public void LaunchLobbyGame(int gameId) {
            //Debug.Log($"NetworkManagerClient.LaunchLobbyGame({gameId})");

            if (lobbyGame == null || lobbyGame.gameId != gameId) {
                // have not joined lobby game, or joined different lobby game
                return;
            }

            // this is our lobby game
            uIManager.clientLobbyGameWindow.CloseWindow();
            LaunchNetworkGame();
        }

        public void AdvertiseSetLobbyGameReadyStatus(int gameId, int accountId, bool ready) {
            //Debug.Log($"NetworkManagerClient.AdvertiseSetLobbyGameReadyStatus({gameId}, {accountId}, {ready})");

            if (lobbyGames.ContainsKey(gameId) == false || lobbyGames[gameId].PlayerList.ContainsKey(accountId) == false) {
                // game does not exist or player is not in game
                return;
            }
            lobbyGames[gameId].PlayerList[accountId].ready = ready;
            OnSetLobbyGameReadyStatus(gameId, accountId, ready);
        }

        public void AdvertiseUnloadSceneClient() {
            //Debug.Log($"NetworkManagerClient.AdvertiseLoadSceneClient({sceneName})");

            levelManager.ProcessBeforeLevelUnload();
        }

        public void InteractWithOption(UnitController sourceUnitController, Interactable targetInteractable, int componentIndex, int choiceIndex) {
            //Debug.Log($"NetworkManagerClient.InteractWithOption({targetInteractable.gameObject.name}, {componentIndex}, {choiceIndex})");

            networkController.InteractWithOption(sourceUnitController, targetInteractable, componentIndex, choiceIndex);
        }

        /*
        public void AdvertiseAddSpawnRequest(SpawnPlayerRequest loadSceneRequest) {
            levelManager.AddSpawnRequest(accountId, loadSceneRequest);
        }
        */

        public void HandleSceneLoadStart(string sceneName) {
            levelManager.NotifyOnBeginLoadingLevel(sceneName);
        }

        public void HandleSceneLoadPercentageChange(float percent) {
            levelManager.SetLoadingProgress(percent);
        }

        public void RequestSetPlayerCharacterClass(Interactable interactable, int componentIndex) {
            networkController.RequestSetPlayerCharacterClass(interactable, componentIndex);
        }

        public void SetPlayerCharacterSpecialization(Interactable interactable, int componentIndex) {
            networkController.SetPlayerCharacterSpecialization(interactable, componentIndex);
        }

        public void RequestSetPlayerFaction(Interactable interactable, int componentIndex) {
            networkController.RequestSetPlayerFaction(interactable, componentIndex);
        }

        public void RequestCreateGuild(Interactable interactable, int componentIndex, string guildName) {
            networkController.RequestCreateGuild(interactable, componentIndex, guildName);
        }

        public void CheckGuildName(Interactable interactable, int componentIndex, string guildName) {
            networkController.CheckGuildName(interactable, componentIndex, guildName);
        }

        public void RequestLearnSkill(Interactable interactable, int componentIndex, int skillId) {
            networkController.RequestLearnSkill(interactable, componentIndex, skillId);
        }

        public void RequestAcceptQuest(Interactable interactable, int componentIndex, Quest quest) {
            networkController.RequestAcceptQuest(interactable, componentIndex, quest);
        }

        public void RequestCompleteQuest(Interactable interactable, int componentIndex, Quest quest, QuestRewardChoices questRewardChoices) {
            networkController.RequestCompleteQuest(interactable, componentIndex, quest, questRewardChoices);
        }

        public void AdvertiseMessageFeedMessage(string message) {
            messageFeedManager.WriteMessage(message);
        }

        public void AdvertiseSystemMessage(string message) {
            messageLogClient.WriteSystemMessage(message);
        }

        public void SellItemToVendor(Interactable interactable, int componentIndex, long itemInstanceId) {
            networkController.SellVendorItem(interactable, componentIndex, itemInstanceId);
        }

        public void RequestSpawnUnit(Interactable interactable, int componentIndex, int unitLevel, int extraLevels, bool useDynamicLevel, string unitProfileName, string unitToughnessName) {
            //Debug.Log($"NetworkManagerClient.RequestSpawnUnit({unitLevel}, {extraLevels}, {useDynamicLevel}, {unitProfileName}, {unitToughnessName})");

            networkController.RequestSpawnUnit(interactable, componentIndex, unitLevel, extraLevels, useDynamicLevel, unitProfileName, unitToughnessName);
        }


        public void AdvertiseAddToBuyBackCollection(UnitController sourceUnitController, Interactable interactable, int componentIndex, long instantiatedItemId) {
            if (systemItemManager.InstantiatedItems.ContainsKey(instantiatedItemId) == false) {
                return;
            }
            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(sourceUnitController);
            if (currentInteractables[componentIndex] is VendorComponent) {
                (currentInteractables[componentIndex] as VendorComponent).AddToBuyBackCollection(sourceUnitController, componentIndex, systemItemManager.InstantiatedItems[instantiatedItemId]);
            }

        }

        public void BuyItemFromVendor(Interactable interactable, int componentIndex, int collectionIndex, int itemIndex, string resourceName) {
            networkController.BuyItemFromVendor(interactable, componentIndex, collectionIndex, itemIndex, resourceName);
        }

        public void TakeAllLoot() {
            networkController.TakeAllLoot();
        }

        public void AddDroppedLoot(int lootDropId, long itemInstanceId) {
            //Debug.Log($"NetworkManagerClient.AddDroppedLoot({lootDropId}, {itemId})");

            lootManager.AddNetworkLootDrop(lootDropId, itemInstanceId);
        }

        public void AddAvailableDroppedLoot(List<int> lootDropIds) {
            //Debug.Log($"NetworkManagerClient.AddAvailableDroppedLoot(count: {lootDropIds.Count})");

            lootManager.AddAvailableLoot(accountId, lootDropIds);
        }

        public void AdvertiseTakeLoot(int lootDropId) {
            //Debug.Log($"NetworkManagerClient.AdvertiseTakeLoot({lootDropId})");

            lootManager.TakeLoot(accountId, lootDropId);
        }

        public void RequestTakeLoot(int lootDropId) {
            //Debug.Log($"NetworkManagerClient.RequestTakeLoot({lootDropId})");

            networkController.RequestTakeLoot(lootDropId);
        }

        public void RequestBeginCrafting(Recipe recipe, int craftAmount) {
            //Debug.Log($"NetworkManagerClient.RequestBeginCrafting({recipe.DisplayName}, {craftAmount})");

            networkController.RequestBeginCrafting(recipe, craftAmount);
        }

        public void RequestCancelCrafting() {
            networkController.RequestCancelCrafting();
        }

        public void RequestUpdatePlayerAppearance(Interactable interactable, int componentIndex, string unitProfileName, string appearanceString, List<SwappableMeshSaveData> swappableMeshSaveData) {
            networkController.RequestUpdatePlayerAppearance(interactable, componentIndex, unitProfileName, appearanceString, swappableMeshSaveData);
        }

        public void RequestChangePlayerName(Interactable interactable, int componentIndex, string newName) {
            networkController.RequestChangePlayerName(interactable, componentIndex, newName);
        }

        public void RequestSpawnPet(UnitProfile unitProfile) {
            networkController.RequestSpawnPet(unitProfile);
        }

        public void RequestDespawnPet(UnitProfile unitProfile) {
            networkController.RequestDespawnPet(unitProfile);
        }

        public void AdvertiseSpawnPlayerRequest(SpawnPlayerRequest spawnPlayerRequest) {
            //Debug.Log($"NetworkManagerClient.AdvertiseSpawnPlayerRequest()");

            playerManagerServer.AddSpawnRequest(accountId, spawnPlayerRequest);
        }

        public void SetStartTime(DateTime startTime) {
            timeOfDayManagerServer.SetStartTime(startTime);
        }

        public void ProcessStartClientConnector() {
            OnClientConnectionStarted();
            uIManager.ProcessLoginSuccess();
        }

        public void AdvertiseChooseWeather(WeatherProfile weatherProfile) {
            weatherManagerClient.ChooseWeather(weatherProfile);
        }

        public void AdvertiseEndWeather(WeatherProfile profile, bool immediate) {
            weatherManagerClient.EndWeather(profile, immediate);
        }

        public void AdvertiseStartWeather() {
            weatherManagerClient.StartWeather();
        }

        public void RequestSceneWeather() {
            networkController.RequestSceneWeather();
        }

        public void RequestDespawnPlayer() {
            //Debug.Log($"NetworkManagerClient.RequestDespawnPlayer()");

            networkController.RequestDespawnPlayerUnit();
        }

        public void AdvertiseLoadCutscene(Cutscene cutscene) {
            levelManager.LoadCutSceneWithDelay(cutscene);

        }

        public void RequestTurnInDialog(Interactable interactable, int componentIndex, Dialog dialog) {
            networkController.RequestTurnInDialog(interactable, componentIndex, dialog);
        }

        public void RequestTurnInQuestDialog(Dialog dialog) {
            networkController.RequestTurnInQuestDialog(dialog);
        }

        public void AdvertiseSceneObjectLoadComplete() {
            mapManager.ProcessLevelLoad();
        }

        public void RequestLoadPlayerCharacter(int playerCharacterId) {
            networkController.RequestLoadPlayerCharacter(playerCharacterId);
        }

        public void AcceptCharacterGroupInvite(int inviteGroupId) {
            networkController.AcceptCharacterGroupInvite(inviteGroupId);
        }

        public void DeclineCharacterGroupInvite() {
            networkController.DeclineCharacterGroupInvite();
        }

        public void ProcessAddCharacterToGroup(int characterGroupId, CharacterGroupMemberNetworkData characterGroupMemberNetworkData) {
            //Debug.Log($"NetworkManagerClient.ProcessCharacterJoinGroup({playerCharacterId}, {characterGroup.characterGroupId})");

            characterGroupServiceClient.ProcessJoinGroup(characterGroupId, characterGroupMemberNetworkData);
        }

        public void ProcessCharacterJoinGuild(int guildId, GuildMemberNetworkData guildMemberNetworkData) {
            //Debug.Log($"NetworkManagerClient.ProcessCharacterJoinGuild(guildId: {guildId}, {characterSummaryNetworkData.CharacterName})");

            guildServiceClient.ProcessJoinGuild(guildId, guildMemberNetworkData);
        }

        public void ProcessLoadCharacterGroup(CharacterGroupNetworkData characterGroupNetworkData) {
            //Debug.Log($"NetworkManagerClient.ProcessCharacterJoinGroup({characterGroup.characterGroupId})");

            characterGroupServiceClient.ProcessLoadGroup(characterGroupNetworkData);
        }

        public void ProcessLoadGuild(GuildNetworkData guildNetworkData) {
            //Debug.Log($"NetworkManagerClient.ProcessLoadGuild({guild.guildId})");
            guildServiceClient.ProcessLoadGuild(guildNetworkData);
        }

        public void RequestLeaveCharacterGroup() {
            networkController.RequestLeaveCharacterGroup();
        }

        public void ProcessCharacterLeaveGroup(int removedPlayerId, int characterGroupId) {
            characterGroupServiceClient.RemoveCharacterFromGroup(removedPlayerId, characterGroupId);
        }

        public void ProcessCharacterLeaveGuild(int removedPlayerId, int guildId) {
            guildServiceClient.RemoveCharacterFromGuild(removedPlayerId, guildId);
        }

        public void RequestRemoveCharacterFromGroup(int playerCharacterId) {
            networkController.RequestRemoveCharacterFromGroup(playerCharacterId);
        }

        public void RequestInviteCharacterToGroup(int characterId) {
            networkController.RequestInviteCharacterToGroup(characterId);
        }

        public void RequestInviteCharacterToGroup(string characterName) {
            networkController.RequestInviteCharacterToGroup(characterName);
        }

        public void ProcessCharacterGroupInvite(int characterGroupId, string leaderName) {
            characterGroupServiceClient.DisplayCharacterGroupInvite(characterGroupId, leaderName);
        }

        public void ProcessGuildInvite(int guildId, string leaderName) {
            guildServiceClient.DisplayGuildInvite(guildId, leaderName);
        }

        public void RequestDisbandCharacterGroup(int characterGroupId) {
            networkController.RequestDisbandCharacterGroup(characterGroupId);
        }

        public void ProcessDisbandCharacterGroup(int characterGroupId) {
            characterGroupServiceClient.ProcessDisbandGroup(characterGroupId);
        }

        public void ProcessDisbandGuild(int guildId) {
            guildServiceClient.ProcessDisbandGuild(guildId);
        }

        public void AdvertisePlayerNameNotAvailable() {
            systemEventManager.NotifyOnPlayerNameNotAvailable();
        }

        public void SetCharacterList(List<PlayerCharacterSaveData> playerCharacterSaveDataList) {
            loadGameManager.SetCharacterList(playerCharacterSaveDataList);
        }

        public void ProcessDeclineCharacterGroupInvite(string decliningPlayerName) {
            messageLogClient.WriteSystemMessage($"{decliningPlayerName} has declined the group invite.");
        }

        public void ProcessDeclineGuildInvite(string decliningPlayerName) {
            messageLogClient.WriteSystemMessage($"{decliningPlayerName} has declined the guild invite.");
        }

        public void ProcessPromoteGroupLeader(int characterGroupId, int newLeaderCharacterId) {
            characterGroupServiceClient.ProcessPromoteGroupLeader(characterGroupId, newLeaderCharacterId);
        }

        public void ProcessPromoteGuildLeader(int guildId, int newLeaderCharacterId) {
            guildServiceClient.ProcessPromoteGuildLeader(guildId, newLeaderCharacterId);
        }

        public void RequestPromoteCharacterToLeader(int characterId) {
            networkController.RequestPromoteCharacterToLeader(characterId);
        }

        public void ProcessRenameCharacterInGroup(int characterGroupId, int characterId, string newName) {
            characterGroupServiceClient.ProcessRenameCharacterInGroup(characterGroupId, characterId, newName);
        }

        public void ProcessRenameCharacterInGuild(int guildId, int characterId, string newName) {
            guildServiceClient.ProcessRenameCharacterInGuild(guildId, characterId, newName);
        }

        public void AdvertiseGroupMessage(int characterGroupId, string messageText) {
            characterGroupServiceClient.AdvertiseGroupMessage(characterGroupId, messageText);
        }

        public void AdvertiseGuildMessage(int guildId, string messageText) {
            guildServiceClient.AdvertiseGuildMessage(guildId, messageText);
        }

        public void AdvertisePrivateMessage(string messageText) {
            messageLogClient.WritePrivateMessage(messageText);
        }

        public void RequestBeginTrade(int characterId) {
            networkController.RequestBeginTrade(characterId);
        }

        public void AdvertiseAcceptTradeInvite(int characterId) {
            //Debug.Log($"NetworkManagerClient.AdvertiseAcceptTradeInvite{characterId}");

            tradeServiceClient.AcceptTradeInvite(characterId);
        }

        public void AdvertiseDeclineTradeInvite() {
            tradeServiceClient.DeclineTradeInvite();
        }

        public void AdvertiseRequestBeginTrade(int sourceCharacterId) {
            tradeServiceClient.AdvertiseRequestBeginTrade(sourceCharacterId);
        }

        public void RequestDeclineTrade() {
            networkController.RequestDeclineTrade();
        }

        public void RequestAcceptTrade() {
            networkController.RequestAcceptTrade();
        }

        public void RequestAddItemsToTradeSlot(int buttonIndex, List<long> itemInstanceIdList) {
            networkController.RequestAddItemsToTradeSlot(buttonIndex, itemInstanceIdList);
        }

        public void AdvertiseAddItemsToTargetTradeSlot(int buttonIndex, List<long> itemInstanceIdList) {
            tradeServiceClient.AddItemsToTargetTradeSlot(buttonIndex, itemInstanceIdList);
        }

        public void RequestAddCurrencyToTrade(CurrencyNode currencyNode) {
            //Debug.Log("NetworkManagerClient.RequestAddCurrencyToTrade()");

            networkController.RequestAddCurrencyToTrade(currencyNode);
        }

        public void AdvertiseAddCurrencyToTrade(int amount) {
            tradeServiceClient.AdvertiseAddCurrencyToTrade(amount);
        }

        public void RequestConfirmTrade() {
            networkController.RequestConfirmTrade();
        }

        public void RequestCancelTrade() {
            networkController.RequestCancelTrade();
        }

        public void RequestUnconfirmTrade() {
            networkController.RequestUnconfirmTrade();
        }

        public void AdvertiseCancelTrade() {
            tradeServiceClient.AdvertiseCancelTrade();
        }

        public void AdvertiseTradeComplete() {
            tradeServiceClient.AdvertiseTradeComplete();
        }

        public void RequestSendMail(Interactable interactable, int componentIndex, MailMessageRequest sendMailRequest) {
            //Debug.Log($"mailboxManagerClient.RequestSendMail()");

            networkController.RequestSendMail(interactable, componentIndex, sendMailRequest);
        }

        public void RequestListAuctionItems(Interactable interactable, int componentIndex, ListAuctionItemRequest listAuctionItemRequest) {
            //Debug.Log($"mailboxManagerClient.RequestSendMail()");

            networkController.RequestListAuctionItems(interactable, componentIndex, listAuctionItemRequest);
        }

        public void AdvertiseMailMessages(MailMessageListBundle mailMessageListResponse) {
            mailboxManagerClient.SetMailMessages(mailMessageListResponse);
        }

        public void RequestDeleteMailMessage(int messageId) {
            networkController.RequestDeleteMailMessage(messageId);
        }

        public void RequestTakeMailAttachments(int messageId) {
            networkController.RequestTakeMailAttachments(messageId);
        }

        public void RequestTakeMailAttachment(int messageId, int attachmentSlotId) {
            networkController.RequestTakeMailAttachment(messageId, attachmentSlotId);
        }

        public void AdvertiseDeleteMailMessage(int messageId) {
            mailboxManagerClient.AdvertiseDeleteMailMessage(messageId);
        }

        public void AdvertiseTakeMailAttachment(int messageId, int attachmentSlotId) {
            mailboxManagerClient.AdvertiseTakeMailAttachment(messageId, attachmentSlotId);
        }

        public void AdvertiseTakeMailAttachments(int messageId) {
            mailboxManagerClient.AdvertiseTakeMailAttachments(messageId);
        }

        public void AdvertiseConfirmationPopup(string messageText) {
            uIManager.AdvertiseConfirmationPopup(messageText);
        }

        public void AdvertiseMailSend() {
            mailboxManagerClient.AdvertiseMailSend();
        }

        public void RequestMarkMailAsRead(int currentMessageId) {
            networkController.RequestMarkMailAsRead(currentMessageId);
        }

        public void RequestCancelAuction(int auctionItemId) {
            networkController.RequestCancelAuction(auctionItemId);
        }

        public void RequestBuyAuctionItem(int auctionItemId) {
            networkController.RequestBuyAuctionItem(auctionItemId);
        }

        public void AdvertiseBuyAuctionItem(int auctionItemId) {
            auctionManagerClient.AdvertiseBuyAuctionItem(auctionItemId);
        }

        public void AdvertiseCancelAuction(int auctionItemId) {
            auctionManagerClient.AdvertiseCancelAuction(auctionItemId);
        }

        public void RequestSearchAuctions(Interactable interactable, int componentIndex, string searchText, bool onlyShowOwnAuctions) {
            networkController.RequestSearchAuctions(interactable, componentIndex, searchText, onlyShowOwnAuctions);
        }

        public void AdvertiseAuctionItems(AuctionItemSearchListResult auctionItemListResponse) {
            auctionManagerClient.SetAuctionItems(auctionItemListResponse);
        }

        public void AdvertiseListAuctionItems() {
            auctionManagerClient.AdvertiseListItem();
        }

        public void DeclineGuildInvite() {
            networkController.DeclineGuildInvite();
        }

        public void AcceptGuildInvite(int inviteGuildId) {
            networkController.AcceptGuildInvite(inviteGuildId);
        }

        public void RequestLeaveGuild() {
            networkController.RequestLeaveGuild();
        }

        public void RequestInviteCharacterToGuild(int characterId) {
            networkController.RequestInviteCharacterToGuild(characterId);
        }

        public void RequestInviteCharacterToGuild(string characterName) {
            networkController.RequestInviteCharacterToGuild(characterName);
        }

        public void RequestRemoveCharacterFromGuild(int characterId) {
            networkController.RequestRemoveCharacterFromGuild(characterId);
        }

        public void RequestDisbandGuild(int guildId) {
            networkController.RequestDisbandGuild(guildId);
        }

        public void ProcessGuildNameAvailable() {
            guildServiceClient.ProcessGuildNameAvailable();
        }

        public void ProcessCharacterGroupMemberStatusChange(int characterGroupId, int playerCharacterId, CharacterGroupMemberNetworkData characterGroupMemberNetworkData) {
            characterGroupServiceClient.ProcessCharacterGroupMemberStatusChange(characterGroupId, playerCharacterId, characterGroupMemberNetworkData);
        }

        public void ProcessGuildMemberStatusChange(int guildId, int playerCharacterId, GuildMemberNetworkData guildMemberNetworkData) {
            guildServiceClient.ProcessGuildMemberStatusChange(guildId, playerCharacterId, guildMemberNetworkData);
        }

        public void ProcessAddFriend(CharacterSummaryNetworkData characterSummaryNetworkData) {
            friendServiceClient.AddCharacterToFriendList(characterSummaryNetworkData);
        }

        public void ProcessRemoveCharacterFromFriendList(int removedPlayerId) {
            friendServiceClient.RemoveCharacterFromFriendList(removedPlayerId);
        }

        public void ProcessDeclineFriendInvite(string decliningPlayerName) {
            friendServiceClient.ProcessDeclineFriendInvite(decliningPlayerName);
        }

        public void DeclineFriendInvite(int inviteCharacterId) {
            networkController.DeclineFriendInvite(inviteCharacterId);
        }

        public void ProcessFriendInvite(int inviterCharacterId, string leaderName) {
            friendServiceClient.DisplayFriendInvite(inviterCharacterId, leaderName);
        }

        public void ProcessLoadFriendList(FriendListNetworkData friendListNetworkData) {
            friendServiceClient.ProcessLoadFriendList(friendListNetworkData);
        }

        public void ProcessRenameCharacterInFriendList(int characterId, string newName) {
            friendServiceClient.ProcessRenameCharacterInFriendList(characterId, newName);
        }

        public void ProcessFriendStateChange(int playerCharacterId, CharacterSummaryNetworkData characterSummaryNetworkData) {
            friendServiceClient.ProcessFriendStateChange(playerCharacterId, characterSummaryNetworkData);
        }

        public void AcceptFriendInvite(int inviteCharacterId) {
            networkController.AcceptFriendInvite(inviteCharacterId);
        }

        public void RequestInviteCharacterToFriendList(int characterId) {
            networkController.RequestInviteCharacterToFriendList(characterId);
        }

        public void RequestInviteCharacterToFriendList(string characterName) {
            networkController.RequestInviteCharacterToFriendList(characterName);
        }

        public void RequestRemoveCharacterFromFriendList(int characterId) {
            networkController.RequestRemoveCharacterFromFriendList(characterId);
        }

        public void RequestPromoteGuildCharacter(int characterId) {
            networkController.RequestPromoteGuildCharacter(characterId);
        }

        public void RequestPromoteGroupCharacter(int characterId) {
            networkController.RequestPromoteGroupCharacter(characterId);
        }

        public void RequestDemoteGroupCharacter(int characterId) {
            networkController.RequestDemoteGroupCharacter(characterId);
        }

        public void RequestDemoteGuildCharacter(int characterId) {
            networkController.RequestDemoteGuildCharacter(characterId);
        }

    }

}