using UnityEngine;

namespace AnyRPG {
    public class ConfirmSellItemPanel : WindowPanel {

        // game manager references
        private UIManager uIManager = null;
        private VendorManagerClient vendorManagerClient = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            vendorManagerClient = systemGameManager.VendorManagerClient;
        }

        public void CancelAction() {
            //Debug.Log("NewGameMenuController.CancelAction()");
            uIManager.confirmSellItemMenuWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("NewGameMenuController.ConfirmAction()");
            //SystemGameManager.Instance.UIManager.HandScript.DeleteItem();
            vendorManagerClient.RequestSellItemToVendor();
            uIManager.confirmSellItemMenuWindow.CloseWindow();
        }


    }

}