using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class DescribableCraftingInputIcon : DescribableIcon {
        [SerializeField]
        private TextMeshProUGUI description = null;

        [SerializeField]
        private GameObject materialSlot = null;

        public GameObject MyMaterialSlot { get => materialSlot; }

        public override void UpdateVisual() {
            //Debug.Log("DescribableCraftingInputIcon.UpdateVisual()");
            base.UpdateVisual();
            description.text = Describable.DisplayName;

            //if (count > 1) {
            stackSize.text = InventoryManager.Instance.GetItemCount(Describable.DisplayName) + " / " + count.ToString();
            //} else {
            //stackSize.text = "";
            //}
            SystemGameManager.Instance.CraftingManager.TriggerCraftAmountUpdated();
        }

        protected override void SetDescribableCommon(IDescribable describable) {
            base.SetDescribableCommon(describable);
            SystemGameManager.Instance.EventManager.OnItemCountChanged += UpdateVisual;
        }


        public override void OnDisable() {
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            base.OnDisable();
            if (InventoryManager.Instance != null) {
                SystemGameManager.Instance.EventManager.OnItemCountChanged -= UpdateVisual;
            }

        }

    }

}