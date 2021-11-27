using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AnyRPG {

    public class ControlsManager : ConfiguredMonoBehaviour {

        private float inputHorizontal = 0f;
        private float inputVertical = 0f;

        private float dPadHorizontal = 0f;
        private float dPadVertical = 0f;
        private bool dPadDown = false;
        private bool dPadDownPressed = false;
        private bool dPadUp = false;
        private bool dPadUpPressed = false;
        private bool dPadLeft = false;
        private bool dPadLeftPressed = false;
        private bool dPadRight = false;
        private bool dPadRightPressed = false;

        private float leftTriggerAxis = 0f;
        private float rightTriggerAxis = 0f;
        private bool leftTriggerDown = false;
        private bool leftTriggerUp = false;
        private bool leftTriggerPressed = false;
        private bool rightTriggerDown = false;
        private bool rightTriggerUp = false;
        private bool rightTriggerPressed = false;

        private bool gamePadModeActive = false;

        private int windowStackCount = 0;

        // game manager references
        protected InputManager inputManager = null;
        protected UIManager uIManager = null;
        protected WindowManager windowManager = null;
        protected PlayerManager playerManager = null;
        protected ActionBarManager actionBarManager = null;
        protected CutSceneBarController cutSceneBarController = null;

        public bool GamePadModeActive { get => gamePadModeActive; }
        public bool DPadDownPressed { get => dPadDownPressed; }
        public bool DPadUpPressed { get => dPadUpPressed; }
        public bool DPadLeftPressed { get => dPadLeftPressed; }
        public bool DPadRightPressed { get => dPadRightPressed; }
        public bool LeftTriggerDown { get => leftTriggerDown; }
        public bool LeftTriggerUp { get => leftTriggerUp; }
        public bool LeftTriggerPressed { get => leftTriggerPressed; }
        public bool RightTriggerDown { get => rightTriggerDown; }
        public bool RightTriggerUp { get => rightTriggerUp; }
        public bool RightTriggerPressed { get => rightTriggerPressed; }
        public float InputHorizontal { get => inputHorizontal; }
        public float InputVertical { get => inputVertical; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            if (systemConfigurationManager.DefaultControllerConfiguration == DefaultControllerConfiguration.GamePad) {
                ActivateGamepadMode();
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            inputManager = systemGameManager.InputManager;
            uIManager = systemGameManager.UIManager;
            windowManager = systemGameManager.WindowManager;
            playerManager = systemGameManager.PlayerManager;
            actionBarManager = uIManager.ActionBarManager;
            cutSceneBarController = uIManager.CutSceneBarController;
        }

        private void ActivateGamepadMode() {
            //Debug.Log("ControlsManager.ActivateGamepadMode()");
            gamePadModeActive = true;
            LockMouse();
        }

        private void LockMouse() {
            //Debug.Log("ControlsManager.LockMouse()");
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (playerManager.PlayerController != null) {
                playerManager.PlayerController.DisableMouseOver();
            }
            //Debug.Log("ControlsManager.LockMouse() visibility: " + Cursor.visible);
        }

        private void UnlockMouse() {
            //Debug.Log("ControlsManager.UnlockMouse()");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void CheckMouse() {
            if (Input.GetAxis("Mouse X") != 0f || Input.GetAxis("Mouse Y") != 0f) {
                UnlockMouse();
            }
        }

        void Update() {
            CheckMouse();
            RegisterAxis();
            inputManager.RegisterInput();

            if (inputManager.KeyBindWasPressed("JOYSTICKBUTTON0")
                || inputManager.KeyBindWasPressed("JOYSTICKBUTTON1")
                || inputManager.KeyBindWasPressed("JOYSTICKBUTTON2")
                || inputManager.KeyBindWasPressed("JOYSTICKBUTTON3")
                || inputManager.KeyBindWasPressed("JOYSTICKBUTTON4")
                || inputManager.KeyBindWasPressed("JOYSTICKBUTTON5")
                || inputManager.KeyBindWasPressed("JOYSTICKBUTTON6")
                || inputManager.KeyBindWasPressed("JOYSTICKBUTTON7")
                || inputManager.KeyBindWasPressed("JOYSTICKBUTTON8")
                || inputManager.KeyBindWasPressed("JOYSTICKBUTTON9")) {
                ActivateGamepadMode();
            }

            uIManager.ProcessInput();

            if (windowManager.NavigatingInterface && (inputManager.KeyBindWasPressed("CANCEL") || inputManager.KeyBindWasPressed("JOYSTICKBUTTON1"))) {
                windowManager.EndNavigateInterface();
            }
            if (inputManager.KeyBindWasPressed("JOYSTICKBUTTON6")) {
                windowManager.NavigateInterface();
            }

            // if the window manager has open windows, allow it to process commands
            // don't send input to the player controller if windows are open
            // because the input could close the window, and accidentally do something like select the nearest target
            // by passing the input to the player controller after the window manager
            windowStackCount = windowManager.WindowStack.Count;
            if (windowStackCount > 0) {
                windowManager.Navigate();
            }

            if (windowStackCount == 0 || gamePadModeActive == false) {
                if (cutSceneBarController.CurrentCutscene != null) {
                    cutSceneBarController.ProcessInput();
                } else {
                    if (playerManager.PlayerController != null) {
                        if (gamePadModeActive) {
                            actionBarManager.ProcessInput();
                        }
                        playerManager.PlayerController.ProcessInput();
                    }
                }
            }
        }


        private void RegisterAxis() {

            RegisterJoystickAxis();
            RegisterDPadAxis();
            RegisterTriggerAxis();

        }

        private void RegisterJoystickAxis() {
            inputHorizontal = Input.GetAxis("LeftAnalogHorizontal");
            inputVertical = Input.GetAxis("LeftAnalogVertical");
        }

        private void RegisterTriggerAxis() {
            
            leftTriggerAxis = Input.GetAxis("LT");
            rightTriggerAxis = Input.GetAxis("RT");

            //Debug.Log("leftTriggerAxis: " + leftTriggerAxis + "; rightTriggerAxis: " + rightTriggerAxis);

            //leftTriggerDown = false;
            leftTriggerUp = false;
            leftTriggerPressed = false;
            //rightTriggerDown = false;
            rightTriggerUp = false;
            rightTriggerPressed = false;

            if (leftTriggerAxis == 1 && leftTriggerDown == false) {
                leftTriggerDown = true;
            }
            if (leftTriggerAxis == 0 && leftTriggerDown == true) {
                leftTriggerDown = false;
                leftTriggerUp = true;
                leftTriggerPressed = true;
            }

            if (rightTriggerAxis == 1 && rightTriggerDown == false) {
                rightTriggerDown = true;
            }
            if (rightTriggerAxis == 0 && rightTriggerDown == true) {
                rightTriggerDown = false;
                rightTriggerUp = true;
                rightTriggerPressed = true;
            }


        }

        private void RegisterDPadAxis() {
            dPadHorizontal = Input.GetAxis("D-Pad Horizontal");
            dPadVertical = Input.GetAxis("D-Pad Vertical");
            dPadDownPressed = false;
            dPadUpPressed = false;
            dPadLeftPressed = false;
            dPadRightPressed = false;
            if (dPadDown == false) {
                dPadDown = (dPadVertical < 0f);
                if (dPadDown) {
                    //Debug.Log("dPadDownPressed");
                    dPadDownPressed = true;
                    gamePadModeActive = true;
                }
            } else if (dPadVertical >= 0f) {
                dPadDown = false;
            }
            if (dPadUp == false) {
                dPadUp = (dPadVertical > 0f);
                if (dPadUp) {
                    //Debug.Log("dPadUpPressed");
                    dPadUpPressed = true;
                    gamePadModeActive = true;
                }
            } else if (dPadVertical <= 0f) {
                dPadUp = false;
            }
            if (dPadLeft == false) {
                dPadLeft = (dPadHorizontal < 0f);
                if (dPadLeft) {
                    //Debug.Log("dPadLeftPressed");
                    dPadLeftPressed = true;
                    gamePadModeActive = true;
                }
            } else if (dPadHorizontal >= 0f) {
                dPadLeft = false;
            }
            if (dPadRight == false) {
                dPadRight = (dPadHorizontal > 0f);
                if (dPadRight) {
                    //Debug.Log("dPadRightPressed");
                    dPadRightPressed = true;
                    gamePadModeActive = true;
                }
            } else if (dPadHorizontal <= 0f) {
                dPadRight = false;
            }
        }

    }

}