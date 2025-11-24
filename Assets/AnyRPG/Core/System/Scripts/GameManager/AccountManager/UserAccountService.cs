using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AnyRPG {
    public class UserAccountService : ConfiguredClass {

        /// <summary>
        /// username, userAccount
        /// </summary>
        private Dictionary<string, UserAccount> userAccounts = new Dictionary<string, UserAccount>();

        private int accountIdCounter = 1;
        private string saveFolderName = string.Empty;
        private const string accountIdCounterKey = "Server.AccountIdCounter";

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            LoadAccountIdCounter();
            MakeSaveFolder();
            networkManagerServer.OnStartServer += HandleStartServer;
            networkManagerServer.OnStopServer += HandleStopServer;
        }

        private void HandleStartServer() {
            //Debug.Log("UserAccountService.HandleStartServer()");

            LoadAllUserAccounts();
        }

        private void HandleStopServer() {
            //Debug.Log("UserAccountService.HandleStopServer()");

            ClearAllUserAccounts();
        }

        private void ClearAllUserAccounts() {
            //Debug.Log("UserAccountService.ClearAllUserAccounts()");

            userAccounts.Clear();
        }

        private void LoadAccountIdCounter() {
            //Debug.Log("UserAccountService.LoadAccountIdCounter()");

            accountIdCounter = PlayerPrefs.GetInt(accountIdCounterKey, 1);
            //Debug.Log($"UserAccountService.LoadAccountIdCounter(): {accountIdCounter}");
        }

        private void MakeSaveFolder() {
            //Debug.Log("UserAccountService.MakeSaveFolder()");

            Regex regex = new Regex("[^a-zA-Z0-9]");
            string gameNameString = regex.Replace(systemConfigurationManager.GameName, "");
            if (gameNameString == string.Empty) {
                return;
            }
            saveFolderName = $"{Application.persistentDataPath}/{gameNameString}/Online/UserAccounts";
            if (!Directory.Exists($"{Application.persistentDataPath}/{gameNameString}")) {
                Directory.CreateDirectory($"{Application.persistentDataPath}/{gameNameString}");
            }
            if (!Directory.Exists($"{Application.persistentDataPath}/{gameNameString}/Online")) {
                Directory.CreateDirectory($"{Application.persistentDataPath}/{gameNameString}/Online");
            }
            if (!Directory.Exists(saveFolderName)) {
                Directory.CreateDirectory(saveFolderName);
            }
        }

        private void LoadAllUserAccounts() {
            //Debug.Log("UserAccountService.LoadAllUserAccounts()");

            // load all user accounts from storage
            string[] fileEntries = Directory.GetFiles(saveFolderName, "*.json");
            foreach (string fileName in fileEntries) {
                //Debug.Log($"Loading user account from file: {fileName}");
                string jsonString = File.ReadAllText(fileName);
                UserAccount userAccount = JsonUtility.FromJson<UserAccount>(jsonString);
                if (userAccount.Id == 0) {
                    Debug.LogWarning($"UserAccountService.LoadAllUserAccounts(): User account in file {fileName} has invalid id of 0.  This account will be skipped.");
                    continue;
                }
                //Debug.Log($"Loaded user account: {userAccount.UserName}");
                if (userAccounts.ContainsKey(userAccount.UserName)) {
                    Debug.LogWarning($"UserAccountService.LoadAllUserAccounts(): Duplicate user account name {userAccount.UserName} found in file {fileName}.  This account will be skipped.");
                    continue;
                }
                //Debug.Log($"UserAccountService.LoadAllUserAccounts(): Adding user account {userAccount.UserName} with id {userAccount.Id} to in memory lookup.");
                userAccounts.Add(userAccount.UserName, userAccount);
            }
        }

        /// <summary>
        /// check if username exists
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public bool AccountExists(string userName) {
            //Debug.Log($"UserAccountService.AccountExists({userName})");
            
            return userAccounts.ContainsKey(userName);
        }

        /// <summary>
        /// get user account by username
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public UserAccount GetUserAccount(string userName) {
            //Debug.Log($"UserAccountService.GetUserAccount({userName})");

            if (userAccounts.ContainsKey(userName)) {
                //Debug.Log($"UserAccountService.GetUserAccount({userName}) return id: {userAccounts[userName].Id}");
                return userAccounts[userName];
            }

            return null;
        }

        /// <summary>
        /// add new user account to local storage
        /// </summary>
        /// <param name="userAccount"></param>
        private void SaveNewAccountLocal(UserAccount userAccount) {
            Debug.Log($"UserAccountService.SaveNewAccountLocal({userAccount.UserName})");

            SaveDataFile(userAccount);
        }
        
        public UserAccount CreateNewAccount(string username, string password) {
            Debug.Log($"UserAccountService.CreateNewAccount({username}, {password})");

            // check if username is not taken
            if (AccountExists(username)) {
                return null;
            }

            // create new account
            UserAccount userAccount = new UserAccount() {
                Id = GetNewAccountId(),
                UserName = username,
            };

            // populate salt and hash values
            AuthenticationHelpers.ProvideSaltAndHash(userAccount);

            // add user account to storage
            SaveNewAccountLocal(userAccount);

            // add to in memory lookup
            userAccounts.Add(username, userAccount);

            return userAccount;
        }

        private int GetNewAccountId() {
            //Debug.Log("UserAccountService.GetNewAccountId()");

            int returnValue = accountIdCounter;
            // loop until accountIdCounter is found that is not in use, using the in memory dictionary for speed
            while (true) {
                bool idInUse = false;
                foreach (KeyValuePair<string, UserAccount> entry in userAccounts) {
                    if (entry.Value.Id == accountIdCounter) {
                        Debug.LogWarning($"UserAccountService.GetNewAccountId(): id {accountIdCounter} is already in use, trying next");
                        idInUse = true;
                        break;
                    }
                }
                if (!idInUse) {
                    break;
                }
                accountIdCounter++;
            }
            returnValue = accountIdCounter;
            accountIdCounter++;
            PlayerPrefs.SetInt(accountIdCounterKey, accountIdCounter);

            Debug.Log($"UserAccountService.GetNewAccountId() return {returnValue}");
            return returnValue;
        }

        public bool SaveDataFile(UserAccount userAccount) {
            Debug.Log($"UserAccountService.SaveDataFile({userAccount.UserName}, {userAccount.Id})");

            string jsonString = JsonUtility.ToJson(userAccount);
            string jsonSavePath = $"{saveFolderName}/{userAccount.Id}.json";
            File.WriteAllText(jsonSavePath, jsonString);

            return true;
        }

    }

}
