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
            //Debug.Log($"FishNetNetworkController.HandleClientPresenceChangeEnd(clientId: {args.Connection.ClientId})");

            if (args.Added) {
                clientConnector.AdvertisePresenceChangeEnd(args.Connection);
            }

            // Grab the scene
            Scene scene = args.Scene;

            if (fishNetNetworkManager.SceneManager.SceneConnections.TryGetValue(scene, out var connections)) {

                //Debug.Log($"FishNetNetworkController.HandleClientPresenceChangeEnd(clientId: {args.Connection.ClientId}) connections count: {connections.Count}");

                //int actualCount = args.Added ? connections.Count : connections.Count - 1;

                // Ensure we don't drop below 0 due to timing
                //actualCount = Mathf.Max(0, actualCount);

                networkManagerServer.SetSceneClientCount(scene.name, scene.handle, connections.Count);
            }

        }

        private void UpdateAllSceneCounts() {
            // Iterate through all active scene instances (handles)
            foreach (var entry in fishNetNetworkManager.SceneManager.SceneConnections) {
                var scene = entry.Key;
                var connections = entry.Value;

                // Pass the live count to your server management logic
                networkManagerServer.SetSceneClientCount(scene.name, scene.handle, connections.Count);
            }
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
               //Debug.Log("FishNetNetworkController.Login() Already connected to the server!");
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
               //Debug.Log("FishNetNetworkController.Login() Already disconnected from the server!");
                return;
            }

            clientConnector.RequestLogout();
        }

        public override void Disconnect() {
            if (clientState == LocalConnectionState.Stopped) {
               //Debug.Log("FishNetNetworkController.Login() Already disconnected from the server!");
                return;
            }

            bool connectionResult = fishNetNetworkManager.ClientManager.StopConnection();
           //Debug.Log($"FishNetNetworkController.Login() Result of disconnection attempt: {connectionResult}");
        }


        private void HandleRemoteConnectionState(NetworkConnection networkConnection, RemoteConnectionStateArgs args) {
            //Debug.Log($"FishNetNetworkController.HandleRemoteConnectionState({args.ConnectionState.ToString()})");

            if (args.ConnectionState == RemoteConnectionState.Stopped) {
                networkManagerServer.ProcessClientDisconnect(networkConnection.ClientId);
                UpdateAllSceneCounts();
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
           //Debug.Log($"FishNetNetworkController.RequestDespawnPlayerUnit()");

            clientConnector.RequestDespawnPlayerUnit();
        }

        public override void RequestRevivePlayerUnit() {
           //Debug.Log($"FishNetNetworkController.RequestRevivePlayerUnit()");

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

        public override void RequestCreatePlayerCharacter(CharacterSaveData saveData) {
            //Debug.Log($"FishNetNetworkController.RequestCreatePlayerCharacter(AnyRPGSaveData)");

            clientConnector.RequestCreatePlayerCharacter(saveData);
        }

        public override void DeletePlayerCharacter(int playerCharacterId) {
           //Debug.Log($"FishNetNetworkController.DeletePlayerCharacter({playerCharacterId})");

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

        public override void RequestCreateGuild(Interactable interactable, int componentIndex, string guildName) {
            clientConnector.RequestCreateGuild(interactable, componentIndex, guildName);
        }

        public override void CheckGuildName(Interactable interactable, int componentIndex, string guildName) {
            clientConnector.CheckGuildName(interactable, componentIndex, guildName);
        }

        public override void AcceptFriendInvite(int inviteCharacterId) {
            clientConnector.AcceptFriendInvite(inviteCharacterId);
        }

        public override void RequestLearnSkill(Interactable interactable, int componentIndex, int skillId) {
            clientConnector.RequestLearnSkill(interactable, componentIndex, skillId);
        }

        public override void RequestSendMail(Interactable interactable, int componentIndex, MailMessageRequest sendMailRequest) {
            clientConnector.RequestSendMail(interactable, componentIndex, sendMailRequest);
        }

        public override void RequestListAuctionItems(Interactable interactable, int componentIndex, ListAuctionItemRequest listAuctionItemRequest) {
            clientConnector.RequestListAuctionItems(interactable, componentIndex, listAuctionItemRequest);
        }

        public override void RequestSearchAuctions(Interactable interactable, int componentIndex, string searchText, bool onlyShowOwnAuctions) {
            clientConnector.RequestSearchAuctions(interactable, componentIndex, searchText, onlyShowOwnAuctions);
        }

        public override void RequestAcceptQuest(Interactable interactable, int componentIndex, Quest quest) {
            clientConnector.RequestAcceptQuest(interactable, componentIndex, quest);
        }

        public override void RequestCompleteQuest(Interactable interactable, int componentIndex, Quest quest, QuestRewardChoices questRewardChoices) {
            clientConnector.RequestCompleteQuest(interactable, componentIndex, quest, questRewardChoices);
        }

        public override void SellVendorItem(Interactable interactable, int componentIndex, long itemInstanceId) {
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
           //Debug.Log($"FishNetNetworkController.RequestTakeLoot({lootDropId})");

            clientConnector.RequestTakeLoot(lootDropId);
        }

        public override void RequestBeginCrafting(Recipe recipe, int craftAmount) {
           //Debug.Log($"FishNetNetworkController.RequestBeginCrafting({recipe.ResourceName}, {craftAmount})");

            clientConnector.RequestBeginCrafting(recipe.ResourceName, craftAmount);
        }

        public override void RequestCancelCrafting() {
            clientConnector.RequestCancelCrafting();
        }

        public override void RequestUpdatePlayerAppearance(Interactable interactable, int componentIndex, string unitProfileName, string appearanceString, List<SwappableMeshSaveData> swappableMeshSaveData) {
            clientConnector.RequestUpdatePlayerAppearance(interactable, componentIndex, unitProfileName, appearanceString, swappableMeshSaveData);
        }

        public override void RequestChangePlayerName(Interactable interactable, int componentIndex, string newName) {
           //Debug.Log($"FishNetNetworkController.RequestChangePlayerName({newName})");

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

        public override void RequestInviteCharacterToFriendList(int characterId) {
            clientConnector.RequestInviteCharacterToFriendList(characterId);
        }

        public override void RequestInviteCharacterToFriendList(string characterName) {
            clientConnector.RequestInviteCharacterToFriendList(characterName);
        }

        public override void RequestRemoveCharacterFromFriendList(int characterId) {
            clientConnector.RequestRemoveCharacterFromFriendList(characterId);
        }

        public override void RequestPromoteGuildCharacter(int characterId) {
            clientConnector.RequestPromoteGuildCharacter(characterId);
        }

        public override void RequestDemoteGuildCharacter(int characterId) {
            clientConnector.RequestDemoteGuildCharacter(characterId);
        }

        public override void RequestPromoteGroupCharacter(int characterId) {
            clientConnector.RequestPromoteGroupCharacter(characterId);
        }

        public override void RequestDemoteGroupCharacter(int characterId) {
            clientConnector.RequestDemoteGroupCharacter(characterId);
        }


        public override void AcceptGuildInvite(int inviteGuildId) {
            clientConnector.AcceptGuildInvite(inviteGuildId);
        }

        public override void DeclineGuildInvite() {
            clientConnector.DeclineGuildInvite();
        }

        public override void DeclineFriendInvite(int inviteCharacterId) {
            clientConnector.DeclineFriendInvite(inviteCharacterId);
        }

        public override void RequestLeaveGuild() {
            clientConnector.RequestLeaveGuild();
        }

        public override void RequestDisbandGuild(int guildId) {
            clientConnector.RequestDisbandGuild(guildId);
        }

        public override void RequestInviteCharacterToGuild(int characterId) {
            clientConnector.RequestInviteCharacterToGuild(characterId);
        }

        public override void RequestInviteCharacterToGuild(string characterName) {
            clientConnector.RequestInviteCharacterToGuild(characterName);
        }

        public override void RequestRemoveCharacterFromGuild(int characterId) {
            clientConnector.RequestRemoveCharacterFromGuild(characterId);
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

        public override void RequestInviteCharacterToGroup(string characterName) {
            clientConnector.RequestInviteCharacterToGroup(characterName);
        }

        public override void RequestDisbandCharacterGroup(int characterGroupId) {
            clientConnector.RequestDisbandCharacterGroup(characterGroupId);
        }

        public override void RequestPromoteCharacterToLeader(int characterId) {
            clientConnector.RequestPromoteCharacterToLeader(characterId);
        }

        public override void RequestBeginTrade(int characterId) {
            clientConnector.RequestBeginTrade(characterId);
        }

        public override void RequestDeclineTrade() {
            clientConnector.RequestDeclineTrade();
        }

        public override void RequestAcceptTrade() {
            clientConnector.RequestAcceptTrade();
        }

        public override void RequestAddItemsToTradeSlot(int buttonIndex, List<long> itemInstanceIdList) {
            clientConnector.RequestAddItemsToTradeSlot(buttonIndex, itemInstanceIdList);
        }

        public override void RequestAddCurrencyToTrade(CurrencyNode currencyNode) {
            clientConnector.RequestAddCurrencyToTrade(currencyNode.Amount);
        }

        public override void RequestCancelTrade() {
            clientConnector.RequestCancelTrade();
        }

        public override void RequestConfirmTrade() {
            clientConnector.RequestConfirmTrade();
        }

        public override void RequestUnconfirmTrade() {
            clientConnector.RequestUnconfirmTrade();
        }

        public override void RequestDeleteMailMessage(int messageId) {
            clientConnector.RequestDeleteMailMessage(messageId);
        }

        public override void RequestTakeMailAttachment(int messageId, int attachmentSlotId) {
            clientConnector.RequestTakeMailAttachment(messageId, attachmentSlotId);
        }

        public override void RequestTakeMailAttachments(int messageId) {
            clientConnector.RequestTakeMailAttachments(messageId);
        }

        public override void RequestMarkMailAsRead(int currentMessageId) {
            clientConnector.RequestMarkMailAsRead(currentMessageId);
        }

        public override void RequestBuyAuctionItem(int auctionItemId) {
            clientConnector.RequestBuyAuctionItem(auctionItemId);
        }

        public override void RequestCancelAuction(int auctionItemId) {
            clientConnector.RequestCancelAuction(auctionItemId);
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

        public override void KickPlayer(int clientId) {
            //Debug.Log($"FishNetNetworkController.KickPlayer(clientId: {clientId})");

            fishNetNetworkManager.ServerManager.Kick(clientId, KickReason.Unset);
        }

        public override string GetClientIPAddress(int clientId) {

            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                return "ClientId not found";
            }

            return fishNetNetworkManager.ServerManager.Clients[clientId].GetAddress();
        }

        public override void AdvertiseCreateLobbyGame(LobbyGame lobbyGame) {
            //Debug.Log($"FishNetNetworkController.AdvertiseCreateLobbyGame()");

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

        public override void AdvertiseJoinMMOGameInProgress(int accountId) {
            clientConnector.AdvertiseJoinMMOGameInProgress(accountId);
        }

        public override void AdvertiseAddCharacterToGroup(int accountId, int characterGroupId, CharacterGroupMemberNetworkData characterGroupMemberNetworkData) {
            clientConnector.AdvertiseAddCharacterToGroup(accountId, characterGroupId, characterGroupMemberNetworkData);
        }

        public override void AdvertiseAddCharacterToGuild(int existingAccountId, int guildId, GuildMemberNetworkData guildMemberNetworkData) {
            //Debug.Log($"FishNetNetworkController.AdvertiseAddCharacterToGuild({existingAccountId}, {guildId}, {characterSummaryNetworkData.CharacterName})");

            clientConnector.AdvertiseAddCharacterToGuild(existingAccountId, guildId, guildMemberNetworkData);
        }

        public override void AdvertiseCharacterGroup(int accountId, CharacterGroupNetworkData characterGroupNetworkData) {
            clientConnector.AdvertiseCharacterGroup(accountId, characterGroupNetworkData);
        }

        public override void AdvertiseRemoveCharacterFromGroup(int accountId, int characterId, int characterGroupId) {
            clientConnector.AdvertiseRemoveCharacterFromGroup(accountId, characterId, characterGroupId);
        }

        public override void AdvertiseRemoveCharacterFromGuild(int accountId, int characterId, int guildId) {
            clientConnector.AdvertiseRemoveCharacterFromGuild(accountId, characterId, guildId);
        }

        public override void AdvertiseCharacterGroupInvite(int invitedCharacterId, int characterGroupId, string leaderName) {
            clientConnector.AdvertiseCharacterGroupInvite(invitedCharacterId, characterGroupId, leaderName);
        }

        public override void AdvertiseDisbandCharacterGroup(int accountId, int characterGroupId) {
            clientConnector.AdvertiseDisbandCharacterGroup(accountId, characterGroupId);
        }

        public override void AdvertiseRenameCharacterInGroup(int accountId, int groupId, int characterId, string newName) {
            clientConnector.AdvertiseRenameCharacterInGroup(accountId, groupId, characterId, newName);
        }

        public override void AdvertiseGroupMessage(int accountId, int characterGroupId, string messageText) {
            clientConnector.AdvertiseGroupMessage(accountId, characterGroupId, messageText);
        }

        public override void AdvertiseGuildMessage(int accountId, int guildId, string messageText) {
            clientConnector.AdvertiseGuildMessage(accountId, guildId, messageText);
        }

        public override void AdvertisePrivateMessage(int targetAccountId, string messageText) {
            clientConnector.AdvertisePrivateMessage(targetAccountId, messageText);
        }

        public override void AdvertiseAcceptTradeInvite(int sourceAccountId, int targetCharacterId) {
            clientConnector.AdvertiseAcceptTradeInvite(sourceAccountId, targetCharacterId);
        }

        public override void AdvertiseDeclineTradeInvite(int sourceAccountId) {
            clientConnector.AdvertiseDeclineTradeInvite(sourceAccountId);
        }

        public override void AdvertiseRequestBeginTrade(int targetAccountId, int sourceCharacterId) {
            clientConnector.AdvertiseRequestBeginTrade(targetAccountId, sourceCharacterId);
        }

        public override void AdvertiseAddItemsToTargetTradeSlot(int targetAccountId, int buttonIndex, List<long> itemInstanceIdList) {
            clientConnector.AdvertiseAddItemsToTargetTradeSlot(targetAccountId, buttonIndex, itemInstanceIdList);
        }

        public override void AdvertiseAddCurrencyToTrade(int targetAccountId, int amount) {
            clientConnector.AdvertiseAddCurrencyToTrade(targetAccountId, amount);
        }

        public override void AdvertisePlayerNameNotAvailable(int accountId) {
            clientConnector.AdvertisePlayerNameNotAvailable(accountId);
        }

        public override void AdvertiseLoadCharacterList(int accountId, List<PlayerCharacterSaveData> playerCharacterSaveDataList) {
            clientConnector.AdvertiseLoadCharacterList(accountId, playerCharacterSaveDataList);
        }

        public override void AdvertiseDeclineCharacterGroupInvite(int leaderAccountId, string decliningPlayerName) {
            clientConnector.AdvertiseDeclineCharacterGroupInvite(leaderAccountId, decliningPlayerName);
        }

        public override void AdvertisePromoteGroupLeader(int accountId, int characterGroupId, int newLeaderCharacterId) {
            clientConnector.AdvertisePromoteGroupLeader(accountId, characterGroupId, newLeaderCharacterId);
        }

        public override void AdvertiseSetLobbyGameReadyStatus(int gameId, int accountId, bool ready) {
            clientConnector.AdvertiseSetLobbyGameReadyStatus(gameId, accountId, ready);
        }


        public override void AdvertiseUnloadScene(int accountId) {
            //Debug.Log($"FishNetNetworkController.AdvertiseLoadScene({sceneResourceName}, {accountId})");

            clientConnector.AdvertiseUnloadSceneServer(accountId);
        }

        public override void AdvertiseCancelTrade(int accountId) {
            clientConnector.AdvertiseCancelTrade(accountId);
        }

        public override void AdvertiseMailMessages(int accountId, MailMessageListBundle mailMessageListResponse) {
            clientConnector.AdvertiseMailMessages(accountId, mailMessageListResponse);
        }

        public override void AdvertiseAuctionItems(int accountId, AuctionItemSearchListResult auctionItemListResponse) {
            clientConnector.AdvertiseAuctionItems(accountId, auctionItemListResponse);
        }

        public override void AdvertiseListAuctionItems(int accountId) {
            clientConnector.AdvertiseListAuctionItems(accountId);
        }

        public override void AdvertiseCompleteTrade(int accountId) {
            clientConnector.AdvertiseCompleteTrade(accountId);
        }

        public override void AdvertiseDeleteMailMessage(int accountId, int messageId) {
            clientConnector.AdvertiseDeleteMailMessage(accountId, messageId);
        }

        public override void AdvertiseTakeMailAttachment(int accountId, int messageId, int attachmentSlotId) {
            clientConnector.AdvertiseTakeMailAttachment(accountId, messageId, attachmentSlotId);
        }

        public override void AdvertiseTakeMailAttachments(int accountId, int messageId) {
            clientConnector.AdvertiseTakeMailAttachments(accountId, messageId);
        }

        public override void AdvertiseConfirmationPopup(int accountId, string messageText) {
            clientConnector.AdvertiseConfirmationPopup(accountId, messageText);
        }

        public override void AdvertiseBuyAuctionItem(int accountId, int auctionItemId) {
            clientConnector.AdvertiseBuyAuctionItem(accountId, auctionItemId);
        }

        public override void AdvertiseCancelAuction(int accountId, int auctionItemId) {
            clientConnector.AdvertiseCancelAuction(accountId, auctionItemId);
        }

        public override void AdvertiseMailSend(int accountId) {
            clientConnector.AdvertiseMailSend(accountId);
        }

        public override int GetServerPort() {
            return fishNetNetworkManager.TransportManager.Transport.GetPort();
        }

        public override void ReturnObjectToPool(GameObject returnedObject) {
            clientConnector.ReturnObjectToPool(returnedObject);
        }

        public override void AdvertiseDeclineGuildInvite(int leaderAccountId, string playerName) {
            clientConnector.AdvertiseDeclineGuildInvite(leaderAccountId, playerName);
        }

        public override void AdvertiseDisbandGuild(int accountId, int guildId) {
            clientConnector.AdvertiseDisbandGuild(accountId, guildId);
        }

        public override void AdvertiseGuild(int accountId, GuildNetworkData guildNetworkData) {
            clientConnector.AdvertiseGuild(accountId, guildNetworkData);
        }

        public override void AdvertiseGuildNameAvailable(int accountId) {
            clientConnector.AdvertiseGuildNameAvailable(accountId);
        }

        public override void AdvertiseGuildInvite(int invitedCharacterId, int guildId, string leaderName) {
            clientConnector.AdvertiseGuildInvite(invitedCharacterId, guildId, leaderName);
        }

        public override void AdvertisePromoteGuildLeader(int accountId, int guildId, int newLeaderCharacterId) {
            clientConnector.AdvertisePromoteGuildLeader(accountId, guildId, newLeaderCharacterId);
        }

        public override void AdvertiseRenameCharacterInGuild(int accountId, int guildId, int characterId, string newName) {
            clientConnector.AdvertiseRenameCharacterInGuild(accountId, guildId, characterId, newName);
        }

        public override void AdvertiseCharacterGroupMemberStatusChange(int accountId, int characterGroupId, int playerCharacterId, CharacterGroupMemberNetworkData characterGroupMemberNetworkData) {
            clientConnector.AdvertiseCharacterGroupMemberStatusChange(accountId, characterGroupId, playerCharacterId, characterGroupMemberNetworkData);
        }

        public override void AdvertiseGuildMemberStatusChange(int accountId, int guildId, int playerCharacterId, GuildMemberNetworkData guildMemberNetworkData) {
            clientConnector.AdvertiseGuildMemberStatusChange(accountId, guildId, playerCharacterId, guildMemberNetworkData);
        }

        public override void AdvertiseAddFriend(int sourceCharacterAccountId, CharacterSummaryNetworkData characterSummaryNetworkData) {
            clientConnector.AdvertiseAddFriend(sourceCharacterAccountId, characterSummaryNetworkData);
        }

        public override void AdvertiseRemoveCharacterFromFriendList(int targetCharacterAccountId, int sourceCharacterId) {
            clientConnector.AdvertiseRemoveCharacterFromFriendList(targetCharacterAccountId, sourceCharacterId);
        }

        public override void AdvertiseDeclineFriendInvite(int friendAccountId, string characterName) {
            clientConnector.AdvertiseDeclineFriendInvite(friendAccountId, characterName);
        }

        public override void AdvertiseFriendInvite(int invitedAccountId, int sourceCharacterId, string sourceCharacterName) {
            clientConnector.AdvertiseFriendInvite(invitedAccountId, sourceCharacterId, sourceCharacterName);
        }

        public override void AdvertiseFriendList(int accountId, FriendListNetworkData friendListNetworkData) {
            clientConnector.AdvertiseFriendList(accountId, friendListNetworkData);
        }

        public override void AdvertiseRenameCharacterInFriendList(int targetAccountId, int characterId, string newName) {
            clientConnector.AdvertiseRenameCharacterInFriendList(targetAccountId, characterId, newName);
        }

        public override void AdvertiseFriendStateChange(int targetAccountId, int playerCharacterId, CharacterSummaryNetworkData characterSummaryNetworkData) {
            clientConnector.AdvertiseFriendStateChange(targetAccountId, playerCharacterId, characterSummaryNetworkData);
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

        public override void AddAvailableDroppedLoot(int accountId, List<int> lootDropIds) {
            //Debug.Log($"FishNetNetworkController.AddAvailableDroppedLoot({accountId}, {items.Count})");

            clientConnector.AddAvailableDroppedLoot(accountId, lootDropIds);
        }

        public override void AddLootDrop(int accountId, int lootDropId, long itemInstanceId) {
            clientConnector.AddDroppedLoot(accountId, lootDropId, itemInstanceId);
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

        public override void UnloadScene(int sceneHandle) {
            //Debug.Log($"FishNetNetworkController.UnloadScene(sceneHandle: {sceneHandle})");

            // Manually trigger the unload to force the OnUnloadStart/End events
            SceneUnloadData sud = new SceneUnloadData(sceneHandle);
            fishNetNetworkManager.SceneManager.UnloadConnectionScenes(sud);
        }

        public override void LoadExistingScene(int accountId, int sceneHandle) {
            clientConnector.LoadExistingScene(accountId, sceneHandle);
        }

        public override void LoadNewScene(int accountId, int playerCharacterId, SceneInstanceType sceneInstanceType, SceneNode sceneNode) {
            clientConnector.LoadNewScene(accountId, playerCharacterId, sceneInstanceType, sceneNode);
        }

        public override void LoadNewLobbyGameScene(int accountId, LobbyGame lobbyGame, SceneNode sceneNode) {
            clientConnector.LoadNewLobbyGameScene(accountId, lobbyGame, sceneNode);
        }


        /*
        public override void SetCraftingManagerAbility(int accountId, string abilityName) {
           //Debug.Log($"FishNetNetworkController.SetCraftingManagerAbility({accountId}, {abilityName})");

            clientConnector.SetCraftingManagerAbility(accountId, abilityName);
        }
        */

        #endregion

    }
}
