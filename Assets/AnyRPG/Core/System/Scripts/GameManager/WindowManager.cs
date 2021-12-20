using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AnyRPG {

    public class WindowManager : ConfiguredMonoBehaviour {

        private List<CloseableWindowContents> windowStack = new List<CloseableWindowContents>();
        //private List<UINavigationController> navigationStack = new List<UINavigationController>();

        private bool navigatingInterface = false;
        private int interfaceIndex = -1;

        // game manager references
        protected InputManager inputManager = null;
        protected ControlsManager controlsManager = null;
        protected UIManager uIManager = null;

        public List<CloseableWindowContents> WindowStack { get => windowStack; }
        public bool NavigatingInterface { get => navigatingInterface; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            SystemEventManager.StartListening("OnLevelUnload", HandleLevelUnload);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            inputManager = systemGameManager.InputManager;
            controlsManager = systemGameManager.ControlsManager;
            uIManager = systemGameManager.UIManager;
        }

        public void OnDestroy() {
            SystemEventManager.StopListening("OnLevelUnload", HandleLevelUnload);
        }

        public void HandleLevelUnload(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log("WindowManager.HandleLevelUnload()");
            windowStack.Clear();
            //navigationStack.Clear();
        }

        public void AddWindow(CloseableWindowContents closeableWindowContents) {
            //Debug.Log("WindowManager.AddWindow(" + closeableWindowContents.name + ")");
            windowStack.Add(closeableWindowContents);
        }

        public void RemoveWindow(CloseableWindowContents closeableWindowContents) {
            //Debug.Log("WindowManager.RemoveWindow(" + closeableWindowContents.name + ")");
            if (windowStack.Contains(closeableWindowContents)) {
                windowStack.Remove(closeableWindowContents);
            }
            if (windowStack.Count > 0) {
                windowStack[windowStack.Count - 1].FocusCurrentButton();
            }
        }

        public void NavigateInterface() {
            navigatingInterface = true;
            if (interfaceIndex != -1) {
                if (interfaceIndex < uIManager.NavigableInterfaceElements.Count) {
                    uIManager.NavigableInterfaceElements[interfaceIndex].UnFocus();
                    windowStack.Remove(uIManager.NavigableInterfaceElements[interfaceIndex]);
                }
            }

            // increase the index
            interfaceIndex++;

            // if the last element has been navigated past, reset to the first element
            if (interfaceIndex >= uIManager.NavigableInterfaceElements.Count) {
                interfaceIndex = 0;
            }

            // focus the current element and add it to the window stack
            //Debug.Log("index: " + interfaceIndex + "; count: " + uIManager.NavigableInterfaceElements.Count);
            uIManager.NavigableInterfaceElements[interfaceIndex].Focus();
            windowStack.Add(uIManager.NavigableInterfaceElements[interfaceIndex]);
        }

        public void EndNavigateInterface() {
            if (navigatingInterface == true) {
                navigatingInterface = false;
                uIManager.NavigableInterfaceElements[interfaceIndex].UnFocus();
                windowStack.Remove(uIManager.NavigableInterfaceElements[interfaceIndex]);
                interfaceIndex = -1;
            }
        }

        public void ActivateGamepadMode() {
            if (windowStack.Count != 0) {
                windowStack[windowStack.Count - 1].FocusCurrentButton();
            }
        }

        public void DeactivateGamepadInput() {
            foreach (CloseableWindowContents closeableWindowContents in windowStack) {
                closeableWindowContents.DeactivateGamepadInput();
            }
        }

        public void Navigate() {
            //Debug.Log("WindowManager.Navigate()");

            if (windowStack.Count != 0) {
                //Debug.Log("WindowManager.Navigate() : windowstack is not zero: " + windowStack[windowStack.Count - 1].gameObject.name);

                // joystick movement
                if (controlsManager.InputHorizontal != 0f || controlsManager.InputVertical != 0f) {
                    windowStack[windowStack.Count - 1].LeftAnalog(controlsManager.InputHorizontal, controlsManager.InputVertical);
                }

                // d pad navigation
                if (controlsManager.DPadUpPressed) {
                    windowStack[windowStack.Count - 1].UpButton();
                }
                if (controlsManager.DPadDownPressed) {
                    windowStack[windowStack.Count - 1].DownButton();
                }
                if (controlsManager.DPadLeftPressed) {
                    windowStack[windowStack.Count - 1].LeftButton();
                }
                if (controlsManager.DPadRightPressed) {
                    windowStack[windowStack.Count - 1].RightButton();
                }
                if (controlsManager.LeftTriggerPressed) {
                    windowStack[windowStack.Count - 1].LeftTrigger();
                }
                if (controlsManager.RightTriggerPressed) {
                    windowStack[windowStack.Count - 1].RightTrigger();
                }

                // buttons
                if (inputManager.KeyBindWasPressed("ACCEPT") || inputManager.KeyBindWasPressed("JOYSTICKBUTTON0")) {
                    //Debug.Log("Accept");
                    windowStack[windowStack.Count - 1].Accept();
                }
                if (inputManager.KeyBindWasPressed("JOYSTICKBUTTON1")) {
                    windowStack[windowStack.Count - 1].Cancel();
                }
                if (inputManager.KeyBindWasPressed("JOYSTICKBUTTON2")) {
                    windowStack[windowStack.Count - 1].JoystickButton2();
                }
                if (inputManager.KeyBindWasPressed("JOYSTICKBUTTON3")) {
                    windowStack[windowStack.Count - 1].JoystickButton3();
                }
                if (inputManager.KeyBindWasPressed("JOYSTICKBUTTON4")) {
                    windowStack[windowStack.Count - 1].JoystickButton4();
                }
                if (inputManager.KeyBindWasPressed("JOYSTICKBUTTON5")) {
                    windowStack[windowStack.Count - 1].JoystickButton5();
                }
                if (inputManager.KeyBindWasPressed("JOYSTICKBUTTON9")) {
                    windowStack[windowStack.Count - 1].JoystickButton9();
                }

            }
        }

       

    }

}