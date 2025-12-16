using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AnyRPG {
    public class SystemItemManager : ConfiguredClass {

        private int clientItemIdCount = 1;
        private int serverItemIdCount = -1;
        private string saveFolderName = string.Empty;

        // game manager references
        private LootManager lootManager = null;

        private Dictionary<int, InstantiatedItem> instantiatedItems = new Dictionary<int, InstantiatedItem>();

        public Dictionary<int, InstantiatedItem> InstantiatedItems { get => instantiatedItems; set => instantiatedItems = value; }
        public int ClientItemIdCount { get => clientItemIdCount; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            MakeSaveFolder();
            networkManagerServer.OnStartServer += HandleStartServer;
            networkManagerServer.OnStopServer += HandleStopServer;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            lootManager = systemGameManager.LootManager;
        }

        private void HandleStartServer() {
            //Debug.Log("SystemItemManager.HandleStartServer()");

            LoadAllItems();
        }

        private void HandleStopServer() {
            //Debug.Log("SystemItemManager.HandleStopServer()");

            ClearInstantiatedItems();
        }

        private void LoadAllItems() {
            //Debug.Log("SystemItemManager.LoadAllItems()");

            // load all user accounts from storage
            string[] fileEntries = Directory.GetFiles(saveFolderName, "*.json");
            foreach (string fileName in fileEntries) {
                //Debug.Log($"Loading user account from file: {fileName}");
                string jsonString = File.ReadAllText(fileName);
                ItemInstanceSaveData itemInstanceSaveData = JsonUtility.FromJson<ItemInstanceSaveData>(jsonString);
                if (itemInstanceSaveData.ItemInstanceId == 0) {
                    Debug.LogWarning($"SystemItemManager.LoadAllItems(): item in file {fileName} has invalid instance id of 0.  This item will be skipped.");
                    continue;
                }
                if (instantiatedItems.ContainsKey(itemInstanceSaveData.ItemInstanceId) == true) {
                    Debug.LogWarning($"SystemItemManager.LoadAllItems(): Duplicate item id {itemInstanceSaveData.ItemInstanceId} found in file {fileName}.  This item will be skipped.");
                    continue;
                }
                LoadItemInstanceSaveData(itemInstanceSaveData);
            }
        }

        private void LoadItemInstanceSaveData(ItemInstanceSaveData itemInstanceSaveData) {
            //Debug.Log($"SystemItemManager.LoadItemInstanceSaveData(id: {itemInstanceSaveData.ItemInstanceId}, name: {itemInstanceSaveData.ItemName})");

            if (instantiatedItems.ContainsKey(itemInstanceSaveData.ItemInstanceId)) {
                return;
            }

            ItemQuality usedItemQuality = null;
            if (itemInstanceSaveData.ItemQuality != null && itemInstanceSaveData.ItemQuality != string.Empty) {
                usedItemQuality = systemDataFactory.GetResource<ItemQuality>(itemInstanceSaveData.ItemQuality);
            }
            Item item = systemDataFactory.GetResource<Item>(itemInstanceSaveData.ItemName);
            if (item == null) {
                Debug.LogWarning($"SystemItemManager.LoadItemInstanceSaveData(): item {itemInstanceSaveData.ItemName} not found.  This item will be skipped.");
                return;
            }
            InstantiatedItem instantiatedItem = GetNewInstantiatedItem(itemInstanceSaveData.ItemInstanceId, item, usedItemQuality);
            //instantiatedItem.InitializeNewItem(usedItemQuality);
            instantiatedItem.LoadSaveData(itemInstanceSaveData);
        }


        private void MakeSaveFolder() {
            //Debug.Log("SystemItemManager.MakeSaveFolder()");

            Regex regex = new Regex("[^a-zA-Z0-9]");
            string gameNameString = regex.Replace(systemConfigurationManager.GameName, "");
            if (gameNameString == string.Empty) {
                return;
            }
            saveFolderName = $"{Application.persistentDataPath}/{gameNameString}/Online/Items";
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

        public InstantiatedItem GetNewInstantiatedItem(string itemName, ItemQuality usedItemQuality = null) {
            //Debug.Log(this.GetType().Name + ".GetNewResource(" + resourceName + ")");
            Item item = systemDataFactory.GetResource<Item>(itemName);
            if (item == null) {
                return null;
            }
            return GetNewInstantiatedItem(item, usedItemQuality);
        }

        public InstantiatedItem GetNewInstantiatedItem(Item item) {
            //Debug.Log($"SystemItemManager.GetNewInstantiatedItem({item.ResourceName})");

            return GetNewInstantiatedItem(item, null);
        }

        public InstantiatedItem GetNewInstantiatedItem(Item item, ItemQuality usedItemQuality) {
            //Debug.Log($"SystemItemManager.GetNewInstantiatedItem({item.ResourceName})");

            InstantiatedItem instantiatedItem = GetNewInstantiatedItem(GetNewItemInstanceId(), item, usedItemQuality);
            if (networkManagerServer.ServerModeActive == true && item != lootManager.CurrencyLootItem) {
                SaveDataFile(instantiatedItem);
            }
            return instantiatedItem;
        }

        public int GetNewItemInstanceId() {
            //Debug.Log($"SystemItemManager.GetNewItemInstanceId()");

            if (networkManagerServer.ServerModeActive == true ) {
                return GetNewServerItemInstanceId();
            } else {
                return GetNewClientItemInstanceId();
            }
        }

        public int GetNewServerItemInstanceId() {
            //Debug.Log($"SystemItemManager.GetNewServerItemInstanceId()");

            // ensure unique item id returned even if count is off
            int returnValue = serverItemIdCount;
            while (instantiatedItems.ContainsKey(serverItemIdCount)) {
                serverItemIdCount--;
                returnValue = serverItemIdCount;
            }
            serverItemIdCount--;
            serverStateService.SetItemIdCounter(serverItemIdCount);

            //Debug.Log($"SystemItemManager.GetNewServerItemInstanceId() return {returnValue}");
            return returnValue;
        }

        public int GetNewClientItemInstanceId() {
            //Debug.Log($"SystemItemManager.GetNewClientItemInstanceId()");

            // ensure unique item id returned even if count is off
            int returnValue = clientItemIdCount;
            while (instantiatedItems.ContainsKey(clientItemIdCount)) {
                clientItemIdCount++;
                returnValue = clientItemIdCount;
            }
            clientItemIdCount++;

            //Debug.Log($"SystemItemManager.GetNewClientItemInstanceId() return {returnValue}");
            return returnValue;
        }

        public InstantiatedItem GetNewInstantiatedItem(int itemInstanceId, Item item, ItemQuality usedItemQuality = null) {
            //Debug.Log($"SystemItemManager.GetNewInstantiatedItem({itemInstanceId}, {item?.ResourceName}, {usedItemQuality?.ResourceName})");
            if (instantiatedItems.ContainsKey(itemInstanceId)) {
                return instantiatedItems[itemInstanceId];
            }
            InstantiatedItem instantiatedItem = item.GetNewInstantiatedItem(systemGameManager, itemInstanceId, item, usedItemQuality);
            instantiatedItem.InitializeNewItem(usedItemQuality);
            instantiatedItems.Add(itemInstanceId, instantiatedItem);
            return instantiatedItem;
        }

        public void SetClientItemIdCount(int clientItemIdCount) {
            //Debug.Log($"SystemItemManager.SetClientItemIdCount({clientItemIdCount})");

            this.clientItemIdCount = clientItemIdCount;
        }

        public void ClearInstantiatedItems() {
            //Debug.Log($"SystemItemManager.ClearInstantiatedItems()");

            instantiatedItems.Clear();
        }

        public void ClientReset() {
            //Debug.Log($"SystemItemManager.ClientReset()");

            clientItemIdCount = 1;
            ClearInstantiatedItems();
        }

        public InstantiatedItem GetExistingInstantiatedItem(int itemInstanceId) {
            if (itemInstanceId == 0) {
                return null;
            }
            
            if (instantiatedItems.ContainsKey(itemInstanceId)) {
                return instantiatedItems[itemInstanceId];
            }
            return null;
        }

        public void LoadItemIdCounter(int itemInstanceIdCounter) {
            //Debug.Log($"SystemItemManager.LoadItemIdCounter({itemInstanceIdCounter})");

            serverItemIdCount = itemInstanceIdCounter;
        }

        public bool SaveDataFile(InstantiatedItem instantiatedItem) {
            //Debug.Log($"SystemItemManager.SaveDataFile({instantiatedItem.Item.ResourceName})");

            ItemInstanceSaveData itemInstanceSaveData = instantiatedItem.GetItemSaveData();
            string jsonString = JsonUtility.ToJson(itemInstanceSaveData);
            string jsonSavePath = $"{saveFolderName}/{instantiatedItem.InstanceId}.json";
            File.WriteAllText(jsonSavePath, jsonString);

            return true;
        }

        public void LoadPlayerCharacterSaveData(PlayerCharacterSaveData playerCharacterSaveData) {
            //Debug.Log($"SystemItemManager.LoadPlayerCharacterSaveData(characterId: {playerCharacterSaveData.CharacterSaveData.CharacterId})");

            SetClientItemIdCount(playerCharacterSaveData.CharacterSaveData.ClientItemIdCount);
            LoadItemInstanceListSaveData(playerCharacterSaveData.ItemInstanceListSaveData);
        }

        public void LoadItemInstanceListSaveData(ItemInstanceListSaveData itemInstanceListSaveData) {
            //Debug.Log($"SystemItemManager.LoadItemInstanceListSaveData(count: {itemInstanceListSaveData.ItemInstances.Count})");

            foreach (ItemInstanceSaveData itemInstanceSaveData in itemInstanceListSaveData.ItemInstances) {
                LoadItemInstanceSaveData(itemInstanceSaveData);
            }
        }

        /*
        public InstantiatedItem GetNewInstantiatedItem(Item item, ItemQuality usedItemQuality = null) {
            //Debug.Log(this.GetType().Name + ".GetNewResource(" + resourceName + ")");
            InstantiatedItem instantiatedItem = new InstantiatedItem(systemGameManager, itemIdCount, item, usedItemQuality);
            instantiatedItem.InitializeNewItem(usedItemQuality);
            return instantiatedItem;
        }
        */

    }

}