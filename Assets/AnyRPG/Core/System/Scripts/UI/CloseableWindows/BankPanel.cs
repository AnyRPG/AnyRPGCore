using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class BankPanel : BagPanel {

        //public override event Action<ICloseableWindowContents> OnOpenWindow;

        [SerializeField]
        protected BagBarController bagBarController;

        // game manager references
        protected InventoryManager inventoryManager = null;

        public BagBarController MyBagBarController { get => bagBarController; set => bagBarController = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            bagBarController.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            inventoryManager = systemGameManager.InventoryManager;
        }

        public override void ReceiveOpenWindowNotification() {
            base.ReceiveOpenWindowNotification();
            inventoryManager.OpenBank();
        }

        public override void ReceiveClosedWindowNotification() {
            base.ReceiveClosedWindowNotification();
            inventoryManager.CloseBank();
        }

    }

}