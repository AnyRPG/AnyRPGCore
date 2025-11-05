using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AnyRPG { 
    public class PlayerCharacterService : ConfiguredClass {

        private int playerCharacterIdCounter = 1;
        private string baseSaveFolderName = string.Empty;
        private const string playerIdCounterKey = "Server.PlayerCharacterIdCounter";

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            MakeBaseSaveFolder();
            LoadPlayerCharacterIdCounter();
        }

        private void LoadPlayerCharacterIdCounter() {
            //Debug.Log("UserAccountService.LoadAccountIdCounter()");

            playerCharacterIdCounter = PlayerPrefs.GetInt(playerIdCounterKey, 1);
        }


        private void MakeBaseSaveFolder() {
            //Debug.Log("UserAccountService.MakeSaveFolder()");

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
            //Debug.Log("UserAccountService.MakeSaveFolder()");

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
            PlayerCharacterSaveData playerCharacterSaveData = new PlayerCharacterSaveData() {
                PlayerCharacterId = GetNewPlayerCharacterId(),
                SaveData = anyRPGSaveData
            };
            SaveDataFile(accountId, playerCharacterSaveData);

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
            //Debug.Log("UserAccountService.GetNewAccountId()");

            int returnValue = playerCharacterIdCounter;
            playerCharacterIdCounter++;
            PlayerPrefs.SetInt(playerIdCounterKey, playerCharacterIdCounter);
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

        public bool DeletePlayerCharacter(int accountId, int playerCharacterId) {
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