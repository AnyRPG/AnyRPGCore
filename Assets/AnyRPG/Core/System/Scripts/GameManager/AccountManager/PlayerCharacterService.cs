using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AnyRPG { 
    public class PlayerCharacterService : ConfiguredClass {

        private int playerCharacterIdCounter = 1;
        private string baseSaveFolderName = string.Empty;
        private const string playerIdCounterKey = "Server.PlayerCharacterIdCounter";

        /// <summary>
        /// playerName, playerCharacterId
        /// </summary>
        private Dictionary<string, int> playerNameMap = new Dictionary<string, int>();

        private Dictionary<int, string> playerNameLookupMap = new Dictionary<int, string>();

        // game manager references
        SystemEventManager systemEventManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            MakeBaseSaveFolder();
            LoadPlayerCharacterIdCounter();
            systemEventManager.OnStartServer += HandleStartServer;
            systemEventManager.OnStopServer += HandleStopServer;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemEventManager = systemGameManager.SystemEventManager;
        }

        private void HandleStopServer() {
            ClearPlayerNameMap();
        }

        private void HandleStartServer() {
            LoadPlayerNameMap();
        }

        public string GetPlayerNameFromId(int playerCharacterId) {
            if (playerNameLookupMap.ContainsKey(playerCharacterId)) {
                return playerNameLookupMap[playerCharacterId];
            }
            return "Unknown";
        }

        private void LoadPlayerNameMap() {
            Debug.Log("PlayerCharacterService.LoadPlayerNameMap()");

            if (Directory.Exists(baseSaveFolderName)) {
                string[] accountDirectories = Directory.GetDirectories(baseSaveFolderName);
                foreach (string accountDirectory in accountDirectories) {
                    string[] fileEntries = Directory.GetFiles(accountDirectory);
                    foreach (string fileName in fileEntries) {
                        if (fileName.EndsWith(".json")) {
                            string jsonString = File.ReadAllText(fileName);
                            PlayerCharacterSaveData playerCharacterSaveData = JsonUtility.FromJson<PlayerCharacterSaveData>(jsonString);
                            if (!playerNameMap.ContainsKey(playerCharacterSaveData.SaveData.playerName) && !playerNameLookupMap.ContainsKey(playerCharacterSaveData.PlayerCharacterId)) {
                                Debug.Log($"PlayerCharacterService.LoadPlayerNameMap(): Loaded player ({playerCharacterSaveData.SaveData.playerName}) with ID ({playerCharacterSaveData.PlayerCharacterId})");
                                playerNameMap.Add(playerCharacterSaveData.SaveData.playerName, playerCharacterSaveData.PlayerCharacterId);
                                playerNameLookupMap.Add(playerCharacterSaveData.PlayerCharacterId, playerCharacterSaveData.SaveData.playerName);
                            } else {
                                Debug.LogWarning($"PlayerCharacterService.LoadPlayerNameMap(): Duplicate player name ({playerCharacterSaveData.SaveData.playerName}) or character ID ({playerCharacterSaveData.PlayerCharacterId}) found . This character will be skipped.");
                            }
                        }
                    }
                }
            }
        }

        private void ClearPlayerNameMap() {
            playerNameMap.Clear();
            playerNameLookupMap.Clear();
        }

        private void LoadPlayerCharacterIdCounter() {
            //Debug.Log("PlayerCharacterService.LoadPlayerCharacterIdCounter()");

            playerCharacterIdCounter = PlayerPrefs.GetInt(playerIdCounterKey, 1);
            Debug.Log($"PlayerCharacterService.LoadPlayerCharacterIdCounter(): {playerCharacterIdCounter}");
        }


        private void MakeBaseSaveFolder() {
            //Debug.Log("PlayerCharacterService.MakeSaveFolder()");

            Regex regex = new Regex("[^a-zA-Z0-9]");
            string gameNameString = regex.Replace(systemConfigurationManager.GameName, "");
            if (gameNameString == string.Empty) {
                return;
            }
            baseSaveFolderName = $"{Application.persistentDataPath}/{gameNameString}/Online/PlayerCharacters";
            if (!Directory.Exists($"{Application.persistentDataPath}/{gameNameString}")) {
                Directory.CreateDirectory($"{Application.persistentDataPath}/{gameNameString}");
            }
            if (!Directory.Exists($"{Application.persistentDataPath}/{gameNameString}/Online")) {
                Directory.CreateDirectory($"{Application.persistentDataPath}/{gameNameString}/Online");
            }
            if (!Directory.Exists(baseSaveFolderName)) {
                Directory.CreateDirectory(baseSaveFolderName);
            }
        }

        private void MakeAccountSaveFolder(int accountId) {
            //Debug.Log("PlayerCharacterService.MakeSaveFolder()");

            string saveFolderName = GetAccountSaveFolder(accountId);
            if (!Directory.Exists(saveFolderName)) {
                Directory.CreateDirectory(saveFolderName);
            }
        }

        private string GetAccountSaveFolder(int accountId) {
            
            return $"{baseSaveFolderName}/{accountId}";
        }

        public bool AddPlayerCharacter(int accountId, AnyRPGSaveData anyRPGSaveData) {
            
            MakeAccountSaveFolder(accountId);

            if (playerNameMap.ContainsKey(anyRPGSaveData.playerName)) {
                return false;
            }

            PlayerCharacterSaveData playerCharacterSaveData = new PlayerCharacterSaveData() {
                PlayerCharacterId = GetNewPlayerCharacterId(),
                SaveData = anyRPGSaveData
            };
            SaveDataFile(accountId, playerCharacterSaveData);
            playerNameMap.Add(anyRPGSaveData.playerName, playerCharacterSaveData.PlayerCharacterId);
            playerNameLookupMap.Add(playerCharacterSaveData.PlayerCharacterId, anyRPGSaveData.playerName);

            return true;
        }

        public bool SaveDataFile(int accountId, PlayerCharacterSaveData playerCharacterSaveData) {
            //Debug.Log($"UserAccountService.SaveDataFile({userAccount.UserName})");

            string jsonString = JsonUtility.ToJson(playerCharacterSaveData);
            string jsonSavePath = $"{GetAccountSaveFolder(accountId)}/{playerCharacterSaveData.PlayerCharacterId}.json";
            File.WriteAllText(jsonSavePath, jsonString);

            return true;
        }

        private int GetNewPlayerCharacterId() {
            Debug.Log("PlayerCharacterService.GetNewPlayerCharacterId()");

            int returnValue = playerCharacterIdCounter;
            // loop until accountIdCounter is found that is not in use, using the in memory dictionary for speed
            while (true) {
                bool idInUse = false;
                foreach (int entry in playerNameLookupMap.Keys) {
                    if (entry == playerCharacterIdCounter) {
                        Debug.LogWarning($"PlayerCharacterService.GetNewPlayerCharacterId(): id {playerCharacterIdCounter} is already in use, trying next");
                        idInUse = true;
                        break;
                    }
                }
                if (!idInUse) {
                    break;
                }
                playerCharacterIdCounter++;
            }
            returnValue = playerCharacterIdCounter;
            playerCharacterIdCounter++;
            PlayerPrefs.SetInt(playerIdCounterKey, playerCharacterIdCounter);

            Debug.Log($"PlayerCharacterService.GetNewPlayerCharacterId() return {returnValue}");
            return returnValue;
        }


        public bool SavePlayerCharacter(int accountId, PlayerCharacterSaveData playerCharacterSaveData) {

            return SaveDataFile(accountId, playerCharacterSaveData);
        }

        public bool SavePlayerCharacter(int accountId, int playerCharacterId, AnyRPGSaveData anyRPGSaveData) {
            
            PlayerCharacterSaveData playerCharacterSaveData = new PlayerCharacterSaveData() {
                PlayerCharacterId = playerCharacterId,
                SaveData = anyRPGSaveData
            };
            return SaveDataFile(accountId, playerCharacterSaveData);
        }

        public bool RenamePlayerCharacter(int characterId, string newName) {
            if (playerNameMap.ContainsKey(newName)) {
                return false;
            }
            if (playerNameLookupMap.ContainsKey(characterId) == false) {
                return false;
            }
            string oldName = playerNameLookupMap[characterId];
            playerNameMap.Remove(oldName);
            playerNameMap.Add(newName, characterId);
            playerNameLookupMap[characterId] = newName;
            return true;
        }

        public bool DeletePlayerCharacter(int accountId, int playerCharacterId) {
            if (playerNameLookupMap.ContainsKey(playerCharacterId) == false) {
                return false;
            }
            string playerName = playerNameLookupMap[playerCharacterId];
            playerNameLookupMap.Remove(playerCharacterId);
            playerNameMap.Remove(playerName);
            string jsonSavePath = $"{GetAccountSaveFolder(accountId)}/{playerCharacterId}.json";
            if (File.Exists(jsonSavePath)) {
                File.Delete(jsonSavePath);
            }

            return true;
        }

        public PlayerCharacterListResponse GetPlayerCharacters(int accountId) {
            PlayerCharacterListResponse playerCharacterListResponse = new PlayerCharacterListResponse();
            string accountSaveFolder = GetAccountSaveFolder(accountId);
            if (Directory.Exists(accountSaveFolder)) {
                string[] fileEntries = Directory.GetFiles(accountSaveFolder);
                foreach (string fileName in fileEntries) {
                    if (fileName.EndsWith(".json")) {
                        string jsonString = File.ReadAllText(fileName);
                        PlayerCharacterSaveData playerCharacterSaveData = JsonUtility.FromJson<PlayerCharacterSaveData>(jsonString);
                        if (playerCharacterSaveData == null) {
                            Debug.LogWarning($"PlayerCharacterService.GetPlayerCharacters({accountId}): Could not load player character save data from file {fileName}. This character will be skipped.");
                            continue;
                        }
                        if (playerNameMap.ContainsKey(playerCharacterSaveData.SaveData.playerName) == false) {
                            Debug.LogWarning($"PlayerCharacterService.GetPlayerCharacters({accountId}): Player name {playerCharacterSaveData.SaveData.playerName} not found in player name map. This character will be skipped.");
                            continue;
                        }
                        PlayerCharacterData playerCharacterData = new PlayerCharacterData() {
                            id = playerCharacterSaveData.PlayerCharacterId,
                            accountId = accountId,
                            name = playerCharacterSaveData.SaveData.playerName,
                            saveData = JsonUtility.ToJson(playerCharacterSaveData.SaveData)
                        };
                        playerCharacterListResponse.playerCharacters.Add(playerCharacterData);
                    }
                }
            }

            return playerCharacterListResponse;
        }

        public PlayerCharacterSaveData GetPlayerCharacterSaveData(int accountId, int playerCharacterId) {
            string jsonSavePath = $"{GetAccountSaveFolder(accountId)}/{playerCharacterId}.json";
            if (File.Exists(jsonSavePath)) {
                string jsonString = File.ReadAllText(jsonSavePath);
                PlayerCharacterSaveData playerCharacterSaveData = JsonUtility.FromJson<PlayerCharacterSaveData>(jsonString);
                return playerCharacterSaveData;
            }
            return null;
        }

    }

}