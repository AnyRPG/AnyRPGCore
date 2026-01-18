using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class FishNetServerBootstrap : ConfiguredNetworkBehaviour {

        [SerializeField]
        private GameObject networkConnectorPrefab = null;

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

        /*
        private void OnEnable() {
            //Debug.Log($"FishNetServerBootstrap.OnEnable()");
        }

        private void Awake() {
            //Debug.Log($"FishNetServerBootstrap.Awake()");
        }
        */

        public void SetNetworkManager(FishNet.Managing.NetworkManager networkManager) {
            this.networkManager = networkManager;
        }

        public NetworkObject SpawnPrefab() {
            //Debug.Log($"FishNetNetworkConnector.SpawnPrefab({networkConnectorPrefab.name})");

            NetworkObject networkPrefab = networkConnectorPrefab.GetComponent<NetworkObject>();
            if (networkPrefab == null) {
                Debug.LogWarning($"Could not find NetworkObject component on {networkConnectorPrefab.name}");
                return null;
            }

            NetworkObject nob = networkManager.GetPooledInstantiated(networkPrefab, true);
            networkManager.ServerManager.Spawn(nob);

            return nob;
        }

        
        public override void OnStartServer() {
            //Debug.Log($"{gameObject.name}.FishNetServerBootstrap.OnStartServer()");

            base.OnStartServer();

            //networkManager = GameObject.FindAnyObjectByType<FishNet.Managing.NetworkManager>();
            //SpawnPrefab();
        }
        

    }
}
