using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AnyRPG {

    public class ControlsManager : ConfiguredMonoBehaviour {

        private List<UINavigationController> navigationStack = new List<UINavigationController>();

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

        // game manager references
        protected InputManager inputManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            SystemEventManager.StartListening("OnLevelUnload", HandleLevelUnload);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            inputManager = systemGameManager.InputManager;
        }

        public void OnDestroy() {
            SystemEventManager.StopListening("OnLevelUnload", HandleLevelUnload);
        }

        public void HandleLevelUnload(string eventName, EventParamProperties eventParamProperties) {
            Debug.Log("ControlsManager.HandleLevelUnload()");
            navigationStack.Clear();
        }

        public void AddNavigationController(UINavigationController uINavigationController) {
            Debug.Log("ControlsManager.AddNavigationController()");
            navigationStack.Add(uINavigationController);
        }

        public void RemoveNavigationController(UINavigationController uINavigationController) {
            Debug.Log("ControlsManager.RemoveNavigationController()");
            if (navigationStack.Contains(uINavigationController)) {
                navigationStack.Remove(uINavigationController);
            }
        }

        void Update() {
            RegisterAxis();
            inputManager.RegisterKeyPresses();

            Navigate();
        }

        private void Navigate() {
            if (navigationStack.Count != 0) {
                
                // d pad navigation
                if (dPadDownPressed || dPadRightPressed) {
                    navigationStack[navigationStack.Count - 1].NextButton();
                }
                if (dPadUpPressed || dPadLeftPressed) {
                    navigationStack[navigationStack.Count - 1].PreviousButton();
                }

                // buttons
                if (inputManager.KeyBindWasPressed("ACCEPT")) {
                    navigationStack[navigationStack.Count - 1].Accept();
                }
                if (inputManager.KeyBindWasPressed("CANCEL")) {
                    navigationStack[navigationStack.Count - 1].Cancel();
                }
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
                }
            } else if (dPadVertical >= 0f) {
                dPadDown = false;
            }
            if (dPadUp == false) {
                dPadUp = (dPadVertical > 0f);
                if (dPadUp) {
                    Debug.Log("dPadUpPressed");
                    dPadUpPressed = true;
                }
            } else if (dPadVertical <= 0f) {
                dPadUp = false;
            }
            if (dPadLeft == false) {
                dPadLeft = (dPadHorizontal < 0f);
                if (dPadLeft) {
                    Debug.Log("dPadLeftPressed");
                    dPadLeftPressed = true;
                }
            } else if (dPadHorizontal >= 0f) {
                dPadLeft = false;
            }
            if (dPadRight == false) {
                dPadRight = (dPadHorizontal > 0f);
                if (dPadRight) {
                    Debug.Log("dPadRightPressed");
                    dPadRightPressed = true;
                }
            } else if (dPadHorizontal <= 0f) {
                dPadRight = false;
            }

        }

    }

}