using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class BagPanel : WindowContentController {

        [Header("Bag Panel")]

        [SerializeField]
        protected GameObject slotPrefab;

        [SerializeField]
        protected Transform contentArea;

        [SerializeField]
        protected UINavigationGrid slotController = null;

        protected List<SlotScript> slots = new List<SlotScript>();

        // game manager references
        protected ObjectPooler objectPooler = null;
        protected InventoryManager inventoryManager = null;

        public List<SlotScript> Slots { get => slots; }
        public Transform ContentArea { get => contentArea; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            objectPooler = systemGameManager.ObjectPooler;
            inventoryManager = systemGameManager.InventoryManager;
        }
        
        protected override void ProcessCreateEventSubscriptions() {
            base.ProcessCreateEventSubscriptions();

            inventoryManager.OnSetSlotBackgroundColor += HandleSetSlotBackgroundColor;

        }

        protected override void ProcessCleanupEventSubscriptions() {
            base.ProcessCleanupEventSubscriptions();

            inventoryManager.OnSetSlotBackgroundColor -= HandleSetSlotBackgroundColor;
        }

        public void HandleSetSlotBackgroundColor() {
            SetSlotColor();
        }

        /*
         // moved to BagNode
        public virtual List<Item> GetItems() {
            //Debug.Log("BagPanel.GetItems() slots count: " + slots.Count);
            List<Item> items = new List<Item>();

            foreach (SlotScript slot in slots) {
                //Debug.Log("BagPanel.GetItems(): found slot");
                if (!slot.IsEmpty) {
                    //Debug.Log("BagPanel.GetItems(): found slot and it is not empty");
                    foreach (Item item in slot.Items) {
                        items.Add(item);
                    }
                } else {
                    //Debug.Log("BagPanel.GetItems(): found slot and it is empty");
                }
            }
            return items;
        }
        */

        public void SetSlotColor() {
            foreach (SlotScript slotScript in Slots) {
                slotScript.SetBackGroundColor();
            }
        }


        /// <summary>
        /// Create slots for this bag
        /// </summary>
        /// <param name="slotCount"></param>
        public virtual List<SlotScript> AddSlots(int slotCount) {
            //Debug.Log(gameObject.name + ".BagPanel.AddSlots(" + slotCount + ")");

            List<SlotScript> returnList = new List<SlotScript>();
            for (int i = 0; i < slotCount; i++) {
                //Debug.Log(gameObject.GetInstanceID() + ".BagPanel.AddSlots(" + slotCount + "): Adding slot " + i);
                SlotScript slot = objectPooler.GetPooledObject(slotPrefab, contentArea).GetComponent<SlotScript>();
                slot.Configure(systemGameManager);
                slot.BagPanel = this;
                Slots.Add(slot);
                returnList.Add(slot);
                slot.SetBackGroundColor();
                slotController.AddActiveButton(slot);
            }
            //slotController.NumRows = Mathf.CeilToInt((float)slots.Count / 8f);

            return returnList;
        }

        public void HandleAddSlot(InventorySlot inventorySlot) {
            //Debug.Log(gameObject.name + ".BagPanel.HandleAddSlot()");
            SlotScript slot = objectPooler.GetPooledObject(slotPrefab, contentArea).GetComponent<SlotScript>();
            slot.Configure(systemGameManager);
            slot.SetInventorySlot(inventorySlot);
            slot.BagPanel = this;
            Slots.Add(slot);
            slot.SetBackGroundColor();
            slotController.AddActiveButton(slot);
            //slotController.NumRows = Mathf.CeilToInt((float) slots.Count / 8f);
        }

        public virtual void HandleRemoveSlot(InventorySlot inventorySlot) {
            //Debug.Log(gameObject.name + ".BagPanel.HandleRemoveSlot()");
            foreach (SlotScript slot in slots) {
                if (slot.InventorySlot == inventorySlot) {
                    objectPooler.ReturnObjectToPool(slot.gameObject);
                    slots.Remove(slot);
                    slotController.ClearActiveButton(slot);
                    return;
                }
            }

        }

        /*
        public virtual void Clear() {
            //Debug.Log(gameObject.name + gameObject.GetInstanceID() + ".BagPanel.Clear()");
            foreach (SlotScript slot in slots) {
                slot.Clear();
                //Debug.Log("BagPanel.Clear(): cleared slot");
            }
            slotController.ClearActiveButtons();
        }
        */

        public virtual void ClearSlots() {
            //Debug.Log(gameObject.name + gameObject.GetInstanceID() + ".BagPanel.ClearSlots()");
            List<SlotScript> removeList = new List<SlotScript>();
            foreach (SlotScript slot in slots) {
                // clearing the slot should not be necessary any more because the inventory slot exists on the character, which got despawned
                //slot.InventorySlot.Clear();
                //Debug.Log("BagPanel.Clear(): cleared slot");
                removeList.Add(slot);
            }
            foreach (SlotScript slot in removeList) {
                objectPooler.ReturnObjectToPool(slot.gameObject);
                //Debug.Log("BagPanel.Clear(): destroyed slot");
            }
            slots.Clear();
            slotController.ClearActiveButtons();
        }

        public virtual void ClearSlots(List<SlotScript> clearSlots) {
            //Debug.Log(gameObject.name + gameObject.GetInstanceID() + ".BagPanel.ClearSlots()");
            foreach (SlotScript slot in clearSlots) {
                objectPooler.ReturnObjectToPool(slot.gameObject);
                slots.Remove(slot);
                slotController.ClearActiveButton(slot);
            }

        }

        /*
        public override void ProcessOpenWindowNotification() {
            base.ProcessOpenWindowNotification();
        }
        */

        public override void ReceiveClosedWindowNotification() {
            base.ReceiveClosedWindowNotification();
            foreach (SlotScript slotScript in slots) {
                slotScript.CheckMouse();
            }
        }

       
    }

}