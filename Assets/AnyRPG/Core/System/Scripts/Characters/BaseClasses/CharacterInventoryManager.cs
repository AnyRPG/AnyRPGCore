using AnyRPG;
using System.Collections;
using System.Collections.Generic;
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

        private BaseCharacter baseCharacter = null;

        // game manager references
        private HandScript handScript = null;
        private MessageFeedManager messageFeedManager = null;
        private SystemItemManager systemItemManager = null;
        private UIManager uIManager = null;
        //private ObjectPooler objectPooler = null;
        private SystemEventManager systemEventManager = null;

        protected bool eventSubscriptionsInitialized = false;


        public int CurrentBagCount {
            get {
                int count = 0;
                foreach (BagNode bagNode in bagNodes) {
                    if (bagNode.Bag != null) {
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

        public CharacterInventoryManager(BaseCharacter baseCharacter, SystemGameManager systemGameManager) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterStats()");
            this.baseCharacter = baseCharacter;
            Configure(systemGameManager);
        }

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log("InventoryManager.Awake()");
            base.Configure(systemGameManager);

            //PerformSetupActivities();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            handScript = uIManager.HandScript;
            messageFeedManager = uIManager.MessageFeedManager;
            systemItemManager = systemGameManager.SystemItemManager;
            //objectPooler = systemGameManager.ObjectPooler;
            systemEventManager = systemGameManager.SystemEventManager;
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
            Debug.Log("InventoryManager.LoadEquippedBagData(" + bank + ")");
            int counter = 0;
            foreach (EquippedBagSaveData saveData in equippedBagSaveData) {
                if (saveData.slotCount > 0) {
                    Bag newBag = systemItemManager.GetNewResource(saveData.BagName) as Bag;
                    if (newBag != null) {
                        if (bank == true) {
                            AddBag(newBag, BankNodes[counter]);
                        } else {
                            AddBag(newBag, BagNodes[counter]);
                        }
                    }
                }
                counter++;
            }
        }

        public List<InventorySlot> AddInventorySlots(int numSlots) {
            Debug.Log("CharacterInventoryManager.AddInventorySlots(" + numSlots + ")");
            List<InventorySlot> returnList = new List<InventorySlot>();
            for (int i = 0; i < numSlots; i++) {
                InventorySlot inventorySlot = new InventorySlot(systemGameManager);
                inventorySlots.Add(inventorySlot);
                returnList.Add(inventorySlot);
                OnAddInventorySlot(inventorySlot);
            }
            return returnList;
        }

        public List<InventorySlot> AddBankSlots(int numSlots) {
            Debug.Log("CharacterInventoryManager.AddBankSlots(" + numSlots + ")");
            List<InventorySlot> returnList = new List<InventorySlot>();
            for (int i = 0; i < numSlots; i++) {
                InventorySlot inventorySlot = new InventorySlot(systemGameManager);
                bankSlots.Add(inventorySlot);
                returnList.Add(inventorySlot);
                OnAddBankSlot(inventorySlot);
            }
            return returnList;
        }

        public void ClearSlots(List<InventorySlot> clearSlots) {
            foreach (InventorySlot inventorySlot in clearSlots) {
                if (inventorySlots.Contains(inventorySlot)) {
                    inventorySlots.Remove(inventorySlot);
                    OnRemoveInventorySlot(inventorySlot);
                }
                if (bankSlots.Contains(inventorySlot)) {
                    bankSlots.Remove(inventorySlot);
                    OnRemoveBankSlot(inventorySlot);
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
                inventorySlots.Add(inventorySlot);
                OnAddInventorySlot(inventorySlot);
            }
        }

        public void InitializeDefaultBankSlots() {
            for (int i = 0; i < systemConfigurationManager.DefaultBankSlots; i++) {
                InventorySlot inventorySlot = new InventorySlot(systemGameManager);
                bankSlots.Add(inventorySlot);
                OnAddBankSlot(inventorySlot);
            }
        }

        public void InitializeBagNodes() {
            //Debug.Log("InventoryManager.InitializeBagNodes()");
            if (bankNodes.Count > 0) {
                //Debug.Log("InventoryManager.InitializeBagNodes(): already initialized.  exiting!");
                return;
            }
            for (int i = 0; i < systemConfigurationManager.MaxInventoryBags; i++) {
                BagNode bagNode = new BagNode(this, false);
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
                BagNode bagNode = new BagNode(this, true);
                bankNodes.Add(bagNode);
                OnAddBankBagNode(bagNode);
            }
        }
        

        public void AddInventoryBag(Bag bag) {
            Debug.Log("InventoryManager.AddInventoryBag(" + bag.DisplayName + ")");
            foreach (BagNode bagNode in bagNodes) {
                if (bagNode.Bag == null) {
                    PopulateBagNode(bagNode, bag);
                    break;
                }
            }
        }

        public void AddBankBag(Bag bag) {
            Debug.Log("InventoryManager.AddBankBag(" + bag.DisplayName + ")");
            foreach (BagNode bagNode in bankNodes) {
                if (bagNode.Bag == null) {
                    PopulateBagNode(bagNode, bag);
                    break;
                }
            }
        }

        public void AddBag(Bag bag, BagNode bagNode) {
            //Debug.Log("InventoryManager.AddBag(Bag, BagNode)");
            PopulateBagNode(bagNode, bag);
        }

        private void PopulateBagNode(BagNode bagNode, Bag bag) {
            //Debug.Log("InventoryManager.PopulateBagNode(" + (bagNode != null ? bagNode.ToString() : "null") + ", " + (bag != null ? bag.DisplayName : "null") + ")");
            if (bag != null) {
                bagNode.AddBag(bag);
            }

            //Debug.Log("InventoryManager.PopulateBagNode(): bagNode.MyBag: " + bagNode.MyBag.GetInstanceID() + "; bagNode.MyBag.MyBagPanel: " + bagNode.MyBag.MyBagPanel.GetInstanceID() + "; bag" + bag.GetInstanceID() + "; bag.MyBagPanel: " + bag.MyBagPanel.GetInstanceID());

            uIManager.UpdateInventoryOpacity();

        }

        /// <summary>
        /// Removes the bag from the inventory
        /// </summary>
        /// <param name="bag"></param>
        public void RemoveBag(Bag bag, bool clearOnly = false) {
            Debug.Log("InventoryManager.RemoveBag(" + bag.DisplayName + ", " + clearOnly + ")");
            foreach (BagNode bagNode in bagNodes) {
                if (bagNode.Bag == bag) {
                    ProcessRemovebag(bagNode, bag, true, clearOnly);
                    return;
                }
            }
            foreach (BagNode bagNode in bankNodes) {
                if (bagNode.Bag == bag) {
                    ProcessRemovebag(bagNode, bag, true, clearOnly);
                    return;
                }
            }
        }

        public void ProcessRemovebag(BagNode bagNode, Bag bag, bool bankNode, bool clearOnly) {
            // give the old bagNode a temp location so we can add its items back to the inventory
            //BagPanel tmpBagPanel = bagNode.BagPanel;

            // make item list before nulling the bag, because that will clear the pane slots
            List<Item> itemsToAddBack = new List<Item>();
            foreach (Item item in bagNode.GetItems()) {
                itemsToAddBack.Add(item);
            }

            // null the bag so the items won't get added back, as we are trying to empty it so we can remove it
            bagNode.RemoveBag();

            if (!clearOnly) {
                // bag is now gone, can add items back to inventory and they won't go back in that bag
                foreach (Item item in itemsToAddBack) {
                    AddItem(item, bankNode);
                }
            }

            // remove references the bag held to the node it belonged to and the panel it spawned
            if (bag != null) {
                if (bag.BagNode != null) {
                    bag.BagNode = null;
                }
            }

        }

        public void SwapBags(Bag oldBag, Bag newBag) {
            int newSlotCount = (TotalSlotCount - oldBag.Slots) + newBag.Slots;

            if (newSlotCount - FullSlotCount >= 0) {
                // do swap
                //List<Item> bagItems = oldBag.MyBagPanel.GetItems();
                List<Item> bagItems = oldBag.BagNode.GetItems();

                newBag.BagNode = oldBag.BagNode;
                RemoveBag(oldBag);
                newBag.Use();
                foreach (Item item in bagItems) {
                    if (item != newBag) {
                        AddItem(item, newBag.BagNode.IsBankNode);
                    }
                }
                AddItem(oldBag, oldBag.BagNode.IsBankNode);
                handScript.Drop();
                fromSlot = null;
            }
        }

        /// <summary>
        /// Adds an item to the inventory
        /// </summary>
        /// <param name="item"></param>
        public bool AddItem(Item item, bool addToBank) {
            Debug.Log("CharacterInventoryManager.AddItem(" + (item == null ? "null" : item.DisplayName) + ", " + addToBank + ")");
            if (item == null) {
                return false;
            }
            if (item.UniqueItem == true && GetItemCount(item.DisplayName) > 0) {
                messageFeedManager.WriteMessage(item.DisplayName + " is unique.  You can only carry one at a time.");
                return false;
            }
            if (item.MaximumStackSize > 0) {
                if (PlaceInStack(item, addToBank)) {
                    return true;
                }
            }
            //Debug.Log("About to attempt placeInEmpty");
            return PlaceInEmpty(item, addToBank);
        }

        public bool AddInventoryItem(Item item, int slotIndex) {
            if (inventorySlots.Count > slotIndex) {
                return inventorySlots[slotIndex].AddItem(item);
            }
            return AddItem(item, false);
        }

        public bool AddBankItem(Item item, int slotIndex) {
            Debug.Log("CharacterInventoryManager.AddBankItem(" + item.DisplayName + ", " + slotIndex + ")");
            if (bankSlots.Count > slotIndex) {
                return bankSlots[slotIndex].AddItem(item);
            }
            return AddItem(item, true);
        }

        public void RemoveItem(Item item) {
            foreach (InventorySlot slot in inventorySlots) {
                if (!slot.IsEmpty && SystemDataFactory.MatchResource(slot.Item.DisplayName, item.DisplayName)) {
                    slot.RemoveItem(item);
                    return;
                }
            }
        }

        private bool PlaceInEmpty(Item item, bool addToBank) {
            Debug.Log("CharacterInventoryManager.PlaceInEmpty(" + item.name + ", " + addToBank + "): checking slot");
            if (addToBank == true) {
                foreach (InventorySlot inventorySlot in bankSlots) {
                    //Debug.Log("CharacterInventoryManager.PlaceInEmpty(" + item.name + "): checking slot");
                    if (inventorySlot.IsEmpty) {
                        //Debug.Log("CharacterInventoryManager.PlaceInEmpty(" + item.name + "): checking slot: its empty.  adding item");
                        inventorySlot.AddItem(item);
                        OnItemCountChanged(item);
                        return true;
                    }
                }
            } else {
                foreach (InventorySlot inventorySlot in inventorySlots) {
                    //Debug.Log("BagPanel.AddItem(" + item.name + "): checking slot");
                    if (inventorySlot.IsEmpty) {
                        //Debug.Log("BagPanel.AddItem(" + item.name + "): checking slot: its empty.  adding item");
                        inventorySlot.AddItem(item);
                        OnItemCountChanged(item);
                        return true;
                    }
                }
            }
            if (EmptySlotCount(addToBank) == 0) {
                //Debug.Log("No empty slots");
                messageFeedManager.WriteMessage((addToBank == false ? "Inventory" : "Bank") + " is full!");
            }
            return false;
        }

        private bool PlaceInStack(Item item, bool addToBank) {
            if (addToBank == false) {
                foreach (InventorySlot inventorySlot in inventorySlots) {
                    if (inventorySlot.StackItem(item)) {
                        OnItemCountChanged(item);
                        return true;
                    }
                }
            } else {
                foreach (InventorySlot inventorySlot in bankSlots) {
                    if (inventorySlot.StackItem(item)) {
                        OnItemCountChanged(item);
                        return true;
                    }
                }
            }

            return false;
        }

        /*
        public IUseable GetUseable(IUseable useable) {
            //IUseable useable = new Stack<IUseable>();
            foreach (BagNode bagNode in bagNodes) {
                if (bagNode.Bag != null) {
                    foreach (SlotScript slot in bagNode.InventorySlots) {
                        if (!slot.IsEmpty && SystemDataFactory.MatchResource(slot.Item.DisplayName, useable.DisplayName)) {
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
                if (!slot.IsEmpty && SystemDataFactory.MatchResource(slot.Item.DisplayName, useable.DisplayName)) {
                    count += slot.Count;
                }
            }
            return count;
        }

        public void OnItemCountChanged(Item item) {
            systemEventManager.NotifyOnItemCountChanged(item);
        }

        public int GetItemCount(string type, bool partialMatch = false) {
            //Debug.Log("InventoryManager.GetItemCount(" + type + ")");
            int itemCount = 0;

            foreach (InventorySlot slot in inventorySlots) {
                if (!slot.IsEmpty && SystemDataFactory.MatchResource(slot.Item.DisplayName, type, partialMatch)) {
                    itemCount += slot.Count;
                }
            }
            foreach (InventorySlot slot in bankSlots) {
                if (!slot.IsEmpty && SystemDataFactory.MatchResource(slot.Item.DisplayName, type, partialMatch)) {
                    itemCount += slot.Count;
                }
            }

            return itemCount;
        }

        public List<Item> GetItems(string itemType, int count) {
            //Debug.Log("InventoryManager.GetItems(" + itemType + ", " + count + ")");
            List<Item> items = new List<Item>();
            foreach (InventorySlot slot in inventorySlots) {
                //Debug.Log("InventoryManager.GetItems() got bagnode and it has a bag and we are looking in a slotscript");
                if (!slot.IsEmpty && SystemDataFactory.MatchResource(slot.Item.DisplayName, itemType)) {
                    //Debug.Log("InventoryManager.GetItems() got bagnode and it has a bag and we are looking in a slotscript and the slot is not empty and it matches");
                    foreach (Item item in slot.Items) {
                        //Debug.Log("InventoryManager.GetItems() got bagnode and it has a bag and we are looking in a slotscript and the slot is not empty and it matches and we are ading and item");
                        items.Add(item);
                        if (items.Count == count) {
                            //Debug.Log("InventoryManager.GetItems() return items with count: " + items.Count);
                            return items;
                        }
                    }
                }
            }
            return items;
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


    }

}