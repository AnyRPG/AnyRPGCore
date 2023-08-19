using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AnyRPG {
    public class NetworkManagerClient : ConfiguredMonoBehaviour {

        private string username = string.Empty;
        private string password = string.Empty;
        
        private bool isLoggingInOrOut = false;

        [SerializeField]
        private NetworkController networkController = null;

        // game manager references
        private PlayerManager playerManager = null;
        private CharacterManager characterManager = null;
        private UIManager uIManager = null;
        private LevelManager levelManager = null;

        public string Username { get => username; }
        public string Password { get => password; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            networkController.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            characterManager = systemGameManager.CharacterManager;
            levelManager = systemGameManager.LevelManager;
            uIManager = systemGameManager.UIManager;
        }

        public bool Login(string username, string password, string server) {
            Debug.Log($"NetworkManagerClient.Login({username}, {password})");
            
            isLoggingInOrOut = true;

            this.username = username;
            this.password = password;
            return networkController.Login(username, password, server);
        }

        public void Logout() {
            isLoggingInOrOut = true;
            networkController.Logout();
        }

        public void LoadScene(string sceneName) {
            Debug.Log($"NetworkManagerClient.LoadScene({sceneName})");

            networkController.LoadScene(sceneName);
        }

        public void SpawnPlayer(CharacterRequestData characterRequestData, /*GameObject playerPrefab,*/ Transform parentTransform, Vector3 position, Vector3 forward) {
            Debug.Log($"NetworkManagerClient.SpawnPlayer()");
            if (characterRequestData.characterConfigurationRequest.unitProfile.UnitPrefabProps.NetworkUnitPrefab == null) {
                Debug.LogWarning($"NetworkManagerClient.SpawnPlayer({characterRequestData.characterConfigurationRequest.unitProfile.ResourceName}) On UnitProfile Network Unit Prefab is null ");
            }
            networkController.SpawnPlayer(characterRequestData, /*playerPrefab,*/ parentTransform, position, forward);
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

        public void ProcessStopConnection() {
            Debug.Log($"NetworkManagerClient.ProcessStopConnection()");
            systemGameManager.SetGameMode(GameMode.Local);
            if (levelManager.GetActiveSceneNode() != systemConfigurationManager.MainMenuSceneNode) {
                uIManager.AddPopupWindowToQueue(uIManager.disconnectedWindow);
                levelManager.LoadMainMenu();
                return;
            }

            // don't open disconnected window if this was an expected logout;
            if (isLoggingInOrOut == true) {
                isLoggingInOrOut = false;
                return;
            }
            
            // main menu, close main menu windows and open the disconnected window
            uIManager.newGameWindow.CloseWindow();
            uIManager.loadGameWindow.CloseWindow();
            uIManager.disconnectedWindow.OpenWindow();
        }

        public void ProcessLoginFailure() {
            Debug.Log($"NetworkManagerClient.ProcessLoginFailure()");

            uIManager.loginFailedWindow.OpenWindow();
        }

        public void ProcessLoginSuccess() {
            Debug.Log($"NetworkManagerClient.ProcessLoginSuccess()");

            // not doing this here because the connector has not spawned yet.
            //uIManager.ProcessLoginSuccess();

            isLoggingInOrOut = false;
        }

        public void CreatePlayerCharacter(AnyRPGSaveData anyRPGSaveData) {
            Debug.Log($"NetworkManagerClient.CreatePlayerCharacterClient(AnyRPGSaveData)");

            networkController.CreatePlayerCharacter(anyRPGSaveData);
        }

        public void LoadCharacterList() {
            Debug.Log($"NetworkManagerClient.LoadCharacterList()");

            networkController.LoadCharacterList();
        }

        public void DeletePlayerCharacter(int playerCharacterId) {
            Debug.Log($"NetworkManagerClient.DeletePlayerCharacter({playerCharacterId})");

            networkController.DeletePlayerCharacter(playerCharacterId);
        }

    }

}