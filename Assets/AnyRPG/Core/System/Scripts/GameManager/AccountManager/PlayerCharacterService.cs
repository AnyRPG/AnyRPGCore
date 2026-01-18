using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AnyRPG { 
    public class PlayerCharacterService : ConfiguredClass {

        private int playerCharacterIdCounter = 1;
        private string baseSaveFolderName = string.Empty;

        /// <summary>
        /// playerName, playerCharacterId
        /// </summary>
        private Dictionary<string, int> playerNameMap = new Dictionary<string, int>();

        /// <summary>
        /// playerCharacterId, playerName
        /// </summary>
        private Dictionary<int, string> playerNameLookupMap = new Dictionary<int, string>();

        /// <summary>
        /// playerCharacterId, CharacterSummaryData
        /// </summary>
        private Dictionary<int, CharacterSummaryData> playerCharacterSummaryData = new Dictionary<int, CharacterSummaryData>();

        // game manager references
        private CharacterGroupServiceServer characterGroupServiceServer = null;
        private FriendServiceServer friendServiceServer = null;
        private GuildServiceServer guildServiceServer = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            MakeBaseSaveFolder();
            //networkManagerServer.OnStartServer += HandleStartServer;
            networkManagerServer.OnStopServer += HandleStopServer;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            characterGroupServiceServer = systemGameManager.CharacterGroupServiceServer;
            friendServiceServer = systemGameManager.FriendServiceServer;
            guildServiceServer = systemGameManager.GuildServiceServer;
        }

        private void HandleStopServer() {
            ClearPlayerNameMap();
        }

        public void ProcessStartServer() {
            LoadPlayerNameMap();
        }

        public string GetPlayerNameFromId(int playerCharacterId) {
            if (playerNameLookupMap.ContainsKey(playerCharacterId)) {
                return playerNameLookupMap[playerCharacterId];
            }
            return "Unknown";
        }

        private void LoadPlayerNameMap() {
            //Debug.Log("PlayerCharacterService.LoadPlayerNameMap()");

            if (Directory.Exists(baseSaveFolderName)) {
                string[] accountDirectories = Directory.GetDirectories(baseSaveFolderName);
                foreach (string accountDirectory in accountDirectories) {
                    string[] fileEntries = Directory.GetFiles(accountDirectory);
                    foreach (string fileName in fileEntries) {
                        if (fileName.EndsWith(".json")) {
                            string jsonString = File.ReadAllText(fileName);
                            CharacterSaveData characterSaveData = JsonUtility.FromJson<CharacterSaveData>(jsonString);
                            if (!playerNameMap.ContainsKey(characterSaveData.CharacterName.ToLower()) && !playerNameLookupMap.ContainsKey(characterSaveData.CharacterId)) {
                                //Debug.Log($"PlayerCharacterService.LoadPlayerNameMap(): Loaded player ({characterSaveData.CharacterName}) with ID ({characterSaveData.CharacterId})");
                                playerNameMap.Add(characterSaveData.CharacterName.ToLower(), characterSaveData.CharacterId);
                                playerNameLookupMap.Add(characterSaveData.CharacterId, characterSaveData.CharacterName);
                                playerCharacterSummaryData.Add(characterSaveData.CharacterId, new CharacterSummaryData(characterSaveData, systemDataFactory));
                            } else {
                                Debug.LogWarning($"PlayerCharacterService.LoadPlayerNameMap(): Duplicate player name ({characterSaveData.CharacterName}) or character ID ({characterSaveData.CharacterId}) found . This character will be skipped.");
                            }
                        }
                    }
                }
            }
        }

        private void ClearPlayerNameMap() {
            playerNameMap.Clear();
            playerNameLookupMap.Clear();
            playerCharacterSummaryData.Clear();
        }

        public void LoadPlayerCharacterIdCounter(int newCounterValue) {
            //Debug.Log($"PlayerCharacterService.LoadPlayerCharacterIdCounter({newCounterValue})");

            playerCharacterIdCounter = newCounterValue;
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

        public bool AddPlayerCharacter(int accountId, CharacterSaveData characterSaveData) {
            
            MakeAccountSaveFolder(accountId);

            if (playerNameMap.ContainsKey(characterSaveData.CharacterName.ToLower())) {
                return false;
            }

            characterSaveData.CharacterId = GetNewPlayerCharacterId();
            SaveDataFile(accountId, characterSaveData);
            playerNameMap.Add(characterSaveData.CharacterName.ToLower(), characterSaveData.CharacterId);
            playerNameLookupMap.Add(characterSaveData.CharacterId, characterSaveData.CharacterName);

            return true;
        }

        public bool SaveDataFile(int accountId, CharacterSaveData characterSaveData) {
            //Debug.Log($"UserAccountService.SaveDataFile({userAccount.UserName})");

            string jsonString = JsonUtility.ToJson(characterSaveData);
            string jsonSavePath = $"{GetAccountSaveFolder(accountId)}/{characterSaveData.CharacterId}.json";
            File.WriteAllText(jsonSavePath, jsonString);

            return true;
        }

        private int GetNewPlayerCharacterId() {
            //Debug.Log("PlayerCharacterService.GetNewPlayerCharacterId()");

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
            serverStateService.SetPlayerCharacterIdCounter(playerCharacterIdCounter);

            //Debug.Log($"PlayerCharacterService.GetNewPlayerCharacterId() return {returnValue}");
            return returnValue;
        }


        public bool SavePlayerCharacter(int accountId, CharacterSaveData characterSaveData) {
            //Debug.Log($"PlayerCharacterService.SavePlayerCharacter({accountId})");

            return SaveDataFile(accountId, characterSaveData);
        }

        public bool SavePlayerCharacter(int accountId, int playerCharacterId, CharacterSaveData characterSaveData) {

            characterSaveData.CharacterId = playerCharacterId;

            return SaveDataFile(accountId, characterSaveData);
        }

        public bool RenamePlayerCharacter(UnitController unitController, string newName) {
            int characterId = unitController.CharacterId;
            if (playerNameMap.ContainsKey(newName.ToLower())) {
                return false;
            }
            if (playerNameLookupMap.ContainsKey(characterId) == false) {
                return false;
            }
            string oldName = playerNameLookupMap[characterId];
            playerNameMap.Remove(oldName.ToLower());
            playerNameMap.Add(newName.ToLower(), characterId);
            playerNameLookupMap[characterId] = newName;
            playerCharacterSummaryData[characterId].CharacterName = newName;

            characterGroupServiceServer.ProcessStatusChange(unitController.CharacterId);
            friendServiceServer.ProcessStatusChange(unitController.CharacterId);
            guildServiceServer.ProcessStatusChange(unitController.CharacterId);
            return true;
        }

        public bool DeletePlayerCharacter(int accountId, int playerCharacterId) {
            if (playerNameLookupMap.ContainsKey(playerCharacterId) == false) {
                return false;
            }
            string playerName = playerNameLookupMap[playerCharacterId];
            playerNameLookupMap.Remove(playerCharacterId);
            playerNameMap.Remove(playerName.ToLower());
            playerCharacterSummaryData.Remove(playerCharacterId);
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
                        CharacterSaveData characterSaveData = JsonUtility.FromJson<CharacterSaveData>(jsonString);
                        if (characterSaveData == null) {
                            Debug.LogWarning($"PlayerCharacterService.GetPlayerCharacters({accountId}): Could not load player character save data from file {fileName}. This character will be skipped.");
                            continue;
                        }
                        if (playerNameMap.ContainsKey(characterSaveData.CharacterName.ToLower()) == false) {
                            Debug.LogWarning($"PlayerCharacterService.GetPlayerCharacters({accountId}): Player name {characterSaveData.CharacterName} not found in player name map. This character will be skipped.");
                            continue;
                        }
                        PlayerCharacterData playerCharacterData = new PlayerCharacterData() {
                            id = characterSaveData.CharacterId,
                            accountId = accountId,
                            name = characterSaveData.CharacterName,
                            saveData = JsonUtility.ToJson(characterSaveData)
                        };
                        playerCharacterListResponse.playerCharacters.Add(playerCharacterData);
                    }
                }
            }

            return playerCharacterListResponse;
        }

        public CharacterSaveData GetPlayerCharacterSaveData(int accountId, int playerCharacterId) {
            string jsonSavePath = $"{GetAccountSaveFolder(accountId)}/{playerCharacterId}.json";
            if (File.Exists(jsonSavePath)) {
                string jsonString = File.ReadAllText(jsonSavePath);
                CharacterSaveData characterSaveData = JsonUtility.FromJson<CharacterSaveData>(jsonString);
                return characterSaveData;
            }
            return null;
        }

        public int GetPlayerIdFromName(string targetPlayerName) {
            //Debug.Log($"PlayerCharacterService.GetPlayerIdFromName({targetPlayerName})");

            if (playerNameMap.ContainsKey(targetPlayerName.ToLower()) == false) {
                return 0;
            }
            return playerNameMap[targetPlayerName.ToLower()];
        }

        public CharacterSummaryData GetSummaryData(int playerCharacterId) {
            if (playerCharacterSummaryData.ContainsKey(playerCharacterId)) {
                return playerCharacterSummaryData[playerCharacterId];
            }
            return null;
        }

        public void ProcessLevelChanged(UnitController unitController, int newLevel) {
            if (playerCharacterSummaryData.ContainsKey(unitController.CharacterId) == false) {
                return;
            }
            playerCharacterSummaryData[unitController.CharacterId].Level = newLevel;

            characterGroupServiceServer.ProcessStatusChange(unitController.CharacterId);
            friendServiceServer.ProcessStatusChange(unitController.CharacterId);
            guildServiceServer.ProcessStatusChange(unitController.CharacterId);
        }

        public void ProcessClassChange(UnitController unitController, CharacterClass newClass) {
            if (playerCharacterSummaryData.ContainsKey(unitController.CharacterId) == false) {
                return;
            }
            playerCharacterSummaryData[unitController.CharacterId].CharacterClass = newClass;

            characterGroupServiceServer.ProcessStatusChange(unitController.CharacterId);
            friendServiceServer.ProcessStatusChange(unitController.CharacterId);
            guildServiceServer.ProcessStatusChange(unitController.CharacterId);
        }

        public void SetCharacterOnline(int characterId, bool isOnline) {
            if (playerCharacterSummaryData.ContainsKey(characterId) == false) {
                return;
            }
            playerCharacterSummaryData[characterId].IsOnline = isOnline;

            characterGroupServiceServer.ProcessStatusChange(characterId);
            friendServiceServer.ProcessStatusChange(characterId);
            guildServiceServer.ProcessStatusChange(characterId);
        }

        public void SetCharacterZone(int characterId, string sceneDisplayName) {
            if (playerCharacterSummaryData.ContainsKey(characterId) == false) {
                return;
            }
            playerCharacterSummaryData[characterId].CurrentZoneName = sceneDisplayName;

            characterGroupServiceServer.ProcessStatusChange(characterId);
            friendServiceServer.ProcessStatusChange(characterId);
            guildServiceServer.ProcessStatusChange(characterId);
        }

    }

}