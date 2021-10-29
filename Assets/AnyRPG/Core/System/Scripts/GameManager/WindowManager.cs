using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AnyRPG {

    public class WindowManager : ConfiguredMonoBehaviour {

        private List<CloseableWindowContents> windowStack = new List<CloseableWindowContents>();
        //private List<UINavigationController> navigationStack = new List<UINavigationController>();

        // game manager references
        protected InputManager inputManager = null;
        protected ControlsManager controlsManager = null;

        public List<CloseableWindowContents> WindowStack { get => windowStack; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            SystemEventManager.StartListening("OnLevelUnload", HandleLevelUnload);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            inputManager = systemGameManager.InputManager;
            controlsManager = systemGameManager.ControlsManager;
        }

        public void OnDestroy() {
            SystemEventManager.StopListening("OnLevelUnload", HandleLevelUnload);
        }

        public void HandleLevelUnload(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log("ControlsManager.HandleLevelUnload()");
            windowStack.Clear();
            //navigationStack.Clear();
        }

        public void AddWindow(CloseableWindowContents closeableWindowContents) {
            //Debug.Log("ControlsManager.AddWindow(" + closeableWindowContents.name + ")");
            windowStack.Add(closeableWindowContents);
        }

        public void RemoveWindow(CloseableWindowContents closeableWindowContents) {
            //Debug.Log("ControlsManager.RemoveWindow(" + closeableWindowContents.name + ")");
            if (windowStack.Contains(closeableWindowContents)) {
                windowStack.Remove(closeableWindowContents);
            }
            if (windowStack.Count > 0) {
                windowStack[windowStack.Count - 1].FocusCurrentButton();
            }
        }

        public void Navigate() {
            if (windowStack.Count != 0) {

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

                // buttons
                if (inputManager.KeyBindWasPressed("ACCEPT") || inputManager.KeyBindWasPressed("JOYSTICKBUTTON0")) {
                    windowStack[windowStack.Count - 1].Accept();
                }
                if (inputManager.KeyBindWasPressed("CANCEL") || inputManager.KeyBindWasPressed("JOYSTICKBUTTON1")) {
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

            }
        }

       

    }

}