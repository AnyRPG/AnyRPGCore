using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ConfirmSellItemPanelController : WindowContentController {

        private Item item = null;

        // game manager references
        private UIManager uIManager = null;

        public Item MyItem { get => item; set => item = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            uIManager = systemGameManager.UIManager;
        }

        public void CancelAction() {
            //Debug.Log("NewGameMenuController.CancelAction()");
            uIManager.confirmSellItemMenuWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("NewGameMenuController.ConfirmAction()");
            //SystemGameManager.Instance.UIManager.HandScript.DeleteItem();
            if (item != null) {
                (uIManager.vendorWindow.CloseableWindowContents as VendorUI).SellItem(MyItem);
            }
            uIManager.confirmSellItemMenuWindow.CloseWindow();
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