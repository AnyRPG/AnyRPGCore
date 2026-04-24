using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    public class StorageContainerComponent : InteractableOptionComponent {

        private List<InventorySlot> inventorySlots = new List<InventorySlot>();

        private int lootTableRemainingDrops = 0;

        private float weight = 0f;

        // game manager references
        private StorageContainerManagerClient storageContainerManagerClient = null;
        private ServerDataService serverDataService = null;

        public StorageContainerProps Props { get => interactableOptionProps as StorageContainerProps; }
        public List<InventorySlot> InventorySlots { get => inventorySlots; }
        public float Weight { get => weight; }

        public StorageContainerComponent(Interactable interactable, StorageContainerProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            if (interactionPanelTitle == string.Empty) {
                interactionPanelTitle = "Container";
            }
        }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            Props.ContainerLootTable.SetupScriptableObjects(systemGameManager);
            PerformSetupActivities();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            storageContainerManagerClient = systemGameManager.StorageContainerManagerClient;
            serverDataService = systemGameManager.ServerDataService;
        }

        public void PerformSetupActivities() {
            InitializeDefaultInventorySlots();

            // do not roll loot on network clients
            if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true) {
                RollLoot();
            }
        }

        /*
        private void AddDefaultItems() {
            foreach (Item item in Props.DefaultItems) {
                InstantiatedItem instantiatedItem = systemItemManager.GetNewInstantiatedItem(item);
                AddItem(instantiatedItem);
            }
        }
        */

        public override void ProcessCreateEventSubscriptions() {
            //Debug.Log("GatheringNode.CreateEventSubscriptions()");
            base.ProcessCreateEventSubscriptions();

            //systemEventManager.OnStorageContainer += HandleStorageContainer;
        }

        public override void ProcessCleanupEventSubscriptions() {
            //Debug.Log($"{gameObject.name}.StorageContainerInteractable.CleanupEventSubscriptions()");
            base.ProcessCleanupEventSubscriptions();

            //systemEventManager.OnStorageContainer -= HandleStorageContainer;
        }

        public override bool ProcessInteract(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            //Debug.Log(interactable.gameObject.name + ".StorageContainerInteractable.Interact()");

            base.ProcessInteract(sourceUnitController, componentIndex, choiceIndex);

            return true;
        }

        public override void ClientInteraction(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            base.ClientInteraction(sourceUnitController, componentIndex, choiceIndex);

            storageContainerManagerClient.SetProps(Props, this, componentIndex, choiceIndex);
            uIManager.storageContainerWindow.OpenWindow();
        }

        public override void StopInteract() {
            base.StopInteract();
            uIManager.storageContainerWindow.CloseWindow();
        }

        public override int GetCurrentOptionCount(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.CharacterCreatorInteractable.GetCurrentOptionCount()");
            return GetValidOptionCount(sourceUnitController);
        }

        public List<InventorySlot> AddInventorySlots(int numSlots) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.AddInventorySlots({numSlots})");

            List<InventorySlot> returnList = new List<InventorySlot>();
            for (int i = 0; i < numSlots; i++) {
                InventorySlot inventorySlot = new InventorySlot(systemGameManager);
                returnList.Add(inventorySlot);
                AddInventorySlot(inventorySlot);
            }
            return returnList;
        }

        private void AddInventorySlot(InventorySlot inventorySlot) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.AddInventorySlot() count: {inventorySlots.Count}");

            inventorySlots.Add(inventorySlot);
            inventorySlot.OnAddItem += HandleAddItemToInventorySlot;
            inventorySlot.OnRemoveItem += HandleRemoveItemFromInventorySlot;
            //OnAddInventorySlot(inventorySlot);
        }

        private void HandleRemoveItemFromInventorySlot(InventorySlot slot, InstantiatedItem instantiatedItem) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.HandleRemoveItemFromInventorySlot({instantiatedItem.Item.ResourceName})");

            weight -= instantiatedItem.Item.Weight;
            //NotifyOnItemCountChanged(instantiatedItem.Item);
            interactable.InteractableEventController.NotifyOnRemoveItemFromStorageContainerSlot(GetCurrentSlotIndex(slot), instantiatedItem);
            //unitController.UnitEventController.NotifyOnCarryWeightChanged();
        }

        private void HandleAddItemToInventorySlot(InventorySlot slot, InstantiatedItem instantiatedItem) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.HandleAddItemToInventorySlot({slot.GetCurrentInventorySlotIndex(unitController)}, {instantiatedItem.Item.ResourceName})");

            weight += instantiatedItem.Item.Weight;
            //NotifyOnItemCountChanged(instantiatedItem.Item);
            interactable.InteractableEventController.NotifyOnAddItemToStorageContainerSlot(GetCurrentSlotIndex(slot), instantiatedItem);
            //unitController.UnitEventController.NotifyOnCarryWeightChanged();
        }

        private void RemoveInventorySlot(InventorySlot inventorySlot) {
            inventorySlots.Remove(inventorySlot);
            inventorySlot.OnAddItem -= HandleAddItemToInventorySlot;
            inventorySlot.OnRemoveItem -= HandleRemoveItemFromInventorySlot;
            //OnRemoveInventorySlot(inventorySlot);
        }

        public void InitializeDefaultInventorySlots() {
            for (int i = 0; i < Props.NumberOfSlots; i++) {
                InventorySlot inventorySlot = new InventorySlot(systemGameManager);
                AddInventorySlot(inventorySlot);
            }
        }

        /// <summary>
        /// Adds an item to the inventory
        /// </summary>
        /// <param name="instantiatedItem"></param>
        public bool AddItem(InstantiatedItem instantiatedItem) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.AddItem({(instantiatedItem == null ? "null" : instantiatedItem.DisplayName)}, {addToBank})");

            if (instantiatedItem == null) {
                return false;
            }
            /*
            if (performUniqueCheck == true && instantiatedItem.Item.UniqueItem == true && GetItemCount(instantiatedItem.Item.ResourceName) > 0) {
                unitController.UnitEventController.NotifyOnWriteMessageFeedMessage($"{instantiatedItem.DisplayName} is unique.  You can only carry one at a time.");
                return false;
            }
            */
            if (instantiatedItem.Item.MaximumStackSize > 0) {
                if (PlaceInStack(instantiatedItem)) {
                    return true;
                }
            }
            //Debug.Log("About to attempt placeInEmpty");
            return PlaceInEmpty(instantiatedItem);
        }


        public bool AddInventoryItem(InstantiatedItem instantiatedItem, InventorySlot inventorySlot) {
            return inventorySlot.AddItem(instantiatedItem);
        }

        public bool AddInventoryItem(InstantiatedItem instantiatedItem, int slotIndex) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.AddInventoryItem({instantiatedItem.ResourceName}, {slotIndex})");

            if (inventorySlots.Count > slotIndex) {
                return inventorySlots[slotIndex].AddItem(instantiatedItem);
            }
            return AddItem(instantiatedItem);
        }

        public bool AddInventoryItem(long itemInstanceId, int slotIndex) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.AddInventoryItem({itemInstanceId}, {slotIndex})");
            InstantiatedItem instantiatedItem = systemItemManager.GetExistingInstantiatedItem(itemInstanceId);
            if (instantiatedItem != null) {
                return AddInventoryItem(instantiatedItem, slotIndex);
            }
            return false;
        }

        public void RemoveInventoryItem(InstantiatedItem instantiatedItem) {
            foreach (InventorySlot slot in inventorySlots) {
                if (!slot.IsEmpty && slot.InstantiatedItem == instantiatedItem) {
                    slot.RemoveItem(instantiatedItem);
                    return;
                }
            }
        }

        public void RemoveInventoryItemFromSlot(int slotIndex, long itemInstanceId) {
            InstantiatedItem instantiatedItem = systemItemManager.GetExistingInstantiatedItem(itemInstanceId);
            if (instantiatedItem == null) {
                return;
            }
            if (inventorySlots.Count > slotIndex) {
                inventorySlots[slotIndex].RemoveItem(instantiatedItem);
            }
        }

        public void RemoveInventoryItem(long itemInstanceId) {
            foreach (InventorySlot slot in inventorySlots) {
                if (!slot.IsEmpty && slot.InstantiatedItem.InstanceId == itemInstanceId) {
                    slot.RemoveItem(slot.InstantiatedItem);
                    return;
                }
            }
        }

        private bool PlaceInEmpty(InstantiatedItem instantiatedItem) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.PlaceInEmpty({instantiatedItem.ResourceName}, {addToBank})");

            int slotIndex = 0;
            foreach (InventorySlot inventorySlot in inventorySlots) {
                //Debug.Log($"CharacterInventoryManager.PlaceInEmpty({instantiatedItem.ResourceName}): checking slot");
                if (inventorySlot.IsEmpty) {
                    //Debug.Log($"CharacterInventoryManager.PlaceInEmpty({instantiatedItem.ResourceName}): checking slot: its empty.  adding item");
                    inventorySlot.AddItem(instantiatedItem);
                    //unitController.UnitEventController.NotifyOnPlaceInEmpty(instantiatedItem, slotIndex);
                    return true;
                }
                slotIndex++;
            }

            if (EmptySlotCount() == 0) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.PlaceInEmpty({instantiatedItem.ResourceName}, {addToBank}): no empty slots");
                //unitController.UnitEventController.NotifyOnWriteMessageFeedMessage($"Container is full!");
            }
            return false;
        }

        public int EmptySlotCount() {
            int count = 0;

            foreach (InventorySlot slot in inventorySlots) {
                if (slot.IsEmpty) {
                    count++;
                }
            }

            return count;
        }


        private bool PlaceInStack(InstantiatedItem instantiatedItem) {
            int slotIndex = 0;
            foreach (InventorySlot inventorySlot in inventorySlots) {
                if (PlaceInStack(inventorySlot, slotIndex, instantiatedItem) == true) {
                    return true;
                }
                slotIndex++;
            }

            return false;
        }

        public void PlaceInStack(int slotIndex, InstantiatedItem instantiatedItem) {
            if (inventorySlots.Count > slotIndex) {
                PlaceInStack(inventorySlots[slotIndex], slotIndex, instantiatedItem);
            }
        }

        private bool PlaceInStack(InventorySlot inventorySlot, int slotIndex, InstantiatedItem instantiatedItem) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.PlaceInStack({slotIndex}, {instantiatedItem.Item.ResourceName}, {addToBank})");

            if (inventorySlot.StackItem(instantiatedItem)) {
                //unitController.UnitEventController.NotifyOnPlaceInStack(instantiatedItem, slotIndex);
                return true;
            }
            return false;
        }

        public override void SetSaveData(InteractableSaveData interactableSaveData) {
            //Debug.Log($"{interactable.gameObject.name}.LootableNodeComponent.SetSaveData()");

            base.SetSaveData(interactableSaveData);
            StorageContainerSaveData storageContainerSaveData = new StorageContainerSaveData();
            storageContainerSaveData.InventorySlotSaveDataList = new List<InventorySlotSaveData>();
            foreach (InventorySlot inventorySlot in inventorySlots) {
                InventorySlotSaveData inventorySlotSaveData = new InventorySlotSaveData();
                foreach (InstantiatedItem instantiatedItem in inventorySlot.InstantiatedItems.Values) {
                    inventorySlotSaveData.ItemInstanceIds.Add(instantiatedItem.InstanceId);
                }
                storageContainerSaveData.InventorySlotSaveDataList.Add(inventorySlotSaveData);
            }
            if (interactableSaveData.StorageContainerSaveData.Count == 0) {
                interactableSaveData.StorageContainerSaveData.Add(storageContainerSaveData);
            } else {
                interactableSaveData.StorageContainerSaveData[0] = storageContainerSaveData;
            }
        }

        public override void LoadFromSaveData(InteractableSaveData interactableSaveData) {
            //Debug.Log($"{interactable.gameObject.name}.DroppedItemComponent.LoadFromSaveData()");

            base.LoadFromSaveData(interactableSaveData);
            ClearAllItems();
            int index = 0;
            foreach (StorageContainerSaveData storageContainerSaveData in interactableSaveData.StorageContainerSaveData) {
                foreach (InventorySlotSaveData inventorySlotSaveData in storageContainerSaveData.InventorySlotSaveDataList) {
                    if (index >= inventorySlots.Count) {
                        Debug.LogWarning($"StorageContainerComponent.LoadFromSaveData(): index {index} is out of range for inventorySlots list.  This should never happen.  Check the save data and make sure it matches the number of inventory slots on this container.");
                        continue;
                    }
                    foreach (long itemInstanceId in inventorySlotSaveData.ItemInstanceIds) {
                        InstantiatedItem instantiatedItem = systemItemManager.GetExistingInstantiatedItem(itemInstanceId);
                        if (instantiatedItem != null) {
                            inventorySlots[index].AddItem(instantiatedItem);
                        }
                    }
                    index++;
                }
            }
        }

        public void ClearAllItems() {
            foreach (InventorySlot inventorySlot in inventorySlots) {
                // remove the items from the database in network mode, otherwise they will be orphaned and never cleaned up
                if (networkManagerServer.ServerModeActive == true) {
                    foreach (InstantiatedItem instantiatedItem in inventorySlot.InstantiatedItems.Values) {
                        serverDataService.DeleteItemInstance(instantiatedItem);
                    }
                }
                inventorySlot.RemoveAllItems();
            }
        }

        public int GetCurrentSlotIndex(InventorySlot inventorySlot) {
            if (!inventorySlots.Contains(inventorySlot)) {
                return -1;
            }
            return inventorySlots.IndexOf(inventorySlot);
        }

        public void SwapItemsInSlots(int fromSlotIndex, int toSlotIndex) {
            if (fromSlotIndex < 0 || fromSlotIndex >= inventorySlots.Count || toSlotIndex < 0 || toSlotIndex >= inventorySlots.Count) {
                Debug.LogWarning($"{interactable.gameObject.name}.StorageContainerComponent.SwapItemsInSlots(): invalid slot indices {fromSlotIndex}, {toSlotIndex}");
                return;
            }
            SwapItemsInSlots(inventorySlots[fromSlotIndex], inventorySlots[toSlotIndex]);
        }

        public void SwapItemsInSlots(InventorySlot fromSlot, InventorySlot toSlot) {
            fromSlot.SwapItems(toSlot);
        }

        public override void Cleanup() {
            base.Cleanup();

            // in network mode, we need to delete any items that are still in the storage container component when it is cleaned up
            // otherwise they will pollute the server with orphaned items that are no longer referenced by anything and will never be cleaned up
            if (networkManagerServer.ServerModeActive == true) {
                foreach (InventorySlot inventorySlot in inventorySlots) {
                    if (inventorySlot.InstantiatedItems.Count > 0) {
                        foreach (InstantiatedItem instantiatedItem in inventorySlot.InstantiatedItems.Values) {
                            serverDataService.DeleteItemInstance(instantiatedItem);
                        }
                        inventorySlot.InstantiatedItems.Clear();
                    }
                }
            }

        }

        private void RollLoot() {
            //Debug.Log($"{interactable.gameObject.name}.StorageContainerComponent.RollLoot()");

            lootTableRemainingDrops = Props.ContainerLootTable.DropLimit;
            bool lootTableUnlimitedDrops = (Props.ContainerLootTable.DropLimit == 0);

            foreach (ContainerLootGroup lootGroup in Props.ContainerLootTable.LootGroups) {
                //Debug.Log($"{interactable.gameObject.name}.StorageContainerComponent.RollLoot(): checking loot group with chance {lootGroup.GroupChance} and drop limit {lootGroup.DropLimit}");

                // check if this group can drop an item
                float randomInt = UnityEngine.Random.Range(0, 100);
                if (lootGroup.GroupChance > randomInt) {
                    //Debug.Log($"{interactable.gameObject.name}.StorageContainerComponent.RollLoot(): loot group passed chance check with random int {randomInt}");
                    // unlimited drops settins for this loot group
                    int lootGroupRemainingDrops = lootGroup.DropLimit;
                    bool lootGroupUnlimitedDrops = (lootGroup.DropLimit == 0);

                    // ignore drop limit settings for this loot group
                    bool ignoreDropLimit = true;

                    // get list of loot that is currenly valid to be rolled so that weights can be properly calculated based on only valid loot
                    List<ContainerLoot> validLoot = new List<ContainerLoot>(lootGroup.Loot);

                    if (lootGroup.GuaranteedDrop == true) {
                        //Debug.Log($"{interactable.gameObject.name}.StorageContainerComponent.RollLoot(): loot group has guaranteed drop");

                        List<int> randomItemIndexes = new List<int>();
                        // guaranteed drops can never have a 0 drop limit, but shouldn't be unlimited because the chance is not random per item like non guaranteed drops
                        int maxCount = (int)Mathf.Min(Mathf.Clamp(lootGroup.DropLimit, 1, Mathf.Infinity), validLoot.Count);
                        while (randomItemIndexes.Count < maxCount) {
                            //Debug.Log($"{interactable.gameObject.name}.StorageContainerComponent.RollLoot(): rolling for guaranteed drop {randomItemIndexes.Count} of {maxCount}");

                            // pure random
                            //int randomNumber = UnityEngine.Random.Range(0, lootGroup.Loot.Count);

                            // weighted
                            int usedIndex = 0;
                            int sum_of_weight = 0;
                            int accumulatedWeight = 0;

                            for (int i = 0; i < validLoot.Count; i++) {
                                sum_of_weight += (int)validLoot[i].DropChance;
                            }
                            int rnd = UnityEngine.Random.Range(0, sum_of_weight);
                            for (int i = 0; i < validLoot.Count; i++) {
                                accumulatedWeight += (int)validLoot[i].DropChance;
                                if (rnd < accumulatedWeight) {
                                    usedIndex = i;
                                    break;
                                }
                            }

                            if (lootGroup.UniqueLimit > 0) {
                                int foundCount = randomItemIndexes.Where(x => x.Equals(usedIndex)).Count();
                                if (foundCount < lootGroup.UniqueLimit) {
                                    randomItemIndexes.Add(usedIndex);
                                }

                            } else {
                                randomItemIndexes.Add(usedIndex);
                            }
                        }
                        foreach (int randomItemIndex in randomItemIndexes) {
                            GetLootDrop(validLoot[randomItemIndex], lootGroupUnlimitedDrops, ignoreDropLimit, lootTableUnlimitedDrops, ref lootGroupRemainingDrops);
                        }
                    } else {
                        foreach (ContainerLoot item in validLoot) {
                            int roll = UnityEngine.Random.Range(0, 100);
                            if (roll <= item.DropChance) {
                                GetLootDrop(item, lootGroupUnlimitedDrops, ignoreDropLimit, lootTableUnlimitedDrops, ref lootGroupRemainingDrops);
                            }
                            if ((lootGroupUnlimitedDrops == false && lootGroupRemainingDrops <= 0) || (lootTableUnlimitedDrops == false && lootTableRemainingDrops <= 0)) {
                                break;
                            }
                        }
                    }

                    if (lootTableUnlimitedDrops == false && lootTableRemainingDrops <= 0) {
                        break;
                    }
                }
            }
        }

        private void GetLootDrop(ContainerLoot loot, bool lootGroupUnlimitedDrops, bool ignoreDropLimit, bool lootTableUnlimitedDrops, ref int lootGroupRemainingDrops) {
            //Debug.Log($"LootTableState.GetLootDrop({interactable.gameObject.name}, {loot.Item.ResourceName}, unlimited: {lootGroupUnlimitedDrops}, {ignoreDropLimit}, {lootTableRemainingDrops})");

            List<InstantiatedItem> returnValue = new List<InstantiatedItem>();
            int itemCount = UnityEngine.Random.Range(loot.MinDrops, loot.MaxDrops + 1);
            for (int i = 0; i < itemCount; i++) {
                InstantiatedItem instantiatedItem = systemItemManager.GetNewInstantiatedItem(loot.Item);
                if (instantiatedItem != null) {
                    AddItem(instantiatedItem);
                    if (networkManagerServer.ServerModeActive == true) {
                        serverDataService.CreateItemInstance(instantiatedItem);
                    }
                }
                if (lootGroupUnlimitedDrops == false && ignoreDropLimit == false) {
                    lootGroupRemainingDrops = lootGroupRemainingDrops - 1;
                    if (lootGroupRemainingDrops <= 0) {
                        break;
                    }
                }
                if (lootTableUnlimitedDrops == false && ignoreDropLimit == false) {
                    lootTableRemainingDrops -= 1;
                    if (lootTableRemainingDrops <= 0) {
                        break;
                    }
                }
            }

        }

        public (int, bool) CanPlaceInStackOrEmpty(InstantiatedItem instantiatedItem) {
            int slotIndex = 0;
            foreach (InventorySlot inventorySlot in inventorySlots) {
                if (inventorySlot.CanStackOrEmptyItem(instantiatedItem) == true) {
                    return (slotIndex, true);
                }
                slotIndex++;
            }

            return (-1, false);
        }
    }

}