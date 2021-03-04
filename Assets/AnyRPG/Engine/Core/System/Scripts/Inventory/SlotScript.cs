using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class SlotScript : DescribableIcon, IPointerClickHandler, IClickable {
        /// <summary>
        /// A stack for all items on this slot
        /// </summary>
        private List<Item> items = new List<Item>();

        [SerializeField]
        private Image backGroundImage;

        private bool localComponentsGotten = false;

        /// <summary>
        /// A referecne to the bag that this slot belongs to
        /// </summary>
        public BagPanel MyBag { get; set; }

        public bool IsEmpty {
            get {
                return MyItems.Count == 0;
            }
        }

        public bool IsFull {
            get {
                if (IsEmpty || MyCount < MyItem.MyMaximumStackSize) {
                    return false;
                }
                return true;
            }
        }

        public Item MyItem {
            get {
                if (!IsEmpty) {
                    return MyItems[0];
                }
                return null;
            }
        }

        public override int MyCount { get => MyItems.Count; }
        public List<Item> MyItems {
            get {
                return items;
            }
            set {
                items = value;
                UpdateSlot();
            }
        }

        protected override void Awake() {
            //Debug.Log("SlotScript.Awake()");
            base.Awake();
            GetLocalComponents();
        }

        public void GetLocalComponents() {
            if (localComponentsGotten == true) {
                return;
            }
            if (backGroundImage == null) {
                //Debug.Log(gameObject.name + "SlotScript.Awake(): background image is null, trying to get component");
                backGroundImage = GetComponent<Image>();
            }
            localComponentsGotten = true;
        }

        private void SetSlotOnItems() {
            //Debug.Log("SlotScript.SetSlotOnItems(): MyItem is null");
            foreach (Item tmpItem in MyItems) {
                //Debug.Log("SlotScript.SetSlotOnItems(): going through MyItems");
                tmpItem.MySlot = this;
            }
        }

        public bool AddItem(Item item) {
            //Debug.Log("Slot " + GetInstanceID().ToString() + " with count " + MyItems.Count.ToString() + " adding item " + item.GetInstanceID().ToString());
            MyItems.Add(item);
            UpdateSlot();
            //Debug.Log("Slot " + GetInstanceID().ToString() + " now has count " + MyItems.Count.ToString());
            return true;
        }

        public bool AddItems(List<Item> newItems) {
            if (IsEmpty || SystemResourceManager.MatchResource(newItems[0].DisplayName, MyItem.DisplayName)) {
                int count = newItems.Count;

                for (int i = 0; i < count; i++) {
                    if (IsFull) {
                        return false;
                    }
                    AddItem(newItems[i]);
                    //newItems[0].Remove();
                }
                return true;
            }
            return false;
        }

        public void RemoveItem(Item item) {
            //Debug.Log("SlotScript.RemoveItem(" + item.MyName + ")");
            if (!IsEmpty) {
                //Debug.Log("slotscript getting ready to remove item: " + item.GetInstanceID().ToString() + "; MyItems count: " + MyItems.Count.ToString());
                MyItems.Remove(item);
                UpdateSlot();
                InventoryManager.MyInstance.OnItemCountChanged(item);
            }
        }

        public void OnPointerClick(PointerEventData eventData) {
            //Debug.Log("SlotScript.OnPointerClick()");

            // Detect a left click on a slot in a bag
            if (eventData.button == PointerEventData.InputButton.Left) {
                HandleLeftClick();
            }
            if (eventData.button == PointerEventData.InputButton.Right) {
                HandleRightClick();
            }
        }

        private void DropItemFromInventorySlot() {
            //Debug.Log("Dropping an item from an inventory slot");
            if (PutItemBack() || MergeItems(InventoryManager.MyInstance.FromSlot) || SwapItems(InventoryManager.MyInstance.FromSlot) || AddItems(InventoryManager.MyInstance.FromSlot.MyItems)) {
                HandScript.MyInstance.Drop();
                InventoryManager.MyInstance.FromSlot = null;
            }
        }

        private void DropItemFromNonInventorySlot() {
            // item comes from somewhere else, like bag bar or character panel
        }

        public void SendItemToHandScript() {
            //Debug.Log("SlotScript.SendItemToHandScript(): setting inventorymanager.myinstance.fromslot to this");
            HandScript.MyInstance.TakeMoveable(MyItem as IMoveable);
            InventoryManager.MyInstance.FromSlot = this;
        }

        public void HandleLeftClick() {
            // we have something to move and it came from the inventory, therefore we are trying to drop something from this slot or another slot onto this slot
            if (InventoryManager.MyInstance.FromSlot != null) {
                DropItemFromInventorySlot();
                return;
            }


            if (!IsEmpty) {
                // This slot has something in it, and the hand script is empty, so we are trying to pick it up
                if (HandScript.MyInstance.MyMoveable == null) {
                    SendItemToHandScript();
                    return;
                }

                // the slot has something in it, and the handscript is not empty, so we are trying to swap with something
                if (HandScript.MyInstance.MyMoveable is Bag) {
                    // the handscript has a bag in it
                    if (MyItem is Bag) {
                        // This slot also has a bag in it, so swap the 2 bags
                        InventoryManager.MyInstance.SwapBags(HandScript.MyInstance.MyMoveable as Bag, MyItem as Bag);
                    }
                } else if (HandScript.MyInstance.MyMoveable is Equipment) {
                    // the handscript has equipment in it
                    if (MyItem is Equipment && (MyItem as Equipment).EquipmentSlotType == (HandScript.MyInstance.MyMoveable as Equipment).EquipmentSlotType) {
                        // this slot has equipment in it, and the equipment matches the slot of the item in the handscript.  swap them
                        EquipmentSlotProfile equipmentSlotProfile = PlayerManager.MyInstance.MyCharacter.CharacterEquipmentManager.FindEquipmentSlotForEquipment(HandScript.MyInstance.MyMoveable as Equipment);
                        PlayerManager.MyInstance.MyCharacter.CharacterEquipmentManager.Unequip(equipmentSlotProfile);
                        PlayerManager.MyInstance.MyCharacter.CharacterEquipmentManager.Equip(MyItem as Equipment, equipmentSlotProfile);
                        MyItem.Remove();
                       //UseItem();
                        //UIManager.MyInstance.RefreshTooltip();
                        HandScript.MyInstance.Drop();
                    }
                }

            } else {
                // This slot has nothing in it, and we are not trying to transfer anything to it from another slot in the bag
                if (HandScript.MyInstance.MyMoveable is Bag) {
                    //Debug.Log("SlotScript.HandleLeftClick(): We are trying to drop a bag into the inventory.");
                    // the handscript had a bag in it, and therefore we are trying to unequip a bag
                    Bag bag = (Bag)HandScript.MyInstance.MyMoveable;
                    if (bag.MyBagPanel != MyBag && InventoryManager.MyInstance.EmptySlotCount() - bag.MySlots > 0) {
                        //Debug.Log("SlotScript.HandleLeftClick(): We are trying to drop a bag into the inventory. There is enough empty space.");
                        AddItem(bag);
                        InventoryManager.MyInstance.RemoveBag(bag);
                        HandScript.MyInstance.Drop();
                    }
                } else if (HandScript.MyInstance.MyMoveable is Equipment) {
                    // the handscript had equipment in it, and therefore we are trying to unequip some equipment
                    Equipment equipment = (Equipment)HandScript.MyInstance.MyMoveable;
                    // probably don't need to do this, since dequip should drop the equipment in the bag anyway
                    //AddItem(equipment);

                    //CharacterPanel.MyInstance.MySelectedButton.DequipEquipment(GetCurrentSlotIndex());
                    EquipmentSlotProfile equipmentSlotProfile = PlayerManager.MyInstance.MyCharacter.CharacterEquipmentManager.FindEquipmentSlotForEquipment(HandScript.MyInstance.MyMoveable as Equipment);
                    PlayerManager.MyInstance.MyCharacter.CharacterEquipmentManager.Unequip(equipmentSlotProfile, GetCurrentSlotIndex());
                    HandScript.MyInstance.Drop();
                }
            }
        }

        public int GetCurrentSlotIndex() {
            List<SlotScript> inventorySlots = InventoryManager.MyInstance.GetSlots();
            for (int i = 0; i < inventorySlots.Count; i++) {
                if (inventorySlots[i] == this) {
                    return i;
                }
            }

            // didn't find anything, this will send whatever needs this to the default slot
            return -1;
        }

        public void HandleRightClick() {
            //Debug.Log("SlotScript.HandleRightClick()");
            // ignore right clicks when something is in the handscript
            if (HandScript.MyInstance.MyMoveable != null) {
                return;
            }

            // DO SWAPITEMS CALL HERE - OR NOT BECAUSE THAT REQUIRES GETTING A SLOT FIRST

            // send items back and forth between inventory and bank if they are both open
            if (InventoryManager.MyInstance.BagsClosed() == false && InventoryManager.MyInstance.BankClosed() == false) {
                List<Item> moveList = new List<Item>();
                if (MyBag is BankPanel) {
                    //Debug.Log("SlotScript.HandleRightClick(): We clicked on something in a bank");
                    foreach (Item item in MyItems) {
                        moveList.Add(item);
                    }
                    foreach (Item item in moveList) {
                        if (InventoryManager.MyInstance.AddItem(item)) {
                            RemoveItem(item);
                        }
                    }
                } else if (MyBag is BagPanel) {
                    /*
                    if (InventoryManager.MyInstance.AddItem(MyItem, true)) {
                        Clear();
                    }
                    */
                    foreach (Item item in MyItems) {
                        moveList.Add(item);
                    }
                    foreach (Item item in moveList) {
                        if (InventoryManager.MyInstance.AddItem(item, true)) {
                            RemoveItem(item);
                        }
                    }
                } else {
                    //Debug.Log("SlotScript.HandleRightClick(): We clicked on something in a chest or bag");
                }
            } else if (InventoryManager.MyInstance.BagsClosed() == false && InventoryManager.MyInstance.BankClosed() == true && PopupWindowManager.MyInstance.vendorWindow.IsOpen) {
                // SELL THE ITEM
                if (MyItem != null) {
                    if (MyItem.ItemQuality != null && MyItem.ItemQuality.MyRequireSellConfirmation) {
                        SystemWindowManager.MyInstance.confirmSellItemMenuWindow.OpenWindow();
                        (SystemWindowManager.MyInstance.confirmSellItemMenuWindow.CloseableWindowContents as ConfirmSellItemPanelController).MyItem = MyItem;
                        return;
                    }
                    if ((PopupWindowManager.MyInstance.vendorWindow.CloseableWindowContents as VendorUI).SellItem(MyItem)) {
                        return;
                    }
                    
                }
            }

            // WHY ARE WE DOING THAT IF WE DIDN'T RETURN EARLIER AFTER PUTTING THINGS IN THE BANK?
            // if we got to here, nothing left to do but use the item

            UseItem();
        }

        public void Clear() {
            if (MyItems.Count > 0) {
                Item tmpItem = MyItems[0];
                MyItems.Clear();
                InventoryManager.MyInstance.OnItemCountChanged(tmpItem);
                UpdateSlot();
            }
        }

        /// <summary>
        /// Uses the item if it is useable
        /// </summary>
        public void UseItem() {
            //Debug.Log("SlotScript.HandleRightClick()");
            if (MyItem is IUseable) {
                (MyItem as IUseable).Use();
            } else if (MyItem is Equipment) {
                (MyItem as Equipment).Use();
                //CharacterPanel.MyInstance.EquipEquipment(MyItem as Equipment);
            }
        }

        public bool StackItem(Item item) {
            if (!IsEmpty && item.DisplayName == MyItem.DisplayName && MyItems.Count < MyItem.MyMaximumStackSize) {
                MyItems.Add(item);
                UpdateSlot();
                item.MySlot = this;
                return true;
            }
            return false;
        }

        public bool PutItemBack() {
            //Debug.Log("attempting to put an item back in a slot");
            if (InventoryManager.MyInstance.FromSlot == this) {
                //Debug.Log("Confirmed that the item came from this slot.  now returning it.");
                UpdateSlot();
                return true;
            } else {
                //Debug.Log("The item did not come from this slot.");
            }
            return false;
        }
        private bool SwapItems(SlotScript from) {
            //Debug.Log("SlotScript " + this.GetInstanceID().ToString() + " receiving items to swap from slotscript " + from.GetInstanceID().ToString());
            // use a temporary list to swap references to the stacks
            List<Item> tmpFrom = new List<Item>(from.MyItems);
            from.MyItems = MyItems;
            MyItems = tmpFrom;

            return true;
        }

        private bool MergeItems(SlotScript from) {
            //Debug.Log("attempting to merge items");
            if (IsEmpty) {
                //Debug.Log("This slot is empty, there is nothing to merge.");
                return false;
            }
            if (SystemResourceManager.MatchResource(from.MyItem.DisplayName, MyItem.DisplayName) && !IsFull) {
                // how many free slots there are in the new stack
                int free = MyItem.MyMaximumStackSize - MyCount;
                if (free >= from.MyCount) {
                    int maxCount = from.MyCount;
                    for (int i = 0; i < maxCount; i++) {
                        AddItem(from.MyItems[0]);
                        from.RemoveItem(from.MyItems[0]);
                    }
                    return true;
                } else {
                    //Debug.Log("There is not enough space in this slot to merge items.");
                }

            }
            return false;
        }

        /// <summary>
        /// Updates the Stack Size count graphic
        /// </summary>
        private void UpdateSlot() {
            //Debug.Log("SlotScript.UpdateSlot(): Update Slot called on slot " + GetInstanceID().ToString() + "; MyItem: " + (MyItem != null ? MyItem.MyName : "null"));
            if (MyItem != null) {
                SetSlotOnItems();
            }
            SetDescribable(MyItem);
            UIManager.MyInstance.UpdateStackSize(this, MyCount);
            SetBackGroundColor();
        }

        public void SetBackGroundColor() {
            GetLocalComponents();
            Color finalColor;
            if (MyItem == null) {
                int slotOpacityLevel = (int)(PlayerPrefs.GetFloat("InventorySlotOpacity") * 255);
                finalColor = new Color32(0, 0, 0, (byte)slotOpacityLevel);
                backGroundImage.sprite = null;
                if (backGroundImage != null) {
                    //Debug.Log(gameObject.name + ".WindowContentController.SetBackGroundColor(): background image is not null, setting color: " + slotOpacityLevel);
                    backGroundImage.color = finalColor;
                } else {
                    //Debug.Log(gameObject.name + ".WindowContentController.SetBackGroundColor(): background image IS NULL!");
                }
            } else {
                // check if the item has a quality.  if not, just do the default color
                UIManager.MyInstance.SetItemBackground(MyItem, backGroundImage, new Color32(0, 0, 0, 255));

            }
            //Debug.Log(gameObject.name + ".WindowContentController.SetBackGroundColor()");
        }

        public override void ShowToolTip(IDescribable describable) {
            UIManager.MyInstance.ShowToolTip(transform.position, describable, "Sell Price: ");
        }


    }

}