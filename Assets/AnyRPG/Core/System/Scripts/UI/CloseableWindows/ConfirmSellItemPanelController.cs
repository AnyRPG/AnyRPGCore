using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ConfirmSellItemPanelController : WindowContentController {

        [SerializeField]
        private HighlightButton noButton = null;

        [SerializeField]
        private HighlightButton yesButton = null;

        private Item item = null;

        // game manager references
        private UIManager uIManager = null;

        public Item MyItem { get => item; set => item = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            noButton.Configure(systemGameManager);
            yesButton.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
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


        public override void ReceiveClosedWindowNotification() {
            base.ReceiveClosedWindowNotification();
            item = null;
        }

    }

}