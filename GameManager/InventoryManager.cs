using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour {

    #region Singleton
    private static InventoryManager instance;

    public static InventoryManager MyInstance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<InventoryManager>();
            }

            return instance;
        }
    }
    #endregion

    private SlotScript fromSlot;

    // BagNodes contain a bag and some metadata about the bag
    private List<BagNode> bagNodes = new List<BagNode>();

    [SerializeField]
    private GameObject inventoryContainer;

    [SerializeField]
    private string defaultBackpackItemName;

    [SerializeField]
    private string defaultBankBagItemName;

    [SerializeField]
    private GameObject windowPrefab;

    [SerializeField]
    private GameObject bagPrefab;

    [SerializeField]
    private GameObject bankBagPrefab;

    [SerializeField]
    private BagBarController bagBarController;

    [SerializeField]
    private Sprite defaultBackPackImage = null;

    // have trouble stopping grid from expanding windows, making holders instead
    [SerializeField]
    private List<GameObject> inventoryWindowHolders;

    protected CanvasGroup canvasGroup;

    private bool debugMode = false;

    // whether bag positions have been loaded
    bool bagWindowPositionsSet = false;

    protected bool startHasRun = false;
    protected bool eventReferencesInitialized = false;

    // the maximum number of bags the character can have equipped
    private int bagCount = 5;
    private int bankCount = 8;

    public int MyEmptySlotCount {
        get {
            int count = 0;
            foreach (BagNode bagNode in bagNodes) {
                if (bagNode.MyBag != null) {
                    count += bagNode.MyBagPanel.MyEmptySlotCount;
                }
            }
            return count;
        }
    }

    public int MyTotalSlotCount {
        get {
            int count = 0;
            foreach (BagNode bagNode in bagNodes) {
                if (bagNode.MyBag != null) {
                    count += bagNode.MyBagPanel.MySlots.Count;
                }
            }
            return count;
        }
    }
    public int MyFullSlotCount { get => MyTotalSlotCount - MyEmptySlotCount; }

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

    public List<BagNode> MyBagNodes { get => bagNodes; set => bagNodes = value; }
    public List<BagNode> MyBankNodes { get => bagNodes; set => bagNodes = value; }

    private void Awake() {
        //Debug.Log("InventoryManager.Awake()");
        canvasGroup = inventoryContainer.GetComponent<CanvasGroup>();
        if (defaultBackPackImage == null) {
            Debug.LogError("InventoryManager.Awake(): WARNING: DEFAULT BACKPACK IMAGE IS NOT SET! PLEASE OPEN GAMEMANAGER/INVENTORYMAGER and set it in the inspector!");
        }
    }

    private void Start() {
        //Debug.Log("InventoryManager.Start()");
        startHasRun = true;
        CreateEventReferences();
    }

    private void CreateEventReferences() {
        //Debug.Log("InventoryManager.CreateEventReferences()");
        if (eventReferencesInitialized || !startHasRun) {
            return;
        }
        SystemEventManager.MyInstance.OnPlayerConnectionDespawn += ClearData;
        eventReferencesInitialized = true;
    }

    private void CleanupEventReferences() {
        //Debug.Log("InventoryManager.CleanupEventReferences()");
        if (!eventReferencesInitialized) {
            return;
        }
        SystemEventManager.MyInstance.OnPlayerConnectionDespawn -= ClearData;
        eventReferencesInitialized = false;
    }

    public void OnDisable() {
        //Debug.Log("PlayerManager.OnDisable()");
        CleanupEventReferences();
    }

    public void ClearData() {
        //Debug.Log("InventoryManager.ClearData()");
        // keep the bag nodes, but clear their data. bag nodes are associated with physical windows and there is no point in re-initiating those
        foreach (BagNode bagNode in bagNodes) {
            //Debug.Log("InventoryManager.ClearData(): got a bag node");
            //bagNode.MyBag = null;
            // TESTING
            if (bagNode.MyIsBankNode == false) {
                //Debug.Log("Got a bag node, removing!");
                RemoveBag(bagNode.MyBag);
            } else {
                //Debug.Log("Got a bank node, not removing!");
            }
        }
        bagWindowPositionsSet = false;
        Close();
        //MyBagNodes.Clear();
    }

    public bool CanAddBag(bool addToBank = false) {
        int counter = 0;
        foreach (BagNode bagNode in bagNodes) {
            if (bagNode.MyIsBankNode == addToBank) {
                if (debugMode)
                    Debug.Log("Checking BagNode for the presence of a bag: " + counter);
                if (bagNode.MyBag == null) {
                    if (debugMode)
                        Debug.Log("found empty bagnode at:" + counter);
                    return true;
                }
                counter++;
            }
        }
        return false;
    }


    public void CreateDefaultBackpack() {
        //Debug.Log("InventoryManager.CreateDefaultBackpack()");
        Bag bag = SystemItemManager.MyInstance.GetNewResource(defaultBackpackItemName) as Bag;
        AddItem(bag, true);
        //bag.Use();
    }

    public void CreateDefaultBankBag() {
        //Debug.Log("InventoryManager.CreateDefaultBankBag()");
        Bag bag = SystemItemManager.MyInstance.GetNewResource(defaultBankBagItemName) as Bag;
        AddBag(bag, true);
    }

    public void LoadEquippedBagData(List<EquippedBagSaveData> equippedBagSaveData) {
        //Debug.Log("InventoryManager.LoadEquippedBagData()");
        int counter = 0;
        foreach (EquippedBagSaveData saveData in equippedBagSaveData) {
            if (saveData.slotCount > 0) {
                // TESTING NEW LOADING CODE TO GET REAL BACKPACK AND NOT HAVE TO WORRY ABOUT IMAGE ISSUES
                Bag newBag = SystemItemManager.MyInstance.GetNewResource(saveData.MyName) as Bag;
                if (newBag != null) {
                    AddBag(newBag, MyBagNodes[counter]);
                } else {
                    Debug.Log("InventoryManager.LoadEquippedBagData(): COULD NOT FIND BAG WITH NAME: " + saveData.MyName);
                }
                //Bag bag = Instantiate(backpackPrefab);
                //bag.Initalize(saveData.slotCount, saveData.MyName, defaultBackPackImage);
                //Debug.Log("InventoryManager.LoadEquippedBagData(): counter: " + counter + "; MyBagNodes.Count: " + MyBagNodes.Count);
                //AddBag(bag, MyBagNodes[counter]);
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
                bagNode.MyBagWindow = Instantiate(windowPrefab, inventoryWindowHolders[i].transform).GetComponent<CloseableWindow>();
                bagNode.MyBagWindow.transform.GetComponent<RectTransform>().pivot = new Vector2(1, 1);
                // create a bagbutton to access this bag node

                bagNode.MyBagButton = bagBarController.AddBagButton();
                if (bagNode.MyBagButton != null) {
                    bagNode.MyBagButton.MyBagNode = bagNode;
                } else {
                    //Debug.Log("InventoryManager.InitializeBagWindows(): create element " + i + " bagNode.MyBagButton is null!!!");
                }
                // give the bagbutton a reference back to the bag node that holds its data
                bagNode.MyIsBankNode = false;
            } else {
                if (i == bagCount) {
                    //Debug.Log("InventoryManager.InitializeBagWindows(): create element " + i + " setting bag window to bank window");
                    bagNode.MyBagWindow = PopupWindowManager.MyInstance.bankWindow;
                } else {
                    //Debug.Log("InventoryManager.InitializeBagWindows(): create element " + i + " creating bag window");
                    bagNode.MyBagWindow = Instantiate(windowPrefab, inventoryWindowHolders[i-1].transform).GetComponent<CloseableWindow>();
                    bagNode.MyBagWindow.transform.GetComponent<RectTransform>().pivot = new Vector2(1, 1);
                }

                bagNode.MyBagButton = (PopupWindowManager.MyInstance.bankWindow.MyCloseableWindowContents as BankPanel).MyBagBarController.AddBagButton();

                if (bagNode.MyBagButton != null) {
                    bagNode.MyBagButton.MyBagNode = bagNode;
                } else {
                    //Debug.Log("InventoryManager.InitializeBagWindows(): create element " + i + " bagNode.MyBagButton is null!!!");
                }

                bagNode.MyIsBankNode = true;
            }

            // save a reference to this bagNode in the main list of bagNodes
            bagNodes.Add(bagNode);
            //Debug.Log("InventoryManager.InitializeBagNodes(): added bag and bagNodes.count is now: " + bagNodes.Count);
        }
        // always update opacity immediately after load
        for (int i = 0; i < 13; i++) {
            //Debug.Log("Bag Nodes initialized. Checking node: " + i);
            if (PlayerPrefs.HasKey("InventoryWindowX" + i) && PlayerPrefs.HasKey("InventoryWindowY" + i)) {
                //InventoryManager.MyInstance.MyBagNodes[i].MyBagWindow.transform.position = new Vector3(PlayerPrefs.GetFloat("InventoryWindowX" + i), PlayerPrefs.GetFloat("InventoryWindowY" + i), 0);
                MyBagNodes[i].MyBagWindow.transform.position = new Vector3(PlayerPrefs.GetFloat("InventoryWindowX" + i), PlayerPrefs.GetFloat("InventoryWindowY" + i), 0);
                //Debug.Log("setting node:" + i + "; to: " + new Vector3(PlayerPrefs.GetFloat("InventoryWindowX" + i), PlayerPrefs.GetFloat("InventoryWindowY" + i), 0));
            } else {
                //Debug.Log(WE DON'T HAVE A WINDOW HERE!!!!!!! " + i);
            }
        }

    }


    public void AddBag(Bag bag, bool addBank = false) {
        //Debug.Log("InventoryManager.AddBag(Bag, " + addBank + ")");

        foreach (BagNode bagNode in bagNodes) {
            if (bagNode.MyBag == null && bagNode.MyIsBankNode == addBank) {
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
        //Debug.Log("InventoryManager.PopulateBagNode(BagNode, Bag)");
        bagNode.MyBag = bag;
        if (bagNode.MyIsBankNode) {
            bagNode.MyBagWindow.InitalizeWindowContents(bankBagPrefab, bag.MyName);
        } else {
            bagNode.MyBagWindow.InitalizeWindowContents(bagPrefab, bag.MyName);
        }
        bagNode.MyBagPanel = bagNode.MyBagWindow.MyCloseableWindowContents as BagPanel;
        if (bagNode.MyBagPanel == null) {
            Debug.Log("bagNode.MyBagPanel is null");
        }
        bagNode.MyBagPanel.AddSlots(bag.MySlots);
        bag.MyBagNode = bagNode;
        bag.MyBagPanel = bagNode.MyBagPanel;

        //Debug.Log("InventoryManager.PopulateBagNode(): bagNode.MyBag: " + bagNode.MyBag.GetInstanceID() + "; bagNode.MyBag.MyBagPanel: " + bagNode.MyBag.MyBagPanel.GetInstanceID() + "; bag" + bag.GetInstanceID() + "; bag.MyBagPanel: " + bag.MyBagPanel.GetInstanceID());

        //UIManager.MyInstance.CheckUISettings();
        // try that instead since we dont' need to actively update anything else
        UIManager.MyInstance.UpdateInventoryOpacity();

    }

    public void CloseBank() {
        foreach (BagNode bagNode in bagNodes) {
            if (bagNode.MyBagWindow != null && bagNode.MyIsBankNode) {
                bagNode.MyBagWindow.CloseWindow();
            }
        }
    }

    public void OpenBank() {
        foreach (BagNode bagNode in bagNodes) {
            if (bagNode.MyBagWindow != null && bagNode.MyIsBankNode && bagNode.MyBagWindow.IsOpen == false) {
                bagNode.MyBagWindow.OpenWindow();
            }
        }
    }

    public void Close() {

        foreach (BagNode bagNode in bagNodes) {
            if (bagNode.MyBagWindow != null) {
                bagNode.MyBagWindow.CloseWindow();
            }
        }
    }

    /// <summary>
    /// Removes the bag from the inventory
    /// </summary>
    /// <param name="bag"></param>
    public void RemoveBag(Bag bag) {
        //Debug.Log("InventoryManager.RemoveBag()");
        foreach (BagNode bagNode in bagNodes) {
            if (bagNode.MyBag == bag) {
                // give the old bagNode a temp location so we can add its items back to the inventory
                BagPanel tmpBagPanel = bagNode.MyBagPanel;

                // make item list before nulling the bag, because that will clear the pane slots
                List<Item> itemsToAddBack = new List<Item>();
                if (tmpBagPanel != null) {
                    foreach (Item item in tmpBagPanel.GetItems()) {
                        itemsToAddBack.Add(item);
                    }
                }

                // null the bag so the items won't get added back, as we are trying to empty it so we can remove it
                bagNode.MyBag = null;

                // bag is now gone, can add items back to inventory and they won't go back in that bag
                foreach (Item item in itemsToAddBack) {
                    AddItem(item);
                }

                // destroy the bagpanel gameobject before setting its reference to null
                bagNode.MyBagWindow.DestroyWindowContents();
                // TESTING SO THAT EMPTY BAR GOES AWAY
                bagNode.MyBagWindow.CloseWindow();


                bagNode.MyBagPanel = null;

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
        Debug.Log("InventoryManager.RemoveBag(): Did not find matching bag in bagNodes");
        //MyBagNode.MyBagButton = null;

    }

    public void SwapBags(Bag oldBag, Bag newBag) {
        int newSlotCount = (MyTotalSlotCount - oldBag.MySlots) + newBag.MySlots;

        if (newSlotCount - MyFullSlotCount >= 0) {
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
            HandScript.MyInstance.Drop();
            MyInstance.fromSlot = null;
        }
    }

    /// <summary>
    /// Adds an item to the inventory
    /// </summary>
    /// <param name="item"></param>
    public bool AddItem(Item item, bool addToBank = false) {
        if (item.MyUniqueItem == true && GetItemCount(item.MyName) > 0) {
            MessageFeedManager.MyInstance.WriteMessage(item.MyName + " is unique.  You can only carry one at a time.");
            return false;
        }
        if (item.MyStackSize > 0) {
            if (PlaceInStack(item, addToBank)) {
                return true;
            }
        }
        return PlaceInEmpty(item, addToBank);
    }

    public bool AddItem(Item item, int slotIndex) {
        //Debug.Log("InventoryManager.AddItem(" + item.MyName + ", " + slotIndex + ")");

        return GetSlots()[slotIndex].AddItem(item);
    }

    public void RemoveItem(Item item) {
        //Debug.Log("InventoryManager.RemoveItem(" + item.itemName + ")");
        foreach (BagNode bagNode in bagNodes) {
            //Debug.Log("InventoryManager.RemoveItem(" + item.itemName + "): checking bagNode");
            if (bagNode.MyBag != null) {
                //Debug.Log("InventoryManager.RemoveItem(" + item.itemName + "): checking bagNode and bag is not null");
                foreach (SlotScript slot in bagNode.MyBagPanel.MySlots) {
                    //Debug.Log("InventoryManager.RemoveItem(" + item.itemName + "): checking bagNode and bag is not null and checking slotscript");
                    if (!slot.IsEmpty && SystemResourceManager.MatchResource(slot.MyItem.MyName, item.MyName)) {
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
            if (bagNode.MyBag != null && bagNode.MyIsBankNode == addToBank) {
                if (bagNode.MyBagPanel.AddItem(item)) {
                    OnItemCountChanged(item);
                    return true;
                }
            }
        }
        return false;
    }

    private bool PlaceInStack(Item item, bool addToBank = false) {
        foreach (BagNode bagNode in bagNodes) {
            if (bagNode.MyBag != null && bagNode.MyIsBankNode == addToBank) {
                foreach (SlotScript slotScript in bagNode.MyBagPanel.MySlots) {
                    if (slotScript.StackItem(item)) {
                        OnItemCountChanged(item);
                        return true;
                    }
                }
            }
        }
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
            if (bagNode.MyBagWindow.IsOpen && bagNode.MyIsBankNode == true) {
                //Debug.Log("InventoryManager.BagsClosed(); isOpen: " + bagNode.MyBagWindow.IsOpen + "; isBankNode: " + bagNode.MyIsBankNode);
                return false;
            }
        }
        return true;
    }

    public bool BagsClosed() {
        //Debug.Log("InventoryManager.BagsClosed()");
        foreach (BagNode bagNode in bagNodes) {
            if (bagNode.MyBagWindow.IsOpen && bagNode.MyIsBankNode == false) {
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
        //Debug.Log("Inventory is closed: " + inventoryClosed);
        foreach (BagNode bagNode in bagNodes) {
            if (bagNode.MyBagWindow.IsOpen != inventoryClosed && bagNode.MyIsBankNode == false) {
                //Debug.Log("Inventory is closed: " + inventoryClosed + "; isOpen: " + bagNode.MyBagWindow.IsOpen + "; isBankNode: " + bagNode.MyIsBankNode);
                bagNode.MyBagWindow.ToggleOpenClose();
            }
        }
        UIManager.MyInstance.UpdateInventoryOpacity();
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
                //InventoryManager.MyInstance.MyBagNodes[i].MyBagWindow.transform.position = new Vector3(PlayerPrefs.GetFloat("InventoryWindowX" + i), PlayerPrefs.GetFloat("InventoryWindowY" + i), 0);
                //Debug.Log("setting node:" + i + "; to: " + new Vector3(PlayerPrefs.GetFloat("InventoryWindowX" + i), PlayerPrefs.GetFloat("InventoryWindowY" + i), 0));
                if (MyBagNodes[i].MyBagWindow.IsOpen) {
                    //Debug.Log("Window was open, moving it");
                    MyBagNodes[i].MyBagWindow.transform.position = new Vector3(PlayerPrefs.GetFloat("InventoryWindowX" + i), PlayerPrefs.GetFloat("InventoryWindowY" + i), 0);
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
                foreach (SlotScript slot in bagNode.MyBagPanel.MySlots) {
                    if (!slot.IsEmpty && SystemResourceManager.MatchResource(slot.MyItem.MyName, useable.MyName)) {
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
                foreach (SlotScript slot in bagNode.MyBagPanel.MySlots) {
                    if (!slot.IsEmpty && SystemResourceManager.MatchResource(slot.MyItem.MyName, useable.MyName)) {
                        count += slot.MyCount;
                    }
                }
            }
        }
        return count;
    }

    public void OnItemCountChanged(Item item) {
        SystemEventManager.MyInstance.NotifyOnItemCountChanged(item);
    }

    public int GetItemCount(string type) {
        //Debug.Log("InventoryManager.GetItemCount(" + type + ")");
        int itemCount = 0;

        foreach (BagNode bagNode in bagNodes) {
            if (bagNode.MyBag != null) {
                foreach (SlotScript slot in bagNode.MyBagPanel.MySlots) {
                    if (!slot.IsEmpty && SystemResourceManager.MatchResource(slot.MyItem.MyName, type)) {
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
                foreach (SlotScript slot in bagNode.MyBagPanel.MySlots) {
                    //Debug.Log("InventoryManager.GetItems() got bagnode and it has a bag and we are looking in a slotscript");
                    if (!slot.IsEmpty && SystemResourceManager.MatchResource(slot.MyItem.MyName, itemType)) {
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
                foreach (SlotScript slot in bagNode.MyBagPanel.MySlots) {
                    items.Add(slot);
                }
            }
        }
        return items;
    }


}
