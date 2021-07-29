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
            SystemGameManager.Instance.UIManager.SystemWindowManager.confirmSellItemMenuWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("NewGameMenuController.ConfirmAction()");
            //HandScript.Instance.DeleteItem();
            if (item != null) {
                (SystemGameManager.Instance.UIManager.PopupWindowManager.vendorWindow.CloseableWindowContents as VendorUI).SellItem(MyItem);
            }
            SystemGameManager.Instance.UIManager.SystemWindowManager.confirmSellItemMenuWindow.CloseWindow();
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