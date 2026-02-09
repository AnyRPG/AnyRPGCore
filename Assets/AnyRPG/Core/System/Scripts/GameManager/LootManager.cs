using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class LootManager : ConfiguredClass {

        public event System.Action OnTakeLoot = delegate { };
        public event Action OnAvailableLootAdded = delegate { };

        /// <summary>
        /// accountId, LootDrop
        /// a list that is reset every time the loot window opens or closes to give the proper list depending on what was looted
        /// </summary>
        private Dictionary<int, List<LootDrop>> availableDroppedLoot = new Dictionary<int, List<LootDrop>>();

        // this list is solely for the purpose of tracking dropped loot to ensure that unique items cannot be dropped twice
        // if one drops and is left on a body unlooted and another enemy is killed
        private List<LootTableState> lootTableStates = new List<LootTableState>();
        private Dictionary<int, LootTableState> lootTableStateDict = new Dictionary<int, LootTableState>();

        private CurrencyItem currencyLootItem = null;

        // a counter to ensure that loot drop ids are unique
        private int lootDropIdCounter = 0;

        // a dictionary of all dropped loot
        private Dictionary<int, LootDrop> lootDropIndex = new Dictionary<int, LootDrop>();

        // game manager references
        private MessageFeedManager messageFeedManager = null;
        private PlayerManager playerManager = null;
        private PlayerManagerServer playerManagerServer = null;
        private SystemEventManager systemEventManager = null;

        public Dictionary<int, List<LootDrop>> AvailableDroppedLoot { get => availableDroppedLoot; }
        public CurrencyItem CurrencyLootItem { get => currencyLootItem; }
        public Dictionary<int, LootDrop> LootDropIndex { get => lootDropIndex; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            currencyLootItem = ScriptableObject.CreateInstance<CurrencyItem>();
            currencyLootItem.ResourceName = "System Currency Loot Item";
            if (systemConfigurationManager.KillCurrency != null) {
                currencyLootItem.Icon = systemConfigurationManager.KillCurrency.Icon;
            }
            // this is normally done by the resource manager when loading from resources, but since we are creating this manually we need to do it ourselves
            currencyLootItem.SetupScriptableObjects(systemGameManager);
            systemDataFactory.AddResource<Item>(currencyLootItem);
            // pre populate client id 0 so this works before loot is dropped
            availableDroppedLoot.Add(0, new List<LootDrop>());
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            messageFeedManager = systemGameManager.UIManager.MessageFeedManager;
            playerManager = systemGameManager.PlayerManager;
            playerManagerServer = systemGameManager.PlayerManagerServer;
            systemEventManager = systemGameManager.SystemEventManager;
        }

        public void AddAvailableLoot(UnitController sourceUnitController, List<LootDrop> items) {
            //Debug.Log($"LootManager.AddAvailableLoot({sourceUnitController.gameObject.name}, count: {items.Count})");

            if (playerManagerServer.ActiveUnitControllerLookup.ContainsKey(sourceUnitController)) {
                AddAvailableLoot(playerManagerServer.ActiveUnitControllerLookup[sourceUnitController], items);
            }
        }

        public void AddAvailableLoot(int accountId, List<int> lootDropIds) {
            //Debug.Log($"LootManager.AddAvailableLoot({accountId}, count: {lootDropIds.Count})");

            List<LootDrop> lootDrops = new List<LootDrop>();
            foreach (int lootDropId in lootDropIds) {
                if (lootDropIndex.ContainsKey(lootDropId)) {
                    lootDrops.Add(lootDropIndex[lootDropId]);
                }
            }
            AddAvailableLoot(accountId, lootDrops);
        }

        public void AddAvailableLoot(int accountId, List<LootDrop> items) {
            //Debug.Log($"LootManager.AddAvailableLoot({accountId}, count: {items.Count})");

            if (availableDroppedLoot.ContainsKey(accountId)) {
                availableDroppedLoot[accountId] = items;
            } else {
                availableDroppedLoot.Add(accountId, items);
            }
            
            // copy this data to the client
            if (networkManagerServer.ServerModeActive == true) {
                List<int> lootDropIds = new List<int>();
                foreach (LootDrop item in items) {
                    lootDropIds.Add(item.LootDropId);
                }
                networkManagerServer.AddAvailableDroppedLoot(accountId, lootDropIds);
            } else {
                OnAvailableLootAdded();
            }
        }

        public void ClearAvailableDroppedLoot() {
            //Debug.Log("LootManager.ClearDroppedLoot()");

            availableDroppedLoot[0].Clear();
        }

        public void RequestTakeLoot(LootDrop lootDrop, UnitController sourceUnitController) {
            //Debug.Log($"LootManager.RequestTakeLoot({lootDrop.LootDropId}, {sourceUnitController.gameObject.name})");

            if (systemGameManager.GameMode == GameMode.Local) {
                lootDrop.TakeLoot(playerManager.UnitController);
            } else {
                networkManagerClient.RequestTakeLoot(lootDrop.LootDropId);
            }
        }

        public void TakeLoot(UnitController sourceUnitController, LootDrop lootDrop) {
            if (playerManagerServer.ActiveUnitControllerLookup.ContainsKey(sourceUnitController)) {
                TakeLoot(playerManagerServer.ActiveUnitControllerLookup[sourceUnitController], lootDrop);
            }
        }

        public void TakeLoot(int accountId, int lootDropId) {
            //Debug.Log($"LootManager.TakeLoot({accountId}, {lootDropId})");

            if (lootDropIndex.ContainsKey(lootDropId) == false) {
                return;
            }
            LootDrop lootDrop = lootDropIndex[lootDropId];
            TakeLoot(accountId, lootDrop);
        }

        public void TakeLoot(int accountId, LootDrop lootDrop) {
            //Debug.Log($"LootManager.TakeLoot({accountId}, {lootDrop.DisplayName}) lootDropId: {lootDrop.LootDropId})");

            RemoveLootTableStateIndex(lootDrop.LootDropId);
            RemoveFromAvailableDroppedItems(accountId, lootDrop);

            if (networkManagerServer.ServerModeActive == true) {
                networkManagerServer.AdvertiseTakeLoot(accountId, lootDrop.LootDropId);
            }

            systemEventManager.NotifyOnTakeLoot(accountId);
            OnTakeLoot();
        }

        public void RemoveFromAvailableDroppedItems(int accountId, LootDrop lootDrop) {
            //Debug.Log("LootManager.RemoveFromDroppedItems()");

            if (availableDroppedLoot.ContainsKey(accountId) && availableDroppedLoot[accountId].Contains(lootDrop)) {
                availableDroppedLoot[accountId].Remove(lootDrop);
            }
        }

        public void TakeAllLoot(UnitController sourceUnitController) {
            //Debug.Log($"LootManager.TakeAllLoot({sourceUnitController.gameObject.name})");

            if (systemGameManager.GameMode == GameMode.Local) {
                TakeAllLootInternal(0, sourceUnitController);
            } else {
                networkManagerClient.TakeAllLoot();
            }
        }

        public void TakeAllLootInternal(int accountId, UnitController sourceUnitController) {
            //Debug.Log("LootManager.TakeAllLoot()");

            // added emptyslotcount to prevent game from freezup when no bag space left and takeall button pressed
            int maximumLoopCount = availableDroppedLoot[accountId].Count;
            int currentLoopCount = 0;
            while (availableDroppedLoot[accountId].Count > 0 && sourceUnitController.CharacterInventoryManager.EmptySlotCount() > 0 && currentLoopCount < maximumLoopCount) {
                availableDroppedLoot[accountId][0].TakeLoot(sourceUnitController);
                currentLoopCount++;
            }

            if (availableDroppedLoot[accountId].Count > 0 && sourceUnitController.CharacterInventoryManager.EmptySlotCount() == 0) {
                if (sourceUnitController.CharacterInventoryManager.EmptySlotCount() == 0) {
                    //Debug.Log("No space left in inventory");
                }
                sourceUnitController.WriteMessageFeedMessage("Inventory is full!");
            }
        }

        public void AddLootTableState(LootTableState lootTableState) {
            //Debug.Log("LootManager.AddLootTableState()");

            if (lootTableStates.Contains(lootTableState) == false) {
                lootTableStates.Add(lootTableState);
            }
        }

        public void RemoveLootTableState(LootTableState lootTableState) {
            //Debug.Log("LootManager.RemoveLootTableState()");

            if (lootTableStates.Contains(lootTableState)) {
                lootTableStates.Remove(lootTableState);
            }
        }

        public void AddLootTableStateIndex(int lootDropId, LootTableState lootTableState) {
            //Debug.Log("LootManager.AddLootTableState()");

            if (lootTableStateDict.ContainsKey(lootDropId) == false) {
                lootTableStateDict.Add(lootDropId, lootTableState);
            }
        }

        public void RemoveLootTableStateIndex(int lootDropId) {
            if (lootTableStateDict.ContainsKey(lootDropId) == false) {
                return;
            }
            LootTableState lootTableState = lootTableStateDict[lootDropId];
            lootTableState.RemoveDroppedItem(lootDropIndex[lootDropId]);
            if (lootTableState.DroppedItems.Count == 0) {
                RemoveLootTableState(lootTableState);
            }
        }

        public bool CanDropUniqueItem(UnitController sourceUnitController, Item item) {
            //Debug.Log("LootManager.CanDropUniqueItem(" + item.DisplayName + ")");
            if (sourceUnitController.CharacterInventoryManager.GetItemCount(item.ResourceName) > 0) {
                return false;
            }
            if (sourceUnitController.CharacterEquipmentManager.HasEquipment(item.ResourceName) == true) {
                return false;
            }
            foreach (LootTableState lootTableState in lootTableStates) {
                foreach (LootDrop lootDrop in lootTableState.DroppedItems) {
                    if (lootDrop.HasItem(item)) {
                        return false;
                    }
                }
            }
            return true;
        }

        public int GetLootDropId() {
            lootDropIdCounter++;
            return lootDropIdCounter;
        }

        public void AddLootDropToIndex(UnitController sourceUnitController, LootDrop lootDrop) {
            //Debug.Log($"LootManager.AddLootDropToIndex({sourceUnitController.gameObject.name}, {lootDrop.LootDropId})");
            
            lootDropIndex.Add(lootDrop.LootDropId, lootDrop);
            if (networkManagerServer.ServerModeActive == true) {
                networkManagerServer.AddLootDrop(playerManagerServer.ActiveUnitControllerLookup[sourceUnitController], lootDrop.LootDropId, lootDrop.InstantiatedItem.InstanceId);
            }
        }

        public void AddNetworkLootDrop(int lootDropId, long itemInstanceId) {
            //Debug.Log($"LootManager.AddNetworkLootDrop({lootDropId}, {itemId})");

            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId) == false) {
                return;
            }
            LootDrop lootDrop = new LootDrop(lootDropId, systemItemManager.InstantiatedItems[itemInstanceId], systemGameManager);
            lootDropIndex.Add(lootDropId, lootDrop);
        }

    }

}