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

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            systemDataFactory = systemGameManager.SystemDataFactory;
        }

        public void SetNetworkManager(FishNet.Managing.NetworkManager networkManager) {
            this.networkManager = networkManager;
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnPlayer(NetworkConnection networkConnection, int spawnRequestId, string unitProfileName, GameObject playerPrefab, Transform parentTransform, Vector3 position, Vector3 forward, UnitControllerMode unitControllerMode, int unitLevel) {
            Debug.Log($"FishNetNetworkConnector.SpawnPlayer({spawnRequestId}, {unitProfileName}, {playerPrefab.name})");

            NetworkObject networkPrefab = playerPrefab.GetComponent<NetworkObject>();
            if (networkPrefab == null) {
                Debug.LogWarning($"Could not find NetworkObject component on {playerPrefab.name}");
                return;
            }

            NetworkObject nob = SpawnPrefab(networkConnection, spawnRequestId, playerPrefab, parentTransform, position, forward);
            if (nob == null) {
                return;
            }

            // the client will configure the unit controller itself, so this should only be done if we are on only a server
            //if (base.IsClient == true) {
            if (networkConnection == base.LocalConnection) {
                return;
            }
            UnitProfile unitProfile = systemDataFactory.GetResource<UnitProfile>(unitProfileName);
            systemGameManager.CharacterManager.ConfigureUnitController(new CharacterRequestData(null, GameMode.Network, unitProfile, unitControllerMode, unitLevel), nob.gameObject);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnModelPrefab(NetworkConnection networkConnection, int spawnRequestId, GameObject prefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            NetworkObject nob = SpawnPrefab(networkConnection, spawnRequestId, prefab, parentTransform, position, forward);
        }


        private NetworkObject SpawnPrefab(NetworkConnection networkConnection, int spawnRequestId, GameObject prefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            Debug.Log($"FishNetNetworkConnector.SpawnPrefab({spawnRequestId}, {prefab.name})");

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
                Debug.Log($"FishNetNetworkConnector.SpawnPrefab({spawnRequestId}, {prefab.name}) setting spawnRequestId on gameobject");
                spawnedNetworkObject.spawnRequestId = spawnRequestId;
            }

            //Debug.Log($"FishNetNetworkController.SpawnPlayer() Spawning player at {position}");
            networkManager.ServerManager.Spawn(nob, networkConnection);

            return nob;
        }

        /*
        public void LoadScene(NetworkConnection networkConnection, string sceneName) {
            Debug.Log($"FishNetNetworkConnector.LoadScene({sceneName})");

            LoadSceneServer(networkConnection, sceneName);
        }
        */

        [ServerRpc(RequireOwnership = false)]
        public void LoadSceneServer(NetworkConnection networkConnection, string sceneName) {
            Debug.Log($"FishNetNetworkConnector.LoadSceneServer({networkConnection.ClientId}, {sceneName})");

            SceneLoadData sceneLoadData = new SceneLoadData(sceneName);
            sceneLoadData.ReplaceScenes = ReplaceOption.All;
            sceneLoadData.PreferredActiveScene = sceneLoadData.SceneLookupDatas[0];

            networkManager.SceneManager.LoadConnectionScenes(networkConnection, sceneLoadData);
        }

        public override void OnStartClient() {
            base.OnStartClient();
            //Debug.Log($"FishNetNetworkConnector.OnStartClient()");

            //FishNetNetworkController fishNetNetworkController = GameObject.FindObjectOfType<FishNetNetworkController>();
            //fishNetNetworkController.RegisterConnector(this);
            //Debug.Log($"FishNetNetworkConnector.OnStartClient() ClientId: {networkManager.ClientManager.Connection.ClientId}");
        }

        public override void OnStartNetwork() {
            base.OnStartNetwork();
            //Debug.Log($"FishNetNetworkConnector.OnStartNetwork()");

            FishNetNetworkController fishNetNetworkController = GameObject.FindObjectOfType<FishNetNetworkController>();
            fishNetNetworkController.RegisterConnector(this);
        }

    }
}
