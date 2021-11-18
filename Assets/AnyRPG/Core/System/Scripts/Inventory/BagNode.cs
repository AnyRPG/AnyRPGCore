using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class BagNode {

        public event System.Action<Bag> OnAddBag = delegate { };
        public event System.Action OnRemoveBag = delegate { };

        //private List<SlotScript> inventorySlots = new List<SlotScript>();
        private List<InventorySlot> inventorySlots = new List<InventorySlot>();

        private CharacterInventoryManager characterInventoryManager = null;

        private Bag bag;

        private bool isBankNode = false;

        //private BagPanel bagPanel;

        public Bag Bag { get => bag; }
        //public BagPanel BagPanel { get => bagPanel; set => bagPanel = value; }

        //public BagButton BagButton { get => bagButton; set => bagButton = value; }
        //public List<SlotScript> InventorySlots { get => inventorySlots; set => inventorySlots = value; }
        public List<InventorySlot> InventorySlots { get => inventorySlots; set => inventorySlots = value; }
        public bool IsBankNode { get => isBankNode; }

        public BagNode() {
        }

        public BagNode(CharacterInventoryManager characterInventoryManager, bool isBankNode) {
            this.characterInventoryManager = characterInventoryManager;
            this.isBankNode = isBankNode;
        }

        public void AddBag(Bag bag) {
            Debug.Log("BagNode.AddBag()");
            this.bag = bag;
            //inventorySlots = bagPanel.AddSlots(bag.Slots);
            if (isBankNode) {
                inventorySlots = characterInventoryManager.AddBankSlots(bag.Slots);
            } else {
                inventorySlots = characterInventoryManager.AddInventorySlots(bag.Slots);
            }
            bag.BagNode = this;

            OnAddBag(bag);
        }

        public void RemoveBag() {
            bag = null;
            OnRemoveBag();
            //foreach (SlotScript inventorySlot in inventorySlots) {
            foreach (InventorySlot inventorySlot in inventorySlots) {
                inventorySlot.Clear();
            }
            /*
            if (BagPanel != null) {
                BagPanel.ClearSlots(inventorySlots);
            }
            */
            characterInventoryManager.ClearSlots(inventorySlots);
            //bagPanel = null;

            ClearSlots();
        }

        public void ClearSlots() {
            inventorySlots.Clear();
        }

        public virtual List<Item> GetItems() {
            //Debug.Log("BagPanel.GetItems() slots count: " + slots.Count);
            List<Item> items = new List<Item>();

            foreach (InventorySlot inventorySlot in inventorySlots) {
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