using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class BagNode {

        public event System.Action<InstantiatedBag> OnAddBag = delegate { };
        public event System.Action OnRemoveBag = delegate { };

        //private List<SlotScript> inventorySlots = new List<SlotScript>();
        private List<InventorySlot> inventorySlots = new List<InventorySlot>();

        private CharacterInventoryManager characterInventoryManager = null;

        private InstantiatedBag instantiatedBag;

        private bool isBankNode = false;
        private int nodeIndex;

        //private BagPanel bagPanel;

        public InstantiatedBag InstantiatedBag { get => instantiatedBag; }
        //public BagPanel BagPanel { get => bagPanel; set => bagPanel = value; }

        //public BagButton BagButton { get => bagButton; set => bagButton = value; }
        //public List<SlotScript> InventorySlots { get => inventorySlots; set => inventorySlots = value; }
        public List<InventorySlot> InventorySlots { get => inventorySlots; set => inventorySlots = value; }
        public bool IsBankNode { get => isBankNode; }
        public int NodeIndex { get => nodeIndex; }

        public BagNode() {
        }

        public BagNode(CharacterInventoryManager characterInventoryManager, bool isBankNode, int nodeIndex) {
            this.characterInventoryManager = characterInventoryManager;
            this.isBankNode = isBankNode;
            this.nodeIndex = nodeIndex;
        }

        public void AddBag(InstantiatedBag instantiatedBag) {
            //Debug.Log("BagNode.AddBag()");

            this.instantiatedBag = instantiatedBag;
            //inventorySlots = bagPanel.AddSlots(bag.Slots);
            if (isBankNode) {
                inventorySlots = characterInventoryManager.AddBankSlots(instantiatedBag.Slots);
            } else {
                inventorySlots = characterInventoryManager.AddInventorySlots(instantiatedBag.Slots);
            }
            instantiatedBag.BagNode = this;

            OnAddBag(instantiatedBag);
        }

        public void RemoveBag() {
            instantiatedBag = null;
            OnRemoveBag();
            //foreach (SlotScript inventorySlot in inventorySlots) {
            foreach (InventorySlot inventorySlot in inventorySlots) {
                inventorySlot.Clear();
            }
            characterInventoryManager.ClearSlots(inventorySlots);
            ClearSlots();
        }

        public void ClearSlots() {
            inventorySlots.Clear();
        }

        public virtual List<InstantiatedItem> GetItems() {
            //Debug.Log("BagPanel.GetItems() slots count: " + slots.Count);
            List<InstantiatedItem> instantiatedItems = new List<InstantiatedItem>();

            foreach (InventorySlot inventorySlot in inventorySlots) {
                //Debug.Log("BagPanel.GetItems(): found slot");
                if (!inventorySlot.IsEmpty) {
                    //Debug.Log("BagPanel.GetItems(): found slot and it is not empty");
                    foreach (InstantiatedItem item in inventorySlot.InstantiatedItems.Values) {
                        instantiatedItems.Add(item);
                    }
                } else {
                    //Debug.Log("BagPanel.GetItems(): found slot and it is empty");
                }
            }
            return instantiatedItems;
        }


    }

}