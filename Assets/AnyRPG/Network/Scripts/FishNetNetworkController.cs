using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class FishNetNetworkController : NetworkController {

        private FishNet.Managing.NetworkManager networkManager;
        private FishNetNetworkConnector networkConnector;

        /// <summary>
        /// Current state of client socket.
        /// </summary>
        private LocalConnectionState clientState = LocalConnectionState.Stopped;


        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            networkManager = GameObject.FindObjectOfType<FishNet.Managing.NetworkManager>();
            if (networkManager != null) {
                Debug.Log("FishNetNetworkController.Configure() Found FishNet NetworkManager");
                networkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;
                networkConnector = networkManager.gameObject.GetComponentInChildren<FishNetNetworkConnector>();
                if (networkConnector != null) {
                    networkConnector.Configure(systemGameManager);
                    networkConnector.SetNetworkManager(networkManager);
                }
            } else {
                Debug.Log("FishNetNetworkController.Configure() Could not find FishNet NetworkManager");
            }
        }

        public override bool Login(string username, string password, string server) {
            if (networkManager == null) {
                return false;
            }

            if (clientState != LocalConnectionState.Stopped) {
                Debug.Log("FishNetNetworkController.Login() Already connected to the server!");
                return false;
            }

            bool connectionResult = networkManager.ClientManager.StartConnection();
            Debug.Log($"FishNetNetworkController.Login() Result of connection attempt: {connectionResult}");

            return connectionResult;
        }

        public override void Logout() {
            if (clientState == LocalConnectionState.Stopped) {
                Debug.Log("FishNetNetworkController.Login() Already disconnected from the server!");
                return;
            }
            
            bool connectionResult = networkManager.ClientManager.StopConnection();
            Debug.Log($"FishNetNetworkController.Login() Result of disconnection attempt: {connectionResult}");
        }

        private void OnClientConnectionState(ClientConnectionStateArgs obj) {
            Debug.Log("OnClientConnectionState()");
            clientState = obj.ConnectionState;
            if (clientState == LocalConnectionState.Started) {
                Debug.Log("Connection Successful. Setting mode to network");
                systemGameManager.SetGameMode(GameMode.Network);
            } else if (clientState == LocalConnectionState.Stopped) {
                Debug.Log("Disconnected from server. Setting mode to local");
                systemGameManager.SetGameMode(GameMode.Local);
            }
        }

        public override GameObject SpawnPlayer(UnitProfile unitProfile, GameObject playerPrefab, Transform parentTransform, Vector3 position, Vector3 forward, UnitControllerMode unitControllerMode, int unitLevel) {
            Debug.Log("FishNetNetworkController.SpawnPlayer()");

            networkConnector.SpawnPlayer(networkManager.ClientManager.Connection, unitProfile, playerPrefab, parentTransform, position, forward, unitControllerMode, unitLevel);
            return null;
        }

        public override GameObject SpawnPrefab(GameObject prefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            Debug.Log("FishNetNetworkController.SpawnPlayer()");

            networkConnector.SpawnPrefab(networkManager.ClientManager.Connection, prefab, parentTransform, position, forward);
            return null;
        }

        public override void LoadScene(string sceneName) {
            Debug.Log($"FishNetNetworkController.LoadScene({sceneName})");

            networkConnector.LoadScene(networkManager.ClientManager.Connection, sceneName);
        }

    }
}
