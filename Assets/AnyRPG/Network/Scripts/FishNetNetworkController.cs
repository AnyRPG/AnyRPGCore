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

        private FishNet.Managing.NetworkManager networkManager;
        private FishNetNetworkConnector networkConnector;
        //private GameObject networkConnectorSpawnPrefab = null;
        //private GameObject networkConnectorSpawnReference = null;

        /// <summary>
        /// Current state of client socket.
        /// </summary>
        private LocalConnectionState clientState = LocalConnectionState.Stopped;

        // game manager references
        private LevelManager levelManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            networkManager = InstanceFinder.NetworkManager;
            if (networkManager != null) {
                //Debug.Log("FishNetNetworkController.Configure() Found FishNet NetworkManager");
                networkManager.ClientManager.OnClientConnectionState += HandleClientConnectionState;
                networkManager.ServerManager.OnClientKick += HandleClientKick;
                networkManager.ServerManager.OnRemoteConnectionState += HandleRemoteConnectionState;
                networkManager.SceneManager.OnClientLoadedStartScenes += HandleClientLoadedStartScenes;
                
                // stuff that was previously done only on active connection
                networkManager.SceneManager.OnActiveSceneSet += HandleActiveSceneSet;
                networkManager.SceneManager.OnLoadStart += HandleLoadStart;
                networkManager.SceneManager.OnLoadPercentChange += HandleLoadPercentChange;
                networkManager.SceneManager.OnLoadEnd += HandleLoadEnd;
                networkManager.SceneManager.OnUnloadStart += HandleUnloadStart;
                networkManager.SceneManager.OnUnloadEnd += HandleUnloadEnd;

            } else {
                Debug.Log("FishNetNetworkController.Configure() Could not find FishNet NetworkManager");
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            levelManager = systemGameManager.LevelManager;
        }

        private void HandleUnloadEnd(SceneUnloadEndEventArgs obj) {
            //Debug.Log($"FishNetNetworkController.HandleUnloadEnd()");
            //foreach (Scene scene in obj.UnloadedScenes) {
            //    Debug.Log($"FishNetNetworkController.HandleUnloadEnd() {scene.name}");
            //}
        }

        private void HandleUnloadStart(SceneUnloadStartEventArgs obj) {
            //Debug.Log($"FishNetNetworkController.HandleUnloadStart()");

            //foreach (SceneLookupData sceneLookupData in obj.QueueData.SceneUnloadData.SceneLookupDatas) {
            //    Debug.Log($"FishNetNetworkController.HandleUnloadStart() {sceneLookupData.Name}");
            //}
        }

        private void HandleRemoteConnectionState(NetworkConnection arg1, RemoteConnectionStateArgs arg2) {
            //Debug.Log($"FishNetNetworkController.HandleRemoteConnectionState({arg2.ConnectionState.ToString()})");
        }

        private void HandleClientKick(NetworkConnection arg1, int arg2, KickReason kickReason) {
            Debug.Log($"FishNetNetworkController.HandleClientKick({kickReason.ToString()})");
        }

        private void HandleClientLoadedStartScenes(NetworkConnection networkConnection, bool asServer) {
            //Debug.Log("FishNetNetworkController.HandleClientLoadedStartScenes()");
            //networkManager.SceneManager.AddConnectionToScene(networkConnection, UnityEngine.SceneManagement.SceneManager.GetSceneByName("DontDestroyOnLoad"));
            //networkManager.SceneManager.AddConnectionToScene(networkConnection, UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        public override bool Login(string username, string password, string server) {
            Debug.Log($"FishNetNetworkController.Login({username}, {password})");

            if (networkManager == null) {
                return false;
            }

            if (clientState != LocalConnectionState.Stopped) {
                Debug.Log("FishNetNetworkController.Login() Already connected to the server!");
                return false;
            }

            bool connectionResult = networkManager.ClientManager.StartConnection();
            //Debug.Log($"FishNetNetworkController.Login() Result of connection attempt: {connectionResult}");

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

        private void HandleClientConnectionState(ClientConnectionStateArgs obj) {
            //Debug.Log($"OnClientConnectionState() {obj.ConnectionState.ToString()}");
            clientState = obj.ConnectionState;
            if (clientState == LocalConnectionState.Started) {
                //Debug.Log("FishNetNetworkController.OnClientConnectionState() Connection Successful. Setting mode to network");
                systemGameManager.SetGameMode(GameMode.Network);
                //networkManager.SceneManager.OnActiveSceneSet += HandleActiveSceneSet;
                //networkManager.SceneManager.OnLoadStart += HandleLoadStart;
                //networkManager.SceneManager.OnLoadPercentChange += HandleLoadPercentChange;
                //networkManager.SceneManager.OnLoadEnd += HandleLoadEnd;
                //InstantiateNetworkConnector();
            } else if (clientState == LocalConnectionState.Stopping) {
                Debug.Log("FishNetNetworkController.OnClientConnectionState() Disconnected from server. Stopping");
                //systemGameManager.SetGameMode(GameMode.Local);
                /*
                if (levelManager.GetActiveSceneNode() != systemConfigurationManager.MainMenuSceneNode) {
                    levelManager.LoadMainMenu();
                }
                */
            } else if (clientState == LocalConnectionState.Stopped) {
                Debug.Log("FishNetNetworkController.OnClientConnectionState() Disconnected from server. Setting mode to local");
                systemGameManager.NetworkManager.ProcessStopConnection();
            }
        }

        private void HandleLoadPercentChange(SceneLoadPercentEventArgs obj) {
            //Debug.Log($"FishNetNetworkController.HandleLoadPercentChange() percent: {obj.Percent} AsServer: {obj.QueueData.AsServer}");
        }

        private void HandleLoadStart(SceneLoadStartEventArgs obj) {
            //Debug.Log($"FishNetNetworkController.HandleLoadStart() name: {obj.QueueData.SceneLoadData.SceneLookupDatas[0].Name} AsServer: {obj.QueueData.AsServer}");
        }

        private void HandleLoadEnd(SceneLoadEndEventArgs obj) {
            //Debug.Log($"FishNetNetworkController.HandleLoadEnd() AsServer: {obj.QueueData.AsServer}");
            //foreach (Scene scene in obj.LoadedScenes) {
            //    Debug.Log($"FishNetNetworkController.HandleLoadEnd() {scene.name}");
            //}
            //Debug.Log($"FishNetNetworkController.HandleLoadEnd() skipped: {string.Join(',', obj.SkippedSceneNames.ToList())}");

            // the level loading code should only be processed on the client
            if (obj.QueueData.AsServer == true) {
                return;
            }

            if (systemGameManager.GameMode == GameMode.Network) {
                levelManager.ProcessLevelLoad();
            }

        }

        private void HandleActiveSceneSet(bool userInitiated) {
            //Debug.Log($"FishNetNetworkController.HandleActiveSceneSet({userInitiated}) current scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");

            //if (systemGameManager.GameMode == GameMode.Network) {
            //    levelManager.ProcessLevelLoad();
            //}
        }


        public void RegisterConnector(FishNetNetworkConnector networkConnector) {
            this.networkConnector = networkConnector;
            if (networkConnector != null) {
                networkConnector.Configure(systemGameManager);
                networkConnector.SetNetworkManager(networkManager);
            }
        }

        /*
        private void InstantiateNetworkConnector() {
            Debug.Log("FishNetNetworkController.InstantiateNetworkConnector()");

            networkConnectorSpawnReference = GameObject.Instantiate(networkConnectorSpawnPrefab);
            //SpawnPrefab(networkConnectorSpawnPrefab, null, Vector3.zero, Vector3.zero);
            networkConnector = networkConnectorSpawnReference.gameObject.GetComponentInChildren<FishNetNetworkConnector>();
            if (networkConnector != null) {
                networkConnector.Configure(systemGameManager);
                networkConnector.SetNetworkManager(networkManager);
            }
        }
        */

        public override void SpawnPlayer(CharacterRequestData characterRequestData, GameObject playerPrefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            //Debug.Log($"FishNetNetworkController.SpawnPlayer({unitProfile.ResourceName}, {playerPrefab.name})");

            networkConnector.SpawnCharacterUnit(characterRequestData.spawnRequestId, characterRequestData.characterConfigurationRequest.unitProfile.ResourceName, playerPrefab, parentTransform, position, forward, characterRequestData.characterConfigurationRequest.unitControllerMode, characterRequestData.unitLevel);
            //return null;
        }

        public override GameObject SpawnModelPrefab(int spawnRequestId, GameObject prefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            Debug.Log($"FishNetNetworkController.SpawnModelPrefab({spawnRequestId})");

            networkConnector.SpawnModelPrefab(spawnRequestId, prefab, parentTransform, position, forward);
            return null;
        }

        public override void LoadScene(string sceneName) {
            //Debug.Log($"FishNetNetworkController.LoadScene({sceneName})");

            networkConnector.LoadSceneServer(networkManager.ClientManager.Connection, sceneName);
        }

        public override bool CanSpawnCharacterOverNetwork() {
            //Debug.Log($"FishNetNetworkController.CanSpawnCharacterOverNetwork() isClient: {networkManager.IsClient}");
            return networkManager.IsClient;
        }

        public override bool OwnPlayer(UnitController unitController) {
            NetworkBehaviour networkBehaviour = unitController.gameObject.GetComponent<NetworkBehaviour>();
            if (networkBehaviour != null && networkBehaviour.IsOwner == true) {
                return true;
            }
            return false;
        }

        //internal override void SetConnectionPrefab(GameObject spawnPrefab) {
        //    this.networkConnectorSpawnPrefab = spawnPrefab;
        //}
    }
}
