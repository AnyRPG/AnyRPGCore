using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    public class NetworkManagerServer : ConfiguredMonoBehaviour {

        public event Action<int, int, bool, bool> OnAuthenticationResult = delegate { };
        public event Action<int> OnAccountLogin = delegate { };
        public event Action<int> OnAccountLogout = delegate { };
        public event Action<LobbyGame> OnCreateLobbyGame = delegate { };
        public event Action<int> OnCancelLobbyGame = delegate { };
        public event Action<int, int, string> OnJoinLobbyGame = delegate { };
        public event Action<int> OnStartLobbyGame = delegate { };
        public event Action<int, int> OnLeaveLobbyGame = delegate { };
        public event Action OnStartServer = delegate { };
        public event Action OnStopServer = delegate { };

        [SerializeField]
        private NetworkController networkController = null;

        // jwt for each client so the server can make API calls to the api server on their behalf
        //private Dictionary<int, string> clientTokens = new Dictionary<int, string>();

        // cached list of player character save data from client lookups used for loading games
        /// <summary>
        /// accountId, playerCharacterId, playerCharacterSaveData
        /// </summary>
        private Dictionary<int, Dictionary<int, CharacterSaveData>> playerCharacterDataDict = new Dictionary<int, Dictionary<int, CharacterSaveData>>();

        /// <summary>
        /// clientId, loggedInAccount
        /// </summary>
        private Dictionary<int, LoggedInAccount> loggedInAccountsByClient = new Dictionary<int, LoggedInAccount>();

        /// <summary>
        /// accountId, loggedInAccount
        /// </summary>
        private Dictionary<int, LoggedInAccount> loggedInAccounts = new Dictionary<int, LoggedInAccount>();


        /// <summary>
        /// clientId, username
        /// </summary>
        private Dictionary<int, string> loginRequests = new Dictionary<int, string>();

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


        private int lobbyGameCounter = 0;
        private int maxLobbyChatTextSize = 64000;
        private ushort port = 7770;

        // lobby chat
        private string lobbyChatText = string.Empty;
        private Dictionary<int, string> lobbyGameChatText = new Dictionary<int, string>();


        private GameServerClient gameServerClient = null;
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
        private NewGameManager newGameManager = null;
        private CharacterGroupServiceServer characterGroupServiceServer = null;
        private TradeServiceServer tradeServiceServer = null;
        private MailboxManagerServer mailboxManagerServer = null;
        private MailService mailService = null;
        private AuctionManagerServer auctionManagerServer = null;
        private AuctionService auctionService = null;

        public bool ServerModeActive { get => serverModeActive; }
        public NetworkServerMode ServerMode { get => serverMode; }
        public Dictionary<int, LoggedInAccount> LoggedInAccounts { get => loggedInAccounts; }
        public Dictionary<int, LoggedInAccount> LoggedInAccountsByClient { get => loggedInAccountsByClient; }
        public Dictionary<int, LobbyGame> LobbyGames { get => lobbyGames; }
        public Dictionary<int, Dictionary<string, int>> LobbyGameSceneHandles { get => lobbyGameSceneHandles; }
        public Dictionary<int, Dictionary<string, int>> CharacterGroupSceneHandles { get => characterGroupSceneHandles; }
        public Dictionary<int, int> LobbyGameAccountLookup { get => lobbyGameAccountLookup; set => lobbyGameAccountLookup = value; }
        public NetworkController NetworkController { get => networkController; set => networkController = value; }

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
            newGameManager = systemGameManager.NewGameManager;
            characterGroupServiceServer = systemGameManager.CharacterGroupServiceServer;
            tradeServiceServer = systemGameManager.TradeServiceServer;
            mailboxManagerServer = systemGameManager.MailboxManagerServer;
            mailService = systemGameManager.MailService;
            auctionManagerServer = systemGameManager.AuctionManagerServer;
            auctionService = systemGameManager.AuctionService;
        }

        public void AddLoggedInAccount(int clientId, int accountId, string token) {
            //Debug.Log($"NetworkManagerServer.AddLoggedInAccount({clientId}, {accountId}, {token})");

            if (loginRequests.ContainsKey(clientId)) {
                if (loggedInAccounts.ContainsKey(accountId)) {
                    //Debug.Log($"NetworkManagerServer.AddLoggedInAccount({clientId}, {accountId}, {token}) : updating existing object");
                    int oldClientId = loggedInAccounts[accountId].clientId;
                    loggedInAccounts[accountId].clientId = clientId;
                    loggedInAccounts[accountId].token = token;
                    loggedInAccounts[accountId].ipAddress = GetClientIPAddress(clientId);
                    loggedInAccounts[accountId].disconnected = false;
                    loggedInAccountsByClient.Remove(oldClientId);
                    loggedInAccountsByClient.Add(clientId, loggedInAccounts[accountId]);
                } else {
                    LoggedInAccount loggedInAccount = new LoggedInAccount(clientId, accountId, loginRequests[clientId], token, GetClientIPAddress(clientId));
                    loggedInAccounts.Add(accountId, loggedInAccount);
                    loggedInAccountsByClient.Add(clientId, loggedInAccount);
                }
            }
        }

        public void OnSetGameMode(GameMode gameMode) {
            //Debug.Log($"NetworkManagerServer.OnSetGameMode({gameMode})");
            
            if (gameMode == GameMode.Network) {
                // create instance of GameServerClient
                gameServerClient = new GameServerClient(systemGameManager, systemConfigurationManager.ApiServerAddress);
                return;
            }

        }

        public void SavePlayerCharacter(PlayerCharacterMonitor playerCharacterMonitor) {
            //Debug.Log($"NetworkManagerServer.SavePlayerCharacter()");

            if (playerCharacterMonitor.unitController != null) {
                playerCharacterMonitor.SavePlayerLocation();
            }
            if (playerCharacterMonitor.saveDataDirty == true) {
                if (serverMode == NetworkServerMode.MMO) {
                    if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                        if (loggedInAccounts.ContainsKey(playerCharacterMonitor.accountId) == false) {
                            // can't do anything without a token
                            return;
                        }
                        gameServerClient.SavePlayerCharacter(
                            playerCharacterMonitor.accountId,
                            loggedInAccounts[playerCharacterMonitor.accountId].token,
                            playerCharacterMonitor.characterSaveData.CharacterId,
                            playerCharacterMonitor.characterSaveData);
                    } else if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                        playerCharacterService.SavePlayerCharacter(playerCharacterMonitor.accountId, playerCharacterMonitor.characterSaveData);
                    }
                }
            }
        }

        public void GetLoginToken(int clientId, string username, string password) {
            //Debug.Log($"NetworkManagerServer.GetLoginToken({clientId}, {username}, {password})");

            loginRequests.Add(clientId, username);
            if (serverMode == NetworkServerMode.MMO) {
                if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                    gameServerClient.Login(clientId, username, password);
                } else if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                    LocalLogin(clientId, username, password);
                }
            } else {
                LocalLogin(clientId, username, password);
            }
        }

        public void LocalLogin(int clientId, string username, string password) {
            //Debug.Log($"NetworkManagerServer.LobbyLogin({clientId}, {username}, {password})");
            authenticationService.LoginOrCreateAccount(clientId, username, password);
        }

        public void ProcessLoginResponse(int clientId, int accountId, bool correctPassword, string token) {
            //Debug.Log($"NetworkManagerServer.ProcessLoginResponse({clientId}, {accountId}, {correctPassword}, {token})");

            SpawnPlayerRequest spawnPlayerRequest = null;
            if (correctPassword == true) {
                if (loggedInAccounts.ContainsKey(accountId) && loggedInAccounts[accountId].disconnected == false) {
                    if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId)) {
                        // if the player is already logged in, we need to add a spawn request to match the current position and direction of the player
                        spawnPlayerRequest = new SpawnPlayerRequest() {
                            overrideSpawnDirection = true,
                            spawnForwardDirection = playerManagerServer.ActiveUnitControllers[accountId].transform.forward,
                            overrideSpawnLocation = true,
                            spawnLocation = playerManagerServer.ActiveUnitControllers[accountId].transform.position
                        };
                    }
                    // if the account is already logged in, kick the old client
                    playerManagerServer.DespawnPlayerUnit(accountId);
                    KickPlayer(accountId);
                } else if (playerManagerServer.PlayerCharacterMonitors.ContainsKey(accountId)) {
                    //Debug.Log($"NetworkManagerServer.ProcessLoginResponse({clientId}, {accountId}, {correctPassword}, {token}) account was disconnected, using last position");
                    // if the account is disconnected but was already logged in, add a spawn request to match the saved position and direction of the player
                    CharacterSaveData saveData = playerManagerServer.PlayerCharacterMonitors[accountId].characterSaveData;
                    spawnPlayerRequest = new SpawnPlayerRequest() {
                        overrideSpawnDirection = true,
                        spawnForwardDirection = new Vector3(saveData.PlayerRotationX, saveData.PlayerRotationY, saveData.PlayerRotationZ),
                        overrideSpawnLocation = true,
                        spawnLocation = new Vector3(saveData.PlayerLocationX, saveData.PlayerLocationY, saveData.PlayerLocationZ)
                    };
                }
                if (spawnPlayerRequest != null) {
                    playerManagerServer.AddSpawnRequest(accountId, spawnPlayerRequest, false);
                }
                AddLoggedInAccount(clientId, accountId, token);
            }
            loginRequests.Remove(clientId);
            OnAuthenticationResult(clientId, accountId, true, correctPassword);
            
            if (correctPassword == false) {
                return;
            }
            //if (spawnPlayerRequest != null) {
            //}

            OnAccountLogin(accountId);
            if (serverMode == NetworkServerMode.Lobby) {
                networkController.AdvertiseLobbyLogin(accountId, loggedInAccounts[accountId].username);
            }
        }

        public void RequestCreatePlayerCharacter(int accountId, CharacterSaveData requestedSaveData) {
            //Debug.Log($"NetworkManagerServer.CreatePlayerCharacter(AnyRPGSaveData)");

            if (loggedInAccounts.ContainsKey(accountId) == false) {
                // can't do anything without a token
                return;
            }
            CharacterSaveData characterSaveData = newGameManager.CreateNewPlayerSaveData(requestedSaveData);

            // create save data from parameters
            if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                gameServerClient.CreatePlayerCharacter(accountId, loggedInAccounts[accountId].token, characterSaveData);
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                
                if (playerCharacterService.AddPlayerCharacter(accountId, characterSaveData)) {
                    ProcessCreatePlayerCharacterResponse(accountId);
                } else {
                    networkController.AdvertisePlayerNameNotAvailable(accountId);
                }
            }
        }

        public void ProcessCreatePlayerCharacterResponse(int accountId) {
            //Debug.Log($"NetworkManagerServer.ProcessCreatePlayerCharacterResponse({accountId})");

            //networkController.AdvertiseCreatePlayerCharacter(accountId);
            LoadCharacterList(accountId);
        }


        public void DeletePlayerCharacter(int accountId, int playerCharacterId) {
            //Debug.Log($"NetworkManagerServer.DeletePlayerCharacter({playerCharacterId})");

            if (loggedInAccounts.ContainsKey(accountId) == false) {
                // can't do anything without a token
                return;
            }
            if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                playerCharacterService.DeletePlayerCharacter(accountId, playerCharacterId);
                ProcessDeletePlayerCharacterResponse(accountId);
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                gameServerClient.DeletePlayerCharacter(accountId, loggedInAccounts[accountId].token, playerCharacterId);
            }
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

        public void ProcessDeletePlayerCharacterResponse(int accountId) {
            //Debug.Log($"NetworkManagerServer.ProcessDeletePlayerCharacterResponse({accountId})");

            networkController.AdvertiseDeletePlayerCharacter(accountId);
        }

        public void LoadCharacterList(int accountId) {
            //Debug.Log($"NetworkManagerServer.LoadCharacterList({accountId})");

            if (loggedInAccounts.ContainsKey(accountId) == false) {
                // can't do anything without a token
                //return new List<PlayerCharacterSaveData>();
                return;
            }
            if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                PlayerCharacterListResponse playerCharacterListResponse = playerCharacterService.GetPlayerCharacters(accountId);
                ProcessLoadCharacterListResponse(accountId, playerCharacterListResponse.playerCharacters);
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                gameServerClient.LoadCharacterList(accountId, loggedInAccounts[accountId].token);
            }
        }

        public void ProcessLoadCharacterListResponse(int accountId, List<PlayerCharacterData> playerCharacters) {
            //Debug.Log($"NetworkManagerServer.ProcessLoadCharacterListResponse({accountId})");

            List<PlayerCharacterSaveData> playerCharacterSaveDataList = new List<PlayerCharacterSaveData>();
            foreach (PlayerCharacterData playerCharacterData in playerCharacters) {
                PlayerCharacterSaveData playerCharacterSaveData = new PlayerCharacterSaveData(saveManager.LoadCharacterSaveDataFromString(playerCharacterData.saveData), systemItemManager);
                playerCharacterSaveDataList.Add(playerCharacterSaveData);
            }
            Dictionary<int, CharacterSaveData> playerCharacterSaveDataDict = new Dictionary<int, CharacterSaveData>();
            foreach (PlayerCharacterSaveData playerCharacterSaveData in playerCharacterSaveDataList) {
                playerCharacterSaveDataDict.Add(playerCharacterSaveData.CharacterSaveData.CharacterId, playerCharacterSaveData.CharacterSaveData);
            }
            if (playerCharacterDataDict.ContainsKey(accountId)) {
                playerCharacterDataDict[accountId] = playerCharacterSaveDataDict;
            } else {
                playerCharacterDataDict.Add(accountId, playerCharacterSaveDataDict);
            }

            networkController.AdvertiseLoadCharacterList(accountId, playerCharacterSaveDataList);
        }

        public CharacterSaveData GetPlayerCharacterSaveData(int accountId, int playerCharacterId) {
            if (playerCharacterDataDict.ContainsKey(accountId) == false) {
                return null;
            }
            if (playerCharacterDataDict[accountId].ContainsKey(playerCharacterId) == false) {
                return null;
            }
            return playerCharacterDataDict[accountId][playerCharacterId];
        }

        public string GetAccountToken(int accountId) {
            //Debug.Log($"NetworkManagerServer.GetClientToken({accountId})");

            if (loggedInAccounts.ContainsKey(accountId)) {
                return loggedInAccounts[accountId].token;
            }
            return string.Empty;
        }

        public void ProcessClientDisconnect(int clientId) {
            //Debug.Log($"NetworkManagerServer.ProcessClientDisconnect({clientId})");

            if (loggedInAccountsByClient.ContainsKey(clientId) == false) {
                return;
            }
            int accountId = loggedInAccountsByClient[clientId].accountId;
            // don't do this - it will remove them from the lobby game
            //ProcessClientLogout(accountId);
            loggedInAccounts[accountId].disconnected = true;

            playerManagerServer.ProcessDisconnect(accountId);
        }

        public void ProcessClientLogout(int accountId) {
            //Debug.Log($"NetworkManagerServer.ProcessClientLogout({accountId})");

            if (loggedInAccounts.ContainsKey(accountId) == false) {
                return;
            }

            // remove the player from any lobby games
            int clientId = loggedInAccounts[accountId].clientId;
            if (lobbyGameAccountLookup.ContainsKey(accountId) == true) {
                int gameId = lobbyGameAccountLookup[accountId];
                LeaveLobbyGame(gameId, accountId);
            }

            loggedInAccounts.Remove(accountId);
            loggedInAccountsByClient.Remove(clientId);

            OnAccountLogout(accountId);
            if (serverMode == NetworkServerMode.Lobby) {
                networkController?.AdvertiseLobbyLogout(accountId);
            }
        }

        public void ActivateServerMode() {
            //Debug.Log($"NetworkManagerServer.ActivateServerMode()");

            serverModeActive = true;
            // ordered dependencies
            // load items first
            systemItemManager.ProcessStartServer();
            // load things that depend on items
            mailService.ProcessStartServer();
            auctionService.ProcessStartServer();

            // unordered dependencies
            OnStartServer();
            systemEventManager.OnChooseWeather += HandleChooseWeather;
            systemEventManager.OnStartWeather += HandleStartWeather;
            systemEventManager.OnEndWeather += HandleEndWeather;

        }

        public void DeactivateServerMode() {
            //Debug.Log($"NetworkManagerServer.DeactivateServerMode()");

            serverModeActive = false;

            loggedInAccountsByClient.Clear();
            lobbyGames.Clear();
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
            systemEventManager.NotifyOnBeforeStopServer();
            
            // logout all logged in accounts
            List<int> loggedInAccountIds = new List<int>(loggedInAccounts.Keys);
            foreach (int accountId in loggedInAccountIds) {
                Logout(accountId);
            }

            networkController?.StopServer();
        }

        public void KickPlayer(int accountId) {
            //Debug.Log($"NetworkManagerServer.KickPlayer({accountId})");

            networkController?.KickPlayer(accountId);
        }

        public string GetClientIPAddress(int clientId) {
            return networkController?.GetClientIPAddress(clientId);
        }

        public void CreateLobbyGame(string sceneResourceName, int accountId, bool allowLateJoin) {
            
            LobbyGame lobbyGame = new LobbyGame(accountId, lobbyGameCounter, sceneResourceName, loggedInAccounts[accountId].username, allowLateJoin);
            lobbyGameCounter++;
            lobbyGames.Add(lobbyGame.gameId, lobbyGame);
            lobbyGameAccountLookup.Add(accountId, lobbyGame.gameId);
            lobbyGameChatText.Add(lobbyGame.gameId, string.Empty);
            OnCreateLobbyGame(lobbyGame);
            networkController.AdvertiseCreateLobbyGame(lobbyGame);
        }

        public void CancelLobbyGame(int accountId, int gameId) {
            if (lobbyGames.ContainsKey(gameId) == false || lobbyGames[gameId].leaderAccountId != accountId) {
                // game not found, or requesting client is not leader
                return;
            }
            foreach (int accountIdInGame in lobbyGames[gameId].PlayerList.Keys) {
                lobbyGameAccountLookup.Remove(accountIdInGame);
            }
            lobbyGames.Remove(gameId);
            lobbyGameChatText.Remove(gameId);
            OnCancelLobbyGame(gameId);
            networkController.AdvertiseCancelLobbyGame(gameId);
        }

        public void JoinLobbyGame(int gameId, int accountId) {
            if (lobbyGames.ContainsKey(gameId) == false || loggedInAccounts.ContainsKey(accountId) == false) {
                // game or client doesn't exist
                return;
            }
            lobbyGames[gameId].AddPlayer(accountId, loggedInAccounts[accountId].username);
            lobbyGameAccountLookup.Add(accountId, gameId);
            OnJoinLobbyGame(gameId, accountId, loggedInAccounts[accountId].username);
            networkController.AdvertiseAccountJoinLobbyGame(gameId, accountId, loggedInAccounts[accountId].username);
        }

        public void RequestLobbyGameList(int accountId) {
            networkController.SetLobbyGameList(accountId, lobbyGames.Values.ToList<LobbyGame>());
        }

        public void RequestLobbyPlayerList(int accountId) {
            Dictionary<int, string> lobbyPlayerList = new Dictionary<int, string>();
            foreach (int loggedInClientId in loggedInAccountsByClient.Keys) {
                lobbyPlayerList.Add(loggedInClientId, loggedInAccountsByClient[loggedInClientId].username);
            }
            networkController.SetLobbyPlayerList(accountId, lobbyPlayerList);
        }

        public void ChooseLobbyGameCharacter(int gameId, int accountId, string unitProfileName, string appearanceString, List<SwappableMeshSaveData> swappableMeshSaveData) {
            //Debug.Log($"NetworkManagerServer.ChooseLobbyGameCharacter({gameId}, {accountId}, {unitProfileName})");

            if (lobbyGames.ContainsKey(gameId) == false || loggedInAccounts.ContainsKey(accountId) == false) {
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

        public void ToggleLobbyGameReadyStatus(int gameId, int accountId) {
            //Debug.Log($"NetworkManagerClient.ToggleLobbyGameReadyStatus({gameId}, {accountId})");

            if (lobbyGames.ContainsKey(gameId) == false || lobbyGames[gameId].PlayerList.ContainsKey(accountId) == false) {
                // game did not exist or client was not in game
                return;
            }

            lobbyGames[gameId].PlayerList[accountId].ready = !lobbyGames[gameId].PlayerList[accountId].ready;
            networkController.AdvertiseSetLobbyGameReadyStatus(gameId, accountId, lobbyGames[gameId].PlayerList[accountId].ready);
        }

        public void RequestStartLobbyGame(int gameId, int accountId) {
            if (lobbyGames.ContainsKey(gameId) == false || lobbyGames[gameId].leaderAccountId != accountId || lobbyGames[gameId].inProgress == true) {
                // game did not exist, non leader tried to start, or already in progress, nothing to do
                return;
            }
            StartLobbyGame(gameId);
        }

        public void RequestJoinLobbyGameInProgress(int gameId, int accountId) {
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

        public void LeaveLobbyGame(int gameId, int accountId) {
            if (lobbyGames.ContainsKey(gameId) == false || loggedInAccounts.ContainsKey(accountId) == false) {
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

        public void SendLobbyChatMessage(string messageText, int accountId) {
            if (loggedInAccounts.ContainsKey(accountId) == false) {
                return;
            }
            string addedText = $"{loggedInAccounts[accountId].username}: {messageText}\n";
            lobbyChatText += addedText;
            lobbyChatText = ShortenStringOnNewline(lobbyChatText, maxLobbyChatTextSize);

            networkController.AdvertiseSendLobbyChatMessage(addedText);
        }

        public void SendLobbyGameChatMessage(string messageText, int accountId, int gameId) {
            if (loggedInAccountsByClient.ContainsKey(accountId) == false) {
                return;
            }
            if (lobbyGames.ContainsKey(gameId) == false || lobbyGames[gameId].PlayerList.ContainsKey(accountId) == false) {
                return;
            }
            string addedText = $"{loggedInAccounts[accountId].username}: {messageText}\n";
            lobbyGameChatText[gameId] += addedText;
            lobbyGameChatText[gameId] = ShortenStringOnNewline(lobbyGameChatText[gameId], maxLobbyChatTextSize);

            networkController.AdvertiseSendLobbyGameChatMessage(addedText, gameId);
        }

        public void SendSceneChatMessage(string messageText, int accountId) {
            if (loggedInAccounts.ContainsKey(accountId) == false) {
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
            networkController.AdvertiseLoadScene(sceneName, accountId);
        }

        public void AdvertiseLoadCutscene(Cutscene cutscene, int accountId) {
            networkController.AdvertiseLoadCutscene(cutscene, accountId);
        }

        public void DespawnPlayerUnit(int accountId) {
            //Debug.Log($"NetworkManagerServer.DespawnPlayerUnit({accountId})");

            playerManagerServer.DespawnPlayerUnit(accountId);
        }

        public void RequestDespawnPlayerUnit(int accountId) {
            //Debug.Log($"NetworkManagerServer.RequestDespawnPlayerUnit({accountId})");

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
            networkController.AdvertiseLoadScene(teleportEffectProperties.LevelName, accountId);
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
            levelManagerServer.AddLoadedScene(scene);
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
            //levelManagerServer.RemoveLoadedScene(sceneHandle, sceneName);
        }

        public UnitController SpawnCharacterPrefab(CharacterRequestData characterRequestData, Transform parentTransform, Vector3 position, Vector3 forward, Scene scene) {
            return networkController.SpawnCharacterPrefab(characterRequestData, parentTransform, position, forward, scene);
        }

        public GameObject SpawnModelPrefab(GameObject spawnPrefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            //Debug.Log($"NetworkManagerServer.SpawnModelPrefab({spawnRequestId})");

            return networkController.SpawnModelPrefabServer(spawnPrefab, parentTransform, position, forward);
        }

        public void TurnInDialog(Interactable interactable, int componentIndex, Dialog dialog, int accountId) {
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }
            dialogManagerServer.TurnInDialog(playerManagerServer.ActiveUnitControllers[accountId], interactable, componentIndex, dialog);
        }

        public void TurnInQuestDialog(Dialog dialog, int accountId) {
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }
            playerManagerServer.ActiveUnitControllers[accountId].CharacterDialogManager.TurnInDialog(dialog);
        }


        public void SetPlayerCharacterClass(Interactable interactable, int componentIndex, int accountId) {
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }
            classChangeManagerServer.ChangeCharacterClass(playerManagerServer.ActiveUnitControllers[accountId], interactable, componentIndex);
        }

        public void SetPlayerCharacterSpecialization(Interactable interactable, int componentIndex, int accountId) {
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }
            specializationChangeManagerServer.ChangeCharacterSpecialization(playerManagerServer.ActiveUnitControllers[accountId], interactable, componentIndex);
        }

        public void SetPlayerFaction(Interactable interactable, int componentIndex, int accountId) {
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }
            factionChangeManagerServer.ChangeCharacterFaction(playerManagerServer.ActiveUnitControllers[accountId], interactable, componentIndex);
        }

        public void LearnSkill(Interactable interactable, int componentIndex, int skillId, int accountId) {
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }
            skillTrainerManagerServer.LearnSkill(playerManagerServer.ActiveUnitControllers[accountId], interactable, componentIndex, skillId);
        }

        public void RequestSendMail(Interactable interactable, int componentIndex, MailMessageRequest sendMailRequest, int accountId) {
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }
            mailboxManagerServer.RequestSendMail(playerManagerServer.ActiveUnitControllers[accountId], interactable, componentIndex, sendMailRequest);
        }

        public void RequestListAuctionItems(Interactable interactable, int componentIndex, ListAuctionItemRequest listAuctionItemRequest, int accountId) {
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }
            auctionManagerServer.RequestListAuctionItems(playerManagerServer.ActiveUnitControllers[accountId], interactable, componentIndex, listAuctionItemRequest);
        }

        public void RequestSearchAuctions(Interactable interactable, int componentIndex, string searchText, bool onlyShowOwnAuctions, int accountId) {
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }
            auctionManagerServer.RequestSearchAuctions(playerManagerServer.ActiveUnitControllers[accountId], interactable, componentIndex, searchText, onlyShowOwnAuctions);
        }


        public void AcceptQuest(Interactable interactable, int componentIndex, Quest quest, int accountId) {
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }

            questGiverManagerServer.AcceptQuest(interactable, componentIndex, playerManagerServer.ActiveUnitControllers[accountId], quest);
        }

        public void CompleteQuest(Interactable interactable, int componentIndex, Quest quest, QuestRewardChoices questRewardChoices, int accountId) {
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }

            questGiverManagerServer.CompleteQuest(interactable, componentIndex, playerManagerServer.ActiveUnitControllers[accountId], quest, questRewardChoices);
        }


        public void AdvertiseMessageFeedMessage(UnitController sourceUnitController, string message) {
            networkController.AdvertiseMessageFeedMessage(playerManagerServer.ActiveUnitControllerLookup[sourceUnitController], message);
        }

        public void AdvertiseSystemMessage(UnitController sourceUnitController, string message) {
            networkController.AdvertiseSystemMessage(playerManagerServer.ActiveUnitControllerLookup[sourceUnitController], message);
        }

        public void SellVendorItem(Interactable interactable, int componentIndex, int itemInstanceId, int accountId) {
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }
            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId) == false) {
                return;
            }
            vendorManagerServer.SellItemToVendor(playerManagerServer.ActiveUnitControllers[accountId], interactable, componentIndex, systemItemManager.InstantiatedItems[itemInstanceId]);
        }

        public void RequestSpawnUnit(Interactable interactable, int componentIndex, int unitLevel, int extraLevels, bool useDynamicLevel, UnitProfile unitProfile, UnitToughness unitToughness, int accountId) {
            //Debug.Log($"NetworkManagerServer.RequestSpawnUnit({interactable.gameObject.name}, {componentIndex}, {unitLevel}, {extraLevels}, {useDynamicLevel}, {unitProfile.ResourceName}, {unitToughness?.ResourceName}, {accountId})");

            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }
            unitSpawnManager.SpawnUnit(playerManagerServer.ActiveUnitControllers[accountId], interactable, componentIndex, unitLevel, extraLevels, useDynamicLevel, unitProfile, unitToughness);
        }


        public void AdvertiseAddToBuyBackCollection(UnitController sourceUnitController, Interactable interactable, int componentIndex, InstantiatedItem newInstantiatedItem) {
            networkController.AdvertiseAddToBuyBackCollection(sourceUnitController, playerManagerServer.ActiveUnitControllerLookup[sourceUnitController], interactable, componentIndex, newInstantiatedItem);
        }

        public void AdvertiseSellItemToPlayer(UnitController sourceUnitController, Interactable interactable, int componentIndex, int collectionIndex, int itemIndex, string resourceName, int remainingQuantity) {
            //Debug.Log($"NetworkManagerServer.AdvertiseSellItemToPlayer({sourceUnitController.gameObject.name}, {interactable.gameObject.name}, {componentIndex}, {collectionIndex}, {itemIndex}, {resourceName}, {remainingQuantity})");
            networkController.AdvertiseSellItemToPlayer(sourceUnitController, interactable, componentIndex, collectionIndex, itemIndex, resourceName, remainingQuantity);
        }

        public void BuyItemFromVendor(Interactable interactable, int componentIndex, int collectionIndex, int itemIndex, string resourceName, int accountId) {
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }
            vendorManagerServer.BuyItemFromVendor(playerManagerServer.ActiveUnitControllers[accountId], interactable, componentIndex, collectionIndex, itemIndex, resourceName, accountId);
        }

        public void TakeAllLoot(int accountId) {
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == true) {
                lootManager.TakeAllLootInternal(accountId, playerManagerServer.ActiveUnitControllers[accountId]);
            }
        }

        public void RequestTakeLoot(int lootDropId, int accountId) {
            //Debug.Log($"NetworkManagerServer.RequestTakeLoot({lootDropId}, {accountId})");

            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == true) {
                //lootManager.TakeLoot(accountId, lootDropId);
                if (lootManager.LootDropIndex.ContainsKey(lootDropId) == false) {
                    return;
                }
                lootManager.LootDropIndex[lootDropId].TakeLoot(playerManagerServer.ActiveUnitControllers[accountId]);
            }
        }

        public void RequestBeginCrafting(Recipe recipe, int craftAmount, int accountId) {
            //Debug.Log($"NetworkManagerServer.RequestBeginCrafting({recipe.DisplayName}, {craftAmount}, {accountId})");

            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == true) {
                craftingManager.BeginCrafting(playerManagerServer.ActiveUnitControllers[accountId], recipe, craftAmount);
            }
        }

        public void RequestCancelCrafting(int accountId) {
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == true) {
                craftingManager.CancelCrafting(playerManagerServer.ActiveUnitControllers[accountId]);
            }
        }


        public void AddAvailableDroppedLoot(int accountId, List<LootDrop> items) {
            //Debug.Log($"NetworkManagerServer.AddAvailableDroppedLoot({accountId}, count: {items.Count})");

            networkController.AddAvailableDroppedLoot(accountId, items);
        }

        public void AddLootDrop(int accountId, int lootDropId, int itemId) {
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

        public void RequestSpawnLobbyGamePlayer(int accountId, int gameId, string sceneName) {
            //Debug.Log($"NetworkManagerServer.RequestSpawnLobbyGamePlayer({accountId}, {gameId}, {sceneName})");

            playerManagerServer.RequestSpawnPlayerUnit(accountId, sceneName);
        }

        public void RequestSpawnPlayer(int accountId, string sceneName) {
            //Debug.Log($"NetworkManagerServer.RequestSpawnLobbyGamePlayer({accountId}, {gameId}, {sceneName})");

            playerManagerServer.RequestSpawnPlayerUnit(accountId, sceneName);
        }

        public void SpawnPlayer(int accountId, CharacterRequestData characterRequestData, Vector3 position, Vector3 forward, string sceneName) {
            
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

        public void RequestRespawnPlayerUnit(int accountId) {
            //Debug.Log($"NetworkManagerServer.RequestRespawnPlayerUnit({accountId})");
            
            playerManagerServer.RespawnPlayerUnit(accountId);
        }

        public void RequestRevivePlayerUnit(int accountId) {
            //Debug.Log($"NetworkManagerServer.RequestRevivePlayerUnit({accountId})");

            playerManagerServer.RevivePlayerUnit(accountId);
        }

        public void MonitorPlayerUnit(int accountId, UnitController unitController) {
            playerManagerServer.MonitorPlayerUnit(accountId, unitController);
        }

        public void RequestUpdatePlayerAppearance(int accountId, Interactable interactable, int componentIndex, string unitProfileName, string appearanceString, List<SwappableMeshSaveData> swappableMeshSaveData) {
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }

            characterAppearanceManagerServer.UpdatePlayerAppearance(playerManagerServer.ActiveUnitControllers[accountId], accountId, interactable, componentIndex, unitProfileName, appearanceString, swappableMeshSaveData);
        }

        public void RequestChangePlayerName(Interactable interactable, int componentIndex, string newName, int accountId) {
            if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId) == false) {
                return;
            }
            nameChangeManagerServer.SetPlayerName(playerManagerServer.ActiveUnitControllers[accountId], interactable, componentIndex, newName);
        }

        public void RequestSpawnPet(int accountId, UnitProfile unitProfile) {
            Debug.Log($"NetworkManagerServer.RequestSpawnPet({accountId}, {unitProfile.ResourceName})");

            playerManagerServer.RequestSpawnPet(accountId, unitProfile);
        }

        public void RequestDespawnPet(int accountId, UnitProfile unitProfile) {
            playerManagerServer.RequestDespawnPet(accountId, unitProfile);
        }

        public void AdvertiseAddSpawnRequest(int accountId, SpawnPlayerRequest loadSceneRequest) {
            //Debug.Log($"NetworkManagerServer.AdvertiseAddSpawnRequest({accountId})");

            networkController.AdvertiseAddSpawnRequest(accountId, loadSceneRequest);
        }



        public void Logout(int accountId) {
            //Debug.Log($"NetworkManagerServer.Logout({accountId})");

            // remove the player from any character groups
            int characterId = -1;
            if (playerManagerServer.PlayerCharacterMonitors.ContainsKey(accountId)) {
                characterId = playerManagerServer.PlayerCharacterMonitors[accountId].characterSaveData.CharacterId;
            }
            if (characterId != -1) {
                characterGroupServiceServer.RemoveCharacterFromGroup(characterId);
            }

            playerManagerServer.StopMonitoringPlayerUnit(accountId);
            KickPlayer(accountId);
            ProcessClientLogout(accountId);
        }

        public void RequestSpawnRequest(int accountId) {
            playerManagerServer.RequestSpawnRequest(accountId);
        }

        public DateTime GetServerStartTime() {
            return timeOfDayManagerServer.StartTime;
        }

        public WeatherProfile GetSceneWeatherProfile(int handle) {
            return weatherManagerServer.GetSceneWeatherProfile(handle);
        }

        public void ReturnFromCutscene(int accountId) {
            //Debug.Log($"NetworkManagerServer.ReturnFromCutscene({accountId})");

            if (playerManagerServer.PlayerCharacterMonitors.ContainsKey(accountId) == false) {
                // no spawn request, nothing to do
                return;
            }
            string sceneName = playerManagerServer.PlayerCharacterMonitors[accountId].characterSaveData.CurrentScene;
            networkController.AdvertiseLoadScene(sceneName, accountId);
        }

        public void SetServerPort(ushort port) {
            //Debug.Log($"NetworkManagerServer.SetServerPort({port})");

            this.port = port;
        }

        public void SetServerMode(NetworkServerMode networkServerMode) {
            //Debug.Log($"NetworkManagerServer.SetServerMode({networkServerMode})");

            this.serverMode = networkServerMode;
        }

        public void RequestLoadPlayerCharacter(int accountId, int playerCharacterId) {
            //Debug.Log($"NetworkManagerServer.RequestLoadPlayerCharacter(accountId: {accountId}, playerCharacterId: {playerCharacterId})");

            string sceneName = string.Empty;
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

            networkController.AdvertiseLoadPlayerCharacter(accountId, sceneName);
            mailService.SendMailMessages(playerCharacterId);
        }

        public void AcceptCharacterGroupInvite(int accountId, int characterGroupId) {
            //Debug.Log($"NetworkManagerServer.AcceptCharacterGroupInvite({accountId}, {characterGroupId})");

            characterGroupServiceServer.AcceptCharacterGroupInvite(accountId, characterGroupId);
        }

        public void DeclineCharacterGroupInvite(int accountId) {
            //Debug.Log($"NetworkManagerServer.DeclineCharacterGroupInvite({accountId})");

            characterGroupServiceServer.DeclineCharacterGroupInvite(accountId);
        }

        public void AdvertiseAddCharacterToGroup(int playerCharacterId, CharacterGroup characterGroup) {
            //Debug.Log($"NetworkManagerServer.AdvertiseAddCharacterToGroup({playerCharacterId}, {characterGroup.characterGroupId})");

            networkController.AdvertiseAddCharacterToGroup(playerCharacterId, characterGroup);
        }

        public void AdvertiseCharacterGroup(int accountId, CharacterGroup characterGroup) {
            //Debug.Log($"NetworkManagerServer.AdvertiseCharacterGroup(accountId: {accountId}, groupId: {characterGroup.characterGroupId})");

            networkController.AdvertiseCharacterGroup(accountId, characterGroup);
        }

        public void RequestLeaveCharacterGroup(int accountId) {
            characterGroupServiceServer.RequestLeaveCharacterGroup(accountId);
        }

        public void AdvertiseRemoveCharacterFromGroup(int characterId, CharacterGroup characterGroup) {
            //Debug.Log($"NetworkManagerServer.AdvertiseRemoveCharacterFromGroup({characterId}, {characterGroup.characterGroupId})");

            networkController.AdvertiseRemoveCharacterFromGroup(characterId, characterGroup);
        }

        public void RequestRemoveCharacterFromGroup(int accountId, int playerCharacterId) {
            characterGroupServiceServer.RequestRemoveCharacterFromGroup(accountId, playerCharacterId);
        }

        public void RequestInviteCharacterToGroup(int accountId, int invitedCharacterId) {
            //Debug.Log($"NetworkManagerServer.RequestInviteCharacterToGroup({accountId}, {invitedCharacterId})");

            characterGroupServiceServer.RequestInviteCharacterToGroup(accountId, invitedCharacterId);
        }

        public void AdvertiseCharacterGroupInvite(int invitedCharacterId, CharacterGroup characterGroup, string leaderName) {
            //Debug.Log($"NetworkManagerServer.AdvertiseCharacterGroupInvite({invitedCharacterId}, {characterGroup.characterGroupId}, {leaderName})");

            networkController.AdvertiseCharacterGroupInvite(invitedCharacterId, characterGroup, leaderName);
        }

        public void RequestDisbandCharacterGroup(int accountId, int characterGroupId) {
            characterGroupServiceServer.DisbandGroup(accountId, characterGroupId);
        }

        public void AdvertiseDisbandCharacterGroup(CharacterGroup characterGroup) {
            networkController.AdvertiseDisbandCharacterGroup(characterGroup);
        }

        public void AdvertiseDeclineCharacterGroupInvite(int leaderAccountId, string decliningPlayerName) {
            networkController.AdvertiseDeclineCharacterGroupInvite(leaderAccountId, decliningPlayerName);
        }

        public void AdvertisePromoteLeader(CharacterGroup characterGroup, int newLeaderCharacterId) {
            networkController.AdvertisePromoteGroupLeader(characterGroup, newLeaderCharacterId);
        }

        public void RequestPromoteCharacterToLeader(int accountId, int characterId) {
            characterGroupServiceServer.RequestPromoteCharacterToLeader(accountId, characterId);
        }

        public void AdvertiseRenameCharacterInGroup(CharacterGroup characterGroup, int characterId, string newName) {
            networkController.AdvertiseRenameCharacterInGroup(characterGroup, characterId, newName);
        }

        public void AdvertiseGroupMessage(CharacterGroup characterGroup, string messageText) {
            networkController.AdvertiseGroupMessage(characterGroup, messageText);
        }

        public void AdvertisePrivateMessage(int targetAccountId, string messageText) {
            //Debug.Log($"NetworkManagerServer.AdvertisePrivateMessage({targetAccountId}, {messageText})");

            networkController.AdvertisePrivateMessage(targetAccountId, messageText);
        }

        public void RequestBeginTrade(int accountId, int targetCharacterId) {
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

        public void RequestDeclineTrade(int accountId) {
            tradeServiceServer.DeclineTradeInvite(accountId);
        }

        public void RequestAcceptTrade(int accountId) {
            tradeServiceServer.AcceptTradeInvite(accountId);
        }

        public void RequestAddItemsToTradeSlot(int accountId, int buttonIndex, List<int> itemIdList) {
            tradeServiceServer.RequestAddItemsToTradeSlot(accountId, buttonIndex, itemIdList);
        }

        public void AdvertiseAddItemsToTargetTradeSlot(int targetAccountId, int buttonIndex, List<int> itemIdList) {
            networkController.AdvertiseAddItemsToTargetTradeSlot(targetAccountId, buttonIndex, itemIdList);
        }

        public void RequestAddCurrencyToTrade(int accountId, int amount) {
            tradeServiceServer.RequestAddCurrencyToTrade(accountId, amount);
        }

        public void AdvertiseAddCurrencyToTrade(int targetAccountId, int amount) {
            networkController.AdvertiseAddCurrencyToTrade(targetAccountId, amount);
        }

        public void RequestCancelTrade(int accountId) {
            tradeServiceServer.RequestCancelTrade(accountId);
        }

        public void RequestConfirmTrade(int accountId) {
            tradeServiceServer.RequestConfirmTrade(accountId);
        }

        public void RequestUnconfirmTrade(int accountId) {
            tradeServiceServer.RequestUnconfirmTrade(accountId);
        }

        public void AdvertiseCompleteTrade(int accountId) {
            networkController.AdvertiseCompleteTrade(accountId);
        }

        public void AdvertiseCancelTrade(int accountId) {
            networkController.AdvertiseCancelTrade(accountId);
        }

        public void AdvertiseMailMessages(int accountId, MailMessageListResponse mailMessageListResponse) {
            networkController.AdvertiseMailMessages(accountId, mailMessageListResponse);
        }

        public void RequestDeleteMailMessage(int accountId, int messageId) {
            mailboxManagerServer.RequestDeleteMailMessage(accountId, messageId);
        }

        public void RequestTakeMailAttachment(int accountId, int messageId, int attachmentSlotId) {
            mailboxManagerServer.RequestTakeMailAttachment(accountId, messageId, attachmentSlotId);
        }

        public void RequestTakeMailAttachments(int accountId, int messageId) {
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

        public void RequestMarkMailAsRead(int accountId, int messageId) {
            mailboxManagerServer.RequestMarkMailAsRead(accountId, messageId);
        }

        public void RequestBuyAuctionItem(int accountId, int auctionItemId) {
            auctionManagerServer.RequestBuyAuctionItem(accountId, auctionItemId);
        }

        public void RequestCancelAuction(int accountId, int auctionItemId) {
            auctionManagerServer.RequestCancelAuction(accountId, auctionItemId);
        }

        public void AdvertiseBuyAuctionItem(int accountId, int auctionItemId) {
            networkController.AdvertiseBuyAuctionItem(accountId, auctionItemId);
        }

        public void AdvertiseCancelAuction(int accountId, int auctionItemId) {
            networkController.AdvertiseCancelAuction(accountId, auctionItemId);
        }

        public void AdvertiseAuctionItems(int accountId, AuctionItemListResponse auctionItemListResponse) {
            networkController.AdvertiseAuctionItems(accountId, auctionItemListResponse);
        }

        public void AdvertiseListAuctionItems(int accountId) {
            networkController.AdvertiseListAuctionItems(accountId);
        }
    }

}