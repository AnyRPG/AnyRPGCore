using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemItemManager : ConfiguredClass {

        private int clientItemIdCount = 1;
        private int serverItemIdCount = -1;

        private Dictionary<int, InstantiatedItem> instantiatedItems = new Dictionary<int, InstantiatedItem>();

        public Dictionary<int, InstantiatedItem> InstantiatedItems { get => instantiatedItems; set => instantiatedItems = value; }
        public int ClientItemIdCount { get => clientItemIdCount; }

        public InstantiatedItem GetNewInstantiatedItem(string itemName, ItemQuality usedItemQuality = null) {
            //Debug.Log(this.GetType().Name + ".GetNewResource(" + resourceName + ")");
            Item item = systemDataFactory.GetResource<Item>(itemName);
            if (item == null) {
                return null;
            }
            return GetNewInstantiatedItem(item, usedItemQuality);
        }

        /// <summary>
        /// Get a new instantiated Item
        /// </summary>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public InstantiatedItem GetNewInstantiatedItem(Item item, ItemQuality usedItemQuality = null) {
            //Debug.Log($"SystemItemManager.GetNewInstantiatedItem({item.ResourceName})");

            InstantiatedItem instantiatedItem = GetNewInstantiatedItem(GetNewItemInstanceId(), item, usedItemQuality);
            return instantiatedItem;
        }

        public int GetNewItemInstanceId() {
            Debug.Log($"SystemItemManager.GetNewItemInstanceId()");

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

            Debug.Log($"SystemItemManager.GetNewServerItemInstanceId() return {returnValue}");
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

            Debug.Log($"SystemItemManager.GetNewClientItemInstanceId() return {returnValue}");
            return returnValue;
        }

        public InstantiatedItem GetNewInstantiatedItem(int itemInstanceId, Item item, ItemQuality usedItemQuality = null) {
            //Debug.Log($"SystemItemManager.GetNewInstantiatedItem({itemInstanceId}, {item?.ResourceName}, {usedItemQuality?.ResourceName})");

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
            instantiatedItems.Clear();
        }

        public InstantiatedItem GetExistingInstantiatedItem(int itemInstanceId) {
            if (instantiatedItems.ContainsKey(itemInstanceId)) {
                return instantiatedItems[itemInstanceId];
            }
            return null;
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