using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class InventorySlot : ConfiguredClass {

        public event System.Action OnUpdateSlot = delegate { };

        /// <summary>
        /// A stack for all items on this slot
        /// </summary>
        protected List<Item> items = new List<Item>();

        // game manager references
        protected HandScript handScript = null;
        protected PlayerManager playerManager = null;

        /// <summary>
        /// A referecne to the bag that this slot belongs to
        /// </summary>
        public BagPanel BagPanel { get; set; }

        public bool IsEmpty {
            get {
                return Items.Count == 0;
            }
        }

        public bool IsFull {
            get {
                if (IsEmpty || Count < Item.MaximumStackSize) {
                    return false;
                }
                return true;
            }
        }

        public Item Item {
            get {
                if (!IsEmpty) {
                    return Items[0];
                }
                return null;
            }
        }

        public int Count { get => Items.Count; }
        public List<Item> Items {
            get {
                return items;
            }
            set {
                items = value;
                UpdateSlot();
            }
        }

        public InventorySlot(SystemGameManager systemGameManager) {
            Configure(systemGameManager);
        }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            //inventoryManager = systemGameManager.InventoryManager;
            handScript = systemGameManager.UIManager.HandScript;
            playerManager = systemGameManager.PlayerManager;
        }

        private void UpdateSlot() {
            SetSlotOnItems();
            OnUpdateSlot();
        }

        private void SetSlotOnItems() {
            //Debug.Log("SlotScript.SetSlotOnItems(): MyItem is null");
            foreach (Item tmpItem in Items) {
                //Debug.Log("SlotScript.SetSlotOnItems(): going through MyItems");
                tmpItem.Slot = this;
            }
        }

        public bool AddItem(Item item) {
            //Debug.Log("Slot " + GetInstanceID().ToString() + " with count " + MyItems.Count.ToString() + " adding item " + item.GetInstanceID().ToString());
            Items.Add(item);
            UpdateSlot();
            //Debug.Log("Slot " + GetInstanceID().ToString() + " now has count " + MyItems.Count.ToString());
            return true;
        }

        public bool AddItems(List<Item> newItems) {
            if (IsEmpty || SystemDataFactory.MatchResource(newItems[0].DisplayName, Item.DisplayName)) {
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
            if (!IsEmpty) {
                Items.Remove(item);
                UpdateSlot();
                playerManager.MyCharacter.CharacterInventoryManager.OnItemCountChanged(item);
            }
        }

        /*
        public void DropItemFromInventorySlot() {
            //Debug.Log("Dropping an item from an inventory slot");
            if (PutItemBack() || MergeItems(playerManager.MyCharacter.CharacterInventoryManager.FromSlot.InventorySlot) || SwapItems(playerManager.MyCharacter.CharacterInventoryManager.FromSlot.InventorySlot) || AddItems(playerManager.MyCharacter.CharacterInventoryManager.FromSlot.InventorySlot.Items)) {
                handScript.Drop();
                playerManager.MyCharacter.CharacterInventoryManager.FromSlot = null;
            }
        }
        */

        private void DropItemFromNonInventorySlot() {
            // item comes from somewhere else, like bag bar or character panel
        }

        /*
        public void SendItemToHandScript() {
            //Debug.Log("SlotScript.SendItemToHandScript(): setting inventorymanager.myinstance.fromslot to this");
            handScript.TakeMoveable(Item as IMoveable);
            playerManager.MyCharacter.CharacterInventoryManager.FromSlot = this;
        }
        */

        public int GetCurrentSlotIndex() {
            for (int i = 0; i < playerManager.MyCharacter.CharacterInventoryManager.InventorySlots.Count; i++) {
                if (playerManager.MyCharacter.CharacterInventoryManager.InventorySlots[i] == this) {
                    return i;
                }
            }

            // didn't find anything, this will send whatever needs this to the default slot
            return -1;
        }

        public void Clear() {
            if (Items.Count > 0) {
                Item tmpItem = Items[0];
                Items.Clear();
                playerManager.MyCharacter.CharacterInventoryManager.OnItemCountChanged(tmpItem);
                UpdateSlot();
            }
        }

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

        public bool StackItem(Item item) {
            if (!IsEmpty && item.DisplayName == Item.DisplayName && Items.Count < Item.MaximumStackSize) {
                Items.Add(item);
                UpdateSlot();
                item.Slot = this;
                return true;
            }
            return false;
        }

        /*
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
        */

        public bool SwapItems(InventorySlot from) {
            //Debug.Log("SlotScript " + this.GetInstanceID().ToString() + " receiving items to swap from slotscript " + from.GetInstanceID().ToString());
            // use a temporary list to swap references to the stacks
            List<Item> tmpFrom = new List<Item>(from.Items);
            from.Items = Items;
            Items = tmpFrom;

            return true;
        }

        public bool MergeItems(InventorySlot from) {
            //Debug.Log("attempting to merge items");
            if (IsEmpty) {
                //Debug.Log("This slot is empty, there is nothing to merge.");
                return false;
            }
            if (SystemDataFactory.MatchResource(from.Item.DisplayName, Item.DisplayName) && !IsFull) {
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

        


    }

}