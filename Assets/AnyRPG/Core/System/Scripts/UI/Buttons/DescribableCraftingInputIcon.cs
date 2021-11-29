using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class DescribableCraftingInputIcon : DescribableCraftingIcon {
        
        [Header("Crafting Input")]

        [SerializeField]
        private TextMeshProUGUI description = null;

        [SerializeField]
        private GameObject materialSlot = null;

        // game manager references
        //private InventoryManager inventoryManager = null;
        protected CraftingManager craftingManager = null;
        protected SystemEventManager systemEventManager = null;
        protected PlayerManager playerManager = null;

        public GameObject MaterialSlot { get => materialSlot; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            //inventoryManager = systemGameManager.InventoryManager;
            craftingManager = systemGameManager.CraftingManager;
            systemEventManager = systemGameManager.SystemEventManager;
            playerManager = systemGameManager.PlayerManager;
        }

        public override void UpdateVisual() {
            //Debug.Log("DescribableCraftingInputIcon.UpdateVisual()");
            base.UpdateVisual();
            description.text = Describable.DisplayName;

            //if (count > 1) {
            stackSize.text = playerManager.MyCharacter.CharacterInventoryManager.GetItemCount(Describable.DisplayName) + " / " + count.ToString();
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
            if (playerManager.MyCharacter.CharacterInventoryManager != null) {
                systemEventManager.OnItemCountChanged -= UpdateVisual;
            }

        }

    }

}