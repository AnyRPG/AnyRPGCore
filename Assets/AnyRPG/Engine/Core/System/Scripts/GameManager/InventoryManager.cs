using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class InventoryManager : MonoBehaviour {

        #region Singleton
        private static InventoryManager instance;

        public static InventoryManager Instance {
            get {
                return instance;
            }
        }

        private void Awake() {
            instance = this;
            Init();
        }
        #endregion

        private SlotScript fromSlot;

        // BagNodes contain a bag and some metadata about the bag
        private List<BagNode> bagNodes = new List<BagNode>();

        [SerializeField]
        private GameObject inventoryContainer = null;

        [SerializeField]
        private GameObject windowPrefab = null;

        [SerializeField]
        private GameObject bagPrefab = null;

        [SerializeField]
        private GameObject bankBagPrefab = null;

        [SerializeField]
        private BagBarController bagBarController = null;

        // have trouble stopping grid from expanding windows, making holders instead
        [SerializeField]
        private List<GameObject> inventoryWindowHolders = new List<GameObject>();

        protected CanvasGroup canvasGroup = null;

        //private bool debugMode = false;

        // whether bag positions have been loaded
        //bool bagWindowPositionsSet = false;

        protected bool eventSubscriptionsInitialized = false;

        // the maximum number of bags the character can have equipped
        private int bagCount = 5;
        private int bankCount = 8;

        public int CurrentBagCount {
            get {
                int count = 0;
                foreach (BagNode bagNode in bagNodes) {
                    if (bagNode.MyBag != null && bagNode.IsBankNode == false) {
                        count++;
                    }
                }
                return count;
            }
        }

        public int TotalSlotCount {
            get {
                int count = 0;
                foreach (BagNode bagNode in bagNodes) {
                    if (bagNode.MyBag != null) {
                        count += bagNode.BagPanel.MySlots.Count;
                    }
                }
                return count;
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
                    fromSlot.MyIcon.color = Color.grey;
                }
            }
        }

        public List<BagNode> BagNodes { get => bagNodes; set => bagNodes = value; }
        public List<BagNode> BankNodes { get => bagNodes; set => bagNodes = value; }

        private void Init() {
            //Debug.Log("InventoryManager.Awake()");
            canvasGroup = inventoryContainer.GetComponent<CanvasGroup>();
        }

        private void Start() {
            //Debug.Log("InventoryManager.Start()");
            CreateEventSubscriptions();
        }

        private void CreateEventSubscriptions() {
            //Debug.Log("InventoryManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StartListening("OnPlayerConnectionDespawn", HandlePlayerConnectionDespawn);
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("InventoryManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StopListening("OnPlayerConnectionDespawn", HandlePlayerConnectionDespawn);
            eventSubscriptionsInitialized = false;
        }

        public void HandlePlayerConnectionDespawn(string eventName, EventParamProperties eventParamProperties) {
            ClearData();
        }

        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            CleanupEventSubscriptions();
        }

        public void ClearData() {
            //Debug.Log("InventoryManager.ClearData()");
            // keep the bag nodes, but clear their data. bag nodes are associated with physical windows and there is no point in re-initiating those
            foreach (BagNode bagNode in bagNodes) {
                //Debug.Log("InventoryManager.ClearData(): got a bag node");
                //bagNode.MyBag = null;
                if (bagNode.IsBankNode == false) {
                    //Debug.Log("Got a bag node, removing!");
                    RemoveBag(bagNode.MyBag, true);
                } else {
                    //Debug.Log("Got a bank node, not removing!");
                }
            }
            //bagWindowPositionsSet = false;
            Close();
            //MyBagNodes.Clear();
        }

        public int EmptySlotCount(bool bankSlot = false) {
            int count = 0;
            foreach (BagNode bagNode in bagNodes) {
                if (bagNode.MyBag != null && bagNode.IsBankNode == bankSlot) {
                    count += bagNode.BagPanel.MyEmptySlotCount;
                }
            }
            return count;
        }


        public bool CanAddBag(bool addToBank = false) {
            int counter = 0;
            foreach (BagNode bagNode in bagNodes) {
                if (bagNode.IsBankNode == addToBank) {
                    //Debug.Log("Checking BagNode for the presence of a bag: " + counter);
                    if (bagNode.MyBag == null) {
                        //Debug.Log("found empty bagnode at:" + counter);
                        return true;
                    }
                    counter++;
                }
            }
            return false;
        }


        public void CreateDefaultBackpack() {
            //Debug.Log("InventoryManager.CreateDefaultBackpack()");
            if (SystemConfigurationManager.Instance?.DefaultBackpackItem != null && SystemConfigurationManager.Instance?.DefaultBackpackItem != string.Empty) {
                Bag bag = SystemItemManager.Instance.GetNewResource(SystemConfigurationManager.Instance?.DefaultBackpackItem) as Bag;
                if (bag == null) {
                    Debug.LogError("InventoryManager.CreateDefaultBankBag(): CHECK INVENTORYMANAGER IN INSPECTOR AND SET DEFAULTBACKPACK TO VALID NAME");
                    return;
                }
                if (SystemConfigurationManager.Instance.EquipDefaultBackPack) {
                    bag.Use();
                } else {
                    AddItem(bag, true);
                }
            }
        }

        public void CreateDefaultBankBag() {
            //Debug.Log("InventoryManager.CreateDefaultBankBag()");
            if (SystemConfigurationManager.Instance?.DefaultBankBagItem == null || SystemConfigurationManager.Instance?.DefaultBankBagItem == string.Empty) {
                return;
            }
            Bag bag = SystemItemManager.Instance.GetNewResource(SystemConfigurationManager.Instance?.DefaultBankBagItem) as Bag;
            if (bag == null) {
                Debug.LogError("InventoryManager.CreateDefaultBankBag() Check SystemConfigurationManager in inspector and set defaultbankbag to valid name");
                return;
            }
            AddBag(bag, true);
        }

        public void LoadEquippedBagData(List<EquippedBagSaveData> equippedBagSaveData) {
            //Debug.Log("InventoryManager.LoadEquippedBagData()");
            int counter = 0;
            foreach (EquippedBagSaveData saveData in equippedBagSaveData) {
                if (saveData.slotCount > 0) {
                    Bag newBag = SystemItemManager.Instance.GetNewResource(saveData.MyName) as Bag;
                    if (newBag != null) {
                        AddBag(newBag, BagNodes[counter]);
                    } else {
                        //Debug.Log("InventoryManager.LoadEquippedBagData(): COULD NOT FIND BAG WITH NAME: " + saveData.MyName);
                    }
                } else {
                    //Debug.Log("InventoryManager.LoadEquippedBagData(): Bag At index: " + counter + " has no slots; MyBagNodes.Count: " + MyBagNodes.Count);
                }
                counter++;
            }
        }

        public void PerformSetupActivities() {
            InitializeBagNodes();
        }

        public void InitializeBagNodes() {
            //Debug.Log("InventoryManager.InitializeBagNodes()");
            if (bagNodes.Count > 0) {
                //Debug.Log("InventoryManager.InitializeBagNodes(): already initialized.  exiting!");
                return;
            }
            for (int i = 0; i < (bagCount + bankCount); i++) {
                //Debug.Log("InventoryManager.InitializeBagNodes(): create element " + i);

                BagNode bagNode = new BagNode();

                if (i < bagCount) {
                    // create a new BagWindow to show the contents of this bag Nodes' bag
                    bagNode.BagWindow = ObjectPooler.Instance.GetPooledObject(windowPrefab, inventoryWindowHolders[i].transform).GetComponent<CloseableWindow>();
                    bagNode.BagWindow.transform.GetComponent<RectTransform>().pivot = new Vector2(1, 1);
                    // create a bagbutton to access this bag node

                    bagNode.BagButton = bagBarController.AddBagButton();
                    if (bagNode.BagButton != null) {
                        bagNode.BagButton.MyBagNode = bagNode;
                    } else {
                        //Debug.Log("InventoryManager.InitializeBagWindows(): create element " + i + " bagNode.MyBagButton is null!!!");
                    }
                    // give the bagbutton a reference back to the bag node that holds its data
                    bagNode.IsBankNode = false;
                } else {
                    if (i == bagCount) {
                        //Debug.Log("InventoryManager.InitializeBagWindows(): create element " + i + " setting bag window to bank window");
                        bagNode.BagWindow = PopupWindowManager.Instance.bankWindow;
                    } else {
                        //Debug.Log("InventoryManager.InitializeBagWindows(): create element " + i + " creating bag window");
                        bagNode.BagWindow = ObjectPooler.Instance.GetPooledObject(windowPrefab, inventoryWindowHolders[i - 1].transform).GetComponent<CloseableWindow>();
                        bagNode.BagWindow.transform.GetComponent<RectTransform>().pivot = new Vector2(1, 1);
                    }

                    bagNode.BagButton = (PopupWindowManager.Instance.bankWindow.CloseableWindowContents as BankPanel).MyBagBarController.AddBagButton();

                    if (bagNode.BagButton != null) {
                        bagNode.BagButton.MyBagNode = bagNode;
                    } else {
                        //Debug.Log("InventoryManager.InitializeBagWindows(): create element " + i + " bagNode.MyBagButton is null!!!");
                    }

                    bagNode.IsBankNode = true;
                }

                // save a reference to this bagNode in the main list of bagNodes
                bagNodes.Add(bagNode);
                //Debug.Log("InventoryManager.InitializeBagNodes(): added bag and bagNodes.count is now: " + bagNodes.Count);
            }
            // always update opacity immediately after load
            for (int i = 0; i < 13; i++) {
                //Debug.Log("Bag Nodes initialized. Checking node: " + i);
                if (PlayerPrefs.HasKey("InventoryWindowX" + i) && PlayerPrefs.HasKey("InventoryWindowY" + i)) {
                    //InventoryManager.Instance.MyBagNodes[i].MyBagWindow.transform.position = new Vector3(PlayerPrefs.GetFloat("InventoryWindowX" + i), PlayerPrefs.GetFloat("InventoryWindowY" + i), 0);
                    BagNodes[i].BagWindow.transform.position = new Vector3(PlayerPrefs.GetFloat("InventoryWindowX" + i), PlayerPrefs.GetFloat("InventoryWindowY" + i), 0);
                    //Debug.Log("setting node:" + i + "; to: " + new Vector3(PlayerPrefs.GetFloat("InventoryWindowX" + i), PlayerPrefs.GetFloat("InventoryWindowY" + i), 0));
                } else {
                    //Debug.Log(WE DON'T HAVE A WINDOW HERE!!!!!!! " + i);
                }
            }

        }


        public void AddBag(Bag bag, bool addBank = false) {
            //Debug.Log("InventoryManager.AddBag(Bag, " + addBank + ")");

            foreach (BagNode bagNode in bagNodes) {
                if (bagNode.MyBag == null && bagNode.IsBankNode == addBank) {
                    PopulateBagNode(bagNode, bag);
                    //bags.Add(bag);
                    //bag.MyBagButton = bagButton;
                    //bag.MyBagScript.transform.SetSiblingIndex(bagButton.MyBagIndex);
                    break;
                }
            }
        }

        public void AddBag(Bag bag, BagNode bagNode) {
            //Debug.Log("InventoryManager.AddBag(Bag, BagNode)");
            foreach (BagNode _bagNode in bagNodes) {
                if (_bagNode == bagNode) {
                    PopulateBagNode(bagNode, bag);
                    return;
                }
            }
            //bags.Add(bag);
            //bagButton.MyBag = bag;
            //bag.MyBagScript.transform.SetSiblingIndex(bagButton.MyBagIndex);
        }

        private void PopulateBagNode(BagNode bagNode, Bag bag) {
            //Debug.Log("InventoryManager.PopulateBagNode(" + (bagNode != null ? bagNode.ToString() : "null") + ", " + (bag != null ? bag.DisplayName : "null") + ")");
            if (bag != null) {
                bagNode.MyBag = bag;
                if (bagNode.IsBankNode) {
                    if (bagNode.BagWindow != null) {
                        bagNode.BagWindow.InitalizeWindowContents(bankBagPrefab, bag.DisplayName);
                    } else {
                        //Debug.Log("InventoryManager.PopulateBagNode(BagNode, Bag): bagwindow was null");
                    }
                } else {
                    bagNode.BagWindow.InitalizeWindowContents(bagPrefab, bag.DisplayName);
                }
                bagNode.BagPanel = bagNode.BagWindow.CloseableWindowContents as BagPanel;
                if (bagNode.BagPanel != null) {
                    //Debug.Log("InventoryManager.PopulateBagNode() bagPanel: " + bagNode.MyBagPanel.gameObject.GetInstanceID() + " for window: " + bagNode.MyBagWindow.gameObject.name);
                    bagNode.BagPanel.AddSlots(bag.MySlots);
                    bag.MyBagNode = bagNode;
                    bag.MyBagPanel = bagNode.BagPanel;
                }
            }

            //Debug.Log("InventoryManager.PopulateBagNode(): bagNode.MyBag: " + bagNode.MyBag.GetInstanceID() + "; bagNode.MyBag.MyBagPanel: " + bagNode.MyBag.MyBagPanel.GetInstanceID() + "; bag" + bag.GetInstanceID() + "; bag.MyBagPanel: " + bag.MyBagPanel.GetInstanceID());

            UIManager.Instance.UpdateInventoryOpacity();

        }

        public void CloseBank() {
            foreach (BagNode bagNode in bagNodes) {
                if (bagNode.BagWindow != null && bagNode.IsBankNode) {
                    bagNode.BagWindow.CloseWindow();
                }
            }
        }

        public void OpenBank() {
            foreach (BagNode bagNode in bagNodes) {
                if (bagNode.BagWindow != null && bagNode.IsBankNode && bagNode.BagWindow.IsOpen == false) {
                    bagNode.BagWindow.OpenWindow();
                }
            }
        }

        public void Close() {

            foreach (BagNode bagNode in bagNodes) {
                if (bagNode.BagWindow != null) {
                    bagNode.BagWindow.CloseWindow();
                }
            }
        }

        /// <summary>
        /// Removes the bag from the inventory
        /// </summary>
        /// <param name="bag"></param>
        public void RemoveBag(Bag bag, bool clearOnly = false) {
            //Debug.Log("InventoryManager.RemoveBag()");
            foreach (BagNode bagNode in bagNodes) {
                if (bagNode.MyBag == bag) {
                    // give the old bagNode a temp location so we can add its items back to the inventory
                    BagPanel tmpBagPanel = bagNode.BagPanel;

                    // make item list before nulling the bag, because that will clear the pane slots
                    List<Item> itemsToAddBack = new List<Item>();
                    if (tmpBagPanel != null) {
                        foreach (Item item in tmpBagPanel.GetItems()) {
                            itemsToAddBack.Add(item);
                        }
                    }

                    // null the bag so the items won't get added back, as we are trying to empty it so we can remove it
                    bagNode.MyBag = null;

                    if (!clearOnly) {
                        // bag is now gone, can add items back to inventory and they won't go back in that bag
                        foreach (Item item in itemsToAddBack) {
                            AddItem(item);
                        }
                    }

                    // destroy the bagpanel gameobject before setting its reference to null
                    bagNode.BagWindow.DestroyWindowContents();

                    // MAKE EMPTY TITLE BAR GO AWAY
                    bagNode.BagWindow.CloseWindow();


                    bagNode.BagPanel = null;

                    // remove references the bag held to the node it belonged to and the panel it spawned
                    if (bag != null) {
                        if (bag.MyBagNode != null) {
                            bag.MyBagNode = null;
                        }
                        if (bag.MyBagPanel != null) {
                            bag.MyBagPanel = null;
                        }
                    }

                    return;
                }
            }
            //Debug.Log("InventoryManager.RemoveBag(): Did not find matching bag in bagNodes");
            //MyBagNode.MyBagButton = null;

        }

        public void SwapBags(Bag oldBag, Bag newBag) {
            int newSlotCount = (TotalSlotCount - oldBag.MySlots) + newBag.MySlots;

            if (newSlotCount - FullSlotCount >= 0) {
                // do swap
                List<Item> bagItems = oldBag.MyBagPanel.GetItems();

                newBag.MyBagNode = oldBag.MyBagNode;
                RemoveBag(oldBag);
                newBag.Use();
                foreach (Item item in bagItems) {
                    if (item != newBag) {
                        AddItem(item);
                    }
                }
                AddItem(oldBag);
                HandScript.Instance.Drop();
                Instance.fromSlot = null;
            }
        }

        /// <summary>
        /// Adds an item to the inventory
        /// </summary>
        /// <param name="item"></param>
        public bool AddItem(Item item, bool addToBank = false) {
            if (item == null) {
                return false;
            }
            if (item.MyUniqueItem == true && GetItemCount(item.DisplayName) > 0) {
                MessageFeedManager.Instance.WriteMessage(item.DisplayName + " is unique.  You can only carry one at a time.");
                return false;
            }
            if (item.MyMaximumStackSize > 0) {
                if (PlaceInStack(item, addToBank)) {
                    return true;
                }
            }
            //Debug.Log("About to attempt placeInEmpty");
            return PlaceInEmpty(item, addToBank);
        }

        public bool AddItem(Item item, int slotIndex) {
            //Debug.Log("InventoryManager.AddItem(" + item.MyName + ", " + slotIndex + ")");
            if (GetSlots().Count > slotIndex) {
                return GetSlots()[slotIndex].AddItem(item);
            }
            return AddItem(item);
        }

        public void RemoveItem(Item item) {
            //Debug.Log("InventoryManager.RemoveItem(" + item.MyName + ")");
            foreach (BagNode bagNode in bagNodes) {
                //Debug.Log("InventoryManager.RemoveItem(" + item.itemName + "): checking bagNode");
                if (bagNode.MyBag != null) {
                    //Debug.Log("InventoryManager.RemoveItem(" + item.itemName + "): checking bagNode and bag is not null");
                    foreach (SlotScript slot in bagNode.BagPanel.MySlots) {
                        //Debug.Log("InventoryManager.RemoveItem(" + item.itemName + "): checking bagNode and bag is not null and checking slotscript");
                        if (!slot.IsEmpty && SystemResourceManager.MatchResource(slot.MyItem.DisplayName, item.DisplayName)) {
                            //Debug.Log("InventoryManager.RemoveItem(" + item.itemName + "): about to remove item from slot");
                            slot.RemoveItem(item);
                            return;
                        }
                    }
                }
            }
        }

        private bool PlaceInEmpty(Item item, bool addToBank = false) {
            foreach (BagNode bagNode in bagNodes) {
                if (bagNode.MyBag != null && bagNode.IsBankNode == addToBank) {
                    if (bagNode.BagPanel.AddItem(item)) {
                        OnItemCountChanged(item);
                        return true;
                    }
                }
            }
            if (EmptySlotCount(addToBank) == 0) {
                //Debug.Log("No empty slots");
                MessageFeedManager.Instance.WriteMessage((addToBank == false ? "Inventory" : "Bank") + " is full!");
            }
            return false;
        }

        private bool PlaceInStack(Item item, bool addToBank = false) {
            foreach (BagNode bagNode in bagNodes) {
                if (bagNode.MyBag != null && bagNode.IsBankNode == addToBank) {
                    foreach (SlotScript slotScript in bagNode.BagPanel.MySlots) {
                        if (slotScript.StackItem(item)) {
                            OnItemCountChanged(item);
                            return true;
                        }
                    }
                }
            }
            // commented below because it may result in 2 full messages being displayed if the last item in the bag is a full stack of the same item type
            /*
            if (MyEmptySlotCount(addToBank) == 0) {
                MessageFeedManager.Instance.WriteMessage((addToBank == false ? "Inventory" : "Bank") + " is full!");
            }
            */
            return false;
        }

        public bool InventoryClosed() {
            /*
            if (canvasGroup.alpha == 0) {
                return true;
            }
            return false;
            */
            return BagsClosed();
        }

        public bool BankClosed() {
            //Debug.Log("InventoryManager.BankClosed()");
            foreach (BagNode bagNode in bagNodes) {
                if (bagNode.BagWindow.IsOpen && bagNode.IsBankNode == true) {
                    //Debug.Log("InventoryManager.BagsClosed(); isOpen: " + bagNode.MyBagWindow.IsOpen + "; isBankNode: " + bagNode.MyIsBankNode);
                    return false;
                }
            }
            return true;
        }

        public bool BagsClosed() {
            //Debug.Log("InventoryManager.BagsClosed()");
            foreach (BagNode bagNode in bagNodes) {
                if (bagNode.BagWindow.IsOpen && bagNode.IsBankNode == false) {
                    //Debug.Log("InventoryManager.BagsClosed(); isOpen: " + bagNode.MyBagWindow.IsOpen + "; isBankNode: " + bagNode.MyIsBankNode);
                    return false;
                }
            }
            return true;
        }

        public void OpenClose() {
            //Debug.Log("InventoryManager.OpenClose()");
            // if the closed bag is true, open all closed bags
            // if closed bag is false, then close all open bags
            bool inventoryClosed = InventoryClosed();
            if (CurrentBagCount == 0) {
                MessageFeedManager.Instance.WriteMessage("You do not have any bags equipped");
                return;
            }
            //Debug.Log("Inventory is closed: " + inventoryClosed);
            foreach (BagNode bagNode in bagNodes) {
                if (bagNode.BagWindow.IsOpen != inventoryClosed && bagNode.IsBankNode == false) {
                    //Debug.Log("Inventory is closed: " + inventoryClosed + "; isOpen: " + bagNode.MyBagWindow.IsOpen + "; isBankNode: " + bagNode.MyIsBankNode);
                    bagNode.BagWindow.ToggleOpenClose();
                }
            }
            UIManager.Instance.UpdateInventoryOpacity();
            // that may look wrong, but it will still read as closed, because we opened it after taking that reading
            //if (inventoryClosed) {
            SetWindowPositions();
            //}

        }

        public void SetWindowPositions() {
            //Debug.Log("InventoryManager.SetWindowPositions()");

            for (int i = 0; i < 13; i++) {
                //Debug.Log("Checking window " + i + " on openclose");
                if (PlayerPrefs.HasKey("InventoryWindowX" + i) && PlayerPrefs.HasKey("InventoryWindowY" + i)) {
                    //InventoryManager.Instance.MyBagNodes[i].MyBagWindow.transform.position = new Vector3(PlayerPrefs.GetFloat("InventoryWindowX" + i), PlayerPrefs.GetFloat("InventoryWindowY" + i), 0);
                    //Debug.Log("setting node:" + i + "; to: " + new Vector3(PlayerPrefs.GetFloat("InventoryWindowX" + i), PlayerPrefs.GetFloat("InventoryWindowY" + i), 0));
                    if (BagNodes[i].BagWindow.IsOpen) {
                        //Debug.Log("Window was open, moving it");
                        BagNodes[i].BagWindow.transform.position = new Vector3(PlayerPrefs.GetFloat("InventoryWindowX" + i), PlayerPrefs.GetFloat("InventoryWindowY" + i), 0);
                        //Debug.Log("Window was open, moving it: " + MyBagNodes[i].MyBagWindow.transform.position);
                    } else {
                        //Debug.Log("Window was closed, not moving it");
                    }
                }
            }
        }

        public IUseable GetUseable(IUseable useable) {
            //IUseable useable = new Stack<IUseable>();
            foreach (BagNode bagNode in bagNodes) {
                if (bagNode.MyBag != null) {
                    foreach (SlotScript slot in bagNode.BagPanel.MySlots) {
                        if (!slot.IsEmpty && SystemResourceManager.MatchResource(slot.MyItem.DisplayName, useable.DisplayName)) {
                            return (slot.MyItem as IUseable);
                        }
                    }
                }
            }
            return null;
            //return useables;
        }

        public int GetUseableCount(IUseable useable) {
            int count = 0;
            foreach (BagNode bagNode in bagNodes) {
                if (bagNode.MyBag != null) {
                    foreach (SlotScript slot in bagNode.BagPanel.MySlots) {
                        if (!slot.IsEmpty && SystemResourceManager.MatchResource(slot.MyItem.DisplayName, useable.DisplayName)) {
                            count += slot.MyCount;
                        }
                    }
                }
            }
            return count;
        }

        public void OnItemCountChanged(Item item) {
            SystemEventManager.Instance.NotifyOnItemCountChanged(item);
        }

        public int GetItemCount(string type) {
            //Debug.Log("InventoryManager.GetItemCount(" + type + ")");
            int itemCount = 0;

            foreach (BagNode bagNode in bagNodes) {
                if (bagNode.MyBag != null) {
                    foreach (SlotScript slot in bagNode.BagPanel.MySlots) {
                        if (!slot.IsEmpty && SystemResourceManager.MatchResource(slot.MyItem.DisplayName, type)) {
                            itemCount += slot.MyCount;
                        }
                    }
                }
            }

            return itemCount;
        }

        public List<Item> GetItems(string itemType, int count) {
            //Debug.Log("InventoryManager.GetItems(" + itemType + ", " + count + ")");
            List<Item> items = new List<Item>();
            foreach (BagNode bagNode in bagNodes) {
                //Debug.Log("InventoryManager.GetItems() got bagnode");
                if (bagNode.MyBag != null) {
                    //Debug.Log("InventoryManager.GetItems() got bagnode and it has a bag");
                    foreach (SlotScript slot in bagNode.BagPanel.MySlots) {
                        //Debug.Log("InventoryManager.GetItems() got bagnode and it has a bag and we are looking in a slotscript");
                        if (!slot.IsEmpty && SystemResourceManager.MatchResource(slot.MyItem.DisplayName, itemType)) {
                            //Debug.Log("InventoryManager.GetItems() got bagnode and it has a bag and we are looking in a slotscript and the slot is not empty and it matches");
                            foreach (Item item in slot.MyItems) {
                                //Debug.Log("InventoryManager.GetItems() got bagnode and it has a bag and we are looking in a slotscript and the slot is not empty and it matches and we are ading and item");
                                items.Add(item);
                                if (items.Count == count) {
                                    //Debug.Log("InventoryManager.GetItems() return items with count: " + items.Count);
                                    return items;
                                }
                            }
                        }
                    }
                }
            }
            return items;
        }

        public List<SlotScript> GetSlots() {
            List<SlotScript> items = new List<SlotScript>();
            foreach (BagNode bagNode in bagNodes) {
                if (bagNode.MyBag != null) {
                    foreach (SlotScript slot in bagNode.BagPanel.MySlots) {
                        items.Add(slot);
                    }
                }
            }
            return items;
        }


    }

}