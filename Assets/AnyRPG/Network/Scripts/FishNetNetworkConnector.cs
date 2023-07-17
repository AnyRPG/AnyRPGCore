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

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
        }

        public void SetNetworkManager(FishNet.Managing.NetworkManager networkManager) {
            this.networkManager = networkManager;
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnPlayer(NetworkConnection networkConnection, UnitProfile unitProfile, GameObject playerPrefab, Transform parentTransform, Vector3 position, Vector3 forward, UnitControllerMode unitControllerMode, int unitLevel) {
            Debug.Log($"FishNetNetworkConnector.SpawnPlayer({playerPrefab.name})");

            NetworkObject networkPrefab = playerPrefab.GetComponent<NetworkObject>();
            if (networkPrefab == null) {
                Debug.LogWarning($"Could not find NetworkObject component on {playerPrefab.name}");
                return;
            }

            NetworkObject nob = SpawnPrefab(networkConnection, playerPrefab, parentTransform, position, forward);

            systemGameManager.CharacterManager.ConfigureUnitController(GameMode.Network, unitProfile, playerPrefab, unitControllerMode, unitLevel);

            return;
        }

        public NetworkObject SpawnPrefab(NetworkConnection networkConnection, GameObject playerPrefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            NetworkObject networkPrefab = playerPrefab.GetComponent<NetworkObject>();
            if (networkPrefab == null) {
                Debug.LogWarning($"Could not find NetworkObject component on {playerPrefab.name}");
                return null;
            }

            NetworkObject nob = networkManager.GetPooledInstantiated(networkPrefab, networkPrefab.SpawnableCollectionId, true);
            //nob.transform.SetPositionAndRotation(position, rotation);
            nob.transform.parent = parentTransform;
            nob.transform.position = position;
            nob.transform.forward = forward;

            Debug.Log($"FishNetNetworkController.SpawnPlayer() Spawning player at {position}");
            networkManager.ServerManager.Spawn(nob, networkConnection);

            return nob;
        }
        
        /*
        [ServerRpc(RequireOwnership = false)]
        public void SpawnPlayer(NetworkObject networkObject, NetworkConnection networkConnection) {
            networkManager.ServerManager.Spawn(networkObject, networkConnection);
        }
        */

        [ServerRpc(RequireOwnership = false)]
        public void LoadScene(NetworkConnection networkConnection, string sceneName) {
            Debug.Log($"FishNetNetworkConnector.LoadScene({sceneName})");

            SceneLoadData sceneLoadData = new SceneLoadData(sceneName);
            sceneLoadData.ReplaceScenes = ReplaceOption.All;
            networkManager.SceneManager.LoadConnectionScenes(networkConnection, sceneLoadData);
        }

    }
}
