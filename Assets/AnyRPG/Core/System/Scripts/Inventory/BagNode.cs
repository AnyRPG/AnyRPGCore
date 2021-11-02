using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class BagNode {

        public event System.Action<Bag> OnAddBag = delegate { };
        public event System.Action OnRemoveBag = delegate { };

        /*
        [SerializeField]
        private BagButton bagButton;
        */

        [SerializeField]
        private bool isBankNode = false;

        //private CloseableWindow bagWindow;

        private List<SlotScript> inventorySlots = new List<SlotScript>();

        private Bag bag;

        private BagPanel bagPanel;


        public Bag Bag { get => bag; }
        public BagPanel BagPanel { get => bagPanel; set => bagPanel = value; }
        public bool IsBankNode { get => isBankNode; set => isBankNode = value; }
        //public BagButton BagButton { get => bagButton; set => bagButton = value; }
        public List<SlotScript> InventorySlots { get => inventorySlots; set => inventorySlots = value; }
        //public CloseableWindow BagWindow { get => bagWindow; set => bagWindow = value; }
        public virtual int EmptySlotCount {
            get {
                int count = 0;
                foreach (SlotScript slot in inventorySlots) {
                    if (slot.IsEmpty) {
                        count++;
                    }
                }
                return count;
            }
        }

        public void AddBag(Bag bag) {
            this.bag = bag;
            if (bagPanel != null) {
                //Debug.Log("InventoryManager.PopulateBagNode() bagPanel: " + bagNode.MyBagPanel.gameObject.GetInstanceID() + " for window: " + bagNode.MyBagWindow.gameObject.name);
                inventorySlots = bagPanel.AddSlots(bag.Slots);
                bag.BagNode = this;
                bag.BagPanel = bagPanel;
            }

            OnAddBag(bag);
        }

        public void RemoveBag() {
            bag = null;
            OnRemoveBag();
            foreach (SlotScript inventorySlot in inventorySlots) {
                inventorySlot.Clear();
            }
            if (BagPanel != null) {
                BagPanel.ClearSlots(inventorySlots);
            }
            bagPanel = null;

            ClearSlots();
        }

        public void ClearSlots() {
            inventorySlots.Clear();
        }

        public virtual bool AddItem(Item item) {
            //Debug.Log("BagPanel.AddItem(" + item.name + ")");
            foreach (SlotScript slot in inventorySlots) {
                //Debug.Log("BagPanel.AddItem(" + item.name + "): checking slot");
                if (slot.IsEmpty) {
                    //Debug.Log("BagPanel.AddItem(" + item.name + "): checking slot: its empty.  adding item");
                    slot.AddItem(item);
                    return true;
                }
            }
            return false;
        }

        public virtual List<Item> GetItems() {
            //Debug.Log("BagPanel.GetItems() slots count: " + slots.Count);
            List<Item> items = new List<Item>();

            foreach (SlotScript inventorySlot in inventorySlots) {
                //Debug.Log("BagPanel.GetItems(): found slot");
                if (!inventorySlot.IsEmpty) {
                    //Debug.Log("BagPanel.GetItems(): found slot and it is not empty");
                    foreach (Item item in inventorySlot.Items) {
                        items.Add(item);
                    }
                } else {
                    //Debug.Log("BagPanel.GetItems(): found slot and it is empty");
                }
            }
            return items;
        }


    }

}