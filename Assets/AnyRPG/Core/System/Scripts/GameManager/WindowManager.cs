using System.Collections.Generic;

namespace AnyRPG {

    public class WindowManager : ConfiguredClass {

        private List<CloseableWindowContents> windowStack = new List<CloseableWindowContents>();
        //private List<UINavigationController> navigationStack = new List<UINavigationController>();

        private bool navigatingInterface = false;
        private int interfaceIndex = -1;

        // game manager references
        protected InputManager inputManager = null;
        protected ControlsManager controlsManager = null;
        protected UIManager uIManager = null;
        protected SystemEventManager systemEventManager = null;
        protected LevelManagerClient levelManagerClient = null;

        public List<CloseableWindowContents> WindowStack { get => windowStack; }
        public bool NavigatingInterface { get => navigatingInterface; }
        public CloseableWindowContents CurrentWindow {
            get {
                if (windowStack.Count > 0) {
                    return windowStack[windowStack.Count - 1];
                }
                return null;
            }
        }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            levelManagerClient.OnLevelUnload += HandleLevelUnload;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            inputManager = systemGameManager.InputManager;
            controlsManager = systemGameManager.ControlsManager;
            uIManager = systemGameManager.UIManager;
            systemEventManager = systemGameManager.SystemEventManager;
            levelManagerClient = systemGameManager.LevelManagerClient;
        }

        public void OnDestroy() {
            levelManagerClient.OnLevelUnload -= HandleLevelUnload;
        }

        public void HandleLevelUnload(int sceneHandle, string sceneName) {
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
                CurrentWindow.FocusCurrentButton();
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
                CurrentWindow.FocusCurrentButton();
            }
        }

        public void DeactivateGamepadInput() {
            foreach (CloseableWindowContents closeableWindowContents in windowStack) {
                closeableWindowContents.DeactivateGamepadInput();
            }
        }

        public void Navigate() {
            //Debug.Log("WindowManager.Navigate()");

            if (windowStack.Count == 0) {
                // no windows to navigate, do nothing
                return;
            }

            //Debug.Log("WindowManager.Navigate() : windowstack is not zero: " + CurrentWindow.gameObject.name);

            // joystick movement
            if (controlsManager.InputHorizontal != 0f || controlsManager.InputVertical != 0f) {
                CurrentWindow.LeftAnalog(controlsManager.InputHorizontal, controlsManager.InputVertical);
            }

            // d pad navigation
            if (controlsManager.DPadUpPressed) {
                CurrentWindow.UpButton();
            }
            if (controlsManager.DPadDownPressed) {
                CurrentWindow.DownButton();
            }
            if (controlsManager.DPadLeftPressed) {
                CurrentWindow.LeftButton();
            }
            if (controlsManager.DPadRightPressed) {
                CurrentWindow.RightButton();
            }
            if (controlsManager.LeftTriggerPressed) {
                CurrentWindow.LeftTrigger();
            }
            if (controlsManager.RightTriggerPressed) {
                CurrentWindow.RightTrigger();
            }

            // buttons
            if (inputManager.KeyBindWasPressed("ACCEPT") || inputManager.KeyBindWasPressed("GAMEPADBUTTONA")) {
                //Debug.Log("Accept");
                CurrentWindow.Accept();
            }
            if (inputManager.KeyBindWasPressed("GAMEPADBUTTONB")) {
                CurrentWindow.Cancel();
            }
            if (inputManager.KeyBindWasPressed("GAMEPADBUTTONX")) {
                CurrentWindow.JoystickButton2();
            }
            if (inputManager.KeyBindWasPressed("GAMEPADBUTTONY")) {
                CurrentWindow.JoystickButton3();
            }
            if (inputManager.KeyBindWasPressed("GAMEPADBUTTONLEFTSHOULDER")) {
                CurrentWindow.JoystickButton4();
            }
            if (inputManager.KeyBindWasPressed("GAMEPADBUTTONRIGHTSHOULDER")) {
                CurrentWindow.JoystickButton5();
            }
            if (inputManager.KeyBindWasPressed("GAMEPADBUTTONRIGHTSTICK")) {
                CurrentWindow.JoystickButton9();
            }

        }

       

    }

}