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

        //private BagPanel bagPanel;

        public Bag Bag { get => bag; }
        //public BagPanel BagPanel { get => bagPanel; set => bagPanel = value; }

        //public BagButton BagButton { get => bagButton; set => bagButton = value; }
        //public List<SlotScript> InventorySlots { get => inventorySlots; set => inventorySlots = value; }
        public List<InventorySlot> InventorySlots { get => inventorySlots; set => inventorySlots = value; }


        public BagNode() {
        }

        public BagNode(CharacterInventoryManager characterInventoryManager) {
            this.characterInventoryManager = characterInventoryManager;
        }

        public void AddBag(Bag bag) {
            this.bag = bag;
            //Debug.Log("InventoryManager.PopulateBagNode() bagPanel: " + bagNode.MyBagPanel.gameObject.GetInstanceID() + " for window: " + bagNode.MyBagWindow.gameObject.name);
            //inventorySlots = bagPanel.AddSlots(bag.Slots);
            inventorySlots = characterInventoryManager.AddSlots(bag.Slots);
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