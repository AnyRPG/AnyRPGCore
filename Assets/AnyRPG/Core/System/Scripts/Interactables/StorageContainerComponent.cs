using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class StorageContainerComponent : InteractableOptionComponent {

        private List<InventorySlot> inventorySlots = new List<InventorySlot>();
        
        private float weight = 0f;

        // game manager references
        private StorageContainerManagerClient storageContainerManagerClient = null;

        public StorageContainerProps Props { get => interactableOptionProps as StorageContainerProps; }

        public StorageContainerComponent(Interactable interactable, StorageContainerProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            if (interactionPanelTitle == string.Empty) {
                interactionPanelTitle = "Container";
            }
        }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            PerformSetupActivities();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            storageContainerManagerClient = systemGameManager.StorageContainerManagerClient;
        }

        public void PerformSetupActivities() {
            InitializeDefaultInventorySlots();
        }

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
            //unitController.UnitEventController.NotifyOnRemoveItemFromInventorySlot(slot, instantiatedItem);
            //unitController.UnitEventController.NotifyOnCarryWeightChanged();
        }

        private void HandleAddItemToInventorySlot(InventorySlot slot, InstantiatedItem instantiatedItem) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.HandleAddItemToInventorySlot({slot.GetCurrentInventorySlotIndex(unitController)}, {instantiatedItem.Item.ResourceName})");

            weight += instantiatedItem.Item.Weight;
            //NotifyOnItemCountChanged(instantiatedItem.Item);
            //unitController.UnitEventController.NotifyOnAddItemToInventorySlot(slot, instantiatedItem);
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

        public void RemoveInventoryItem(InstantiatedItem instantiatedItem) {
            foreach (InventorySlot slot in inventorySlots) {
                if (!slot.IsEmpty && slot.InstantiatedItem == instantiatedItem) {
                    slot.RemoveItem(instantiatedItem);
                    return;
                }
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


    }

}