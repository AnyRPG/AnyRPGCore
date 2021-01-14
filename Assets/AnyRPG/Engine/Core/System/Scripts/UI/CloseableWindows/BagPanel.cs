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

        public List<SlotScript> MySlots { get => slots; }

        public virtual int MyEmptySlotCount {
            get {
                int count = 0;
                foreach (SlotScript slot in MySlots) {
                    if (slot.IsEmpty) {
                        count++;
                    }
                }
                return count;
            }
        }

        public override void Awake() {
            //Debug.Log(gameObject.name + gameObject.GetInstanceID() + ".BagPanel.Awake()");
            base.Awake();
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
            foreach (SlotScript slotScript in MySlots) {
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
                SlotScript slot = Instantiate(slotPrefab, contentArea).GetComponent<SlotScript>();
                slot.MyBag = this;
                MySlots.Add(slot);
                slot.SetBackGroundColor();
            }
        }

        public virtual bool AddItem(Item item) {
            //Debug.Log("BagPanel.AddItem(" + item.name + ")");
            foreach (SlotScript slot in MySlots) {
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
                Destroy(slot.gameObject);
                //Debug.Log("BagPanel.Clear(): destroyed slot");
            }
            slots.Clear();
        }

        public override void ReceiveOpenWindowNotification() {
            base.ReceiveOpenWindowNotification();
        }

        public override void RecieveClosedWindowNotification() {
            base.RecieveClosedWindowNotification();
            foreach (SlotScript slotScript in slots) {
                slotScript.CheckMouse();
            }
        }

        public void OnDisable() {
            //Debug.Log(gameObject.name + gameObject.GetInstanceID() + ".BagPanel.OnDisable()");
        }

        public void OnDestroy() {
            //Debug.Log(gameObject.name + gameObject.GetInstanceID() + ".BagPanel.OnDestroy()");
        }
    }

}