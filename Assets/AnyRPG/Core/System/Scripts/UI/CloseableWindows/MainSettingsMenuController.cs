using AnyRPG;
using UnityEngine;

namespace AnyRPG {
    public class MainSettingsMenuController : WindowContentController {

        [Header("MAIN BUTTONS")]
        public HighlightButton soundButton;
        public HighlightButton controlsButton;
        public HighlightButton videoButton;
        public HighlightButton keyBindingsButton;
        public HighlightButton userInterfaceButton;
        public HighlightButton returnButton;

        [Header("Panels")]
        [Tooltip("The UI Panel that holds the GAME window tab")]
        public SystemSoundPanelController PanelSound;
        [Tooltip("The UI Panel that holds the CONTROLS window tab")]
        public SystemControlsPanelController PanelControls;
        [Tooltip("The UI Panel that holds the VIDEO window tab")]
        public SystemVideoPanelController PanelVideo;
        [Tooltip("The UI Panel that holds the KEY BINDINGS window tab")]
        public SystemKeyBindPanelController PanelKeyBindings;
        [Tooltip("The UI Panel that holds the USER INTERFACE window tab")]
        public SystemUIPanelController PanelUserInterface;

        // game manager references
        private UIManager uIManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            uIManager = systemGameManager.UIManager;
        }

        public void ResetSettingsPanels() {
            // disable all settings panels
            PanelSound.gameObject.SetActive(false);
            PanelControls.gameObject.SetActive(false);
            PanelVideo.gameObject.SetActive(false);
            PanelKeyBindings.gameObject.SetActive(false);
            PanelUserInterface.gameObject.SetActive(false);
        }

        public void SoundPanel() {
            ResetSettingsPanels();
            PanelSound.gameObject.SetActive(true);

            uINavigationControllers[0].UnHightlightButtons(soundButton);
            SetOpenSubPanel(PanelSound);
            soundButton.Select();
        }

        public void VideoPanel() {
            ResetSettingsPanels();
            PanelVideo.gameObject.SetActive(true);

            uINavigationControllers[0].UnHightlightButtons(videoButton);
            SetOpenSubPanel(PanelVideo);

            videoButton.Select();
        }

        public void ControlsPanel() {
            ResetSettingsPanels();
            PanelControls.gameObject.SetActive(true);

            uINavigationControllers[0].UnHightlightButtons(controlsButton);
            SetOpenSubPanel(PanelControls);
            controlsButton.Select();
        }

        public void KeyBindingsPanel() {
            ResetSettingsPanels();
            PanelKeyBindings.gameObject.SetActive(true);

            uINavigationControllers[0].UnHightlightButtons(keyBindingsButton);
            SetOpenSubPanel(PanelKeyBindings);

            keyBindingsButton.Select();
        }


        public void UserInterfacePanel() {
            ResetSettingsPanels();
            PanelUserInterface.gameObject.SetActive(true);

            uINavigationControllers[0].UnHightlightButtons(userInterfaceButton);
            SetOpenSubPanel(PanelUserInterface);
            userInterfaceButton.Select();
        }

        public void CloseMenu() {
            //uIManager.SystemWindowManager.mainMenuWindow.OpenWindow();
            uIManager.settingsMenuWindow.CloseWindow();
        }

        public override void ReceiveOpenWindowNotification() {
            base.ReceiveOpenWindowNotification();
            uINavigationControllers[0].SetCurrentButton(userInterfaceButton);
            userInterfaceButton.HighlightBackground();
            UserInterfacePanel();
        }
    }
}