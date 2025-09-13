using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class InventorySlot : ConfiguredClass {

        public event System.Action OnUpdateSlot = delegate { };
        public event System.Action<InventorySlot, InstantiatedItem> OnAddItem = delegate { };
        public event System.Action<InventorySlot, InstantiatedItem> OnRemoveItem = delegate { };

        /// <summary>
        /// A stack for all items on this slot
        /// </summary>
        protected List<InstantiatedItem> instantiatedItems = new List<InstantiatedItem>();

        // game manager references
        protected HandScript handScript = null;
        protected PlayerManager playerManager = null;

        /// <summary>
        /// A referecne to the bag that this slot belongs to
        /// </summary>
        public BagPanel BagPanel { get; set; }

        public bool IsEmpty {
            get {
                return InstantiatedItems.Count == 0;
            }
        }

        public bool IsFull {
            get {
                if (IsEmpty || Count < InstantiatedItem.Item.MaximumStackSize) {
                    return false;
                }
                return true;
            }
        }

        public InstantiatedItem InstantiatedItem {
            get {
                if (!IsEmpty) {
                    return InstantiatedItems[0];
                }
                return null;
            }
        }

        public int Count { get => InstantiatedItems.Count; }
        public List<InstantiatedItem> InstantiatedItems {
            get {
                return instantiatedItems;
            }
            set {
                instantiatedItems = value;
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

        protected virtual void UpdateSlot() {
            //Debug.Log($"InventorySlot.UpdateSlot()");

            SetSlotOnItems();
            OnUpdateSlot();
        }

        private void SetSlotOnItems() {
            //Debug.Log("SlotScript.SetSlotOnItems(): MyItem is null");
            foreach (InstantiatedItem tmpItem in InstantiatedItems) {
                //Debug.Log("SlotScript.SetSlotOnItems(): going through MyItems");
                tmpItem.Slot = this;
            }
        }

        public bool AddItem(InstantiatedItem instantiatedItem) {
            //Debug.Log($"InventorySlot.Additem({instantiatedItem.Item.ResourceName}) (instance: {GetHashCode()})");

            InstantiatedItems.Add(instantiatedItem);
            instantiatedItem.Slot = this;
            UpdateSlot();
            NotifyOnAddItem(instantiatedItem);
            //Debug.Log("Slot " + GetInstanceID().ToString() + " now has count " + MyItems.Count.ToString());
            return true;
        }

        public virtual void NotifyOnAddItem(InstantiatedItem instantiatedItem) {
            OnAddItem(this, instantiatedItem);
        }

        public bool AddItems(List<InstantiatedItem> newInstantiatedItems) {
            //Debug.Log($"InventorySlot.AddItems({newInstantiatedItems.Count})");

            if (IsEmpty || SystemDataUtility.MatchResource(newInstantiatedItems[0].Item.ResourceName, InstantiatedItem.Item.ResourceName)) {
                int count = newInstantiatedItems.Count;

                for (int i = 0; i < count; i++) {
                    if (IsFull) {
                        return false;
                    }
                    AddItem(newInstantiatedItems[i]);
                    //newItems[0].Remove();
                }
                return true;
            }
            return false;
        }

        public void RemoveAllItems() {
            //Debug.Log($"InventorySlot.RemoveAllItems()");

            while (InstantiatedItems.Count > 0) {
                RemoveItem(InstantiatedItems[0]);
            }
        }

        public void RemoveItem(InstantiatedItem instantiatedItem) {
            //Debug.Log($"InventorySlot.RemoveItem({instantiatedItem.Item.ResourceName})");

            if (!IsEmpty) {
                InstantiatedItems.Remove(instantiatedItem);
                UpdateSlot();
                NotifyOnRemoveItem(instantiatedItem);
            }
        }

        public virtual void NotifyOnRemoveItem(InstantiatedItem instantiatedItem) {
            //Debug.Log($"InventorySlot.NotifyOnRemoveItem({instantiatedItem.Item.ResourceName})");

            OnRemoveItem(this, instantiatedItem);
        }

        /*
        public void DropItemFromInventorySlot() {
            //Debug.Log("Dropping an item from an inventory slot");
            if (PutItemBack() || MergeItems(playerManager.UnitController.CharacterInventoryManager.FromSlot.InventorySlot) || SwapItems(playerManager.UnitController.CharacterInventoryManager.FromSlot.InventorySlot) || AddItems(playerManager.UnitController.CharacterInventoryManager.FromSlot.InventorySlot.Items)) {
                handScript.Drop();
                playerManager.UnitController.CharacterInventoryManager.FromSlot = null;
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
            playerManager.UnitController.CharacterInventoryManager.FromSlot = this;
        }
        */

        public int GetCurrentInventorySlotIndex(UnitController sourceUnitController) {
            for (int i = 0; i < sourceUnitController.CharacterInventoryManager.InventorySlots.Count; i++) {
                if (sourceUnitController.CharacterInventoryManager.InventorySlots[i] == this) {
                    return i;
                }
            }

            // didn't find anything, this will send whatever needs this to the default slot
            return -1;
        }

        public int GetCurrentBankSlotIndex(UnitController sourceUnitController) {
            for (int i = 0; i < sourceUnitController.CharacterInventoryManager.BankSlots.Count; i++) {
                if (sourceUnitController.CharacterInventoryManager.BankSlots[i] == this) {
                    return i;
                }
            }

            // didn't find anything, this will send whatever needs this to the default slot
            return -1;
        }


        public void Clear() {
            if (InstantiatedItems.Count > 0) {
                InstantiatedItems.Clear();
                UpdateSlot();
            }
        }

        /// <summary>
        /// Uses the item if it is useable
        /// </summary>
        public void UseItem(UnitController sourceUnitController) {
            //Debug.Log($"InventorySlot.UseItem({sourceUnitController.gameObject.name})");

            if (InstantiatedItem != null) {
                InstantiatedItem.Use(sourceUnitController);
            }
        }

        public bool StackItem(InstantiatedItem instantiatedItem) {
            if (!IsEmpty && instantiatedItem.Item.ResourceName == InstantiatedItem.Item.ResourceName && InstantiatedItems.Count < InstantiatedItem.Item.MaximumStackSize) {
                AddItem(instantiatedItem);
                return true;
            }
            return false;
        }

        /*
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
        */

        public bool SwapItems(InventorySlot from) {
            Debug.Log($"InventorySlot.SwapItems()");

            // use a temporary list to swap references to the stacks
            List<InstantiatedItem> tmpFrom = new List<InstantiatedItem>(from.InstantiatedItems);
            //from.InstantiatedItems = InstantiatedItems;
            from.RemoveAllItems();
            from.AddItems(InstantiatedItems);
            RemoveAllItems();
            AddItems(tmpFrom);

            return true;
        }

        public bool MergeItems(InventorySlot from) {
            Debug.Log($"InventorySlot.MergeItems()");

            if (IsEmpty) {
                //Debug.Log("This slot is empty, there is nothing to merge.");
                return false;
            }
            if (SystemDataUtility.MatchResource(from.InstantiatedItem.Item.ResourceName, InstantiatedItem.Item.ResourceName) && !IsFull) {
                // how many free slots there are in the new stack
                int free = InstantiatedItem.Item.MaximumStackSize - Count;
                if (free >= from.Count) {
                    int maxCount = from.Count;
                    for (int i = 0; i < maxCount; i++) {
                        AddItem(from.InstantiatedItems[0]);
                        from.RemoveItem(from.InstantiatedItems[0]);
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