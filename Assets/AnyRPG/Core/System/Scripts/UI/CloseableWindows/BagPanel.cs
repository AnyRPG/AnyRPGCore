using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class BagPanel : WindowContentController {

        [SerializeField]
        protected GameObject slotPrefab;

        [SerializeField]
        protected Transform contentArea;

        protected List<SlotScript> slots = new List<SlotScript>();

        // game manager references
        protected ObjectPooler objectPooler = null;

        public List<SlotScript> Slots { get => slots; }

        public virtual int EmptySlotCount {
            get {
                int count = 0;
                foreach (SlotScript slot in Slots) {
                    if (slot.IsEmpty) {
                        count++;
                    }
                }
                return count;
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            objectPooler = systemGameManager.ObjectPooler;
        }

        public virtual List<Item> GetItems() {
            //Debug.Log("BagPanel.GetItems() slots count: " + slots.Count);
            List<Item> items = new List<Item>();

            foreach (SlotScript slot in slots) {
                //Debug.Log("BagPanel.GetItems(): found slot");
                if (!slot.IsEmpty) {
                    //Debug.Log("BagPanel.GetItems(): found slot and it is not empty");
                    foreach (Item item in slot.MyItems) {
                        items.Add(item);
                    }
                } else {
                    //Debug.Log("BagPanel.GetItems(): found slot and it is empty");
                }
            }
            return items;
        }

        public void SetSlotColor() {
            foreach (SlotScript slotScript in Slots) {
                slotScript.SetBackGroundColor();
            }
        }


        /// <summary>
        /// Create slots for this bag
        /// </summary>
        /// <param name="slotCount"></param>
        public virtual void AddSlots(int slotCount) {
            //Debug.Log(gameObject.name + gameObject.GetInstanceID() + ".BagPanel.AddSlots(" + slotCount + ")");
            for (int i = 0; i < slotCount; i++) {
                //Debug.Log(gameObject.GetInstanceID() + ".BagPanel.AddSlots(" + slotCount + "): Adding slot " + i);
                SlotScript slot = objectPooler.GetPooledObject(slotPrefab, contentArea).GetComponent<SlotScript>();
                slot.Configure(systemGameManager);
                slot.MyBag = this;
                Slots.Add(slot);
                slot.SetBackGroundColor();
            }
        }

        public virtual bool AddItem(Item item) {
            //Debug.Log("BagPanel.AddItem(" + item.name + ")");
            foreach (SlotScript slot in Slots) {
                //Debug.Log("BagPanel.AddItem(" + item.name + "): checking slot");
                if (slot.IsEmpty) {
                    //Debug.Log("BagPanel.AddItem(" + item.name + "): checking slot: its empty.  adding item");
                    slot.AddItem(item);
                    return true;
                }
            }
            return false;
        }

        public virtual void Clear() {
            //Debug.Log(gameObject.name + gameObject.GetInstanceID() + ".BagPanel.Clear()");
            foreach (SlotScript slot in slots) {
                slot.Clear();
                //Debug.Log("BagPanel.Clear(): cleared slot");
            }
        }

        public virtual void ClearSlots() {
            //Debug.Log(gameObject.name + gameObject.GetInstanceID() + ".BagPanel.ClearSlots()");
            List<SlotScript> removeList = new List<SlotScript>();
            foreach (SlotScript slot in slots) {
                slot.Clear();
                //Debug.Log("BagPanel.Clear(): cleared slot");
                removeList.Add(slot);
            }
            foreach (SlotScript slot in removeList) {
                objectPooler.ReturnObjectToPool(slot.gameObject);
                //Debug.Log("BagPanel.Clear(): destroyed slot");
            }
            slots.Clear();
        }

        public override void ReceiveOpenWindowNotification() {
            base.ReceiveOpenWindowNotification();
        }

        public override void ReceiveClosedWindowNotification() {
            base.ReceiveClosedWindowNotification();
            foreach (SlotScript slotScript in slots) {
                slotScript.CheckMouse();
            }
        }

       
    }

}