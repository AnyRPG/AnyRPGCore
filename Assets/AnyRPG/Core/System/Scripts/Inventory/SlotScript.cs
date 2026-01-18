using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class SlotScript : DescribableIcon, IPointerClickHandler, IClickable {

        [Header("Slot Script")]

        [SerializeField]
        protected Image backGroundImage = null;

        protected InventorySlot inventorySlot = null;

        // game manager references
        //protected InventoryManager inventoryManager = null;
        protected HandScript handScript = null;
        protected PlayerManager playerManager = null;
        protected ActionBarManager actionBarManager = null;
        protected MessageFeedManager messageFeedManager = null;

        /// <summary>
        /// A reference to the bag that this slot belongs to
        /// </summary>
        public BagPanel BagPanel { get; set; }

        public InventorySlot InventorySlot { get => inventorySlot; }

        public override int Count {
            get {
                if (inventorySlot != null) {
                    return inventorySlot.Count;
                }
                return 0;
            }
        }

        public override bool CaptureCancelButton {
            get {
                if (handScript.Moveable != null) {
                    return true;
                }
                return base.CaptureCancelButton;
            }
        }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            //inventoryManager = systemGameManager.InventoryManager;
            handScript = systemGameManager.UIManager.HandScript;
            playerManager = systemGameManager.PlayerManager;
            actionBarManager = systemGameManager.UIManager.ActionBarManager;
            messageFeedManager = systemGameManager.UIManager.MessageFeedManager;
        }

        public void SetInventorySlot(InventorySlot inventorySlot) {
            //Debug.Log($"{GetInstanceID()}.SlotScript.SetInventorySlot()");

            this.inventorySlot = inventorySlot;
            inventorySlot.OnUpdateSlot += UpdateSlot;
        }

        
        public void ClearInventorySlot() {
            //Debug.Log("SlotScript.ClearInventorySlot()");

            if (inventorySlot != null) {
                inventorySlot.OnUpdateSlot -= UpdateSlot;
                ClearSlot();
            }
        }

        private void DropItemFromNonInventorySlot() {
            // item comes from somewhere else, like bag bar or character panel
        }

        public void SendItemToHandScript() {
            //Debug.Log("SlotScript.SendItemToHandScript(): setting inventorymanager.myinstance.fromslot to this");
            handScript.TakeMoveable(inventorySlot.InstantiatedItem);
            playerManager.UnitController.CharacterInventoryManager.FromSlot = this;
            if (controlsManager.GamePadModeActive == true) {
                handScript.SetPosition(transform.position);
            }
        }

        public void DropItemFromInventorySlot() {
            //Debug.Log("SlotScript.DropItemFromInventorySlot()");

            //Debug.Log("Dropping an item from an inventory slot");
            playerManager.UnitController.CharacterInventoryManager.RequestDropItemFromInventorySlot(playerManager.UnitController.CharacterInventoryManager.FromSlot.InventorySlot, inventorySlot, playerManager.UnitController.CharacterInventoryManager.FromSlot.BagPanel is InventoryPanel, BagPanel is InventoryPanel);
            handScript.Drop();

        }

        protected override void HandleLeftClick() {
            // we have something to move and it came from the inventory, therefore we are trying to drop something from this slot or another slot onto this slot
            if (playerManager.UnitController.CharacterInventoryManager.FromSlot != null) {
                DropItemFromInventorySlot();
                return;
            }


            if (!inventorySlot.IsEmpty) {
                // This slot has something in it, and the hand script is empty, so we are trying to pick it up
                if (handScript.Moveable == null) {
                    SendItemToHandScript();
                    return;
                }

                // the slot has something in it, and the handscript is not empty, so we are trying to swap with something
                if (handScript.Moveable is InstantiatedBag) {
                    // the handscript has a bag in it
                    if (inventorySlot.InstantiatedItem is InstantiatedBag) {
                        // This slot also has a bag in it, so swap the 2 bags
                        playerManager.UnitController.CharacterInventoryManager.RequestSwapBags(handScript.Moveable as InstantiatedBag, inventorySlot.InstantiatedItem as InstantiatedBag);
                        handScript.Drop();
                    }
                } else if (handScript.Moveable is InstantiatedEquipment) {
                    // the handscript has equipment in it
                    if (inventorySlot.InstantiatedItem is InstantiatedEquipment && (inventorySlot.InstantiatedItem as InstantiatedEquipment).Equipment.EquipmentSlotType == (handScript.Moveable as InstantiatedEquipment).Equipment.EquipmentSlotType) {
                        // this slot has equipment in it, and the equipment matches the slot of the item in the handscript.  swap them
                        playerManager.UnitController.CharacterEquipmentManager.RequestSwapInventoryEquipment(handScript.Moveable as InstantiatedEquipment, inventorySlot.InstantiatedItem as InstantiatedEquipment);
                        handScript.Drop();
                    }
                }

            } else {
                // This slot has nothing in it, and we are not trying to transfer anything to it from another slot in the bag
                if (handScript.Moveable is InstantiatedBag) {
                    //Debug.Log("SlotScript.HandleLeftClick(): We are trying to drop a bag into the inventory.");
                    // the handscript had a bag in it, and therefore we are trying to unequip a bag
                    InstantiatedBag instantiatedBag = (InstantiatedBag)handScript.Moveable;
                    if (playerManager.UnitController.CharacterInventoryManager.EmptySlotCount(instantiatedBag.BagNode.IsBankNode) - instantiatedBag.Slots > 0) {
                        //if (playerManager.UnitController.CharacterInventoryManager.EmptySlotCount() - bag.Slots > 0) {
                        //Debug.Log("SlotScript.HandleLeftClick(): We are trying to drop a bag into the inventory. There is enough empty space.");
                        playerManager.UnitController.CharacterInventoryManager.RequestUnequipBagToSlot(instantiatedBag, inventorySlot, BagPanel is BankPanel);
                        handScript.Drop();
                    }
                } else if (handScript.Moveable is InstantiatedEquipment) {
                    // the handscript had equipment in it, and therefore we are trying to unequip some equipment
                    playerManager.UnitController.CharacterEquipmentManager.RequestUnequipToSlot(handScript.Moveable as InstantiatedEquipment, inventorySlot.GetCurrentInventorySlotIndex(playerManager.UnitController));
                    handScript.Drop();
                }
            }
        }

        protected override void HandleRightClick() {
            //Debug.Log("SlotScript.HandleRightClick()");
            InteractWithSlot();
            ProcessMouseEnter();
        }

        public void InteractWithSlot() {
            //Debug.Log("SlotScript.InteractWithSlot()");

            // ignore right clicks when something is in the handscript
            if (handScript.Moveable != null) {
                return;
            }

            // send items back and forth between bank and inventory if they are both open
            if (BagPanel is BankPanel) {
                playerManager.UnitController.CharacterInventoryManager.RequestMoveFromBankToInventory(inventorySlot);
                return;
            }

            // send items back and forth between inventory and bank if they are both open
            if (uIManager.inventoryWindow.IsOpen == true && uIManager.bankWindow.IsOpen == true) {
                if (BagPanel is InventoryPanel) {
                    playerManager.UnitController.CharacterInventoryManager.RequestMoveFromInventoryToBank(inventorySlot);
                } /*else {
                    //Debug.Log("SlotScript.InteractWithSlot(): We clicked on something in a chest or bag");
                }*/
                return;
            } else if (uIManager.inventoryWindow.IsOpen == true && uIManager.bankWindow.IsOpen == false && uIManager.vendorWindow.IsOpen) {
                // SELL THE ITEM
                if (inventorySlot.InstantiatedItem != null) {
                    if (inventorySlot.InstantiatedItem.ItemQuality != null && inventorySlot.InstantiatedItem.ItemQuality.RequireSellConfirmation) {
                        uIManager.confirmSellItemMenuWindow.OpenWindow();
                        (uIManager.confirmSellItemMenuWindow.CloseableWindowContents as ConfirmSellItemPanel).MyItem = inventorySlot.InstantiatedItem;
                        return;
                    }
                    if ((uIManager.vendorWindow.CloseableWindowContents as VendorPanel).SellItem(inventorySlot.InstantiatedItem)) {
                        return;
                    }

                }
                // default case to prevent using an item when the vendor window is open
                return;
            }

            // if we got to here, nothing left to do but use the item
            playerManager.UnitController.CharacterInventoryManager.RequestUseItem(inventorySlot);
        }

        public override void Accept() {
            base.Accept();
            InteractWithSlot();
        }

        public override void Cancel() {
            base.Cancel();
            if (handScript.Moveable != null) {
                handScript.Drop();
                ShowContextInfo();
            }
        }

        public override void JoystickButton2() {
            //Debug.Log("SlotScript.JoystickButton2()");
            base.JoystickButton2();

            if (handScript.Moveable != null) {
                return;
            }

            if (inventorySlot.InstantiatedItem == null) {
                return;
            }

            if (inventorySlot.InstantiatedItem is InstantiatedBag) {
                if (BagPanel is BankPanel) {
                    if ((BagPanel as BankPanel).BagBarController.FreeBagSlots == 0) {
                        messageFeedManager.WriteMessage("There are no free bank bag slots");
                        return;
                    }
                    playerManager.UnitController.CharacterInventoryManager.AddBankBag(inventorySlot.InstantiatedItem as InstantiatedBag);
                    inventorySlot.InstantiatedItem.Remove();
                }
                if (BagPanel is InventoryPanel) {
                    if ((BagPanel as InventoryPanel).BagBarController.FreeBagSlots == 0) {
                        messageFeedManager.WriteMessage("There are no free inventory bag slots");
                        return;
                    }
                    playerManager.UnitController.CharacterInventoryManager.AddInventoryBag(inventorySlot.InstantiatedItem as InstantiatedBag);
                    inventorySlot.InstantiatedItem.Remove();
                }
                return;
            }

            if (inventorySlot.InstantiatedItem is IUseable) {
                actionBarManager.StartUseableAssignment(inventorySlot.InstantiatedItem as IUseable);
                uIManager.assignToActionBarsWindow.OpenWindow();
            }
        }

        public override void JoystickButton3() {
            //Debug.Log("SlotScript.JoystickButton3()");
            base.JoystickButton3();

            if (handScript.Moveable != null) {
                return;
            }

            if (inventorySlot.InstantiatedItem == null) {
                return;
            }

            if (BagPanel is BankPanel) {
                if (inventorySlot.InstantiatedItem is InstantiatedBag) {
                    // if Y button pressed and this is a bag in the bank, equip in the inventory
                    if ((uIManager.inventoryWindow.CloseableWindowContents as InventoryPanel).BagBarController.FreeBagSlots == 0) {
                        messageFeedManager.WriteMessage("There are no free inventory bag slots");
                        return;
                    }
                    playerManager.UnitController.CharacterInventoryManager.AddInventoryBag(inventorySlot.InstantiatedItem as InstantiatedBag);
                    inventorySlot.InstantiatedItem.Remove();
                }
                return;
            }
            if (BagPanel is InventoryPanel) {
                // drop item
                handScript.SetPosition(transform.position);
                SendItemToHandScript();
                uIManager.confirmDestroyMenuWindow.OpenWindow();
            }
        }

        public override void JoystickButton9() {
            base.JoystickButton9();

            if (controlsManager.GamePadModeActive == true && handScript.Moveable != null) {
                if (playerManager.UnitController.CharacterInventoryManager.FromSlot != null) {
                    DropItemFromInventorySlot();
                    ShowContextInfo();
                    return;
                }
            }

            if (!inventorySlot.IsEmpty) {
                // This slot has something in it, and the hand script is empty, so we are trying to pick it up
                if (handScript.Moveable == null) {
                    SendItemToHandScript();
                    ShowContextInfo();
                    return;
                }
            }
        }

        public override void Select() {
            //Debug.Log("SlotScript.Select()");
            base.Select();

            if (controlsManager.GamePadModeActive == true && handScript.Moveable != null) {
                handScript.SetPosition(transform.position);
            }

            ShowContextInfo();
        }

        /// <summary>
        /// show or hide the appropriate tooltip and controller hints
        /// </summary>
        private void ShowContextInfo() {
            if (owner != null) {

                if (handScript.Moveable != null) {
                    owner.SetControllerHints("", "", "", "Cancel Reorder", "Move", "Place");
                    return;
                }

                if (inventorySlot.InstantiatedItem == null) {
                    uIManager.HideToolTip();
                    owner.HideControllerHints();
                    return;
                }
                ShowGamepadTooltip();


                if (BagPanel is BankPanel) {
                    if (inventorySlot.InstantiatedItem is InstantiatedBag) {
                        owner.SetControllerHints("Move To Inventory", "Equip In Bank", "Equip In Inventory", "", "", "Reorder");
                    } else {
                        owner.SetControllerHints("Move To Inventory", "", "", "", "", "Reorder");
                    }
                    return;
                }

                if (uIManager.inventoryWindow.IsOpen == true && uIManager.bankWindow.IsOpen == true) {
                    if (BagPanel is InventoryPanel) {
                        // move to bank
                        owner.SetControllerHints("Move To Bank", "", "Drop", "", "", "Reorder");
                    }
                    // default case to prevent using an item when the bank window is open but bank was full
                    return;
                } else if (uIManager.inventoryWindow.IsOpen == true && uIManager.bankWindow.IsOpen == false && uIManager.vendorWindow.IsOpen) {
                    // SELL THE ITEM
                    owner.SetControllerHints("Sell", "", "Drop", "", "", "Reorder");
                    // default case to prevent using an item when the vendor window is open
                    return;
                }

                if (inventorySlot.InstantiatedItem is InstantiatedEquipment) {
                    owner.SetControllerHints("Equip", "", "Drop", "", "", "Reorder");
                } else if (inventorySlot.InstantiatedItem is IUseable) {
                    if (inventorySlot.InstantiatedItem is InstantiatedBag) {
                        owner.SetControllerHints("Use", "Equip", "Drop", "", "", "Reorder");
                    } else {
                        owner.SetControllerHints("Use", "Add To Action Bars", "Drop", "", "", "Reorder");
                    }
                }
            }
        }



        public override void DeSelect() {
            //Debug.Log("SlotScript.DeSelect()");
            base.DeSelect();
            if (owner != null) {
                owner.HideControllerHints();
            }
            uIManager.HideToolTip();
        }



        /*
        public void Clear() {
            if (Items.Count > 0) {
                Item tmpItem = Items[0];
                Items.Clear();
                playerManager.UnitController.CharacterInventoryManager.OnItemCountChanged(tmpItem);
                UpdateSlot();
            }
        }
        */

        /*
        /// <summary>
        /// Uses the item if it is useable
        /// </summary>
        public void UseItem() {
            //Debug.Log("SlotScript.HandleRightClick()");
            if (Item is IUseable) {
                (Item as IUseable).Use();
            } else if (Item is Equipment) {
                (Item as Equipment).Use();
            }
        }
        */

        /*
        public bool StackItem(Item item) {
            if (!IsEmpty && item.DisplayName == Item.DisplayName && Items.Count < Item.MaximumStackSize) {
                Items.Add(item);
                UpdateSlot();
                item.Slot = this;
                return true;
            }
            return false;
        }
        */

        public bool PutItemBack() {
            //Debug.Log("attempting to put an item back in a slot");
            if (playerManager.UnitController.CharacterInventoryManager.FromSlot == this) {
                //Debug.Log("Confirmed that the item came from this slot.  now returning it.");
                UpdateSlot();
                return true;
            } else {
                //Debug.Log("The item did not come from this slot.");
            }
            return false;
        }

        /// <summary>
        /// Updates the Stack Size count graphic
        /// </summary>
        private void UpdateSlot() {
            //Debug.Log($"SlotScript.UpdateSlot(): Update Slot called on slot {GetInstanceID().ToString()}");

            if (inventorySlot == null) {
                // the inventory slot that this script was referencing no longer exists
                return;
            }

            SetDescribable(inventorySlot.InstantiatedItem?.Item);
            uIManager.UpdateStackSize(this, Count);
            SetBackGroundColor();

            if (selected) {
                ShowContextInfo();
            }
        }

        private void ClearSlot() {
            inventorySlot = null;
            SetDescribable(null, 0);
            uIManager.UpdateStackSize(this, Count);
            SetBackGroundColor();
        }

        public void SetBackGroundColor() {
            Color finalColor;
            if (inventorySlot?.InstantiatedItem == null) {
                int slotOpacityLevel = (int)(PlayerPrefs.GetFloat("InventorySlotOpacity") * 255);
                finalColor = new Color32(0, 0, 0, (byte)slotOpacityLevel);
                backGroundImage.sprite = null;
                if (backGroundImage != null) {
                    //Debug.Log($"{gameObject.name}.WindowContentController.SetBackGroundColor(): background image is not null, setting color: " + slotOpacityLevel);
                    backGroundImage.color = finalColor;
                } else {
                    //Debug.Log($"{gameObject.name}.WindowContentController.SetBackGroundColor(): background image IS NULL!");
                }
            } else {
                // check if the item has a quality.  if not, just do the default color
                uIManager.SetItemBackground(inventorySlot.InstantiatedItem.Item, backGroundImage, new Color32(0, 0, 0, 255), inventorySlot.InstantiatedItem.ItemQuality);

            }
            //Debug.Log($"{gameObject.name}.WindowContentController.SetBackGroundColor()");
        }

        
        public override void ShowToolTip() {
            //base.ShowToolTip
            //uIManager.ShowToolTip(transform.position, describable, "Sell Price: ");
            ShowGamepadTooltip();
        }

        public void ShowGamepadTooltip() {
            //Debug.Log($"SlotScript.ShowGamepadTooltip() bagPanel: {(BagPanel == null ? "null" : "not null")} inventoryslot: {(inventorySlot.InstantiatedItem == null ? "null" : "not null")}");

            uIManager.ShowGamepadTooltip((BagPanel.ContentArea as RectTransform), transform, inventorySlot.InstantiatedItem, "Sell Price: ");
        }

        public override void OnSendObjectToPool() {
            //Debug.Log("SlotScript.OnSendObjectToPool()");
            // this is being called manually for now because if the bag is closed, the message will not be received

            base.OnSendObjectToPool();

            ClearInventorySlot();
        }
    }

}