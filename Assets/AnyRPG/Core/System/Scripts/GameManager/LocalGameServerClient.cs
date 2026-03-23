using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace AnyRPG { 
    public class LocalGameServerClient : ConfiguredClass {

        private ServerStateSaveData serverStateSaveData = new ServerStateSaveData();
        private string serverStateSaveFolderName = string.Empty;
        private const string stateSaveFileName = "ServerState.json";
        private Coroutine monitorCoroutine = null;
        private int saveInterval = 10;
        private bool saveDataDirty = false;
        private int activeSaveTasks = 0;
        private bool isSavingStateFile = false;

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
        private ServerDataService serverDataService = null;

        public int ActiveSaveTasks { get => activeSaveTasks; }

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
            serverDataService = systemGameManager.ServerDataService;
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
            //Debug.Log($"ServerStateService.HandleStopServer()");

            SaveStateDataFileAsync();
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
        }

        public IEnumerator MonitorServerState() {
            //Debug.Log($"ServerStateService.MonitorServerState()");

            var wait = new WaitForSeconds(saveInterval);

            while (systemGameManager.GameMode == GameMode.Network) {
                if (saveDataDirty == true && !isSavingStateFile) {
                    SaveStateDataFileAsync();
                }
                yield return wait;
            }
        }

        public async void SaveStateDataFileAsync() {
            //Debug.Log($"ServerStateService.SaveStateDataFileAsync()");

            if (isSavingStateFile) {
                return;
            }
            isSavingStateFile = true;
            // 1. Mark the save task as active on the Main Thread
            System.Threading.Interlocked.Increment(ref activeSaveTasks);

            try {
                // Snapshot the state data on the Main Thread
                string jsonString = JsonUtility.ToJson(serverStateSaveData);
                string folderPath = serverStateSaveFolderName;
                string fileName = stateSaveFileName;
                string fullPath = Path.Combine(folderPath, fileName);

                // Immediately clear the dirty flag on the Main Thread 
                // to prevent redundant save calls.
                saveDataDirty = false;

                // 2. Offload the folder check and file writing
                await Task.Run(() => {
                    try {
                        if (!Directory.Exists(folderPath)) {
                            Directory.CreateDirectory(folderPath);
                        }
                        File.WriteAllText(fullPath, jsonString);
                    } catch (System.Exception ex) {
                        // If it fails, you might want to set saveDataDirty = true 
                        // so the server tries again later.
                        Debug.LogError($"[Server State Save Error]: {ex.Message}");
                    }
                });
            } finally {
                isSavingStateFile = false;
                // 3. ALWAYS decrement the counter when finished
                System.Threading.Interlocked.Decrement(ref activeSaveTasks);
            }
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

        /*
        public void LoadGuildListAsync() {
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
        */

        public async void LoadGuildListAsync() {
            //Debug.Log("LocalGameServerClient.LoadGuildListAsync()");

            var progressReporter = new Progress<int>(count => {
                serverDataService.NotifyOnLoadGuild(count);
            });

            // Offload the Disk I/O and JSON parsing to a background thread
            List<GuildSaveData> results = await Task.Run(() => PerformGuildFileIOWork(progressReporter));

            guildServiceServer.ProcessLoadGuildListResponse(results);
        }

        // Background Worker (No Unity API calls here except JsonUtility)
        private List<GuildSaveData> PerformGuildFileIOWork(IProgress<int> progress) {
            List<GuildSaveData> guildList = new List<GuildSaveData>();

            if (!Directory.Exists(guildSaveFolderName)) return guildList;

            string[] fileEntries = Directory.GetFiles(guildSaveFolderName, "*.json");

            int count = 0;
            // Note: We can't use Debug.LogWarning safely here on some Unity versions.
            foreach (string fileName in fileEntries) {
                string jsonString = File.ReadAllText(fileName);
                GuildSaveData guild = JsonUtility.FromJson<GuildSaveData>(jsonString);

                if (guild != null) {
                    guildList.Add(guild);
                }
                count++;
                progress?.Report(count);
            }

            return guildList;
        }

        public async void SaveGuildAsync(Guild guild) {
            System.Threading.Interlocked.Increment(ref activeSaveTasks);
            try {
                GuildSaveData guildSaveData = new GuildSaveData(guild);
                string jsonString = JsonUtility.ToJson(guildSaveData, true);
                string fileName = Path.Combine(guildSaveFolderName, $"{guildSaveData.GuildId}.json");

                string errorMessage = null;

                await Task.Run(() => {
                    try {
                        if (!Directory.Exists(guildSaveFolderName)) {
                            Directory.CreateDirectory(guildSaveFolderName);
                        }
                        File.WriteAllText(fileName, jsonString);
                    } catch (System.Exception ex) {
                        // Store the message to report it back on the Main Thread
                        errorMessage = ex.Message;
                    }
                });

                // Back on the Main Thread, we can safely use Debug.Log
                if (errorMessage != null) {
                    Debug.LogError($"Failed to save guild {guild.GuildId}: {errorMessage}");
                }
            } finally {
                // 3. ALWAYS decrement the counter when finished, 
                // whether it succeeded or crashed.
                System.Threading.Interlocked.Decrement(ref activeSaveTasks);
            }
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

        private string GetAuctionItemSaveFolder(int playerCharacterId) {
            //Debug.Log("AuctionService.GetAuctionItemSaveFolder()");

            return $"{auctionSaveFolderName}/{playerCharacterId}";
        }

        public async void GetAuctionItemListAsync() {
            var progressReporter = new Progress<int>(count => {
                serverDataService.NotifyOnLoadAuctionItem(count);
            });

            // Offload the nested directory scanning and file loading
            List<AuctionItem> results = await Task.Run(() => PerformAuctionFileIOWork(progressReporter));

            // Back on Main Thread: Hand the final list to the service
            auctionService.ProcessLoadAuctionItemList(results);
        }

        private List<AuctionItem> PerformAuctionFileIOWork(IProgress<int> progress) {
            List<AuctionItem> auctionItemList = new List<AuctionItem>();

            if (!Directory.Exists(auctionSaveFolderName)) return auctionItemList;

            string[] accountDirectories = Directory.GetDirectories(auctionSaveFolderName);
            int totalItemsLoaded = 0;

            foreach (string playerCharacterDirectory in accountDirectories) {
                // Only grab .json files directly from the OS to save time
                string[] fileEntries = Directory.GetFiles(playerCharacterDirectory, "*.json");

                foreach (string fileName in fileEntries) {
                    try {
                        string jsonString = File.ReadAllText(fileName);
                        AuctionItem auctionItem = JsonUtility.FromJson<AuctionItem>(jsonString);

                        if (auctionItem != null) {
                            auctionItemList.Add(auctionItem);

                            // Increment and report the new total
                            totalItemsLoaded++;
                            progress?.Report(totalItemsLoaded);
                        }
                    } catch (System.Exception) {
                        // Skip corrupted files or access errors
                    }
                }
            }

            return auctionItemList;
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

        public async void SaveAuctionFileAsync(int playerCharacterId, AuctionItem auctionItem) {
            // 1. Mark the task as active before doing anything else
            System.Threading.Interlocked.Increment(ref activeSaveTasks);

            try {
                // Prepare data on the Main Thread (Snapshotting)
                string jsonString = JsonUtility.ToJson(auctionItem);
                string folderPath = GetAuctionItemSaveFolder(playerCharacterId);
                string fileName = $"{auctionItem.AuctionItemId}.json";
                string fullPath = Path.Combine(folderPath, fileName);

                // 2. Offload the folder check and disk write
                await Task.Run(() => {
                    try {
                        // Moved folder creation here to keep the Main Thread 100% smooth
                        if (!Directory.Exists(folderPath)) {
                            Directory.CreateDirectory(folderPath);
                        }

                        File.WriteAllText(fullPath, jsonString);
                    } catch (System.Exception ex) {
                        // We use a local string or handle logging carefully
                        // (Note: In Unity 6, Debug.Log is mostly thread-safe, but finally is safer)
                        Debug.LogError($"[Auction Save Error] ID {auctionItem.AuctionItemId}: {ex.Message}");
                    }
                });
            } finally {
                // 3. ALWAYS decrement so the Shutdown Guard knows this specific write finished
                System.Threading.Interlocked.Decrement(ref activeSaveTasks);
            }
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

        public async void SaveMailFileAsync(int playerCharacterId, MailMessage mailMessage) {
            // 1. Increment the safety counter on the Main Thread
            System.Threading.Interlocked.Increment(ref activeSaveTasks);

            try {
                // Prepare data and paths on the Main Thread (Snapshotting)
                string jsonString = JsonUtility.ToJson(mailMessage);
                string folderPath = GetMessageSaveFolder(playerCharacterId);
                string fileName = $"{mailMessage.MessageId}.json";
                string fullPath = Path.Combine(folderPath, fileName);

                // 2. Offload the slow Disk I/O to a background thread
                await Task.Run(() => {
                    try {
                        if (!Directory.Exists(folderPath)) {
                            Directory.CreateDirectory(folderPath);
                        }
                        File.WriteAllText(fullPath, jsonString);
                    } catch (System.Exception ex) {
                        // Caught on background, safely logged to Unity console
                        Debug.LogError($"[Mail Save Error] Player {playerCharacterId}, Message {mailMessage.MessageId}: {ex.Message}");
                    }
                });
            } finally {
                // 3. ALWAYS decrement the counter when finished
                System.Threading.Interlocked.Decrement(ref activeSaveTasks);
            }
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

        public async void GetMailMessageListAsync(int accountId, int playerCharacterId) {
            // 1. Capture the path on the Main Thread
            string accountSaveFolder = GetMessageSaveFolder(playerCharacterId);

            // 2. Offload the file reading and parsing to the background
            List<MailMessage> results = await Task.Run(() => PerformMailFileIOWork(accountSaveFolder));

            // 3. Back on Main Thread: Send the data to the service
            mailService.ProcessMailMessageListResponse(accountId, results);
        }

        private List<MailMessage> PerformMailFileIOWork(string folderPath) {
            List<MailMessage> mailMessageList = new List<MailMessage>();

            if (!Directory.Exists(folderPath)) return mailMessageList;

            // Filter for .json directly in the OS call
            string[] fileEntries = Directory.GetFiles(folderPath, "*.json");

            foreach (string fileName in fileEntries) {
                try {
                    string jsonString = File.ReadAllText(fileName);
                    MailMessage mailMessage = JsonUtility.FromJson<MailMessage>(jsonString);

                    if (mailMessage != null) {
                        mailMessageList.Add(mailMessage);
                    }
                } catch (System.Exception) {
                    // Silently skip corrupted files to keep the player's inbox loading
                }
            }

            return mailMessageList;
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

        public async void LoadAllUserAccountsAsync() {
            // 1. Snapshot any paths needed on the Main Thread
            string folderPath = accountSaveFolderName;

            // 2. Offload the bulk disk reading and JSON parsing
            List<UserAccount> results = await Task.Run(() => PerformUserAccountFileIOWork(folderPath));

            // 3. Back on Main Thread: Finalize the data
            userAccountService.ProcessLoadAllUserAccounts(results);
        }

        private List<UserAccount> PerformUserAccountFileIOWork(string folderPath) {
            List<UserAccount> userAccounts = new List<UserAccount>();

            if (!Directory.Exists(folderPath)) return userAccounts;

            // Use the OS-level filter for .json files
            string[] fileEntries = Directory.GetFiles(folderPath, "*.json");

            foreach (string fileName in fileEntries) {
                try {
                    string jsonString = File.ReadAllText(fileName);
                    UserAccount userAccount = JsonUtility.FromJson<UserAccount>(jsonString);

                    if (userAccount != null) {
                        userAccounts.Add(userAccount);
                    }
                } catch (System.Exception) {
                    // Background threads should catch exceptions to avoid silent crashes
                    // We skip the specific file and move to the next one
                }
            }

            return userAccounts;
        }

        /// <summary>
        /// add new user account to local storage
        /// </summary>
        /// <param name="userAccount"></param>
        public async void SaveAccountAsync(UserAccount userAccount) {
            // 1. Mark the save task as active on the Main Thread
            System.Threading.Interlocked.Increment(ref activeSaveTasks);

            try {
                // Prepare data and paths on the Main Thread (Snapshotting)
                string jsonString = JsonUtility.ToJson(userAccount);
                string folderPath = accountSaveFolderName;
                string fileName = $"{userAccount.Id}.json";
                string fullPath = Path.Combine(folderPath, fileName);

                // 2. Offload the disk work
                await Task.Run(() => {
                    try {
                        if (!Directory.Exists(folderPath)) {
                            Directory.CreateDirectory(folderPath);
                        }
                        File.WriteAllText(fullPath, jsonString);
                    } catch (System.Exception ex) {
                        Debug.LogError($"[Account Save Error] ID {userAccount.Id}: {ex.Message}");
                    }
                });
            } finally {
                // 3. ALWAYS decrement the counter when finished
                System.Threading.Interlocked.Decrement(ref activeSaveTasks);
            }
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

        public async void LoadPlayerNameListAsync() {
            // 1. Setup the reporter to send integers back to the UI/Status window
            var progressReporter = new Progress<int>(count => {
                serverDataService.NotifyOnLoadPlayerName(count);
            });

            // Capture the base folder path on the Main Thread
            string baseFolder = playerCharacterSaveFolderName;

            // 2. Offload the nested directory scanning and file loading
            List<CharacterSaveData> results = await Task.Run(() => PerformPlayerNameFileIOWork(baseFolder, progressReporter));

            // 3. Back on Main Thread: Process the full list
            playerCharacterService.ProcessLoadPlayerNameList(results);
        }

        private List<CharacterSaveData> PerformPlayerNameFileIOWork(string baseFolder, IProgress<int> progress) {
            List<CharacterSaveData> characterSaveDataList = new List<CharacterSaveData>();

            if (!Directory.Exists(baseFolder)) return characterSaveDataList;

            // Get all account sub-directories
            string[] accountDirectories = Directory.GetDirectories(baseFolder);
            int totalLoaded = 0;

            foreach (string accountDirectory in accountDirectories) {
                // Grab only .json files from the specific account folder
                string[] fileEntries = Directory.GetFiles(accountDirectory, "*.json");

                foreach (string fileName in fileEntries) {
                    try {
                        string jsonString = File.ReadAllText(fileName);
                        CharacterSaveData characterSaveData = JsonUtility.FromJson<CharacterSaveData>(jsonString);

                        if (characterSaveData != null) {
                            characterSaveDataList.Add(characterSaveData);

                            // Increment and report progress for every character found
                            totalLoaded++;
                            progress?.Report(totalLoaded);
                        }
                    } catch (System.Exception) {
                        // Ignore individual file errors to keep the process moving
                    }
                }
            }

            return characterSaveDataList;
        }

        public void LoadPlayerCharacterIdCounter(int newCounterValue) {
            //Debug.Log($"PlayerCharacterService.LoadPlayerCharacterIdCounter({newCounterValue})");

            playerCharacterIdCounter = newCounterValue;
        }

        private string GetAccountSaveFolder(int accountId) {

            return $"{playerCharacterSaveFolderName}/{accountId}";
        }

        public async Task<int> AddPlayerCharacterAsync(int accountId, CharacterSaveData characterSaveData) {
            // 1. Core logic on Main Thread
            characterSaveData.CharacterId = GetNewPlayerCharacterId();

            // 2. Start the save and await its completion 
            // (This ensures the ID is returned ONLY after the file is safe on disk)
            await SavePlayerCharacterDataFileAsync(accountId, characterSaveData);

            return characterSaveData.CharacterId;
        }

        public async Task SavePlayerCharacterDataFileAsync(int accountId, CharacterSaveData characterSaveData) {
            // 1. Increment the safety counter on the Main Thread
            System.Threading.Interlocked.Increment(ref activeSaveTasks);

            try {
                // Prepare data and paths on the Main Thread (Snapshotting)
                string folderPath = GetAccountSaveFolder(accountId);
                string jsonString = JsonUtility.ToJson(characterSaveData);
                string fileName = $"{characterSaveData.CharacterId}.json";
                string fullPath = Path.Combine(folderPath, fileName);

                // 2. Offload the I/O to a background thread
                await Task.Run(() => {
                    try {
                        if (!Directory.Exists(folderPath)) {
                            Directory.CreateDirectory(folderPath);
                        }
                        File.WriteAllText(fullPath, jsonString);
                    } catch (System.Exception ex) {
                        Debug.LogError($"[Character Save Error] Account {accountId}: {ex.Message}");
                    }
                });
            } finally {
                // 3. ALWAYS decrement the counter when finished
                System.Threading.Interlocked.Decrement(ref activeSaveTasks);
            }
        }


        private int GetNewPlayerCharacterId() {
            //Debug.Log("PlayerCharacterService.GetNewPlayerCharacterId()");

            int returnValue = playerCharacterIdCounter;
            playerCharacterIdCounter++;
            SetPlayerCharacterIdCounter(playerCharacterIdCounter);

            //Debug.Log($"PlayerCharacterService.GetNewPlayerCharacterId() return {returnValue}");
            return returnValue;
        }

        public async void GetPlayerCharactersAsync(int accountId) {
            // 1. Capture the path on the Main Thread
            string accountSaveFolder = GetAccountSaveFolder(accountId);

            // 2. Offload the folder scanning and file parsing to a background thread
            List<CharacterSaveData> results = await Task.Run(() => PerformCharacterListIOWork(accountId, accountSaveFolder));

            // 3. Back on Main Thread: Send the data to the service
            playerCharacterService.ProcessLoadCharacterListResponse(accountId, results);
        }

        private List<CharacterSaveData> PerformCharacterListIOWork(int accountId, string folderPath) {
            List<CharacterSaveData> characterSaveDataList = new List<CharacterSaveData>();

            if (!Directory.Exists(folderPath)) return characterSaveDataList;

            // Filter for .json directly at the OS level for speed
            string[] fileEntries = Directory.GetFiles(folderPath, "*.json");

            foreach (string fileName in fileEntries) {
                try {
                    string jsonString = File.ReadAllText(fileName);
                    CharacterSaveData characterSaveData = JsonUtility.FromJson<CharacterSaveData>(jsonString);

                    if (characterSaveData != null) {
                        characterSaveDataList.Add(characterSaveData);
                    }
                } catch (System.Exception) {
                    // Note: Debug.LogWarning is technically a Unity API; 
                    // staying silent here prevents background thread crashes.
                }
            }

            return characterSaveDataList;
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

        public async void LoadAllItemsAsync() {
            //Debug.Log("LocalGameServerClient.LoadAllItemsAsync()");

            var progressReporter = new Progress<int>(status => {
                serverDataService.NotifyOnLoadItem(status);
            });
            List<ItemInstanceSaveData> results = await Task.Run(() => LoadAllItems(progressReporter));
            systemItemManager.ProcessLoadAllItemInstances(results);
        }

        private List<ItemInstanceSaveData> LoadAllItems(IProgress<int> progress) {
            //Debug.Log("LocalGameServerClient.LoadAllItems()");

            List<ItemInstanceSaveData> itemInstanceListResponse = new List<ItemInstanceSaveData>();
            string[] fileEntries = Directory.GetFiles(itemInstanceSaveFolderName, "*.json");

            int count = 0;
            foreach (string fileName in fileEntries) {
                string jsonString = File.ReadAllText(fileName);
                ItemInstanceSaveData data = JsonUtility.FromJson<ItemInstanceSaveData>(jsonString);

                if (data != null) {
                    itemInstanceListResponse.Add(data);
                }

                count++;
                // 3. Report progress back to the UI
                progress?.Report(count);
            }

            return itemInstanceListResponse;
        }

        public async void SaveItemInstanceDataFileAsync(InstantiatedItem instantiatedItem) {
            // 1. Mark the save task as active on the Main Thread
            System.Threading.Interlocked.Increment(ref activeSaveTasks);

            try {
                // Snapshot the data on the Main Thread (Snapshotting)
                ItemInstanceSaveData itemInstanceSaveData = instantiatedItem.GetItemSaveData();
                string jsonString = JsonUtility.ToJson(itemInstanceSaveData);

                // Capture the save path
                string folderPath = itemInstanceSaveFolderName;
                string fileName = $"{instantiatedItem.InstanceId}.json";
                string fullPath = Path.Combine(folderPath, fileName);

                // 2. Offload the directory creation and disk write
                await Task.Run(() => {
                    try {
                        if (!Directory.Exists(folderPath)) {
                            Directory.CreateDirectory(folderPath);
                        }
                        File.WriteAllText(fullPath, jsonString);
                    } catch (System.Exception ex) {
                        Debug.LogError($"[Item Save Error] Instance {instantiatedItem.InstanceId}: {ex.Message}");
                    }
                });
            } finally {
                // 3. ALWAYS decrement the counter when finished
                System.Threading.Interlocked.Decrement(ref activeSaveTasks);
            }
        }

        public void DeleteItemInstance(InstantiatedItem instantiatedItem) {
            //Debug.Log($"AuctionService.DeleteAuctionItem({auctionItem.AuctionItemId})");

            string folderPath = itemInstanceSaveFolderName;
            string fileName = $"{instantiatedItem.InstanceId}.json";
            string fullPath = Path.Combine(folderPath, fileName);

            if (File.Exists(fullPath)) {
                File.Delete(fullPath);
            }
        }


        // ****************************
        // FRIENDS
        // ****************************

        public async void SaveFriendListAsync(FriendList friendList) {
            // 1. Mark the task as active on the Main Thread
            System.Threading.Interlocked.Increment(ref activeSaveTasks);

            try {
                // Prepare data and paths on the Main Thread (Snapshotting)
                FriendListSaveData friendListSaveData = new FriendListSaveData(friendList);
                string jsonString = JsonUtility.ToJson(friendListSaveData, true);

                // Capture the save path
                string folderPath = friendSaveFolderName;
                string fileName = $"{friendListSaveData.PlayerCharacterId}.json";
                string fullPath = Path.Combine(folderPath, fileName);

                // 2. Offload the folder check and file writing to a background thread
                await Task.Run(() => {
                    try {
                        // Ensure the directory exists before writing
                        if (!Directory.Exists(folderPath)) {
                            Directory.CreateDirectory(folderPath);
                        }

                        File.WriteAllText(fullPath, jsonString);
                    } catch (System.Exception ex) {
                        // Safely log the error to the Unity console
                        Debug.LogError($"[Friend List Save Error] Character ID {friendListSaveData.PlayerCharacterId}: {ex.Message}");
                    }
                });
            } finally {
                // 3. ALWAYS decrement the counter when finished
                System.Threading.Interlocked.Decrement(ref activeSaveTasks);
            }
        }


        public async void LoadAllFriendsAsync() {
            // 1. Setup the reporter for the status window
            var progressReporter = new Progress<int>(count => {
                serverDataService.NotifyOnLoadFriend(count);
            });

            // Capture the folder path on the Main Thread
            string folderPath = friendSaveFolderName;

            // 2. Offload the disk scanning and JSON parsing
            List<FriendListSaveData> results = await Task.Run(() => PerformFriendFileIOWork(folderPath, progressReporter));

            // 3. Back on Main Thread: Hand the data to the service
            friendServiceServer.ProcessLoadAllFriendLists(results);
        }

        private List<FriendListSaveData> PerformFriendFileIOWork(string folderPath, IProgress<int> progress) {
            List<FriendListSaveData> friendListSaveDatas = new List<FriendListSaveData>();

            if (!Directory.Exists(folderPath)) return friendListSaveDatas;

            string[] fileEntries = Directory.GetFiles(folderPath, "*.json");
            int count = 0;

            foreach (string fileName in fileEntries) {
                try {
                    string jsonString = File.ReadAllText(fileName);
                    FriendListSaveData data = JsonUtility.FromJson<FriendListSaveData>(jsonString);

                    if (data != null) {
                        friendListSaveDatas.Add(data);

                        // Increment and report progress to the UI
                        count++;
                        progress?.Report(count);
                    }
                } catch (System.Exception) {
                    // Skip corrupted files to keep the boot process moving
                }
            }

            return friendListSaveDatas;
        }


    }

}