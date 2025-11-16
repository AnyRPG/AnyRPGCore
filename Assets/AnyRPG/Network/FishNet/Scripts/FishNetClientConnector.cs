using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;

namespace AnyRPG {
    public class FishNetClientConnector : ConfiguredNetworkBehaviour {

        private FishNet.Managing.NetworkManager fishNetNetworkManager;

        // game manager references
        private SystemDataFactory systemDataFactory = null;
        private CharacterManager characterManager = null;
        private NetworkManagerServer networkManagerServer = null;
        private NetworkManagerClient networkManagerClient = null;
        private PlayerManagerServer playerManagerServer = null;
        private SaveManager saveManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log($"FishNetClientConnector.Configure(): instanceId: {GetInstanceID()}");

            base.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            systemDataFactory = systemGameManager.SystemDataFactory;
            characterManager = systemGameManager.CharacterManager;
            networkManagerServer = systemGameManager.NetworkManagerServer;
            networkManagerClient = systemGameManager.NetworkManagerClient;
            playerManagerServer = systemGameManager.PlayerManagerServer;
            saveManager = systemGameManager.SaveManager;
        }

        public void SetNetworkManager(FishNet.Managing.NetworkManager networkManager) {
            this.fishNetNetworkManager = networkManager;
        }

        public override void OnStartClient() {
            base.OnStartClient();
            //Debug.Log($"FishNetClientConnector.OnStartClient() ClientId: {networkManager.ClientManager.Connection.ClientId}");

            RequestServerTime();
            RequestSpawnRequest();
            networkManagerClient.ProcessStartClientConnector();
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestServerTime(NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.RequestServerTime()");

            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                return;
            }

            SetStartTime(networkConnection, networkManagerServer.GetServerStartTime());
        }

        [TargetRpc]
        public void SetStartTime(NetworkConnection networkConnection, DateTime serverTime) {
            //Debug.Log($"FishNetClientConnector.SetStartTime({serverTime})");

            networkManagerClient.SetStartTime(serverTime);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestSpawnRequest(NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.RequestSpawnRequest()");

            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                return;
            }
            //int accountId = networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId;

            networkManagerServer.RequestSpawnRequest(networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);
        }


        [ServerRpc(RequireOwnership = false)]
        public void RequestSpawnPlayerUnit(string sceneName, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.RequestSpawnPlayerUnit({sceneName}, {networkConnection.ClientId})");

            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                return;
            }
            int accountId = networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId;

