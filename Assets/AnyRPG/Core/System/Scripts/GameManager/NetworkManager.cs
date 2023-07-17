using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AnyRPG {
    public class NetworkManager : ConfiguredMonoBehaviour {

        private NetworkController networkController = null;

        // game manager references
        private SystemDataFactory systemDataFactory = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            // hard coded to use FishNet for now - update in the future to accept from configuration?
            networkController = new FishNetNetworkController();
            networkController.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemDataFactory = systemGameManager.SystemDataFactory;
        }

        public bool Login(string username, string password, string server) {
            return networkController.Login(username, password, server);
        }

        public void Logout() {
            networkController.Logout();
        }

        public void LoadScene(string sceneName) {
            Debug.Log($"NetworkManager.LoadScene({sceneName})");

            networkController.LoadScene(sceneName);
        }

        public GameObject SpawnPlayer(UnitProfile unitProfile, GameObject playerPrefab, Transform parentTransform, Vector3 position, Vector3 forward, UnitControllerMode unitControllerMode, int level) {
            return networkController.SpawnPlayer(unitProfile, playerPrefab, parentTransform, position, forward, unitControllerMode, level);
        }

        public GameObject SpawnPrefab(GameObject prefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            return networkController.SpawnPrefab(prefab, parentTransform, position, forward);
        }

    }

}