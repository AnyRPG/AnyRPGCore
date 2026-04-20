using UnityEngine;

namespace AnyRPG {
    public class BankPanel : BagPanel {

        [Header("Bank Panel")]

        [SerializeField]
        protected BagBarController bagBarController;

        // game manager references
        private StorageContainerManagerClient storageContainerManagerClient = null;

        public BagBarController BagBarController { get => bagBarController; set => bagBarController = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            bagBarController.Configure(systemGameManager);
            bagBarController.SetBagButtonCount(systemConfigurationManager.MaxBankBags);
            bagBarController.SetBagPanel(this);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            storageContainerManagerClient = systemGameManager.StorageContainerManagerClient;
        }

        protected override void ProcessCreateEventSubscriptions() {
            //Debug.Log("BankPanel.ProcessCreateEventSubscriptions()");
            base.ProcessCreateEventSubscriptions();

            systemEventManager.OnPlayerUnitDespawn += HandlePlayerUnitDespawn;
            systemEventManager.OnAddBankBagNode += HandleAddBankBagNode;
            systemEventManager.OnAddBankSlot += HandleAddSlot;
            systemEventManager.OnRemoveBankSlot += HandleRemoveSlot;

        }

        protected override void ProcessCleanupEventSubscriptions() {
            base.ProcessCleanupEventSubscriptions();

            systemEventManager.OnPlayerUnitDespawn -= HandlePlayerUnitDespawn;
            systemEventManager.OnAddBankBagNode -= HandleAddBankBagNode;
            systemEventManager.OnAddBankSlot -= HandleAddSlot;
            systemEventManager.OnRemoveBankSlot -= HandleRemoveSlot;
        }

        public void HandlePlayerUnitDespawn(UnitController unitController) {
            ClearSlots();
            bagBarController.ClearBagButtons();
        }

        public void HandleAddBankBagNode(BagNode bagNode) {
            //Debug.Log("BankPanel.HandleAddBankBagNode()");
            bagBarController.AddBagButton(bagNode);
            //bagNode.BagPanel = this;
        }

        public override void DropItemFromInventorySlot(SlotScript toSlot, SlotScript fromSlot) {
            //Debug.Log($"BankPanel.DropFromInventorySlot() toSlot: {toSlot.DisplayName} fromSlot: {fromSlot.DisplayName}");

            base.DropItemFromInventorySlot(toSlot, fromSlot);

            // swap or drop item from a character based panel
            if (fromSlot.BagPanel == this || fromSlot.BagPanel is InventoryPanel) {
                playerManagerClient.UnitController.CharacterInventoryManager.RequestDropItemFromInventorySlot(fromSlot.InventorySlot, toSlot.InventorySlot, fromSlot.BagPanel is InventoryPanel, toSlot.BagPanel is InventoryPanel);
                return;
            }

            // swap or drop items from a storage container
            playerManagerClient.UnitController.CharacterInventoryManager.RequestSwapItemToStorageContainer(storageContainerManagerClient.StorageContainerComponent,
                            storageContainerManagerClient.StorageContainerComponent.GetCurrentSlotIndex(fromSlot.InventorySlot),
                            toSlot.InventorySlot,
                            toSlot.BagPanel is BankPanel);
        }

        public override void DropItemFromNonInventorySlot(SlotScript slotScript, InstantiatedItem instantiatedItem) {
            base.DropItemFromNonInventorySlot(slotScript, instantiatedItem);
            // This slot has nothing in it, and we are not trying to transfer anything to it from another slot in the bag
            if (instantiatedItem is InstantiatedBag) {
                //Debug.Log("SlotScript.HandleLeftClick(): We are trying to drop a bag into the inventory.");
                // the handscript had a bag in it, and therefore we are trying to unequip a bag
                InstantiatedBag instantiatedBag = (InstantiatedBag)instantiatedItem;
                if (playerManagerClient.UnitController.CharacterInventoryManager.EmptySlotCount(instantiatedBag.BagNode.IsBankNode) - instantiatedBag.Slots > 0) {
                    //if (playerManager.UnitController.CharacterInventoryManager.EmptySlotCount() - bag.Slots > 0) {
                    //Debug.Log("SlotScript.HandleLeftClick(): We are trying to drop a bag into the inventory. There is enough empty space.");
                    playerManagerClient.UnitController.CharacterInventoryManager.RequestUnequipBagToSlot(instantiatedBag, slotScript.InventorySlot, true);
                }
            }
        }

        public override void SwapItemFromNonInventorySlot(SlotScript slotScript, InstantiatedItem instantiatedItem) {
            base.SwapItemFromNonInventorySlot(slotScript, instantiatedItem);
            if (instantiatedItem is InstantiatedBag) {
                // the handscript has a bag in it
                if (slotScript.InventorySlot.InstantiatedItem is InstantiatedBag) {
                    // This slot also has a bag in it, so swap the 2 bags
                    playerManagerClient.UnitController.CharacterInventoryManager.RequestSwapBags(instantiatedItem as InstantiatedBag, slotScript.InventorySlot.InstantiatedItem as InstantiatedBag);
                }
            }
        }

        public override void SetupContextMenu(ContextMenuPanel contextMenuPanel, InventorySlot inventorySlot) {
            base.SetupContextMenu(contextMenuPanel, inventorySlot);

            contextMenuPanel.EnableTakeButton(true);
            if (inventorySlot.InstantiatedItem is InstantiatedBag) {
                contextMenuPanel.EnableEquipButton(true);
            }
        }

        public override void PerformContextMenuAction(SlotScript slotScript, string actionName) {
            //Debug.Log($"InventoryPanel.PerformContextMenuAction() actionName: {actionName}");

            base.PerformContextMenuAction(slotScript, actionName);
            switch (actionName) {
                case "Take":
                    playerManagerClient.UnitController.CharacterInventoryManager.RequestMoveFromBankToInventory(slotScript.InventorySlot);
                    break;
                case "Equip":
                    if (slotScript.InventorySlot.InstantiatedItem is InstantiatedBag) {
                        playerManagerClient.UnitController.CharacterInventoryManager.RequestEquipBagFromSlot(slotScript.InventorySlot.InstantiatedItem as InstantiatedBag, slotScript.InventorySlot, true);
                    }
                    break;
            }
        }

        public override void PerformContextMenuAction(BagButton bagButton, string actionName) {
            base.PerformContextMenuAction(bagButton, actionName);
            if (bagButton.BagNode.InstantiatedBag == null) {
                return;
            }
            switch (actionName) {
                case "Unequip":
                    playerManagerClient.UnitController.CharacterInventoryManager.RequestUnequipBag(bagButton.BagNode.InstantiatedBag, true);
                    break;
            }
        }



    }

}