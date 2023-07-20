using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AnyRPG {
    public class NetworkManager : ConfiguredMonoBehaviour {

        // serialized
        //[SerializeField]
        //[Tooltip("If not empty, this object will be spawned")]
        //private GameObject spawnPrefab = null;

        [SerializeField]
        private NetworkController networkController = null;


        // game manager references
        private SystemDataFactory systemDataFactory = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            // hard coded to use FishNet for now - update in the future to accept from configuration?
            //networkController = new FishNetNetworkController();
            networkController.Configure(systemGameManager);
            //networkController.SetConnectionPrefab(spawnPrefab);

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
            //Debug.Log($"NetworkManager.LoadScene({sceneName})");

            networkController.LoadScene(sceneName);
        }

        public void SpawnPlayer(CharacterRequestData characterRequestData, GameObject playerPrefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            networkController.SpawnPlayer(characterRequestData, playerPrefab, parentTransform, position, forward);
        }

        public GameObject SpawnModelPrefab(int spawnRequestId, GameObject prefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            return networkController.SpawnModelPrefab(spawnRequestId, prefab, parentTransform, position, forward);
        }

        public bool CanSpawnPlayerOverNetwork() {
            return networkController.CanSpawnCharacterOverNetwork();
        }

        public bool OwnPlayer(UnitController unitController) {
            return networkController.OwnPlayer(unitController);
        }
    }

}