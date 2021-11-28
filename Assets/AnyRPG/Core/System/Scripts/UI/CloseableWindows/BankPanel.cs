using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class BankPanel : BagPanel {

        //public override event Action<ICloseableWindowContents> OnOpenWindow;
        [Header("Bank Panel")]

        [SerializeField]
        protected BagBarController bagBarController;

        // game manager references
        protected PlayerManager playerManager = null;

        public BagBarController BagBarController { get => bagBarController; set => bagBarController = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            bagBarController.Configure(systemGameManager);
            bagBarController.SetBagButtonCount(systemConfigurationManager.MaxBankBags);
            bagBarController.SetBagPanel(this);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
        }

        protected override void ProcessCreateEventSubscriptions() {
            base.ProcessCreateEventSubscriptions();

            SystemEventManager.StartListening("OnPlayerConnectionDespawn", HandlePlayerConnectionDespawn);
            inventoryManager.OnAddBankBagNode += HandleAddBankBagNode;
            inventoryManager.OnAddBankSlot += HandleAddSlot;
            inventoryManager.OnRemoveBankSlot += HandleRemoveSlot;

        }

        protected override void ProcessCleanupEventSubscriptions() {
            base.ProcessCleanupEventSubscriptions();

            SystemEventManager.StopListening("OnPlayerConnectionDespawn", HandlePlayerConnectionDespawn);
            inventoryManager.OnAddBankBagNode -= HandleAddBankBagNode;
            inventoryManager.OnAddBankSlot -= HandleAddSlot;
            inventoryManager.OnRemoveBankSlot -= HandleRemoveSlot;
        }

        public void HandlePlayerConnectionDespawn(string eventName, EventParamProperties eventParamProperties) {
            ClearSlots();
            bagBarController.ClearBagButtons();
        }

        public void HandleAddBankBagNode(BagNode bagNode) {
            //Debug.Log("BankPanel.HandleAddBankBagNode()");
            bagBarController.AddBagButton(bagNode);
            //bagNode.BagPanel = this;
        }


    }

}