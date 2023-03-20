using AnyRPG;
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

        public override void OnPointerClick(PointerEventData eventData) {
            //Debug.Log("SlotScript.OnPointerClick()");
            base.OnPointerClick(eventData);
            // Detect a left click on a slot in a bag
            if (eventData.button == PointerEventData.InputButton.Left) {
                HandleLeftClick();
            }
            if (eventData.button == PointerEventData.InputButton.Right) {
                HandleRightClick();
            }
        }

        private void DropItemFromNonInventorySlot() {
            // item comes from somewhere else, like bag bar or character panel
        }

        public void SendItemToHandScript() {
            //Debug.Log("SlotScript.SendItemToHandScript(): setting inventorymanager.myinstance.fromslot to this");
            handScript.TakeMoveable(inventorySlot.Item);
            playerManager.MyCharacter.CharacterInventoryManager.FromSlot = this;
            if (controlsManager.GamePadModeActive == true) {
                handScript.SetPosition(transform.position);
            }
        }

        public void DropItemFromInventorySlot() {
            //Debug.Log("Dropping an item from an inventory slot");
            if (PutItemBack()
                || inventorySlot.MergeItems(playerManager.MyCharacter.CharacterInventoryManager.FromSlot.InventorySlot)
                || inventorySlot.SwapItems(playerManager.MyCharacter.CharacterInventoryManager.FromSlot.InventorySlot)
                || inventorySlot.AddItems(playerManager.MyCharacter.CharacterInventoryManager.FromSlot.InventorySlot.Items)) {
                handScript.Drop();
                playerManager.MyCharacter.CharacterInventoryManager.FromSlot = null;
            }
        }
        public void HandleLeftClick() {
            // we have something to move and it came from the inventory, therefore we are trying to drop something from this slot or another slot onto this slot
            if (playerManager.MyCharacter.CharacterInventoryManager.FromSlot != null) {
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
                if (handScript.Moveable is Bag) {
                    // the handscript has a bag in it
                    if (inventorySlot.Item is Bag) {
                        // This slot also has a bag in it, so swap the 2 bags
                        playerManager.MyCharacter.CharacterInventoryManager.SwapBags(handScript.Moveable as Bag, inventorySlot.Item as Bag);
                    }
                } else if (handScript.Moveable is Equipment) {
                    // the handscript has equipment in it
                    if (inventorySlot.Item is Equipment && (inventorySlot.Item as Equipment).EquipmentSlotType == (handScript.Moveable as Equipment).EquipmentSlotType) {
                        // this slot has equipment in it, and the equipment matches the slot of the item in the handscript.  swap them
                        EquipmentSlotProfile equipmentSlotProfile = playerManager.MyCharacter.CharacterEquipmentManager.FindEquipmentSlotForEquipment(handScript.Moveable as Equipment);
                        playerManager.MyCharacter.CharacterEquipmentManager.Unequip(equipmentSlotProfile);
                        playerManager.MyCharacter.CharacterEquipmentManager.Equip(inventorySlot.Item as Equipment, equipmentSlotProfile);
                        playerManager.UnitController.UnitModelController.RebuildModelAppearance();
                        inventorySlot.Item.Remove();
                       //UseItem();
                        //uIManager.RefreshTooltip();
                        handScript.Drop();
                    }
                }

            } else {
                // This slot has nothing in it, and we are not trying to transfer anything to it from another slot in the bag
                if (handScript.Moveable is Bag) {
                    //Debug.Log("SlotScript.HandleLeftClick(): We are trying to drop a bag into the inventory.");
                    // the handscript had a bag in it, and therefore we are trying to unequip a bag
                    Bag bag = (Bag)handScript.Moveable;
                    if (playerManager.MyCharacter.CharacterInventoryManager.EmptySlotCount(bag.BagNode.IsBankNode) - bag.Slots > 0) {
                        //if (playerManager.MyCharacter.CharacterInventoryManager.EmptySlotCount() - bag.Slots > 0) {
                        //Debug.Log("SlotScript.HandleLeftClick(): We are trying to drop a bag into the inventory. There is enough empty space.");
                        inventorySlot.AddItem(bag);
                        playerManager.MyCharacter.CharacterInventoryManager.RemoveBag(bag);
                        handScript.Drop();
                    }
                } else if (handScript.Moveable is Equipment) {
                    // the handscript had equipment in it, and therefore we are trying to unequip some equipment
                    Equipment equipment = (Equipment)handScript.Moveable;
                    EquipmentSlotProfile equipmentSlotProfile = playerManager.MyCharacter.CharacterEquipmentManager.FindEquipmentSlotForEquipment(handScript.Moveable as Equipment);
                    playerManager.MyCharacter.CharacterEquipmentManager.Unequip(equipmentSlotProfile, inventorySlot.GetCurrentSlotIndex());
                    playerManager.UnitController.UnitModelController.RebuildModelAppearance();
                    handScript.Drop();
                }
            }
        }

        public void HandleRightClick() {
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
                List<Item> moveList = new List<Item>();
                //Debug.Log("SlotScript.InteractWithSlot(): interacting with item in bank");
                foreach (Item item in inventorySlot.Items) {
                    moveList.Add(item);
                }
                foreach (Item item in moveList) {
                    if (playerManager.MyCharacter.CharacterInventoryManager.AddItem(item, false, false)) {
                        inventorySlot.RemoveItem(item);
                    }
                }
                return;
            }

            // send items back and forth between inventory and bank if they are both open
            if (uIManager.inventoryWindow.IsOpen == true && uIManager.bankWindow.IsOpen == true) {
                List<Item> moveList = new List<Item>();
                if (BagPanel is InventoryPanel) {
                    //Debug.Log("SlotScript.InteractWithSlot(): interacting with item in inventory");
                    /*
                    if (inventoryManager.AddItem(MyItem, true)) {
                        Clear();
                    }
                    */
                    foreach (Item item in inventorySlot.Items) {
                        moveList.Add(item);
                    }
                    foreach (Item item in moveList) {
                        if (playerManager.MyCharacter.CharacterInventoryManager.AddItem(item, true, false)) {
                            inventorySlot.RemoveItem(item);
                        }
                    }
                } /*else {
                    Debug.Log("SlotScript.InteractWithSlot(): We clicked on something in a chest or bag");
                }*/
                // default case to prevent using an item when the bank window is open but bank was full
                return;
            } else if (uIManager.inventoryWindow.IsOpen == true && uIManager.bankWindow.IsOpen == false && uIManager.vendorWindow.IsOpen) {
                // SELL THE ITEM
                if (inventorySlot.Item != null) {
                    if (inventorySlot.Item.ItemQuality != null && inventorySlot.Item.ItemQuality.RequireSellConfirmation) {
                        uIManager.confirmSellItemMenuWindow.OpenWindow();
                        (uIManager.confirmSellItemMenuWindow.CloseableWindowContents as ConfirmSellItemPanelController).MyItem = inventorySlot.Item;
                        return;
                    }
                    if ((uIManager.vendorWindow.CloseableWindowContents as VendorUI).SellItem(inventorySlot.Item)) {
                        return;
                    }

                }
                // default case to prevent using an item when the vendor window is open
                return;
            }

            // if we got to here, nothing left to do but use the item
            inventorySlot.UseItem();
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

            if (inventorySlot.Item == null) {
                return;
            }

            if (inventorySlot.Item is Bag) {
                if (BagPanel is BankPanel) {
                    if ((BagPanel as BankPanel).BagBarController.FreeBagSlots == 0) {
                        messageFeedManager.WriteMessage("There are no free bank bag slots");
                        return;
                    }
                    playerManager.MyCharacter.CharacterInventoryManager.AddBankBag(inventorySlot.Item as Bag);
                    inventorySlot.Item.Remove();
                }
                if (BagPanel is InventoryPanel) {
                    if ((BagPanel as InventoryPanel).BagBarController.FreeBagSlots == 0) {
                        messageFeedManager.WriteMessage("There are no free inventory bag slots");
                        return;
                    }
                    playerManager.MyCharacter.CharacterInventoryManager.AddInventoryBag(inventorySlot.Item as Bag);
                    inventorySlot.Item.Remove();
                }
                return;
            }

            if (inventorySlot.Item is IUseable) {
                actionBarManager.StartUseableAssignment(inventorySlot.Item as IUseable);
                uIManager.assignToActionBarsWindow.OpenWindow();
            }
        }

        public override void JoystickButton3() {
            //Debug.Log("SlotScript.JoystickButton3()");
            base.JoystickButton3();

            if (handScript.Moveable != null) {
                return;
            }

            if (inventorySlot.Item == null) {
                return;
            }

            if (BagPanel is BankPanel) {
                if (inventorySlot.Item is Bag) {
                    // if Y button pressed and this is a bag in the bank, equip in the inventory
                    if ((uIManager.inventoryWindow.CloseableWindowContents as InventoryPanel).BagBarController.FreeBagSlots == 0) {
                        messageFeedManager.WriteMessage("There are no free inventory bag slots");
                        return;
                    }
                    playerManager.MyCharacter.CharacterInventoryManager.AddInventoryBag(inventorySlot.Item as Bag);
                    inventorySlot.Item.Remove();
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
                if (playerManager.MyCharacter.CharacterInventoryManager.FromSlot != null) {
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

                if (inventorySlot.Item == null) {
                    uIManager.HideToolTip();
                    owner.HideControllerHints();
                    return;
                }
                ShowGamepadTooltip();


                if (BagPanel is BankPanel) {
                    if (inventorySlot.Item is Bag) {
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

                if (inventorySlot.Item is Equipment) {
                    owner.SetControllerHints("Equip", "", "Drop", "", "", "Reorder");
                } else if (inventorySlot.Item is IUseable) {
                    if (inventorySlot.Item is Bag) {
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
                playerManager.MyCharacter.CharacterInventoryManager.OnItemCountChanged(tmpItem);
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
            if (playerManager.MyCharacter.CharacterInventoryManager.FromSlot == this) {
                //Debug.Log("Confirmed that the item came from this slot.  now returning it.");
                UpdateSlot();
                return true;
            } else {
                //Debug.Log("The item did not come from this slot.");
            }
            return false;
        }

        /*
        private bool SwapItems(SlotScript from) {
            //Debug.Log("SlotScript " + this.GetInstanceID().ToString() + " receiving items to swap from slotscript " + from.GetInstanceID().ToString());
            // use a temporary list to swap references to the stacks
            List<Item> tmpFrom = new List<Item>(from.Items);
            from.Items = Items;
            Items = tmpFrom;

            return true;
        }
        */

        /*
        private bool MergeItems(SlotScript from) {
            //Debug.Log("attempting to merge items");
            if (IsEmpty) {
                //Debug.Log("This slot is empty, there is nothing to merge.");
                return false;
            }
            if (SystemDataUtility.MatchResource(from.Item.DisplayName, Item.DisplayName) && !IsFull) {
                // how many free slots there are in the new stack
                int free = Item.MaximumStackSize - Count;
                if (free >= from.Count) {
                    int maxCount = from.Count;
                    for (int i = 0; i < maxCount; i++) {
                        AddItem(from.Items[0]);
                        from.RemoveItem(from.Items[0]);
                    }
                    return true;
                } else {
                    //Debug.Log("There is not enough space in this slot to merge items.");
                }

            }
            return false;
        }
        */

        /// <summary>
        /// Updates the Stack Size count graphic
        /// </summary>
        private void UpdateSlot() {
            //Debug.Log("SlotScript.UpdateSlot(): Update Slot called on slot " + GetInstanceID().ToString() + "; MyItem: " + (MyItem != null ? MyItem.DisplayName : "null"));
            if (inventorySlot == null) {
                // the inventory slot that this script was referencing no longer exists
                return;
            }

            SetDescribable(inventorySlot.Item);
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
            if (inventorySlot?.Item == null) {
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
                uIManager.SetItemBackground(inventorySlot.Item, backGroundImage, new Color32(0, 0, 0, 255));

            }
            //Debug.Log($"{gameObject.name}.WindowContentController.SetBackGroundColor()");
        }

        
        public override void ShowToolTip() {
            //base.ShowToolTip
            //uIManager.ShowToolTip(transform.position, describable, "Sell Price: ");
            ShowGamepadTooltip();
        }

        public void ShowGamepadTooltip() {
            //Rect panelRect = RectTransformToScreenSpace((BagPanel.ContentArea as RectTransform));
            uIManager.ShowGamepadTooltip((BagPanel.ContentArea as RectTransform), transform, inventorySlot.Item, "Sell Price: ");
        }

        public override void OnSendObjectToPool() {
            //Debug.Log("SlotScript.OnSendObjectToPool()");
            // this is being called manually for now because if the bag is closed, the message will not be received

            base.OnSendObjectToPool();

            ClearInventorySlot();
        }
    }

}