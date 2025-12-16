using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AnyRPG { 
    public class ServerStateService : ConfiguredClass {

        private ServerStateSaveData serverStateSaveData = new ServerStateSaveData();
        private string baseSaveFolderName = string.Empty;
        private const string saveFileName = "ServerState.json";
        private Coroutine monitorCoroutine = null;
        private int saveInterval = 10;
        private bool saveDataDirty = false;

        // game manager references
        private MailService mailService = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            MakeBaseSaveFolder();
            networkManagerServer.OnStartServer += HandleStartServer;
            networkManagerServer.OnStopServer += HandleStopServer;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            mailService = systemGameManager.MailService;
        }

        private void HandleStopServer() {
            SaveDataFile();
            EndMonitoringServerState();
        }

        private void EndMonitoringServerState() {
            if (monitorCoroutine != null) {
                systemGameManager.StopCoroutine(monitorCoroutine);
            }
            SaveDataFile();
        }

        private void HandleStartServer() {
            //Debug.Log($"ServerStateService.HandleStartServer()");

            LoadDataFile();
            userAccountService.LoadAccountIdCounter(serverStateSaveData.accountIdCounter);
            playerCharacterService.LoadPlayerCharacterIdCounter(serverStateSaveData.playerCharacterIdCounter);
            systemItemManager.LoadItemIdCounter(serverStateSaveData.itemInstanceIdCounter);
            mailService.LoadMailIdCounter(serverStateSaveData.mailIdCounter);
            BeginMonitoringServerState();
        }

        private void BeginMonitoringServerState() {
            if (monitorCoroutine == null) {
                monitorCoroutine = systemGameManager.StartCoroutine(MonitorServerState());
            }
        }

        public IEnumerator MonitorServerState() {
            //Debug.Log($"ServerStateService.MonitorServerState()");

            while (systemGameManager.GameMode == GameMode.Network) {
                if (saveDataDirty == true) {
                    SaveDataFile();
                }
                yield return new WaitForSeconds(saveInterval);
            }
        }

        public void SetAccountIdCounter(int newCounter) {
            serverStateSaveData.accountIdCounter = newCounter;
            saveDataDirty = true;
        }

        public void SetPlayerCharacterIdCounter(int playerCharacterIdCounter) {
            serverStateSaveData.playerCharacterIdCounter = playerCharacterIdCounter;
            saveDataDirty = true;
        }

        public void SetMailIdCounter(int mailIdCounter) {
            serverStateSaveData.mailIdCounter = mailIdCounter;
            saveDataDirty = true;
        }

        public void SetItemIdCounter(int serverItemIdCount) {
            serverStateSaveData.itemInstanceIdCounter = serverItemIdCount;
            saveDataDirty = true;
        }

        private void MakeBaseSaveFolder() {
            //Debug.Log("PlayerCharacterService.MakeSaveFolder()");

            Regex regex = new Regex("[^a-zA-Z0-9]");
            string gameNameString = regex.Replace(systemConfigurationManager.GameName, "");
            if (gameNameString == string.Empty) {
                return;
            }
            baseSaveFolderName = $"{Application.persistentDataPath}/{gameNameString}/Online/Server";
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

        public void SaveDataFile() {
            //Debug.Log($"ServerStateService.SaveDataFile()");

            string jsonString = JsonUtility.ToJson(serverStateSaveData);
            string jsonSavePath = $"{baseSaveFolderName}/{saveFileName}";
            File.WriteAllText(jsonSavePath, jsonString);

            saveDataDirty = false;
        }

        public void LoadDataFile() {
            //Debug.Log($"ServerStateService.LoadDataFile()");

            string jsonSavePath = $"{baseSaveFolderName}/{saveFileName}";
            if (File.Exists(jsonSavePath)) {
                string jsonString = File.ReadAllText(jsonSavePath);
                serverStateSaveData = JsonUtility.FromJson<ServerStateSaveData>(jsonString);
            }
        }

    }

}