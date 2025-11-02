using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ConfirmSellItemPanel : WindowPanel {

        /*
        [SerializeField]
        private HighlightButton noButton = null;

        [SerializeField]
        private HighlightButton yesButton = null;
        */

        private InstantiatedItem instantiatedItem = null;

        // game manager references
        private UIManager uIManager = null;

        public InstantiatedItem MyItem { get => instantiatedItem; set => instantiatedItem = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            /*
            noButton.Configure(systemGameManager);
            yesButton.Configure(systemGameManager);
            */
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
            if (instantiatedItem != null) {
                (uIManager.vendorWindow.CloseableWindowContents as VendorPanel).SellItem(MyItem);
            }
            uIManager.confirmSellItemMenuWindow.CloseWindow();
        }

        public override void ProcessOpenWindowNotification() {
            base.ProcessOpenWindowNotification();
            instantiatedItem = null;
        }


        public override void ReceiveClosedWindowNotification() {
            base.ReceiveClosedWindowNotification();
            instantiatedItem = null;
        }

    }

}