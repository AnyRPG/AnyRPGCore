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
        //protected InventoryManager inventoryManager = null;
        protected PlayerManager playerManager = null;

        public BagBarController BagBarController { get => bagBarController; set => bagBarController = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            bagBarController.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            //inventoryManager = systemGameManager.InventoryManager;
            playerManager = systemGameManager.PlayerManager;
        }

        protected override void ProcessCreateEventSubscriptions() {
            base.ProcessCreateEventSubscriptions();

            SystemEventManager.StartListening("OnPlayerConnectionDespawn", HandlePlayerConnectionDespawn);
            playerManager.MyCharacter.CharacterInventoryManager.OnAddInventoryBagNode += HandleAddInventoryBagNode;
        }

        protected override void ProcessCleanupEventSubscriptions() {
            base.ProcessCleanupEventSubscriptions();

            SystemEventManager.StopListening("OnPlayerConnectionDespawn", HandlePlayerConnectionDespawn);
            playerManager.MyCharacter.CharacterInventoryManager.OnAddInventoryBagNode -= HandleAddInventoryBagNode;
        }

        public void HandlePlayerConnectionDespawn(string eventName, EventParamProperties eventParamProperties) {
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