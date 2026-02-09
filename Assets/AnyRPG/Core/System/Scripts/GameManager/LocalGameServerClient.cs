using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AnyRPG { 
    public class LocalGameServerClient : ConfiguredClass {

        private ServerStateSaveData serverStateSaveData = new ServerStateSaveData();
        private string serverStateSaveFolderName = string.Empty;
        private const string stateSaveFileName = "ServerState.json";
        private Coroutine monitorCoroutine = null;
        private int saveInterval = 10;
        private bool saveDataDirty = false;

        int nextGuildId = 1;
        private string guildSaveFolderName = string.Empty;

        private int auctionIdCounter = 1;
        private string auctionSaveFolderName = string.Empty;

        private int mailIdCounter = 1;
        private string mailSaveFolderName = string.Empty;

        private int accountIdCounter = 1;
        private string accountSaveFolderName = string.Empty;

        private int playerCharacterIdCounter = 1;
        private string playerCharacterSaveFolderName = string.Empty;

        private string itemInstanceSaveFolderName = string.Empty;

        private string friendSaveFolderName = string.Empty;


        // game manager references
        private MailService mailService = null;
        private AuctionService auctionService = null;
        private GuildServiceServer guildServiceServer = null;
        private FriendServiceServer friendServiceServer = null;

        public LocalGameServerClient(SystemGameManager systemGameManager) {
            Configure(systemGameManager);
        }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            
            MakeSaveFolders();
            //networkManagerServer.OnStartServer += HandleStartServer;
            networkManagerServer.OnStopServer += HandleStopServer;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            mailService = systemGameManager.MailService;
            auctionService = systemGameManager.AuctionService;
            guildServiceServer = systemGameManager.GuildServiceServer;
            friendServiceServer = systemGameManager.FriendServiceServer;
        }

        private void MakeSaveFolders() {
            //Debug.Log("UserAccountService.MakeSaveFolder()");

            Regex regex = new Regex("[^a-zA-Z0-9]");
            string gameNameString = regex.Replace(systemConfigurationManager.GameName, "");
            if (gameNameString == string.Empty) {
                return;
            }

            serverStateSaveFolderName = $"{Application.persistentDataPath}/{gameNameString}/Online/Server";
            accountSaveFolderName = $"{Application.persistentDataPath}/{gameNameString}/Online/UserAccounts";
            itemInstanceSaveFolderName = $"{Application.persistentDataPath}/{gameNameString}/Online/Items";
            playerCharacterSaveFolderName = $"{Application.persistentDataPath}/{gameNameString}/Online/PlayerCharacters";
            guildSaveFolderName = $"{Application.persistentDataPath}/{gameNameString}/Online/Guilds";
            auctionSaveFolderName = $"{Application.persistentDataPath}/{gameNameString}/Online/Auction";
            mailSaveFolderName = $"{Application.persistentDataPath}/{gameNameString}/Online/Mail";
            friendSaveFolderName = $"{Application.persistentDataPath}/{gameNameString}/Online/Friends";

            if (!Directory.Exists($"{Application.persistentDataPath}/{gameNameString}")) {
                Directory.CreateDirectory($"{Application.persistentDataPath}/{gameNameString}");
            }
            if (!Directory.Exists($"{Application.persistentDataPath}/{gameNameString}/Online")) {
                Directory.CreateDirectory($"{Application.persistentDataPath}/{gameNameString}/Online");
            }
            if (!Directory.Exists(serverStateSaveFolderName)) {
                Directory.CreateDirectory(serverStateSaveFolderName);
            }
            if (!Directory.Exists(accountSaveFolderName)) {
                Directory.CreateDirectory(accountSaveFolderName);
            }
            if (!Directory.Exists(itemInstanceSaveFolderName)) {
                Directory.CreateDirectory(itemInstanceSaveFolderName);
            }
            if (!Directory.Exists(playerCharacterSaveFolderName)) {
                Directory.CreateDirectory(playerCharacterSaveFolderName);
            }
            if (!Directory.Exists(guildSaveFolderName)) {
                Directory.CreateDirectory(guildSaveFolderName);
            }
            if (!Directory.Exists(auctionSaveFolderName)) {
                Directory.CreateDirectory(auctionSaveFolderName);
            }
            if (!Directory.Exists(mailSaveFolderName)) {
                Directory.CreateDirectory(mailSaveFolderName);
            }
            if (!Directory.Exists(friendSaveFolderName)) {
                Directory.CreateDirectory(friendSaveFolderName);
            }
        }

        public void ProcessStartServer() {
            //Debug.Log($"ServerStateService.HandleStartServer()");

            LoadStateDataFile();
            LoadAccountIdCounter(serverStateSaveData.accountIdCounter);
            LoadPlayerCharacterIdCounter(serverStateSaveData.playerCharacterIdCounter);
            LoadMailIdCounter(serverStateSaveData.mailIdCounter);
            LoadAuctionIdCounter(serverStateSaveData.auctionIdCounter);
            LoadGuildIdCounter(serverStateSaveData.guildIdCounter);
            BeginMonitoringServerState();
        }

        private void HandleStopServer() {
            SaveStateDataFile();
            EndMonitoringServerState();
        }


        private void BeginMonitoringServerState() {
            if (monitorCoroutine == null) {
                monitorCoroutine = systemGameManager.StartCoroutine(MonitorServerState());
            }
        }

        private void EndMonitoringServerState() {
            if (monitorCoroutine != null) {
                systemGameManager.StopCoroutine(monitorCoroutine);
            }
            SaveStateDataFile();
        }

        public IEnumerator MonitorServerState() {
            //Debug.Log($"ServerStateService.MonitorServerState()");

            while (systemGameManager.GameMode == GameMode.Network) {
                if (saveDataDirty == true) {
                    SaveStateDataFile();
                }
                yield return new WaitForSeconds(saveInterval);
            }
        }

        public void SaveStateDataFile() {
            //Debug.Log($"ServerStateService.SaveStateDataFile()");

            string jsonString = JsonUtility.ToJson(serverStateSaveData);
            string jsonSavePath = $"{serverStateSaveFolderName}/{stateSaveFileName}";
            File.WriteAllText(jsonSavePath, jsonString);

            saveDataDirty = false;
        }

        public void LoadStateDataFile() {
            //Debug.Log($"ServerStateService.LoadDataFile()");

            string jsonSavePath = $"{serverStateSaveFolderName}/{stateSaveFileName}";
            if (File.Exists(jsonSavePath)) {
                string jsonString = File.ReadAllText(jsonSavePath);
                serverStateSaveData = JsonUtility.FromJson<ServerStateSaveData>(jsonString);
            }
        }

        // ************************************
        // GUILD
        // ************************************

        public void SetGuildIdCounter(int guildIdCounter) {
            serverStateSaveData.guildIdCounter = guildIdCounter;
            saveDataDirty = true;
        }

        public void LoadGuildList() {
            //Debug.Log("UserAccountService.LoadAllUserAccounts()");
            List<GuildSaveData> guildList = new List<GuildSaveData>();
            // load all user accounts from storage
            string[] fileEntries = Directory.GetFiles(guildSaveFolderName, "*.json");
            foreach (string fileName in fileEntries) {
                //Debug.Log($"Loading user account from file: {fileName}");
                string jsonString = File.ReadAllText(fileName);
                GuildSaveData guild = JsonUtility.FromJson<GuildSaveData>(jsonString);
                if (guild == null) {
                    Debug.LogWarning($"LocalGameServerClient.LoadGuildList() file {fileName} produced an invalid Guild. It will be skipped.");
                    continue;
                }

                guildList.Add(guild);
            }

            guildServiceServer.ProcessLoadGuildListResponse(guildList);
        }

        public void SaveGuild(Guild guild) {
            //Debug.Log($"GuildServiceServer.SaveGuildFile({guild.guildId})");

            GuildSaveData guildSaveData = new GuildSaveData(guild);

            string jsonString = JsonUtility.ToJson(guildSaveData, true);
            string fileName = $"{guildSaveFolderName}/{guildSaveData.GuildId}.json";
            File.WriteAllText(fileName, jsonString);
        }

        public void DeleteGuild(int guildId) {
            //Debug.Log($"GuildServiceServer.DeleteGuildFile({guildId})");

            string fileName = $"{guildSaveFolderName}/{guildId}.json";
            if (File.Exists(fileName)) {
                File.Delete(fileName);
            }
        }

        public void LoadGuildIdCounter(int guildIdCounter) {
            nextGuildId = guildIdCounter;
        }

        public void GetGuildId(Guild guild) {
            guild.GuildId = nextGuildId;
            nextGuildId++;
            SetGuildIdCounter(nextGuildId);
        }

        // ************************************
        // AUCTION ITEM
        // ************************************

        public void SetAuctionIdCounter(int auctionIdCounter) {
            serverStateSaveData.auctionIdCounter = auctionIdCounter;
            saveDataDirty = true;
        }

        public void LoadAuctionIdCounter(int newCounterValue) {
            //Debug.Log($"AuctionService.LoadAuctionIdCounter({newCounterValue})");

            auctionIdCounter = newCounterValue;
        }

        private void MakeAuctionItemSaveFolder(int playerCharacterId) {
            //Debug.Log("AuctionService.MakeAuctionItemSaveFolder()");

            string saveFolderName = GetAuctionItemSaveFolder(playerCharacterId);
            if (!Directory.Exists(saveFolderName)) {
                Directory.CreateDirectory(saveFolderName);
            }
        }

        private string GetAuctionItemSaveFolder(int playerCharacterId) {
            //Debug.Log("AuctionService.GetAuctionItemSaveFolder()");

            return $"{auctionSaveFolderName}/{playerCharacterId}";
        }

        public void GetAuctionItemList() {
            //Debug.Log("AuctionService.GetAuctionItemList()");
            List<AuctionItem> auctionItemList = new List<AuctionItem>();
            if (Directory.Exists(auctionSaveFolderName)) {
                string[] accountDirectories = Directory.GetDirectories(auctionSaveFolderName);
                foreach (string playerCharacterDirectory in accountDirectories) {
                    string[] fileEntries = Directory.GetFiles(playerCharacterDirectory);
                    foreach (string fileName in fileEntries) {
                        if (fileName.EndsWith(".json")) {
                            string jsonString = File.ReadAllText(fileName);
                            AuctionItem auctionItem = JsonUtility.FromJson<AuctionItem>(jsonString);
                            if (auctionItem == null) {
                                Debug.LogWarning($"LocalGameServerClient.GetAuctionItemList() file {fileName} produced an invalid Auction Item. It will be skipped.");
                                continue;
                            }
                            auctionItemList.Add(auctionItem);
                        }
                    }
                }
            }

            auctionService.ProcessLoadAuctionItemList(auctionItemList);
        }

        public void DeleteAuctionItem(AuctionItem auctionItem) {
            //Debug.Log($"AuctionService.DeleteAuctionItem({auctionItem.AuctionItemId})");

            string jsonSavePath = $"{GetAuctionItemSaveFolder(auctionItem.SellerPlayerCharacterId)}/{auctionItem.AuctionItemId}.json";
            if (File.Exists(jsonSavePath)) {
                File.Delete(jsonSavePath);
            }
        }

        public void GetNewAuctionItemId(AuctionItem auctionItem) {
            //Debug.Log($"AuctionService.GetNewAuctionItemId()");

            auctionItem.AuctionItemId = auctionIdCounter;
            auctionIdCounter++;
            SetAuctionIdCounter(auctionIdCounter);
        }

        public bool SaveAuctionFile(int playerCharacterId, AuctionItem auctionItem) {
            //Debug.Log($"AuctionService.SaveAuctionFile(playerCharacterId: {playerCharacterId}, {auctionItemId})");

            MakeAuctionItemSaveFolder(playerCharacterId);

            string jsonString = JsonUtility.ToJson(auctionItem);
            string jsonSavePath = $"{GetAuctionItemSaveFolder(playerCharacterId)}/{auctionItem.AuctionItemId}.json";
            File.WriteAllText(jsonSavePath, jsonString);

            return true;
        }

        // ****************************
        // MAIL
        // ****************************


        public void SetMailIdCounter(int mailIdCounter) {
            serverStateSaveData.mailIdCounter = mailIdCounter;
            saveDataDirty = true;
        }

        public void LoadMailIdCounter(int newCounterValue) {
            //Debug.Log($"MailService.LoadMailIdCounter({newCounterValue})");

            mailIdCounter = newCounterValue;
        }

        private void MakeMessageSaveFolder(int playerCharacterId) {
            //Debug.Log("PlayerCharacterService.MakeSaveFolder()");

            string saveFolderName = GetMessageSaveFolder(playerCharacterId);
            if (!Directory.Exists(saveFolderName)) {
                Directory.CreateDirectory(saveFolderName);
            }
        }

        private string GetMessageSaveFolder(int playerCharacterId) {

            return $"{mailSaveFolderName}/{playerCharacterId}";
        }

        public void GetNewMailMessageId(MailMessage mailMessage) {
            //Debug.Log($"MailService.GetNewMessageId()");

            mailMessage.MessageId = mailIdCounter;
            mailIdCounter++;
            SetMailIdCounter(mailIdCounter);
        }

        public bool DeleteMessage(int playerCharacterId, int messageId) {
            string jsonSavePath = $"{GetMessageSaveFolder(playerCharacterId)}/{messageId}.json";
            if (File.Exists(jsonSavePath)) {
                File.Delete(jsonSavePath);
            }
            return true;
        }

        public bool SaveMailFile(int playerCharacterId, MailMessage mailMessage) {
            //Debug.Log($"MailService.SaveMailFile({playerCharacterId}, {messageId})");

            MakeMessageSaveFolder(playerCharacterId);

            string jsonString = JsonUtility.ToJson(mailMessage);
            string jsonSavePath = $"{GetMessageSaveFolder(playerCharacterId)}/{mailMessage.MessageId}.json";
            File.WriteAllText(jsonSavePath, jsonString);

            return true;
        }

        public MailMessage GetMailMessage(int messageId, int playerCharacterId) {
            string jsonSavePath = $"{GetMessageSaveFolder(playerCharacterId)}/{messageId}.json";
            if (File.Exists(jsonSavePath)) {
                string jsonString = File.ReadAllText(jsonSavePath);
                MailMessage mailMessage = JsonUtility.FromJson<MailMessage>(jsonString);
                if (mailMessage == null) {
                    Debug.LogWarning($"LocalGameServerClient.GetMailMessage(messageId: {messageId}, playerCharacterId: {playerCharacterId}) file {jsonSavePath} produced an invalid Mail Message");
                }
                return mailMessage;
            }
            return null;
        }

        public void GetMailMessageList(int accountId, int playerCharacterId) {
            //Debug.Log($"MailService.GetMailMessages(accountId: {accountId}, playerCharacterId: {playerCharacterId})");

            List<MailMessage> mailMessageList = new List<MailMessage>();
            string accountSaveFolder = GetMessageSaveFolder(playerCharacterId);
            if (Directory.Exists(accountSaveFolder)) {
                string[] fileEntries = Directory.GetFiles(accountSaveFolder);
                foreach (string fileName in fileEntries) {
                    if (fileName.EndsWith(".json")) {
                        string jsonString = File.ReadAllText(fileName);
                        MailMessage mailMessage = JsonUtility.FromJson<MailMessage>(jsonString);
                        if (mailMessage == null) {
                            Debug.LogWarning($"LocalGameServerClient.GetMailMessages(accountId: {accountId}, playerCharacterId: {playerCharacterId}) file {fileName} produced an invalid Mail Message.  It will be skipped.");
                            continue;
                        }
                        mailMessageList.Add(mailMessage);
                    }
                }
            }
            mailService.ProcessMailMessageListResponse(accountId, mailMessageList);
        }

        // ****************************
        // ACCOUNTS
        // ****************************


        public void SetAccountIdCounter(int newCounter) {
            serverStateSaveData.accountIdCounter = newCounter;
            saveDataDirty = true;
        }

        public void LoadAccountIdCounter(int counterValue) {
            //Debug.Log($"UserAccountService.LoadAccountIdCounter({counterValue})");

            accountIdCounter = counterValue;
        }

        public void LoadAllUserAccounts() {
            //Debug.Log("UserAccountService.LoadAllUserAccounts()");

            List<UserAccount> userAccounts = new List<UserAccount>();
            // load all user accounts from storage
            string[] fileEntries = Directory.GetFiles(accountSaveFolderName, "*.json");
            foreach (string fileName in fileEntries) {
                //Debug.Log($"Loading user account from file: {fileName}");
                string jsonString = File.ReadAllText(fileName);
                UserAccount userAccount = JsonUtility.FromJson<UserAccount>(jsonString);
                if (userAccount == null) {
                    Debug.LogWarning($"LocalGameServerClient.LoadAllUserAccounts() file {fileName} produced an invalid User Account.  It will be skipped.");
                    continue;
                }
                userAccounts.Add(userAccount);
            }
            userAccountService.ProcessLoadAllUserAccounts(userAccounts);
        }

        /// <summary>
        /// add new user account to local storage
        /// </summary>
        /// <param name="userAccount"></param>
        public void SaveAccount(UserAccount userAccount) {
            //Debug.Log($"UserAccountService.SaveNewAccountLocal({userAccount.UserName})");

            string jsonString = JsonUtility.ToJson(userAccount);
            string jsonSavePath = $"{accountSaveFolderName}/{userAccount.Id}.json";
            File.WriteAllText(jsonSavePath, jsonString);
        }

        public int GetNewAccountId() {
            //Debug.Log("UserAccountService.GetNewAccountId()");

            int returnValue = accountIdCounter;
            accountIdCounter++;
            SetAccountIdCounter(accountIdCounter);

            //Debug.Log($"UserAccountService.GetNewAccountId() return {returnValue}");
            return returnValue;
        }

        // ****************************
        // PLAYER CHARACTERS
        // ****************************

        public void SetPlayerCharacterIdCounter(int playerCharacterIdCounter) {
            serverStateSaveData.playerCharacterIdCounter = playerCharacterIdCounter;
            saveDataDirty = true;
        }

        public void LoadPlayerNameList() {
            //Debug.Log("PlayerCharacterService.LoadPlayerNameMap()");

            List<CharacterSaveData> characterSaveDataList = new List<CharacterSaveData>();
            if (Directory.Exists(playerCharacterSaveFolderName)) {
                string[] accountDirectories = Directory.GetDirectories(playerCharacterSaveFolderName);
                foreach (string accountDirectory in accountDirectories) {
                    string[] fileEntries = Directory.GetFiles(accountDirectory);
                    foreach (string fileName in fileEntries) {
                        if (fileName.EndsWith(".json")) {
                            string jsonString = File.ReadAllText(fileName);
                            CharacterSaveData characterSaveData = JsonUtility.FromJson<CharacterSaveData>(jsonString);
                            if (characterSaveData == null) {
                                Debug.LogWarning($"LocalGameServerClient.LoadPlayerNameList() file {fileName} produced an invalid Character Save Data.  It will be skipped.");
                                continue;
                            }
                            characterSaveDataList.Add(characterSaveData);
                        }
                    }
                }
            }

            playerCharacterService.ProcessLoadPlayerNameList(characterSaveDataList);
        }

        public void LoadPlayerCharacterIdCounter(int newCounterValue) {
            //Debug.Log($"PlayerCharacterService.LoadPlayerCharacterIdCounter({newCounterValue})");

            playerCharacterIdCounter = newCounterValue;
        }

        private void MakeAccountSaveFolder(int accountId) {
            //Debug.Log("PlayerCharacterService.MakeSaveFolder()");

            string saveFolderName = GetAccountSaveFolder(accountId);
            if (!Directory.Exists(saveFolderName)) {
                Directory.CreateDirectory(saveFolderName);
            }
        }

        private string GetAccountSaveFolder(int accountId) {

            return $"{playerCharacterSaveFolderName}/{accountId}";
        }

        public int AddPlayerCharacter(int accountId, CharacterSaveData characterSaveData) {

            MakeAccountSaveFolder(accountId);

            characterSaveData.CharacterId = GetNewPlayerCharacterId();
            SaveDataFile(accountId, characterSaveData);

            return characterSaveData.CharacterId;
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
            playerCharacterIdCounter++;
            SetPlayerCharacterIdCounter(playerCharacterIdCounter);

            //Debug.Log($"PlayerCharacterService.GetNewPlayerCharacterId() return {returnValue}");
            return returnValue;
        }

        public bool SavePlayerCharacter(int accountId, CharacterSaveData characterSaveData) {
            //Debug.Log($"PlayerCharacterService.SavePlayerCharacter({accountId})");

            return SaveDataFile(accountId, characterSaveData);
        }

        public void GetPlayerCharacters(int accountId) {
            List<CharacterSaveData> characterSaveDataList = new List<CharacterSaveData>();
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
                        characterSaveDataList.Add(characterSaveData);
                    }
                }
            }

            playerCharacterService.ProcessLoadCharacterListResponse(accountId, characterSaveDataList);
        }

        public bool DeletePlayerCharacter(int accountId, int playerCharacterId) {
            string jsonSavePath = $"{GetAccountSaveFolder(accountId)}/{playerCharacterId}.json";
            if (File.Exists(jsonSavePath)) {
                File.Delete(jsonSavePath);
            }

            return true;
        }

        public CharacterSaveData GetPlayerCharacterSaveData(int accountId, int playerCharacterId) {
            string jsonSavePath = $"{GetAccountSaveFolder(accountId)}/{playerCharacterId}.json";
            if (File.Exists(jsonSavePath)) {
                string jsonString = File.ReadAllText(jsonSavePath);
                CharacterSaveData characterSaveData = JsonUtility.FromJson<CharacterSaveData>(jsonString);
                if (characterSaveData == null) {
                    Debug.LogWarning($"LocalGameServerClient.GetPlayerCharacterSaveData(accountId: {accountId}, playerCharacterId: {playerCharacterId}): Could not load player character save data from file {jsonSavePath}.");
                }
                return characterSaveData;
            }
            return null;
        }

        // ****************************
        // ITEM INSTANCES
        // ****************************


        public void LoadAllItems() {
            //Debug.Log("LocalGameServerClient.LoadAllItems()");

            List<ItemInstanceSaveData> itemInstanceListResponse = new List<ItemInstanceSaveData>();
            string[] fileEntries = Directory.GetFiles(itemInstanceSaveFolderName, "*.json");
            foreach (string fileName in fileEntries) {
                string jsonString = File.ReadAllText(fileName);
                ItemInstanceSaveData itemInstanceSaveData = JsonUtility.FromJson<ItemInstanceSaveData>(jsonString);
                if (itemInstanceSaveData == null) {
                    Debug.LogWarning($"LocalGameServerClient.LoadAllItems(): Could not load item save data from file {fileName}. This item will be skipped.");
                    continue;
                }
                itemInstanceListResponse.Add(itemInstanceSaveData);
            }
            systemItemManager.ProcessLoadAllItemInstances(itemInstanceListResponse);
        }

        public bool SaveItemInstanceDataFile(InstantiatedItem instantiatedItem) {
            //Debug.Log($"SystemItemManager.SaveDataFile({instantiatedItem.Item.ResourceName})");

            ItemInstanceSaveData itemInstanceSaveData = instantiatedItem.GetItemSaveData();
            string jsonString = JsonUtility.ToJson(itemInstanceSaveData);
            string jsonSavePath = $"{itemInstanceSaveFolderName}/{instantiatedItem.InstanceId}.json";
            File.WriteAllText(jsonSavePath, jsonString);

            return true;
        }

        // ****************************
        // FRIENDS
        // ****************************

        public void SaveFriendList(FriendList friendList) {
            //Debug.Log($"FriendServiceServer.SaveFriendFile({friendList.playerCharacterId})");

            FriendListSaveData friendListSaveData = new FriendListSaveData(friendList);

            string jsonString = JsonUtility.ToJson(friendListSaveData, true);
            string fileName = $"{friendSaveFolderName}/{friendListSaveData.PlayerCharacterId}.json";
            File.WriteAllText(fileName, jsonString);
        }

        public void LoadAllFriends() {
            
            List<FriendListSaveData> friendListSaveDatas = new List<FriendListSaveData>();

            // load all user accounts from storage
            string[] fileEntries = Directory.GetFiles(friendSaveFolderName, "*.json");
            foreach (string fileName in fileEntries) {
                string jsonString = File.ReadAllText(fileName);
                FriendListSaveData friendListSaveData = JsonUtility.FromJson<FriendListSaveData>(jsonString);
                if (friendListSaveData == null) {
                    Debug.LogWarning($"LocalGameServerClient.LoadAllItems(): Could not load friend list save data from file {fileName}. This list will be skipped.");
                    continue;
                }
                friendListSaveDatas.Add(friendListSaveData);
            }
            friendServiceServer.ProcessLoadAllFriendLists(friendListSaveDatas);
        }
    }

}