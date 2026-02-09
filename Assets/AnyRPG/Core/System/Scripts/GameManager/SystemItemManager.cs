using IdGen;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemItemManager : ConfiguredClass {

        IdGenerator clientIdGenerator = null;
        IdGenerator serverIdGenerator = null;

        // game manager references
        private LootManager lootManager = null;
        private ServerDataService serverDataService = null;

        private Dictionary<long, InstantiatedItem> instantiatedItems = new Dictionary<long, InstantiatedItem>();

        public Dictionary<long, InstantiatedItem> InstantiatedItems { get => instantiatedItems; set => instantiatedItems = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            //networkManagerServer.OnStartServer += HandleStartServer;
            networkManagerServer.OnStopServer += HandleStopServer;

            SetupIDGenerator();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            lootManager = systemGameManager.LootManager;
            serverDataService = systemGameManager.ServerDataService;
        }

        private void SetupIDGenerator() {
            //Debug.Log("SystemItemManager.SetupIDGenerator()");

            var epoch = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            IdGeneratorOptions idGeneratorOptions = new IdGeneratorOptions(IdStructure.Default, new DefaultTimeSource(epoch));
            clientIdGenerator = new IdGenerator(0, idGeneratorOptions);
            serverIdGenerator = new IdGenerator(1, idGeneratorOptions);
        }

        private void HandleStopServer() {
            //Debug.Log("SystemItemManager.HandleStopServer()");

            ClearInstantiatedItems();
        }

        public void LoadAllItems() {
            //Debug.Log("SystemItemManager.LoadAllItems()");

            // this is only called in network mode.  In offline mode, items are loaded as part of the player data load
            serverDataService.LoadAllItems();
        }

        public void ProcessLoadAllItemInstances(List<ItemInstanceSerializedData> itemInstances) {

            List<ItemInstanceSaveData> itemInstanceSaveDataList = new List<ItemInstanceSaveData>();
            foreach (ItemInstanceSerializedData itemInstanceSerializedData in itemInstances) {
                ItemInstanceSaveData itemInstanceSaveData = JsonUtility.FromJson<ItemInstanceSaveData>(itemInstanceSerializedData.saveData);
                if (itemInstanceSaveData == null) {
                    Debug.LogWarning($"SystemItemManager.ProcessLoadAllItemInstances(): ItemInstanceSaveData is null for id {itemInstanceSerializedData.id}.  This item will be skipped.");
                    continue;
                }
                itemInstanceSaveDataList.Add(itemInstanceSaveData);
            }

            ProcessLoadAllItemInstances(itemInstanceSaveDataList);
        }

        public void ProcessLoadAllItemInstances(List<ItemInstanceSaveData> itemInstances) {

            foreach (ItemInstanceSaveData itemInstanceSaveData in itemInstances) {
                //Debug.Log($"Loading user account from file: {fileName}");
                if (itemInstanceSaveData.ItemInstanceId == -1) {
                    Debug.LogWarning($"SystemItemManager.LoadAllItems(): item has invalid instance id of -1.  This item will be skipped.");
                    continue;
                }
                if (instantiatedItems.ContainsKey(itemInstanceSaveData.ItemInstanceId) == true) {
                    Debug.LogWarning($"SystemItemManager.LoadAllItems(): Duplicate item id {itemInstanceSaveData.ItemInstanceId} found.  This item will be skipped.");
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

        public InstantiatedItem GetNewInstantiatedItem(Item item) {
            //Debug.Log($"SystemItemManager.GetNewInstantiatedItem({item.ResourceName})");

            return GetNewInstantiatedItem(item, null);
        }

        public InstantiatedItem GetNewInstantiatedItem(string itemName, ItemQuality usedItemQuality = null) {
            //Debug.Log(this.GetType().Name + ".GetNewResource(" + resourceName + ")");
            Item item = systemDataFactory.GetResource<Item>(itemName);
            if (item == null) {
                return null;
            }
            return GetNewInstantiatedItem(item, usedItemQuality);
        }

        public InstantiatedItem GetNewInstantiatedItem(Item item, ItemQuality usedItemQuality) {
            //Debug.Log($"SystemItemManager.GetNewInstantiatedItem({item.ResourceName})");

            InstantiatedItem instantiatedItem = GetNewInstantiatedItem(GetNewItemInstanceId(), item, usedItemQuality);
            if (networkManagerServer.ServerModeActive == true && item != lootManager.CurrencyLootItem) {
                serverDataService.CreateItemInstance(instantiatedItem);
            }
            return instantiatedItem;
        }

        public InstantiatedItem GetNewInstantiatedItem(long itemInstanceId, Item item, ItemQuality usedItemQuality = null) {
            //Debug.Log($"SystemItemManager.GetNewInstantiatedItem({itemInstanceId}, {item?.ResourceName}, {usedItemQuality?.ResourceName})");
            if (instantiatedItems.ContainsKey(itemInstanceId)) {
                return instantiatedItems[itemInstanceId];
            }
            InstantiatedItem instantiatedItem = item.GetNewInstantiatedItem(systemGameManager, itemInstanceId, item, usedItemQuality);
            instantiatedItem.InitializeNewItem(usedItemQuality);
            instantiatedItems.Add(itemInstanceId, instantiatedItem);
            return instantiatedItem;
        }


        public void SaveItemInstance(InstantiatedItem instantiatedItem) {
            serverDataService.SaveItemInstance(instantiatedItem);
        }

        public void CreateItemInstance(InstantiatedItem instantiatedItem) {
            serverDataService.CreateItemInstance(instantiatedItem);
        }

        public long GetNewItemInstanceId() {
            //Debug.Log($"SystemItemManager.GetNewItemInstanceId()");

            if (networkManagerServer.ServerModeActive == true ) {
                return GetNewServerItemInstanceId();
            } else {
                return GetNewClientItemInstanceId();
            }
        }

        public long GetNewClientItemInstanceId() {
            //Debug.Log($"SystemItemManager.GetNewClientItemInstanceId()");

            return clientIdGenerator.CreateId();
        }

        public void ClearInstantiatedItems() {
            //Debug.Log($"SystemItemManager.ClearInstantiatedItems()");

            instantiatedItems.Clear();
        }

        public void ClientReset() {
            //Debug.Log($"SystemItemManager.ClientReset()");

            ClearInstantiatedItems();
        }

        public InstantiatedItem GetExistingInstantiatedItem(long itemInstanceId) {
            //Debug.Log($"SystemItemManager.GetExistingInstantiatedItem(itemInstanceId: {itemInstanceId})");

            if (itemInstanceId == -1) {
                return null;
            }
            
            if (instantiatedItems.ContainsKey(itemInstanceId)) {
                return instantiatedItems[itemInstanceId];
            }
            return null;
        }

        public void LoadPlayerCharacterSaveData(PlayerCharacterSaveData playerCharacterSaveData) {
            //Debug.Log($"SystemItemManager.LoadPlayerCharacterSaveData(characterId: {playerCharacterSaveData.CharacterSaveData.CharacterId})");

            LoadItemInstanceListSaveData(playerCharacterSaveData.ItemInstanceListSaveData);
        }

        public void LoadItemInstanceListSaveData(ItemInstanceListSaveData itemInstanceListSaveData) {
            //Debug.Log($"SystemItemManager.LoadItemInstanceListSaveData(count: {itemInstanceListSaveData.ItemInstances.Count})");

            foreach (ItemInstanceSaveData itemInstanceSaveData in itemInstanceListSaveData.ItemInstances) {
                LoadItemInstanceSaveData(itemInstanceSaveData);
            }
        }

        public long GetNewServerItemInstanceId() {
            //Debug.Log($"SystemItemManager.GetNewServerItemInstanceId()");

            return serverIdGenerator.CreateId();
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