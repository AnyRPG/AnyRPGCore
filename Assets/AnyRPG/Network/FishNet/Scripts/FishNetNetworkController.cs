using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Managing.Server;
using FishNet.Object;
using FishNet.Transporting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    public class FishNetNetworkController : NetworkController {

        private FishNet.Managing.NetworkManager fishNetNetworkManager;
        private FishNetClientConnector clientConnector;
        
        [SerializeField]
        private GameObject networkConnectorSpawnPrefab = null;
        
        //private GameObject networkConnectorSpawnReference = null;

        /// <summary>
        /// Current state of client socket.
        /// </summary>
        private LocalConnectionState clientState = LocalConnectionState.Stopped;

        /// <summary>
        /// Current state of server socket.
        /// </summary>
        private LocalConnectionState serverState = LocalConnectionState.Stopped;

        // game manager references
        private LevelManager levelManager = null;
        private NetworkManagerClient networkManagerClient = null;
        private NetworkManagerServer networkManagerServer = null;

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log("FishNetNetworkController.Configure()");

            base.Configure(systemGameManager);
            fishNetNetworkManager = InstanceFinder.NetworkManager;
            if (fishNetNetworkManager != null) {
                //Debug.Log("FishNetNetworkController.Configure() Found FishNet NetworkManager");

                fishNetNetworkManager.ClientManager.OnClientConnectionState += HandleClientConnectionState;
                //fishNetNetworkManager.SceneManager.OnClientLoadedStartScenes += HandleClientLoadedStartScenes;
                fishNetNetworkManager.ServerManager.OnServerConnectionState += HandleServerConnectionState;
                fishNetNetworkManager.SceneManager.OnClientPresenceChangeEnd += HandleClientPresenceChangeEnd;

                // stuff that was previously done only on active connection
                fishNetNetworkManager.SceneManager.OnActiveSceneSet += HandleActiveSceneSet;
                //fishNetNetworkManager.SceneManager.OnUnloadStart += HandleUnloadStartServer;
                fishNetNetworkManager.SceneManager.OnQueueStart += HandleQueueStart;
                fishNetNetworkManager.SceneManager.OnQueueEnd += HandleQueueEnd;

            } else {
                //Debug.Log("FishNetNetworkController.Configure() Could not find FishNet NetworkManager");
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            levelManager = systemGameManager.LevelManager;
            networkManagerServer = systemGameManager.NetworkManagerServer;
            networkManagerClient = systemGameManager.NetworkManagerClient;
        }

        private void HandleClientPresenceChangeEnd(ClientPresenceChangeEventArgs args) {
            //Debug.Log($"FishNetNetworkController.HandleClientPresenceChangeEnd({args.Connection.ClientId})");

            clientConnector.AdvertisePresenceChangeEnd(args.Connection);
        }

        private void HandleQueueEnd() {
            //Debug.Log("FishNetNetworkController.HandleQueueEnd()");
        }

        private void HandleQueueStart() {
            //Debug.Log("FishNetNetworkController.HandleQueueStart()");
        }


        private void HandleServerConnectionState(ServerConnectionStateArgs obj) {
            //Debug.Log($"FishNetNetworkController.HandleServerConnectionState() {obj.ConnectionState.ToString()}");

            serverState = obj.ConnectionState;
            if (serverState == LocalConnectionState.Started) {
                //Debug.Log("FishNetNetworkController.HandleServerConnectionState() Server connection started.  Activating Server Mode.");
                systemGameManager.SetGameMode(GameMode.Network);
                networkManagerServer.ActivateServerMode();
                SubscribeToServerEvents();
                InstantiateNetworkConnector();
            } else if (serverState == LocalConnectionState.Stopping) {
                //Debug.Log("FishNetNetworkController.HandleServerConnectionState() Stopping");
            } else if (serverState == LocalConnectionState.Stopped) {
                //Debug.Log("FishNetNetworkController.HandleServerConnectionState() Stopped");
                systemGameManager.SetGameMode(GameMode.Local);
                networkManagerServer.DeactivateServerMode();
                UnsubscribeFromServerEvents();
            }
        }

        public void SubscribeToServerEvents() {
            //Debug.Log($"FishNetNetworkController.SubscribeToServerEvents()");

            fishNetNetworkManager.SceneManager.OnLoadEnd += HandleSceneLoadEndServer;
            fishNetNetworkManager.ServerManager.OnClientKick += HandleClientKick;
            fishNetNetworkManager.ServerManager.OnRemoteConnectionState += HandleRemoteConnectionState;
            fishNetNetworkManager.SceneManager.OnUnloadStart += HandleUnloadStartServer;
            //fishNetNetworkManager.SceneManager.OnUnloadEnd += HandleUnloadEndServer;
        }

        public void UnsubscribeFromServerEvents() {
            //Debug.Log($"FishNetNetworkController.UnsubscribeFromServerEvents()");

            fishNetNetworkManager.SceneManager.OnLoadEnd -= HandleSceneLoadEndServer;
            fishNetNetworkManager.ServerManager.OnClientKick -= HandleClientKick;
            fishNetNetworkManager.ServerManager.OnRemoteConnectionState -= HandleRemoteConnectionState;
            fishNetNetworkManager.SceneManager.OnUnloadStart -= HandleUnloadStartServer;
            //fishNetNetworkManager.SceneManager.OnUnloadEnd -= HandleUnloadEndServer;
        }

        private void HandleClientConnectionState(ClientConnectionStateArgs obj) {
            //Debug.Log($"HandleClientConnectionState() {obj.ConnectionState.ToString()}");

            clientState = obj.ConnectionState;
            if (clientState == LocalConnectionState.Starting) {
                SubscribeToClientEvents();
            } else if (clientState == LocalConnectionState.Started) {
                //Debug.Log("FishNetNetworkController.OnClientConnectionState() Connection Successful. Setting mode to network");
                systemGameManager.SetGameMode(GameMode.Network);
            } else if (clientState == LocalConnectionState.Stopping) {
                //Debug.Log("FishNetNetworkController.OnClientConnectionState() Disconnected from server. Stopping");
            } else if (clientState == LocalConnectionState.Stopped) {
                //Debug.Log("FishNetNetworkController.OnClientConnectionState() Disconnected from server. Setting mode to local");
                systemGameManager.NetworkManagerClient.ProcessStopConnection();
                UnsubscribeFromClientEvents();
            }
        }

        public void SubscribeToClientEvents() {
            fishNetNetworkManager.ClientManager.OnAuthenticated += HandleClientAuthenticated;
            fishNetNetworkManager.SceneManager.OnLoadStart += HandleLoadStartClient;
            fishNetNetworkManager.SceneManager.OnLoadPercentChange += HandleLoadPercentChangeClient;
            fishNetNetworkManager.SceneManager.OnLoadEnd += HandleLoadEndClient;
        }

        public void UnsubscribeFromClientEvents() {
            fishNetNetworkManager.ClientManager.OnAuthenticated -= HandleClientAuthenticated;
            fishNetNetworkManager.SceneManager.OnLoadStart -= HandleLoadStartClient;
            fishNetNetworkManager.SceneManager.OnLoadPercentChange -= HandleLoadPercentChangeClient;
            fishNetNetworkManager.SceneManager.OnLoadEnd -= HandleLoadEndClient;
        }

        #region client functions

        public override bool Login(string username, string password, string server) {
            //Debug.Log($"FishNetNetworkController.Login({username}, {password})");

            if (fishNetNetworkManager == null) {
                return false;
            }

            bool customPort = false;
            int port = 0;
            string serverAddress = server;
            if (string.IsNullOrEmpty(server)) {
                server = "localhost";
            } else {
                if (server.Contains(':')) {
                    string[] splitList = server.Split(":");
                    serverAddress = splitList[0];
                    if (int.TryParse(splitList[1], out port)) {
                        customPort = true;
                    }
                }
            }

            if (clientState != LocalConnectionState.Stopped) {
                Debug.Log("FishNetNetworkController.Login() Already connected to the server!");
                return false;
            }

            bool connectionResult;
            if (customPort) {
                connectionResult = fishNetNetworkManager.ClientManager.StartConnection(serverAddress, (ushort)port);
            } else {
                connectionResult = fishNetNetworkManager.ClientManager.StartConnection(serverAddress);
            }
            
            //Debug.Log($"FishNetNetworkController.Login() Result of connection attempt: {connectionResult}");

            return connectionResult;
        }

        public override void RequestLogout() {
            if (clientState == LocalConnectionState.Stopped) {
                Debug.Log("FishNetNetworkController.Login() Already disconnected from the server!");
                return;
            }

            clientConnector.RequestLogout();
        }

        public override void Disconnect() {
            if (clientState == LocalConnectionState.Stopped) {
                Debug.Log("FishNetNetworkController.Login() Already disconnected from the server!");
                return;
            }

            bool connectionResult = fishNetNetworkManager.ClientManager.StopConnection();
            Debug.Log($"FishNetNetworkController.Login() Result of disconnection attempt: {connectionResult}");
        }


        private void HandleRemoteConnectionState(NetworkConnection networkConnection, RemoteConnectionStateArgs args) {
            //Debug.Log($"FishNetNetworkController.HandleRemoteConnectionState({args.ConnectionState.ToString()})");

            if (args.ConnectionState == RemoteConnectionState.Stopped) {
                networkManagerServer.ProcessClientDisconnect(networkConnection.ClientId);
            }
        }

        private void HandleClientAuthenticated() {
            //Debug.Log($"FishNetNetworkController.HandleClientAuthenticated({fishNetNetworkManager.ClientManager.Connection.ClientId})");

            //networkManagerClient.SetClientId(fishNetNetworkManager.ClientManager.Connection.ClientId);
        }

        private void HandleClientKick(NetworkConnection arg1, int arg2, KickReason kickReason) {
            //Debug.Log($"FishNetNetworkController.HandleClientKick({kickReason.ToString()})");
        }

        /*
        private void HandleClientLoadedStartScenes(NetworkConnection networkConnection, bool asServer) {
            //Debug.Log("FishNetNetworkController.HandleClientLoadedStartScenes()");
            //networkManager.SceneManager.AddConnectionToScene(networkConnection, UnityEngine.SceneManagement.SceneManager.GetSceneByName("DontDestroyOnLoad"));
            //networkManager.SceneManager.AddConnectionToScene(networkConnection, UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
        */

        public void HandleLoadStartClient(SceneLoadStartEventArgs args) {
            networkManagerClient.HandleSceneLoadStart(args.QueueData.SceneLoadData.GetFirstLookupScene().name);
        }

        public void HandleLoadPercentChangeClient(SceneLoadPercentEventArgs args) {
            networkManagerClient.HandleSceneLoadPercentageChange(args.Percent);
        }

        private void HandleLoadEndClient(SceneLoadEndEventArgs obj) {
            //Debug.Log($"FishNetNetworkController.HandleLoadEnd() AsServer: {obj.QueueData.AsServer}");

            if (systemGameManager.GameMode == GameMode.Network) {
                levelManager.ProcessLevelLoad(true);
            }

        }

        private void HandleActiveSceneSet(bool userInitiated) {
            //Debug.Log($"FishNetNetworkController.HandleActiveSceneSet({userInitiated}) current scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");

            //if (systemGameManager.GameMode == GameMode.Network) {
            //    levelManager.ProcessLevelLoad();
            //}
        }

        /*
        private void HandleUnloadEndServer(SceneUnloadEndEventArgs obj) {
            //Debug.Log($"FishNetNetworkController.HandleUnloadEndServer()");

            foreach (UnloadedScene scene in obj.UnloadedScenesV2) {
                //Debug.Log($"FishNetNetworkController.HandleUnloadEnd() {scene.Name}");
                networkManagerServer.HandleSceneUnloadEnd(scene.Handle, scene.Name);
            }
        }
        */

        private void HandleUnloadStartServer(SceneUnloadStartEventArgs obj) {
            //Debug.Log($"FishNetNetworkController.HandleUnloadStart({obj.QueueData.SceneUnloadData.SceneLookupDatas[0].Name})");

            networkManagerServer.HandleSceneUnloadStart(obj.QueueData.SceneUnloadData.SceneLookupDatas[0].Handle, obj.QueueData.SceneUnloadData.SceneLookupDatas[0].Name);
        }


        public void RegisterConnector(FishNetClientConnector clientConnector) {
            this.clientConnector = clientConnector;
            if (clientConnector != null) {
                clientConnector.Configure(systemGameManager);
                clientConnector.SetNetworkManager(fishNetNetworkManager);
            }
        }

        
        /// <summary>
        /// Instantiate the network connector on the server that will spawn on every client, allowing them to issue requests to the server
        /// </summary>
        public void InstantiateNetworkConnector() {
            //Debug.Log("FishNetNetworkController.InstantiateNetworkConnector()");

            //networkConnectorSpawnReference = GameObject.Instantiate(networkConnectorSpawnPrefab);
            /*
            clientConnector = networkConnectorSpawnReference.gameObject.GetComponentInChildren<FishNetClientConnector>();
            if (clientConnector != null) {
                clientConnector.Configure(systemGameManager);
                clientConnector.SetNetworkManager(fishNetNetworkManager);
            }
            */

            NetworkObject networkPrefab = networkConnectorSpawnPrefab.GetComponent<NetworkObject>();
            if (networkPrefab == null) {
                Debug.LogWarning($"Could not find NetworkObject component on {networkConnectorSpawnPrefab.name}");
                return;
            }

            NetworkObject nob = fishNetNetworkManager.GetPooledInstantiated(networkPrefab, true);
            fishNetNetworkManager.ServerManager.Spawn(nob);

        }

        public override void RequestSpawnPlayerUnit(string sceneName) {
            //Debug.Log($"FishNetNetworkController.SpawnLobbyGamePlayer({characterRequestData.characterConfigurationRequest.unitProfile.ResourceName})");

            clientConnector.RequestSpawnPlayerUnit(sceneName);
        }

        public override void RequestRespawnPlayerUnit() {
            //Debug.Log($"FishNetNetworkController.SpawnLobbyGamePlayer({characterRequestData.characterConfigurationRequest.unitProfile.ResourceName})");

            clientConnector.RequestRespawnPlayerUnit();
        }

        public override void RequestDespawnPlayerUnit() {
            Debug.Log($"FishNetNetworkController.RequestDespawnPlayerUnit()");

            clientConnector.RequestDespawnPlayerUnit();
        }

        public override void RequestRevivePlayerUnit() {
            Debug.Log($"FishNetNetworkController.RequestRevivePlayerUnit()");

            clientConnector.RequestRevivePlayerUnit();
        }


        public override GameObject RequestSpawnModelPrefab(GameObject prefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            //Debug.Log($"FishNetNetworkController.RequestSpawnModelPrefab({prefab.name}, {parentTransform.gameObject.name}, {position}, {forward})");

            clientConnector.RequestSpawnModelPrefab(prefab, parentTransform, position, forward);
            return null;
        }

        public override void RequestReturnFromCutscene() {
            //Debug.Log($"FishNetNetworkController.LoadScene({sceneName})");

            clientConnector.RequestReturnFromCutscene();
        }

        public override bool CanSpawnCharacterOverNetwork() {
            //Debug.Log($"FishNetNetworkController.CanSpawnCharacterOverNetwork() isClient: {networkManager.IsClient}");
            return fishNetNetworkManager.IsClientStarted;
        }

        public override bool OwnPlayer(UnitController unitController) {
            NetworkBehaviour networkBehaviour = unitController.gameObject.GetComponent<NetworkBehaviour>();
            if (networkBehaviour != null && networkBehaviour.IsOwner == true) {
                return true;
            }
            return false;
        }

        public override void RequestCreatePlayerCharacter(AnyRPGSaveData saveData) {
            //Debug.Log($"FishNetNetworkController.RequestCreatePlayerCharacter(AnyRPGSaveData)");

            clientConnector.RequestCreatePlayerCharacter(saveData);
        }

        public override void DeletePlayerCharacter(int playerCharacterId) {
            Debug.Log($"FishNetNetworkController.DeletePlayerCharacter({playerCharacterId})");

            clientConnector.DeletePlayerCharacter(playerCharacterId);
        }

        public override void LoadCharacterList() {
            //Debug.Log($"FishNetNetworkController.LoadCharacterList()");

            if (clientConnector == null) {
                Debug.LogWarning($"FishNetNetworkController.LoadCharacterList(): networkConnector is null");
                return;
            }
            clientConnector.LoadCharacterList();
        }

        public override void RequestCreateLobbyGame(string sceneResourceName, bool allowLateJoin) {
            clientConnector.RequestCreateLobbyGame(sceneResourceName, allowLateJoin);
        }

        public override void CancelLobbyGame(int gameId) {
            clientConnector.CancelLobbyGame(gameId);
        }

        public override void JoinLobbyGame(int gameId) {
            clientConnector.JoinLobbyGame(gameId);
        }

        public override void LeaveLobbyGame(int gameId) {
            clientConnector.LeaveLobbyGame(gameId);
        }

        public override int GetClientId() {
            return fishNetNetworkManager.ClientManager.Connection.ClientId;
        }

        public override void SendLobbyChatMessage(string messageText) {
            clientConnector.SendLobbyChatMessage(messageText);
        }

        public override void SendLobbyGameChatMessage(string messageText, int gameId) {
            clientConnector.SendLobbyGameChatMessage(messageText, gameId);
        }

        public override void SendSceneChatMessage(string messageText) {
            clientConnector.SendSceneChatMessage(messageText);
        }

        public override void RequestLobbyGameList() {
            clientConnector.RequestLobbyGameList();
        }

        public override void RequestLobbyPlayerList() {
            clientConnector.RequestLobbyPlayerList();
        }

        public override void ChooseLobbyGameCharacter(string unitProfileName, int gameId, string appearanceString, List<SwappableMeshSaveData> swappableMeshSaveData) {
            //Debug.Log($"FishNetNetworkController.ChooseLobbyGameCharacter({unitProfileName}, {gameId})");

            clientConnector.ChooseLobbyGameCharacter(unitProfileName, gameId, appearanceString, swappableMeshSaveData);
        }

        public override void RequestStartLobbyGame(int gameId) {
            clientConnector.RequestStartLobbyGame(gameId);
        }

        public override void RequestJoinLobbyGameInProgress(int gameId) {
            clientConnector.RequestJoinLobbyGameInProgress(gameId);
        }

        public override void ToggleLobbyGameReadyStatus(int gameId) {
            clientConnector.ToggleLobbyGameReadyStatus(gameId);
        }

        public override void InteractWithOption(UnitController sourceUnitController, Interactable targetInteractable, int componentIndex, int choiceIndex) {
            clientConnector.InteractWithOptionClient(sourceUnitController, targetInteractable, componentIndex, choiceIndex);
        }

        public override void RequestSetPlayerCharacterClass(Interactable interactable, int componentIndex) {
            clientConnector.RequestSetPlayerCharacterClass(interactable, componentIndex);
        }

        public override void SetPlayerCharacterSpecialization(Interactable interactable, int componentIndex) {
            clientConnector.RequestSetPlayerCharacterSpecialization(interactable, componentIndex);
        }

        public override void RequestSetPlayerFaction(Interactable interactable, int componentIndex) {
            clientConnector.RequestSetPlayerFaction(interactable, componentIndex);
        }

        public override void RequestLearnSkill(Interactable interactable, int componentIndex, int skillId) {
            clientConnector.RequestLearnSkill(interactable, componentIndex, skillId);
        }

        public override void RequestAcceptQuest(Interactable interactable, int componentIndex, Quest quest) {
            clientConnector.RequestAcceptQuest(interactable, componentIndex, quest);
        }

        public override void RequestCompleteQuest(Interactable interactable, int componentIndex, Quest quest, QuestRewardChoices questRewardChoices) {
            clientConnector.RequestCompleteQuest(interactable, componentIndex, quest, questRewardChoices);
        }

        public override void SellVendorItem(Interactable interactable, int componentIndex, int itemInstanceId) {
            clientConnector.SellVendorItemClient(interactable, componentIndex, itemInstanceId);
        }

        public override void RequestSpawnUnit(Interactable interactable, int componentIndex, int unitLevel, int extraLevels, bool useDynamicLevel, string unitProfileName, string unitToughnessName) {
            //Debug.Log($"FishNetNetworkController.RequestSpawnUnit({unitProfileName}) {interactable.gameObject.name} {componentIndex} {unitLevel} {extraLevels} {useDynamicLevel} {unitToughnessName}");

            clientConnector.RequestSpawnUnit(interactable, componentIndex, unitLevel, extraLevels, useDynamicLevel, unitProfileName, unitToughnessName);
        }

        public override void RequestTurnInDialog(Interactable interactable, int componentIndex, Dialog dialog) {
            //Debug.Log($"FishNetNetworkController.RequestTurnInDialog({dialog.ResourceName})");

            clientConnector.RequestTurnInDialog(interactable, componentIndex, dialog);
        }

        public override void RequestTurnInQuestDialog(Dialog dialog) {
            //Debug.Log($"FishNetNetworkController.RequestTurnInQuestDialog({dialog.ResourceName})");

            clientConnector.RequestTurnInQuestDialog(dialog);
        }

        public override void BuyItemFromVendor(Interactable interactable, int componentIndex, int collectionIndex, int itemIndex, string resourceName) {
            clientConnector.BuyItemFromVendor(interactable, componentIndex, collectionIndex, itemIndex, resourceName);
        }

        public override void TakeAllLoot() {
            clientConnector.TakeAllLoot();
        }

        public override void RequestTakeLoot(int lootDropId) {
            Debug.Log($"FishNetNetworkController.RequestTakeLoot({lootDropId})");

            clientConnector.RequestTakeLoot(lootDropId);
        }

        public override void RequestBeginCrafting(Recipe recipe, int craftAmount) {
            Debug.Log($"FishNetNetworkController.RequestBeginCrafting({recipe.ResourceName}, {craftAmount})");

            clientConnector.RequestBeginCrafting(recipe.ResourceName, craftAmount);
        }

        public override void RequestCancelCrafting() {
            clientConnector.RequestCancelCrafting();
        }

        public override void RequestUpdatePlayerAppearance(Interactable interactable, int componentIndex, string unitProfileName, string appearanceString, List<SwappableMeshSaveData> swappableMeshSaveData) {
            clientConnector.RequestUpdatePlayerAppearance(interactable, componentIndex, unitProfileName, appearanceString, swappableMeshSaveData);
        }

        public override void RequestChangePlayerName(Interactable interactable, int componentIndex, string newName) {
            Debug.Log($"FishNetNetworkController.RequestChangePlayerName({newName})");

            clientConnector.RequestChangePlayerName(interactable, componentIndex, newName);
        }

        public override void RequestSpawnPet(UnitProfile unitProfile) {
            //Debug.Log($"FishNetNetworkController.RequestSpawnPet({unitProfile.DisplayName})");
            clientConnector.RequestSpawnPet(unitProfile.ResourceName);
        }

        public override void RequestDespawnPet(UnitProfile unitProfile) {
            //Debug.Log($"FishNetNetworkController.RequestSpawnPet({unitProfile.DisplayName})");
            clientConnector.RequestDespawnPet(unitProfile.ResourceName);
        }

        public override void RequestSceneWeather() {
            clientConnector.RequestSceneWeather();
        }

        public override void RequestLoadPlayerCharacter(int playerCharacterId) {
            clientConnector.RequestLoadPlayerCharacter(playerCharacterId);
        }

        public override void AcceptCharacterGroupInvite(int inviteGroupId) {
            clientConnector.AcceptCharacterGroupInvite(inviteGroupId);
        }

        public override void DeclineCharacterGroupInvite() {
            clientConnector.DeclineCharacterGroupInvite();
        }

        public override void RequestLeaveCharacterGroup() {
            clientConnector.RequestLeaveCharacterGroup();
        }

        public override void RequestRemoveCharacterFromGroup(int playerCharacterId) {
            clientConnector.RequestRemoveCharacterFromGroup(playerCharacterId);
        }

        public override void RequestInviteCharacterToGroup(int characterId) {
            clientConnector.RequestInviteCharacterToGroup(characterId);
        }

        public override void RequestDisbandCharacterGroup(int characterGroupId) {
            clientConnector.RequestDisbandCharacterGroup(characterGroupId);
        }

        #endregion

        #region server functions

        private void HandleSceneLoadEndServer(SceneLoadEndEventArgs obj) {
            //Debug.Log($"FishNetNetworkController.HandleLoadEndServer() skipped: {obj.SkippedSceneNames.Length}; loaded: {obj.LoadedScenes.Length} options hashcode: {obj.QueueData.SceneLoadData.GetHashCode()}");

            if (obj.SkippedSceneNames.Length > 0 && obj.LoadedScenes.Length == 0) {
                return;
            }
            foreach (Scene scene in obj.LoadedScenes) {
                //Debug.Log($"FishNetNetworkController.HandleSceneLoadEndServer() loaded: {scene.name} handle: {scene.handle}");
                networkManagerServer.HandleSceneLoadEnd(scene, obj.QueueData.SceneLoadData.GetHashCode());
            }
            //Debug.Log($"FishNetNetworkController.HandleLoadEnd() skipped: {string.Join(',', obj.SkippedSceneNames.ToList())}");

        }

        public override void StartServer(ushort port) {
            //Debug.Log($"FishNetNetworkController.StartServer()");

            fishNetNetworkManager.ServerManager.StartConnection(port);
        }

        public override void StopServer() {
            fishNetNetworkManager.ServerManager.StopConnection(true);
        }

        public override void KickPlayer(int accountId) {
            if (networkManagerServer.LoggedInAccounts.ContainsKey(accountId) == false) {
                return;
            }
            fishNetNetworkManager.ServerManager.Kick(networkManagerServer.LoggedInAccounts[accountId].clientId, KickReason.Unset);
        }

        public override string GetClientIPAddress(int clientId) {

            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                return "ClientId not found";
            }

            return fishNetNetworkManager.ServerManager.Clients[clientId].GetAddress();
        }

        public override void AdvertiseCreateLobbyGame(LobbyGame lobbyGame) {
            clientConnector.AdvertiseCreateLobbyGame(lobbyGame);
        }

        public override void AdvertiseCancelLobbyGame(int gameId) {
            clientConnector.AdvertiseCancelLobbyGame(gameId);
        }

        public override void AdvertiseAccountJoinLobbyGame(int gameId, int accountId, string userName) {
            clientConnector.AdvertiseAccountJoinLobbyGame(gameId, accountId, userName);
        }

        public override void AdvertiseAccountLeaveLobbyGame(int gameId, int accountId) {
            clientConnector.AdvertiseAccountLeaveLobbyGame(gameId, accountId);
        }

        public override void AdvertiseSendLobbyChatMessage(string messageText) {
            clientConnector.AdvertiseSendLobbyChatMessage(messageText);
        }

        public override void AdvertiseSendLobbyGameChatMessage(string messageText, int gameId) {
            clientConnector.AdvertiseSendLobbyGameChatMessage(messageText, gameId);
        }

        public override void AdvertiseSendSceneChatMessage(string messageText, int accountId) {
            clientConnector.AdvertiseSendSceneChatMessage(messageText, accountId);
        }

        public override void AdvertiseLobbyLogin(int accountId, string userName) {
            clientConnector.AdvertiseLobbyLogin(accountId, userName);
        }

        public override void AdvertiseLobbyLogout(int accountId) {
            clientConnector.AdvertiseLobbyLogout(accountId);
        }

        public override void SetLobbyGameList(int accountId, List<LobbyGame> lobbyGames) {
            clientConnector.SendLobbyGameList(accountId, lobbyGames);
        }

        public override void SetLobbyPlayerList(int accountId, Dictionary<int, string> lobbyPlayers) {
            clientConnector.SendLobbyPlayerList(accountId, lobbyPlayers);
        }

        public override void AdvertiseChooseLobbyGameCharacter(int gameId, int accountId, string unitProfileName) {
            //Debug.Log($"FishNetNetworkController.AdvertiseChooseLobbyGameCharacter({gameId}, {accountId}, {unitProfileName})");

            clientConnector.AdvertiseChooseLobbyGameCharacter(gameId, accountId, unitProfileName);
        }

        public override void StartLobbyGame(int gameId/*, string sceneName*/) {
            clientConnector.StartLobbyGame(gameId);
        }

        public override void AdvertiseJoinLobbyGameInProgress(int gameId, int accountId, string sceneResourceName) {
            clientConnector.JoinLobbyGameInProgress(gameId, accountId, sceneResourceName);
        }

        public override void AdvertiseLoadPlayerCharacter(int accountId, string sceneName) {
            clientConnector.JoinMMOGameInProgress(accountId, sceneName);
        }

        public override void AdvertiseAddCharacterToGroup(int playerCharacterId, CharacterGroup characterGroup) {
            clientConnector.AdvertiseAddCharacterToGroup(playerCharacterId, characterGroup);
        }

        public override void AdvertiseCharacterGroup(int accountId, CharacterGroup characterGroup) {
            clientConnector.AdvertiseCharacterGroup(accountId, characterGroup);
        }

        public override void AdvertiseRemoveCharacterFromGroup(int characterId, CharacterGroup characterGroup) {
            clientConnector.AdvertiseRemoveCharacterFromGroup(characterId, characterGroup);
        }

        public override void AdvertiseCharacterGroupInvite(int invitedCharacterId, CharacterGroup characterGroup, string leaderName) {
            clientConnector.AdvertiseCharacterGroupInvite(invitedCharacterId, characterGroup, leaderName);
        }

        public override void AdvertiseDisbandCharacterGroup(CharacterGroup characterGroup) {
            clientConnector.AdvertiseDisbandCharacterGroup(characterGroup);
        }

        public override void AdvertisePlayerNameNotAvailable(int accountId) {
            clientConnector.AdvertisePlayerNameNotAvailable(accountId);
        }

        public override void AdvertiseLoadCharacterList(int accountId, List<PlayerCharacterSaveData> playerCharacterSaveDataList) {
            clientConnector.AdvertiseLoadCharacterList(accountId, playerCharacterSaveDataList);
        }

        public override void AdvertiseDeletePlayerCharacter(int accountId) {
            clientConnector.AdvertiseDeletePlayerCharacter(accountId);
        }

        public override void AdvertiseDeclineCharacterGroupInvite(int leaderAccountId, string decliningPlayerName) {
            clientConnector.AdvertiseDeclineCharacterGroupInvite(leaderAccountId, decliningPlayerName);
        }


        public override void AdvertiseSetLobbyGameReadyStatus(int gameId, int accountId, bool ready) {
            clientConnector.AdvertiseSetLobbyGameReadyStatus(gameId, accountId, ready);
        }

        public override int GetServerPort() {
            return fishNetNetworkManager.TransportManager.Transport.GetPort();
        }

        public override void AdvertiseLoadScene(string sceneResourceName, int accountId) {
            Debug.Log($"FishNetNetworkController.AdvertiseLoadScene({sceneResourceName}, {accountId})");

            clientConnector.AdvertiseLoadSceneServer(sceneResourceName, accountId);
        }

        public override void ReturnObjectToPool(GameObject returnedObject) {
            clientConnector.ReturnObjectToPool(returnedObject);
        }

        /*
        public override void AdvertiseInteractWithQuestGiver(Interactable interactable, int optionIndex, int accountId) {

            NetworkInteractable networkInteractable = null;
            if (interactable != null) {
                networkInteractable = interactable.GetComponent<NetworkInteractable>();
            }
            clientConnector.AdvertiseInteractWithQuestGiver(networkInteractable, optionIndex, accountId);
        }
        */

        /*
        public override void AdvertiseAddSpawnRequest(int accountId, SpawnPlayerRequest loadSceneRequest) {
            clientConnector.AdvertiseAddSpawnRequestServer(accountId, loadSceneRequest);
        }
        */

        /*
        public override void AdvertiseInteractWithClassChangeComponentServer(int accountId, Interactable interactable, int optionIndex) {
            clientConnector.AdvertiseInteractWithClassChangeComponentServer(accountId, interactable, optionIndex);
        }

        public override void AdvertiseInteractWithSkillTrainerComponentServer(int accountId, Interactable interactable, int optionIndex) {
            clientConnector.AdvertiseInteractWithSkillTrainerComponentServer(accountId, interactable, optionIndex);
        }

        public override void AdvertiseInteractWithAnimatedObjectComponentServer(int accountId, Interactable interactable, int optionIndex) {
            clientConnector.AdvertiseInteractWithAnimatedObjectComponentServer(accountId, interactable, optionIndex);
        }
        */

        public override UnitController SpawnCharacterPrefab(CharacterRequestData characterRequestData, Transform parentTransform, Vector3 position, Vector3 forward, Scene scene) {
            return clientConnector.SpawnCharacterUnit(characterRequestData, parentTransform, position, forward, scene);
        }

        public override GameObject SpawnModelPrefabServer(GameObject prefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            //Debug.Log($"FishNetNetworkController.SpawnModelPrefabServer({spawnRequestId}, {parentTransform.gameObject.name})");

            clientConnector.SpawnModelPrefabServer(prefab, parentTransform, position, forward);
            return null;
        }

        public override void AdvertiseMessageFeedMessage(int accountId, string message) {
            clientConnector.AdvertiseMessageFeedMessage(accountId, message);
        }

        public override void AdvertiseSystemMessage(int accountId, string message) {
            clientConnector.AdvertiseSystemMessage(accountId, message);
        }

        public override void AdvertiseAddToBuyBackCollection(UnitController sourceUnitController, int accountId, Interactable interactable, int componentIndex, InstantiatedItem newInstantiatedItem) {
            clientConnector.AdvertiseAddToBuyBackCollection(sourceUnitController, accountId, interactable, componentIndex, newInstantiatedItem);
        }

        public override void AdvertiseSellItemToPlayer(UnitController sourceUnitController, Interactable interactable, int componentIndex, int collectionIndex, int itemIndex, string resourceName, int remainingQuantity) {
            //Debug.Log($"FishNetNetworkController.AdvertiseSellItemToPlayer({sourceUnitController.gameObject.name}, {interactable.gameObject.name}, {componentIndex}, {collectionIndex}, {itemIndex}, {resourceName}, {remainingQuantity})");
            
            clientConnector.AdvertiseSellItemToPlayer(sourceUnitController, interactable, componentIndex, collectionIndex, itemIndex, resourceName, remainingQuantity);
        }

        public override void AddAvailableDroppedLoot(int accountId, List<LootDrop> items) {
            //Debug.Log($"FishNetNetworkController.AddAvailableDroppedLoot({accountId}, {items.Count})");

            clientConnector.AddAvailableDroppedLoot(accountId, items);
        }

        public override void AddLootDrop(int accountId, int lootDropId, int itemId) {
            clientConnector.AddDroppedLoot(accountId, lootDropId, itemId);
        }

        public override void AdvertiseTakeLoot(int accountId, int lootDropId) {
            clientConnector.AdvertiseTakeLoot(accountId, lootDropId);
        }

        public override void AdvertiseAddSpawnRequest(int accountId, SpawnPlayerRequest loadSceneRequest) {
            //Debug.Log($"FishNetNetworkController.AdvertiseAddSpawnRequest({accountId})");

            clientConnector.AdvertiseAddSpawnRequestServer(accountId, loadSceneRequest);
        }

        public override void AdvertiseLoadCutscene(Cutscene cutscene, int accountId) {
            clientConnector.AdvertiseLoadCutscene(cutscene, accountId);
        }

        public override void AdvertiseStartWeather(int sceneHandle) {
            //Debug.Log($"FishNetNetworkController.AdvertiseStartWeather({sceneHandle})");

            clientConnector.AdvertiseStartWeather(sceneHandle);
        }

        public override void AdvertiseChooseWeather(int sceneHandle, WeatherProfile weatherProfile) {
            //Debug.Log($"FishNetNetworkController.AdvertiseChooseWeather({sceneHandle}, {(weatherProfile == null ? "null" : weatherProfile.ResourceName)})");

            clientConnector.AdvertiseChooseWeather(sceneHandle, weatherProfile);
        }

        public override void AdvertiseEndWeather(int sceneHandle, WeatherProfile weatherProfile, bool immediate) {
            //Debug.Log($"FishNetNetworkController.AdvertiseEndWeather({sceneHandle}, {(weatherProfile == null ? "null" : weatherProfile.ResourceName)}, {immediate})");
            
            clientConnector.AdvertiseEndWeather(sceneHandle, weatherProfile, immediate);
        }

        public override void SpawnPlayer(int accountId, CharacterRequestData characterRequestData, Vector3 position, Vector3 forward, string sceneName) {
            clientConnector.SpawnPlayer(accountId, characterRequestData, position, forward, sceneName);
        }

        public override Scene GetAccountScene(int accountId, string sceneName) {
            return clientConnector.GetAccountScene(accountId, sceneName);
        }

        /*
        public override void SetCraftingManagerAbility(int accountId, string abilityName) {
            Debug.Log($"FishNetNetworkController.SetCraftingManagerAbility({accountId}, {abilityName})");

            clientConnector.SetCraftingManagerAbility(accountId, abilityName);
        }
        */

        #endregion

    }
}
