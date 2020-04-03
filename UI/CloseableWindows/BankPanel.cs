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

        public BagBarController MyBagBarController { get => bagBarController; set => bagBarController = value; }

        public override void ReceiveOpenWindowNotification() {
            base.ReceiveOpenWindowNotification();
            InventoryManager.MyInstance.OpenBank();
        }

        public override void RecieveClosedWindowNotification() {
            base.RecieveClosedWindowNotification();
            InventoryManager.MyInstance.CloseBank();
        }

    }

}