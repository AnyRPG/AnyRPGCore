using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class StorageContainerPanel : BagPanel {

        //public override event Action<ICloseableWindowContents> OnOpenWindow;

        [Header("StorageContainer Panel")]

        [SerializeField]
        protected TextMeshProUGUI weightText = null;

        // game manager references
        private StorageContainerManagerClient storageContainerManagerClient = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            storageContainerManagerClient = systemGameManager.StorageContainerManagerClient;
        }

        protected override void ProcessCreateEventSubscriptions() {
            //Debug.Log("InventoryPanel.ProcessCreateEventSubscriptions()");
            base.ProcessCreateEventSubscriptions();

            //systemEventManager.OnPlayerUnitDespawn += HandlePlayerUnitDespawn;
        }

        protected override void ProcessCleanupEventSubscriptions() {
            base.ProcessCleanupEventSubscriptions();

            //systemEventManager.OnPlayerUnitDespawn += HandlePlayerUnitDespawn;
        }

        private void UpdateWeightText() {
            if (storageContainerManagerClient.StorageContainerComponent == null) {
                weightText.text = "0 / 0";
                return;
            }
            float inventoryWeight = storageContainerManagerClient.StorageContainerComponent.Weight;
            weightText.text = $"Weight: {Mathf.Ceil(inventoryWeight)} kg";
        }

        public override void ProcessOpenWindowNotification() {
            base.ProcessOpenWindowNotification();
            AddStorageSlots();
            UpdateWeightText();
            closeableWindow.SetWindowTitle(storageContainerManagerClient.StorageContainerComponent.Interactable.DisplayName);
        }

        public override void ReceiveClosedWindowNotification() {
            base.ReceiveClosedWindowNotification();
            foreach (SlotScript slotScript in slots) {
                slotScript.InventorySlot.OnAddItem -= HandleAddItem;
                slotScript.InventorySlot.OnRemoveItem -= HandleRemoveItem;
            }
            ClearSlots();
        }

        private void AddStorageSlots() {
            //Debug.Log("StorageContainerPanel.AddStorageSlots()");
            List<InventorySlot> inventorySlots = storageContainerManagerClient.StorageContainerComponent?.InventorySlots;
            foreach (InventorySlot inventorySlot in inventorySlots) {
                //Debug.Log("StorageContainerPanel.AddStorageSlots() found slot: " + inventorySlot.DisplayName);
                HandleAddSlot(inventorySlot);
                inventorySlot.OnAddItem += HandleAddItem;
                inventorySlot.OnRemoveItem += HandleRemoveItem;
            }
            foreach (SlotScript slotScript in slots) {
                slotScript.UpdateSlot();
            }
        }

        private void HandleRemoveItem(InventorySlot slot, InstantiatedItem item) {
            //Debug.Log($"StorageContainerPanel.HandleRemoveItem(item: {item.ResourceName})");

            UpdateWeightText();
        }

        private void HandleAddItem(InventorySlot slot, InstantiatedItem item) {
            //Debug.Log($"StorageContainerPanel.HandleAddItem(item: {item.ResourceName})");
            
            UpdateWeightText();
        }


        public override void DropItemFromInventorySlot(SlotScript toSlot, SlotScript fromSlot) {
            //Debug.Log($"StorageContainerPanel.DropFromInventorySlot() toSlot: {toSlot.DisplayName} fromSlot: {fromSlot.DisplayName}");

            base.DropItemFromInventorySlot(toSlot, fromSlot);

            // swap or drop item from same panel
            if (fromSlot.BagPanel == this) {
                playerManagerClient.UnitController.CharacterInventoryManager.RequestSwapItemsInStorageContainerSlots(
                    storageContainerManagerClient.StorageContainerComponent,
                    toSlot.InventorySlot,
                    fromSlot.InventorySlot);
                return;
            }

            // swap or drop items from a character
            playerManagerClient.UnitController.CharacterInventoryManager.RequestMoveItemToStorageContainer(storageContainerManagerClient.StorageContainerComponent,
                GetCurrentStorageContainerSlotIndex(toSlot.InventorySlot),
                fromSlot.InventorySlot,
                fromSlot.BagPanel is BankPanel);
        }

        public int GetCurrentStorageContainerSlotIndex(InventorySlot inventorySlot) {
            return storageContainerManagerClient.StorageContainerComponent.GetCurrentSlotIndex(inventorySlot);
        }

        public override void DropItemFromNonInventorySlot(SlotScript slotScript, InstantiatedItem instantiatedItem) {
            // do nothing intentionally, we don't want to allow dropping items from non inventory slots into storage container slots.
        }

        public override void SwapItemFromNonInventorySlot(SlotScript slotScript, InstantiatedItem instantiatedItem) {
            // do nothing intentionally, we don't want to allow swapping items from non inventory slots into storage container slots.
        }

    }

}