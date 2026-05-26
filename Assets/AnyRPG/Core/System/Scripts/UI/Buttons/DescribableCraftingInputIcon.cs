using TMPro;
using UnityEngine;

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
        protected PlayerManagerClient playerManagerClient = null;

        public GameObject MaterialSlot { get => materialSlot; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            //inventoryManager = systemGameManager.InventoryManager;
            craftingManager = systemGameManager.CraftingManager;
            playerManagerClient = systemGameManager.PlayerManagerClient;
        }

        public override void UpdateVisual() {
            //Debug.Log("DescribableCraftingInputIcon.UpdateVisual()");
            base.UpdateVisual();
            description.text = Describable.DisplayName;

            //if (count > 1) {
            stackSize.text = playerManagerClient.UnitController.CharacterInventoryManager.GetItemCount(Describable.DisplayName) + " / " + count.ToString();
            //} else {
            //stackSize.text = "";
            //}
        }

      

        protected override void SetDescribableCommon(IDescribable describable) {
            base.SetDescribableCommon(describable);
            systemEventManager.OnItemCountChanged += UpdateVisual;
        }

        private void UpdateVisual(UnitController controller, Item item) {
            UpdateVisual();
        }

        public override void OnDisable() {
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            base.OnDisable();
            if (playerManagerClient.UnitController.CharacterInventoryManager != null) {
                systemEventManager.OnItemCountChanged -= UpdateVisual;
            }

        }

    }

}