            if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                if (networkManagerServer.LobbyGameAccountLookup.ContainsKey(accountId)) {
                    networkManagerServer.RequestSpawnLobbyGamePlayer(accountId, networkManagerServer.LobbyGameAccountLookup[accountId], sceneName);
                }
            } else if (networkManagerServer.ServerMode == NetworkServerMode.MMO) {
                networkManagerServer.RequestSpawnPlayer(accountId, sceneName);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestRespawnPlayerUnit(NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.RequestRespawnPlayerUnit()");

            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                return;
            }
            networkManagerServer.RequestRespawnPlayerUnit(networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestDespawnPlayerUnit(NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.RequestDespawnPlayerUnit()");

            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                return;
            }
            networkManagerServer.RequestDespawnPlayerUnit(networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestRevivePlayerUnit(NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.RequestRevivePlayerUnit()");

            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                return;
            }
            networkManagerServer.RequestRevivePlayerUnit(networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);
        }

        public void SpawnPlayer(int accountId, CharacterRequestData characterRequestData, Vector3 position, Vector3 forward, string sceneName) {
            //Debug.Log($"FishNetClientConnector.SpawnPlayer({accountId}, {characterRequestData.characterConfigurationRequest.unitProfile.ResourceName}, {position}, {forward}, {sceneName})");

            NetworkConnection networkConnection = fishNetNetworkManager.ServerManager.Clients[networkManagerServer.LoggedInAccounts[accountId].clientId];

            NetworkObject nob = GetSpawnablePrefab(characterRequestData.characterConfigurationRequest.unitProfile.UnitPrefabProps.NetworkUnitPrefab, null, position, forward);
            if (nob == null) {
                return;
            }

            FishNetUnitController networkCharacterUnit = nob.gameObject.GetComponent<FishNetUnitController>();
            if (networkCharacterUnit == null) {
                return;
            }

            // update syncvars
            networkCharacterUnit.unitProfileName.Value = characterRequestData.characterConfigurationRequest.unitProfile.ResourceName;
            //Debug.Log($"FishNetClientConnector.SpawnLobbyGamePlayer() setting characterName to {networkCharacterUnit.characterName.Value}");
            networkCharacterUnit.unitControllerMode.Value = UnitControllerMode.Player;
            networkCharacterUnit.characterId.Value = characterRequestData.characterId;

            UnitController unitController = nob.gameObject.GetComponent<UnitController>();
            if (unitController == null) {
                return;
            }
            unitController.SetCharacterRequestData(characterRequestData);
            networkManagerServer.MonitorPlayerUnit(accountId, unitController);

            SpawnPrefab(nob, networkConnection, GetConnectionScene(networkConnection, sceneName));
        }


        public void AdvertiseAddSpawnRequestServer(int accountId, SpawnPlayerRequest loadSceneRequest) {
            //Debug.Log($"FishNetClientConnector.AdvertiseAddSpawnRequestServer({accountId})");
            if (networkManagerServer.LoggedInAccounts.ContainsKey(accountId) == false) {
                return;
            }
            NetworkConnection networkConnection = fishNetNetworkManager.ServerManager.Clients[networkManagerServer.LoggedInAccounts[accountId].clientId];
            //Debug.Log($"FishNetClientConnector.AdvertiseAddSpawnRequestServer({accountId}) networkConnection.ClientId = {networkConnection.ClientId}");
            AdvertiseAddSpawnRequestClient(networkConnection, loadSceneRequest);
        }

        [TargetRpc]
        public void AdvertiseAddSpawnRequestClient(NetworkConnection networkConnection, SpawnPlayerRequest spawnPlayerRequest) {
            //Debug.Log($"FishNetClientConnector.AdvertiseAddSpawnRequestClient()");

            networkManagerClient.AdvertiseSpawnPlayerRequest(spawnPlayerRequest);
        }


        public Scene GetAccountScene(int accountId, string sceneName) {
            //Debug.Log($"FishNetClientConnector.GetAccountScene({accountId}, {sceneName})");

            if (networkManagerServer.LoggedInAccounts.ContainsKey(accountId) == false) {
                return default;
            }
            NetworkConnection networkConnection = fishNetNetworkManager.ServerManager.Clients[networkManagerServer.LoggedInAccounts[accountId].clientId];
            if (networkConnection == null) {
                return default;
            }
            return GetConnectionScene(networkConnection, sceneName);
        }

        public Scene GetConnectionScene(NetworkConnection networkConnection, string sceneName) {
            foreach (Scene scene in networkConnection.Scenes) {
                if (scene.name == sceneName) {
                    return scene;
                }
            }
            return default;
        }

        public UnitController SpawnCharacterUnit(CharacterRequestData characterRequestData, Transform parentTransform, Vector3 position, Vector3 forward, Scene scene) {
            //Debug.Log($"FishNetClientConnector.SpawnCharacterUnit({characterRequestData.characterConfigurationRequest.unitProfile.ResourceName})");

            UnitProfile unitProfile = systemDataFactory.GetResource<UnitProfile>(characterRequestData.characterConfigurationRequest.unitProfile.ResourceName);
            if (unitProfile == null) {
                return null;
            }
            NetworkObject networkPrefab = unitProfile.UnitPrefabProps.NetworkUnitPrefab.GetComponent<NetworkObject>();
            if (networkPrefab == null) {
                Debug.LogWarning($"Could not find NetworkObject component on {unitProfile.UnitPrefabProps.NetworkUnitPrefab.name}");
                return null;
            }

            NetworkObject nob = GetSpawnablePrefab(unitProfile.UnitPrefabProps.NetworkUnitPrefab, null, position, forward);
            // update syncvars
            FishNetUnitController networkCharacterUnit = nob.gameObject.GetComponent<FishNetUnitController>();
            if (networkCharacterUnit != null) {
                networkCharacterUnit.unitProfileName.Value = unitProfile.ResourceName;
                networkCharacterUnit.unitControllerMode.Value = characterRequestData.characterConfigurationRequest.unitControllerMode;
                networkCharacterUnit.characterId.Value = characterRequestData.characterId;
            }

            UnitController unitController = nob.gameObject.GetComponent<UnitController>();
            unitController.SetCharacterRequestData(characterRequestData);

            if (characterRequestData.characterConfigurationRequest.unitControllerMode == UnitControllerMode.Mount && characterRequestData.accountId != -1) {
                // if the request is for a mount, we need to determine if the mount is owned by a player and set ownership
                if (networkManagerServer.LoggedInAccounts.ContainsKey(characterRequestData.accountId)
                    && fishNetNetworkManager.ServerManager.Clients.ContainsKey(networkManagerServer.LoggedInAccounts[characterRequestData.accountId].clientId)) {
                    NetworkConnection networkConnection = fishNetNetworkManager.ServerManager.Clients[networkManagerServer.LoggedInAccounts[characterRequestData.accountId].clientId];
                    if (networkConnection != null) {
                        SpawnPrefab(nob, networkConnection, scene/*, parentTransform*/);
                    }
                }
            } else {
                SpawnPrefab(nob, null, scene);
            }

            return unitController;
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestSpawnModelPrefab(GameObject prefab, Transform parentTransform, Vector3 position, Vector3 forward, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.RequestSpawnModelPrefab({prefab.name}, {parentTransform.gameObject.name}, {position}, {forward})");

            NetworkObject nob = GetSpawnablePrefab(prefab, parentTransform, position, forward);
            SpawnPrefab(nob, networkConnection, default);
        }

        public void SpawnModelPrefabServer(GameObject prefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            //Debug.Log($"FishNetClientConnector.SpawnModelPrefabServer({prefab.name}{(parentTransform == null ? "null" : parentTransform.gameObject.name)}, {position}, {forward})");

            NetworkObject nob = GetSpawnablePrefab(prefab, parentTransform, position, forward);
            SpawnPrefab(nob, null, default);
        }

        private NetworkObject GetSpawnablePrefab(GameObject prefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            //Debug.Log($"FishNetClientConnector.SpawnPrefab({clientSpawnRequestId}, {prefab.name}, {position}, {forward})");

            NetworkObject networkPrefab = prefab.GetComponent<NetworkObject>();
            if (networkPrefab == null) {
                Debug.LogWarning($"Could not find NetworkObject component on {prefab.name}");
                return null;
            }

            NetworkObject nob = fishNetNetworkManager.GetPooledInstantiated(networkPrefab, position, Quaternion.LookRotation(forward), true);

            if (parentTransform != null) {
                NetworkObject nob2 = parentTransform.GetComponent<NetworkObject>();
                if (nob2 == null) {
                    //Debug.Log($"FishNetClientConnector.SpawnPrefab() could not find network object on {parentTransform.gameObject.name}");
                } else {
                    //Debug.Log($"FishNetClientConnector.SpawnPrefab() found a network object on {parentTransform.gameObject.name}");
                    nob.SetParent(nob2);
                }
            }

            //SpawnedNetworkObject spawnedNetworkObject = nob.gameObject.GetComponent<SpawnedNetworkObject>();
            /*
            if (spawnedNetworkObject != null) {
                //Debug.Log($"FishNetClientConnector.SpawnPrefab({clientSpawnRequestId}, {prefab.name}) setting spawnRequestId on gameobject");
                spawnedNetworkObject.clientSpawnRequestId.Value = clientSpawnRequestId;
                spawnedNetworkObject.serverSpawnRequestId.Value = serverSpawnRequestId;
            }
            */

            return nob;
        }

        /*
        private void SetNetworkObjectParent(NetworkObject networkObject, Transform parentTransform) {
            Debug.Log($"FishNetClientConnector.SetNetworkObjectParent({networkObject.gameObject.name}, {(parentTransform == null ? "null" : parentTransform.gameObject.name)})");

            if (parentTransform != null) {
                NetworkObject networkObjectParent = parentTransform.GetComponent<NetworkObject>();
                if (networkObjectParent == null) {
                    //Debug.Log($"FishNetClientConnector.SpawnPrefab() could not find network object on {parentTransform.gameObject.name}");
                } else {
                    //Debug.Log($"FishNetClientConnector.SpawnPrefab() found a network object on {parentTransform.gameObject.name}");
                    networkObject.SetParent(networkObjectParent);
                }
            }
        }
        */

        /*
        private void SpawnScenePrefab(NetworkObject nob, Scene scene) {
            //Debug.Log($"FishNetNetworkController.SpawnPlayer() Spawning player at {position}");
            fishNetNetworkManager.ServerManager.Spawn(nob, null, scene);
        }
        */

        private void SpawnPrefab(NetworkObject networkObject, NetworkConnection networkConnection, Scene scene) {
            //Debug.Log($"FishNetClientConnector.SpawnPrefab({networkObject.gameObject.name}, {scene.name}({scene.handle}))");

            //fishNetNetworkManager.ServerManager.Spawn(networkObject, null, scene);
            fishNetNetworkManager.ServerManager.Spawn(networkObject, networkConnection, scene);
            /*
            if (parentTransform != null) {
                SetNetworkObjectParent(networkObject, parentTransform);
            }
            */
            /*
            if (networkConnection != null) {
                Debug.Log($"FishNetClientConnector.SpawnPrefab({networkObject.gameObject.name}, {scene.name}({scene.handle}), {(parentTransform == null ? "null" : parentTransform.gameObject.name)}) give ownership to {networkConnection.ClientId}");
                networkObject.GiveOwnership(networkConnection);
            }
            */
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestReturnFromCutscene(NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.LoadSceneServer({networkConnection.ClientId}, {sceneName})");

            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                Debug.LogWarning($"FishNetClientConnector.CreatePlayerCharacter() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }

            networkManagerServer.ReturnFromCutscene(networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestCreatePlayerCharacter(AnyRPGSaveData saveData, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.RequestCreatePlayerCharacter()");

            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                Debug.LogWarning($"FishNetClientConnector.CreatePlayerCharacter() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }
            
            networkManagerServer.RequestCreatePlayerCharacter(networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId, saveData);
        }

        /*
        public void AdvertiseCreatePlayerCharacter(int accountId) {
            //Debug.Log($"FishNetClientConnector.HandleCreatePlayerCharacter({accountId})");

            //LoadCharacterList(networkManager.ServerManager.Clients[accountId]);
            networkManagerServer.LoadCharacterList(accountId);
        }
        */


        [ServerRpc(RequireOwnership = false)]
        public void DeletePlayerCharacter(int playerCharacterId, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.DeletePlayerCharacter({playerCharacterId})");

            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                Debug.LogWarning($"FishNetClientConnector.DeletePlayerCharacter() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }

            networkManagerServer.DeletePlayerCharacter(networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId, playerCharacterId);

            // now that character is deleted, just load the character list
            //LoadCharacterList(networkConnection);
        }

        public void AdvertiseDeletePlayerCharacter(int accountId) {
            //Debug.Log($"FishNetClientConnector.HandleDeletePlayerCharacter({accountId})");

            networkManagerServer.LoadCharacterList(accountId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void ToggleLobbyGameReadyStatus(int gameId, NetworkConnection networkConnection = null) {
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                Debug.LogWarning($"FishNetClientConnector.ToggleLobbyGameReadyStatus() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }
            networkManagerServer.ToggleLobbyGameReadyStatus(gameId, networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);
        }


        /*
        [TargetRpc]
        public void LoadCharacterList(NetworkConnection networkConnection, List<PlayerCharacterSaveData> playerCharacterSaveDataList) {
            Debug.Log($"FishNetClientConnector.SetCharacterList({playerCharacterSaveDataList.Count})");

            systemGameManager.LoadGameManager.SetCharacterList(playerCharacterSaveDataList);
        }
        */

        [ServerRpc(RequireOwnership = false)]
        public void LoadCharacterList(NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.LoadCharacterList()");
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                //Debug.LogWarning($"FishNetClientConnector.LoadCharacterList() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }
            networkManagerServer.LoadCharacterList(networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);
        }

        public void AdvertiseLoadCharacterList(int accountId, List<PlayerCharacterSaveData> playerCharacterSaveDataList) {
            //Debug.Log($"FishNetClientConnector.HandleLoadCharacterList({accountId})");

            if (networkManagerServer.LoggedInAccounts.ContainsKey(accountId) == false) {
                return;
            }
            int clientId = networkManagerServer.LoggedInAccounts[accountId].clientId;
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                return;
            }

            SetCharacterList(fishNetNetworkManager.ServerManager.Clients[clientId], playerCharacterSaveDataList);
        }

        [TargetRpc]
        public void SetCharacterList(NetworkConnection networkConnection, List<PlayerCharacterSaveData> playerCharacterSaveDataList) {
            //Debug.Log($"FishNetClientConnector.SetCharacterList({playerCharacterSaveDataList.Count})");

            networkManagerClient.SetCharacterList(playerCharacterSaveDataList);
        }


        [ServerRpc(RequireOwnership = false)]
        public void RequestLobbyGameList(NetworkConnection networkConnection = null) {
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                //Debug.LogWarning($"FishNetClientConnector.RequestLobbyGameList() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }
            networkManagerServer.RequestLobbyGameList(networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestLobbyPlayerList(NetworkConnection networkConnection = null) {
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                //Debug.LogWarning($"FishNetClientConnector.RequestLobbyPlayerList() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }
            networkManagerServer.RequestLobbyPlayerList(networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SendLobbyChatMessage(string messageText, NetworkConnection networkConnection = null) {
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                //Debug.LogWarning($"FishNetClientConnector.SendLobbyChatMessage() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }
            networkManagerServer.SendLobbyChatMessage(messageText, networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SendLobbyGameChatMessage(string messageText, int gameId, NetworkConnection networkConnection = null) {
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                //Debug.LogWarning($"FishNetClientConnector.SendLobbyGameChatMessage() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }
            networkManagerServer.SendLobbyGameChatMessage(messageText, networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId, gameId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SendSceneChatMessage(string messageText, NetworkConnection networkConnection = null) {
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                //Debug.LogWarning($"FishNetClientConnector.SendSceneChatMessage() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }
            networkManagerServer.SendSceneChatMessage(messageText, networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);
        }


        [ServerRpc(RequireOwnership = false)]
        public void ChooseLobbyGameCharacter(string unitProfileName, int gameId, string appearanceString, List<SwappableMeshSaveData> swappableMeshSaveData, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.ChooseLobbyGameCharacter({unitProfileName}, {gameId})");

            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                Debug.LogWarning($"FishNetClientConnector.ChooseLobbyGameCharacter({unitProfileName}, {gameId}) could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }
            networkManagerServer.ChooseLobbyGameCharacter(gameId, networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId, unitProfileName, appearanceString, swappableMeshSaveData);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestStartLobbyGame(int gameId, NetworkConnection networkConnection = null) {
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                //Debug.LogWarning($"FishNetClientConnector.RequestStartLobbyGame() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }
            networkManagerServer.RequestStartLobbyGame(gameId, networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestJoinLobbyGameInProgress(int gameId, NetworkConnection networkConnection = null) {
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                //Debug.LogWarning($"FishNetClientConnector.RequestJoinLobbyGameInProgress() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }
            networkManagerServer.RequestJoinLobbyGameInProgress(gameId, networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);
        }

        public override void OnStartNetwork() {
            base.OnStartNetwork();
            //Debug.Log($"FishNetClientConnector.OnStartNetwork()");

            FishNetNetworkController fishNetNetworkController = GameObject.FindAnyObjectByType<FishNetNetworkController>();
            fishNetNetworkController.RegisterConnector(this);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestCreateLobbyGame(string sceneResourceName, bool allowLateJoin, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.CreateLobbyGame()");
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                //Debug.LogWarning($"FishNetClientConnector.CreateLobbyGame() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }
            networkManagerServer.CreateLobbyGame(sceneResourceName, networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId, allowLateJoin);
        }


        [ObserversRpc]
        public void AdvertiseCreateLobbyGame(LobbyGame lobbyGame) {
            networkManagerClient.AdvertiseCreateLobbyGame(lobbyGame);
        }

        [ServerRpc(RequireOwnership = false)]
        public void CancelLobbyGame(int gameId, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.CancelLobbyGame()");
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                //Debug.LogWarning($"FishNetClientConnector.CancelLobbyGame() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }
            networkManagerServer.CancelLobbyGame(networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId, gameId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void JoinLobbyGame(int gameId, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.JoinLobbyGame()");
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                //Debug.LogWarning($"FishNetClientConnector.JoinLobbyGame() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }
            networkManagerServer.JoinLobbyGame(gameId, networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);
        }

        [ObserversRpc]
        public void AdvertiseLobbyLogin(int accountId, string userName) {
            Debug.Log($"FishNetClientConnector.AdvertiseLobbyLogin({accountId}, {userName})");

            networkManagerClient.AdvertiseLobbyLogin(accountId, userName);
        }

        public void SendLobbyGameList(int accountId, List<LobbyGame> lobbyGames) {
            if (networkManagerServer.LoggedInAccounts.ContainsKey(accountId) == false) {
                return;
            }
            int clientId = networkManagerServer.LoggedInAccounts[accountId].clientId;
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                return;
            }
            SetLobbyGameList(fishNetNetworkManager.ServerManager.Clients[clientId], lobbyGames);
        }

        [TargetRpc]
        public void SetLobbyGameList(NetworkConnection networkConnection, List<LobbyGame> lobbyGames) {
            networkManagerClient.SetLobbyGameList(lobbyGames);
        }

        public void SendLobbyPlayerList(int accountId, Dictionary<int, string> lobbyPlayers) {
            if (networkManagerServer.LoggedInAccounts.ContainsKey(accountId) == false) {
                return;
            }
            int clientId = networkManagerServer.LoggedInAccounts[accountId].clientId;
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                return;
            }
            SetLobbyPlayerList(fishNetNetworkManager.ServerManager.Clients[clientId], lobbyPlayers);
        }

        [TargetRpc]
        public void SetLobbyPlayerList(NetworkConnection networkConnection, Dictionary<int, string> lobbyPlayers) {
            networkManagerClient.SetLobbyPlayerList(lobbyPlayers);
        }

        public void JoinLobbyGameInProgress(int gameId, int accountId, string sceneResourceName) {
            //Debug.Log($"FishNetClientConnector.JoinLobbyGameInProgress({gameId}, {accountId})");

            if (networkManagerServer.LoggedInAccounts.ContainsKey(accountId) == false) {
                return;
            }
            int clientId = networkManagerServer.LoggedInAccounts[accountId].clientId;
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                return;
            }
            NetworkConnection networkConnection = fishNetNetworkManager.ServerManager.Clients[clientId];
            LobbyGame lobbyGame = networkManagerServer.LobbyGames[gameId];

            // first try the scene resource name provided, then fallback to the lobby game default scene resource name
            SceneNode loadingSceneNode = systemDataFactory.GetResource<SceneNode>(sceneResourceName);
            if (loadingSceneNode == null) {
                loadingSceneNode = systemDataFactory.GetResource<SceneNode>(lobbyGame.sceneResourceName);
                if (loadingSceneNode == null) {
                    return;
                }
            }

            AdvertiseJoinLobbyGameInProgress(networkConnection, gameId);

            LoadLobbyGameScene(lobbyGame, loadingSceneNode, networkConnection);
        }

        public void JoinMMOGameInProgress(int accountId, string sceneResourceName) {
            if (networkManagerServer.LoggedInAccounts.ContainsKey(accountId) == false) {
                return;
            }
            int clientId = networkManagerServer.LoggedInAccounts[accountId].clientId;
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                return;
            }
            NetworkConnection networkConnection = fishNetNetworkManager.ServerManager.Clients[clientId];
            SceneNode loadingSceneNode = systemDataFactory.GetResource<SceneNode>(sceneResourceName);
            if (loadingSceneNode == null) {
                return;
            }

            AdvertiseJoinMMOGameInProgress(networkConnection);

            LoadMMOGameScene(loadingSceneNode, networkConnection);
        }


        public void LoadMMOGameScene(SceneNode sceneNode, NetworkConnection networkConnection) {
            //Debug.Log($"FishNetClientConnector.LoadMMOGameScene({sceneNode.SceneFile}, {networkConnection.ClientId}");

                // load new scene
                SceneLoadData sceneLoadData = new SceneLoadData(sceneNode.SceneFile);
                sceneLoadData.ReplaceScenes = ReplaceOption.All;
                sceneLoadData.Options.LocalPhysics = LocalPhysicsMode.Physics3D;
                sceneLoadData.Options.AllowStacking = false;
                sceneLoadData.PreferredActiveScene = new PreferredScene(SceneLookupData.CreateData(sceneNode.SceneFile));
                fishNetNetworkManager.SceneManager.LoadConnectionScenes(networkConnection, sceneLoadData);
        }

        public void LoadLobbyGameScene(LobbyGame lobbyGame, SceneNode sceneNode, NetworkConnection networkConnection) {
            //Debug.Log($"FishNetClientConnector.LoadLobbyGameScene({lobbyGame.gameId}, {sceneNode.SceneFile}, {networkConnection.ClientId}");

            if (networkManagerServer.LobbyGameSceneHandles.ContainsKey(lobbyGame.gameId) == false || networkManagerServer.LobbyGameSceneHandles[lobbyGame.gameId].ContainsKey(sceneNode.SceneFile) == false) {
                // load new scene
                SceneLoadData sceneLoadData = new SceneLoadData(sceneNode.SceneFile);
                sceneLoadData.ReplaceScenes = ReplaceOption.All;
                sceneLoadData.Options.LocalPhysics = LocalPhysicsMode.Physics3D;
                sceneLoadData.Options.AllowStacking = true;
                sceneLoadData.PreferredActiveScene = new PreferredScene(SceneLookupData.CreateData(lobbyGame.sceneResourceName));
                networkManagerServer.SetLobbyGameLoadRequestHashcode(lobbyGame.gameId, sceneLoadData.GetHashCode());
                //Debug.Log($"FishNetClientConnector.LoadLobbyGameScene({lobbyGame.gameId}) sceneloadDataHashCode {sceneLoadData.GetHashCode()}");

                fishNetNetworkManager.SceneManager.LoadConnectionScenes(networkConnection, sceneLoadData);
            } else {
                // load existing scene
                SceneLoadData sceneLoadData = new(networkManagerServer.LobbyGameSceneHandles[lobbyGame.gameId][sceneNode.SceneFile]);
                sceneLoadData.ReplaceScenes = ReplaceOption.All;
                sceneLoadData.Options.LocalPhysics = LocalPhysicsMode.Physics3D;
                sceneLoadData.Options.AllowStacking = true;
                sceneLoadData.PreferredActiveScene = new PreferredScene(SceneLookupData.CreateData(sceneNode.SceneFile));

                fishNetNetworkManager.SceneManager.LoadConnectionScenes(networkConnection, sceneLoadData);
            }
        }

        public void StartLobbyGame(int gameId) {
            //Debug.Log($"FishNetClientConnector.StartLobbyGame({gameId})");

            NetworkConnection[] networkConnections = new NetworkConnection[networkManagerServer.LobbyGames[gameId].PlayerList.Keys.Count];
            //Debug.Log($"FishNetClientConnector.StartLobbyGame() networkConnections.Length = {networkConnections.Length}");
            LobbyGame lobbyGame = networkManagerServer.LobbyGames[gameId];
            SceneNode loadingSceneNode = systemDataFactory.GetResource<SceneNode>(lobbyGame.sceneResourceName);
            if (loadingSceneNode == null) {
                return;
            }

            int i = 0;
            int clientId = -1;
            foreach (int accountId in networkManagerServer.LobbyGames[gameId].PlayerList.Keys) {
                if (networkManagerServer.LoggedInAccounts.ContainsKey(accountId) == false) {
                    continue;
                }
                clientId = networkManagerServer.LoggedInAccounts[accountId].clientId;
                if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                    continue;
                }
                networkConnections[i] = fishNetNetworkManager.ServerManager.Clients[clientId];
                //Debug.Log($"FishNetClientConnector.StartLobbyGame() adding client {clientId} to networkConnections[{i}] for game {gameId}");
                i++;
            }

            AdvertiseStartLobbyGame(gameId);

            SceneLoadData sceneLoadData = new SceneLoadData(loadingSceneNode.SceneFile);
            sceneLoadData.ReplaceScenes = ReplaceOption.All;
            sceneLoadData.Options.LocalPhysics = LocalPhysicsMode.Physics3D;
            sceneLoadData.Options.AllowStacking = true;
            sceneLoadData.PreferredActiveScene = new PreferredScene(SceneLookupData.CreateData(loadingSceneNode.SceneFile));
            networkManagerServer.SetLobbyGameLoadRequestHashcode(gameId, sceneLoadData.GetHashCode());
            //Debug.Log($"FishNetClientConnector.StartLobbyGame({gameId}) sceneloadDataHashCode {sceneLoadData.GetHashCode()}");

            fishNetNetworkManager.SceneManager.LoadConnectionScenes(networkConnections, sceneLoadData);
        }

        [TargetRpc]
        public void AdvertiseJoinLobbyGameInProgress(NetworkConnection networkConnection, int gameId) {
            networkManagerClient.AdvertiseJoinLobbyGameInProgress(gameId);
        }

        [TargetRpc]
        public void AdvertiseJoinMMOGameInProgress(NetworkConnection networkConnection) {
            networkManagerClient.AdvertiseJoinMMOGameInProgress();
        }


        [ObserversRpc]
        public void AdvertiseStartLobbyGame(int gameId) {
            //Debug.Log($"FishNetClientConnector.AdvertiseStartLobbyGame({gameId})");

            networkManagerClient.AdvertiseStartLobbyGame(gameId);
        }

        [ObserversRpc]
        public void AdvertiseChooseLobbyGameCharacter(int gameId, int accountId, string unitProfileName) {
            //Debug.Log($"FishNetClientConnector.AdvertiseChooseLobbyGameCharacter({gameId}, {accountId}, {unitProfileName})");

            networkManagerClient.AdvertiseChooseLobbyGameCharacter(gameId, accountId, unitProfileName);
        }

        [ObserversRpc]
        public void AdvertiseLobbyLogout(int accountId) {
            networkManagerClient.AdvertiseLobbyLogout(accountId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void LeaveLobbyGame(int gameId, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.LeaveLobbyGame()");
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                //Debug.LogWarning($"FishNetClientConnector.LeaveLobbyGame() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }
            networkManagerServer.LeaveLobbyGame(gameId, networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);
        }

        [ObserversRpc]
        public void AdvertiseCancelLobbyGame(int gameId) {
            networkManagerClient.AdvertiseCancelLobbyGame(gameId);
        }

        [ObserversRpc]
        public void AdvertiseAccountJoinLobbyGame(int gameId, int accountId, string userName) {
            networkManagerClient.AdvertiseAccountJoinLobbyGame(gameId, accountId, userName);
        }

        [ObserversRpc]
        public void AdvertiseAccountLeaveLobbyGame(int gameId, int accountId) {
            networkManagerClient.AdvertiseAccountLeaveLobbyGame(gameId, accountId);
        }

        [ObserversRpc]
        public void AdvertiseSendLobbyChatMessage(string messageText) {
            networkManagerClient.AdvertiseSendLobbyChatMessage(messageText);
        }

        [ObserversRpc]
        public void AdvertiseSendLobbyGameChatMessage(string messageText, int gameId) {
            networkManagerClient.AdvertiseSendLobbyGameChatMessage(messageText, gameId);
        }

        [ObserversRpc]
        public void AdvertiseSendSceneChatMessage(string messageText, int accountId) {
            networkManagerClient.AdvertiseSendSceneChatMessage(messageText, accountId);
        }

        [ObserversRpc]
        public void AdvertiseSetLobbyGameReadyStatus(int gameId, int accountId, bool ready) {
            networkManagerClient.AdvertiseSetLobbyGameReadyStatus(gameId, accountId, ready);
        }

        public void AdvertiseLoadSceneServer(string sceneResourceName, int accountId) {
            //Debug.Log($"FishNetClientConnector.AdvertiseLoadSceneServer({sceneResourceName}, {accountId})");

            if (networkManagerServer.LoggedInAccounts.ContainsKey(accountId) == false) {
                //Debug.Log($"FishNetClientConnector.AdvertiseLoadSceneServer() could not find client id {accountId}");
                return;
            }
            int clientId = networkManagerServer.LoggedInAccounts[accountId].clientId;
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                return;
            }

            // unload the current scene for the client
            NetworkConnection networkConnection = fishNetNetworkManager.ServerManager.Clients[clientId];

            // this code works only for lobby game.  It will need to be modified to work with MMO
            AdvertiseLoadSceneClient(networkConnection, sceneResourceName);

            if (networkConnection.Scenes.Count == 0) {
                //Debug.Log($"FishNetClientConnector.AdvertiseLoadSceneServer() no scenes found for client {clientId}");
                //return;
            } else {
                //Debug.Log($"FishNetClientConnector.AdvertiseLoadSceneServer() unloading current scene {networkConnection.Scenes.First().name}({networkConnection.Scenes.First().handle}) for client {clientId}");
                SceneUnloadData sceneUnloadData = new SceneUnloadData(networkConnection.Scenes.First());
                base.NetworkManager.SceneManager.UnloadConnectionScenes(networkConnection, sceneUnloadData);
            }

            SceneNode loadingSceneNode = systemDataFactory.GetResource<SceneNode>(sceneResourceName);
            if (loadingSceneNode == null) {
                return;
            }

            if (networkManagerServer.ServerMode == NetworkServerMode.MMO) {
                LoadMMOGameScene(loadingSceneNode, networkConnection);
            } else if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                LobbyGame lobbyGame = networkManagerServer.LobbyGames[networkManagerServer.LobbyGameAccountLookup[accountId]];
                LoadLobbyGameScene(lobbyGame, loadingSceneNode, networkConnection);
            }
        }

        [TargetRpc]
        public void AdvertiseLoadSceneClient(NetworkConnection networkConnection, string sceneName) {
            //Debug.Log($"FishNetClientConnector.AdvertiseLoadSceneClient({sceneName})");

            networkManagerClient.AdvertiseLoadSceneClient(sceneName);
        }

        public void ReturnObjectToPool(GameObject returnedObject) {
            //Debug.Log($"FishNetClientConnector.ReturnObjectToPool({returnedObject.name})");

            fishNetNetworkManager.ServerManager.Despawn(returnedObject);
        }

        public void InteractWithOptionClient(UnitController sourceUnitController, Interactable targetInteractable, int componentIndex, int choiceIndex) {
            FishNetUnitController networkCharacterUnit = null;
            if (sourceUnitController != null) {
                networkCharacterUnit = sourceUnitController.GetComponent<FishNetUnitController>();
            }
            FishNetInteractable networkInteractable = null;
            if (targetInteractable != null) {
                networkInteractable = targetInteractable.GetComponent<FishNetInteractable>();
            }
            InteractWithOptionServer(networkCharacterUnit, networkInteractable, componentIndex, choiceIndex);
        }

        [ServerRpc(RequireOwnership = false)]
        public void InteractWithOptionServer(FishNetUnitController sourceNetworkCharacterUnit, FishNetInteractable targetNetworkInteractable, int componentIndex, int choiceIndex) {
            //Debug.Log($"FishNetClientConnector.InteractWithOptionServer({sourceNetworkCharacterUnit?.gameObject.name}, {targetNetworkInteractable?.gameObject.name}, {componentIndex}, {choiceIndex})");

            UnitController sourceUnitController = null;
            if (sourceNetworkCharacterUnit != null) {
                sourceUnitController = sourceNetworkCharacterUnit.UnitController;
            }
            Interactable interactable = null;
            if (targetNetworkInteractable != null) {
                interactable = targetNetworkInteractable.Interactable;
            }
            networkManagerServer.InteractWithOption(sourceUnitController, interactable, componentIndex, choiceIndex);
        }

        /*
        public void AdvertiseAddSpawnRequestServer(int accountId, SpawnPlayerRequest loadSceneRequest) {
            if (networkManagerServer.LoggedInAccounts.ContainsKey(accountId) == false) {
                Debug.Log($"FishNetClientConnector.AdvertiseAddSpawnRequestServer() could not find client id {accountId}");
                return;
            }
            int clientId = networkManagerServer.LoggedInAccounts[accountId].clientId;
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId)) {
                AdvertiseAddSpawnRequestClient(fishNetNetworkManager.ServerManager.Clients[clientId], loadSceneRequest);
            }
        }
        */

        /*
        [TargetRpc]
        public void AdvertiseAddSpawnRequestClient(NetworkConnection networkConnection, SpawnPlayerRequest loadSceneRequest) {
            networkManagerClient.AdvertiseAddSpawnRequest(loadSceneRequest);
        }
        */

        public void RequestTurnInDialog(Interactable interactable, int componentIndex, Dialog dialog) {
            FishNetInteractable networkInteractable = null;
            if (interactable != null) {
                networkInteractable = interactable.GetComponent<FishNetInteractable>();
            }
            RequestTurnInDialogServer(networkInteractable, componentIndex, dialog.ResourceName);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestTurnInDialogServer(FishNetInteractable targetNetworkInteractable, int componentIndex, string dialogResourceName, NetworkConnection networkConnection = null) {
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                Debug.LogWarning($"FishNetClientConnector.RequestTurnInDialogServer() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }

            Dialog dialog = systemDataFactory.GetResource<Dialog>(dialogResourceName);
            if (dialog == null) {
                return;
            }

            Interactable interactable = null;
            if (targetNetworkInteractable != null) {
                interactable = targetNetworkInteractable.Interactable;
            }

            networkManagerServer.TurnInDialog(interactable, componentIndex, dialog, networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);
        }

        public void RequestTurnInQuestDialog(Dialog dialog) {
            RequestTurnInQuestDialogServer(dialog.ResourceName);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestTurnInQuestDialogServer(string dialogResourceName, NetworkConnection networkConnection = null) {
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                Debug.LogWarning($"FishNetClientConnector.RequestTurnInDialogServer() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }

            Dialog dialog = systemDataFactory.GetResource<Dialog>(dialogResourceName);
            if (dialog == null) {
                return;
            }

            networkManagerServer.TurnInQuestDialog(dialog, networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);
        }



        public void RequestSetPlayerCharacterClass(Interactable interactable, int componentIndex) {
            FishNetInteractable networkInteractable = null;
            if (interactable != null) {
                networkInteractable = interactable.GetComponent<FishNetInteractable>();
            }
            RequestSetPlayerCharacterClassServer(networkInteractable, componentIndex);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestSetPlayerCharacterClassServer(FishNetInteractable targetNetworkInteractable, int componentIndex, NetworkConnection networkConnection = null) {
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                Debug.LogWarning($"FishNetClientConnector.RequestSetPlayerCharacterClassServer() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }

            Interactable interactable = null;
            if (targetNetworkInteractable != null) {
                interactable = targetNetworkInteractable.Interactable;
            }
            networkManagerServer.SetPlayerCharacterClass(interactable, componentIndex, networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);
        }

        public void RequestSetPlayerCharacterSpecialization(Interactable interactable, int componentIndex) {
            FishNetInteractable networkInteractable = null;
            if (interactable != null) {
                networkInteractable = interactable.GetComponent<FishNetInteractable>();
            }
            RequestSetPlayerCharacterSpecializationServer(networkInteractable, componentIndex);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestSetPlayerCharacterSpecializationServer(FishNetInteractable targetNetworkInteractable, int componentIndex, NetworkConnection networkConnection = null) {
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                Debug.LogWarning($"FishNetClientConnector.Specialization() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }

            Interactable interactable = null;
            if (targetNetworkInteractable != null) {
                interactable = targetNetworkInteractable.Interactable;
            }
            networkManagerServer.SetPlayerCharacterSpecialization(interactable, componentIndex, networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);
        }

        public void RequestSetPlayerFaction(Interactable interactable, int componentIndex) {
            FishNetInteractable networkInteractable = null;
            if (interactable != null) {
                networkInteractable = interactable.GetComponent<FishNetInteractable>();
            }
            RequestSetPlayerFactionServer(networkInteractable, componentIndex);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestSetPlayerFactionServer(FishNetInteractable targetNetworkInteractable, int componentIndex, NetworkConnection networkConnection = null) {
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                Debug.LogWarning($"FishNetClientConnector.BuyItemFromVendorServer() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }

            Interactable interactable = null;
            if (targetNetworkInteractable != null) {
                interactable = targetNetworkInteractable.Interactable;
            }
            networkManagerServer.SetPlayerFaction(interactable, componentIndex, networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);
        }

        public void RequestLearnSkill(Interactable interactable, int componentIndex, int skillId) {
            FishNetInteractable networkInteractable = null;
            if (interactable != null) {
                networkInteractable = interactable.GetComponent<FishNetInteractable>();
            }
            RequestLearnSkillServer(networkInteractable, componentIndex, skillId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestLearnSkillServer(FishNetInteractable targetNetworkInteractable, int componentIndex, int skillId, NetworkConnection networkConnection = null) {
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                Debug.LogWarning($"FishNetClientConnector.BuyItemFromVendorServer() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }

            Interactable interactable = null;
            if (targetNetworkInteractable != null) {
                interactable = targetNetworkInteractable.Interactable;
            }
            networkManagerServer.LearnSkill(interactable, componentIndex, skillId, networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);
        }

        public void RequestAcceptQuest(Interactable interactable, int componentIndex, Quest quest) {
            FishNetInteractable networkInteractable = null;
            if (interactable != null) {
                networkInteractable = interactable.GetComponent<FishNetInteractable>();
            }
            RequestAcceptQuestServer(networkInteractable, componentIndex, quest.ResourceName);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestAcceptQuestServer(FishNetInteractable targetNetworkInteractable, int componentIndex, string questResourceName, NetworkConnection networkConnection = null) {
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                Debug.LogWarning($"FishNetClientConnector.BuyItemFromVendorServer() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }

            Quest quest = systemDataFactory.GetResource<Quest>(questResourceName);
            if (quest == null) {
                return;
            }

            Interactable interactable = null;
            if (targetNetworkInteractable != null) {
                interactable = targetNetworkInteractable.Interactable;
            }

            networkManagerServer.AcceptQuest(interactable, componentIndex, quest, networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);
        }

        public void RequestCompleteQuest(Interactable interactable, int componentIndex, Quest quest, QuestRewardChoices questRewardChoices) {
            FishNetInteractable networkInteractable = null;
            if (interactable != null) {
                networkInteractable = interactable.GetComponent<FishNetInteractable>();
            }
            RequestCompleteQuestServer(networkInteractable, componentIndex, quest.ResourceName, questRewardChoices);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestCompleteQuestServer(FishNetInteractable targetNetworkInteractable, int componentIndex, string questResourceName, QuestRewardChoices questRewardChoices, NetworkConnection networkConnection = null) {
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                Debug.LogWarning($"FishNetClientConnector.BuyItemFromVendorServer() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }

            Quest quest = systemDataFactory.GetResource<Quest>(questResourceName);
            if (quest == null) {
                return;
            }

            Interactable interactable = null;
            if (targetNetworkInteractable != null) {
                interactable = targetNetworkInteractable.Interactable;
            }

            networkManagerServer.CompleteQuest(interactable, componentIndex, quest, questRewardChoices, networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);
        }

        public void SellVendorItemClient(Interactable interactable, int componentIndex, int itemInstanceId) {
            FishNetInteractable networkInteractable = null;
            if (interactable != null) {
                networkInteractable = interactable.GetComponent<FishNetInteractable>();
            }
            SellVendorItemServer(networkInteractable, componentIndex, itemInstanceId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SellVendorItemServer(FishNetInteractable targetNetworkInteractable, int componentIndex, int itemInstanceId, NetworkConnection networkConnection = null) {
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                Debug.LogWarning($"FishNetClientConnector.SellVendorItemServer() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }
            Interactable interactable = null;
            if (targetNetworkInteractable != null) {
                interactable = targetNetworkInteractable.Interactable;
            }
            networkManagerServer.SellVendorItem(interactable, componentIndex, itemInstanceId, networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);
        }

        public void RequestSpawnUnit(Interactable interactable, int componentIndex, int unitLevel, int extraLevels, bool useDynamicLevel, string unitProfileName, string unitToughnessName) {
            //Debug.Log($"FishNetClientConnector.RequestSpawnUnit({interactable.gameObject.name}, {componentIndex}, {unitLevel}, {extraLevels}, {useDynamicLevel}, {unitProfileName}, {unitToughnessName})");

            FishNetInteractable networkInteractable = null;
            if (interactable != null) {
                networkInteractable = interactable.GetComponent<FishNetInteractable>();
            }
            RequestSpawnUnitServer(networkInteractable, componentIndex, unitLevel, extraLevels, useDynamicLevel, unitProfileName, unitToughnessName);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestSpawnUnitServer(FishNetInteractable targetNetworkInteractable, int componentIndex, int unitLevel, int extraLevels, bool useDynamicLevel, string unitProfileName, string unitToughnessName, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.RequestSpawnUnitServer({componentIndex}, {unitLevel}, {extraLevels}, {useDynamicLevel}, {unitProfileName}, {unitToughnessName})");

            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                Debug.LogWarning($"FishNetClientConnector.RequestSpawnUnitServer() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }
            Interactable interactable = null;
            if (targetNetworkInteractable != null) {
                interactable = targetNetworkInteractable.Interactable;
            }
            UnitProfile unitProfile = systemDataFactory.GetResource<UnitProfile>(unitProfileName);
            if (unitProfile == null) {
                return;
            }
            UnitToughness unitToughness = systemDataFactory.GetResource<UnitToughness>(unitToughnessName);
            networkManagerServer.RequestSpawnUnit(interactable, componentIndex, unitLevel, extraLevels, useDynamicLevel, unitProfile, unitToughness, networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);
        }

        public void BuyItemFromVendor(Interactable interactable, int componentIndex, int collectionIndex, int itemIndex, string resourceName) {
            FishNetInteractable networkInteractable = null;
            if (interactable != null) {
                networkInteractable = interactable.GetComponent<FishNetInteractable>();
            }
            BuyItemFromVendorServer(networkInteractable, componentIndex, collectionIndex, itemIndex, resourceName);
        }

        [ServerRpc(RequireOwnership = false)]
        public void BuyItemFromVendorServer(FishNetInteractable targetNetworkInteractable, int componentIndex, int collectionIndex, int itemIndex, string resourceName, NetworkConnection networkConnection = null) {
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                Debug.LogWarning($"FishNetClientConnector.BuyItemFromVendorServer() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }

            Interactable interactable = null;
            if (targetNetworkInteractable != null) {
                interactable = targetNetworkInteractable.Interactable;
            }
            networkManagerServer.BuyItemFromVendor(interactable, componentIndex, collectionIndex, itemIndex, resourceName, networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);
        }


        public void AdvertiseMessageFeedMessage(int accountId, string message) {
            Debug.Log($"FishNetClientConnector.AdvertiseMessageFeedMessage({accountId}, {message})");
            if (networkManagerServer.LoggedInAccounts.ContainsKey(accountId) == false) {
                return;
            }
            int clientId = networkManagerServer.LoggedInAccounts[accountId].clientId;
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                return;
            }
            AdvertiseMessageFeedMessageClient(fishNetNetworkManager.ServerManager.Clients[clientId], message);
        }

        [TargetRpc]
        public void AdvertiseMessageFeedMessageClient(NetworkConnection networkConnection, string message) {
            Debug.Log($"FishNetClientConnector.AdvertiseMessageFeedMessageClient({message})");
            networkManagerClient.AdvertiseMessageFeedMessage(message);
        }

        public void AdvertiseSystemMessage(int clientId, string message) {
            AdvertiseSystemMessageClient(fishNetNetworkManager.ServerManager.Clients[clientId], message);
        }

        [TargetRpc]
        public void AdvertiseSystemMessageClient(NetworkConnection networkConnection, string message) {
            networkManagerClient.AdvertiseSystemMessage(message);
        }

        public void AdvertiseAddToBuyBackCollection(UnitController sourceUnitController, int accountId, Interactable interactable, int componentIndex, InstantiatedItem newInstantiatedItem) {
            FishNetInteractable networkInteractable = null;
            if (interactable != null) {
                networkInteractable = interactable.GetComponent<FishNetInteractable>();
            }
            FishNetUnitController networkCharacterUnit = null;
            if (sourceUnitController != null) {
                networkCharacterUnit = interactable.GetComponent<FishNetUnitController>();
            }
            if (networkManagerServer.LoggedInAccounts.ContainsKey(accountId) == false) {
                Debug.Log($"FishNetClientConnector.AdvertiseAddToBuyBackCollection() could not find client id {accountId}");
                return;
            }
            int clientId = networkManagerServer.LoggedInAccounts[accountId].clientId;
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                Debug.Log($"FishNetClientConnector.AdvertiseAddToBuyBackCollection() could not find client id {clientId}");
                return;
            }
            AdvertiseAddToBuyBackCollectionClient(fishNetNetworkManager.ServerManager.Clients[clientId], networkCharacterUnit, networkInteractable, componentIndex, newInstantiatedItem.InstanceId);
        }

        [TargetRpc]
        public void AdvertiseAddToBuyBackCollectionClient(NetworkConnection networkConnection, FishNetUnitController networkCharacterUnit, FishNetInteractable networkInteractable, int componentIndex, int instantiatedItemId) {
            networkManagerClient.AdvertiseAddToBuyBackCollection(networkCharacterUnit.UnitController, networkInteractable.Interactable, componentIndex, instantiatedItemId);
        }

        public void AdvertiseSellItemToPlayer(UnitController sourceUnitController, Interactable interactable, int componentIndex, int collectionIndex, int itemIndex, string resourceName, int remainingQuantity) {
            //Debug.Log($"FishNetClientConnector.AdvertiseSellItemToPlayer({sourceUnitController.gameObject.name}, {interactable.gameObject.name}, {componentIndex}, {collectionIndex}, {itemIndex}, {resourceName}, {remainingQuantity})");
            
            FishNetInteractable networkInteractable = null;
            if (interactable != null) {
                networkInteractable = interactable.GetComponent<FishNetInteractable>();
            }
            FishNetUnitController networkCharacterUnit = null;
            if (sourceUnitController != null) {
                networkCharacterUnit = interactable.GetComponent<FishNetUnitController>();
            }
            AdvertiseSellItemToPlayerClient(networkCharacterUnit, networkInteractable, componentIndex, collectionIndex, itemIndex, resourceName, remainingQuantity);
        }

        [ObserversRpc]
        public void AdvertiseSellItemToPlayerClient(FishNetUnitController networkCharacterUnit, FishNetInteractable networkInteractable, int componentIndex, int collectionIndex, int itemIndex, string resourceName, int remainingQuantity) {
            //Debug.Log($"FishNetClientConnector.AdvertiseSellItemToPlayer({networkCharacterUnit.gameObject.name}, {networkInteractable.gameObject.name}, {componentIndex}, {collectionIndex}, {itemIndex}, {resourceName}, {remainingQuantity})");
            networkManagerClient.AdvertiseSellItemToPlayerClient(networkCharacterUnit.UnitController, networkInteractable.Interactable, componentIndex, collectionIndex, itemIndex, resourceName, remainingQuantity);
        }

        [ServerRpc(RequireOwnership = false)]
        public void TakeAllLoot(NetworkConnection networkConnection = null) {
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                Debug.LogWarning($"FishNetClientConnector.TakeAllLoot() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }
            networkManagerServer.TakeAllLoot(networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestTakeLoot(int lootDropId, NetworkConnection networkConnection = null) {
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                Debug.LogWarning($"FishNetClientConnector.RequestTakeLoot() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }
            networkManagerServer.RequestTakeLoot(lootDropId, networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);
        }


        public void AddDroppedLoot(int accountId, int lootDropId, int itemId) {
            //Debug.Log($"FishNetClientConnector.AddDroppedLoot({accountId}, {lootDropId}, {itemId})");

            if (networkManagerServer.LoggedInAccounts.ContainsKey(accountId) == false) {
                Debug.Log($"FishNetClientConnector.AddDroppedLoot() could not find client id {accountId}");
                return;
            }
            int clientId = networkManagerServer.LoggedInAccounts[accountId].clientId;
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                return;
            }
            AddDroppedLootClient(ServerManager.Clients[clientId], lootDropId, itemId);
        }

        [TargetRpc]
        public void AddDroppedLootClient(NetworkConnection networkConnection, int lootDropId, int itemId) {
            //Debug.Log($"FishNetClientConnector.AddDroppedLootClient({networkConnection.ClientId}, {lootDropId}, {itemId})");

            networkManagerClient.AddDroppedLoot(lootDropId, itemId);
        }

        public void AddAvailableDroppedLoot(int accountId, List<LootDrop> items) {
            //Debug.Log($"FishNetClientConnector.AddAvailableDroppedLoot({accountId}, {items.Count})");

            if (networkManagerServer.LoggedInAccounts.ContainsKey(accountId) == false) {
                Debug.Log($"FishNetClientConnector.AddAvailableDroppedLoot() could not find client id {accountId}");
                return;
            }
            int clientId = networkManagerServer.LoggedInAccounts[accountId].clientId;
            if (ServerManager.Clients.ContainsKey(clientId) == false) {
                return;
            }

            List<int> lootDropIds = new List<int>();
            
            foreach (LootDrop item in items) {
                lootDropIds.Add(item.LootDropId);
            }
            AddAvailableDroppedLootClient(ServerManager.Clients[clientId], lootDropIds);
            
        }

        [TargetRpc]
        public void AddAvailableDroppedLootClient(NetworkConnection networkConnection, List<int> lootDropIds) {
            //Debug.Log($"FishNetClientConnector.AddAvailableDroppedLootClient({networkConnection.ClientId}, count: {lootDropIds.Count})");

            networkManagerClient.AddAvailableDroppedLoot(lootDropIds);
        }

        public void AdvertiseTakeLoot(int accountId, int lootDropId) {
            //Debug.Log($"FishNetClientConnector.AdvertiseTakeLoot({accountId}, {lootDropId})");

            if (networkManagerServer.LoggedInAccounts.ContainsKey(accountId) == false) {
                Debug.Log($"FishNetClientConnector.AdvertiseTakeLoot() could not find client id {accountId}");
                return;
            }
            int clientId = networkManagerServer.LoggedInAccounts[accountId].clientId;
            if (ServerManager.Clients.ContainsKey(clientId) == false) {
                return;
            }
            AdvertiseTakeLootClient(ServerManager.Clients[clientId], lootDropId);
        }

        [TargetRpc]
        public void AdvertiseTakeLootClient(NetworkConnection networkConnection, int lootDropId) {
            //Debug.Log($"FishNetClientConnector.AdvertiseTakeLootClient({networkConnection.ClientId}, {lootDropId})");

            networkManagerClient.AdvertiseTakeLoot(lootDropId);
        }

        /*
        public void SetCraftingManagerAbility(int accountId, string abilityName) {
            Debug.Log($"FishNetClientConnector.SetCraftingManagerAbility({accountId}, {abilityName})");

            SetCraftingManagerAbilityClient(ServerManager.Clients[accountId], abilityName);
        }

        [TargetRpc]
        public void SetCraftingManagerAbilityClient(NetworkConnection networkConnection, string abilityName) {
            Debug.Log($"FishNetClientConnector.SetCraftingManagerAbilityClient({networkConnection.ClientId}, {abilityName})");

            CraftAbility craftAbility = systemDataFactory.GetResource<Ability>(abilityName) as CraftAbility;
            if (craftAbility == null) {
                return;
            }
            networkManagerClient.SetCraftingManagerAbility(craftAbility);
        }
        */

        [ServerRpc(RequireOwnership = false)]
        public void RequestBeginCrafting(string recipeName, int craftAmount, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.RequestBeginCrafting({recipeName}, {craftAmount})");
            
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                //Debug.LogWarning($"FishNetClientConnector.RequestBeginCrafting() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }

            Recipe recipe = systemDataFactory.GetResource<Recipe>(recipeName);
            if (recipe == null) {
                return;
            }
            networkManagerServer.RequestBeginCrafting(recipe, craftAmount, networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);

        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestCancelCrafting(NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.RequestCancelCrafting()");

            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                return;
            }
            networkManagerServer.RequestCancelCrafting(networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);
        }

        public void RequestUpdatePlayerAppearance(Interactable interactable, int componentIndex, string unitProfileName, string appearanceString, List<SwappableMeshSaveData> swappableMeshSaveData) {
            FishNetInteractable networkInteractable = null;
            if (interactable != null) {
                networkInteractable = interactable.GetComponent<FishNetInteractable>();
            }
            RequestUpdatePlayerAppearanceServer(networkInteractable, componentIndex, unitProfileName, appearanceString, swappableMeshSaveData);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestUpdatePlayerAppearanceServer(FishNetInteractable targetNetworkInteractable, int componentIndex, string unitProfileName, string appearanceString, List<SwappableMeshSaveData> swappableMeshSaveData, NetworkConnection networkConnection = null) {
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                Debug.LogWarning($"FishNetClientConnector.RequestChangePlayerNameServer() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }

            Interactable interactable = null;
            if (targetNetworkInteractable != null) {
                interactable = targetNetworkInteractable.Interactable;
            }
            networkManagerServer.RequestUpdatePlayerAppearance(networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId, interactable, componentIndex, unitProfileName, appearanceString, swappableMeshSaveData);
        }

        public void RequestChangePlayerName(Interactable interactable, int componentIndex, string newName) {
            FishNetInteractable networkInteractable = null;
            if (interactable != null) {
                networkInteractable = interactable.GetComponent<FishNetInteractable>();
            }
            RequestChangePlayerNameServer(networkInteractable, componentIndex, newName);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestChangePlayerNameServer(FishNetInteractable targetNetworkInteractable, int componentIndex, string newName, NetworkConnection networkConnection = null) {
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                Debug.LogWarning($"FishNetClientConnector.RequestChangePlayerNameServer() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }

            Interactable interactable = null;
            if (targetNetworkInteractable != null) {
                interactable = targetNetworkInteractable.Interactable;
            }
            networkManagerServer.RequestChangePlayerName(interactable, componentIndex, newName, networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);
        }


        [ServerRpc(RequireOwnership = false)]
        public void RequestSpawnPet(string resourceName, NetworkConnection networkConnection = null) {
            Debug.Log($"FishNetClientConnector.RequestSpawnPet({resourceName})");

            UnitProfile unitProfile = systemDataFactory.GetResource<UnitProfile>(resourceName);
            if (unitProfile == null) {
                Debug.LogWarning($"FishNetClientConnector.RequestSpawnPet() could not find unit profile {resourceName}");
                return;
            }
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                return;
            }
            networkManagerServer.RequestSpawnPet(networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId, unitProfile);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestDespawnPet(string resourceName, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.RequestDespawnPet({resourceName})");

            UnitProfile unitProfile = systemDataFactory.GetResource<UnitProfile>(resourceName);
            if (unitProfile == null) {
                Debug.LogWarning($"FishNetClientConnector.RequestDespawnPet() could not find unit profile {resourceName}");
                return;
            }
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                return;
            }
            networkManagerServer.RequestDespawnPet(networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId, unitProfile);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestLoadPlayerCharacter(int playerCharacterId, NetworkConnection networkConnection = null) {
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(networkConnection.ClientId) == false) {
                Debug.LogWarning($"FishNetClientConnector.RequestSceneWeather() could not find clientId {networkConnection.ClientId} in server clients");
                return;
            }
            networkManagerServer.RequestLoadPlayerCharacter(networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId, playerCharacterId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void AcceptCharacterGroupInvite(int characterGroupId, NetworkConnection networkConnection = null) {
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                Debug.LogWarning($"FishNetClientConnector.AcceptCharacterGroupInvite() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }
            networkManagerServer.AcceptCharacterGroupInvite(networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId, characterGroupId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void DeclineCharacterGroupInvite(NetworkConnection networkConnection = null) {
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                Debug.LogWarning($"FishNetClientConnector.AcceptCharacterGroupInvite() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }
            networkManagerServer.DeclineCharacterGroupInvite(networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestLeaveCharacterGroup(NetworkConnection networkConnection = null) {
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                Debug.LogWarning($"FishNetClientConnector.AcceptCharacterGroupInvite() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }
            networkManagerServer.RequestLeaveCharacterGroup(networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestRemoveCharacterFromGroup(int playerCharacterId, NetworkConnection networkConnection = null) {
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                Debug.LogWarning($"FishNetClientConnector.AcceptCharacterGroupInvite() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }
            networkManagerServer.RequestRemoveCharacterFromGroup(networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId, playerCharacterId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestInviteCharacterToGroup(int characterId, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.RequestInviteCharacterToGroup({characterId})");

            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                Debug.LogWarning($"FishNetClientConnector.AcceptCharacterGroupInvite() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }
            networkManagerServer.RequestInviteCharacterToGroup(networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId, characterId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestDisbandCharacterGroup(int characterGroupId, NetworkConnection networkConnection = null) {
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                Debug.LogWarning($"FishNetClientConnector.AcceptCharacterGroupInvite() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }
            networkManagerServer.RequestDisbandCharacterGroup(networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId, characterGroupId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestPromoteCharacterToLeader(int characterId, NetworkConnection networkConnection = null) {
            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                Debug.LogWarning($"FishNetClientConnector.AcceptCharacterGroupInvite() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }
            networkManagerServer.RequestPromoteCharacterToLeader(networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId, characterId);
        }



        [ServerRpc(RequireOwnership = false)]
        public void RequestSceneWeather(NetworkConnection networkConnection = null) {
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(networkConnection.ClientId) == false) {
                Debug.LogWarning($"FishNetClientConnector.RequestSceneWeather() could not find clientId {networkConnection.ClientId} in server clients");
                return;
            }
            HashSet<Scene> sceneList = fishNetNetworkManager.ServerManager.Clients[networkConnection.ClientId].Scenes;
            if (sceneList.Count == 0) {
                Debug.LogWarning($"FishNetClientConnector.RequestSceneWeather() could not find any scenes for clientId {networkConnection.ClientId}");
                return;
            }
            WeatherProfile weatherProfile = networkManagerServer.GetSceneWeatherProfile(sceneList.First().handle);
            AdvertiseChooseWeatherClient(networkConnection, weatherProfile == null ? string.Empty : weatherProfile.ResourceName);
            AdvertiseStartWeatherClient(networkConnection);
        }


        [ServerRpc(RequireOwnership = false)]
        public void RequestLogout(NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.RequestLogout()");

            if (networkManagerServer.LoggedInAccountsByClient.ContainsKey(networkConnection.ClientId) == false) {
                Debug.LogWarning($"FishNetClientConnector.RequestLogout() could not find clientId {networkConnection.ClientId} in logged in accounts");
                return;
            }
            networkManagerServer.Logout(networkManagerServer.LoggedInAccountsByClient[networkConnection.ClientId].accountId);
        }

        public void AdvertiseChooseWeather(int sceneHandle, WeatherProfile weatherProfile) {
            //Debug.Log($"FishNetClientConnector.AdvertiseChooseWeather({sceneHandle}, {(weatherProfile == null ? "null" : weatherProfile.ResourceName)})");

            // get list of clients in scene
            Scene scene = FishNet.Managing.Scened.SceneManager.GetScene(sceneHandle);
            if (scene.IsValid() == false) {
                Debug.LogWarning($"FishNetClientConnector.AdvertiseChooseWeather() could not find scene with handle {sceneHandle}");
                return;
            }
            if (SceneManager.SceneConnections.ContainsKey(scene) == true) {
                HashSet<NetworkConnection> clientsInScene = SceneManager.SceneConnections[scene];
                foreach (NetworkConnection networkConnection in clientsInScene) {
                    AdvertiseChooseWeatherClient(networkConnection, weatherProfile == null ? string.Empty : weatherProfile.ResourceName);
                }
            }
        }

        [TargetRpc]
        public void AdvertiseChooseWeatherClient(NetworkConnection networkConnection, string weatherProfileName) {
            //Debug.Log($"FishNetClientConnector.AdvertiseChooseWeatherClient({weatherProfileName})");

            WeatherProfile weatherProfile = null;
            if (weatherProfileName != string.Empty) {
                weatherProfile = systemDataFactory.GetResource<WeatherProfile>(weatherProfileName);
            }
            networkManagerClient.AdvertiseChooseWeather(weatherProfile);
        }

        public void AdvertiseEndWeather(int sceneHandle, WeatherProfile weatherProfile, bool immediate) {
            //Debug.Log($"FishNetClientConnector.AdvertiseEndWeather({sceneHandle}, {(weatherProfile == null ? "null" : weatherProfile.ResourceName)}, {immediate})");

            Scene scene = FishNet.Managing.Scened.SceneManager.GetScene(sceneHandle);
            if (scene.IsValid() == false) {
                Debug.LogWarning($"FishNetClientConnector.AdvertiseChooseWeather() could not find scene with handle {sceneHandle}");
                return;
            }
            if (SceneManager.SceneConnections.ContainsKey(scene) == true) {
                HashSet<NetworkConnection> clientsInScene = SceneManager.SceneConnections[scene];
                foreach (NetworkConnection networkConnection in clientsInScene) {
                    AdvertiseEndWeatherClient(networkConnection, weatherProfile == null ? string.Empty : weatherProfile.ResourceName, immediate);
                }
            }
        }

        [TargetRpc]
        public void AdvertiseEndWeatherClient(NetworkConnection networkConnection, string profileName, bool immediate) {
            //Debug.Log($"FishNetClientConnector.AdvertiseEndWeatherClient({profileName}, {immediate})");
            WeatherProfile weatherProfile = null;
            if (profileName != string.Empty) {
                weatherProfile = systemDataFactory.GetResource<WeatherProfile>(profileName);
            }
            networkManagerClient.AdvertiseEndWeather(weatherProfile, immediate);
        }

        public void AdvertiseStartWeather(int sceneHandle) {
            //Debug.Log($"FishNetClientConnector.AdvertiseStartWeather({sceneHandle})");

            Scene scene = FishNet.Managing.Scened.SceneManager.GetScene(sceneHandle);
            if (scene.IsValid() == false) {
                Debug.LogWarning($"FishNetClientConnector.AdvertiseChooseWeather() could not find scene with handle {sceneHandle}");
                return;
            }
            if (SceneManager.SceneConnections.ContainsKey(scene) == true) {
                HashSet<NetworkConnection> clientsInScene = SceneManager.SceneConnections[scene];
                foreach (NetworkConnection networkConnection in clientsInScene) {
                    AdvertiseStartWeatherClient(networkConnection);
                }
            }
        }

        [TargetRpc]
        public void AdvertiseStartWeatherClient(NetworkConnection networkConnection) {
            //Debug.Log($"FishNetClientConnector.AdvertiseStartWeatherClient()");

            networkManagerClient.AdvertiseStartWeather();
        }

        public void AdvertiseLoadCutscene(Cutscene cutscene, int accountId) {
            //Debug.Log($"FishNetClientConnector.AdvertiseLoadCutscene({cutscene.ResourceName}, {accountId})");

            if (networkManagerServer.LoggedInAccounts.ContainsKey(accountId) == false) {
                Debug.Log($"FishNetClientConnector.AdvertiseLoadCutscene() could not find client id {accountId}");
                return;
            }
            int clientId = networkManagerServer.LoggedInAccounts[accountId].clientId;
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                return;
            }

            // unload the current scene for the client
            NetworkConnection networkConnection = fishNetNetworkManager.ServerManager.Clients[clientId];
            SceneUnloadData sceneUnloadData = new SceneUnloadData(networkConnection.Scenes.First());
            base.NetworkManager.SceneManager.UnloadConnectionScenes(networkConnection, sceneUnloadData);

            AdvertiseLoadCutsceneClient(fishNetNetworkManager.ServerManager.Clients[clientId], cutscene.ResourceName);
        }

        [TargetRpc]
        public void AdvertiseLoadCutsceneClient(NetworkConnection networkConnection, string cutSceneName) {
            //Debug.Log($"FishNetClientConnector.AdvertiseLoadCutsceneClient({cutSceneName})");

            Cutscene cutScene = systemDataFactory.GetResource<Cutscene>(cutSceneName);
            if (cutScene == null) {
                Debug.LogWarning($"FishNetClientConnector.AdvertiseLoadCutsceneClient() could not find cutscene {cutSceneName}");
                return;
            }
            networkManagerClient.AdvertiseLoadCutscene(cutScene);
        }

        [TargetRpc]
        public void AdvertisePresenceChangeEnd(NetworkConnection connection) {
            //Debug.Log("FishNetClientConnector.AdvertisePresenceChangeEnd()");

            networkManagerClient.AdvertiseSceneObjectLoadComplete();
        }

        public void AdvertiseAddCharacterToGroup(int playerCharacterId, CharacterGroup characterGroup) {
            //Debug.Log($"FishNetClientConnector.AdvertiseAddCharacterToGroup({playerCharacterId}, {characterGroup.characterGroupId})");

            foreach (int memberId in characterGroup.CharacterIdList[UnitControllerMode.Player].Keys) {
                // get account id from player id
                int accountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(memberId);
                if (accountId == 0) {
                    continue;
                }
                if (networkManagerServer.LoggedInAccounts.ContainsKey(accountId) == false) {
                    Debug.Log($"FishNetClientConnector.AdvertiseAddCharacterToGroup() could not find account id {accountId}");
                    continue;
                }
                int clientId = networkManagerServer.LoggedInAccounts[accountId].clientId;
                if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                    Debug.Log($"FishNetClientConnector.AdvertiseAddCharacterToGroup() could not find client id {clientId}");
                    continue;
                }
                AdvertiseAddCharacterToGroupClient(fishNetNetworkManager.ServerManager.Clients[clientId], playerCharacterId, characterGroup);
            }
        }

        public void AdvertiseCharacterGroup(int accountId, CharacterGroup characterGroup) {
            //Debug.Log($"FishNetClientConnector.AdvertiseCharacterGroup({accountId}, {characterGroup.characterGroupId})");

            if (networkManagerServer.LoggedInAccounts.ContainsKey(accountId) == false) {
                Debug.Log($"FishNetClientConnector.AdvertiseAddCharacterToGroup() could not find client id {accountId}");
                return;
            }
            int clientId = networkManagerServer.LoggedInAccounts[accountId].clientId;
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                Debug.Log($"FishNetClientConnector.AdvertiseAddCharacterToGroup() could not find client id {clientId}");
                return;
            }
            AdvertiseCharacterGroupClient(fishNetNetworkManager.ServerManager.Clients[clientId], characterGroup);
        }

        [TargetRpc]
        public void AdvertiseCharacterGroupClient(NetworkConnection networkConnection, CharacterGroup characterGroup) {
            //Debug.Log($"FishNetClientConnector.AdvertiseAddCharacterToGroupClient({characterGroup.characterGroupId})");

            networkManagerClient.ProcessLoadCharacterGroup(characterGroup);
        }


        [TargetRpc]
        public void AdvertiseAddCharacterToGroupClient(NetworkConnection networkConnection, int playerCharacterId, CharacterGroup characterGroup) {
            //Debug.Log($"FishNetClientConnector.AdvertiseAddCharacterToGroupClient({playerCharacterId}, {characterGroup.characterGroupId})");

            networkManagerClient.ProcessCharacterJoinGroup(playerCharacterId, characterGroup);
        }

        public void AdvertiseRemoveCharacterFromGroup(int removedCharacterId, CharacterGroup characterGroup) {
            //Debug.Log($"FishNetClientConnector.AdvertiseRemoveCharacterFromGroup({removedCharacterId}, {characterGroup.characterGroupId})");

            foreach (int memberId in characterGroup.CharacterIdList[UnitControllerMode.Player].Keys) {
                // get account id from player id
                int memberAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(memberId);
                if (memberAccountId == 0) {
                    continue;
                }
                if (networkManagerServer.LoggedInAccounts.ContainsKey(memberAccountId) == false) {
                    Debug.Log($"FishNetClientConnector.AdvertiseAddCharacterToGroup() could not find client id {memberAccountId}");
                    continue;
                }
                int _clientId = networkManagerServer.LoggedInAccounts[memberAccountId].clientId;
                if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
                    Debug.Log($"FishNetClientConnector.AdvertiseAddCharacterToGroup() could not find client id {_clientId}");
                    continue;
                }
                AdvertiseRemoveCharacterFromGroupClient(fishNetNetworkManager.ServerManager.Clients[_clientId], removedCharacterId, characterGroup.characterGroupId);
            }
            // get account id from player id
            int accountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(removedCharacterId);
            if (networkManagerServer.LoggedInAccounts.ContainsKey(accountId) == false) {
                Debug.Log($"FishNetClientConnector.AdvertiseAddCharacterToGroup() could not find client id {accountId}");
                return;
            }
            int clientId = networkManagerServer.LoggedInAccounts[accountId].clientId;
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                Debug.Log($"FishNetClientConnector.AdvertiseAddCharacterToGroup() could not find client id {clientId}");
                return;
            }
            AdvertiseRemoveCharacterFromGroupClient(fishNetNetworkManager.ServerManager.Clients[clientId], removedCharacterId, characterGroup.characterGroupId);
        }

        [TargetRpc]
        public void AdvertiseRemoveCharacterFromGroupClient(NetworkConnection networkConnection, int removedPlayerId, int characterGroupId) {
            networkManagerClient.ProcessCharacterLeaveGroup(removedPlayerId, characterGroupId);
        }

        public void AdvertiseCharacterGroupInvite(int invitedCharacterId, CharacterGroup characterGroup, string leaderName) {
            //Debug.Log($"FishNetClientConnector.AdvertiseCharacterGroupInvite({invitedCharacterId}, {characterGroup.characterGroupId}, {leaderName})");

            int accountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(invitedCharacterId);
            if (networkManagerServer.LoggedInAccounts.ContainsKey(accountId) == false) {
                Debug.Log($"FishNetClientConnector.AdvertiseCharacterGroupInvite() account id {accountId} is not logged in");
                return;
            }
            int clientId = networkManagerServer.LoggedInAccounts[accountId].clientId;
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                Debug.Log($"FishNetClientConnector.AdvertiseCharacterGroupInvite() could not find client id {clientId}");
                return;
            }
            AdvertiseCharacterGroupInviteClient(fishNetNetworkManager.ServerManager.Clients[clientId], characterGroup, leaderName);
        }

        [TargetRpc]
        public void AdvertiseCharacterGroupInviteClient(NetworkConnection networkConnection, CharacterGroup characterGroup, string leaderName) {
            networkManagerClient.ProcessCharacterGroupInvite(characterGroup, leaderName);
        }

        public void AdvertiseDisbandCharacterGroup(CharacterGroup characterGroup) {
            foreach (int memberId in characterGroup.CharacterIdList[UnitControllerMode.Player].Keys) {
                // get account id from player id
                int memberAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(memberId);
                if (memberAccountId == 0) {
                    continue;
                }
                if (networkManagerServer.LoggedInAccounts.ContainsKey(memberAccountId) == false) {
                    Debug.Log($"FishNetClientConnector.AdvertiseAddCharacterToGroup() could not find client id {memberAccountId}");
                    continue;
                }
                int _clientId = networkManagerServer.LoggedInAccounts[memberAccountId].clientId;
                if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
                    Debug.Log($"FishNetClientConnector.AdvertiseAddCharacterToGroup() could not find client id {_clientId}");
                    continue;
                }
                AdvertiseDisbandCharacterGroupClient(fishNetNetworkManager.ServerManager.Clients[_clientId], characterGroup.characterGroupId);
            }
        }

        [TargetRpc]
        public void AdvertiseDisbandCharacterGroupClient(NetworkConnection networkConnection, int characterGroupId) {
            networkManagerClient.ProcessDisbandCharacterGroup(characterGroupId);
        }

        public void AdvertiseRenameCharacterInGroup(CharacterGroup characterGroup, int characterId, string newName) {
            foreach (int memberId in characterGroup.CharacterIdList[UnitControllerMode.Player].Keys) {
                // get account id from player id
                int memberAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(memberId);
                if (memberAccountId == 0) {
                    continue;
                }
                if (networkManagerServer.LoggedInAccounts.ContainsKey(memberAccountId) == false) {
                    Debug.Log($"FishNetClientConnector.AdvertiseRenameCharacterInGroup() could not find client id {memberAccountId}");
                    continue;
                }
                int _clientId = networkManagerServer.LoggedInAccounts[memberAccountId].clientId;
                if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
                    Debug.Log($"FishNetClientConnector.AdvertiseRenameCharacterInGroup() could not find client id {_clientId}");
                    continue;
                }
                AdvertiseRenameCharacterInGroupClient(fishNetNetworkManager.ServerManager.Clients[_clientId], characterGroup.characterGroupId, characterId, newName);
            }
        }

        [TargetRpc]
        public void AdvertiseRenameCharacterInGroupClient(NetworkConnection networkConnection, int characterGroupId, int characterId, string newName) {
            networkManagerClient.ProcessRenameCharacterInGroup(characterGroupId, characterId, newName);
        }

        public void AdvertisePlayerNameNotAvailable(int accountId) {
            if (networkManagerServer.LoggedInAccounts.ContainsKey(accountId) == false) {
                Debug.Log($"FishNetClientConnector.AdvertiseCharacterGroupInvite() account id {accountId} is not logged in");
                return;
            }
            int clientId = networkManagerServer.LoggedInAccounts[accountId].clientId;
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                Debug.Log($"FishNetClientConnector.AdvertiseCharacterGroupInvite() could not find client id {clientId}");
                return;
            }
            AdvertisePlayerNameNotAvailableClient(fishNetNetworkManager.ServerManager.Clients[clientId]);
        }

        [TargetRpc]
        public void AdvertisePlayerNameNotAvailableClient(NetworkConnection networkConnection) {
            networkManagerClient.AdvertisePlayerNameNotAvailable();
        }

        public void AdvertiseDeclineCharacterGroupInvite(int leaderAccountId, string decliningPlayerName) {
            if (networkManagerServer.LoggedInAccounts.ContainsKey(leaderAccountId) == false) {
                Debug.Log($"FishNetClientConnector.AdvertiseCharacterGroupInvite() account id {leaderAccountId} is not logged in");
                return;
            }
            int clientId = networkManagerServer.LoggedInAccounts[leaderAccountId].clientId;
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                Debug.Log($"FishNetClientConnector.AdvertiseCharacterGroupInvite() could not find client id {clientId}");
                return;
            }
            AdvertiseDeclineCharacterGroupInviteClient(fishNetNetworkManager.ServerManager.Clients[clientId], decliningPlayerName);
        }

        [TargetRpc]
        public void AdvertiseDeclineCharacterGroupInviteClient(NetworkConnection networkConnection, string decliningPlayerName) {
            networkManagerClient.ProcessDeclineCharacterGroupInvite(decliningPlayerName);
        }

        public void AdvertisePromoteGroupLeader(CharacterGroup characterGroup, int newLeaderCharacterId) {
            foreach (int memberId in characterGroup.CharacterIdList[UnitControllerMode.Player].Keys) {
                // get account id from player id
                int memberAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(memberId);
                if (memberAccountId == 0) {
                    continue;
                }
                if (networkManagerServer.LoggedInAccounts.ContainsKey(memberAccountId) == false) {
                    Debug.Log($"FishNetClientConnector.AdvertiseAddCharacterToGroup() could not find client id {memberAccountId}");
                    continue;
                }
                int _clientId = networkManagerServer.LoggedInAccounts[memberAccountId].clientId;
                if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
                    Debug.Log($"FishNetClientConnector.AdvertiseAddCharacterToGroup() could not find client id {_clientId}");
                    continue;
                }
                AdvertisePromoteGroupLeaderClient(fishNetNetworkManager.ServerManager.Clients[_clientId], characterGroup.characterGroupId, newLeaderCharacterId);
            }
        }

        [TargetRpc]
        public void AdvertisePromoteGroupLeaderClient(NetworkConnection networkConnection, int characterGroupId, int newLeaderCharacterId) {
            networkManagerClient.ProcessPromoteGroupLeader(characterGroupId, newLeaderCharacterId);
        }

        public void AdvertiseGroupMessage(CharacterGroup characterGroup, string messageText) {
            foreach (int memberId in characterGroup.CharacterIdList[UnitControllerMode.Player].Keys) {
                // get account id from player id
                int memberAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(memberId);
                if (memberAccountId == 0) {
                    continue;
                }
                if (networkManagerServer.LoggedInAccounts.ContainsKey(memberAccountId) == false) {
                    Debug.Log($"FishNetClientConnector.AdvertiseGroupMessage() could not find client id {memberAccountId}");
                    continue;
                }
                int _clientId = networkManagerServer.LoggedInAccounts[memberAccountId].clientId;
                if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
                    Debug.Log($"FishNetClientConnector.AdvertiseGroupMessage() could not find client id {_clientId}");
                    continue;
                }
                AdvertiseGroupMessageClient(fishNetNetworkManager.ServerManager.Clients[_clientId], messageText);
            }
        }

        [TargetRpc]
        public void AdvertiseGroupMessageClient(NetworkConnection networkConnection, string messageText) {
            networkManagerClient.AdvertiseGroupMessage(messageText);
        }

        public void AdvertisePrivateMessage(int targetAccountId, string messageText) {
            //Debug.Log($"FishNetClientConnector.AdvertisePrivateMessage({targetAccountId}, {messageText})");

            if (networkManagerServer.LoggedInAccounts.ContainsKey(targetAccountId) == false) {
                Debug.Log($"FishNetClientConnector.AdvertisePrivateMessage() could not find account id {targetAccountId}");
                return;
            }
            int _clientId = networkManagerServer.LoggedInAccounts[targetAccountId].clientId;
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
                Debug.Log($"FishNetClientConnector.AdvertiseRenameCharacterInGroup() could not find client id {_clientId}");
                return;
            }
            AdvertisePrivateMessageClient(fishNetNetworkManager.ServerManager.Clients[_clientId], messageText);
        }

        [TargetRpc]
        public void AdvertisePrivateMessageClient(NetworkConnection networkConnection, string messageText) {
            //Debug.Log($"FishNetClientConnector.AdvertisePrivateMessageClient({messageText})");

            networkManagerClient.AdvertisePrivateMessage(messageText);
        }



        /*
        public override void OnStartServer() {
            base.OnStartServer();
            Debug.Log($"FishNetClientConnector.OnStartServer()");

            // on server gameMode should always bet set to network
            //Debug.Log($"FishNetClientConnector.OnStartServer(): setting gameMode to network");
            systemGameManager.SetGameMode(GameMode.Network);
            networkManagerServer.ActivateServerMode();
        }

        public override void OnStopServer() {
            base.OnStopServer();
            Debug.Log($"FishNetClientConnector.OnStopServer()");

            systemGameManager.SetGameMode(GameMode.Local);
            networkManagerServer.DeactivateServerMode();
        }
        */
    }
}
