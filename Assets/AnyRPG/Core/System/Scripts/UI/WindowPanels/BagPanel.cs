using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class BagPanel : WindowPanel {

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
        protected PlayerManagerClient playerManagerClient = null;

        public List<SlotScript> Slots { get => slots; }
        public Transform ContentArea { get => contentArea; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            objectPooler = systemGameManager.ObjectPooler;
            playerManagerClient = systemGameManager.PlayerManagerClient;
        }

        protected override void ProcessCreateEventSubscriptions() {
            base.ProcessCreateEventSubscriptions();

            systemEventManager.OnSetSlotBackgroundColor += HandleSetSlotBackgroundColor;
        }

        protected override void ProcessCleanupEventSubscriptions() {
            base.ProcessCleanupEventSubscriptions();

            systemEventManager.OnSetSlotBackgroundColor -= HandleSetSlotBackgroundColor;
        }

        public void HandleSetSlotBackgroundColor() {
            SetSlotColor();
        }

        public void SetSlotColor() {
            foreach (SlotScript slotScript in Slots) {
                slotScript.SetBackGroundColor();
            }
        }


        public void HandleAddSlot(InventorySlot inventorySlot) {
            //Debug.Log($"{gameObject.name}.BagPanel.HandleAddSlot()");

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
            //Debug.Log($"{gameObject.name}.BagPanel.HandleRemoveSlot()");
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
            //Debug.Log($"{gameObject.name}.BagPanel.ClearSlots() instanceId: {GetInstanceID()} slots count: {slots.Count}");

            List<SlotScript> removeList = new List<SlotScript>();
            foreach (SlotScript slot in slots) {
                // clearing the slot should not be necessary any more because the inventory slot exists on the character, which got despawned
                // above line is incorrect.  must give slot chance to clear icons because of object pooling
                //slot.InventorySlot.Clear();
                slot.OnSendObjectToPool();
                //Debug.Log(gameObject.name + gameObject.GetInstanceID() + ".BagPanel.ClearSlots() adding slot to clear list");
                removeList.Add(slot);
            }
            foreach (SlotScript slot in removeList) {
                //Debug.Log(gameObject.name + gameObject.GetInstanceID() + ".BagPanel.ClearSlots() sending slot to object pool");
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

        public override void ReceiveClosedWindowNotification() {
            base.ReceiveClosedWindowNotification();
            foreach (SlotScript slotScript in slots) {
                slotScript.CheckMouse();
            }
        }

        public virtual void DropItemFromInventorySlot(SlotScript toSlot, SlotScript fromSlot) {
            // meant to be overridden
        }

        public virtual void SwapItemFromNonInventorySlot(SlotScript slotScript, InstantiatedItem instantiatedItem) {
            // meant to be overridden
        }

        public virtual void DropItemFromNonInventorySlot(SlotScript slotScript, InstantiatedItem instantiatedItem) {
            // meant to be overridden
        }

        public virtual void SetupContextMenu(ContextMenuPanel contextMenuPanel, InventorySlot inventorySlot) {
            // meant to be overridden
        }

        public virtual void PerformContextMenuAction(SlotScript slotScript, string actionName) {
            // meant to be overridden
        }

        public virtual void PerformContextMenuAction(BagButton bagButton, string actionName) {
            // meant to be overridden
        }
    }

}