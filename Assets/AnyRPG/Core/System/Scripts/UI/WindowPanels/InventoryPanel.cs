using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class InventoryPanel : BagPanel {

        //public override event Action<ICloseableWindowContents> OnOpenWindow;

        [Header("Inventory Panel")]

        [SerializeField]
        protected BagBarController bagBarController;

        [SerializeField]
        protected TextMeshProUGUI carryWeightText = null;

        [SerializeField]
        protected CurrencyBarController currencyBarController = null;

        public BagBarController BagBarController { get => bagBarController; set => bagBarController = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            bagBarController.Configure(systemGameManager);
            bagBarController.SetBagButtonCount(systemConfigurationManager.MaxInventoryBags);
            bagBarController.SetBagPanel(this);
            currencyBarController.Configure(systemGameManager);
        }

        protected override void ProcessCreateEventSubscriptions() {
            //Debug.Log("InventoryPanel.ProcessCreateEventSubscriptions()");
            base.ProcessCreateEventSubscriptions();

            systemEventManager.OnPlayerUnitDespawn += HandlePlayerUnitDespawn;
            systemEventManager.OnAddInventoryBagNode += HandleAddInventoryBagNode;
            systemEventManager.OnAddInventorySlot += HandleAddSlot;
            systemEventManager.OnRemoveInventorySlot += HandleRemoveSlot;
            systemEventManager.OnCurrencyChange += HandleCurrencyChange;
            systemEventManager.OnCarryWeightChanged += HandleCarryWeightChanged;
        }

        protected override void ProcessCleanupEventSubscriptions() {
            base.ProcessCleanupEventSubscriptions();

            systemEventManager.OnPlayerUnitDespawn += HandlePlayerUnitDespawn;
            systemEventManager.OnAddInventoryBagNode -= HandleAddInventoryBagNode;
            systemEventManager.OnAddInventorySlot -= HandleAddSlot;
            systemEventManager.OnRemoveInventorySlot -= HandleRemoveSlot;
            systemEventManager.OnCurrencyChange -= HandleCurrencyChange;
            systemEventManager.OnCarryWeightChanged -= HandleCarryWeightChanged;
        }


        private void HandleCarryWeightChanged() {
            UpdateCarryWeightText();
        }

        private void UpdateCarryWeightText() {
            if (playerManagerClient.UnitController == null) {
                carryWeightText.text = "0 / 0";
                return;
            }
            float inventoryWeight = playerManagerClient.UnitController.CharacterInventoryManager.Weight;
            float equippedWeight = playerManagerClient.UnitController.CharacterEquipmentManager.EquippedWeight;
            float totalWeight = inventoryWeight + equippedWeight;
            float carryWeight = playerManagerClient.UnitController.CharacterStats.SecondaryStats[SecondaryStatType.CarryWeight].CurrentValue + systemConfigurationManager.BaseCarryWeight;
            carryWeightText.text = $"<color={(totalWeight > carryWeight ? "red" : "white")}>Inventory: {Mathf.Ceil(inventoryWeight)} kg\n" +
                $"Equipped: {Mathf.Ceil(equippedWeight)} kg\n" +
                $"Total: {Mathf.Ceil(totalWeight)}";
            if (systemConfigurationManager.UseEncumberance == true) {
                carryWeightText.text += $" / {Mathf.Ceil(carryWeight)}";
            }
            carryWeightText.text += " kg</color>";
        }

        private void HandleCurrencyChange() {
            UpdateCurrencyAmount();
        }

        private void UpdateCurrencyAmount() {
            if (playerManagerClient.UnitController == null) {
                return;
            }
            if (systemConfigurationManager.DefaultCurrencyGroup?.BaseCurrency != null) {
                currencyBarController.UpdateCurrencyAmount(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, playerManagerClient.UnitController.CharacterCurrencyManager.GetBaseCurrencyValue(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency));
            } else {
                currencyBarController.ClearCurrencyAmounts();
            }
        }

        public void HandlePlayerUnitDespawn(UnitController unitController) {
            //Debug.Log("InventoryPanel.HandlePlayerUnitDespawn()");

            ClearSlots();
            bagBarController.ClearBagButtons();
        }

        public void HandleAddInventoryBagNode(BagNode bagNode) {
            //Debug.Log("InventoryPanel.HandleAddInventoryBagNode()");
            bagBarController.AddBagButton(bagNode);
            //bagNode.BagPanel = this;
        }

        public override void ProcessOpenWindowNotification() {
            base.ProcessOpenWindowNotification();
            UpdateCarryWeightText();
            UpdateCurrencyAmount();
        }


    }

}