using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class FishNetNetworkConnector : ConfiguredNetworkBehaviour {

        private FishNet.Managing.NetworkManager networkManager;

        // game manager references
        private SystemDataFactory systemDataFactory = null;
        private CharacterManager characterManager = null;
        private NetworkManagerServer networkManagerServer = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            networkManagerServer.OnLoadCharacterList += HandleLoadCharacterList;
            networkManagerServer.OnDeletePlayerCharacter += HandleDeletePlayerCharacter;
            networkManagerServer.OnCreatePlayerCharacter += HandleCreatePlayerCharacter;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            systemDataFactory = systemGameManager.SystemDataFactory;
            characterManager = systemGameManager.CharacterManager;
            networkManagerServer = systemGameManager.NetworkManagerServer;
        }

        public void SetNetworkManager(FishNet.Managing.NetworkManager networkManager) {
            this.networkManager = networkManager;
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnPlayer(int clientSpawnRequestId, int playerCharacterId, Transform parentTransform, NetworkConnection networkConnection = null) {
            Debug.Log($"FishNetNetworkConnector.SpawnPlayer({clientSpawnRequestId}, {playerCharacterId})");


            PlayerCharacterSaveData playerCharacterSaveData = networkManagerServer.GetPlayerCharacterSaveData(networkConnection.ClientId, playerCharacterId);
            if (playerCharacterSaveData == null) {
                Debug.LogWarning($"FishNetNetworkConnector.SpawnPlayer({clientSpawnRequestId}, {playerCharacterId}) could not find playerCharacterId");
                return;
            }

            UnitControllerMode unitControllerMode = UnitControllerMode.Player;
            UnitProfile unitProfile = systemDataFactory.GetResource<UnitProfile>(playerCharacterSaveData.SaveData.unitProfileName);
            if (unitProfile == null) {
                return;
            }
            NetworkObject networkPrefab = unitProfile.UnitPrefabProps.NetworkUnitPrefab.GetComponent<NetworkObject>();
            if (networkPrefab == null) {
                Debug.LogWarning($"Could not find NetworkObject component on {unitProfile.UnitPrefabProps.NetworkUnitPrefab.name}");
                return;
            }
            int serverSpawnRequestId = characterManager.GetServerSpawnRequestId();
            /*
            CharacterConfigurationRequest characterConfigurationRequest = new CharacterConfigurationRequest(unitProfile);
            characterConfigurationRequest.unitLevel = playerCharacterSaveData.SaveData.PlayerLevel;
            characterConfigurationRequest.unitControllerMode = unitControllerMode;
            CharacterRequestData characterRequestData = new CharacterRequestData(null, GameMode.Network, characterConfigurationRequest);
            */
            //characterManager.AddUnitSpawnRequest(serverSpawnRequestId, characterRequestData);
            Vector3 position = new Vector3(
                playerCharacterSaveData.SaveData.PlayerLocationX,
                playerCharacterSaveData.SaveData.PlayerLocationY,
                playerCharacterSaveData.SaveData.PlayerLocationZ);
            Vector3 forward = new Vector3(
                playerCharacterSaveData.SaveData.PlayerRotationX,
                playerCharacterSaveData.SaveData.PlayerRotationY,
                playerCharacterSaveData.SaveData.PlayerRotationZ);
            NetworkObject nob = GetSpawnablePrefab(networkConnection, clientSpawnRequestId, serverSpawnRequestId, unitProfile.UnitPrefabProps.NetworkUnitPrefab, parentTransform, position, forward);
            // update syncvars
            NetworkCharacterUnit networkCharacterUnit = nob.gameObject.GetComponent<NetworkCharacterUnit>();
            if (networkCharacterUnit != null) {
                networkCharacterUnit.unitProfileName = playerCharacterSaveData.SaveData.unitProfileName;
                networkCharacterUnit.unitControllerMode = unitControllerMode;
                networkCharacterUnit.unitLevel = playerCharacterSaveData.SaveData.PlayerLevel;
                networkCharacterUnit.serverRequestId = serverSpawnRequestId;
            }

            SpawnPrefab(nob, networkConnection);
            if (nob == null) {
                return;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnCharacterUnit(int clientSpawnRequestId, string unitProfileName, Transform parentTransform, Vector3 position, Vector3 forward, UnitControllerMode unitControllerMode, int unitLevel, NetworkConnection networkConnection = null) {
            Debug.Log($"FishNetNetworkConnector.SpawnPlayer({clientSpawnRequestId}, {unitProfileName})");

            UnitProfile unitProfile = systemDataFactory.GetResource<UnitProfile>(unitProfileName);
            if (unitProfile == null) {
                return;
            }
            NetworkObject networkPrefab = unitProfile.UnitPrefabProps.NetworkUnitPrefab.GetComponent<NetworkObject>();
            if (networkPrefab == null) {
                Debug.LogWarning($"Could not find NetworkObject component on {unitProfile.UnitPrefabProps.NetworkUnitPrefab.name}");
                return;
            }
            int serverSpawnRequestId = characterManager.GetServerSpawnRequestId();
            CharacterConfigurationRequest characterConfigurationRequest = new CharacterConfigurationRequest(unitProfile);
            characterConfigurationRequest.unitLevel = unitLevel;
            characterConfigurationRequest.unitControllerMode = unitControllerMode;
            CharacterRequestData characterRequestData = new CharacterRequestData(null, GameMode.Network, characterConfigurationRequest);
            //characterManager.AddUnitSpawnRequest(serverSpawnRequestId, characterRequestData);
            NetworkObject nob = GetSpawnablePrefab(networkConnection, clientSpawnRequestId, serverSpawnRequestId, unitProfile.UnitPrefabProps.NetworkUnitPrefab, parentTransform, position, forward);
            // update syncvars
            NetworkCharacterUnit networkCharacterUnit = nob.gameObject.GetComponent<NetworkCharacterUnit>();
            if (networkCharacterUnit != null) {
                networkCharacterUnit.unitProfileName = unitProfileName;
                networkCharacterUnit.unitControllerMode = unitControllerMode;
                networkCharacterUnit.unitLevel = unitLevel;
                networkCharacterUnit.serverRequestId = serverSpawnRequestId;
            }

            SpawnPrefab(nob, networkConnection);
            if (nob == null) {
                return;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnModelPrefab(int clientSpawnRequestId, GameObject prefab, Transform parentTransform, Vector3 position, Vector3 forward, NetworkConnection networkConnection = null) {
            int serverSpawnRequestId = characterManager.GetServerSpawnRequestId();
            NetworkObject nob = GetSpawnablePrefab(networkConnection, clientSpawnRequestId, serverSpawnRequestId, prefab, parentTransform, position, forward);
            SpawnPrefab(nob, networkConnection);
        }


        private NetworkObject GetSpawnablePrefab(NetworkConnection networkConnection, int clientSpawnRequestId, int serverSpawnRequestId, GameObject prefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            Debug.Log($"FishNetNetworkConnector.SpawnPrefab({clientSpawnRequestId}, {prefab.name})");

            NetworkObject networkPrefab = prefab.GetComponent<NetworkObject>();
            if (networkPrefab == null) {
                Debug.LogWarning($"Could not find NetworkObject component on {prefab.name}");
                return null;
            }

            NetworkObject nob = networkManager.GetPooledInstantiated(networkPrefab, true);
            //nob.transform.SetPositionAndRotation(position, rotation);
            nob.transform.parent = parentTransform;
            nob.transform.position = position;
            nob.transform.forward = forward;

            SpawnedNetworkObject spawnedNetworkObject = nob.gameObject.GetComponent<SpawnedNetworkObject>();
            if (spawnedNetworkObject != null) {
                Debug.Log($"FishNetNetworkConnector.SpawnPrefab({clientSpawnRequestId}, {prefab.name}) setting spawnRequestId on gameobject");
                spawnedNetworkObject.clientSpawnRequestId = clientSpawnRequestId;
                spawnedNetworkObject.serverRequestId = serverSpawnRequestId;
            }

            return nob;
        }

        private void SpawnPrefab(NetworkObject nob, NetworkConnection networkConnection) {
            //Debug.Log($"FishNetNetworkController.SpawnPlayer() Spawning player at {position}");
            networkManager.ServerManager.Spawn(nob, networkConnection);
        }

        [ServerRpc(RequireOwnership = false)]
        public void LoadSceneServer(NetworkConnection networkConnection, string sceneName) {
            //Debug.Log($"FishNetNetworkConnector.LoadSceneServer({networkConnection.ClientId}, {sceneName})");

            SceneLoadData sceneLoadData = new SceneLoadData(sceneName);
            sceneLoadData.ReplaceScenes = ReplaceOption.All;
            sceneLoadData.PreferredActiveScene = sceneLoadData.SceneLookupDatas[0];

            networkManager.SceneManager.LoadConnectionScenes(networkConnection, sceneLoadData);
        }

        [ServerRpc(RequireOwnership = false)]
        public void CreatePlayerCharacter(AnyRPGSaveData anyRPGSaveData, NetworkConnection networkConnection = null) {
            Debug.Log($"FishNetNetworkConnector.CreatePlayerCharacter(AnyRPGSaveData)");

            systemGameManager.NetworkManagerServer.CreatePlayerCharacter(networkConnection.ClientId, anyRPGSaveData);
        }

        public void HandleCreatePlayerCharacter(int clientId) {
            Debug.Log($"FishNetNetworkConnector.HandleCreatePlayerCharacter({clientId})");

            if (networkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                Debug.Log($"FishNetNetworkConnector.HandleCreatePlayerCharacter() could not find client id {clientId}");
                return;
            }

            //LoadCharacterList(networkManager.ServerManager.Clients[clientId]);
            systemGameManager.NetworkManagerServer.LoadCharacterList(clientId);
        }


        [ServerRpc(RequireOwnership = false)]
        public void DeletePlayerCharacter(int playerCharacterId, NetworkConnection networkConnection = null) {
            Debug.Log($"FishNetNetworkConnector.DeletePlayerCharacter({playerCharacterId})");

            systemGameManager.NetworkManagerServer.DeletePlayerCharacter(networkConnection.ClientId, playerCharacterId);

            // now that character is deleted, just load the character list
            //LoadCharacterList(networkConnection);
        }

        public void HandleDeletePlayerCharacter(int clientId) {
            Debug.Log($"FishNetNetworkConnector.HandleDeletePlayerCharacter({clientId})");

            if (networkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                Debug.Log($"FishNetNetworkConnector.HandleDeletePlayerCharacter() could not find client id {clientId}");
                return;
            }

            //LoadCharacterList(networkManager.ServerManager.Clients[clientId]);
            systemGameManager.NetworkManagerServer.LoadCharacterList(clientId);
        }


        /*
        [TargetRpc]
        public void LoadCharacterList(NetworkConnection networkConnection, List<PlayerCharacterSaveData> playerCharacterSaveDataList) {
            Debug.Log($"FishNetNetworkConnector.SetCharacterList({playerCharacterSaveDataList.Count})");

            systemGameManager.LoadGameManager.SetCharacterList(playerCharacterSaveDataList);
        }
        */

        [ServerRpc(RequireOwnership = false)]
        public void LoadCharacterList(NetworkConnection networkConnection = null) {
            Debug.Log($"FishNetNetworkConnector.LoadCharacterList()");

            systemGameManager.NetworkManagerServer.LoadCharacterList(networkConnection.ClientId);
            //List<PlayerCharacterSaveData> playerCharacterSaveDataList = systemGameManager.NetworkManagerServer.LoadCharacterList(networkConnection.ClientId);

            //Debug.Log($"FishNetNetworkConnector.LoadCharacterList() list size: {playerCharacterSaveDataList.Count}");
            //SetCharacterList(networkConnection, playerCharacterSaveDataList);
        }

        public void HandleLoadCharacterList(int clientId, List<PlayerCharacterSaveData> playerCharacterSaveDataList) {
            Debug.Log($"FishNetNetworkConnector.LoadCharacterList()");

            if (networkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                foreach (int client in networkManager.ServerManager.Clients.Keys) {
                    Debug.Log($"FishNetNetworkConnector.LoadCharacterList() found client id {client}");
                }
                Debug.Log($"FishNetNetworkConnector.LoadCharacterList() could not find client id {clientId}");
                return;
            }

            SetCharacterList(networkManager.ServerManager.Clients[clientId], playerCharacterSaveDataList);
        }

        [TargetRpc]
        public void SetCharacterList(NetworkConnection networkConnection, List<PlayerCharacterSaveData> playerCharacterSaveDataList) {
            Debug.Log($"FishNetNetworkConnector.SetCharacterList({playerCharacterSaveDataList.Count})");

            systemGameManager.LoadGameManager.SetCharacterList(playerCharacterSaveDataList);
        }

        public override void OnStartClient() {
            base.OnStartClient();
            Debug.Log($"FishNetNetworkConnector.OnStartClient() ClientId: {networkManager.ClientManager.Connection.ClientId}");

            //systemGameManager.NetworkManager.ProcessLoginSuccess();
            systemGameManager.UIManager.ProcessLoginSuccess();
        }

        public override void OnStartNetwork() {
            base.OnStartNetwork();
            Debug.Log($"FishNetNetworkConnector.OnStartNetwork()");

            FishNetNetworkController fishNetNetworkController = GameObject.FindObjectOfType<FishNetNetworkController>();
            fishNetNetworkController.RegisterConnector(this);
        }

        public override void OnStartServer() {
            base.OnStartServer();
            //Debug.Log($"FishNetNetworkConnector.OnStartServer()");

            // on server gameMode should always bet set to network
            Debug.Log($"FishNetNetworkConnector.OnStartServer(): setting gameMode to network");
            systemGameManager.SetGameMode(GameMode.Network);
        }
    }
}
