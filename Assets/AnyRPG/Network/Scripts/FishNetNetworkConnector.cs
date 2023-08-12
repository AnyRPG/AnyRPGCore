using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class FishNetNetworkConnector : ConfiguredNetworkBehaviour {

        private FishNet.Managing.NetworkManager networkManager;

        // game manager references
        private SystemDataFactory systemDataFactory = null;
        private CharacterManager characterManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            systemDataFactory = systemGameManager.SystemDataFactory;
            characterManager = systemGameManager.CharacterManager;
        }

        public void SetNetworkManager(FishNet.Managing.NetworkManager networkManager) {
            this.networkManager = networkManager;
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnCharacterUnit(int clientSpawnRequestId, string unitProfileName, GameObject playerPrefab, Transform parentTransform, Vector3 position, Vector3 forward, UnitControllerMode unitControllerMode, int unitLevel, NetworkConnection networkConnection = null) {
            //Debug.Log($"FishNetNetworkConnector.SpawnPlayer({clientSpawnRequestId}, {unitProfileName}, {playerPrefab.name})");

            NetworkObject networkPrefab = playerPrefab.GetComponent<NetworkObject>();
            if (networkPrefab == null) {
                Debug.LogWarning($"Could not find NetworkObject component on {playerPrefab.name}");
                return;
            }
            int serverSpawnRequestId = characterManager.GetServerSpawnRequestId();
            UnitProfile unitProfile = systemDataFactory.GetResource<UnitProfile>(unitProfileName);
            CharacterConfigurationRequest characterConfigurationRequest = new CharacterConfigurationRequest(unitProfile);
            characterConfigurationRequest.unitLevel = unitLevel;
            characterConfigurationRequest.unitControllerMode = unitControllerMode;
            CharacterRequestData characterRequestData = new CharacterRequestData(null, GameMode.Network, characterConfigurationRequest);
            //characterManager.AddUnitSpawnRequest(serverSpawnRequestId, characterRequestData);
            NetworkObject nob = GetSpawnablePrefab(networkConnection, clientSpawnRequestId, serverSpawnRequestId, playerPrefab, parentTransform, position, forward);
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

            // the client will configure the unit controller itself, so this should only be done if we are on only a server
            //if (base.IsClient == true) {
            /*
            if (networkConnection == base.LocalConnection) {
                return;
            }
            systemGameManager.CharacterManager.ConfigureUnitController(new CharacterRequestData(null, GameMode.Network, unitProfile, unitControllerMode, unitLevel), nob.gameObject, false);
            */
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

        public override void OnStartClient() {
            base.OnStartClient();
            //Debug.Log($"FishNetNetworkConnector.OnStartClient()");

            //Debug.Log($"FishNetNetworkConnector.OnStartClient() ClientId: {networkManager.ClientManager.Connection.ClientId}");
        }

        public override void OnStartNetwork() {
            base.OnStartNetwork();
            //Debug.Log($"FishNetNetworkConnector.OnStartNetwork()");

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
