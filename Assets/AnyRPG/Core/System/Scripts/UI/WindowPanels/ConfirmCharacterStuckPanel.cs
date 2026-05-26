namespace AnyRPG {
    public class ConfirmCharacterStuckPanel : WindowPanel {

        // game manager references
        private UIManager uIManager = null;
        private PlayerManagerClient playerManagerClient = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            playerManagerClient = systemGameManager.PlayerManagerClient;
        }

        public void CancelAction() {
            //Debug.Log("NewGameMenuController.CancelAction()");
            uIManager.confirmCharacterStuckWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("NewGameMenuController.ConfirmAction()");
            uIManager.confirmCharacterStuckWindow.CloseWindow();
            uIManager.helpMenuWindow.CloseWindow();
            playerManagerClient.RequestRespawnPlayer();
        }

    }

}