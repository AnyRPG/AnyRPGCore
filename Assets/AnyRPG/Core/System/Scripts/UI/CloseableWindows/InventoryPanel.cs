using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class InventoryPanel : BagPanel {

        //public override event Action<ICloseableWindowContents> OnOpenWindow;

        [Header("Inventory Panel")]

        [SerializeField]
        protected BagBarController bagBarController;

        // game manager references
        protected InventoryManager inventoryManager = null;

        public BagBarController BagBarController { get => bagBarController; set => bagBarController = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            bagBarController.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            inventoryManager = systemGameManager.InventoryManager;
        }

        protected override void ProcessCreateEventSubscriptions() {
            base.ProcessCreateEventSubscriptions();

            inventoryManager.OnClearData += ProcessClearData;
            inventoryManager.OnAddInventoryBagNode += HandleAddInventoryBagNode;
        }

        protected override void ProcessCleanupEventSubscriptions() {
            base.ProcessCleanupEventSubscriptions();

            inventoryManager.OnClearData -= ProcessClearData;
            inventoryManager.OnAddInventoryBagNode -= HandleAddInventoryBagNode;
        }

        public void ProcessClearData() {
            //Debug.Log("InventoryPanel.ProcessClearData()");

            ClearSlots();
            bagBarController.ClearBagButtons();
        }

        public void HandleAddInventoryBagNode(BagNode bagNode) {
            //Debug.Log("InventoryPanel.HandleAddInventoryBagNode()");
            bagBarController.AddBagButton(bagNode);
            bagNode.BagPanel = this;
        }


    }

}