using UnityEngine;

namespace AnyRPG {

    public class ControlsManager : ConfiguredClass {

        private bool gamePadModeActive = false;
        private bool gamePadInputActive = false;
        private bool mouseDisabled = false;

        private int windowStackCount = 0;

        // is text being entered in an input field ?
        // used to disable keyboard shortcuts and movement
        private bool textInputActive = false;

        // game manager references
        protected InputManager inputManager = null;
        protected UIManager uIManager = null;
        protected WindowManager windowManager = null;
        protected PlayerManagerClient playerManagerClient = null;
        protected ActionBarManager actionBarManager = null;
        protected CutsceneBarController cutSceneBarController = null;
        protected CastTargetController castTargetController = null;

        public bool GamepadModeActive { get => gamePadModeActive; }
        public bool GamePadInputActive { get => gamePadInputActive; }
        public bool MouseDisabled { get => mouseDisabled; }
        public int WindowStackCount { get => windowStackCount; }
        public bool TextInputActive { get => textInputActive; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            if (systemConfigurationManager.DefaultControllerConfiguration == DefaultControllerConfiguration.GamePad) {
                ActivateGamepadMode(false);
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            inputManager = systemGameManager.InputManager;
            uIManager = systemGameManager.UIManager;
            windowManager = systemGameManager.WindowManager;
            playerManagerClient = systemGameManager.PlayerManagerClient;
            actionBarManager = uIManager.ActionBarManager;
            cutSceneBarController = uIManager.CutSceneBarController;
            castTargetController = systemGameManager.CastTargettingManager.CastTargetController;
        }

        public void ActivateTextInput() {
            //Debug.Log("ControlsManager.ActivateTextInput()");
            textInputActive = true;
        }

        public void DeactivateTextInput() {
            //Debug.Log("ControlsManager.DeactivateTextInput()");
            textInputActive = false;
        }

        public void ActivateGamepadMode(bool toggleUI) {
            //Debug.Log("ControlsManager.ActivateGamepadMode()");
            gamePadModeActive = true;
            gamePadInputActive = true;
            LockMouse();
            if (toggleUI == true) {
                uIManager.ToggleGamepadMode();
            }
        }

        public void DeactivateGamepadMode(bool toggleUI) {
            //Debug.Log("ControlsManager.DeactivateGamepadMode()");
            gamePadModeActive = false;
            gamePadInputActive = false;
            UnlockMouse();
            if (toggleUI == true) {
                uIManager.ToggleGamepadMode();
            }
        }


        private void ActivateGamepadInput() {
            //Debug.Log("ControlsManager.ActivateGamepadInput()");
            gamePadInputActive = true;
            LockMouse();
        }

        private void DeactivateGamepadInput() {
            //Debug.Log("ControlsManager.DeactivateGamepadInput()");

            // this if condition was added because a mouse click in an input field was causing that inputfield
            // to be deselected due to deactivating gamepadinput sending deselection events to another open window
            if (gamePadInputActive == false) {
                return;
            }

            gamePadInputActive = false;
            windowManager.DeactivateGamepadInput();
        }

        private void LockMouse() {
            //Debug.Log("ControlsManager.LockMouse()");
            //Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            mouseDisabled = true;
            if (playerManagerClient.PlayerController != null) {
                playerManagerClient.PlayerController.DisableMouseOver();
            }
            //Debug.Log("ControlsManager.LockMouse() visibility: " + Cursor.visible);
        }

        private void UnlockMouse() {
            //Debug.Log("ControlsManager.UnlockMouse()");
            //Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            mouseDisabled = false;
        }

        private void CheckMouse() {
            if (inputManager.mouseDeltaX != 0f || inputManager.mouseDeltaY != 0f) {
                UnlockMouse();
            }
            if (inputManager.leftMouseButtonClicked == true
                || inputManager.rightMouseButtonClicked == true) {
                UnlockMouse();
                DeactivateGamepadInput();
            }
        }

        public void Update() {
            /*
            if (playerManager.PlayerController != null) {
                playerManager.PlayerController.ResetMoveInput();
            }
            */
            inputManager.RegisterInput();
            CheckMouse();

            if (gamePadInputActive == false) {
                if (inputManager.KeyBindWasPressed("GAMEPADBUTTONA")
                    || inputManager.KeyBindWasPressed("GAMEPADBUTTONB")
                    || inputManager.KeyBindWasPressed("GAMEPADBUTTONX")
                    || inputManager.KeyBindWasPressed("GAMEPADBUTTONY")
                    || inputManager.KeyBindWasPressed("GAMEPADBUTTONLEFTSHOULDER")
                    || inputManager.KeyBindWasPressed("GAMEPADBUTTONRIGHTSHOULDER")
                    || inputManager.KeyBindWasPressed("GAMEPADBUTTONSELECT")
                    || inputManager.KeyBindWasPressed("GAMEPADBUTTONSTART")
                    || inputManager.KeyBindWasPressed("GAMEPADBUTTONLEFTSTICK")
                    || inputManager.KeyBindWasPressed("GAMEPADBUTTONRIGHTSTICK")
                    || inputManager.rightTriggerPressed
                    || inputManager.leftTriggerPressed
                    || inputManager.dPadDownPressed
                    || inputManager.dPadUpPressed
                    || inputManager.dPadLeftPressed
                    || inputManager.dPadRightPressed) {
                    //ActivateGamepadMode();
                    ActivateGamepadInput();
                    if (windowManager.CurrentWindow != null) {
                        windowManager.ActivateGamepadMode();
                        return;
                    }
                }
            }

            // taking window stack count here because window could be closed in uIManager.ProcessInput()
            windowStackCount = windowManager.WindowStack.Count;

            // only send input to the next block if the name change window is not open
            if (textInputActive == false) {
                uIManager.ProcessInput();

                if (windowManager.NavigatingInterface && inputManager.KeyBindWasPressed("GAMEPADBUTTONB")) {
                    windowManager.EndNavigateInterface();
                }
                if (inputManager.KeyBindWasPressed("GAMEPADBUTTONSELECT")) {
                    windowManager.NavigateInterface();
                }
            }

            // if the window manager has open windows, allow it to process commands
            // don't send input to the player controller if windows are open
            // because the input could close the window, and accidentally do something like select the nearest target
            // by passing the input to the player controller after the window manager
            if (windowStackCount > 0) {
                windowManager.Navigate();
            }

            if (textInputActive == true) {
                //Debug.Log("Not allowing movement or attacks during name change");
                return;
            }

            if (windowStackCount == 0 || gamePadInputActive == false) {
                if (cutSceneBarController.CurrentCutscene != null) {
                    cutSceneBarController.ProcessInput();
                } else {
                    if (playerManagerClient.PlayerController != null) {
                        if (gamePadModeActive) {
                            actionBarManager.ProcessGamepadInput();
                        }
                        playerManagerClient.PlayerController.ProcessInput();
                    }
                }
            }

            castTargetController.Follow();
        }


       

    }

}