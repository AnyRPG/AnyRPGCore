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

        // game manager references
        private InventoryManager inventoryManager = null;
        private CraftingManager craftingManager = null;
        private SystemEventManager systemEventManager = null;

        public GameObject MyMaterialSlot { get => materialSlot; }

        public override void Init(SystemGameManager systemGameManager) {
            base.Init(systemGameManager);

            inventoryManager = systemGameManager.InventoryManager;
            craftingManager = systemGameManager.CraftingManager;
            systemEventManager = systemGameManager.SystemEventManager;
        }

        public override void UpdateVisual() {
            //Debug.Log("DescribableCraftingInputIcon.UpdateVisual()");
            base.UpdateVisual();
            description.text = Describable.DisplayName;

            //if (count > 1) {
            stackSize.text = inventoryManager.GetItemCount(Describable.DisplayName) + " / " + count.ToString();
            //} else {
            //stackSize.text = "";
            //}
            craftingManager.TriggerCraftAmountUpdated();
        }

        protected override void SetDescribableCommon(IDescribable describable) {
            base.SetDescribableCommon(describable);
            systemEventManager.OnItemCountChanged += UpdateVisual;
        }


        public override void OnDisable() {
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            base.OnDisable();
            if (inventoryManager != null) {
                systemEventManager.OnItemCountChanged -= UpdateVisual;
            }

        }

    }

}