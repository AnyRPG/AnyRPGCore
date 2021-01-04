using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ConfirmSellItemPanelController : WindowContentController {

        private Item item = null;

        public Item MyItem { get => item; set => item = value; }

        public void CancelAction() {
            //Debug.Log("NewGameMenuController.CancelAction()");
            SystemWindowManager.MyInstance.confirmSellItemMenuWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("NewGameMenuController.ConfirmAction()");
            //HandScript.MyInstance.DeleteItem();
            if (item != null) {
                (PopupWindowManager.MyInstance.vendorWindow.CloseableWindowContents as VendorUI).SellItem(MyItem);
            }
            SystemWindowManager.MyInstance.confirmSellItemMenuWindow.CloseWindow();
        }

        public override void ReceiveOpenWindowNotification() {
            base.ReceiveOpenWindowNotification();
            item = null;
        }


        public override void RecieveClosedWindowNotification() {
            base.RecieveClosedWindowNotification();
            item = null;
        }

    }

}