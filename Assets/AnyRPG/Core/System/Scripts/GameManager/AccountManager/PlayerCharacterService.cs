using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AnyRPG { 
    public class PlayerCharacterService : ConfiguredClass {

        // cached list of player character save data from client lookups used for loading games
        /// <summary>
        /// accountId, playerCharacterId, playerCharacterSaveData
        /// </summary>
        private Dictionary<int, Dictionary<int, CharacterSaveData>> playerCharacterDataDict = new Dictionary<int, Dictionary<int, CharacterSaveData>>();

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
        private NewGameManager newGameManager = null;
        private AuthenticationService authenticationService = null;
        private SaveManager saveManager = null;
        private ServerDataService serverDataService = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            //networkManagerServer.OnStartServer += HandleStartServer;
            networkManagerServer.OnStopServer += HandleStopServer;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            characterGroupServiceServer = systemGameManager.CharacterGroupServiceServer;
            friendServiceServer = systemGameManager.FriendServiceServer;
            guildServiceServer = systemGameManager.GuildServiceServer;
            newGameManager = systemGameManager.NewGameManager;
            authenticationService = systemGameManager.AuthenticationService;
            saveManager = systemGameManager.SaveManager;
            serverDataService = systemGameManager.ServerDataService;
        }

        private void HandleStopServer() {
            ClearPlayerNameMap();
        }

        public CharacterSaveData GetPlayerCharacterSaveData(int accountId, int playerCharacterId) {
            //Debug.Log($"PlayerCharacterService.GetPlayerCharacterSaveData(accountId: {accountId}, accountId: {playerCharacterId})");

            if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                return GetPlayerCharacterSaveDataFromCache(accountId, playerCharacterId);
            }
            return serverDataService.GetPlayerCharacterSaveData(accountId, playerCharacterId);
        }

        public CharacterSaveData GetPlayerCharacterSaveDataFromCache(int accountId, int playerCharacterId) {
            //Debug.Log($"PlayerCharacterService.GetPlayerCharacterSaveDataFromCache({accountId}, {playerCharacterId})");

            if (playerCharacterDataDict.ContainsKey(accountId) == false) {
                return null;
            }
            if (playerCharacterDataDict[accountId].ContainsKey(playerCharacterId) == false) {
                return null;
            }
            return playerCharacterDataDict[accountId][playerCharacterId];
        }

        public string GetPlayerNameFromId(int playerCharacterId) {
            if (playerNameLookupMap.ContainsKey(playerCharacterId)) {
                return playerNameLookupMap[playerCharacterId];
            }
            return "Unknown";
        }

        public void LoadPlayerNameList() {
            //Debug.Log("PlayerCharacterService.LoadPlayerNameMap()");

            serverDataService.LoadPlayerNameList();
        }

        public void ProcessLoadPlayerNameList(List<PlayerCharacterSerializedData> playerCharacterList) {
            //Debug.Log($"PlayerCharacterService.ProcessLoadPlayerNameList(count: {playerCharacterList.Count})");

            List<CharacterSaveData> characterSaveDataList = new List<CharacterSaveData>();
            foreach (PlayerCharacterSerializedData playerCharacterSerializedData in playerCharacterList) {
                CharacterSaveData characterSaveData = JsonUtility.FromJson<CharacterSaveData>(playerCharacterSerializedData.saveData);
                if (characterSaveData == null) {
                    Debug.LogWarning($"LocalGameServerClient.LoadPlayerNameList() invalid Character Save Data with id: {playerCharacterSerializedData.id}.  It will be skipped.");
                    continue;
                }
                characterSaveDataList.Add(characterSaveData);
            }
            ProcessLoadPlayerNameList(characterSaveDataList);
        }

        public void ProcessLoadPlayerNameList(List<CharacterSaveData> characterSaveDataList) {
            //Debug.Log($"PlayerCharacterService.ProcessLoadPlayerNameList(count: {characterSaveDataList.Count})");

            foreach (CharacterSaveData characterSaveData in characterSaveDataList) {
                saveManager.CharacterSaveDataPostLoad(characterSaveData);
                //CharacterSaveData characterSaveData = saveManager.LoadCharacterSaveDataFromString(playerCharacterData.saveData);
                if (!playerNameMap.ContainsKey(characterSaveData.CharacterName.ToLower()) && !playerNameLookupMap.ContainsKey(characterSaveData.CharacterId)) {
                    //Debug.Log($"PlayerCharacterService.LoadPlayerNameMap(): Loaded player ({characterSaveData.CharacterName}) with ID ({characterSaveData.CharacterId})");
                    playerNameMap.Add(characterSaveData.CharacterName.ToLower(), characterSaveData.CharacterId);
                    playerNameLookupMap.Add(characterSaveData.CharacterId, characterSaveData.CharacterName);
                    playerCharacterSummaryData.Add(characterSaveData.CharacterId, new CharacterSummaryData(characterSaveData, systemDataFactory));
                } else {
                    Debug.LogWarning($"PlayerCharacterService.ProcessLoadPlayerNameList(): Duplicate player name ({characterSaveData.CharacterName}) or character ID ({characterSaveData.CharacterId}) found. This character will be skipped.");
                }
            }
        }

        private void ClearPlayerNameMap() {
            playerNameMap.Clear();
            playerNameLookupMap.Clear();
            playerCharacterSummaryData.Clear();
        }

        public void RequestCreatePlayerCharacter(int accountId, CharacterSaveData requestedSaveData) {

            if (playerNameMap.ContainsKey(requestedSaveData.CharacterName.ToLower())) {
                ProcessCreatePlayerCharacterResponse(accountId, false, 0, requestedSaveData);
                LoadCharacterList(accountId);
                return;
            }

            CharacterSaveData characterSaveData = newGameManager.CreateNewPlayerSaveData(requestedSaveData);
            serverDataService.CreatePlayerCharacter(accountId, characterSaveData);
        }

        public void ProcessCreatePlayerCharacterResponse(int accountId, bool createSucceeded, int characterId, CharacterSaveData characterSaveData) {
            if (createSucceeded) {
                characterSaveData.CharacterId = characterId;
                if (playerNameMap.ContainsKey(characterSaveData.CharacterName.ToLower()) == false) {
                    playerNameMap.Add(characterSaveData.CharacterName.ToLower(), characterId);
                    playerNameLookupMap.Add(characterId, characterSaveData.CharacterName);
                    playerCharacterSummaryData.Add(characterSaveData.CharacterId, new CharacterSummaryData(characterSaveData, systemDataFactory));
                }
            } else {
                networkManagerServer.AdvertisePlayerNameNotAvailable(accountId);
            }
        }

        public void SavePlayerCharacter(PlayerCharacterMonitor playerCharacterMonitor) {
            //Debug.Log($"NetworkManagerServer.SavePlayerCharacter()");

            if (playerCharacterMonitor.unitController != null) {
                playerCharacterMonitor.SavePlayerLocation();
            }
            if (playerCharacterMonitor.saveDataDirty == true) {
                if (networkManagerServer.ServerMode == NetworkServerMode.MMO) {
                    serverDataService.SavePlayerCharacter(playerCharacterMonitor);
                }
            }
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

        public int GetPlayerIdFromName(string targetPlayerName) {
            //Debug.Log($"PlayerCharacterService.GetPlayerIdFromName({targetPlayerName})");

            if (playerNameMap.ContainsKey(targetPlayerName.ToLower()) == false) {
                return -1;
            }
            return playerNameMap[targetPlayerName.ToLower()];
        }

        public CharacterSummaryData GetSummaryData(int playerCharacterId) {
            //Debug.Log($"PlayerCharacterService.GetSummaryData(playerCharacterId: {playerCharacterId})");

            if (playerCharacterSummaryData.ContainsKey(playerCharacterId)) {
                return playerCharacterSummaryData[playerCharacterId];
            }
            Debug.LogWarning($"PlayerCharacterService.GetSummaryData(playerCharacterId: {playerCharacterId}) did not find summary data for character");
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

        public void RequestDeletePlayerCharacter(int accountId, int playerCharacterId) {
            serverDataService.DeletePlayerCharacter(accountId, playerCharacterId);
        }

        public void ProcessDeletePlayerCharacterResponse(int accountId, int playerCharacterId) {
            //Debug.Log($"NetworkManagerServer.ProcessDeletePlayerCharacterResponse({accountId})");

            if (playerNameLookupMap.ContainsKey(playerCharacterId) == false) {
                return;
            }
            string playerName = playerNameLookupMap[playerCharacterId];
            playerNameLookupMap.Remove(playerCharacterId);
            playerNameMap.Remove(playerName.ToLower());
            playerCharacterSummaryData.Remove(playerCharacterId);

            LoadCharacterList(accountId);
        }

        public void LoadCharacterList(int accountId) {
            //Debug.Log($"PlayerCharacterService.LoadCharacterList(accountId: {accountId})");

            serverDataService.LoadCharacterList(accountId);
        }

        public void ProcessLoadCharacterListResponse(int accountId, List<PlayerCharacterSerializedData> playerCharacterSerializedDataList) {
            //Debug.Log($"PlayerCharacterService.ProcessLoadCharacterListResponse(accountId: {accountId}, count: {playerCharacters.Count})");

            List<CharacterSaveData> characterSaveDataList = new List<CharacterSaveData>();
            foreach (PlayerCharacterSerializedData playerCharacterSerializedData in playerCharacterSerializedDataList) {
                CharacterSaveData characterSaveData = saveManager.LoadCharacterSaveDataFromString(playerCharacterSerializedData.saveData);
                if (characterSaveData == null) {
                    Debug.LogWarning($"PlayerCharacterService.ProcessLoadCharacterListResponse(accountId: {accountId}) invalid Character Save Data for id {playerCharacterSerializedData.id}.  It will be skipped.");
                    continue;
                }
                characterSaveDataList.Add(characterSaveData);
            }

            ProcessLoadCharacterListResponse(accountId, characterSaveDataList);
        }

        public void ProcessLoadCharacterListResponse(int accountId, List<CharacterSaveData> characterSaveDataList) {
            //Debug.Log($"PlayerCharacterService.ProcessLoadCharacterListResponse(accountId: {accountId}, count: {playerCharacters.Count})");

            List<PlayerCharacterSaveData> playerCharacterSaveDataList = new List<PlayerCharacterSaveData>();
            foreach (CharacterSaveData characterSaveData in characterSaveDataList) {
                PlayerCharacterSaveData playerCharacterSaveData = new PlayerCharacterSaveData(characterSaveData, systemItemManager);
                playerCharacterSaveDataList.Add(playerCharacterSaveData);
            }
            Dictionary<int, CharacterSaveData> characterSaveDataDict = new Dictionary<int, CharacterSaveData>();
            foreach (PlayerCharacterSaveData playerCharacterSaveData in playerCharacterSaveDataList) {
                characterSaveDataDict.Add(playerCharacterSaveData.CharacterSaveData.CharacterId, playerCharacterSaveData.CharacterSaveData);
            }
            if (playerCharacterDataDict.ContainsKey(accountId)) {
                playerCharacterDataDict[accountId] = characterSaveDataDict;
            } else {
                playerCharacterDataDict.Add(accountId, characterSaveDataDict);
            }

            networkManagerServer.AdvertiseLoadCharacterList(accountId, playerCharacterSaveDataList);
        }


    }

}