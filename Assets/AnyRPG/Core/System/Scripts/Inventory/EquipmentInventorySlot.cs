using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class EquipmentInventorySlot : InventorySlot {

        public event System.Action<EquipmentInventorySlot> OnUpdateEquipmentSlot = delegate { };
        public event System.Action<EquipmentInventorySlot, InstantiatedEquipment> OnAddEquipment = delegate { };
        public event System.Action<EquipmentInventorySlot, InstantiatedEquipment> OnRemoveEquipment = delegate { };

        protected InstantiatedEquipment instantiatedEquipment;

        public InstantiatedEquipment InstantiatedEquipment {
            get { return instantiatedEquipment; }
        }

        public EquipmentInventorySlot(SystemGameManager systemGameManager) : base(systemGameManager) {
            Configure(systemGameManager);
        }

        protected override void UpdateSlot() {
            if (instantiatedItems.Count > 0 && instantiatedItems.First().Value is InstantiatedEquipment) {
                instantiatedEquipment = instantiatedItems.First().Value as InstantiatedEquipment;
            } else {
                instantiatedEquipment = null;
            }
            base.UpdateSlot();
            OnUpdateEquipmentSlot(this);
        }

        public override void NotifyOnAddItem(InstantiatedItem instantiatedItem) {
            //Debug.Log($"EquipmentInventorySlot.NotifyOnAddItem({instantiatedItem.ResourceName}) (instance: {GetHashCode()})");

            base.NotifyOnAddItem(instantiatedItem);
            if (instantiatedItem is InstantiatedEquipment) {
                OnAddEquipment(this, instantiatedItem as InstantiatedEquipment);
            }
        }

        public override void NotifyOnRemoveItem(InstantiatedItem instantiatedItem) {
            //Debug.Log($"EquipmentInventorySlot.NotifyOnRemoveItem({instantiatedItem.ResourceName})");

            base.NotifyOnRemoveItem(instantiatedItem);
            if (instantiatedItem is InstantiatedEquipment) {
                OnRemoveEquipment(this, instantiatedItem as InstantiatedEquipment);
            }
        }
    }

}