using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    public class NetworkManagerServer : ConfiguredMonoBehaviour {

        public event Action<LobbyGame> OnCreateLobbyGame = delegate { };
        public event Action<int> OnCancelLobbyGame = delegate { };
        public event Action<int, int, string> OnJoinLobbyGame = delegate { };
        public event Action<int> OnStartLobbyGame = delegate { };
        public event Action<int, int> OnLeaveLobbyGame = delegate { };
        public event Action OnStartServer = delegate { };
        public event Action OnBeforeStopServer = delegate { };
        public event Action OnStopServer = delegate { };

        [SerializeField]
        private NetworkController networkController = null;

        // list of lobby games
        private Dictionary<int, LobbyGame> lobbyGames = new Dictionary<int, LobbyGame>();

        /// <summary>
        /// accountId, gameId
        /// </summary>
        private Dictionary<int, int> lobbyGameAccountLookup = new Dictionary<int, int>();

        /// <summary>
        /// hashcode, gameId
        /// </summary>
        private Dictionary<int, int> lobbyGameLoadRequestHashCodes = new Dictionary<int, int>();

        /// <summary>
        /// gameId, sceneFileName, sceneHandle
        /// </summary>
        private Dictionary<int, Dictionary<string, int>> lobbyGameSceneHandles = new Dictionary<int, Dictionary<string, int>>();

        /// <summary>
        /// sceneHandle, gameId
        /// </summary>
        private Dictionary<int, int> lobbyGameSceneHandleLookup = new Dictionary<int, int>();

        /// <summary>
        /// hashcode, characterGroupId
        /// </summary>
        private Dictionary<int, int> characterGroupLoadRequestHashCodes = new Dictionary<int, int>();

        /// <summary>
        /// characterGroupId, sceneFileName, sceneHandle
        /// </summary>
        private Dictionary<int, Dictionary<string, int>> characterGroupSceneHandles = new Dictionary<int, Dictionary<string, int>>();

        /// <summary>
        /// sceneHandle, characterGroupId
        /// </summary>
        private Dictionary<int, int> characterGroupSceneHandleLookup = new Dictionary<int, int>();

        /// <summary>
        /// playerCharacterId, sceneFileName, sceneHandle
        /// </summary>
        private Dictionary<int, Dictionary<string, int>> personalSceneHandles = new Dictionary<int, Dictionary<string, int>>();

        /// <summary>
        /// hashcode, playerCharacterId
        /// </summary>
        private Dictionary<int, int> personalLoadRequestHashCodes = new Dictionary<int, int>();

        /// <summary>
        /// sceneHandle, playerCharacterId
        /// </summary>
        private Dictionary<int, int> personalSceneHandleLookup = new Dictionary<int, int>();


        private int lobbyGameCounter = 0;
        private int maxLobbyChatTextSize = 64000;
        private ushort port = 7770;

        // lobby chat
        private string lobbyChatText = string.Empty;
        private Dictionary<int, string> lobbyGameChatText = new Dictionary<int, string>();

        private bool serverModeActive = false;
        private NetworkServerMode serverMode = NetworkServerMode.MMO;

        // game manager references
        private SaveManager saveManager = null;
        private MessageLogServer messageLogServer = null;
        private PlayerManagerServer playerManagerServer = null;
        private CharacterManager characterManager = null;
        private InteractionManager interactionManager = null;
        private LevelManagerServer levelManagerServer = null;
        private SystemDataFactory systemDataFactory = null;
        private VendorManagerServer vendorManagerServer = null;
        private SystemItemManager systemItemManager = null;
        private LootManager lootManager = null;
        private CraftingManager craftingManager = null;
        private UnitSpawnManager unitSpawnManager = null;
        private LevelManager levelManager = null;
        private TimeOfDayManagerServer timeOfDayManagerServer = null;
        private WeatherManagerServer weatherManagerServer = null;
        private ClassChangeManagerServer classChangeManagerServer = null;
        private FactionChangeManagerServer factionChangeManagerServer = null;
        private SpecializationChangeManagerServer specializationChangeManagerServer = null;
        private SkillTrainerManagerServer skillTrainerManagerServer = null;
        private NameChangeManagerServer nameChangeManagerServer = null;
        private QuestGiverManagerServer questGiverManagerServer = null;
        private CharacterAppearanceManagerServer characterAppearanceManagerServer = null;
        private DialogManagerServer dialogManagerServer = null;
        private AuthenticationService authenticationService = null;
        private PlayerCharacterService playerCharacterService = null;
        private CharacterGroupServiceServer characterGroupServiceServer = null;
        private TradeServiceServer tradeServiceServer = null;
        private MailboxManagerServer mailboxManagerServer = null;
        private MailService mailService = null;
        private AuctionManagerServer auctionManagerServer = null;
        private GuildServiceServer guildServiceServer = null;
        private GuildmasterManagerServer guildmasterManagerServer = null;
        private FriendServiceServer friendServiceServer = null;
        private ServerDataService gameDataService = null;

        public bool ServerModeActive { get => serverModeActive; }
        public NetworkServerMode ServerMode { get => serverMode; }
        public Dictionary<int, LobbyGame> LobbyGames { get => lobbyGames; }
        public Dictionary<int, Dictionary<string, int>> LobbyGameSceneHandles { get => lobbyGameSceneHandles; }
        public Dictionary<int, Dictionary<string, int>> CharacterGroupSceneHandles { get => characterGroupSceneHandles; }
        public Dictionary<int, int> LobbyGameAccountLookup { get => lobbyGameAccountLookup; set => lobbyGameAccountLookup = value; }
        public NetworkController NetworkController { get => networkController; set => networkController = value; }
        //public RemoteGameServerClient GameServerClient { get => remoteGameServerClient; set => remoteGameServerClient = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            networkController?.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            saveManager = systemGameManager.SaveManager;
            messageLogServer = systemGameManager.MessageLogServer;
            playerManagerServer = systemGameManager.PlayerManagerServer;
            characterManager = systemGameManager.CharacterManager;
            interactionManager = systemGameManager.InteractionManager;
            levelManagerServer = systemGameManager.LevelManagerServer;
            systemDataFactory = systemGameManager.SystemDataFactory;
            vendorManagerServer = systemGameManager.VendorManagerServer;
            systemItemManager = systemGameManager.SystemItemManager;
            lootManager = systemGameManager.LootManager;
            craftingManager = systemGameManager.CraftingManager;
            unitSpawnManager = systemGameManager.UnitSpawnManager;
            levelManager = systemGameManager.LevelManager;
            timeOfDayManagerServer = systemGameManager.TimeOfDayManagerServer;
            weatherManagerServer = systemGameManager.WeatherManagerServer;
            classChangeManagerServer = systemGameManager.ClassChangeManagerServer;
            factionChangeManagerServer = systemGameManager.FactionChangeManagerServer;
            specializationChangeManagerServer = systemGameManager.SpecializationChangeManagerServer;
            skillTrainerManagerServer = systemGameManager.SkillTrainerManagerServer;
            nameChangeManagerServer = systemGameManager.NameChangeManagerServer;
            characterAppearanceManagerServer = systemGameManager.CharacterAppearanceManagerServer;
            dialogManagerServer = systemGameManager.DialogManagerServer;
            questGiverManagerServer = systemGameManager.QuestGiverManagerServer;
            authenticationService = systemGameManager.AuthenticationService;
            playerCharacterService = systemGameManager.PlayerCharacterService;
            characterGroupServiceServer = systemGameManager.CharacterGroupServiceServer;
            tradeServiceServer = systemGameManager.TradeServiceServer;
            mailboxManagerServer = systemGameManager.MailboxManagerServer;
            mailService = systemGameManager.MailService;
            auctionManagerServer = systemGameManager.AuctionManagerServer;
            guildServiceServer = systemGameManager.GuildServiceServer;
            guildmasterManagerServer = systemGameManager.GuildmasterManagerServer;
            friendServiceServer = systemGameManager.FriendServiceServer;
            gameDataService = systemGameManager.ServerDataService;
        }

        public void RequestCreatePlayerCharacter(int clientId, CharacterSaveData requestedSaveData) {
            //Debug.Log($"NetworkManagerServer.CreatePlayerCharacter(clientId: {clientId})");

            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            if (authenticationService.LoggedInAccounts.ContainsKey(accountId) == false) {
                // can't do anything without a token
                return;
            }
            playerCharacterService.RequestCreatePlayerCharacter(accountId, requestedSaveData);
        }

        public void RequestDeletePlayerCharacter(int clientId, int playerCharacterId) {
            //Debug.Log($"NetworkManagerServer.DeletePlayerCharacter({playerCharacterId})");

            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            playerCharacterService.RequestDeletePlayerCharacter(accountId, playerCharacterId);
        }

        public void ProcessStopNetworkUnitServer(UnitController unitController) {
            //Debug.Log($"NetworkManagerServer.ProcessStopNetworkUnitServer({unitController.gameObject.name})");

            if (SystemGameManager.IsShuttingDown == true) {
                return;
            }

            characterManager.ProcessStopNetworkUnit(unitController);
            //if (unitController.UnitControllerMode == UnitControllerMode.Player) {
                //playerManagerServer.StopMonitoringPlayerUnit(unitController);
            //}
        }

        public void RequestLoadCharacterList(int clientId) {
            //Debug.Log($"NetworkManagerServer.RequestLoadCharacterList(clientId: {clientId})");

            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }

            playerCharacterService.LoadCharacterList(accountId);

        }

        public void ProcessClientDisconnect(int clientId) {
            authenticationService.ProcessClientDisconnect(clientId);
        }

        public void ActivateServerMode() {
            //Debug.Log($"NetworkManagerServer.ActivateServerMode()");

            systemEventManager.OnChooseWeather += HandleChooseWeather;
            systemEventManager.OnStartWeather += HandleStartWeather;
            systemEventManager.OnEndWeather += HandleEndWeather;

            serverModeActive = true;

            // run functions with no dependencies on database data
            OnStartServer();

            gameDataService.LoadServerData();
        }

        public void DeactivateServerMode() {
            //Debug.Log($"NetworkManagerServer.DeactivateServerMode()");

            serverModeActive = false;

            authenticationService.ProcessDeactivateServerMode();
            CancelLobbyGames();
            lobbyGameChatText.Clear();

            OnStopServer();
            systemEventManager.OnChooseWeather -= HandleChooseWeather;
            systemEventManager.OnStartWeather -= HandleStartWeather;
            systemEventManager.OnEndWeather -= HandleEndWeather;

        }

        private void HandleStartWeather(int sceneHandle) {
            networkController.AdvertiseStartWeather(sceneHandle);
        }

        private void HandleEndWeather(int sceneHandle, WeatherProfile profile, bool immediate) {
            networkController?.AdvertiseEndWeather(sceneHandle, profile, immediate);
        }

        private void HandleChooseWeather(int sceneHandle, WeatherProfile profile) {
            networkController?.AdvertiseChooseWeather(sceneHandle, profile);
        }

        public void StartServer() {
            //Debug.Log($"NetworkManagerServer.StartServer()");

            if (serverModeActive == true) {
                return;
            }

            networkController?.StartServer(port);
        }

        public void StopServer() {
            //Debug.Log($"NetworkManagerServer.StartServer()");

            if (serverModeActive == false) {
                return;
            }
            OnBeforeStopServer();

            CancelLobbyGames();

            // logout all logged in accounts
            List<int> loggedInAccountIds = new List<int>(authenticationService.LoggedInAccounts.Keys);
            foreach (int accountId in loggedInAccountIds) {
                Logout(accountId);
            }

            networkController?.StopServer();
        }

        public void KickPlayer(int accountId) {
            //Debug.Log($"NetworkManagerServer.KickPlayer(accountId: {accountId})");

            int clientId = GetClientIDForAccount(accountId);
            if (clientId == -1) {
                return;
            }
            networkController.KickPlayer(clientId);
        }

        public string GetClientIPAddress(int clientId) {
            return networkController?.GetClientIPAddress(clientId);
        }

        public void CreateLobbyGame(string sceneResourceName, int clientId, bool allowLateJoin) {
            //Debug.Log($"NetworkManagerServer.CreateLobbyGame(sceneResourceName: {sceneResourceName}, clientId: {clientId}, allowLateJoin: {allowLateJoin})");

            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                Debug.LogWarning($"NetworkManagerServer.CreateLobbyGame(sceneResourceName: {sceneResourceName}, clientId: {clientId}, allowLateJoin: {allowLateJoin}) could not get account for client");
                return;
            }
            LobbyGame lobbyGame = new LobbyGame(accountId, lobbyGameCounter, sceneResourceName, authenticationService.LoggedInAccounts[accountId].username, allowLateJoin);
            lobbyGameCounter++;
            lobbyGames.Add(lobbyGame.gameId, lobbyGame);
            lobbyGameAccountLookup.Add(accountId, lobbyGame.gameId);
            lobbyGameChatText.Add(lobbyGame.gameId, string.Empty);
            OnCreateLobbyGame(lobbyGame);
            networkController.AdvertiseCreateLobbyGame(lobbyGame);
        }

        public void CancelLobbyGame(int clientId, int gameId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            if (lobbyGames.ContainsKey(gameId) == false || lobbyGames[gameId].leaderAccountId != accountId) {
                // game not found, or requesting client is not leader
                return;
            }
            CancelLobbyGame(gameId);
        }

        public void CancelLobbyGame(int gameId) {
            foreach (int accountIdInGame in lobbyGames[gameId].PlayerList.Keys) {
                lobbyGameAccountLookup.Remove(accountIdInGame);
            }
            lobbyGames.Remove(gameId);
            lobbyGameChatText.Remove(gameId);
            OnCancelLobbyGame(gameId);
            networkController.AdvertiseCancelLobbyGame(gameId);
        }

        private void CancelLobbyGames() {
            List<int> lobbyGameIds = new List<int>(lobbyGames.Keys);
            foreach (int lobbyGameId in lobbyGameIds) {
                CancelLobbyGame(lobbyGameId);
            }
        }

        public void JoinLobbyGame(int gameId, int clientId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            if (lobbyGames.ContainsKey(gameId) == false || authenticationService.LoggedInAccounts.ContainsKey(accountId) == false) {
                // game or client doesn't exist
                return;
            }
            lobbyGames[gameId].AddPlayer(accountId, authenticationService.LoggedInAccounts[accountId].username);
            lobbyGameAccountLookup.Add(accountId, gameId);
            OnJoinLobbyGame(gameId, accountId, authenticationService.LoggedInAccounts[accountId].username);
            networkController.AdvertiseAccountJoinLobbyGame(gameId, accountId, authenticationService.LoggedInAccounts[accountId].username);
        }

        public void RequestLobbyGameList(int clientId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            networkController.SetLobbyGameList(accountId, lobbyGames.Values.ToList<LobbyGame>());
        }

        public void RequestLobbyPlayerList(int clientId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            Dictionary<int, string> lobbyPlayerList = new Dictionary<int, string>();
            foreach (int loggedInClientId in authenticationService.LoggedInAccountsByClient.Keys) {
                lobbyPlayerList.Add(loggedInClientId, authenticationService.LoggedInAccountsByClient[loggedInClientId].username);
            }
            networkController.SetLobbyPlayerList(accountId, lobbyPlayerList);
        }

        public void ChooseLobbyGameCharacter(int gameId, int clientId, string unitProfileName, string appearanceString, List<SwappableMeshSaveData> swappableMeshSaveData) {
            //Debug.Log($"NetworkManagerServer.ChooseLobbyGameCharacter({gameId}, {accountId}, {unitProfileName})");

            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            if (lobbyGames.ContainsKey(gameId) == false || authenticationService.LoggedInAccounts.ContainsKey(accountId) == false) {
                Debug.LogWarning($"NetworkManagerServer.ChooseLobbyGameCharacter({gameId}, {accountId}, {unitProfileName}) - lobby game or client does not exist");
                return;
            }
            if (lobbyGames[gameId].PlayerList.ContainsKey(accountId) == false) {
                Debug.LogWarning($"NetworkManagerServer.ChooseLobbyGameCharacter({gameId}, {accountId}, {unitProfileName}) - client not in lobby game");
                return;
            }
            lobbyGames[gameId].PlayerList[accountId].unitProfileName = unitProfileName;
            lobbyGames[gameId].PlayerList[accountId].appearanceString = appearanceString;
            lobbyGames[gameId].PlayerList[accountId].swappableMeshSaveData = swappableMeshSaveData;
            networkController.AdvertiseChooseLobbyGameCharacter(gameId, accountId, unitProfileName);
        }

        public void ToggleLobbyGameReadyStatus(int gameId, int clientId) {
            //Debug.Log($"NetworkManagerClient.ToggleLobbyGameReadyStatus({gameId}, {accountId})");

            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }


            if (lobbyGames.ContainsKey(gameId) == false || lobbyGames[gameId].PlayerList.ContainsKey(accountId) == false) {
                // game did not exist or client was not in game
                return;
            }

            lobbyGames[gameId].PlayerList[accountId].ready = !lobbyGames[gameId].PlayerList[accountId].ready;
            networkController.AdvertiseSetLobbyGameReadyStatus(gameId, accountId, lobbyGames[gameId].PlayerList[accountId].ready);
        }

        public void RequestStartLobbyGame(int gameId, int clientId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            if (lobbyGames.ContainsKey(gameId) == false || lobbyGames[gameId].leaderAccountId != accountId || lobbyGames[gameId].inProgress == true) {
                // game did not exist, non leader tried to start, or already in progress, nothing to do
                return;
            }
            StartLobbyGame(gameId);
        }

        public void RequestJoinLobbyGameInProgress(int gameId, int clientId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            if (lobbyGames.ContainsKey(gameId) == false || lobbyGames[gameId].inProgress == false || lobbyGames[gameId].allowLateJoin == false) {
                // game did not exist or not in progress or does not allow late joins
                return;
            }
            JoinLobbyGameInProgress(gameId, accountId);
        }

        public void JoinLobbyGameInProgress(int gameId, int accountId) {
            //Debug.Log($"NetworkManagerServer.JoinLobbyGameInProgress({gameId}, {accountId})");

            string sceneName = string.Empty;
            //if (playerManagerServer.SpawnRequests.ContainsKey(accountId) == false) {
            if (playerManagerServer.PlayerCharacterMonitors.ContainsKey(accountId) == false) {
                //Debug.Log($"NetworkManagerServer.JoinLobbyGameInProgress({gameId}, {accountId}) - new spawn setting appearance");
                sceneName = lobbyGames[gameId].sceneResourceName;
                CharacterSaveData playerCharacterSaveData = GetNewLobbyGameCharacterSaveData(gameId, accountId, lobbyGames[gameId].PlayerList[accountId].unitProfileName);
                playerCharacterSaveData.AppearanceString = lobbyGames[gameId].PlayerList[accountId].appearanceString;
                playerCharacterSaveData.SwappableMeshSaveData = lobbyGames[gameId].PlayerList[accountId].swappableMeshSaveData;

                playerManagerServer.AddPlayerMonitor(accountId, playerCharacterSaveData);
            } else {
                // player already has a spawn request, so this is a rejoin.  Leave it alone because it contains the last correct position and direction
                //Debug.Log($"NetworkManagerServer.JoinLobbyGameInProgress({gameId}, {accountId}) - reusing existing scene from save data");
                sceneName = playerManagerServer.PlayerCharacterMonitors[accountId].characterSaveData.CurrentScene;
                if (levelManager.SceneDictionary.ContainsKey(sceneName)) {
                    sceneName = levelManager.SceneDictionary[sceneName].ResourceName;
                }
            }
            networkController.AdvertiseJoinLobbyGameInProgress(gameId, accountId, sceneName);

            LobbyGame lobbyGame = lobbyGames[gameId];

            // first try the scene resource name provided, then fallback to the lobby game default scene resource name
            SceneNode loadingSceneNode = systemDataFactory.GetResource<SceneNode>(sceneName);
            if (loadingSceneNode == null) {
                loadingSceneNode = systemDataFactory.GetResource<SceneNode>(lobbyGame.sceneResourceName);
                if (loadingSceneNode == null) {
                    return;
                }
            }

            LoadLobbyGameScene(accountId, lobbyGame, loadingSceneNode);
        }

        public void StartLobbyGame(int gameId) {
            //Debug.Log($"NetworkManagerServer.StartLobbyGame({gameId})");

            lobbyGames[gameId].inProgress = true;
            OnStartLobbyGame(gameId);
            // create spawn requests for all players in the game
            foreach (KeyValuePair<int, LobbyGamePlayerInfo> playerInfo in lobbyGames[gameId].PlayerList) {
                CharacterSaveData characterSaveData = GetNewLobbyGameCharacterSaveData(gameId, playerInfo.Key, playerInfo.Value.unitProfileName);
                characterSaveData.AppearanceString = playerInfo.Value.appearanceString;
                characterSaveData.SwappableMeshSaveData = playerInfo.Value.swappableMeshSaveData;

                playerManagerServer.AddPlayerMonitor(playerInfo.Key, characterSaveData);

            }
            networkController.StartLobbyGame(gameId);
        }

        public void LeaveLobbyGame(int gameId, int clientId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            if (lobbyGames.ContainsKey(gameId) == false || authenticationService.LoggedInAccounts.ContainsKey(accountId) == false) {
                // game or client doesn't exist
                return;
            }
            if (lobbyGames[gameId].leaderAccountId == accountId && lobbyGames[gameId].inProgress == false) {
                CancelLobbyGame(accountId, gameId);
            } else {
                lobbyGames[gameId].RemovePlayer(accountId);
                lobbyGameAccountLookup.Remove(accountId);
                OnLeaveLobbyGame(gameId, accountId);
                networkController.AdvertiseAccountLeaveLobbyGame(gameId, accountId);
            }
        }

        public void SendLobbyChatMessage(string messageText, int clientId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            if (authenticationService.LoggedInAccounts.ContainsKey(accountId) == false) {
                return;
            }
            string addedText = $"{authenticationService.LoggedInAccounts[accountId].username}: {messageText}\n";
            lobbyChatText += addedText;
            lobbyChatText = ShortenStringOnNewline(lobbyChatText, maxLobbyChatTextSize);

            networkController.AdvertiseSendLobbyChatMessage(addedText);
        }

        public void SendLobbyGameChatMessage(string messageText, int clientId, int gameId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            if (authenticationService.LoggedInAccounts.ContainsKey(accountId) == false) {
                return;
            }
            if (lobbyGames.ContainsKey(gameId) == false || lobbyGames[gameId].PlayerList.ContainsKey(accountId) == false) {
                return;
            }
            string addedText = $"{authenticationService.LoggedInAccounts[accountId].username}: {messageText}\n";
            lobbyGameChatText[gameId] += addedText;
            lobbyGameChatText[gameId] = ShortenStringOnNewline(lobbyGameChatText[gameId], maxLobbyChatTextSize);

            networkController.AdvertiseSendLobbyGameChatMessage(addedText, gameId);
        }

        public void SendSceneChatMessage(string messageText, int clientId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            if (authenticationService.LoggedInAccounts.ContainsKey(accountId) == false) {
                return;
            }
            messageLogServer.WriteChatMessage(accountId, messageText);
        }

        public void AdvertiseSceneChatMessage(string messageText, string modifiedMessageText, int accountId) {
            if (playerManagerServer.PlayerCharacterMonitors.ContainsKey(accountId) == false) {
                // no unit logged in
                return;
            }

            // send the modified text with username to the chat window
            networkController.AdvertiseSendSceneChatMessage(modifiedMessageText, accountId);

            // send original text to the dialog popup over the player's head
            playerManagerServer.PlayerCharacterMonitors[accountId].unitController.UnitEventController.NotifyOnBeginChatMessage(messageText);
        }

        public void AdvertiseLoadScene(UnitController sourceUnitController, string sceneName, int accountId) {
            //Debug.Log($"NetworkManagerServer.AdvertiseLoadScene({sourceUnitController.gameObject.name}, {sceneName}, {accountId})");
            
            //string oldSceneName = sourceUnitController.gameObject.scene.name;
            //int oldSceneHandle = sourceUnitController.gameObject.scene.handle;
            DespawnPlayerUnit(accountId);
            //systemEventManager.NotifyOnLevelUnloadServer(oldSceneHandle, oldSceneName);
            ChangeScene(accountId, sceneName);
        }

        public void ChangeScene(int accountId, string sceneName) {
            //Debug.Log($"NetworkManagerServer.ChangeScene(accountId: {accountId}, sceneName: {sceneName})");

            networkController.AdvertiseUnloadScene(accountId);

            int playerCharacterId = playerManagerServer.GetPlayerCharacterId(accountId);

            SceneNode sceneNode = levelManager.GetSceneNodeBySceneName(sceneName);
            if (sceneNode == null) {
                Debug.LogWarning($"NetworkManagerServer.ChangeScene(accountId: {accountId}, sceneName: {sceneName}) could not find scene node");
                return;
            }

            if (serverMode == NetworkServerMode.MMO) {
                LoadMMOGameScene(accountId, playerCharacterId, sceneNode);
            } else if (serverMode == NetworkServerMode.Lobby) {
                LobbyGame lobbyGame = lobbyGames[lobbyGameAccountLookup[accountId]];
                LoadLobbyGameScene(accountId, lobbyGame, sceneNode);
            }
        }

        public void LoadLobbyGameScene(int accountId, LobbyGame lobbyGame, SceneNode sceneNode) {
            //Debug.Log($"FishNetClientConnector.LoadLobbyGameScene({lobbyGame.gameId}, {sceneNode.SceneFile}, {networkConnection.ClientId}");

            if (lobbyGameSceneHandles.ContainsKey(lobbyGame.gameId) == false || lobbyGameSceneHandles[lobbyGame.gameId].ContainsKey(sceneNode.SceneFile) == false) {
                networkController.LoadNewLobbyGameScene(accountId, lobbyGame, sceneNode);
                return;
            }
            networkController.LoadExistingScene(accountId, lobbyGameSceneHandles[lobbyGame.gameId][sceneNode.SceneFile]);
        }

        public void AdvertiseLoadCutscene(Cutscene cutscene, int accountId) {
            networkController.AdvertiseLoadCutscene(cutscene, accountId);
        }

        public void DespawnPlayerUnit(int accountId) {
            //Debug.Log($"NetworkManagerServer.DespawnPlayerUnit({accountId})");

            playerManagerServer.DespawnPlayerUnit(accountId);
        }

        public void RequestDespawnPlayerUnit(int clientId) {
            //Debug.Log($"NetworkManagerServer.RequestDespawnPlayerUnit({accountId})");
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }

            // this method is only called when loading a cutscene, so we need to create a spawn request so the player spawns
            // at the correct position and direction when the cutscene ends
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId)) {
                SpawnPlayerRequest spawnPlayerRequest = new SpawnPlayerRequest() {
                    overrideSpawnDirection = true,
                    spawnForwardDirection = playerManagerServer.ActiveUnitControllers[accountId].transform.forward,
                    overrideSpawnLocation = true,
                    spawnLocation = playerManagerServer.ActiveUnitControllers[accountId].transform.position
                };
                DespawnPlayerUnit(accountId);
                playerManagerServer.AddSpawnRequest(accountId, spawnPlayerRequest, true);
            }
        }

        public static string ShortenStringOnNewline(string message, int messageLength) {
            // if the chat text is greater than the max size, keep splitting it on newlines until reaches an acceptable size
            while (message.Length > messageLength && message.Contains("\n")) {
                message = message.Split("\n", 1)[1];
            }
            return message;
        }

        public int GetServerPort() {
            return networkController.GetServerPort();
        }

        public void AdvertiseTeleport(int accountId, UnitController sourceUnitController, TeleportEffectProperties teleportEffectProperties) {
            //Debug.Log($"NetworkManagerServer.AdvertiseTeleport({accountId}, {teleportEffectProperties.LevelName})");

            //string oldSceneName = sourceUnitController.gameObject.scene.name;
            //int oldSceneHandle = sourceUnitController.gameObject.scene.handle;
            DespawnPlayerUnit(accountId);
            //systemEventManager.NotifyOnLevelUnloadServer(oldSceneHandle, oldSceneName);
            ChangeScene(accountId, teleportEffectProperties.LevelName);
        }

        public void ReturnObjectToPool(GameObject returnedObject) {
            //Debug.Log($"NetworkManagerServer.ReturnObjectToPool({returnedObject.name})");

            networkController.ReturnObjectToPool(returnedObject);
        }

        /*
        public void AdvertiseInteractWithQuestGiver(Interactable interactable, int optionIndex, UnitController sourceUnitController) {
            if (playerManagerServer.ActivePlayerLookup.ContainsKey(sourceUnitController)) {
                networkController.AdvertiseInteractWithQuestGiver(interactable, optionIndex, playerManagerServer.ActivePlayerLookup[sourceUnitController]);
            }
        }
        */

        public void InteractWithOption(UnitController sourceUnitController, Interactable interactable, int componentIndex, int choiceIndex) {
            interactionManager.InteractWithOptionServer(sourceUnitController, interactable, componentIndex, choiceIndex);
        }

        /*
        public void AdvertiseAddSpawnRequest(int accountId, SpawnPlayerRequest loadSceneRequest) {
            networkController.AdvertiseAddSpawnRequest(accountId, loadSceneRequest);
        }
        */

        /*
        public void AdvertiseInteractWithClassChangeComponent(int accountId, Interactable interactable, int optionIndex) {
            networkController.AdvertiseInteractWithClassChangeComponentServer(accountId, interactable, optionIndex);
        }
        */

        public void HandleSceneLoadEnd(Scene scene, int loadRequestHashCode) {
            //Debug.Log($"NetworkManagerServer.HandleSceneLoadEnd({scene.name}, {loadRequestHashCode})");

            if (lobbyGameLoadRequestHashCodes.ContainsKey(loadRequestHashCode) == true) {
                //Debug.Log($"NetworkManagerServer.HandleSceneLoadEnd({scene.name}, {loadRequestHashCode}) - lobby game load request");
                AddLobbyGameSceneHandle(lobbyGameLoadRequestHashCodes[loadRequestHashCode], scene);
                lobbyGameLoadRequestHashCodes.Remove(loadRequestHashCode);
            }
            if (characterGroupLoadRequestHashCodes.ContainsKey(loadRequestHashCode) == true) {
                //Debug.Log($"NetworkManagerServer.HandleSceneLoadEnd({scene.name}, {loadRequestHashCode}) - character group load request");
                AddCharacterGroupSceneHandle(characterGroupLoadRequestHashCodes[loadRequestHashCode], scene);
                characterGroupLoadRequestHashCodes.Remove(loadRequestHashCode);
            }
            if (personalLoadRequestHashCodes.ContainsKey(loadRequestHashCode) == true) {
                //Debug.Log($"NetworkManagerServer.HandleSceneLoadEnd({scene.name}, {loadRequestHashCode}) - personalp load request");
                AddPersonalSceneHandle(personalLoadRequestHashCodes[loadRequestHashCode], scene);
                personalLoadRequestHashCodes.Remove(loadRequestHashCode);
            }

            levelManagerServer.AddLoadedScene(loadRequestHashCode, scene);
            levelManagerServer.ProcessLevelLoad(scene);
        }

        private void AddLobbyGameSceneHandle(int lobbyGameId, Scene scene) {
            //Debug.Log($"NetworkManagerServer.AddLobbyGameSceneHandle({lobbyGameId}, {scene.name}, {scene.handle})");

            if (lobbyGameSceneHandles.ContainsKey(lobbyGameId) == false) {
                lobbyGameSceneHandles.Add(lobbyGameId, new Dictionary<string, int>());
            }
            if (lobbyGameSceneHandles[lobbyGameId].ContainsKey(scene.name) == false) {
                lobbyGameSceneHandles[lobbyGameId].Add(scene.name, scene.handle);
            }
            if (lobbyGameSceneHandleLookup.ContainsKey(scene.handle) == false) {
                lobbyGameSceneHandleLookup.Add(scene.handle, lobbyGameId);
            }
        }

        private void AddCharacterGroupSceneHandle(int characterGroupId, Scene scene) {
            //Debug.Log($"NetworkManagerServer.AddCharacterGroupSceneHandle({characterGroupId}, {scene.name}, {scene.handle})");

            if (characterGroupSceneHandles.ContainsKey(characterGroupId) == false) {
                characterGroupSceneHandles.Add(characterGroupId, new Dictionary<string, int>());
            }
            if (characterGroupSceneHandles[characterGroupId].ContainsKey(scene.name) == false) {
                characterGroupSceneHandles[characterGroupId].Add(scene.name, scene.handle);
            }
            if (characterGroupSceneHandleLookup.ContainsKey(scene.handle) == false) {
                characterGroupSceneHandleLookup.Add(scene.handle, characterGroupId);
            }
        }

        private void AddPersonalSceneHandle(int playerCharacterId, Scene scene) {
            //Debug.Log($"NetworkManagerServer.AddCharacterGroupSceneHandle({characterGroupId}, {scene.name}, {scene.handle})");

            if (personalSceneHandles.ContainsKey(playerCharacterId) == false) {
                personalSceneHandles.Add(playerCharacterId, new Dictionary<string, int>());
            }
            if (personalSceneHandles[playerCharacterId].ContainsKey(scene.name) == false) {
                personalSceneHandles[playerCharacterId].Add(scene.name, scene.handle);
            }
            if (personalSceneHandleLookup.ContainsKey(scene.handle) == false) {
                personalSceneHandleLookup.Add(scene.handle, playerCharacterId);
            }
        }

        public void HandleSceneUnloadStart(int sceneHandle, string sceneName) {
            //Debug.Log($"NetworkManagerServer.HandleSceneUnloadStart({sceneName}, {sceneHandle})");

            systemEventManager.NotifyOnLevelUnloadServer(sceneHandle, sceneName);
        }

        public void HandleSceneUnloadEnd(int sceneHandle, string sceneName) {
            //Debug.Log($"NetworkManagerServer.HandleSceneUnloadEnd({sceneName}, {sceneHandle})");

            if (lobbyGameSceneHandleLookup.ContainsKey(sceneHandle) == true) {
                //Debug.Log($"NetworkManagerServer.HandleSceneUnloadEnd({sceneName}, {sceneHandle}) - lobby game unload request");
                int lobbyGameId = lobbyGameSceneHandleLookup[sceneHandle];
                if (lobbyGameSceneHandles.ContainsKey(lobbyGameId) == true && lobbyGameSceneHandles[lobbyGameId].ContainsKey(sceneName) == true) {
                    lobbyGameSceneHandles[lobbyGameId].Remove(sceneName);
                }
                lobbyGameSceneHandleLookup.Remove(sceneHandle);
            }
            if (characterGroupSceneHandleLookup.ContainsKey(sceneHandle) == true) {
                //Debug.Log($"NetworkManagerServer.HandleSceneUnloadEnd({sceneName}, {sceneHandle}) - character group unload request");
                int characterGroupId = characterGroupSceneHandleLookup[sceneHandle];
                if (characterGroupSceneHandles.ContainsKey(characterGroupId) == true && characterGroupSceneHandles[characterGroupId].ContainsKey(sceneName) == true) {
                    characterGroupSceneHandles[characterGroupId].Remove(sceneName);
                }
                characterGroupSceneHandleLookup.Remove(sceneHandle);
            }
            if (personalSceneHandleLookup.ContainsKey(sceneHandle) == true) {
                //Debug.Log($"NetworkManagerServer.HandleSceneUnloadEnd({sceneName}, {sceneHandle}) - character group unload request");
                int playerCharacterId = personalSceneHandleLookup[sceneHandle];
                if (personalSceneHandles.ContainsKey(playerCharacterId) == true && personalSceneHandles[playerCharacterId].ContainsKey(sceneName) == true) {
                    personalSceneHandles[playerCharacterId].Remove(sceneName);
                }
                personalSceneHandleLookup.Remove(sceneHandle);
            }


            //levelManagerServer.RemoveLoadedScene(sceneHandle, sceneName);
        }

        public UnitController SpawnCharacterPrefab(CharacterRequestData characterRequestData, Transform parentTransform, Vector3 position, Vector3 forward, Scene scene) {
            return networkController.SpawnCharacterPrefab(characterRequestData, parentTransform, position, forward, scene);
        }

        public GameObject SpawnModelPrefab(GameObject spawnPrefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            //Debug.Log($"NetworkManagerServer.SpawnModelPrefab({spawnRequestId})");

            return networkController.SpawnModelPrefabServer(spawnPrefab, parentTransform, position, forward);
        }

        public void TurnInDialog(Interactable interactable, int componentIndex, Dialog dialog, int clientId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }
            dialogManagerServer.TurnInDialog(playerManagerServer.ActiveUnitControllers[accountId], interactable, componentIndex, dialog);
        }

        public void TurnInQuestDialog(Dialog dialog, int clientId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }
            playerManagerServer.ActiveUnitControllers[accountId].CharacterDialogManager.TurnInDialog(dialog);
        }


        public void SetPlayerCharacterClass(Interactable interactable, int componentIndex, int clientId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }
            classChangeManagerServer.ChangeCharacterClass(playerManagerServer.ActiveUnitControllers[accountId], interactable, componentIndex);
        }

        public void SetPlayerCharacterSpecialization(Interactable interactable, int componentIndex, int clientId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }
            specializationChangeManagerServer.ChangeCharacterSpecialization(playerManagerServer.ActiveUnitControllers[accountId], interactable, componentIndex);
        }

        public void SetPlayerFaction(Interactable interactable, int componentIndex, int clientId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }
            factionChangeManagerServer.ChangeCharacterFaction(playerManagerServer.ActiveUnitControllers[accountId], interactable, componentIndex);
        }

        public void RequestCreateGuild(Interactable interactable, int componentIndex, string guildName, int clientId) {
            //Debug.Log($"NetworkManagerServer.RequestCreateGuild({interactable.gameObject.name}, {componentIndex}, {guildName}, {accountId})");

            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }
            guildmasterManagerServer.CreateGuild(playerManagerServer.ActiveUnitControllers[accountId], interactable, componentIndex, guildName);
        }

        public void CheckGuildName(Interactable interactable, int componentIndex, string guildName, int clientId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }
            guildmasterManagerServer.CheckGuildName(playerManagerServer.ActiveUnitControllers[accountId], interactable, componentIndex, guildName);
        }


        public void LearnSkill(Interactable interactable, int componentIndex, int skillId, int clientId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }
            skillTrainerManagerServer.LearnSkill(playerManagerServer.ActiveUnitControllers[accountId], interactable, componentIndex, skillId);
        }

        public void RequestSendMail(Interactable interactable, int componentIndex, MailMessageRequest sendMailRequest, int clientId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }
            mailboxManagerServer.RequestSendMail(playerManagerServer.ActiveUnitControllers[accountId], interactable, componentIndex, sendMailRequest);
        }

        public void RequestListAuctionItems(Interactable interactable, int componentIndex, ListAuctionItemRequest listAuctionItemRequest, int clientId) {
            //Debug.Log($"NetworkManagerServer.RequestListAuctionItems()");
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }
            auctionManagerServer.RequestListAuctionItems(playerManagerServer.ActiveUnitControllers[accountId], interactable, componentIndex, listAuctionItemRequest);
        }

        public void RequestSearchAuctions(Interactable interactable, int componentIndex, string searchText, bool onlyShowOwnAuctions, int clientId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }
            auctionManagerServer.RequestSearchAuctions(playerManagerServer.ActiveUnitControllers[accountId], interactable, componentIndex, searchText, onlyShowOwnAuctions);
        }


        public void AcceptQuest(Interactable interactable, int componentIndex, Quest quest, int clientId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }

            questGiverManagerServer.AcceptQuest(interactable, componentIndex, playerManagerServer.ActiveUnitControllers[accountId], quest);
        }

        public void CompleteQuest(Interactable interactable, int componentIndex, Quest quest, QuestRewardChoices questRewardChoices, int clientId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }

            questGiverManagerServer.CompleteQuest(interactable, componentIndex, playerManagerServer.ActiveUnitControllers[accountId], quest, questRewardChoices);
        }


        public void AdvertiseMessageFeedMessage(UnitController sourceUnitController, string message) {
            networkController.AdvertiseMessageFeedMessage(playerManagerServer.ActiveUnitControllerLookup[sourceUnitController], message);
        }

        public void AdvertiseSystemMessage(int accountId, string message) {
            networkController.AdvertiseSystemMessage(accountId, message);
        }

        public void AdvertiseSystemMessage(UnitController sourceUnitController, string message) {
            networkController.AdvertiseSystemMessage(playerManagerServer.ActiveUnitControllerLookup[sourceUnitController], message);
        }

        public void SellVendorItem(Interactable interactable, int componentIndex, long itemInstanceId, int clientId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }
            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId) == false) {
                return;
            }
            vendorManagerServer.SellItemToVendor(playerManagerServer.ActiveUnitControllers[accountId], interactable, componentIndex, systemItemManager.InstantiatedItems[itemInstanceId]);
        }

        public void RequestSpawnUnit(Interactable interactable, int componentIndex, int unitLevel, int extraLevels, bool useDynamicLevel, UnitProfile unitProfile, UnitToughness unitToughness, int clientId) {
            //Debug.Log($"NetworkManagerServer.RequestSpawnUnit({interactable.gameObject.name}, {componentIndex}, {unitLevel}, {extraLevels}, {useDynamicLevel}, {unitProfile.ResourceName}, {unitToughness?.ResourceName}, {accountId})");
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }

            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }
            unitSpawnManager.SpawnUnit(playerManagerServer.ActiveUnitControllers[accountId], interactable, componentIndex, unitLevel, extraLevels, useDynamicLevel, unitProfile, unitToughness);
        }


        public void AdvertiseAddToBuyBackCollection(UnitController sourceUnitController, Interactable interactable, int componentIndex, InstantiatedItem newInstantiatedItem) {
            networkController.AdvertiseAddToBuyBackCollection(sourceUnitController, playerManagerServer.ActiveUnitControllerLookup[sourceUnitController], interactable, componentIndex, newInstantiatedItem);
        }

        public void BuyItemFromVendor(Interactable interactable, int componentIndex, int collectionIndex, int itemIndex, string resourceName, int clientId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }
            vendorManagerServer.BuyItemFromVendor(playerManagerServer.ActiveUnitControllers[accountId], interactable, componentIndex, collectionIndex, itemIndex, resourceName, accountId);
        }

        public void TakeAllLoot(int clientId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == true) {
                lootManager.TakeAllLootInternal(accountId, playerManagerServer.ActiveUnitControllers[accountId]);
            }
        }

        public void RequestTakeLoot(int lootDropId, int clientId) {
            //Debug.Log($"NetworkManagerServer.RequestTakeLoot({lootDropId}, {accountId})");

            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == true) {
                //lootManager.TakeLoot(accountId, lootDropId);
                if (lootManager.LootDropIndex.ContainsKey(lootDropId) == false) {
                    return;
                }
                lootManager.LootDropIndex[lootDropId].TakeLoot(playerManagerServer.ActiveUnitControllers[accountId]);
            }
        }

        public void RequestBeginCrafting(Recipe recipe, int craftAmount, int clientId) {
            //Debug.Log($"NetworkManagerServer.RequestBeginCrafting({recipe.DisplayName}, {craftAmount}, {accountId})");

            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == true) {
                craftingManager.BeginCrafting(playerManagerServer.ActiveUnitControllers[accountId], recipe, craftAmount);
            }
        }

        public void RequestCancelCrafting(int clientId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == true) {
                craftingManager.CancelCrafting(playerManagerServer.ActiveUnitControllers[accountId]);
            }
        }


        public void AddAvailableDroppedLoot(int accountId, List<int> lootDropIds) {
            //Debug.Log($"NetworkManagerServer.AddAvailableDroppedLoot({accountId}, count: {items.Count})");

            networkController.AddAvailableDroppedLoot(accountId, lootDropIds);
        }

        public void AddLootDrop(int accountId, int lootDropId, long itemId) {
            networkController.AddLootDrop(accountId, lootDropId, itemId);
        }

        public void AdvertiseTakeLoot(int accountId, int lootDropId) {
            networkController.AdvertiseTakeLoot(accountId, lootDropId);
        }

        public void SetLobbyGameLoadRequestHashcode(int gameId, int hashCode) {
            //Debug.Log($"NetworkManagerServer.SetLobbyGameLoadRequestHashcode({gameId}, {hashCode})");

            if (lobbyGameLoadRequestHashCodes.ContainsKey(hashCode) == false) {
                lobbyGameLoadRequestHashCodes.Add(hashCode, gameId);
            }
        }

        public void SetCharacterGroupLoadRequestHashcode(int characterGroupId, int hashCode) {
            //Debug.Log($"NetworkManagerServer.SetLobbyGameLoadRequestHashcode({gameId}, {hashCode})");

            if (characterGroupLoadRequestHashCodes.ContainsKey(hashCode) == false) {
                characterGroupLoadRequestHashCodes.Add(hashCode, characterGroupId);
            }
        }

        public void SetPersonalLoadRequestHashcode(int playerCharacterId, int hashCode) {
            //Debug.Log($"NetworkManagerServer.SetLobbyGameLoadRequestHashcode({gameId}, {hashCode})");

            if (personalLoadRequestHashCodes.ContainsKey(hashCode) == false) {
                personalLoadRequestHashCodes.Add(hashCode, playerCharacterId);
            }
        }

        public void RequestSpawnLobbyGamePlayer(int accountId, int gameId, string sceneName) {
            //Debug.Log($"NetworkManagerServer.RequestSpawnLobbyGamePlayer({accountId}, {gameId}, {sceneName})");

            playerManagerServer.RequestSpawnPlayerUnit(accountId, sceneName);
        }

        public void RequestSpawnPlayer(int accountId, string sceneName) {
            //Debug.Log($"NetworkManagerServer.RequestSpawnPlayer(accountId: {accountId}, {sceneName})");

            playerManagerServer.RequestSpawnPlayerUnit(accountId, sceneName);
        }

        public void SpawnPlayer(int accountId, CharacterRequestData characterRequestData, Vector3 position, Vector3 forward, string sceneName) {
            //Debug.Log($"NetworkManagerServer.SpawnPlayer(accountId: {accountId}, sceneName: {sceneName})");

            networkController.SpawnPlayer(accountId, characterRequestData, position, forward, sceneName);
        }

        private CharacterSaveData GetNewLobbyGameCharacterSaveData(int gameId, int accountId, string unitProfileName) {
            //Debug.Log($"NetworkManagerServer.GetNewLobbyGamePlayerCharacterSaveData({gameId}, {accountId}, {unitProfileName})");

            UnitProfile unitProfile = systemDataFactory.GetResource<UnitProfile>(unitProfileName);
            return GetNewLobbyGameCharacterSaveData(gameId, accountId, unitProfile);
        }

        private CharacterSaveData GetNewLobbyGameCharacterSaveData(int gameId, int accountId, UnitProfile unitProfile) {
            //Debug.Log($"NetworkManagerServer.GetNewLobbyGamePlayerCharacterSaveData({gameId}, {accountId}, {unitProfile.ResourceName})");

            CharacterSaveData characterSaveData = saveManager.CreateSaveData();
            characterSaveData.CharacterId = accountId;
            characterSaveData.CharacterName = lobbyGames[gameId].PlayerList[accountId].userName;
            characterSaveData.UnitProfileName = unitProfile.ResourceName;
            characterSaveData.CurrentScene = lobbyGames[gameId].sceneResourceName;
            return characterSaveData;
        }

        public Scene GetAccountScene(int accountId, string sceneName) {
            return networkController.GetAccountScene(accountId, sceneName);
        }

        public void RequestRespawnPlayerUnit(int clientId) {
            //Debug.Log($"NetworkManagerServer.RequestRespawnPlayerUnit({accountId})");
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }

            playerManagerServer.RespawnPlayerUnit(accountId);
        }

        public void RequestRevivePlayerUnit(int clientId) {
            //Debug.Log($"NetworkManagerServer.RequestRevivePlayerUnit({accountId})");
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }

            playerManagerServer.RevivePlayerUnit(accountId);
        }

        public void MonitorPlayerUnit(int accountId, UnitController unitController) {
            playerManagerServer.MonitorPlayerUnit(accountId, unitController);
        }

        public void RequestUpdatePlayerAppearance(int clientId, Interactable interactable, int componentIndex, string unitProfileName, string appearanceString, List<SwappableMeshSaveData> swappableMeshSaveData) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }

            characterAppearanceManagerServer.UpdatePlayerAppearance(playerManagerServer.ActiveUnitControllers[accountId], accountId, interactable, componentIndex, unitProfileName, appearanceString, swappableMeshSaveData);
        }

        public void RequestChangePlayerName(Interactable interactable, int componentIndex, string newName, int clientId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }
            nameChangeManagerServer.SetPlayerName(playerManagerServer.ActiveUnitControllers[accountId], interactable, componentIndex, newName);
        }

        public void RequestSpawnPet(int clientId, UnitProfile unitProfile) {
            //Debug.Log($"NetworkManagerServer.RequestSpawnPet({accountId}, {unitProfile.ResourceName})");

            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            playerManagerServer.RequestSpawnPet(accountId, unitProfile);
        }

        public void RequestDespawnPet(int clientId, UnitProfile unitProfile) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            playerManagerServer.RequestDespawnPet(accountId, unitProfile);
        }

        public void AdvertiseAddSpawnRequest(int accountId, SpawnPlayerRequest loadSceneRequest) {
            //Debug.Log($"NetworkManagerServer.AdvertiseAddSpawnRequest({accountId})");

            networkController.AdvertiseAddSpawnRequest(accountId, loadSceneRequest);
        }

        public void Logout(int accountId) {
            authenticationService.Logout(accountId);
        }

        public void RequestSpawnRequest(int clientId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            playerManagerServer.RequestSpawnRequest(accountId);
        }

        public DateTime GetServerStartTime() {
            return timeOfDayManagerServer.StartTime;
        }

        public WeatherProfile GetSceneWeatherProfile(int handle) {
            return weatherManagerServer.GetSceneWeatherProfile(handle);
        }

        public void ReturnFromCutscene(int clientId) {
            //Debug.Log($"NetworkManagerServer.ReturnFromCutscene({accountId})");

            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }

            if (playerManagerServer.PlayerCharacterMonitors.ContainsKey(accountId) == false) {
                // no spawn request, nothing to do
                return;
            }
            string sceneName = playerManagerServer.PlayerCharacterMonitors[accountId].characterSaveData.CurrentScene;
            ChangeScene(accountId, sceneName);
        }

        public void SetServerPort(ushort port) {
            //Debug.Log($"NetworkManagerServer.SetServerPort({port})");

            this.port = port;
        }

        public void SetServerMode(NetworkServerMode networkServerMode) {
            //Debug.Log($"NetworkManagerServer.SetServerMode({networkServerMode})");

            this.serverMode = networkServerMode;
        }

        public void RequestLoadPlayerCharacter(int clientId, int playerCharacterId) {
            //Debug.Log($"NetworkManagerServer.RequestLoadPlayerCharacter(clientId: {clientId}, playerCharacterId: {playerCharacterId})");

            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            string sceneName = string.Empty;
            playerCharacterService.SetCharacterOnline(playerCharacterId, true);
            if (playerManagerServer.PlayerCharacterMonitors.ContainsKey(accountId) == false) {
                // no existing monitor, so this is a fresh login
                CharacterSaveData characterSaveData = playerCharacterService.GetPlayerCharacterSaveData(accountId, playerCharacterId);
                sceneName = characterSaveData.CurrentScene;
                playerManagerServer.AddPlayerMonitor(accountId, characterSaveData);
                // configure location and rotation overrides
                SpawnPlayerRequest spawnPlayerRequest = new SpawnPlayerRequest();
                if (characterSaveData.OverrideLocation == true) {
                    spawnPlayerRequest.overrideSpawnLocation = true;
                    spawnPlayerRequest.spawnLocation = new Vector3(characterSaveData.PlayerLocationX, characterSaveData.PlayerLocationY, characterSaveData.PlayerLocationZ);
                    //Debug.Log($"NetworkManagerServer.RequestLoadPlayerCharacter() overrideSpawnLocation: {loadSceneRequest.overrideSpawnLocation} location: {loadSceneRequest.spawnLocation}");
                }
                if (characterSaveData.OverrideRotation == true) {
                    spawnPlayerRequest.overrideSpawnDirection = true;
                    spawnPlayerRequest.spawnForwardDirection = new Vector3(characterSaveData.PlayerRotationX, characterSaveData.PlayerRotationY, characterSaveData.PlayerRotationZ);
                    //Debug.Log($"Savemanager.LoadGame() overrideRotation: {loadSceneRequest.overrideSpawnDirection} location: {loadSceneRequest.spawnForwardDirection}");
                }
                playerManagerServer.AddSpawnRequest(accountId, spawnPlayerRequest, true);
            } else {
                // there is an existing monitor, so the player must have been disconnected
                CharacterSaveData saveData = playerManagerServer.PlayerCharacterMonitors[accountId].characterSaveData;
                sceneName = playerManagerServer.PlayerCharacterMonitors[accountId].characterSaveData.CurrentScene;
                if (levelManager.SceneDictionary.ContainsKey(sceneName)) {
                    sceneName = levelManager.SceneDictionary[sceneName].ResourceName;
                }
                characterGroupServiceServer.SendCharacterGroupInfo(accountId, playerCharacterId);
            }
            if (serverMode != NetworkServerMode.Lobby) {
                guildServiceServer.SendGuildInfo(accountId, playerCharacterId);
                friendServiceServer.SendFriendListInfo(accountId);
            }

            SceneNode sceneNode = levelManager.GetSceneNodeBySceneName(sceneName);
            if (sceneNode == null) {
                Debug.LogWarning($"NetworkManagerServer.RequestLoadPlayerCharacter(clientId: {clientId}, playerCharacterId: {playerCharacterId}) could not find scene node for {sceneName}");
                return;
            }
            networkController.AdvertiseJoinMMOGameInProgress(accountId);
            LoadMMOGameScene(accountId, playerCharacterId, sceneNode);
            mailService.SendMailMessages(playerCharacterId);
        }

        public void LoadMMOGameScene(int accountId, int playerCharacterId, SceneNode sceneNode) {
            //Debug.Log($"FishNetClientConnector.LoadMMOGameScene(accountId: {accountId}, {sceneNode.SceneFile}, clientId: {networkConnection.ClientId}");

            // get characterGroupId for accountId
            int characterGroupId = characterGroupServiceServer.GetCharacterGroupIdFromCharacterId(playerCharacterId);
            //Debug.Log($"playerId: {playerId}; characterGroupId: {characterGroupId}");

            // dungeon cases
            if (sceneNode.IsDungeon == true) {
                
                if (characterGroupId != -1) {
                    // group dungeon with existing instance
                    if (CharacterGroupSceneHandles.ContainsKey(characterGroupId)
                    && CharacterGroupSceneHandles[characterGroupId].ContainsKey(sceneNode.SceneFile) == true) {
                        networkController.LoadExistingScene(accountId, CharacterGroupSceneHandles[characterGroupId][sceneNode.SceneFile]);
                        return;
                    }
                    // group dungeon with new instance
                    networkController.LoadNewScene(accountId, playerCharacterId,SceneInstanceType.Group, sceneNode);
                    return;
                }

                // personal dungeon with existing instance
                if (personalSceneHandles.ContainsKey(playerCharacterId)
                    && personalSceneHandles[playerCharacterId].ContainsKey(sceneNode.SceneFile) == true) {
                    networkController.LoadExistingScene(accountId, personalSceneHandles[playerCharacterId][sceneNode.SceneFile]);
                    return;
                }

                // personal dungeon with new instance
                networkController.LoadNewScene(accountId, playerCharacterId, SceneInstanceType.Personal, sceneNode);
                return;
            }

            // world scene case
            networkController.LoadNewScene(accountId, playerCharacterId, SceneInstanceType.World, sceneNode);
        }

        public void AcceptCharacterGroupInvite(int clientId, int characterGroupId) {
            //Debug.Log($"NetworkManagerServer.AcceptCharacterGroupInvite({accountId}, {characterGroupId})");

            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            characterGroupServiceServer.AcceptCharacterGroupInvite(accountId, characterGroupId);
        }

        public void DeclineCharacterGroupInvite(int clientId) {
            //Debug.Log($"NetworkManagerServer.DeclineCharacterGroupInvite({accountId})");

            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            characterGroupServiceServer.DeclineCharacterGroupInvite(accountId);
        }

        public void AcceptGuildInvite(int clientId, int guildId) {
            //Debug.Log($"NetworkManagerServer.AcceptGuildInvite({accountId}, {characterGroupId})");

            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            guildServiceServer.AcceptGuildInvite(accountId, guildId);
        }

        public void DeclineGuildInvite(int clientId) {
            //Debug.Log($"NetworkManagerServer.DeclineGuildInvite({accountId})");

            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            guildServiceServer.DeclineGuildInvite(accountId);
        }

        public void DeclineFriendInvite(int clientId, int friendId) {
            //Debug.Log($"NetworkManagerServer.DeclineGuildInvite({accountId})");
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }

            friendServiceServer.DeclineFriendInvite(accountId, friendId);
        }

        public void AdvertiseAddCharacterToGroup(int accountId, int characterGroupId, CharacterGroupMemberNetworkData characterGroupMemberNetworkData) {
            //Debug.Log($"NetworkManagerServer.AdvertiseAddCharacterToGroup({playerCharacterId}, {characterGroup.characterGroupId})");

            networkController.AdvertiseAddCharacterToGroup(accountId, characterGroupId, characterGroupMemberNetworkData);
        }

        public void AdvertiseAddCharacterToGuild(int existingAccountId, int guildId, GuildMemberNetworkData guildMemberNetworkData) {
            //Debug.Log($"NetworkManagerServer.AdvertiseAddCharacterToGuild(accountId: {existingAccountId}, guildId: {guildId})");

            networkController.AdvertiseAddCharacterToGuild(existingAccountId, guildId, guildMemberNetworkData);
        }

        public void AdvertiseCharacterGroup(int accountId, CharacterGroupNetworkData characterGroupNetworkData) {
            //Debug.Log($"NetworkManagerServer.AdvertiseCharacterGroup(accountId: {accountId}, groupId: {characterGroup.characterGroupId})");

            networkController.AdvertiseCharacterGroup(accountId, characterGroupNetworkData);
        }

        public void RequestLeaveCharacterGroup(int clientId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            characterGroupServiceServer.RequestLeaveCharacterGroup(accountId);
        }

        public void AdvertiseRemoveCharacterFromGroup(int accountId, int characterId, int groupId) {
            //Debug.Log($"NetworkManagerServer.AdvertiseRemoveCharacterFromGroup({characterId}, {characterGroup.characterGroupId})");

            networkController.AdvertiseRemoveCharacterFromGroup(accountId, characterId, groupId);
        }

        public void RequestRemoveCharacterFromGroup(int clientId, int playerCharacterId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            characterGroupServiceServer.RequestRemoveCharacterFromGroup(accountId, playerCharacterId);
        }

        public void RequestInviteCharacterToGroup(int clientId, int invitedCharacterId) {
            //Debug.Log($"NetworkManagerServer.RequestInviteCharacterToGroup({accountId}, {invitedCharacterId})");

            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            characterGroupServiceServer.RequestInviteCharacterToGroup(accountId, invitedCharacterId);
        }

        public void RequestInviteCharacterToGroup(int clientId, string invitedCharacterName) {
            //Debug.Log($"NetworkManagerServer.RequestInviteCharacterToGroup({accountId}, {invitedCharacterId})");

            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            characterGroupServiceServer.RequestInviteCharacterToGroup(accountId, invitedCharacterName);
        }

        public void RequestLeaveGuild(int clientId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            guildServiceServer.RequestLeaveGuild(accountId);
        }

        public void AdvertiseRemoveCharacterFromGuild(int accountId, int characterId, int guildId) {
            //Debug.Log($"NetworkManagerServer.AdvertiseRemoveCharacterFromGroup({characterId}, {characterGroup.characterGroupId})");

            networkController.AdvertiseRemoveCharacterFromGuild(accountId, characterId, guildId);
        }

        public void RequestRemoveCharacterFromGuild(int clientId, int playerCharacterId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            guildServiceServer.RequestRemoveCharacterFromGuild(accountId, playerCharacterId);
        }

        public void RequestRemoveCharacterFromFriendList(int clientId, int playerCharacterId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            friendServiceServer.RequestRemoveCharacterFromFriendList(accountId, playerCharacterId);
        }

        public void RequestInviteCharacterToGuild(int clientId, int invitedCharacterId) {
            //Debug.Log($"NetworkManagerServer.RequestInviteCharacterToGroup({accountId}, {invitedCharacterId})");

            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            guildServiceServer.RequestInviteCharacterToGuild(accountId, invitedCharacterId);
        }

        public void RequestInviteCharacterToFriendList(int clientId, int invitedCharacterId) {
            //Debug.Log($"NetworkManagerServer.RequestInviteCharacterToGroup({accountId}, {invitedCharacterId})");

            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            friendServiceServer.RequestInviteCharacterToFriend(accountId, invitedCharacterId);
        }

        public void RequestInviteCharacterToFriendList(int clientId, string characterName) {
            //Debug.Log($"NetworkManagerServer.RequestInviteCharacterToGroup(clientId: {clientId}, characterName: {characterName})");

            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }

            friendServiceServer.RequestInviteCharacterToFriend(accountId, characterName);
        }

        public void RequestInviteCharacterToGuild(int clientId, string invitedCharacterName) {
            //Debug.Log($"NetworkManagerServer.RequestInviteCharacterToGuild(clientId: {clientId}, invitedCharacterName: {invitedCharacterName})");

            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }

            guildServiceServer.RequestInviteCharacterToGuild(accountId, invitedCharacterName);
        }

        public void AdvertiseCharacterGroupInvite(int invitedCharacterId, int characterGroupId, string leaderName) {
            //Debug.Log($"NetworkManagerServer.AdvertiseCharacterGroupInvite({invitedCharacterId}, {characterGroup.characterGroupId}, {leaderName})");

            networkController.AdvertiseCharacterGroupInvite(invitedCharacterId, characterGroupId, leaderName);
        }

        public void RequestDisbandCharacterGroup(int clientId, int characterGroupId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            characterGroupServiceServer.DisbandGroup(accountId, characterGroupId);
        }

        public void RequestDisbandGuild(int clientId, int guildId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            guildServiceServer.DisbandGuild(accountId, guildId);
        }

        public void AdvertiseDisbandCharacterGroup(int accountId, int characterGroupId) {
            networkController.AdvertiseDisbandCharacterGroup(accountId, characterGroupId);
        }

        public void AdvertiseDeclineCharacterGroupInvite(int leaderAccountId, string decliningPlayerName) {
            networkController.AdvertiseDeclineCharacterGroupInvite(leaderAccountId, decliningPlayerName);
        }

        public void AdvertisePromoteGroupLeader(int accountId, int characterGroupId, int newLeaderCharacterId) {
            networkController.AdvertisePromoteGroupLeader(accountId, characterGroupId, newLeaderCharacterId);
        }

        public void RequestPromoteCharacterToLeader(int clientId, int characterId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            characterGroupServiceServer.RequestPromoteCharacter(accountId, characterId);
        }

        public void RequestPromoteGuildCharacter(int clientId, int characterId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            guildServiceServer.RequestPromoteCharacter(accountId, characterId);
        }

        public void RequestDemoteGuildCharacter(int clientId, int characterId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            guildServiceServer.RequestDemoteCharacter(accountId, characterId);
        }

        public void RequestPromoteGroupCharacter(int clientId, int characterId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            characterGroupServiceServer.RequestPromoteCharacter(accountId, characterId);
        }

        public void RequestDemoteGroupCharacter(int clientId, int characterId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            characterGroupServiceServer.RequestDemoteCharacter(accountId, characterId);
        }

        public void AdvertiseRenameCharacterInGroup(int accountId, int groupId, int characterId, string newName) {
            networkController.AdvertiseRenameCharacterInGroup(accountId, groupId, characterId, newName);
        }

        public void AdvertiseGroupMessage(int accountId, int characterGroupId, string messageText) {
            networkController.AdvertiseGroupMessage(accountId, characterGroupId, messageText);
        }

        public void AdvertiseGuildMessage(int accountId, int guildId, string messageText) {
            networkController.AdvertiseGuildMessage(accountId, guildId, messageText);
        }

        public void AdvertisePrivateMessage(int targetAccountId, string messageText) {
            //Debug.Log($"NetworkManagerServer.AdvertisePrivateMessage({targetAccountId}, {messageText})");

            networkController.AdvertisePrivateMessage(targetAccountId, messageText);
        }

        public void RequestBeginTrade(int clientId, int targetCharacterId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            tradeServiceServer.RequestBeginTrade(accountId, targetCharacterId);
        }

        public void AdvertiseAcceptTradeInvite(int sourceAccountId, int targetCharacterId) {
            //Debug.Log($"NetworkManagerServer.AdvertiseAcceptTradeInvite({sourceAccountId}, {targetCharacterId})");

            networkController.AdvertiseAcceptTradeInvite(sourceAccountId, targetCharacterId);
        }

        public void AdvertiseDeclineTradeInvite(int sourceAccountId) {
            networkController.AdvertiseDeclineTradeInvite(sourceAccountId);
        }

        public void AdvertiseRequestBeginTrade(int targetAccountId, int sourceCharacterId) {
            networkController.AdvertiseRequestBeginTrade(targetAccountId, sourceCharacterId);
        }

        public void RequestDeclineTrade(int clientId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            tradeServiceServer.DeclineTradeInvite(accountId);
        }

        public void RequestAcceptTrade(int clientId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            tradeServiceServer.AcceptTradeInvite(accountId);
        }

        public void RequestAddItemsToTradeSlot(int clientId, int buttonIndex, List<long> itemInstanceIdList) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            tradeServiceServer.RequestAddItemsToTradeSlot(accountId, buttonIndex, itemInstanceIdList);
        }

        public void AdvertiseAddItemsToTargetTradeSlot(int targetAccountId, int buttonIndex, List<long> itemInstanceIdList) {
            networkController.AdvertiseAddItemsToTargetTradeSlot(targetAccountId, buttonIndex, itemInstanceIdList);
        }

        public void RequestAddCurrencyToTrade(int clientId, int amount) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            tradeServiceServer.RequestAddCurrencyToTrade(accountId, amount);
        }

        public void AdvertiseAddCurrencyToTrade(int targetAccountId, int amount) {
            networkController.AdvertiseAddCurrencyToTrade(targetAccountId, amount);
        }

        public void RequestCancelTrade(int clientId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            tradeServiceServer.RequestCancelTrade(accountId);
        }

        public void RequestConfirmTrade(int clientId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            tradeServiceServer.RequestConfirmTrade(accountId);
        }

        public void RequestUnconfirmTrade(int clientId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            tradeServiceServer.RequestUnconfirmTrade(accountId);
        }

        public void AdvertiseCompleteTrade(int accountId) {
            networkController.AdvertiseCompleteTrade(accountId);
        }

        public void AdvertiseCancelTrade(int accountId) {
            networkController.AdvertiseCancelTrade(accountId);
        }

        public void AdvertiseMailMessages(int accountId, MailMessageListBundle mailMessageListResponse) {
            networkController.AdvertiseMailMessages(accountId, mailMessageListResponse);
        }

        public void RequestDeleteMailMessage(int clientId, int messageId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            mailboxManagerServer.RequestDeleteMailMessage(accountId, messageId);
        }

        public void RequestTakeMailAttachment(int clientId, int messageId, int attachmentSlotId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            mailboxManagerServer.RequestTakeMailAttachment(accountId, messageId, attachmentSlotId);
        }

        public void RequestTakeMailAttachments(int clientId, int messageId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            mailboxManagerServer.RequestTakeMailAttachments(accountId, messageId);
        }

        public void AdvertiseDeleteMailMessage(int accountId, int messageId) {
            networkController.AdvertiseDeleteMailMessage(accountId, messageId);
        }

        public void AdvertiseTakeMailAttachment(int accountId, int messageId, int attachmentSlotId) {
            networkController.AdvertiseTakeMailAttachment(accountId, messageId, attachmentSlotId);
        }

        public void AdvertiseTakeMailAttachments(int accountId, int messageId) {
            networkController.AdvertiseTakeMailAttachments(accountId, messageId);
        }

        public void AdvertiseConfirmationPopup(int accountId, string messageText) {
            networkController.AdvertiseConfirmationPopup(accountId, messageText);
        }

        public void AdvertiseMailSend(int accountId) {
            networkController.AdvertiseMailSend(accountId);
        }

        public void RequestMarkMailAsRead(int clientId, int messageId) {
            //Debug.Log($"NetworkManagerServer.RequestMarkMailAsRead(clientId: {clientId}, messageId: {messageId})");

            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            mailboxManagerServer.RequestMarkMailAsRead(accountId, messageId);
        }

        public void RequestBuyAuctionItem(int clientId, int auctionItemId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            auctionManagerServer.RequestBuyAuctionItem(accountId, auctionItemId);
        }

        public void RequestCancelAuction(int clientId, int auctionItemId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            auctionManagerServer.RequestCancelAuction(accountId, auctionItemId);
        }

        public void AdvertiseBuyAuctionItem(int accountId, int auctionItemId) {
            networkController.AdvertiseBuyAuctionItem(accountId, auctionItemId);
        }

        public void AdvertiseCancelAuction(int accountId, int auctionItemId) {
            networkController.AdvertiseCancelAuction(accountId, auctionItemId);
        }

        public void AdvertiseAuctionItems(int accountId, AuctionItemSearchListResult auctionItemListResponse) {
            networkController.AdvertiseAuctionItems(accountId, auctionItemListResponse);
        }

        public void AdvertiseListAuctionItems(int accountId) {
            networkController.AdvertiseListAuctionItems(accountId);
        }

        public void AdvertiseDisbandGuild(int accountId, int guildId) {
            networkController.AdvertiseDisbandGuild(accountId, guildId);
        }

        public void AdvertiseDeclineGuildInvite(int leaderAccountId, string playerName) {
            networkController.AdvertiseDeclineGuildInvite(leaderAccountId, playerName);
        }

        public void AdvertiseGuildInvite(int invitedCharacterId, int guildId, string leaderName) {
            networkController.AdvertiseGuildInvite(invitedCharacterId, guildId, leaderName);
        }

        public void AdvertiseGuild(int accountId, GuildNetworkData guildNetworkData) {
            //Debug.Log($"NetworkManagerServer.AdvertiseGuild(accountId: {accountId}, guildId: {guildNetworkData.GuildId})");

            networkController.AdvertiseGuild(accountId, guildNetworkData);
        }

        public void AdvertisePromoteGuildLeader(int accountId, int guildId, int newLeaderCharacterId) {
            networkController.AdvertisePromoteGuildLeader(accountId, guildId, newLeaderCharacterId);
        }

        public void AdvertiseRenameCharacterInGuild(int accountId, int guildId, int characterId, string newName) {
            networkController.AdvertiseRenameCharacterInGuild(accountId, guildId, characterId, newName);
        }

        public void AdvertiseGuildNameAvailable(int accountId) {
            networkController.AdvertiseGuildNameAvailable(accountId);
        }

        public void AdvertiseGroupMemberStatusChange(int accountId, int characterGroupId, int playerCharacterId, CharacterGroupMemberNetworkData characterGroupMemberNetworkData) {
            networkController.AdvertiseCharacterGroupMemberStatusChange(accountId, characterGroupId, playerCharacterId, characterGroupMemberNetworkData);
        }

        public void AdvertiseGuildMemberStatusChange(int accountId, int guildId, int playerCharacterId, GuildMemberNetworkData guildMemberNetworkData) {
            networkController.AdvertiseGuildMemberStatusChange(accountId, guildId, playerCharacterId, guildMemberNetworkData);
        }

        public void AdvertiseAddFriend(int sourceCharacterAccountId, CharacterSummaryNetworkData characterSummaryNetworkData) {
            networkController.AdvertiseAddFriend(sourceCharacterAccountId, characterSummaryNetworkData);
        }

        public void AdvertiseRemoveCharacterFromFriendList(int targetCharacterAccountId, int sourceCharacterId) {
            networkController.AdvertiseRemoveCharacterFromFriendList(targetCharacterAccountId, sourceCharacterId);
        }

        public void AdvertiseDeclineFriendInvite(int friendAccountId, string characterName) {
            networkController.AdvertiseDeclineFriendInvite(friendAccountId, characterName);
        }

        public void AdvertiseFriendInvite(int invitedAccountId, int sourceCharacterId, string sourceCharacterName) {
            networkController.AdvertiseFriendInvite(invitedAccountId, sourceCharacterId, sourceCharacterName);
        }

        public void AdvertiseFriendList(int accountId, FriendListNetworkData friendListNetworkData) {
            networkController.AdvertiseFriendList(accountId, friendListNetworkData);
        }

        public void AdvertiseRenameCharacterInFriendList(int targetAccountId, int characterId, string newName) {
            networkController.AdvertiseRenameCharacterInFriendList(targetAccountId, characterId, newName);
        }

        public void AdvertiseFriendStateChange(int targetAccountId, int playerCharacterId, CharacterSummaryNetworkData characterSummaryNetworkData) {
            networkController.AdvertiseFriendStateChange(targetAccountId, playerCharacterId, characterSummaryNetworkData);
        }

        public void AcceptFriendInvite(int clientId, int friendId) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            friendServiceServer.AcceptFriendInvite(accountId, friendId);
        }

        public void AdvertisePlayerNameNotAvailable(int accountId) {
            networkController.AdvertisePlayerNameNotAvailable(accountId);
        }

        public void AdvertiseLobbyLogin(int accountId, string username) {
            networkController.AdvertiseLobbyLogin(accountId, username);
        }

        public void AdvertiseLobbyLogout(int accountId) {
            networkController.AdvertiseLobbyLogout(accountId);
        }

        public void RemoveLobbyGamePlayer(int accountId) {
            if (lobbyGameAccountLookup.ContainsKey(accountId) == true) {
                int gameId = lobbyGameAccountLookup[accountId];
                LeaveLobbyGame(gameId, accountId);
            }
        }

        public int GetClientIDForAccount(int accountId) {
            if (authenticationService.LoggedInAccounts.ContainsKey(accountId) == false) {
                //Debug.Log($"FishNetClientConnector.AdvertiseRequestBeginTrade() could not find account id {accountId}");
                return -1;
            }
            return authenticationService.LoggedInAccounts[accountId].clientId;

        }

        public void LogoutByClientId(int clientId) {
            //Debug.Log($"NetworkManagerServer.LogoutByClientId(clientId: {clientId})");

            authenticationService.LogoutByClientId(clientId);
        }

        public void RequestSpawnPlayerUnit(int clientId, string sceneName) {
            int accountId = authenticationService.GetAccountId(clientId);
            if (accountId == -1) {
                return;
            }
            if (serverMode == NetworkServerMode.Lobby) {
                if (LobbyGameAccountLookup.ContainsKey(accountId)) {
                    RequestSpawnLobbyGamePlayer(accountId, LobbyGameAccountLookup[accountId], sceneName);
                }
            } else if (serverMode == NetworkServerMode.MMO) {
                RequestSpawnPlayer(accountId, sceneName);
            }
        }

        public void AdvertiseLoadCharacterList(int accountId, List<PlayerCharacterSaveData> playerCharacterSaveDataList) {
            networkController.AdvertiseLoadCharacterList(accountId, playerCharacterSaveDataList);
        }

        public void SetSceneLoadRequestHashCode(SceneInstanceType sceneInstanceType, int hashCode) {
            levelManagerServer.SetSceneLoadRequestHashCode(sceneInstanceType, hashCode);
        }

        public void UnloadScene(int handle) {
            networkController.UnloadScene(handle);
        }

        internal void SetSceneClientCount(string name, int handle, int clientCount) {
            levelManagerServer.SetSceneClientCount(name, handle, clientCount);
        }
    }

}