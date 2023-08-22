using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AnyRPG {
    public class NetworkManagerServer : ConfiguredMonoBehaviour {

        public event Action<int, bool, bool> OnAuthenticationResult = delegate { };
        public event Action<int, List<PlayerCharacterSaveData>> OnLoadCharacterList = delegate { };
        public event Action<int> OnDeletePlayerCharacter = delegate { };
        public event Action<int> OnCreatePlayerCharacter = delegate { };

        // jwt for each client so the server can make API calls to the api server on their behalf
        private Dictionary<int, string> clientTokens = new Dictionary<int, string>();
        
        // cached list of player character save data from client lookups used for loading games
        private Dictionary<int, Dictionary<int, PlayerCharacterSaveData>> playerCharacterDataDict = new Dictionary<int, Dictionary<int, PlayerCharacterSaveData>>();

        private Dictionary<int, PlayerCharacterMonitor> activePlayerCharacters = new Dictionary<int, PlayerCharacterMonitor>();

        private GameServerClient gameServerClient = null;
        private Coroutine monitorPlayerCharactersCoroutine = null;

        // game manager references
        private SaveManager saveManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            saveManager = systemGameManager.SaveManager;
        }

        public void SetClientToken(int clientId, string token) {
            Debug.Log($"NetworkManagerServer.SetClientToken({clientId}, {token})");

            clientTokens.Add(clientId, token);
        }

        public void OnSetGameMode(GameMode gameMode) {
            Debug.Log($"NetworkManagerServer.OnSetGameMode({gameMode})");
            
            if (gameMode == GameMode.Network) {
                // create instance of GameServerClient
                gameServerClient = new GameServerClient(systemGameManager, systemConfigurationManager.ApiServerAddress);
                if (monitorPlayerCharactersCoroutine == null) {
                    monitorPlayerCharactersCoroutine = StartCoroutine(MonitorPlayerCharacters());
                }
                return;
            }

            // local mode
            if (monitorPlayerCharactersCoroutine != null) {
                StopCoroutine(monitorPlayerCharactersCoroutine);
            }
        }

        public IEnumerator MonitorPlayerCharacters() {
            while (systemGameManager.GameMode == GameMode.Network) {
                foreach (PlayerCharacterMonitor playerCharacterMonitor in activePlayerCharacters.Values) {
                    SavePlayerCharacter(playerCharacterMonitor);
                }
                yield return new WaitForSeconds(10);
            }
        }

        private void SavePlayerCharacter(PlayerCharacterMonitor playerCharacterMonitor) {
            playerCharacterMonitor.SavePlayerLocation();
            if (playerCharacterMonitor.saveDataDirty == true) {
                if (clientTokens.ContainsKey(playerCharacterMonitor.clientId) == false) {
                    // can't do anything without a token
                    return;
                }
                gameServerClient.SavePlayerCharacter(
                    playerCharacterMonitor.clientId,
                    clientTokens[playerCharacterMonitor.clientId],
                    playerCharacterMonitor.playerCharacterSaveData.PlayerCharacterId,
                    playerCharacterMonitor.playerCharacterSaveData.SaveData);
            }
        }

        public void GetLoginToken(int clientId, string username, string password) {
            Debug.Log($"NetworkManagerServer.GetLoginToken({username}, {password})");
            //(bool correctPassword, string token) = gameServerClient.Login(clientId, username, password);
            gameServerClient.Login(clientId, username, password);
            //if (correctPassword == true) {
            //    SetClientToken(clientId, token);
            //}
            //OnAuthenticationResult(clientId, correctPassword);
            //return correctPassword;
        }

        public void ProcessLoginResponse(int clientId, bool correctPassword, string token) {
            Debug.Log($"NetworkManagerServer.ProcessLoginResponse({clientId}, {correctPassword}, {token})");

            if (correctPassword == true) {
                SetClientToken(clientId, token);
            }
            OnAuthenticationResult(clientId, true, correctPassword);
        }

        public void CreatePlayerCharacter(int clientId, AnyRPGSaveData anyRPGSaveData) {
            Debug.Log($"NetworkManagerServer.CreatePlayerCharacter(AnyRPGSaveData)");
            if (clientTokens.ContainsKey(clientId) == false) {
                // can't do anything without a token
                return;
            }

            gameServerClient.CreatePlayerCharacter(clientId, clientTokens[clientId], anyRPGSaveData);
        }

        public void ProcessCreatePlayerCharacterResponse(int clientId) {
            Debug.Log($"NetworkManagerServer.ProcessCreatePlayerCharacterResponse({clientId})");

            OnCreatePlayerCharacter(clientId);
        }


        public void DeletePlayerCharacter(int clientId, int playerCharacterId) {
            Debug.Log($"NetworkManagerServer.DeletePlayerCharacter({playerCharacterId})");

            if (clientTokens.ContainsKey(clientId) == false) {
                // can't do anything without a token
                return;
            }

            gameServerClient.DeletePlayerCharacter(clientId, clientTokens[clientId], playerCharacterId);
        }

        public void ProcessDeletePlayerCharacterResponse(int clientId) {
            Debug.Log($"NetworkManagerServer.ProcessDeletePlayerCharacterResponse({clientId})");

            OnDeletePlayerCharacter(clientId);
        }

        public void LoadCharacterList(int clientId) {
            Debug.Log($"NetworkManagerServer.LoadCharacterList({clientId})");
            if (clientTokens.ContainsKey(clientId) == false) {
                // can't do anything without a token
                //return new List<PlayerCharacterSaveData>();
                return;
            }
            gameServerClient.LoadCharacterList(clientId, clientTokens[clientId]);
            //List<PlayerCharacterData> playerCharacterDataList = gameServerClient.LoadCharacterList(clientId, clientTokens[clientId]);

            //Debug.Log($"NetworkManagerServer.LoadCharacterListServer({clientId}) list size: {playerCharacterDataList.Count}");

            //List<PlayerCharacterSaveData> playerCharacterSaveDataList = new List<PlayerCharacterSaveData>();
            //foreach (PlayerCharacterData playerCharacterData in playerCharacterDataList) {
            //    playerCharacterSaveDataList.Add(new PlayerCharacterSaveData() {
            //        PlayerCharacterId = playerCharacterData.id,
            //        SaveData = saveManager.LoadSaveDataFromString(playerCharacterData.saveData)
            //    });
            //}
            //if (playerCharacterDataDict.ContainsKey(clientId)) {
            //    playerCharacterDataDict[clientId] = playerCharacterSaveDataList;
            //} else {
            //    playerCharacterDataDict.Add(clientId, playerCharacterSaveDataList);
            //}

            //return playerCharacterSaveDataList;
        }

        public void MonitorPlayerUnit(int clientId,  PlayerCharacterSaveData playerCharacterSaveData, UnitController unitController) {
            activePlayerCharacters.Add(playerCharacterSaveData.PlayerCharacterId, new PlayerCharacterMonitor(
                systemGameManager,
                clientId,
                playerCharacterSaveData,
                unitController
            ));
        }

        public void StopMonitoringPlayerUnit(int playerCharacterId) {
            if (activePlayerCharacters.ContainsKey(playerCharacterId)) {
                activePlayerCharacters[playerCharacterId].StopMonitoring();
                // flush data to database before stop monitoring
                SavePlayerCharacter(activePlayerCharacters[playerCharacterId]);
                activePlayerCharacters.Remove(playerCharacterId);
            }
        }

        public void ProcessLoadCharacterListResponse(int clientId, List<PlayerCharacterData> playerCharacters) {
            Debug.Log($"NetworkManagerServer.ProcessLoadCharacterListResponse({clientId})");

            List<PlayerCharacterSaveData> playerCharacterSaveDataList = new List<PlayerCharacterSaveData>();
            foreach (PlayerCharacterData playerCharacterData in playerCharacters) {
                playerCharacterSaveDataList.Add(new PlayerCharacterSaveData() {
                    PlayerCharacterId = playerCharacterData.id,
                    SaveData = saveManager.LoadSaveDataFromString(playerCharacterData.saveData)
                });
            }
            Dictionary<int, PlayerCharacterSaveData> playerCharacterSaveDataDict = new Dictionary<int, PlayerCharacterSaveData>();
            foreach (PlayerCharacterSaveData playerCharacterSaveData in playerCharacterSaveDataList) {
                playerCharacterSaveDataDict.Add(playerCharacterSaveData.PlayerCharacterId, playerCharacterSaveData);
            }
            if (playerCharacterDataDict.ContainsKey(clientId)) {
                playerCharacterDataDict[clientId] = playerCharacterSaveDataDict;
            } else {
                playerCharacterDataDict.Add(clientId, playerCharacterSaveDataDict);
            }

            OnLoadCharacterList(clientId, playerCharacterSaveDataList);
        }

        public PlayerCharacterSaveData GetPlayerCharacterSaveData(int clientId, int playerCharacterId) {
            if (playerCharacterDataDict.ContainsKey(clientId) == false) {
                return null;
            }
            if (playerCharacterDataDict[clientId].ContainsKey(playerCharacterId) == false) {
                return null;
            }
            return playerCharacterDataDict[clientId][playerCharacterId];
        }

        public string GetClientToken(int clientId) {
            Debug.Log($"NetworkManagerServer.GetClientToken({clientId})");

            if (clientTokens.ContainsKey(clientId)) {
                return clientTokens[clientId];
            }
            return string.Empty;
        }

    }

}