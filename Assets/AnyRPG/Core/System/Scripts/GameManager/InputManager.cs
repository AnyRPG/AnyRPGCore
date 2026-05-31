using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace AnyRPG {

    public class InputManager : ConfiguredMonoBehaviour {

        public bool rightMouseButtonDown = false;
        public bool middleMouseButtonDown = false;
        public bool rightMouseButtonUp = false;
        public bool middleMouseButtonUp = false;
        //public bool rightMouseButtonStayDown = false;
        public bool rightMouseButtonClicked = false;
        public bool middleMouseButtonClicked = false;
        public bool rightMouseButtonClickedOverUI = false;
        public bool middleMouseButtonClickedOverUI = false;
        public Vector3 rightMouseButtonDownPosition = Vector3.zero;
        public Vector3 rightMouseButtonUpPosition = Vector3.zero;
        public Vector3 middleMouseButtonDownPosition = Vector3.zero;
        public Vector3 middleMouseButtonUpPosition = Vector3.zero;
        public bool leftMouseButtonDown = false;
        public bool leftMouseButtonUp = false;
        //public bool leftMouseButtonStayDown = false;
        public bool leftMouseButtonClicked = false;
        public bool leftMouseButtonClickedOverUI = false;
        public Vector3 leftMouseButtonDownPosition = Vector3.zero;
        public Vector3 leftMouseButtonUpPosition = Vector3.zero;
        public bool mouseScrolled = false;
        
        private int lastRegisteredFrame = 0;

        // game manager references
        private KeyBindManager keyBindManager = null;
        private UIManager uIManager = null;
        private NameplateManager namePlateManager = null;
        private LevelManagerClient levelManagerClient = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            levelManagerClient.OnLevelLoad += HandleLevelLoad;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            keyBindManager = systemGameManager.KeyBindManager;
            uIManager = systemGameManager.UIManager;
            namePlateManager = uIManager.NameplateManager;
            levelManagerClient = systemGameManager.LevelManagerClient;
        }

        public void HandleLevelLoad() {
            ResetAllInput();
        }

        private void ResetAllInput() {
            //Debug.Log("InputManager.ResetAllInput()");
            rightMouseButtonClicked = false;
            rightMouseButtonClickedOverUI = false;
            rightMouseButtonDown = false;
            rightMouseButtonUp = false;
            leftMouseButtonClicked = false;
            leftMouseButtonClickedOverUI = false;
            leftMouseButtonDown = false;
            leftMouseButtonUp = false;
            middleMouseButtonClicked = false;
            middleMouseButtonClickedOverUI = false;
            middleMouseButtonDown = false;
            middleMouseButtonUp = false;
            mouseScrolled = false;
            ResetKeyBindNodes();
        }

        public void ResetKeyBindNodes() {
            foreach (KeyBindNode keyBindNode in keyBindManager.KeyBinds.Values) {
                keyBindNode.UnRegisterKeyPress(true);
                keyBindNode.UnRegisterKeyHeld();
                keyBindNode.UnRegisterKeyUp();
            }
        }

        public void RegisterInput() {
            if (keyBindManager.BindName != string.Empty) {
                // we are binding a key.  discard all input
                //Debug.Log("Key Binding in progress.  returning.");
                foreach (KeyBindNode keyBindNode in keyBindManager.KeyBinds.Values) {
                    keyBindNode.UnRegisterKeyPress(true);
                    keyBindNode.UnRegisterKeyHeld();
                    keyBindNode.UnRegisterKeyUp();
                }
                return;
            }

            RegisterMouseActions();
            RegisterKeyPresses();

        }

        public void RegisterKeyPresses() {
            if (lastRegisteredFrame >= Time.frameCount) {
                // we have already registered keypresses this frame
                //Debug.Log("keypresses already registered this frame");
                return;
            }
            lastRegisteredFrame = Time.frameCount;

            // Get references to active hardware devices
            var keyboard = Keyboard.current;
            var gamepad = Gamepad.current;

            bool control = false;
            bool shift = false;

            // 1. Process Modifiers (Control)
            if (keyboard != null) {
                if (keyboard.leftCtrlKey.wasPressedThisFrame || keyboard.leftCtrlKey.isPressed ||
                    keyboard.rightCtrlKey.wasPressedThisFrame || keyboard.rightCtrlKey.isPressed) {
                    control = true;
                }
            }

            // 2. Process Modifiers (Shift)
            if (keyboard != null) {
                if (keyboard.leftShiftKey.wasPressedThisFrame || keyboard.leftShiftKey.isPressed ||
                    keyboard.rightShiftKey.wasPressedThisFrame || keyboard.rightShiftKey.isPressed) {
                    shift = true;
                }
            }

            foreach (KeyBindNode keyBindNode in keyBindManager.KeyBinds.Values) {

                // Setup temporary state flags for this loop cycle
                bool keyDown = false;
                bool keyHeld = false;
                bool keyUp = false;

                // 1. Evaluate Keyboard Hardware State
                if (keyboard != null && keyBindNode.KeyboardKey != Key.None) {
                    KeyControl keyControl = keyboard[keyBindNode.KeyboardKey];
                    if (keyControl.wasPressedThisFrame) keyDown = true;
                    if (keyControl.isPressed) keyHeld = true;
                    if (keyControl.wasReleasedThisFrame) keyUp = true;
                }

                // 2. Evaluate Gamepad Hardware State
                if (gamepad != null) {
                    // Cast the node's stored enum to an integer to safely verify if it's assigned
                    int buttonValue = (int)keyBindNode.GamepadButton;

                    // Only run polling logic if the value is 0 or greater (skipping our custom -1 "None" state)
                    if (buttonValue >= 0) {
                        var buttonControl = gamepad[keyBindNode.GamepadButton];

                        // If any keyboard state was already true, preserve it via the ||= operator
                        if (buttonControl.wasPressedThisFrame) keyDown = true;
                        if (buttonControl.isPressed) keyHeld = true;
                        if (buttonControl.wasReleasedThisFrame) keyUp = true;
                    }
                }

                // --- 3. Execute AnyRPG Logical Matrix Filters ---

                // Register key down state
                if (keyDown && (keyBindNode.KeyBindType == KeyBindType.Normal ||
                   ((control == keyBindNode.Control) && (shift == keyBindNode.Shift)))) {
                    keyBindNode.RegisterKeyPress();
                } else {
                    keyBindNode.UnRegisterKeyPress();
                }

                // Register key held state
                if (keyHeld && (keyBindNode.KeyBindType == KeyBindType.Normal ||
                   ((control == keyBindNode.Control) && (shift == keyBindNode.Shift)))) {
                    keyBindNode.RegisterKeyHeld();
                } else {
                    keyBindNode.UnRegisterKeyHeld();
                }

                // Register key up state
                if (keyUp) {
                    keyBindNode.RegisterKeyUp();
                } else {
                    keyBindNode.UnRegisterKeyUp();
                }

                /*
                // normal should eventually changed to movement, but there is only one other key (toggle run) that is normal for now, so normal is ok until more keys are added
                // register key down
                if ((Input.GetKeyDown(keyBindNode.KeyboardKey) || Input.GetKeyDown(keyBindNode.GamepadButton))
                    && (keyBindNode.KeyBindType == KeyBindType.Normal || ((control == keyBindNode.Control) && (shift == keyBindNode.Shift)))) {
                    //Debug.Log(keyBindNode.KeyboardKeyCode + " " + keyBindNode.JoystickKeyCode + " pressed true! " + keyBindNode.KeyBindID);
                    keyBindNode.RegisterKeyPress();
                } else {
                    keyBindNode.UnRegisterKeyPress();
                }

                if ((Input.GetKey(keyBindNode.KeyboardKey) || Input.GetKey(keyBindNode.GamepadButton))
                    && (keyBindNode.KeyBindType == KeyBindType.Normal || (control == keyBindNode.Control) && (shift == keyBindNode.Shift))) {
                    //Debug.Log(keyBindNode.KeyboardKeyCode + " " + keyBindNode.JoystickKeyCode + " held true!");
                    keyBindNode.RegisterKeyHeld();
                } else {
                    keyBindNode.UnRegisterKeyHeld();
                }

                // register key up
                if (Input.GetKeyUp(keyBindNode.KeyboardKey) || Input.GetKeyUp(keyBindNode.GamepadButton)) {
                    //Debug.Log(keyBindNode.KeyboardKeyCode + " " + keyBindNode.JoystickKeyCode + " up true! " + keyBindNode.KeyBindID);
                    keyBindNode.RegisterKeyUp();
                } else {
                    keyBindNode.UnRegisterKeyUp();
                }
                */

            }
        }

        public bool KeyBindWasPressedOrHeld(string keyBindID) {
            if (keyBindManager.KeyBinds.ContainsKey(keyBindID) &&
                (keyBindManager.KeyBinds[keyBindID].KeyPressed == true || keyBindManager.KeyBinds[keyBindID].KeyHeld == true)) {
                return true;
            }
            return false;
        }

        public bool KeyBindWasPressed(string keyBindID) {
            if (keyBindManager.KeyBinds.ContainsKey(keyBindID) && keyBindManager.KeyBinds[keyBindID].KeyPressed == true) {
                return true;
            }
            //Debug.Log(keyBindID + " : False");
            return false;
        }

        /// <summary>
        /// Detect clicks and drags
        /// </summary>
        private void RegisterMouseActions() {
            leftMouseButtonClicked = false;
            rightMouseButtonClicked = false;
            middleMouseButtonClicked = false;
            leftMouseButtonUp = false;
            rightMouseButtonUp = false;
            middleMouseButtonUp = false;
            mouseScrolled = false;
            EventParamProperties eventParam = new EventParamProperties();


            // track left mouse button up and down events to determine difference in click vs drag
            if (Input.GetMouseButtonUp(0)) {
                if (leftMouseButtonDown) {
                    leftMouseButtonUpPosition = Input.mousePosition;
                    leftMouseButtonUp = true;
                    //Debug.Log($"down mouse position: {leftMouseButtonDownPosition.ToString()} up mouse position: {leftMouseButtonUpPosition.ToString()}");
                    if (leftMouseButtonUpPosition == leftMouseButtonDownPosition) {
                        leftMouseButtonClicked = true;
                    }
                    leftMouseButtonDown = false;
                    leftMouseButtonClickedOverUI = false;
                }
            } else {
                leftMouseButtonUp = false;
            }

            // track right mouse button up and down events to determine difference in click vs drag
            if (Input.GetMouseButtonUp(1)) {
                SystemEventManager.TriggerEvent("OnRightMouseButtonUp", eventParam);
                if (rightMouseButtonDown) {
                    rightMouseButtonUpPosition = Input.mousePosition;
                    rightMouseButtonUp = true;
                    //Debug.Log($"down mouse position: {rightMouseButtonDownPosition.ToString()} up mouse position: {rightMouseButtonUpPosition.ToString()}");
                    if (rightMouseButtonUpPosition == rightMouseButtonDownPosition) {
                        rightMouseButtonClicked = true;
                    }
                    rightMouseButtonDown = false;
                    rightMouseButtonClickedOverUI = false;
                }
            } else {
                rightMouseButtonUp = false;
            }


            // track middle mouse button up and down events to determine difference in click vs drag
            if (Input.GetMouseButtonUp(2) && middleMouseButtonDown) {
                middleMouseButtonUpPosition = Input.mousePosition;
                middleMouseButtonUp = true;
                //Debug.Log($"down mouse position: {rightMouseButtonDownPosition.ToString()} up mouse position: {rightMouseButtonUpPosition.ToString()}");
                if (middleMouseButtonUpPosition == middleMouseButtonDownPosition) {
                    middleMouseButtonClicked = true;
                }
                middleMouseButtonDown = false;
                middleMouseButtonClickedOverUI = false;
            } else {
                middleMouseButtonUp = false;
            }


            // return if the mouse is off the screen, after detecting mouse up events (in case we started a pan while on screen)
            Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
            if (!screenRect.Contains(Input.mousePosition))
                return;

            if (Input.GetMouseButtonDown(1)) {
                rightMouseButtonDown = true;
                rightMouseButtonDownPosition = Input.mousePosition;
                // IGNORE NAMEPLATES FOR THE PURPOSE OF CAMERA MOVEMENT
                if (EventSystem.current.IsPointerOverGameObject() && (namePlateManager != null ? !namePlateManager.MouseOverNameplate() : true)) {
                    rightMouseButtonClickedOverUI = true;
                }
            }

            // track left mouse button up and down events to determine difference in click vs drag
            if (Input.GetMouseButtonDown(0)) {
                leftMouseButtonDown = true;
                leftMouseButtonDownPosition = Input.mousePosition;
                if (EventSystem.current.IsPointerOverGameObject() && (namePlateManager != null ? !namePlateManager.MouseOverNameplate() : true)) {
                    leftMouseButtonClickedOverUI = true;
                }
            }

            if (Input.GetMouseButtonDown(2)) {
                middleMouseButtonDown = true;
                middleMouseButtonDownPosition = Input.mousePosition;
                if (EventSystem.current.IsPointerOverGameObject() && (namePlateManager != null ? !namePlateManager.MouseOverNameplate() : true)) {
                    middleMouseButtonClickedOverUI = true;
                }
            }

            float mouseWheel = Input.GetAxis("Mouse ScrollWheel");
            if (mouseWheel != 0) {
                //Debug.Log($"Mouse scrolled: {mouseWheel}");
                mouseScrolled = true;
            }

        }
    }

}