using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;

namespace AnyRPG {
    public class FishNetClientConnector : ConfiguredNetworkBehaviour {

        private FishNet.Managing.NetworkManager fishNetNetworkManager;

        // game manager references
        private SystemDataFactory systemDataFactory = null;
        private NetworkManagerServer networkManagerServer = null;
        private NetworkManagerClient networkManagerClient = null;
        private PlayerManagerServer playerManagerServer = null;
        private CharacterGroupServiceServer characterGroupServiceServer = null;

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log($"FishNetClientConnector.Configure(): instanceId: {GetInstanceID()}");

            base.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            systemDataFactory = systemGameManager.SystemDataFactory;
            networkManagerServer = systemGameManager.NetworkManagerServer;
            networkManagerClient = systemGameManager.NetworkManagerClient;
            playerManagerServer = systemGameManager.PlayerManagerServer;
            characterGroupServiceServer = systemGameManager.CharacterGroupServiceServer;
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

            networkManagerServer.RequestSpawnRequest(networkConnection.ClientId);
        }


        [ServerRpc(RequireOwnership = false)]
        public void RequestSpawnPlayerUnit(string sceneName, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.RequestSpawnPlayerUnit({sceneName}, {networkConnection.ClientId})");

            networkManagerServer.RequestSpawnPlayerUnit(networkConnection.ClientId, sceneName);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestRespawnPlayerUnit(NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.RequestRespawnPlayerUnit()");

            networkManagerServer.RequestRespawnPlayerUnit(networkConnection.ClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestDespawnPlayerUnit(NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.RequestDespawnPlayerUnit()");

            networkManagerServer.RequestDespawnPlayerUnit(networkConnection.ClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestRevivePlayerUnit(NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.RequestRevivePlayerUnit()");

            networkManagerServer.RequestRevivePlayerUnit(networkConnection.ClientId);
        }

        public void SpawnPlayer(int accountId, CharacterRequestData characterRequestData, Vector3 position, Vector3 forward, string sceneName) {
            //Debug.Log($"FishNetClientConnector.SpawnPlayer(accountId: {accountId}, {characterRequestData.characterConfigurationRequest.unitProfile.ResourceName}, {position}, {forward}, {sceneName})");

            int clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (clientId == -1) {
                return;
            }
            NetworkConnection networkConnection = fishNetNetworkManager.ServerManager.Clients[clientId];

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
            int clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (clientId < 0) {
                return;
            }
            NetworkConnection networkConnection = fishNetNetworkManager.ServerManager.Clients[clientId];
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

            int clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (clientId < 0) {
                return default;
            }

            NetworkConnection networkConnection = fishNetNetworkManager.ServerManager.Clients[clientId];
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
                int clientId = networkManagerServer.GetClientIDForAccount(characterRequestData.accountId);
                if (clientId < 0) {
                    return null;
                }
                if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId)) {
                    NetworkConnection networkConnection = fishNetNetworkManager.ServerManager.Clients[clientId];
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
            //Debug.Log($"FishNetClientConnector.SetNetworkObjectParent({networkObject.gameObject.name}, {(parentTransform == null ? "null" : parentTransform.gameObject.name)})");

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
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestReturnFromCutscene(NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.LoadSceneServer({networkConnection.ClientId}, {sceneName})");

            networkManagerServer.ReturnFromCutscene(networkConnection.ClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestCreatePlayerCharacter(CharacterSaveData saveData, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.RequestCreatePlayerCharacter()");

            networkManagerServer.RequestCreatePlayerCharacter(networkConnection.ClientId, saveData);
        }


        [ServerRpc(RequireOwnership = false)]
        public void DeletePlayerCharacter(int playerCharacterId, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.DeletePlayerCharacter({playerCharacterId})");

            networkManagerServer.RequestDeletePlayerCharacter(networkConnection.ClientId, playerCharacterId);

            // now that character is deleted, just load the character list
            //LoadCharacterList(networkConnection);
        }

        [ServerRpc(RequireOwnership = false)]
        public void ToggleLobbyGameReadyStatus(int gameId, NetworkConnection networkConnection = null) {
            networkManagerServer.ToggleLobbyGameReadyStatus(gameId, networkConnection.ClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void LoadCharacterList(NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.LoadCharacterList()");
            networkManagerServer.RequestLoadCharacterList(networkConnection.ClientId);
        }

        public void AdvertiseLoadCharacterList(int accountId, List<PlayerCharacterSaveData> playerCharacterSaveDataList) {
            //Debug.Log($"FishNetClientConnector.HandleLoadCharacterList({accountId})");

            int clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (clientId == -1) {
                return;
            }
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
            networkManagerServer.RequestLobbyGameList(networkConnection.ClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestLobbyPlayerList(NetworkConnection networkConnection = null) {
            networkManagerServer.RequestLobbyPlayerList(networkConnection.ClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SendLobbyChatMessage(string messageText, NetworkConnection networkConnection = null) {
            networkManagerServer.SendLobbyChatMessage(messageText, networkConnection.ClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SendLobbyGameChatMessage(string messageText, int gameId, NetworkConnection networkConnection = null) {
            networkManagerServer.SendLobbyGameChatMessage(messageText, networkConnection.ClientId, gameId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SendSceneChatMessage(string messageText, NetworkConnection networkConnection = null) {
            networkManagerServer.SendSceneChatMessage(messageText, networkConnection.ClientId);
        }


        [ServerRpc(RequireOwnership = false)]
        public void ChooseLobbyGameCharacter(string unitProfileName, int gameId, string appearanceString, List<SwappableMeshSaveData> swappableMeshSaveData, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.ChooseLobbyGameCharacter({unitProfileName}, {gameId})");

            networkManagerServer.ChooseLobbyGameCharacter(gameId, networkConnection.ClientId, unitProfileName, appearanceString, swappableMeshSaveData);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestStartLobbyGame(int gameId, NetworkConnection networkConnection = null) {
            networkManagerServer.RequestStartLobbyGame(gameId, networkConnection.ClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestJoinLobbyGameInProgress(int gameId, NetworkConnection networkConnection = null) {
            networkManagerServer.RequestJoinLobbyGameInProgress(gameId, networkConnection.ClientId);
        }

        public override void OnStartNetwork() {
            base.OnStartNetwork();
            //Debug.Log($"FishNetClientConnector.OnStartNetwork()");

            FishNetNetworkController fishNetNetworkController = GameObject.FindAnyObjectByType<FishNetNetworkController>();
            fishNetNetworkController.RegisterConnector(this);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestCreateLobbyGame(string sceneResourceName, bool allowLateJoin, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.RequestCreateLobbyGame(sceneResourceName: {sceneResourceName}, allowLateJoin: {allowLateJoin})");

            networkManagerServer.CreateLobbyGame(sceneResourceName, networkConnection.ClientId, allowLateJoin);
        }


        [ObserversRpc]
        public void AdvertiseCreateLobbyGame(LobbyGame lobbyGame) {
            networkManagerClient.AdvertiseCreateLobbyGame(lobbyGame);
        }

        [ServerRpc(RequireOwnership = false)]
        public void CancelLobbyGame(int gameId, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.CancelLobbyGame()");
            networkManagerServer.CancelLobbyGame(networkConnection.ClientId, gameId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void JoinLobbyGame(int gameId, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.JoinLobbyGame()");
            networkManagerServer.JoinLobbyGame(gameId, networkConnection.ClientId);
        }

        [ObserversRpc]
        public void AdvertiseLobbyLogin(int accountId, string userName) {
            //Debug.Log($"FishNetClientConnector.AdvertiseLobbyLogin({accountId}, {userName})");

            networkManagerClient.AdvertiseLobbyLogin(accountId, userName);
        }

        public void SendLobbyGameList(int accountId, List<LobbyGame> lobbyGames) {
            int clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (clientId == -1) {
                return;
            }
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
            int clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (clientId == -1) {
                return;
            }
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

            int clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (clientId == -1) {
                return;
            }
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                return;
            }
            NetworkConnection networkConnection = fishNetNetworkManager.ServerManager.Clients[clientId];

            AdvertiseJoinLobbyGameInProgress(networkConnection, gameId);

        }

        public void AdvertiseJoinMMOGameInProgress(int accountId) {
            //Debug.Log($"FishNetClientConnector.AdvertiseJoinMMOGameInProgress(accountId: {accountId})");

            int clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (clientId == -1) {
                return;
            }
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                return;
            }
            NetworkConnection networkConnection = fishNetNetworkManager.ServerManager.Clients[clientId];

            AdvertiseJoinMMOGameInProgress(networkConnection);
        }

        public void LoadNewScene(int accountId, int playerCharacterId, SceneInstanceType sceneInstanceType, SceneNode sceneNode) {
            //Debug.Log($"FishNetClientConnector.LoadNewScene(accountId: {accountId}, playerCharacterId: {playerCharacterId}, sceneInstanceType: {sceneInstanceType.ToString()})");

            int clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (clientId == -1) {
                return;
            }
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                return;
            }
            NetworkConnection networkConnection = fishNetNetworkManager.ServerManager.Clients[clientId];

            int characterGroupId = characterGroupServiceServer.GetCharacterGroupIdFromCharacterId(playerCharacterId);

            SceneLoadData sceneLoadData = new SceneLoadData(sceneNode.SceneFile);
            sceneLoadData.ReplaceScenes = ReplaceOption.All;
            sceneLoadData.Options.AutomaticallyUnload = false;
            sceneLoadData.Options.LocalPhysics = LocalPhysicsMode.Physics3D;
            sceneLoadData.PreferredActiveScene = new PreferredScene(SceneLookupData.CreateData(sceneNode.SceneFile));
            if (characterGroupId != -1 && sceneInstanceType == SceneInstanceType.Group) {
                // this is a dungeon and the character is in a group. set the request hash and stacking so that this instance gets linked to the group
                sceneLoadData.Options.AllowStacking = true;
                networkManagerServer.SetCharacterGroupLoadRequestHashcode(characterGroupId, sceneLoadData.GetHashCode());
            } else if (sceneInstanceType == SceneInstanceType.Personal) {
                // this is a dungeon and the character is not in a group.  no hash code will be set because this instance will be unique to this character
                sceneLoadData.Options.AllowStacking = true;
                networkManagerServer.SetPersonalLoadRequestHashcode(playerCharacterId, sceneLoadData.GetHashCode());
            } else {
                // this is a world scene and should not be stacked
                sceneLoadData.Options.AllowStacking = false;
            }
            networkManagerServer.SetSceneLoadRequestHashCode(sceneInstanceType, sceneLoadData.GetHashCode());
            fishNetNetworkManager.SceneManager.LoadConnectionScenes(networkConnection, sceneLoadData);
        }

        public void LoadExistingScene(int accountId, int sceneHandle) {
            //Debug.Log($"FishNetClientConnector.LoadExistingScene(accountId: {accountId}, sceneHandle: {sceneHandle})");

            int clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (clientId == -1) {
                return;
            }
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                return;
            }
            NetworkConnection networkConnection = fishNetNetworkManager.ServerManager.Clients[clientId];

            SceneLoadData sceneLoadData = new(sceneHandle);
            sceneLoadData.Options.AutomaticallyUnload = false;
            sceneLoadData.ReplaceScenes = ReplaceOption.All;
            sceneLoadData.Options.LocalPhysics = LocalPhysicsMode.Physics3D;
            sceneLoadData.Options.AllowStacking = true;
            sceneLoadData.PreferredActiveScene = new PreferredScene(SceneLookupData.CreateData(sceneHandle));
            
            fishNetNetworkManager.SceneManager.LoadConnectionScenes(networkConnection, sceneLoadData);
        }

        /*
        public void LoadMMOGameScene(int accountId, SceneNode sceneNode, NetworkConnection networkConnection) {
            //Debug.Log($"FishNetClientConnector.LoadMMOGameScene(accountId: {accountId}, {sceneNode.SceneFile}, clientId: {networkConnection.ClientId}");

            // get characterGroupId for accountId
            int playerId = playerManagerServer.GetPlayerCharacterId(accountId);
            int characterGroupId = characterGroupServiceServer.GetCharacterGroupIdFromCharacterId(playerId);
            //Debug.Log($"playerId: {playerId}; characterGroupId: {characterGroupId}");

            if (characterGroupId != -1
                && sceneNode.IsDungeon == true
                && networkManagerServer.CharacterGroupSceneHandles.ContainsKey(characterGroupId)
                && networkManagerServer.CharacterGroupSceneHandles[characterGroupId].ContainsKey(sceneNode.SceneFile) == true) {
                // this is a dungeon and an existing scene exists for this character group
                //Debug.Log("this is a dungeon and an existing scene exists for this character group");
                SceneLoadData sceneLoadData = new(networkManagerServer.CharacterGroupSceneHandles[characterGroupId][sceneNode.SceneFile]);
                sceneLoadData.Options.AutomaticallyUnload = false;
                sceneLoadData.ReplaceScenes = ReplaceOption.All;
                sceneLoadData.Options.LocalPhysics = LocalPhysicsMode.Physics3D;
                sceneLoadData.Options.AllowStacking = true;
                sceneLoadData.PreferredActiveScene = new PreferredScene(SceneLookupData.CreateData(sceneNode.SceneFile));
                fishNetNetworkManager.SceneManager.LoadConnectionScenes(networkConnection, sceneLoadData);
            } else {
                // load new scene
                //Debug.Log("loading a new scene");
                SceneLoadData sceneLoadData = new SceneLoadData(sceneNode.SceneFile);
                sceneLoadData.ReplaceScenes = ReplaceOption.All;
                sceneLoadData.Options.AutomaticallyUnload = false;
                sceneLoadData.Options.LocalPhysics = LocalPhysicsMode.Physics3D;
                sceneLoadData.PreferredActiveScene = new PreferredScene(SceneLookupData.CreateData(sceneNode.SceneFile));
                SceneInstanceType sceneInstanceType = SceneInstanceType.World;
                if (characterGroupId > 0 && sceneNode.IsDungeon == true) {
                    // this is a dungeon and the character is in a group. set the request hash and stacking so that this instance gets linked to the group
                    sceneLoadData.Options.AllowStacking = true;
                    networkManagerServer.SetCharacterGroupLoadRequestHashcode(characterGroupId, sceneLoadData.GetHashCode());
                    sceneInstanceType = SceneInstanceType.Dungeon;
                } else if (sceneNode.IsDungeon == true) {
                    // this is a dungeon and the character is not in a group.  no hash code will be set because this instance will be unique to this character
                    sceneInstanceType = SceneInstanceType.Dungeon;
                    sceneLoadData.Options.AllowStacking = true;
                } else {
                    // this is a world scene and should not be stacked
                    sceneLoadData.Options.AllowStacking = false;
                }
                networkManagerServer.SetSceneLoadRequestHashCode(sceneInstanceType, sceneLoadData.GetHashCode());
                fishNetNetworkManager.SceneManager.LoadConnectionScenes(networkConnection, sceneLoadData);
            }
        }
        */

        public void LoadNewLobbyGameScene(int accountId, LobbyGame lobbyGame, SceneNode sceneNode) {
            //Debug.Log($"FishNetClientConnector.LoadNewLobbyGameScene(accountId: {accountId}, lobbyGameId: {lobbyGame.gameId}, sceneFile: {sceneNode.SceneFile}");

            int clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (clientId == -1) {
                return;
            }
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                return;
            }
            NetworkConnection networkConnection = fishNetNetworkManager.ServerManager.Clients[clientId];

            SceneLoadData sceneLoadData = new SceneLoadData(sceneNode.SceneFile);
            sceneLoadData.ReplaceScenes = ReplaceOption.All;
            sceneLoadData.Options.AutomaticallyUnload = false;
            sceneLoadData.Options.LocalPhysics = LocalPhysicsMode.Physics3D;
            sceneLoadData.Options.AllowStacking = true;
            sceneLoadData.PreferredActiveScene = new PreferredScene(SceneLookupData.CreateData(lobbyGame.sceneResourceName));
            networkManagerServer.SetLobbyGameLoadRequestHashcode(lobbyGame.gameId, sceneLoadData.GetHashCode());
            networkManagerServer.SetSceneLoadRequestHashCode(SceneInstanceType.LobbyGame, sceneLoadData.GetHashCode());
            //Debug.Log($"FishNetClientConnector.LoadLobbyGameScene({lobbyGame.gameId}) sceneloadDataHashCode {sceneLoadData.GetHashCode()}");

            fishNetNetworkManager.SceneManager.LoadConnectionScenes(networkConnection, sceneLoadData);
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
                clientId = networkManagerServer.GetClientIDForAccount(accountId);
                if (clientId == -1) {
                    continue;
                }
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
            sceneLoadData.Options.AutomaticallyUnload = false;
            sceneLoadData.Options.LocalPhysics = LocalPhysicsMode.Physics3D;
            sceneLoadData.Options.AllowStacking = true;
            sceneLoadData.PreferredActiveScene = new PreferredScene(SceneLookupData.CreateData(loadingSceneNode.SceneFile));
            networkManagerServer.SetLobbyGameLoadRequestHashcode(gameId, sceneLoadData.GetHashCode());
            networkManagerServer.SetSceneLoadRequestHashCode(SceneInstanceType.LobbyGame, sceneLoadData.GetHashCode());
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
            networkManagerServer.LeaveLobbyGame(gameId, networkConnection.ClientId);
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

        public void AdvertiseUnloadSceneServer(int accountId) {
            //Debug.Log($"FishNetClientConnector.AdvertiseLoadSceneServer({sceneResourceName}, {accountId})");

            int clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (clientId == -1) {
                return;
            }
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                return;
            }

            // unload the current scene for the client
            NetworkConnection networkConnection = fishNetNetworkManager.ServerManager.Clients[clientId];

            AdvertiseUnloadSceneClient(networkConnection);

            // testing - disabled this block because it seems to be instantly unloading the previous scene on the server
            // which is overriding the manual scene unload timers
            /*
            if (networkConnection.Scenes.Count == 0) {
                //Debug.Log($"FishNetClientConnector.AdvertiseLoadSceneServer() no scenes found for client {clientId}");
                //return;
            } else {
                //Debug.Log($"FishNetClientConnector.AdvertiseLoadSceneServer() unloading current scene {networkConnection.Scenes.First().name}({networkConnection.Scenes.First().handle}) for client {clientId}");
                SceneUnloadData sceneUnloadData = new SceneUnloadData(networkConnection.Scenes.First());
                base.NetworkManager.SceneManager.UnloadConnectionScenes(networkConnection, sceneUnloadData);
            }
            */
        }

        [TargetRpc]
        public void AdvertiseUnloadSceneClient(NetworkConnection networkConnection) {
            //Debug.Log($"FishNetClientConnector.AdvertiseLoadSceneClient({sceneName})");

            networkManagerClient.AdvertiseUnloadSceneClient();
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
                //Debug.Log($"FishNetClientConnector.AdvertiseAddSpawnRequestServer() could not find client id {accountId}");
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

            Dialog dialog = systemDataFactory.GetResource<Dialog>(dialogResourceName);
            if (dialog == null) {
                return;
            }

            Interactable interactable = null;
            if (targetNetworkInteractable != null) {
                interactable = targetNetworkInteractable.Interactable;
            }

            networkManagerServer.TurnInDialog(interactable, componentIndex, dialog, networkConnection.ClientId);
        }

        public void RequestTurnInQuestDialog(Dialog dialog) {
            RequestTurnInQuestDialogServer(dialog.ResourceName);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestTurnInQuestDialogServer(string dialogResourceName, NetworkConnection networkConnection = null) {

            Dialog dialog = systemDataFactory.GetResource<Dialog>(dialogResourceName);
            if (dialog == null) {
                return;
            }

            networkManagerServer.TurnInQuestDialog(dialog, networkConnection.ClientId);
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

            Interactable interactable = null;
            if (targetNetworkInteractable != null) {
                interactable = targetNetworkInteractable.Interactable;
            }
            networkManagerServer.SetPlayerCharacterClass(interactable, componentIndex, networkConnection.ClientId);
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

            Interactable interactable = null;
            if (targetNetworkInteractable != null) {
                interactable = targetNetworkInteractable.Interactable;
            }
            networkManagerServer.SetPlayerCharacterSpecialization(interactable, componentIndex, networkConnection.ClientId);
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

            Interactable interactable = null;
            if (targetNetworkInteractable != null) {
                interactable = targetNetworkInteractable.Interactable;
            }
            networkManagerServer.SetPlayerFaction(interactable, componentIndex, networkConnection.ClientId);
        }

        public void RequestCreateGuild(Interactable interactable, int componentIndex, string guildName) {
            //Debug.Log($"FishNetClientConnector.RequestCreateGuild({guildName})");

            FishNetInteractable networkInteractable = null;
            if (interactable != null) {
                networkInteractable = interactable.GetComponent<FishNetInteractable>();
            }
            RequestCreateGuildServer(networkInteractable, componentIndex, guildName);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestCreateGuildServer(FishNetInteractable targetNetworkInteractable, int componentIndex, string guildName, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.RequestCreateGuildServer({guildName})");

            Interactable interactable = null;
            if (targetNetworkInteractable != null) {
                interactable = targetNetworkInteractable.Interactable;
            }
            networkManagerServer.RequestCreateGuild(interactable, componentIndex, guildName, networkConnection.ClientId);
        }

        public void CheckGuildName(Interactable interactable, int componentIndex, string guildName) {
            FishNetInteractable networkInteractable = null;
            if (interactable != null) {
                networkInteractable = interactable.GetComponent<FishNetInteractable>();
            }
            CheckGuildNameServer(networkInteractable, componentIndex, guildName);
        }

        [ServerRpc(RequireOwnership = false)]
        public void CheckGuildNameServer(FishNetInteractable targetNetworkInteractable, int componentIndex, string guildName, NetworkConnection networkConnection = null) {
            Interactable interactable = null;
            if (targetNetworkInteractable != null) {
                interactable = targetNetworkInteractable.Interactable;
            }
            networkManagerServer.CheckGuildName(interactable, componentIndex, guildName, networkConnection.ClientId);
        }


        public void RequestSendMail(Interactable interactable, int componentIndex, MailMessageRequest sendMailRequest) {
            FishNetInteractable networkInteractable = null;
            if (interactable != null) {
                networkInteractable = interactable.GetComponent<FishNetInteractable>();
            }
            RequestSendMailServer(networkInteractable, componentIndex, sendMailRequest);

        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestSendMailServer(FishNetInteractable networkInteractable, int componentIndex, MailMessageRequest sendMailRequest, NetworkConnection networkConnection = null) {

            Interactable interactable = null;
            if (networkInteractable != null) {
                interactable = networkInteractable.Interactable;
            }
            networkManagerServer.RequestSendMail(interactable, componentIndex, sendMailRequest, networkConnection.ClientId);
        }

        public void RequestListAuctionItems(Interactable interactable, int componentIndex, ListAuctionItemRequest listAuctionItemRequest) {
            FishNetInteractable networkInteractable = null;
            if (interactable != null) {
                networkInteractable = interactable.GetComponent<FishNetInteractable>();
            }
            RequestListAuctionItemsServer(networkInteractable, componentIndex, listAuctionItemRequest);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestListAuctionItemsServer(FishNetInteractable networkInteractable, int componentIndex, ListAuctionItemRequest listAuctionItemRequest, NetworkConnection networkConnection = null) {
            Interactable interactable = null;
            if (networkInteractable != null) {
                interactable = networkInteractable.Interactable;
            }
            networkManagerServer.RequestListAuctionItems(interactable, componentIndex, listAuctionItemRequest, networkConnection.ClientId);
        }

        public void RequestSearchAuctions(Interactable interactable, int componentIndex, string searchText, bool onlyShowOwnAuctions) {
            FishNetInteractable networkInteractable = null;
            if (interactable != null) {
                networkInteractable = interactable.GetComponent<FishNetInteractable>();
            }
            RequestSearchAuctionsServer(networkInteractable, componentIndex, searchText, onlyShowOwnAuctions);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestSearchAuctionsServer(FishNetInteractable networkInteractable, int componentIndex, string searchText, bool onlyShowOwnAuctions, NetworkConnection networkConnection = null) {
            Interactable interactable = null;
            if (networkInteractable != null) {
                interactable = networkInteractable.Interactable;
            }
            networkManagerServer.RequestSearchAuctions(interactable, componentIndex, searchText, onlyShowOwnAuctions, networkConnection.ClientId);
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

            Interactable interactable = null;
            if (targetNetworkInteractable != null) {
                interactable = targetNetworkInteractable.Interactable;
            }
            networkManagerServer.LearnSkill(interactable, componentIndex, skillId, networkConnection.ClientId);
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

            Quest quest = systemDataFactory.GetResource<Quest>(questResourceName);
            if (quest == null) {
                return;
            }

            Interactable interactable = null;
            if (targetNetworkInteractable != null) {
                interactable = targetNetworkInteractable.Interactable;
            }

            networkManagerServer.AcceptQuest(interactable, componentIndex, quest, networkConnection.ClientId);
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

            Quest quest = systemDataFactory.GetResource<Quest>(questResourceName);
            if (quest == null) {
                return;
            }

            Interactable interactable = null;
            if (targetNetworkInteractable != null) {
                interactable = targetNetworkInteractable.Interactable;
            }

            networkManagerServer.CompleteQuest(interactable, componentIndex, quest, questRewardChoices, networkConnection.ClientId);
        }

        public void SellVendorItemClient(Interactable interactable, int componentIndex, long itemInstanceId) {
            FishNetInteractable networkInteractable = null;
            if (interactable != null) {
                networkInteractable = interactable.GetComponent<FishNetInteractable>();
            }
            SellVendorItemServer(networkInteractable, componentIndex, itemInstanceId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SellVendorItemServer(FishNetInteractable targetNetworkInteractable, int componentIndex, long itemInstanceId, NetworkConnection networkConnection = null) {
            Interactable interactable = null;
            if (targetNetworkInteractable != null) {
                interactable = targetNetworkInteractable.Interactable;
            }
            networkManagerServer.SellVendorItem(interactable, componentIndex, itemInstanceId, networkConnection.ClientId);
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

            Interactable interactable = null;
            if (targetNetworkInteractable != null) {
                interactable = targetNetworkInteractable.Interactable;
            }
            UnitProfile unitProfile = systemDataFactory.GetResource<UnitProfile>(unitProfileName);
            if (unitProfile == null) {
                return;
            }
            UnitToughness unitToughness = systemDataFactory.GetResource<UnitToughness>(unitToughnessName);
            networkManagerServer.RequestSpawnUnit(interactable, componentIndex, unitLevel, extraLevels, useDynamicLevel, unitProfile, unitToughness, networkConnection.ClientId);
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

            Interactable interactable = null;
            if (targetNetworkInteractable != null) {
                interactable = targetNetworkInteractable.Interactable;
            }
            networkManagerServer.BuyItemFromVendor(interactable, componentIndex, collectionIndex, itemIndex, resourceName, networkConnection.ClientId);
        }


        public void AdvertiseMessageFeedMessage(int accountId, string message) {
            //Debug.Log($"FishNetClientConnector.AdvertiseMessageFeedMessage({accountId}, {message})");

            int clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (clientId == -1) {
                return;
            }

            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                return;
            }
            AdvertiseMessageFeedMessageClient(fishNetNetworkManager.ServerManager.Clients[clientId], message);
        }

        [TargetRpc]
        public void AdvertiseMessageFeedMessageClient(NetworkConnection networkConnection, string message) {
            //Debug.Log($"FishNetClientConnector.AdvertiseMessageFeedMessageClient({message})");

            networkManagerClient.AdvertiseMessageFeedMessage(message);
        }

        public void AdvertiseSystemMessage(int accountId, string message) {

            int clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (clientId == -1) {
                return;
            }
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                return;
            }

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
            int clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (clientId == -1) {
                return;
            }
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                //Debug.Log($"FishNetClientConnector.AdvertiseAddToBuyBackCollection() could not find client id {clientId}");
                return;
            }
            AdvertiseAddToBuyBackCollectionClient(fishNetNetworkManager.ServerManager.Clients[clientId], networkCharacterUnit, networkInteractable, componentIndex, newInstantiatedItem.InstanceId);
        }

        [TargetRpc]
        public void AdvertiseAddToBuyBackCollectionClient(NetworkConnection networkConnection, FishNetUnitController networkCharacterUnit, FishNetInteractable networkInteractable, int componentIndex, long instantiatedItemId) {
            networkManagerClient.AdvertiseAddToBuyBackCollection(networkCharacterUnit.UnitController, networkInteractable.Interactable, componentIndex, instantiatedItemId);
        }

        public void TakeAllLoot() {
            TakeAllLootServer();
        }

        [ServerRpc(RequireOwnership = false)]
        public void TakeAllLootServer(NetworkConnection networkConnection = null) {
            networkManagerServer.TakeAllLoot(networkConnection.ClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestTakeLoot(int lootDropId, NetworkConnection networkConnection = null) {
            networkManagerServer.RequestTakeLoot(lootDropId, networkConnection.ClientId);
        }


        public void AddDroppedLoot(int accountId, int lootDropId, long itemId) {
            //Debug.Log($"FishNetClientConnector.AddDroppedLoot({accountId}, {lootDropId}, {itemId})");

            int clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (clientId == -1) {
                return;
            }
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                return;
            }
            AddDroppedLootClient(ServerManager.Clients[clientId], lootDropId, itemId);
        }

        [TargetRpc]
        public void AddDroppedLootClient(NetworkConnection networkConnection, int lootDropId, long itemInstanceId) {
            //Debug.Log($"FishNetClientConnector.AddDroppedLootClient({networkConnection.ClientId}, {lootDropId}, {itemId})");

            networkManagerClient.AddDroppedLoot(lootDropId, itemInstanceId);
        }

        public void AddAvailableDroppedLoot(int accountId, List<int> lootDropIds) {
            //Debug.Log($"FishNetClientConnector.AddAvailableDroppedLoot({accountId}, {items.Count})");

            int clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (clientId == -1) {
                return;
            }
            if (ServerManager.Clients.ContainsKey(clientId) == false) {
                return;
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

            int clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (clientId == -1) {
                return;
            }
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

        [ServerRpc(RequireOwnership = false)]
        public void RequestBeginCrafting(string recipeName, int craftAmount, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.RequestBeginCrafting({recipeName}, {craftAmount})");
            
            Recipe recipe = systemDataFactory.GetResource<Recipe>(recipeName);
            if (recipe == null) {
                return;
            }
            networkManagerServer.RequestBeginCrafting(recipe, craftAmount, networkConnection.ClientId);

        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestCancelCrafting(NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.RequestCancelCrafting()");

            networkManagerServer.RequestCancelCrafting(networkConnection.ClientId);
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

            Interactable interactable = null;
            if (targetNetworkInteractable != null) {
                interactable = targetNetworkInteractable.Interactable;
            }
            networkManagerServer.RequestUpdatePlayerAppearance(networkConnection.ClientId, interactable, componentIndex, unitProfileName, appearanceString, swappableMeshSaveData);
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

            Interactable interactable = null;
            if (targetNetworkInteractable != null) {
                interactable = targetNetworkInteractable.Interactable;
            }
            networkManagerServer.RequestChangePlayerName(interactable, componentIndex, newName, networkConnection.ClientId);
        }


        [ServerRpc(RequireOwnership = false)]
        public void RequestSpawnPet(string resourceName, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.RequestSpawnPet({resourceName})");

            UnitProfile unitProfile = systemDataFactory.GetResource<UnitProfile>(resourceName);
            if (unitProfile == null) {
                Debug.LogWarning($"FishNetClientConnector.RequestSpawnPet() could not find unit profile {resourceName}");
                return;
            }
            networkManagerServer.RequestSpawnPet(networkConnection.ClientId, unitProfile);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestDespawnPet(string resourceName, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.RequestDespawnPet({resourceName})");

            UnitProfile unitProfile = systemDataFactory.GetResource<UnitProfile>(resourceName);
            if (unitProfile == null) {
                Debug.LogWarning($"FishNetClientConnector.RequestDespawnPet() could not find unit profile {resourceName}");
                return;
            }
            networkManagerServer.RequestDespawnPet(networkConnection.ClientId, unitProfile);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestLoadPlayerCharacter(int playerCharacterId, NetworkConnection networkConnection = null) {
            networkManagerServer.RequestLoadPlayerCharacter(networkConnection.ClientId, playerCharacterId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void AcceptCharacterGroupInvite(int characterGroupId, NetworkConnection networkConnection = null) {
            networkManagerServer.AcceptCharacterGroupInvite(networkConnection.ClientId, characterGroupId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void DeclineCharacterGroupInvite(NetworkConnection networkConnection = null) {
            networkManagerServer.DeclineCharacterGroupInvite(networkConnection.ClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void AcceptGuildInvite(int guildId, NetworkConnection networkConnection = null) {
            networkManagerServer.AcceptGuildInvite(networkConnection.ClientId, guildId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void AcceptFriendInvite(int friendId, NetworkConnection networkConnection = null) {
            networkManagerServer.AcceptFriendInvite(networkConnection.ClientId, friendId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void DeclineGuildInvite(NetworkConnection networkConnection = null) {
            networkManagerServer.DeclineGuildInvite(networkConnection.ClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void DeclineFriendInvite(int friendId, NetworkConnection networkConnection = null) {
            networkManagerServer.DeclineFriendInvite(networkConnection.ClientId, friendId);
        }


        [ServerRpc(RequireOwnership = false)]
        public void RequestLeaveCharacterGroup(NetworkConnection networkConnection = null) {
            networkManagerServer.RequestLeaveCharacterGroup(networkConnection.ClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestLeaveGuild(NetworkConnection networkConnection = null) {
            networkManagerServer.RequestLeaveGuild(networkConnection.ClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestRemoveCharacterFromGroup(int playerCharacterId, NetworkConnection networkConnection = null) {
            networkManagerServer.RequestRemoveCharacterFromGroup(networkConnection.ClientId, playerCharacterId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestRemoveCharacterFromGuild(int playerCharacterId, NetworkConnection networkConnection = null) {
            networkManagerServer.RequestRemoveCharacterFromGuild(networkConnection.ClientId, playerCharacterId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestRemoveCharacterFromFriendList(int playerCharacterId, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.RequestRemoveCharacterFromFriendList({playerCharacterId})");

            networkManagerServer.RequestRemoveCharacterFromFriendList(networkConnection.ClientId, playerCharacterId);
        }


        [ServerRpc(RequireOwnership = false)]
        public void RequestInviteCharacterToGroup(int characterId, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.RequestInviteCharacterToGroup({characterId})");

            networkManagerServer.RequestInviteCharacterToGroup(networkConnection.ClientId, characterId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestInviteCharacterToGroup(string characterName, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.RequestInviteCharacterToGroup({characterId})");

            networkManagerServer.RequestInviteCharacterToGroup(networkConnection.ClientId, characterName);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestInviteCharacterToGuild(int characterId, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.RequestInviteCharacterToGuild({characterId})");

            networkManagerServer.RequestInviteCharacterToGuild(networkConnection.ClientId, characterId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestInviteCharacterToFriendList(int characterId, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.RequestInviteCharacterToGuild({characterId})");

            networkManagerServer.RequestInviteCharacterToFriendList(networkConnection.ClientId, characterId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestInviteCharacterToFriendList(string characterName, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.RequestInviteCharacterToGuild({characterId})");

            networkManagerServer.RequestInviteCharacterToFriendList(networkConnection.ClientId, characterName);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestInviteCharacterToGuild(string characterName, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.RequestInviteCharacterToGuild({characterId})");

            networkManagerServer.RequestInviteCharacterToGuild(networkConnection.ClientId, characterName);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestDisbandCharacterGroup(int characterGroupId, NetworkConnection networkConnection = null) {
            networkManagerServer.RequestDisbandCharacterGroup(networkConnection.ClientId, characterGroupId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestDisbandGuild(int characterGroupId, NetworkConnection networkConnection = null) {
            networkManagerServer.RequestDisbandGuild(networkConnection.ClientId, characterGroupId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestPromoteCharacterToLeader(int characterId, NetworkConnection networkConnection = null) {
            networkManagerServer.RequestPromoteCharacterToLeader(networkConnection.ClientId, characterId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestPromoteGuildCharacter(int characterId, NetworkConnection networkConnection = null) {
            networkManagerServer.RequestPromoteGuildCharacter(networkConnection.ClientId, characterId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestDemoteGuildCharacter(int characterId, NetworkConnection networkConnection = null) {
            networkManagerServer.RequestDemoteGuildCharacter(networkConnection.ClientId, characterId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestPromoteGroupCharacter(int characterId, NetworkConnection networkConnection = null) {
            networkManagerServer.RequestPromoteGroupCharacter(networkConnection.ClientId, characterId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestDemoteGroupCharacter(int characterId, NetworkConnection networkConnection = null) {
            networkManagerServer.RequestDemoteGroupCharacter(networkConnection.ClientId, characterId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestBeginTrade(int characterId, NetworkConnection networkConnection = null) {

            networkManagerServer.RequestBeginTrade(networkConnection.ClientId, characterId);
        }

        [ServerRpc(RequireOwnership = false)]
        internal void RequestDeclineTrade(NetworkConnection networkConnection = null) {

            networkManagerServer.RequestDeclineTrade(networkConnection.ClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestAcceptTrade(NetworkConnection networkConnection = null) {
            //Debug.Log($"FishnetClientConnector.RequestAcceptTrade()");

            networkManagerServer.RequestAcceptTrade(networkConnection.ClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestAddItemsToTradeSlot(int buttonIndex, List<long> itemInstanceIdList, NetworkConnection networkConnection = null) {

            networkManagerServer.RequestAddItemsToTradeSlot(networkConnection.ClientId, buttonIndex, itemInstanceIdList);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestAddCurrencyToTrade(int amount, NetworkConnection networkConnection = null) {

            networkManagerServer.RequestAddCurrencyToTrade(networkConnection.ClientId, amount);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestCancelTrade(NetworkConnection networkConnection = null) {
            networkManagerServer.RequestCancelTrade(networkConnection.ClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestConfirmTrade(NetworkConnection networkConnection = null) {
            networkManagerServer.RequestConfirmTrade(networkConnection.ClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestUnconfirmTrade(NetworkConnection networkConnection = null) {
            networkManagerServer.RequestUnconfirmTrade(networkConnection.ClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestDeleteMailMessage(int messageId, NetworkConnection networkConnection = null) {
            networkManagerServer.RequestDeleteMailMessage(networkConnection.ClientId, messageId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestTakeMailAttachment(int messageId, int attachmentSlotId, NetworkConnection networkConnection = null) {
            networkManagerServer.RequestTakeMailAttachment(networkConnection.ClientId, messageId, attachmentSlotId);

        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestTakeMailAttachments(int messageId, NetworkConnection networkConnection = null) {
            networkManagerServer.RequestTakeMailAttachments(networkConnection.ClientId, messageId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestMarkMailAsRead(int messageId, NetworkConnection networkConnection = null) {
            networkManagerServer.RequestMarkMailAsRead(networkConnection.ClientId, messageId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestBuyAuctionItem(int auctionItemId, NetworkConnection networkConnection = null) {
            networkManagerServer.RequestBuyAuctionItem(networkConnection.ClientId, auctionItemId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestCancelAuction(int auctionItemId, NetworkConnection networkConnection = null) {

            networkManagerServer.RequestCancelAuction(networkConnection.ClientId, auctionItemId);
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

        public void RequestLogout() {
            //Debug.Log("FishNetClientConnector.RequestLogout()");

            RequestLogoutServer();
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestLogoutServer(NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetClientConnector.RequestLogout()");

            networkManagerServer.LogoutByClientId(networkConnection.ClientId);
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

            int clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                return;
            }

            // testing - disabled this block because it seems to be instantly unloading the previous scene on the server
            // which is overriding the manual scene unload timers
            /*
            // unload the current scene for the client
            NetworkConnection networkConnection = fishNetNetworkManager.ServerManager.Clients[clientId];
            SceneUnloadData sceneUnloadData = new SceneUnloadData(networkConnection.Scenes.First());
            base.NetworkManager.SceneManager.UnloadConnectionScenes(networkConnection, sceneUnloadData);
            */

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

        public void AdvertiseAddCharacterToGroup(int accountId, int characterGroupId, CharacterGroupMemberNetworkData characterGroupMemberNetworkData) {
            //Debug.Log($"FishNetClientConnector.AdvertiseAddCharacterToGroup({playerCharacterId}, {characterGroup.characterGroupId})");

            int clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                //Debug.Log($"FishNetClientConnector.AdvertiseAddCharacterToGroup() could not find client id {clientId}");
                return;
            }
            AdvertiseAddCharacterToGroupClient(fishNetNetworkManager.ServerManager.Clients[clientId], characterGroupId, characterGroupMemberNetworkData);

        }

        [TargetRpc]
        public void AdvertiseAddCharacterToGroupClient(NetworkConnection networkConnection, int characterGroupId, CharacterGroupMemberNetworkData characterGroupMemberNetworkData) {
            //Debug.Log($"FishNetClientConnector.AdvertiseAddCharacterToGroupClient({playerCharacterId}, {characterGroup.characterGroupId})");

            networkManagerClient.ProcessAddCharacterToGroup(characterGroupId, characterGroupMemberNetworkData);
        }

        public void AdvertiseAddCharacterToGuild(int accountId, int guildId, GuildMemberNetworkData guildMemberNetworkData) {
            //Debug.Log($"FishNetClientConnector.AdvertiseAddCharacterToGuild({existingAccountId}, {guildId})");

            int clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                //Debug.Log($"FishNetClientConnector.AdvertiseAddCharacterToGuild() could not find client id {clientId}");
                return;
            }
            AdvertiseAddCharacterToGuildClient(fishNetNetworkManager.ServerManager.Clients[clientId], guildId, guildMemberNetworkData);
        }

        [TargetRpc]
        public void AdvertiseAddCharacterToGuildClient(NetworkConnection networkConnection, int guildId, GuildMemberNetworkData guildMemberNetworkData) {
            //Debug.Log($"FishNetClientConnector.AdvertiseAddCharacterToGuildClient(guildId: {guildId}, {characterSummaryNetworkData.CharacterName})");

            networkManagerClient.ProcessCharacterJoinGuild(guildId, guildMemberNetworkData);
        }

        public void AdvertiseAddFriend(int accountId, CharacterSummaryNetworkData characterSummaryNetworkData) {
            //Debug.Log($"FishNetClientConnector.AdvertiseAddCharacterToGroup({playerCharacterId}, {characterGroup.characterGroupId})");

            int clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                //Debug.Log($"FishNetClientConnector.AdvertiseAddCharacterToGroup() could not find client id {clientId}");
                return;
            }
            AdvertiseAddFriendClient(fishNetNetworkManager.ServerManager.Clients[clientId], characterSummaryNetworkData);
        }

        [TargetRpc]
        public void AdvertiseAddFriendClient(NetworkConnection networkConnection, CharacterSummaryNetworkData characterSummaryNetworkData) {
            //Debug.Log($"FishNetClientConnector.AdvertiseAddCharacterToGuildClient({playerCharacterId}, {guild.guildId})");
            networkManagerClient.ProcessAddFriend(characterSummaryNetworkData);
        }

        public void AdvertiseCharacterGroup(int accountId, CharacterGroupNetworkData characterGroupNetworkData) {
            //Debug.Log($"FishNetClientConnector.AdvertiseCharacterGroup({accountId}, {characterGroup.characterGroupId})");

            int clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                //Debug.Log($"FishNetClientConnector.AdvertiseCharacterGroup() could not find client id {clientId}");
                return;
            }
            AdvertiseCharacterGroupClient(fishNetNetworkManager.ServerManager.Clients[clientId], characterGroupNetworkData);
        }

        [TargetRpc]
        public void AdvertiseCharacterGroupClient(NetworkConnection networkConnection, CharacterGroupNetworkData characterGroupNetworkData) {
            //Debug.Log($"FishNetClientConnector.AdvertiseCharacterGroup({characterGroup.characterGroupId})");

            networkManagerClient.ProcessLoadCharacterGroup(characterGroupNetworkData);
        }

        public void AdvertiseGuild(int accountId, GuildNetworkData guildNetworkData) {
            //Debug.Log($"FishNetClientConnector.AdvertiseGuild({accountId}, {characterGroup.characterGroupId})");

            int clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                //Debug.Log($"FishNetClientConnector.AdvertiseGuild() could not find client id {clientId}");
                return;
            }
            AdvertiseGuildClient(fishNetNetworkManager.ServerManager.Clients[clientId], guildNetworkData);
        }

        [TargetRpc]
        public void AdvertiseGuildClient(NetworkConnection networkConnection, GuildNetworkData guildNetworkData) {
            //Debug.Log($"FishNetClientConnector.AdvertiseAddCharacterToGroupClient({guild.guildId})");
            networkManagerClient.ProcessLoadGuild(guildNetworkData);
        }

        public void AdvertiseFriendList(int accountId, FriendListNetworkData friendListNetworkData) {
            //Debug.Log($"FishNetClientConnector.AdvertiseGuild({accountId}, {characterGroup.characterGroupId})");

            int clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                //Debug.Log($"FishNetClientConnector.AdvertiseGuild() could not find client id {clientId}");
                return;
            }
            AdvertiseFriendListClient(fishNetNetworkManager.ServerManager.Clients[clientId], friendListNetworkData);
        }

        [TargetRpc]
        public void AdvertiseFriendListClient(NetworkConnection networkConnection, FriendListNetworkData friendListNetworkData) {
            //Debug.Log($"FishNetClientConnector.AdvertiseAddCharacterToGroupClient({guild.guildId})");
            networkManagerClient.ProcessLoadFriendList(friendListNetworkData);
        }

        public void AdvertiseGuildNameAvailable(int accountId) {
            //Debug.Log($"FishNetClientConnector.AdvertiseGuildNameAvailable({accountId})");

            int clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseGuild() could not find client id {clientId}");
                return;
            }
            AdvertiseGuildNameAvailableClient(fishNetNetworkManager.ServerManager.Clients[clientId]);
        }

        [TargetRpc]
        public void AdvertiseGuildNameAvailableClient(NetworkConnection networkConnection) {
            //Debug.Log($"FishNetClientConnector.AdvertiseGuildNameAvailableClient()");
            networkManagerClient.ProcessGuildNameAvailable();
        }


        public void AdvertiseRemoveCharacterFromGroup(int accountId, int removedCharacterId, int characterGroupId) {
            //Debug.Log($"FishNetClientConnector.AdvertiseRemoveCharacterFromGroup({removedCharacterId}, {characterGroup.characterGroupId})");

            int _clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseAddCharacterToGroup() could not find client id {_clientId}");
                return;
            }
            AdvertiseRemoveCharacterFromGroupClient(fishNetNetworkManager.ServerManager.Clients[_clientId], removedCharacterId, characterGroupId);
        }

        [TargetRpc]
        public void AdvertiseRemoveCharacterFromGroupClient(NetworkConnection networkConnection, int removedPlayerId, int characterGroupId) {
            networkManagerClient.ProcessCharacterLeaveGroup(removedPlayerId, characterGroupId);
        }

        public void AdvertiseRemoveCharacterFromGuild(int accountId, int removedCharacterId, int guildId) {
            //Debug.Log($"FishNetClientConnector.AdvertiseRemoveCharacterFromGroup({removedCharacterId}, {characterGroup.characterGroupId})");

            int _clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseAddCharacterToGroup() could not find client id {_clientId}");
                return;
            }
            AdvertiseRemoveCharacterFromGuildClient(fishNetNetworkManager.ServerManager.Clients[_clientId], removedCharacterId, guildId);
        }

        [TargetRpc]
        public void AdvertiseRemoveCharacterFromGuildClient(NetworkConnection networkConnection, int removedPlayerId, int guildId) {
            networkManagerClient.ProcessCharacterLeaveGuild(removedPlayerId, guildId);
        }

        public void AdvertiseRemoveCharacterFromFriendList(int accountId, int removedCharacterId) {
            //Debug.Log($"FishNetClientConnector.AdvertiseRemoveCharacterFromFriendList({accountId}, {removedCharacterId})");

            int clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseAddCharacterToGroup() could not find client id {clientId}");
                return;
            }
            AdvertiseRemoveCharacterFromFriendListClient(fishNetNetworkManager.ServerManager.Clients[clientId], removedCharacterId);
        }

        [TargetRpc]
        public void AdvertiseRemoveCharacterFromFriendListClient(NetworkConnection networkConnection, int removedPlayerId) {
            //Debug.Log($"FishNetClientConnector.AdvertiseRemoveCharacterFromFriendListClient({removedPlayerId})");

            networkManagerClient.ProcessRemoveCharacterFromFriendList(removedPlayerId);
        }

        public void AdvertiseCharacterGroupInvite(int invitedCharacterId, int characterGroupId, string leaderName) {
            //Debug.Log($"FishNetClientConnector.AdvertiseCharacterGroupInvite({invitedCharacterId}, {characterGroup.characterGroupId}, {leaderName})");

            int accountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(invitedCharacterId);
            int clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseCharacterGroupInvite() could not find client id {clientId}");
                return;
            }
            AdvertiseCharacterGroupInviteClient(fishNetNetworkManager.ServerManager.Clients[clientId], characterGroupId, leaderName);
        }

        [TargetRpc]
        public void AdvertiseCharacterGroupInviteClient(NetworkConnection networkConnection, int characterGroupId, string leaderName) {
            networkManagerClient.ProcessCharacterGroupInvite(characterGroupId, leaderName);
        }

        public void AdvertiseGuildInvite(int invitedCharacterId, int guildId, string leaderName) {
            //Debug.Log($"FishNetClientConnector.AdvertiseGuildInvite({invitedCharacterId}, {characterGroup.characterGroupId}, {leaderName})");

            int accountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(invitedCharacterId);
            int clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseGuildInvite() could not find client id {clientId}");
                return;
            }
            AdvertiseGuildInviteClient(fishNetNetworkManager.ServerManager.Clients[clientId], guildId, leaderName);
        }

        [TargetRpc]
        public void AdvertiseGuildInviteClient(NetworkConnection networkConnection, int guildId, string leaderName) {
            networkManagerClient.ProcessGuildInvite(guildId, leaderName);
        }

        public void AdvertiseFriendInvite(int accountId, int inviterCharacterId, string characterName) {
            //Debug.Log($"FishNetClientConnector.AdvertiseGuildInvite({invitedCharacterId}, {characterGroup.characterGroupId}, {leaderName})");

            int clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseGuildInvite() could not find client id {clientId}");
                return;
            }
            AdvertiseFriendInviteClient(fishNetNetworkManager.ServerManager.Clients[clientId], inviterCharacterId, characterName);
        }

        [TargetRpc]
        public void AdvertiseFriendInviteClient(NetworkConnection networkConnection, int inviterCharacterId, string leaderName) {
            networkManagerClient.ProcessFriendInvite(inviterCharacterId, leaderName);
        }

        public void AdvertiseDisbandCharacterGroup(int accountId, int characterGroupId) {
            int _clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseAddCharacterToGroup() could not find client id {_clientId}");
                return;
            }
            AdvertiseDisbandCharacterGroupClient(fishNetNetworkManager.ServerManager.Clients[_clientId], characterGroupId);
        }

        [TargetRpc]
        public void AdvertiseDisbandCharacterGroupClient(NetworkConnection networkConnection, int characterGroupId) {
            networkManagerClient.ProcessDisbandCharacterGroup(characterGroupId);
        }

        public void AdvertiseDisbandGuild(int accountId, int guildId) {
            int _clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseAddCharacterToGroup() could not find client id {_clientId}");
                return;
            }
            AdvertiseDisbandGuildClient(fishNetNetworkManager.ServerManager.Clients[_clientId], guildId);
        }

        [TargetRpc]
        public void AdvertiseDisbandGuildClient(NetworkConnection networkConnection, int guildId) {
            networkManagerClient.ProcessDisbandGuild(guildId);
        }

        public void AdvertiseRenameCharacterInGroup(int accountId, int groupId, int characterId, string newName) {
            int _clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseRenameCharacterInGroup() could not find client id {_clientId}");
                return;
            }
            AdvertiseRenameCharacterInGroupClient(fishNetNetworkManager.ServerManager.Clients[_clientId], groupId, characterId, newName);
        }

        [TargetRpc]
        public void AdvertiseRenameCharacterInGroupClient(NetworkConnection networkConnection, int characterGroupId, int characterId, string newName) {
            networkManagerClient.ProcessRenameCharacterInGroup(characterGroupId, characterId, newName);
        }

        public void AdvertiseRenameCharacterInGuild(int accountId, int guildId, int characterId, string newName) {
            int _clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseRenameCharacterInGroup() could not find client id {_clientId}");
                return;
            }
            AdvertiseRenameCharacterInGuildClient(fishNetNetworkManager.ServerManager.Clients[_clientId], guildId, characterId, newName);
        }

        [TargetRpc]
        public void AdvertiseRenameCharacterInGuildClient(NetworkConnection networkConnection, int guildId, int characterId, string newName) {
            networkManagerClient.ProcessRenameCharacterInGuild(guildId, characterId, newName);
        }

        public void AdvertiseRenameCharacterInFriendList(int accountId, int characterId, string newName) {
            int _clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseRenameCharacterInFriendList() could not find client id {_clientId}");
                return;
            }
            AdvertiseRenameCharacterInFriendListClient(fishNetNetworkManager.ServerManager.Clients[_clientId], characterId, newName);
        }

        [TargetRpc]
        public void AdvertiseRenameCharacterInFriendListClient(NetworkConnection networkConnection, int characterId, string newName) {
            networkManagerClient.ProcessRenameCharacterInFriendList(characterId, newName);
        }

        public void AdvertiseCharacterGroupMemberStatusChange(int accountId, int characterGroupId, int playerCharacterId, CharacterGroupMemberNetworkData characterGroupMemberNetworkData) {
            int _clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseCharacterGroupMemberOnline() could not find client id {_clientId}");
                return;
            }
            AdvertiseCharacterGroupMemberStatusChangeClient(fishNetNetworkManager.ServerManager.Clients[_clientId], characterGroupId, playerCharacterId, characterGroupMemberNetworkData);
        }

        [TargetRpc]
        public void AdvertiseCharacterGroupMemberStatusChangeClient(NetworkConnection networkConnection, int characterGroupId, int playerCharacterId, CharacterGroupMemberNetworkData characterGroupMemberNetworkData) {
            networkManagerClient.ProcessCharacterGroupMemberStatusChange(characterGroupId, playerCharacterId, characterGroupMemberNetworkData);
        }

        public void AdvertiseGuildMemberStatusChange(int accountId, int guildId, int playerCharacterId, GuildMemberNetworkData guildMemberNetworkData) {
            int _clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseGuildMemberOnline() could not find client id {_clientId}");
                return;
            }
            AdvertiseGuildMemberStatusChangeClient(fishNetNetworkManager.ServerManager.Clients[_clientId], guildId, playerCharacterId, guildMemberNetworkData);
        }

        [TargetRpc]
        public void AdvertiseGuildMemberStatusChangeClient(NetworkConnection networkConnection, int guildId, int playerCharacterId, GuildMemberNetworkData guildMemberNetworkData) {
            networkManagerClient.ProcessGuildMemberStatusChange(guildId, playerCharacterId, guildMemberNetworkData);
        }

        public void AdvertiseFriendStateChange(int accountId, int playerCharacterId, CharacterSummaryNetworkData characterSummaryNetworkData) {
            int clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseFriendOnline() could not find client id {_clientId}");
                return;
            }
            AdvertiseFriendStateChangeClient(fishNetNetworkManager.ServerManager.Clients[clientId], playerCharacterId, characterSummaryNetworkData);
        }

        [TargetRpc]
        public void AdvertiseFriendStateChangeClient(NetworkConnection networkConnection, int playerCharacterId, CharacterSummaryNetworkData characterSummaryNetworkData) {
            networkManagerClient.ProcessFriendStateChange(playerCharacterId, characterSummaryNetworkData);
        }

        public void AdvertisePlayerNameNotAvailable(int accountId) {
            int clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseCharacterGroupInvite() could not find client id {clientId}");
                return;
            }
            AdvertisePlayerNameNotAvailableClient(fishNetNetworkManager.ServerManager.Clients[clientId]);
        }

        [TargetRpc]
        public void AdvertisePlayerNameNotAvailableClient(NetworkConnection networkConnection) {
            networkManagerClient.AdvertisePlayerNameNotAvailable();
        }

        public void AdvertiseDeclineCharacterGroupInvite(int leaderAccountId, string decliningPlayerName) {
            int clientId = networkManagerServer.GetClientIDForAccount(leaderAccountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseCharacterGroupInvite() could not find client id {clientId}");
                return;
            }
            AdvertiseDeclineCharacterGroupInviteClient(fishNetNetworkManager.ServerManager.Clients[clientId], decliningPlayerName);
        }

        [TargetRpc]
        public void AdvertiseDeclineCharacterGroupInviteClient(NetworkConnection networkConnection, string decliningPlayerName) {
            networkManagerClient.ProcessDeclineCharacterGroupInvite(decliningPlayerName);
        }

        public void AdvertiseDeclineGuildInvite(int leaderAccountId, string decliningPlayerName) {
            int clientId = networkManagerServer.GetClientIDForAccount(leaderAccountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseCharacterGroupInvite() could not find client id {clientId}");
                return;
            }
            AdvertiseDeclineGuildInviteClient(fishNetNetworkManager.ServerManager.Clients[clientId], decliningPlayerName);
        }

        [TargetRpc]
        public void AdvertiseDeclineGuildInviteClient(NetworkConnection networkConnection, string decliningPlayerName) {
            networkManagerClient.ProcessDeclineGuildInvite(decliningPlayerName);
        }

        public void AdvertiseDeclineFriendInvite(int accountId, string decliningPlayerName) {
            int _clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseCharacterGroupInvite() could not find client id {clientId}");
                return;
            }
            AdvertiseDeclineFriendInviteClient(fishNetNetworkManager.ServerManager.Clients[_clientId], decliningPlayerName);
        }

        [TargetRpc]
        public void AdvertiseDeclineFriendInviteClient(NetworkConnection networkConnection, string decliningPlayerName) {
            networkManagerClient.ProcessDeclineFriendInvite(decliningPlayerName);
        }

        public void AdvertisePromoteGroupLeader(int accountId, int characterGroupId, int newLeaderCharacterId) {
            int _clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseAddCharacterToGroup() could not find client id {_clientId}");
                return;
            }
            AdvertisePromoteGroupLeaderClient(fishNetNetworkManager.ServerManager.Clients[_clientId], characterGroupId, newLeaderCharacterId);
        }

        [TargetRpc]
        public void AdvertisePromoteGroupLeaderClient(NetworkConnection networkConnection, int characterGroupId, int newLeaderCharacterId) {
            networkManagerClient.ProcessPromoteGroupLeader(characterGroupId, newLeaderCharacterId);
        }

        public void AdvertisePromoteGuildLeader(int accountId, int guildId, int newLeaderCharacterId) {
            int _clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseAddCharacterToGroup() could not find client id {_clientId}");
                return;
            }
            AdvertisePromoteGuildLeaderClient(fishNetNetworkManager.ServerManager.Clients[_clientId], guildId, newLeaderCharacterId);
        }

        [TargetRpc]
        public void AdvertisePromoteGuildLeaderClient(NetworkConnection networkConnection, int guildId, int newLeaderCharacterId) {
            networkManagerClient.ProcessPromoteGuildLeader(guildId, newLeaderCharacterId);
        }

        public void AdvertiseGroupMessage(int accountId, int characterGroupId, string messageText) {
            int _clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseGroupMessage() could not find client id {_clientId}");
                return;
            }
            AdvertiseGroupMessageClient(fishNetNetworkManager.ServerManager.Clients[_clientId], characterGroupId, messageText);
        }

        [TargetRpc]
        public void AdvertiseGroupMessageClient(NetworkConnection networkConnection, int characterGroupId, string messageText) {
            networkManagerClient.AdvertiseGroupMessage(characterGroupId, messageText);
        }

        public void AdvertiseGuildMessage(int accountId, int guildId, string messageText) {
            int _clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseGroupMessage() could not find client id {_clientId}");
                return;
            }
            AdvertiseGuildMessageClient(fishNetNetworkManager.ServerManager.Clients[_clientId], guildId, messageText);
        }

        [TargetRpc]
        public void AdvertiseGuildMessageClient(NetworkConnection networkConnection, int guildId, string messageText) {
            networkManagerClient.AdvertiseGuildMessage(guildId, messageText);
        }


        public void AdvertisePrivateMessage(int targetAccountId, string messageText) {
            //Debug.Log($"FishNetClientConnector.AdvertisePrivateMessage({targetAccountId}, {messageText})");

            int _clientId = networkManagerServer.GetClientIDForAccount(targetAccountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertisePrivateMessage() could not find client id {_clientId}");
                return;
            }
            AdvertisePrivateMessageClient(fishNetNetworkManager.ServerManager.Clients[_clientId], messageText);
        }

        [TargetRpc]
        public void AdvertisePrivateMessageClient(NetworkConnection networkConnection, string messageText) {
            //Debug.Log($"FishNetClientConnector.AdvertisePrivateMessageClient({messageText})");

            networkManagerClient.AdvertisePrivateMessage(messageText);
        }

        public void AdvertiseAcceptTradeInvite(int accountId, int targetCharacterId) {
            //Debug.Log($"FishNetClientConnector.AdvertiseAcceptTradeInvite({accountId}, {targetCharacterId})");

            int _clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseAcceptTradeInvite() could not find client id {_clientId}");
                return;
            }
            AdvertiseAcceptTradeInviteClient(fishNetNetworkManager.ServerManager.Clients[_clientId], targetCharacterId);
        }

        [TargetRpc]
        public void AdvertiseAcceptTradeInviteClient(NetworkConnection networkConnection, int characterId) {
            //Debug.Log($"FishNetClientConnector.AdvertiseAcceptTradeInviteClient({characterId})");

            networkManagerClient.AdvertiseAcceptTradeInvite(characterId);
        }

        public void AdvertiseDeclineTradeInvite(int accountId) {
            int _clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseDeclineTradeInvite() could not find client id {_clientId}");
                return;
            }
            AdvertiseDeclineTradeInviteClient(fishNetNetworkManager.ServerManager.Clients[_clientId]);
        }

        [TargetRpc]
        public void AdvertiseDeclineTradeInviteClient(NetworkConnection networkConnection) {
            networkManagerClient.AdvertiseDeclineTradeInvite();
        }

        public void AdvertiseRequestBeginTrade(int targetAccountId, int sourceCharacterId) {
            int _clientId = networkManagerServer.GetClientIDForAccount(targetAccountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseRequestBeginTrade() could not find client id {_clientId}");
                return;
            }
            AdvertiseRequestBeginTradeClient(fishNetNetworkManager.ServerManager.Clients[_clientId], sourceCharacterId);
        }

        [TargetRpc]
        public void AdvertiseRequestBeginTradeClient(NetworkConnection networkConnection, int sourceCharacterId) {
            networkManagerClient.AdvertiseRequestBeginTrade(sourceCharacterId);
        }

        public void AdvertiseAddItemsToTargetTradeSlot(int accountId, int buttonIndex, List<long> itemInstanceIdList) {
            int _clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseRequestBeginTrade() could not find client id {_clientId}");
                return;
            }
            AdvertiseAddItemsToTargetTradeSlotClient(fishNetNetworkManager.ServerManager.Clients[_clientId], buttonIndex, itemInstanceIdList);
        }

        [TargetRpc]
        public void AdvertiseAddItemsToTargetTradeSlotClient(NetworkConnection networkConnection, int buttonIndex, List<long> itemInstanceIdList) {
            networkManagerClient.AdvertiseAddItemsToTargetTradeSlot(buttonIndex, itemInstanceIdList);
        }

        public void AdvertiseAddCurrencyToTrade(int accountId, int amount) {
            int _clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseRequestBeginTrade() could not find client id {_clientId}");
                return;
            }
            AdvertiseAddCurrencyToTradeClient(fishNetNetworkManager.ServerManager.Clients[_clientId], amount);
        }

        [TargetRpc]
        private void AdvertiseAddCurrencyToTradeClient(NetworkConnection networkConnection, int amount) {
            networkManagerClient.AdvertiseAddCurrencyToTrade(amount);
        }

        public void AdvertiseCancelTrade(int accountId) {
            int _clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseRequestBeginTrade() could not find client id {_clientId}");
                return;
            }
            AdvertiseCancelTradeClient(fishNetNetworkManager.ServerManager.Clients[_clientId]);
        }

        [TargetRpc]
        private void AdvertiseCancelTradeClient(NetworkConnection networkConnection) {
            networkManagerClient.AdvertiseCancelTrade();
        }

        public void AdvertiseCompleteTrade(int accountId) {
            int _clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseRequestBeginTrade() could not find client id {_clientId}");
                return;
            }
            AdvertiseTradeCompleteClient(fishNetNetworkManager.ServerManager.Clients[_clientId]);
        }

        [TargetRpc]
        private void AdvertiseTradeCompleteClient(NetworkConnection networkConnection) {
            networkManagerClient.AdvertiseTradeComplete();
        }

        public void AdvertiseMailMessages(int accountId, MailMessageListBundle mailMessageListResponse) {
            int _clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseRequestBeginTrade() could not find client id {_clientId}");
                return;
            }
            AdvertiseMailMessagesClient(fishNetNetworkManager.ServerManager.Clients[_clientId], mailMessageListResponse);
        }

        [TargetRpc]
        public void AdvertiseMailMessagesClient(NetworkConnection networkConnection, MailMessageListBundle mailMessageListResponse) {
            networkManagerClient.AdvertiseMailMessages(mailMessageListResponse);
        }

        public void AdvertiseDeleteMailMessage(int accountId, int messageId) {
            int _clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseRequestBeginTrade() could not find client id {_clientId}");
                return;
            }
            AdvertiseDeleteMailMessageClient(fishNetNetworkManager.ServerManager.Clients[_clientId], messageId);
        }

        public void AdvertiseAuctionItems(int accountId, AuctionItemSearchListResult auctionItemListResponse) {
            int _clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseRequestBeginTrade() could not find client id {_clientId}");
                return;
            }
            AdvertiseAuctionItemsClient(fishNetNetworkManager.ServerManager.Clients[_clientId], auctionItemListResponse);
        }

        [TargetRpc]
        public void AdvertiseAuctionItemsClient(NetworkConnection networkConnection, AuctionItemSearchListResult auctionItemListResponse) {
            networkManagerClient.AdvertiseAuctionItems(auctionItemListResponse);
        }


        [TargetRpc]
        public void AdvertiseDeleteMailMessageClient(NetworkConnection networkConnection, int messageId) {
            networkManagerClient.AdvertiseDeleteMailMessage(messageId);
        }

        public void AdvertiseTakeMailAttachment(int accountId, int messageId, int attachmentSlotId) {
            int _clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseRequestBeginTrade() could not find client id {_clientId}");
                return;
            }
            AdvertiseTakeMailAttachmentClient(fishNetNetworkManager.ServerManager.Clients[_clientId], messageId, attachmentSlotId);
        }

        [TargetRpc]
        public void AdvertiseTakeMailAttachmentClient(NetworkConnection networkConnection, int messageId, int attachmentSlotId) {
            networkManagerClient.AdvertiseTakeMailAttachment(messageId, attachmentSlotId);
        }

        public void AdvertiseTakeMailAttachments(int accountId, int messageId) {
            int _clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseRequestBeginTrade() could not find client id {_clientId}");
                return;
            }
            AdvertiseTakeMailAttachmentsClient(fishNetNetworkManager.ServerManager.Clients[_clientId], messageId);
        }

        [TargetRpc]
        public void AdvertiseTakeMailAttachmentsClient(NetworkConnection networkConnection, int messageId) {
            networkManagerClient.AdvertiseTakeMailAttachments(messageId);
        }

        public void AdvertiseConfirmationPopup(int accountId, string messageText) {
            int _clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseRequestBeginTrade() could not find client id {_clientId}");
                return;
            }
            AdvertiseConfirmationPopupClient(fishNetNetworkManager.ServerManager.Clients[_clientId], messageText);
        }

        [TargetRpc]
        public void AdvertiseConfirmationPopupClient(NetworkConnection networkConnection, string messageText) {
            networkManagerClient.AdvertiseConfirmationPopup(messageText);
        }

        public void AdvertiseMailSend(int accountId) {
            int _clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseRequestBeginTrade() could not find client id {_clientId}");
                return;
            }
            AdvertiseMailSendClient(fishNetNetworkManager.ServerManager.Clients[_clientId]);
        }

        [TargetRpc]
        public void AdvertiseMailSendClient(NetworkConnection networkConnection) {
            networkManagerClient.AdvertiseMailSend();
        }

        public void AdvertiseBuyAuctionItem(int accountId, int auctionItemId) {
            int _clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseRequestBeginTrade() could not find client id {_clientId}");
                return;
            }
            AdvertiseBuyAuctionItemClient(fishNetNetworkManager.ServerManager.Clients[_clientId], auctionItemId);
        }

        [TargetRpc]
        public void AdvertiseBuyAuctionItemClient(NetworkConnection networkConnection, int auctionItemId) {
            networkManagerClient.AdvertiseBuyAuctionItem(auctionItemId);
        }

        public void AdvertiseCancelAuction(int accountId, int auctionItemId) {
            int _clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseRequestBeginTrade() could not find client id {_clientId}");
                return;
            }
            AdvertiseCancelAuctionClient(fishNetNetworkManager.ServerManager.Clients[_clientId], auctionItemId);
        }

        [TargetRpc]
        public void AdvertiseCancelAuctionClient(NetworkConnection networkConnection, int auctionItemId) {
            networkManagerClient.AdvertiseCancelAuction(auctionItemId);
        }

        public void AdvertiseListAuctionItems(int accountId) {
            int _clientId = networkManagerServer.GetClientIDForAccount(accountId);
            if (fishNetNetworkManager.ServerManager.Clients.ContainsKey(_clientId) == false) {
               //Debug.Log($"FishNetClientConnector.AdvertiseRequestBeginTrade() could not find client id {_clientId}");
                return;
            }
            AdvertiseListAuctionItemsClient(fishNetNetworkManager.ServerManager.Clients[_clientId]);
        }

        [TargetRpc]
        public void AdvertiseListAuctionItemsClient(NetworkConnection networkConnection) {
            networkManagerClient.AdvertiseListAuctionItems();
        }

        /*
        public override void OnStartServer() {
            base.OnStartServer();
           //Debug.Log($"FishNetClientConnector.OnStartServer()");

            // on server gameMode should always bet set to network
            //Debug.Log($"FishNetClientConnector.OnStartServer(): setting gameMode to network");
            systemGameManager.SetGameMode(GameMode.Network);
            networkManagerServer.ActivateServerMode();
        }

        public override void OnStopServer() {
            base.OnStopServer();
           //Debug.Log($"FishNetClientConnector.OnStopServer()");

            systemGameManager.SetGameMode(GameMode.Local);
            networkManagerServer.DeactivateServerMode();
        }
        */
    }
}
