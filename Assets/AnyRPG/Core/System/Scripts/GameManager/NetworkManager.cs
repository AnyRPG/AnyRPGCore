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

        private string username = string.Empty;
        private string password = string.Empty;
        private string clientToken = string.Empty;

        [SerializeField]
        private NetworkController networkController = null;


        // game manager references
        private SystemDataFactory systemDataFactory = null;
        private PlayerManager playerManager = null;
        private CharacterManager characterManager = null;
        private UIManager uIManager = null;
        private LevelManager levelManager = null;

        public string Username { get => username; }
        public string Password { get => password; }

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
            playerManager = systemGameManager.PlayerManager;
            characterManager = systemGameManager.CharacterManager;
            levelManager = systemGameManager.LevelManager;
            uIManager = systemGameManager.UIManager;
        }

        public bool Login(string username, string password, string server) {
            Debug.Log($"NetworkManager.Login({username}, {password})");

            this.username = username;
            this.password = password;
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

        public void ProcessStopClient(UnitController unitController) {
            if (playerManager.UnitController == unitController) {
                playerManager.ProcessStopClient();
            } else {
                characterManager.ProcessStopClient(unitController);
            }
        }

        public void SetClientToken(string token) {
            Debug.Log($"NetworkManager.SetClientToken({token})");
            clientToken = token;
        }

        public void ProcessStopConnection() {
            systemGameManager.SetGameMode(GameMode.Local);
            if (levelManager.GetActiveSceneNode() != systemConfigurationManager.MainMenuSceneNode) {
                uIManager.AddPopupWindowToQueue(uIManager.disconnectedWindow);
                levelManager.LoadMainMenu();
            }
        }

        public void ProcessLoginFailure() {
            Debug.Log($"NetworkManager.ProcessLoginFailure()");
            uIManager.loginFailedWindow.OpenWindow();
        }

        public void ProcessLoginSuccess() {
            Debug.Log($"NetworkManager.ProcessLoginSuccess()");

        }
    }

}