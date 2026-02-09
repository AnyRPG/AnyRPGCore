using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CharacterInventoryManager : ConfiguredClass {

        //public event System.Action OnClearData = delegate { };
        public event System.Action<BagNode> OnAddInventoryBagNode = delegate { };
        public event System.Action<BagNode> OnAddBankBagNode = delegate { };
        public event System.Action<InventorySlot> OnAddInventorySlot = delegate { };
        public event System.Action<InventorySlot> OnAddBankSlot = delegate { };
        public event System.Action<InventorySlot> OnRemoveInventorySlot = delegate { };
        public event System.Action<InventorySlot> OnRemoveBankSlot = delegate { };

        private SlotScript fromSlot;

        // BagNodes contain a bag and some metadata about the bag
        private List<BagNode> bagNodes = new List<BagNode>();
        private List<BagNode> bankNodes = new List<BagNode>();

        private List<InventorySlot> inventorySlots = new List<InventorySlot>();
        private List<InventorySlot> bankSlots = new List<InventorySlot>();
        private List<EquipmentInventorySlot> equipmentSlots = new List<EquipmentInventorySlot>();

        //private Dictionary<int, InstantiatedItem> instantiatedItems = new Dictionary<int, InstantiatedItem>();

        private UnitController unitController = null;

        // game manager references
        private LootManager lootManager = null;
        private MessageLogServer messageLogServer = null;

        protected bool eventSubscriptionsInitialized = false;


        public int CurrentBagCount {
            get {
                int count = 0;
                foreach (BagNode bagNode in bagNodes) {
                    if (bagNode.InstantiatedBag != null) {
                        count++;
                    }
                }
                return count;
            }
        }

        public int TotalSlotCount {
            get {
                return inventorySlots.Count + bankSlots.Count;
            }
        }

        public int FullSlotCount { get => TotalSlotCount - EmptySlotCount(); }

        public SlotScript FromSlot {
            get {
                return fromSlot;
            }

            set {
                fromSlot = value;
                if (value != null) {
                    fromSlot.Icon.color = Color.grey;
                }
            }
        }

        public List<BagNode> BagNodes { get => bagNodes; set => bagNodes = value; }
        public List<BagNode> BankNodes { get => bankNodes; set => bankNodes = value; }
        public List<InventorySlot> InventorySlots { get => inventorySlots; set => inventorySlots = value; }
        public List<InventorySlot> BankSlots { get => bankSlots; set => bankSlots = value; }
        public List<EquipmentInventorySlot> EquipmentSlots { get => equipmentSlots; set => equipmentSlots = value; }

        public CharacterInventoryManager(UnitController unitController, SystemGameManager systemGameManager) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterStats()");
            this.unitController = unitController;
            Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            lootManager = systemGameManager.LootManager;
            messageLogServer = systemGameManager.MessageLogServer;
        }


        /*
        public void ClearData() {
            //Debug.Log("InventoryManager.ClearData()");
            // keep the bag nodes, but clear their data. bag nodes are associated with physical windows and there is no point in re-initiating those
            foreach (BagNode bagNode in bagNodes) {
                //Debug.Log("InventoryManager.ClearData(): got a bag node");
                //bagNode.MyBag = null;
                if (bagNode.IsBankNode == false) {
                    //Debug.Log("Got a bag node, removing!");
                    RemoveBag(bagNode.Bag, true);
                } else {
                    //Debug.Log("Got a bank node, not removing!");
                }
            }
            //bagWindowPositionsSet = false;
            uIManager.bankWindow.CloseWindow();
            uIManager.inventoryWindow.CloseWindow();
            //MyBagNodes.Clear();

            OnClearData();
        }
        */

        public int EmptySlotCount(bool bankSlot = false) {
            int count = 0;

            if (bankSlot) {
                foreach (InventorySlot slot in bankSlots) {
                    if (slot.IsEmpty) {
                        count++;
                    }
                }
            } else {
                foreach (InventorySlot slot in inventorySlots) {
                    if (slot.IsEmpty) {
                        count++;
                    }
                }
            }

            return count;
        }

        public void LoadEquippedBagData(List<EquippedBagSaveData> equippedBagSaveData, bool bank) {
            //Debug.Log("InventoryManager.LoadEquippedBagData(" + bank + ")");
            int counter = 0;
            foreach (EquippedBagSaveData saveData in equippedBagSaveData) {
                    InstantiatedBag newBag = GetInstantiatedBagFromSaveData(saveData);
                    if (newBag != null) {
                        if (bank == true) {
                            AddBag(newBag, BankNodes[counter]);
                        } else {
                            AddBag(newBag, BagNodes[counter]);
                        }
                    }
                counter++;
            }
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
            OnAddInventorySlot(inventorySlot);
        }

        private void HandleRemoveItemFromInventorySlot(InventorySlot slot, InstantiatedItem instantiatedItem) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.HandleRemoveItemFromInventorySlot({instantiatedItem.Item.ResourceName})");

            NotifyOnItemCountChanged(instantiatedItem.Item);
            unitController.UnitEventController.NotifyOnRemoveItemFromInventorySlot(slot, instantiatedItem);
        }

        private void HandleAddItemToInventorySlot(InventorySlot slot, InstantiatedItem instantiatedItem) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.HandleAddItemToInventorySlot({slot.GetCurrentInventorySlotIndex(unitController)}, {instantiatedItem.Item.ResourceName})");

            NotifyOnItemCountChanged(instantiatedItem.Item);
            unitController.UnitEventController.NotifyOnAddItemToInventorySlot(slot, instantiatedItem);
        }

        private void RemoveInventorySlot(InventorySlot inventorySlot) {
            inventorySlots.Remove(inventorySlot);
            inventorySlot.OnAddItem -= HandleAddItemToInventorySlot;
            inventorySlot.OnRemoveItem -= HandleRemoveItemFromInventorySlot;
            OnRemoveInventorySlot(inventorySlot);
        }

        private void AddBankSlot(InventorySlot inventorySlot) {
            bankSlots.Add(inventorySlot);
            inventorySlot.OnAddItem += HandleAddItemToBankSlot;
            inventorySlot.OnRemoveItem += HandleRemoveItemFromBankSlot;
            OnAddBankSlot(inventorySlot);
        }

        private void RemoveBankSlot(InventorySlot inventorySlot) {
            bankSlots.Remove(inventorySlot);
            inventorySlot.OnAddItem -= HandleAddItemToBankSlot;
            inventorySlot.OnRemoveItem -= HandleRemoveItemFromBankSlot;
            OnRemoveBankSlot(inventorySlot);
        }

        private void HandleRemoveItemFromBankSlot(InventorySlot slot, InstantiatedItem item) {
            unitController.UnitEventController.NotifyOnRemoveItemFromBankSlot(slot, item);

        }

        private void HandleAddItemToBankSlot(InventorySlot slot, InstantiatedItem item) {
            unitController.UnitEventController.NotifyOnAddItemToBankSlot(slot, item);
        }

        public List<InventorySlot> AddBankSlots(int numSlots) {
            //Debug.Log("CharacterInventoryManager.AddBankSlots(" + numSlots + ")");
            List<InventorySlot> returnList = new List<InventorySlot>();
            for (int i = 0; i < numSlots; i++) {
                InventorySlot inventorySlot = new InventorySlot(systemGameManager);
                returnList.Add(inventorySlot);
                AddBankSlot(inventorySlot);
            }
            return returnList;
        }

        public void ClearSlots(List<InventorySlot> clearSlots) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.ClearSlots()");

            foreach (InventorySlot inventorySlot in clearSlots) {
                if (inventorySlots.Contains(inventorySlot)) {
                    RemoveInventorySlot(inventorySlot);
                }
                if (bankSlots.Contains(inventorySlot)) {
                    RemoveBankSlot(inventorySlot);
                }
            }
        }

        
        public void PerformSetupActivities() {
            InitializeDefaultInventorySlots();
            InitializeDefaultBankSlots();
            InitializeBagNodes();
            InitializeBankNodes();
        }

        public void InitializeDefaultInventorySlots() {
            for (int i = 0; i < systemConfigurationManager.DefaultInventorySlots; i++) {
                InventorySlot inventorySlot = new InventorySlot(systemGameManager);
                AddInventorySlot(inventorySlot);
            }
        }

        public void InitializeDefaultBankSlots() {
            for (int i = 0; i < systemConfigurationManager.DefaultBankSlots; i++) {
                InventorySlot inventorySlot = new InventorySlot(systemGameManager);
                AddBankSlot(inventorySlot);
            }
        }

        public void InitializeBagNodes() {
            //Debug.Log("InventoryManager.InitializeBagNodes()");
            if (bagNodes.Count > 0) {
                //Debug.Log("InventoryManager.InitializeBagNodes(): already initialized.  exiting!");
                return;
            }
            for (int i = 0; i < systemConfigurationManager.MaxInventoryBags; i++) {
                BagNode bagNode = new BagNode(this, false, i);
                bagNodes.Add(bagNode);
                OnAddInventoryBagNode(bagNode);
            }
        }

        public void InitializeBankNodes() {
            //Debug.Log("InventoryManager.InitializeBagNodes()");
            if (bankNodes.Count > 0) {
                //Debug.Log("InventoryManager.InitializeBagNodes(): already initialized.  exiting!");
                return;
            }
            for (int i = 0; i < systemConfigurationManager.MaxBankBags; i++) {
                BagNode bagNode = new BagNode(this, true, i);
                bankNodes.Add(bagNode);
                OnAddBankBagNode(bagNode);
            }
        }
        

        public void AddInventoryBag(InstantiatedBag instantiatedBag) {
            //Debug.Log("InventoryManager.AddInventoryBag(" + bag.DisplayName + ")");
            foreach (BagNode bagNode in bagNodes) {
                if (bagNode.InstantiatedBag == null) {
                    PopulateBagNode(bagNode, instantiatedBag);
                    break;
                }
            }
        }

        public void AddBankBag(InstantiatedBag instantiatedBag) {
            //Debug.Log("InventoryManager.AddBankBag(" + bag.DisplayName + ")");
            foreach (BagNode bagNode in bankNodes) {
                if (bagNode.InstantiatedBag == null) {
                    PopulateBagNode(bagNode, instantiatedBag);
                    break;
                }
            }
        }

        public void AddBag(InstantiatedBag instantiatedBag, BagNode bagNode) {
            //Debug.Log("CharacterInventoryManager.AddBag(Bag, BagNode)");
            PopulateBagNode(bagNode, instantiatedBag);
            unitController.UnitEventController.NotifyOnAddBag(instantiatedBag, bagNode);
        }

        private void PopulateBagNode(BagNode bagNode, InstantiatedBag instantiatedBag) {
            //Debug.Log("InventoryManager.PopulateBagNode(" + (bagNode != null ? bagNode.ToString() : "null") + ", " + (bag != null ? bag.DisplayName : "null") + ")");
            if (instantiatedBag != null) {
                bagNode.AddBag(instantiatedBag);
            }

            //Debug.Log("InventoryManager.PopulateBagNode(): bagNode.MyBag: " + bagNode.MyBag.GetInstanceID() + "; bagNode.MyBag.MyBagPanel: " + bagNode.MyBag.MyBagPanel.GetInstanceID() + "; bag" + bag.GetInstanceID() + "; bag.MyBagPanel: " + bag.MyBagPanel.GetInstanceID());

        }

        /// <summary>
        /// Removes the bag from the inventory
        /// </summary>
        /// <param name="instantiatedBag"></param>
        public void RemoveBag(InstantiatedBag instantiatedBag, bool clearOnly = false) {
            //Debug.Log("InventoryManager.RemoveBag(" + bag.DisplayName + ", " + clearOnly + ")");
            foreach (BagNode bagNode in bagNodes) {
                if (bagNode.InstantiatedBag == instantiatedBag) {
                    ProcessRemovebag(bagNode, instantiatedBag, false, clearOnly);
                    return;
                }
            }
            foreach (BagNode bagNode in bankNodes) {
                if (bagNode.InstantiatedBag == instantiatedBag) {
                    ProcessRemovebag(bagNode, instantiatedBag, true, clearOnly);
                    return;
                }
            }
        }

        public void ProcessRemovebag(BagNode bagNode, InstantiatedBag instantiatedBag, bool bankNode, bool clearOnly) {
            // give the old bagNode a temp location so we can add its items back to the inventory
            //BagPanel tmpBagPanel = bagNode.BagPanel;

            // make item list before nulling the bag, because that will clear the pane slots
            List<InstantiatedItem> itemsToAddBack = new List<InstantiatedItem>();
            foreach (InstantiatedItem instantiatedItem in bagNode.GetItems()) {
                itemsToAddBack.Add(instantiatedItem);
            }

            // null the bag so the items won't get added back, as we are trying to empty it so we can remove it
            bagNode.RemoveBag();

            if (!clearOnly) {
                // bag is now gone, can add items back to inventory and they won't go back in that bag
                foreach (InstantiatedItem instantiatedItem in itemsToAddBack) {
                    AddItem(instantiatedItem, bankNode);
                }
            }

            // remove references the bag held to the node it belonged to and the panel it spawned
            if (instantiatedBag != null) {
                if (instantiatedBag.BagNode != null) {
                    instantiatedBag.BagNode = null;
                }
            }
            unitController.UnitEventController.NotifyOnRemoveBag(instantiatedBag);
        }

        public void RequestSwapBags(InstantiatedBag oldInstantiatedBag, InstantiatedBag newInstantiatedBag) {
            if (systemGameManager.GameMode == GameMode.Local) {
                SwapEquippedOrUnequippedBags(oldInstantiatedBag, newInstantiatedBag);
            } else {
                unitController.UnitEventController.NotifyOnRequestSwapBags(oldInstantiatedBag, newInstantiatedBag);
            }
        }

        public void SwapEquippedOrUnequippedBags(InstantiatedBag oldInstantiatedBag, InstantiatedBag newInstantiatedBag) {
            if (oldInstantiatedBag.BagNode != null && newInstantiatedBag.BagNode != null) {
                SwapEquippedBags(oldInstantiatedBag, newInstantiatedBag);
            } else {
                SwapBags(oldInstantiatedBag, newInstantiatedBag);
            }
        }

        public void SwapEquippedBags(InstantiatedBag oldInstantiatedBag, InstantiatedBag newInstantiatedBag) {
            int newSlotCount = TotalSlotCount - (oldInstantiatedBag.Slots + newInstantiatedBag.Slots);

            // if there will not be enough space in the inventory after the bag swap, don't swap
            if (newSlotCount - FullSlotCount < 0) {
                return;
            }

            BagNode oldBagNode = oldInstantiatedBag.BagNode;
            BagNode newBagNode = newInstantiatedBag.BagNode;

            RemoveBag(oldInstantiatedBag);
            RemoveBag(newInstantiatedBag);
            AddBag(oldInstantiatedBag, newBagNode);
            AddBag(newInstantiatedBag, oldBagNode);
        }

        public void SwapBags(InstantiatedBag oldInstantiatedBag, InstantiatedBag newInstantiatedBag) {
            int newSlotCount = (TotalSlotCount - oldInstantiatedBag.Slots) + newInstantiatedBag.Slots;

            // if there will not be enough space in the inventory after the bag swap, don't swap
            if (newSlotCount - FullSlotCount < 0) {
                return;
            }

            // check if the new bag is in the old bag
            bool bagInBag = false;
            if (oldInstantiatedBag.BagNode.InventorySlots.Contains(newInstantiatedBag.Slot)) {
                bagInBag = true;
            }

            // do swap
            //List<Item> bagItems = oldBag.MyBagPanel.GetItems();
            List<InstantiatedItem> instantiatedBagItems = oldInstantiatedBag.BagNode.GetItems();

            BagNode oldBagNode = oldInstantiatedBag.BagNode;
            InventorySlot oldSlot = newInstantiatedBag.Slot;
            //newBag.BagNode = oldBag.BagNode;
            bool isBankNode = oldInstantiatedBag.BagNode.IsBankNode;

            // remove bag and do not add items back since the new bag hasn't been added yet and the inventory may not have the space
            RemoveBag(oldInstantiatedBag, true);

            // clear the slot the new bag is in so the bag doesn't get duplicated
            //newInstantiatedBag.Slot.Clear();
            newInstantiatedBag.Remove();

            // make space by adding the new bag
            AddBag(newInstantiatedBag, oldBagNode);

            if (bagInBag == false) {
                // add the old bag into the same inventory slot
                oldSlot.AddItem(oldInstantiatedBag);
            } else {
                // add the old bag into any available inventory slot
                AddItem(oldInstantiatedBag, isBankNode);
            }

            // add items back now that the space exists
            foreach (InstantiatedItem instantiatedItem in instantiatedBagItems) {
                if (instantiatedItem != newInstantiatedBag) {
                    AddItem(instantiatedItem, newInstantiatedBag.BagNode.IsBankNode);
                }
            }
        }

        /// <summary>
        /// Adds an item to the inventory
        /// </summary>
        /// <param name="instantiatedItem"></param>
        public bool AddItem(InstantiatedItem instantiatedItem, bool addToBank, bool performUniqueCheck = true) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.AddItem({(instantiatedItem == null ? "null" : instantiatedItem.DisplayName)}, {addToBank})");

            if (instantiatedItem == null) {
                return false;
            }
            if (performUniqueCheck == true && instantiatedItem.Item.UniqueItem == true && GetItemCount(instantiatedItem.Item.ResourceName) > 0) {
                unitController.UnitEventController.NotifyOnWriteMessageFeedMessage($"{instantiatedItem.DisplayName} is unique.  You can only carry one at a time.");
                return false;
            }
            if (instantiatedItem.Item.MaximumStackSize > 0) {
                if (PlaceInStack(instantiatedItem, addToBank)) {
                    return true;
                }
            }
            //Debug.Log("About to attempt placeInEmpty");
            return PlaceInEmpty(instantiatedItem, addToBank);
        }

        public bool AddInventoryItem(InstantiatedItem instantiatedItem, InventorySlot inventorySlot) {
            return inventorySlot.AddItem(instantiatedItem);
        }

        public bool AddInventoryItem(InstantiatedItem instantiatedItem, int slotIndex) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.AddInventoryItem({instantiatedItem.ResourceName}, {slotIndex})");

            if (inventorySlots.Count > slotIndex) {
                return inventorySlots[slotIndex].AddItem(instantiatedItem);
            }
            return AddItem(instantiatedItem, false);
        }

        public bool AddBankItem(InstantiatedItem instantiatedItem, int slotIndex) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.AddBankItem({instantiatedItem.ResourceName}, {slotIndex})");

            if (bankSlots.Count > slotIndex) {
                return bankSlots[slotIndex].AddItem(instantiatedItem);
            }
            return AddItem(instantiatedItem, true);
        }

        public void RemoveInventoryItem(InstantiatedItem instantiatedItem, int slotIndex) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.RemoveInventoryItem({instantiatedItem.ResourceName}, {slotIndex})");

            if (inventorySlots.Count > slotIndex) {
                inventorySlots[slotIndex].RemoveItem(instantiatedItem);
            }
        }

        public void RemoveBankItem(InstantiatedItem instantiatedItem, int slotIndex) {
            if (bankSlots.Count > slotIndex) {
                bankSlots[slotIndex].RemoveItem(instantiatedItem);
            }
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

        private bool PlaceInEmpty(InstantiatedItem instantiatedItem, bool addToBank) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.PlaceInEmpty({instantiatedItem.ResourceName}, {addToBank})");

            int slotIndex = 0;
            if (addToBank == true) {
                foreach (InventorySlot inventorySlot in bankSlots) {
                    //Debug.Log($"CharacterInventoryManager.PlaceInEmpty({instantiatedItem.ResourceName}): checking slot");
                    if (inventorySlot.IsEmpty) {
                        //Debug.Log($"CharacterInventoryManager.PlaceInEmpty({instantiatedItem.ResourceName}): checking slot: its empty.  adding item");
                        inventorySlot.AddItem(instantiatedItem);
                        unitController.UnitEventController.NotifyOnPlaceInEmpty(instantiatedItem, addToBank, slotIndex);
                        return true;
                    }
                    slotIndex++;
                }
            } else {
                foreach (InventorySlot inventorySlot in inventorySlots) {
                    //Debug.Log($"CharacterInventoryManager.PlaceInEmpty({instantiatedItem.ResourceName}): checking slot");
                    if (inventorySlot.IsEmpty) {
                        //Debug.Log($"CharacterInventoryManager.PlaceInEmpty({instantiatedItem.ResourceName}): checking slot: its empty.  adding item");
                        inventorySlot.AddItem(instantiatedItem);
                        unitController.UnitEventController.NotifyOnPlaceInEmpty(instantiatedItem, addToBank, slotIndex);
                        return true;
                    }
                    slotIndex++;
                }
            }
            if (EmptySlotCount(addToBank) == 0) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.PlaceInEmpty({instantiatedItem.ResourceName}, {addToBank}): no empty slots");
                unitController.UnitEventController.NotifyOnWriteMessageFeedMessage($"{(addToBank == false ? "Inventory" : "Bank")} is full!");
            }
            return false;
        }

        private bool PlaceInStack(InstantiatedItem instantiatedItem, bool addToBank) {
            int slotIndex = 0;
            if (addToBank == false) {
                foreach (InventorySlot inventorySlot in inventorySlots) {
                    if (PlaceInStack(inventorySlot, slotIndex, instantiatedItem, addToBank) == true) {
                        return true;
                    }
                    slotIndex++;
                }
            } else {
                foreach (InventorySlot inventorySlot in bankSlots) {
                    if (PlaceInStack(inventorySlot, slotIndex, instantiatedItem, addToBank) == true) {
                        return true;
                    }
                    slotIndex++;
                }
            }

            return false;
        }

        public void PlaceInStack(int slotIndex, InstantiatedItem instantiatedItem, bool addToBank) {
            if (addToBank == false) {
                if (inventorySlots.Count > slotIndex) {
                    PlaceInStack(inventorySlots[slotIndex], slotIndex, instantiatedItem, addToBank);
                }
            } else {
                if (bankSlots.Count > slotIndex) {
                    PlaceInStack(inventorySlots[slotIndex], slotIndex, instantiatedItem, addToBank);
                }
            }
        }


        private bool PlaceInStack(InventorySlot inventorySlot, int slotIndex, InstantiatedItem instantiatedItem, bool addToBank) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.PlaceInStack({slotIndex}, {instantiatedItem.Item.ResourceName}, {addToBank})");

            if (inventorySlot.StackItem(instantiatedItem)) {
                unitController.UnitEventController.NotifyOnPlaceInStack(instantiatedItem, addToBank, slotIndex);
                return true;
            }
            return false;
        }

        /*
        public IUseable GetUseable(IUseable useable) {
            //IUseable useable = new Stack<IUseable>();
            foreach (BagNode bagNode in bagNodes) {
                if (bagNode.Bag != null) {
                    foreach (SlotScript slot in bagNode.InventorySlots) {
                        if (!slot.IsEmpty && SystemDataUtility.MatchResource(slot.Item.DisplayName, useable.DisplayName)) {
                            return (slot.Item as IUseable);
                        }
                    }
                }
            }
            return null;
            //return useables;
        }
        */

        public int GetUseableCount(IUseable useable) {
            int count = 0;
            foreach (InventorySlot slot in inventorySlots) {
                if (!slot.IsEmpty && SystemDataUtility.MatchResource(slot.InstantiatedItem.Item.ResourceName, useable.ResourceName)) {
                    count += slot.Count;
                }
            }
            return count;
        }

        public void NotifyOnItemCountChanged(Item item) {
            unitController.UnitEventController.NotifyOnItemCountChanged(item);
        }

        public int GetItemCount(string type, bool partialMatch = false) {
            //Debug.Log("InventoryManager.GetItemCount(" + type + ")");
            int itemCount = 0;

            foreach (InventorySlot slot in inventorySlots) {
                if (!slot.IsEmpty && SystemDataUtility.MatchResource(slot.InstantiatedItem.Item.ResourceName, type, partialMatch)) {
                    itemCount += slot.Count;
                }
            }
            foreach (InventorySlot slot in bankSlots) {
                if (!slot.IsEmpty && SystemDataUtility.MatchResource(slot.InstantiatedItem.Item.ResourceName, type, partialMatch)) {
                    itemCount += slot.Count;
                }
            }

            return itemCount;
        }

        public List<InstantiatedItem> GetItems(string itemType, int count) {
            //Debug.Log("InventoryManager.GetItems(" + itemType + ", " + count + ")");
            List<InstantiatedItem> instantiatedItems = new List<InstantiatedItem>();
            foreach (InventorySlot slot in inventorySlots) {
                //Debug.Log("InventoryManager.GetItems() got bagnode and it has a bag and we are looking in a slotscript");
                if (!slot.IsEmpty && SystemDataUtility.MatchResource(slot.InstantiatedItem.Item.ResourceName, itemType)) {
                    //Debug.Log("InventoryManager.GetItems() got bagnode and it has a bag and we are looking in a slotscript and the slot is not empty and it matches");
                    foreach (InstantiatedItem instantiatedItem in slot.InstantiatedItems.Values) {
                        //Debug.Log("InventoryManager.GetItems() got bagnode and it has a bag and we are looking in a slotscript and the slot is not empty and it matches and we are ading and item");
                        instantiatedItems.Add(instantiatedItem);
                        if (instantiatedItems.Count == count) {
                            //Debug.Log("InventoryManager.GetItems() return items with count: " + items.Count);
                            return instantiatedItems;
                        }
                    }
                }
            }
            return instantiatedItems;
        }

        /*
        public List<SlotScript> GetSlots() {
            List<SlotScript> items = new List<SlotScript>();
            foreach (BagNode bagNode in bagNodes) {
                if (bagNode.Bag != null) {
                    foreach (SlotScript slot in bagNode.InventorySlots) {
                        items.Add(slot);
                    }
                }
            }
            return items;
        }
        */

        /*
        /// <summary>
        /// Get a new instantiated Item
        /// </summary>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public InstantiatedItem GetNewInstantiatedItemFromSaveData(Item item, InventorySlotSaveData inventorySlotSaveData) {
            //Debug.Log(this.GetType().Name + ".GetNewResource(" + resourceName + ")");
            long itemInstanceId = systemItemManager.GetNewItemInstanceId();
            return GetNewInstantiatedItemFromSaveData(itemInstanceId, item, inventorySlotSaveData);
        }
        */


        public InstantiatedItem GetNewInstantiatedItem(string itemName, ItemQuality itemQuality = null) {
            //Debug.Log(this.GetType().Name + ".GetNewResource(" + resourceName + ")");
            Item item = systemDataFactory.GetResource<Item>(itemName);
            if (item == null) {
                return null;
            }
            return GetNewInstantiatedItem(item, itemQuality);
        }

        public InstantiatedItem GetNewInstantiatedItem(Item item, ItemQuality itemQuality = null, IInstantiatedItemRequestor requestor = null) {
            //Debug.Log(this.GetType().Name + ".GetNewResource(" + resourceName + ")");
            InstantiatedItem instantiatedItem = systemItemManager.GetNewInstantiatedItem(item, itemQuality);
            //instantiatedItem.InitializeNewItem(itemQuality);
            instantiatedItem.DropLevel = unitController.CharacterStats.Level;
            if (requestor != null) {
                requestor.InitializeItem(instantiatedItem);
            }
            //instantiatedItems.Add(instantiatedItem.InstanceId, instantiatedItem);
            unitController.UnitEventController.NotifyOnGetNewInstantiatedItem(instantiatedItem);
            return instantiatedItem;
        }

        public InstantiatedItem GetNewInstantiatedItemFromSaveData(ItemInstanceSaveData itemInstanceSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.GetNewInstantiatedItemFromSaveData({inventorySlotSaveData.ItemName})");

            return GetNewInstantiatedItemFromSaveData(itemInstanceSaveData, itemInstanceSaveData.ItemInstanceId);
        }

        public InstantiatedItem GetNewInstantiatedItemFromSaveData(ItemInstanceSaveData itemInstanceSaveData, long itemInstanceId) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.GetNewInstantiatedItemFromSaveData({inventorySlotSaveData.ItemName})");

            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId)) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.GetNewInstantiatedItemFromSaveData() item already exists in instantiated items");
                return systemItemManager.InstantiatedItems[itemInstanceId];
            }

            Item item = null;
            if (itemInstanceSaveData.ItemName == "System Currency Loot Item") {
                item = lootManager.CurrencyLootItem;
            } else {
                item = systemDataFactory.GetResource<Item>(itemInstanceSaveData.ItemName);
            }
            if (item == null) {
                return null;
            }
            return GetNewInstantiatedItemFromSaveData(item, itemInstanceSaveData);
        }

        public InstantiatedItem GetNewInstantiatedItemFromSaveData(Item item, ItemInstanceSaveData itemInstanceSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.GetNewInstantiatedItemFromSaveData({item.ResourceName})");

            ItemQuality usedItemQuality = null;
            if (itemInstanceSaveData.ItemQuality != null && itemInstanceSaveData.ItemQuality != string.Empty) {
                usedItemQuality = systemDataFactory.GetResource<ItemQuality>(itemInstanceSaveData.ItemQuality);
            }
            InstantiatedItem instantiatedItem = systemItemManager.GetNewInstantiatedItem(itemInstanceSaveData.ItemInstanceId, item, usedItemQuality);
            //instantiatedItem.InitializeNewItem(usedItemQuality);
            instantiatedItem.LoadSaveData(itemInstanceSaveData);

            //instantiatedItems.Add(instantiatedItem.InstanceId, instantiatedItem);
            return instantiatedItem;
        }

        public InstantiatedBag GetInstantiatedBagFromSaveData(EquippedBagSaveData equippedBagSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.GetNewInstantiatedItemFromSaveData({itemInstanceId}, {itemName})");

            if (equippedBagSaveData.HasItem == false) {
                return null;
            }

            if (systemItemManager.InstantiatedItems.ContainsKey(equippedBagSaveData.ItemInstanceId)) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.GetNewInstantiatedItemFromSaveData() item already exists in instantiated items");
                return systemItemManager.InstantiatedItems[equippedBagSaveData.ItemInstanceId] as InstantiatedBag;
            }
            return null;
        }

        public InstantiatedEquipment GetInstantiatedEquipmentFromSaveData(EquipmentInventorySlotSaveData equipmentSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.GetNewInstantiatedItemFromSaveData({equipmentSaveData.EquipmentName}({equipmentSaveData.itemInstanceId}))");

            if (equipmentSaveData.HasItem == false) {
                return null;
            }

            if (systemItemManager.InstantiatedItems.ContainsKey(equipmentSaveData.ItemInstanceId)) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.GetNewInstantiatedItemFromSaveData() item already exists in instantiated items");
                return systemItemManager.InstantiatedItems[equipmentSaveData.ItemInstanceId] as InstantiatedEquipment;
            }
            return null;
        }

        public void RequestDeleteItem(InstantiatedItem instantiatedItem) {
            if (systemGameManager.GameMode == GameMode.Local ) {
                // currently this only gets called from the hand script
                DeleteItem(instantiatedItem);
            } else {
                unitController.UnitEventController.NotifyOnRequestDeleteItem(instantiatedItem);
            }
        }

        public void DeleteItem(long instanceId) {
            if (systemItemManager.InstantiatedItems.ContainsKey(instanceId)) {
                DeleteItem(systemItemManager.InstantiatedItems[instanceId]);
            }
        }

        public void DeleteItem(InstantiatedItem instantiatedItem) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.DeleteItem({instantiatedItem.InstanceId}({instantiatedItem.ResourceName}))");

            if (instantiatedItem.Slot != null) {
                //instantiatedItem.Slot.Clear();
                instantiatedItem.Slot.RemoveAllItems();
                NotifyOnItemCountChanged(instantiatedItem.Item);
            } else {
                // first we want to get this items equipment slot
                // next we want to query the equipmentmanager on the charcter to see if he has an item in this items slot, and if it is the item we are dropping
                // if it is, then we will unequip it, and then destroy it
                if (instantiatedItem is InstantiatedEquipment) {
                    unitController.CharacterEquipmentManager.Unequip(instantiatedItem as InstantiatedEquipment);
                    if (instantiatedItem.Slot != null) {
                        instantiatedItem.Slot.RemoveAllItems();
                        NotifyOnItemCountChanged(instantiatedItem.Item);
                    }
                    unitController.UnitModelController.RebuildModelAppearance();
                }
            }
            //unitController.UnitEventController.NotifyOnDeleteItem(instantiatedItem);
            messageLogServer.WriteSystemMessage(unitController, $"Destroyed {instantiatedItem.DisplayName}");
        }

        public void RequestDropItemFromInventorySlot(InventorySlot fromSlot, InventorySlot toSlot, bool fromSlotIsInventory, bool toSlotIsInventory) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.RequestDropItemFromInventorySlot()");

            unitController.UnitEventController.NotifyOnRequestDropItemFromInventorySlot(fromSlot, toSlot, fromSlotIsInventory, toSlotIsInventory);
            if (systemGameManager.GameMode == GameMode.Local) {
                DropItemFromInventorySlot(fromSlot, toSlot);
            }
        }

        public void DropItemFromInventorySlot(int fromslotIndex, int toSlotIndex, bool fromSlotIsInventory, bool toSlotIsInventory) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.DropItemFromInventorySlot({fromslotIndex}, {toSlotIndex})");

            if (fromSlotIsInventory) {
                if (inventorySlots.Count <= fromslotIndex) {
                    return;
                }
            } else {
                if (bankSlots.Count <= fromslotIndex) {
                    return;
                }
            }
            if (toSlotIsInventory) {
                if (inventorySlots.Count <= toSlotIndex) {
                    return;
                }
            } else {
                if (bankSlots.Count <= toSlotIndex) {
                    return;
                }
            }
            InventorySlot fromSlot;
            InventorySlot toSlot;
            if (fromSlotIsInventory) {
                fromSlot = inventorySlots[fromslotIndex];
            } else {
                fromSlot = bankSlots[fromslotIndex];
            }
            if (toSlotIsInventory) {
                toSlot = inventorySlots[toSlotIndex];
            } else {
                toSlot = bankSlots[toSlotIndex];
            }
            DropItemFromInventorySlot(fromSlot, toSlot);
        }

        public void DropItemFromInventorySlot(InventorySlot fromSlot, InventorySlot toSlot) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.DropItemFromInventorySlot()");

            if (toSlot.MergeItems(fromSlot)) {
                return;
            }
            if (toSlot.SwapItems(fromSlot)) {
                return;
            }

            // merge and swap failed, so attempt to add items
            toSlot.AddItems(fromSlot.InstantiatedItems.Values.ToList());

        }

        public void RequestMoveFromBankToInventory(InventorySlot inventorySlot) {
            if (systemGameManager.GameMode == GameMode.Local) {
                MoveFromBankToInventory(inventorySlot);
            } else {
                unitController.UnitEventController.NotifyOnRequestMoveFromBankToInventory(inventorySlot.GetCurrentBankSlotIndex(unitController));
            }
        }

        public void MoveFromBankToInventory(InventorySlot inventorySlot) {
            List<InstantiatedItem> moveList = new List<InstantiatedItem>();
            //Debug.Log("SlotScript.InteractWithSlot(): interacting with item in bank");
            foreach (InstantiatedItem instantiatedItem in inventorySlot.InstantiatedItems.Values) {
                moveList.Add(instantiatedItem);
            }
            foreach (InstantiatedItem instantiatedItem in moveList) {
                if (AddItem(instantiatedItem, false, false)) {
                    inventorySlot.RemoveItem(instantiatedItem);
                }
            }
        }

        public void MoveFromBankToInventory(int slotIndex) {
            //Debug.Log($"CharacterInventoryManager.MoveFromBankToInventory({slotIndex})");

            if (bankSlots.Count > slotIndex) {
                MoveFromBankToInventory(bankSlots[slotIndex]);
            }
        }

        public void RequestMoveFromInventoryToBank(InventorySlot inventorySlot) {
            if (systemGameManager.GameMode == GameMode.Local) {
                MoveFromInventoryToBank(inventorySlot);
            } else {
                unitController.UnitEventController.NotifyOnRequestMoveFromInventoryToBank(inventorySlot.GetCurrentInventorySlotIndex(unitController));
            }
        }

        public void MoveFromInventoryToBank(InventorySlot inventorySlot) {
            List<InstantiatedItem> moveList = new List<InstantiatedItem>();
            foreach (InstantiatedItem instantiatedItem in inventorySlot.InstantiatedItems.Values) {
                moveList.Add(instantiatedItem);
            }
            foreach (InstantiatedItem instantiatedItem in moveList) {
                if (AddItem(instantiatedItem, true, false)) {
                    inventorySlot.RemoveItem(instantiatedItem);
                }
            }
        }

        public void MoveFromInventoryToBank(int slotIndex) {
            //Debug.Log($"CharacterInventoryManager.MoveFromInventoryToBank({slotIndex})");

            if (inventorySlots.Count > slotIndex) {
                MoveFromInventoryToBank(inventorySlots[slotIndex]);
            }
        }

        public void RequestUseItem(InventorySlot inventorySlot) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.RequestUseItem()");

            if (systemGameManager.GameMode == GameMode.Local) {
                UseItem(inventorySlot);
            } else {
                unitController.UnitEventController.NotifyOnRequestUseItem(inventorySlot.GetCurrentInventorySlotIndex(unitController));
            }
        }

        public void UseItem(InventorySlot inventorySlot) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.UseItem()");

            if (inventorySlot.InstantiatedItem != null) {
                inventorySlot.UseItem(unitController);
            }
        }

        public void UseItem(int slotIndex) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.UseItem({slotIndex})");

            if (inventorySlots.Count > slotIndex) {
                UseItem(inventorySlots[slotIndex]);
            }
        }

        public void RequestUnequipBag(InstantiatedBag instantiatedBag, bool isBank) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.RequestUnequipBag({instantiatedBag.DisplayName}, {isBank})");

            if (systemGameManager.GameMode == GameMode.Local) {
                UnequipBag(instantiatedBag, isBank);
            } else {
                unitController.UnitEventController.NotifyOnRequestUnequipBag(instantiatedBag, isBank);
            }
        }

        public void UnequipBag(InstantiatedBag instantiatedBag, bool isBank) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.UnequipBag({instantiatedBag.DisplayName}, {isBank})");

            AddItem(instantiatedBag, isBank);
            RemoveBag(instantiatedBag);
        }

        public void RequestUnequipBagToSlot(InstantiatedBag instantiatedBag, InventorySlot inventorySlot, bool isBankSlot) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.RequestUnequipBag({instantiatedBag.DisplayName}, inventoryslot, {isBankSlot})");

            if (systemGameManager.GameMode == GameMode.Local) {
                UnequipBagToSlot(instantiatedBag, inventorySlot);
            } else {
                int slotIndex;
                if (isBankSlot) {
                    slotIndex = inventorySlot.GetCurrentBankSlotIndex(unitController);
                } else {
                    slotIndex = inventorySlot.GetCurrentInventorySlotIndex(unitController);
                }

                unitController.UnitEventController.NotifyOnRequestUnequipBagToSlot(instantiatedBag, slotIndex, isBankSlot);
            }
        }

        public void UnequipBagToSlot(InstantiatedBag instantiatedBag, int slotIndex, bool isBank) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.UnequipBag({instantiatedBag.DisplayName}, {slotIndex}, {isBank})");

            InventorySlot inventorySlot;
            if (isBank && bankSlots.Count > slotIndex) {
                inventorySlot = bankSlots[slotIndex];
            } else if (isBank == false && inventorySlots.Count > slotIndex) {
                inventorySlot = inventorySlots[slotIndex];
            } else {
                return;
            }
            UnequipBagToSlot(instantiatedBag, inventorySlot);
        }

        public void UnequipBagToSlot(InstantiatedBag instantiatedBag, InventorySlot inventorySlot) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.UnequipBag({instantiatedBag.DisplayName})");

            inventorySlot.AddItem(instantiatedBag);
            RemoveBag(instantiatedBag);
        }

        public void RequestMoveBag(InstantiatedBag bag, BagNode bagNode) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.RequestMoveBag({bag.DisplayName}, {bagNode.NodeIndex})");

            if (systemGameManager.GameMode == GameMode.Local) {
                MoveBag(bag, bagNode);
            } else {
                unitController.UnitEventController.NotifyOnRequestMoveBag(bag, bagNode.NodeIndex, bagNode.IsBankNode);
            }
        }

        public void MoveBag(InstantiatedBag bag, BagNode bagNode) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.MoveBag({bag.DisplayName}, {bagNode.NodeIndex})");

            if (EmptySlotCount(bag.BagNode.IsBankNode) - bag.Slots >= 0) {
                RemoveBag(bag);
                AddBag(bag, bagNode);
            }
        }

        public void MoveBag(InstantiatedBag instantiatedBag, int nodeIndex, bool isBankNode) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.MoveBag({instantiatedBag.DisplayName}, {nodeIndex}, {isBankNode})");

            if (isBankNode) {
                if (bankNodes.Count > nodeIndex) {
                    MoveBag(instantiatedBag, bankNodes[nodeIndex]);
                }
            } else {
                if (bagNodes.Count > nodeIndex) {
                    MoveBag(instantiatedBag, bagNodes[nodeIndex]);
                }
            }
        }

        public void RequestAddBagFromInventory(InstantiatedBag instantiatedBag, BagNode bagNode) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.RequestAddBagFromInventory({instantiatedBag.DisplayName}, {bagNode})");

            if (systemGameManager.GameMode == GameMode.Local) {
                AddBagFromInventory(instantiatedBag, bagNode);
            } else {
                unitController.UnitEventController.NotifyOnRequestAddBagFromInventory(instantiatedBag, bagNode.NodeIndex, bagNode.IsBankNode);
            }
        }

        public void AddBagFromInventory(InstantiatedBag instantiatedBag, BagNode bagNode) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.AddBagFromInventory({instantiatedBag.DisplayName}, {bagNode})");

            AddBag(instantiatedBag, bagNode);
            instantiatedBag.Remove();
        }

        public void AddBagFromInventory(InstantiatedBag instantiatedBag, int nodeIndex, bool isBankNode) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterInventoryManager.AddBagFromInventory({instantiatedBag.DisplayName}, {nodeIndex}, {isBankNode})");

            if (isBankNode) {
                if (bankNodes.Count > nodeIndex) {
                    AddBagFromInventory(instantiatedBag, bankNodes[nodeIndex]);
                }
            } else {
                if (bagNodes.Count > nodeIndex) {
                    AddBagFromInventory(instantiatedBag, bagNodes[nodeIndex]);
                }
            }
        }

        public bool HasItem(long itemInstanceId) {
            foreach (InventorySlot slot in inventorySlots) {
                foreach (InstantiatedItem instantiatedItem in slot.InstantiatedItems.Values) {
                    if (instantiatedItem.InstanceId == itemInstanceId) {
                        return true;
                    }
                }
            }
            return false;
        }
    }

}