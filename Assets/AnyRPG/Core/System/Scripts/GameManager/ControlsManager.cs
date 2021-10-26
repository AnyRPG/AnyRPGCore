using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AnyRPG {

    public class ControlsManager : ConfiguredMonoBehaviour {

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

        private bool gamePadModeActive = false;

        // game manager references
        protected InputManager inputManager = null;
        protected UIManager uIManager = null;
        protected WindowManager windowManager = null;
        protected PlayerManager playerManager = null;

        public bool DPadDownPressed { get => dPadDownPressed; }
        public bool DPadUpPressed { get => dPadUpPressed; }
        public bool DPadLeftPressed { get => dPadLeftPressed; }
        public bool DPadRightPressed { get => dPadRightPressed; }
        public bool GamePadModeActive { get => gamePadModeActive; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            if (systemConfigurationManager.DefaultControllerConfiguration == DefaultControllerConfiguration.GamePad) {
                gamePadModeActive = true;
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            inputManager = systemGameManager.InputManager;
            uIManager = systemGameManager.UIManager;
            windowManager = systemGameManager.WindowManager;
            playerManager = systemGameManager.PlayerManager;
        }

        void Update() {
            RegisterAxis();
            inputManager.RegisterInput();

            if (inputManager.KeyBindWasPressed("ACCEPT") || inputManager.KeyBindWasPressed("CANCEL")) {
                gamePadModeActive = true;
            }


            uIManager.ProcessInput();
            windowManager.Navigate();

            if (playerManager.PlayerController != null) {
                playerManager.PlayerController.ProcessInput();
            }
        }


        private void RegisterAxis() {
            dPadHorizontal = Input.GetAxis("D-Pad Horizontal");
            dPadVertical = Input.GetAxis("D-Pad Vertical");
            dPadDownPressed = false;
            dPadUpPressed = false;
            dPadLeftPressed = false;
            dPadRightPressed = false;
            if (dPadDown == false) {
                dPadDown = (dPadVertical < 0f);
                if (dPadDown) {
                    Debug.Log("dPadDownPressed");
                    dPadDownPressed = true;
                    gamePadModeActive = true;
                }
            } else if (dPadVertical >= 0f) {
                dPadDown = false;
            }
            if (dPadUp == false) {
                dPadUp = (dPadVertical > 0f);
                if (dPadUp) {
                    Debug.Log("dPadUpPressed");
                    dPadUpPressed = true;
                    gamePadModeActive = true;
                }
            } else if (dPadVertical <= 0f) {
                dPadUp = false;
            }
            if (dPadLeft == false) {
                dPadLeft = (dPadHorizontal < 0f);
                if (dPadLeft) {
                    Debug.Log("dPadLeftPressed");
                    dPadLeftPressed = true;
                    gamePadModeActive = true;
                }
            } else if (dPadHorizontal >= 0f) {
                dPadLeft = false;
            }
            if (dPadRight == false) {
                dPadRight = (dPadHorizontal > 0f);
                if (dPadRight) {
                    Debug.Log("dPadRightPressed");
                    dPadRightPressed = true;
                    gamePadModeActive = true;
                }
            } else if (dPadHorizontal <= 0f) {
                dPadRight = false;
            }

        }

    }

}