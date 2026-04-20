using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class InventoryPanel : BagPanel {

        //public override event Action<ICloseableWindowContents> OnOpenWindow;

        [Header("Inventory Panel")]

        [SerializeField]
        protected BagBarController bagBarController;

        [SerializeField]
        protected TextMeshProUGUI carryWeightText = null;

        [SerializeField]
        protected CurrencyBarController currencyBarController = null;

        // game manager references
        private StorageContainerManagerClient storageContainerManagerClient = null;
        private VendorManagerClient vendorManagerClient = null;
        private UIManager uIManager = null;
        private HandScript handScript = null;
        private MailboxManagerClient mailboxManagerClient = null;
        private TradeServiceClient tradeServiceClient = null;

        public BagBarController BagBarController { get => bagBarController; set => bagBarController = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            bagBarController.Configure(systemGameManager);
            bagBarController.SetBagButtonCount(systemConfigurationManager.MaxInventoryBags);
            bagBarController.SetBagPanel(this);
            currencyBarController.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            storageContainerManagerClient = systemGameManager.StorageContainerManagerClient;
            uIManager = systemGameManager.UIManager;
            handScript = uIManager.HandScript;
            vendorManagerClient = systemGameManager.VendorManagerClient;
            mailboxManagerClient = systemGameManager.MailboxManagerClient;
            tradeServiceClient = systemGameManager.TradeServiceClient;
        }

        protected override void ProcessCreateEventSubscriptions() {
            //Debug.Log("InventoryPanel.ProcessCreateEventSubscriptions()");
            base.ProcessCreateEventSubscriptions();

            systemEventManager.OnPlayerUnitDespawn += HandlePlayerUnitDespawn;
            systemEventManager.OnAddInventoryBagNode += HandleAddInventoryBagNode;
            systemEventManager.OnAddInventorySlot += HandleAddSlot;
            systemEventManager.OnRemoveInventorySlot += HandleRemoveSlot;
            systemEventManager.OnCurrencyChange += HandleCurrencyChange;
            systemEventManager.OnCarryWeightChanged += HandleCarryWeightChanged;
            systemEventManager.OnStatChanged += HandleStatChanged;
        }

        protected override void ProcessCleanupEventSubscriptions() {
            base.ProcessCleanupEventSubscriptions();

            systemEventManager.OnPlayerUnitDespawn += HandlePlayerUnitDespawn;
            systemEventManager.OnAddInventoryBagNode -= HandleAddInventoryBagNode;
            systemEventManager.OnAddInventorySlot -= HandleAddSlot;
            systemEventManager.OnRemoveInventorySlot -= HandleRemoveSlot;
            systemEventManager.OnCurrencyChange -= HandleCurrencyChange;
            systemEventManager.OnCarryWeightChanged -= HandleCarryWeightChanged;
            systemEventManager.OnStatChanged -= HandleStatChanged;
        }

        private void HandleStatChanged() {
            UpdateCarryWeightText();
        }

        private void HandleCarryWeightChanged() {
            UpdateCarryWeightText();
        }

        private void UpdateCarryWeightText() {
            if (playerManagerClient.UnitController == null) {
                carryWeightText.text = "0 / 0";
                return;
            }
            float inventoryWeight = playerManagerClient.UnitController.CharacterInventoryManager.Weight;
            float equippedWeight = playerManagerClient.UnitController.CharacterEquipmentManager.EquippedWeight;
            float totalWeight = inventoryWeight + equippedWeight;
            float carryWeight = playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.CarryWeight].CurrentValue + systemConfigurationManager.BaseCarryWeight;
            carryWeightText.text = $"<color={(totalWeight > carryWeight ? "red" : "white")}>Inventory: {Mathf.Ceil(inventoryWeight)} kg\n" +
                $"Equipped: {Mathf.Ceil(equippedWeight)} kg\n" +
                $"Total: {Mathf.Ceil(totalWeight)}";
            if (systemConfigurationManager.UseEncumberance == true) {
                carryWeightText.text += $" / {Mathf.Ceil(carryWeight)}";
            }
            carryWeightText.text += " kg</color>";
        }

        private void HandleCurrencyChange() {
            UpdateCurrencyAmount();
        }

        private void UpdateCurrencyAmount() {
            if (playerManagerClient.UnitController == null) {
                return;
            }
            if (systemConfigurationManager.DefaultCurrencyGroup?.BaseCurrency != null) {
                currencyBarController.UpdateCurrencyAmount(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, playerManagerClient.UnitController.CharacterCurrencyManager.GetBaseCurrencyValue(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency));
            } else {
                currencyBarController.ClearCurrencyAmounts();
            }
        }

        public void HandlePlayerUnitDespawn(UnitController unitController) {
            //Debug.Log("InventoryPanel.HandlePlayerUnitDespawn()");

            ClearSlots();
            bagBarController.ClearBagButtons();
        }

        public void HandleAddInventoryBagNode(BagNode bagNode) {
            //Debug.Log("InventoryPanel.HandleAddInventoryBagNode()");
            bagBarController.AddBagButton(bagNode);
            //bagNode.BagPanel = this;
        }

        public override void ProcessOpenWindowNotification() {
            base.ProcessOpenWindowNotification();
            UpdateCarryWeightText();
            UpdateCurrencyAmount();
        }

        public override void DropItemFromInventorySlot(SlotScript toSlot, SlotScript fromSlot) {
            //Debug.Log($"InventoryPanel.DropFromInventorySlot() toSlot: {toSlot.DisplayName} fromSlot: {fromSlot.DisplayName}");

            base.DropItemFromInventorySlot(toSlot, fromSlot);

            // swap or drop item from a character based panel
            if (fromSlot.BagPanel == this || fromSlot.BagPanel is BankPanel) {
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
                    playerManagerClient.UnitController.CharacterInventoryManager.RequestUnequipBagToSlot(instantiatedBag, slotScript.InventorySlot, false);
                }
            } else if (instantiatedItem is InstantiatedEquipment) {
                // the handscript had equipment in it, and therefore we are trying to unequip some equipment
                playerManagerClient.UnitController.CharacterEquipmentManager.RequestUnequipToSlot(instantiatedItem as InstantiatedEquipment, slotScript.InventorySlot.GetCurrentInventorySlotIndex(playerManagerClient.UnitController));
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
            } else if (instantiatedItem is InstantiatedEquipment) {
                // the handscript has equipment in it
                if (slotScript.InventorySlot.InstantiatedItem is InstantiatedEquipment && (slotScript.InventorySlot.InstantiatedItem as InstantiatedEquipment).Equipment.EquipmentSlotType == (instantiatedItem as InstantiatedEquipment).Equipment.EquipmentSlotType) {
                    // this slot has equipment in it, and the equipment matches the slot of the item in the handscript.  swap them
                    playerManagerClient.UnitController.CharacterEquipmentManager.RequestSwapInventoryEquipment(instantiatedItem as InstantiatedEquipment, slotScript.InventorySlot.InstantiatedItem as InstantiatedEquipment);
                }
            }
        }

        public override void SetupContextMenu(ContextMenuPanel contextMenuPanel, InventorySlot inventorySlot) {
            base.SetupContextMenu(contextMenuPanel, inventorySlot);

            if (inventorySlot.InstantiatedItem.Item.ItemPickupPrefabProfile != null && systemConfigurationManager.CanDropItems == true) {
                contextMenuPanel.EnableDropButton(true);
            }
            contextMenuPanel.EnableSplitButton(inventorySlot.InstantiatedItem.Item.MaximumStackSize > 1 && inventorySlot.InstantiatedItems.Count > 1);
            contextMenuPanel.EnableDestroyButton(true);
            if (inventorySlot.InstantiatedItem.IsUseable()) {
                contextMenuPanel.EnableUseButton(true);
            }
            if (uIManager.bankWindow.IsOpen == true) {
                contextMenuPanel.EnableBankButton(true);
            }
            if (uIManager.storageContainerWindow.IsOpen == true) {
                contextMenuPanel.EnableStoreButton(true);
            }
            if (uIManager.mailComposeWindow.IsOpen == true) {
                contextMenuPanel.EnableMailButton(true);
            }
            if (uIManager.tradeWindow.IsOpen == true) {
                contextMenuPanel.EnableTradeButton(true);
            }
            if (inventorySlot.InstantiatedItem is InstantiatedBag) {
                contextMenuPanel.EnableEquipButton(true);
            }
            if (inventorySlot.InstantiatedItem is InstantiatedEquipment) {
                contextMenuPanel.EnableEquipButton(true);
            }
        }

        public override void PerformContextMenuAction(SlotScript slotScript, string actionName) {
            //Debug.Log($"InventoryPanel.PerformContextMenuAction() actionName: {actionName}");

            base.PerformContextMenuAction(slotScript, actionName);
            if (slotScript.InventorySlot.InstantiatedItem == null) {
                return;
            }
            switch (actionName) {
                case "Destroy":
                    handScript.SetPosition(slotScript.transform.position);
                    slotScript.SendItemToHandScript();
                    uIManager.confirmDestroyMenuWindow.OpenWindow();
                    break;
                case "Drop":
                    playerManagerClient.UnitController.CharacterInventoryManager.RequestDropItemOnGround(slotScript.InventorySlot);
                    break;
                case "Split":
                    if (slotScript.InventorySlot.InstantiatedItem != null) {
                        playerManagerClient.UnitController.CharacterInventoryManager.FromSlot = slotScript;
                        uIManager.splitStackWindow.OpenWindow();
                    }
                    break;
                case "Use":
                    playerManagerClient.UnitController.CharacterInventoryManager.RequestUseItem(slotScript.InventorySlot);
                    break;
                case "Equip":
                    if (slotScript.InventorySlot.InstantiatedItem is InstantiatedEquipment) {
                        playerManagerClient.UnitController.CharacterEquipmentManager.RequestEquip(slotScript.InventorySlot.InstantiatedItem as InstantiatedEquipment);
                    } else if (slotScript.InventorySlot.InstantiatedItem is InstantiatedBag) {
                        playerManagerClient.UnitController.CharacterInventoryManager.RequestEquipBagFromSlot(slotScript.InventorySlot.InstantiatedItem as InstantiatedBag, slotScript.InventorySlot, false);
                    }
                    break;
                case "Bank":
                    playerManagerClient.UnitController.CharacterInventoryManager.RequestMoveFromInventoryToBank(slotScript.InventorySlot);
                    break;
                case "Store":
                    playerManagerClient.UnitController.CharacterInventoryManager.RequestMoveItemToStorageContainer(storageContainerManagerClient.StorageContainerComponent, slotScript.InventorySlot, false);
                    break;
                case "Sell":
                    if (uIManager.vendorWindow.IsOpen == true) {
                        if (slotScript.InventorySlot.InstantiatedItem.ItemQuality != null && slotScript.InventorySlot.InstantiatedItem.ItemQuality.RequireSellConfirmation) {
                            vendorManagerClient.SetSellItem(slotScript.InventorySlot.InstantiatedItem);
                            uIManager.confirmSellItemMenuWindow.OpenWindow();
                        } else {
                            vendorManagerClient.RequestSellItemToVendor(slotScript.InventorySlot.InstantiatedItem);
                        }
                    }
                    break;
                case "Mail":
                    mailboxManagerClient.RequestAddAttachment(slotScript.InventorySlot);
                    break;
                case "Trade":
                    tradeServiceClient.RequestAddItemsToTrade(slotScript.InventorySlot);
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
                    playerManagerClient.UnitController.CharacterInventoryManager.RequestUnequipBag(bagButton.BagNode.InstantiatedBag, false);
                    break;
            }
        }


    }

}