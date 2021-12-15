using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AnyRPG {

    public class InventoryManager : ConfiguredMonoBehaviour {

        public event System.Action<BagNode> OnAddInventoryBagNode = delegate { };
        public event System.Action<BagNode> OnAddBankBagNode = delegate { };
        public event System.Action<InventorySlot> OnAddInventorySlot = delegate { };
        public event System.Action<InventorySlot> OnAddBankSlot = delegate { };
        public event System.Action<InventorySlot> OnRemoveInventorySlot = delegate { };
        public event System.Action<InventorySlot> OnRemoveBankSlot = delegate { };
        public event System.Action OnSetSlotBackgroundColor = delegate { };

        public void AddInventoryBagNode(BagNode bagNode) {
            OnAddInventoryBagNode(bagNode);
        }

        public void AddBankBagNode(BagNode bagNode) {
            OnAddBankBagNode(bagNode);
        }

        public void AddInventorySlot(InventorySlot inventorySlot) {
            OnAddInventorySlot(inventorySlot);
        }

        public void AddBankSlot(InventorySlot inventorySlot) {
            OnAddBankSlot(inventorySlot);
        }

        public void RemoveInventorySlot(InventorySlot inventorySlot) {
            OnRemoveInventorySlot(inventorySlot);
        }

        public void RemoveBankSlot(InventorySlot inventorySlot) {
            OnRemoveBankSlot(inventorySlot);
        }

        public void SetSlotBackgroundColor() {
            OnSetSlotBackgroundColor();
        }

    }

}