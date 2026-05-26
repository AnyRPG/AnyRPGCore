namespace AnyRPG {
    public class ExitToMainMenuPanel : WindowPanel {

        /*
        [SerializeField]
        private HighlightButton noButton = null;

        [SerializeField]
        private HighlightButton yesButton = null;
        */

        // game manager references
        private UIManager uIManager = null;
        private LevelManagerClient levelManagerClient = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            //noButton.Configure(systemGameManager);
            //yesButton.Configure(systemGameManager);

        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            levelManagerClient = systemGameManager.LevelManagerClient;
        }

        public void CancelExit() {
            //Debug.Log("ExitMenuController.CancelExit()");
            uIManager.exitToMainMenuWindow.CloseWindow();
        }

        public void ConfirmExit() {
            //Debug.Log("ExitMenuController.ConfirmExit()");
            uIManager.exitToMainMenuWindow.CloseWindow();
            uIManager.playerOptionsMenuWindow.CloseWindow();
            levelManagerClient.LoadMainMenu(false);
        }

    }

}