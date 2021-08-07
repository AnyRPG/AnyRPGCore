using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
        private NamePlateManager namePlateManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            keyBindManager = systemGameManager.KeyBindManager;
            uIManager = systemGameManager.UIManager;
            namePlateManager = uIManager.NamePlateManager;

            SystemEventManager.StartListening("OnLevelLoad", HandleLevelLoad);
        }

        public void OnDestroy() {
            SystemEventManager.StopListening("OnLevelLoad", HandleLevelLoad);
        }

        public void HandleLevelLoad(string eventName, EventParamProperties eventParamProperties) {
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
        }

        void Update() {
            if (keyBindManager.MyBindName != string.Empty) {
                // we are binding a key.  discard all input
                //Debug.Log("Key Binding in progress.  returning.");
                foreach (KeyBindNode keyBindNode in keyBindManager.KeyBinds.Values) {
                    keyBindNode.UnRegisterKeyPress();
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
                return;
            }
            lastRegisteredFrame = Time.frameCount;

            if (uIManager.nameChangeWindow.IsOpen) {
                //Debug.Log("Not allowing registration of keystrokes to keybinds during name change");
                return;
            }

            bool control = false;

            if (Input.GetKeyDown(KeyCode.LeftControl)) {
                //Debug.Log("InputManager.KeyBindWasPressed(): left control pressed");
                control = true;
            }
            if (Input.GetKey(KeyCode.LeftControl)) {
                //Debug.Log("InputManager.KeyBindWasPressed(): left control held");
                control = true;
            }
            if (Input.GetKeyDown(KeyCode.RightControl)) {
                //Debug.Log("InputManager.KeyBindWasPressed(): right control pressed");
                control = true;
            }
            if (Input.GetKey(KeyCode.RightControl)) {
                //Debug.Log("InputManager.KeyBindWasPressed(): right control held");
                control = true;
            }

            bool shift = false;

            if (Input.GetKeyDown(KeyCode.LeftShift)) {
                //Debug.Log("InputManager.KeyBindWasPressed(): left shift pressed");
                shift = true;
            }
            if (Input.GetKey(KeyCode.LeftShift)) {
                //Debug.Log("InputManager.KeyBindWasPressed(): left shift held");
                shift = true;
            }
            if (Input.GetKeyDown(KeyCode.RightShift)) {
                //Debug.Log("InputManager.KeyBindWasPressed(): right shift pressed");
                shift = true;
            }
            if (Input.GetKey(KeyCode.RightShift)) {
                //Debug.Log("InputManager.KeyBindWasPressed(): right shift held");
                shift = true;
            }

            foreach (KeyBindNode keyBindNode in keyBindManager.KeyBinds.Values) {
                // normal should eventually changed to movement, but there is only one other key (toggle run) that is normal for now, so normal is ok until more keys are added
                // register key down
                if (Input.GetKeyDown(keyBindNode.MyKeyCode) && (keyBindNode.MyKeyBindType == KeyBindType.Normal || ((control == keyBindNode.MyControl) && (shift == keyBindNode.MyShift)))) {
                    //Debug.Log(keyBindNode.MyKeyCode + " pressed true!");
                    keyBindNode.RegisterKeyPress();
                } else {
                    keyBindNode.UnRegisterKeyPress();
                }

                if (Input.GetKey(keyBindNode.MyKeyCode) && (keyBindNode.MyKeyBindType == KeyBindType.Normal || (control == keyBindNode.MyControl) && (shift == keyBindNode.MyShift))) {
                    //Debug.Log(keyBindNode.MyKeyCode + " held true!");
                    keyBindNode.RegisterKeyHeld();
                } else {
                    keyBindNode.UnRegisterKeyHeld();
                }

                // register key up
                if (Input.GetKeyUp(keyBindNode.MyKeyCode)) {
                    //Debug.Log(keyBindNode.MyKeyCode + " pressed true!");
                    keyBindNode.RegisterKeyUp();
                } else {
                    keyBindNode.UnRegisterKeyUp();
                }

            }
        }

        public bool KeyBindWasPressedOrHeld(string keyBindID) {
            RegisterKeyPresses();
            if (keyBindManager.KeyBinds.ContainsKey(keyBindID) &&
                (keyBindManager.KeyBinds[keyBindID].KeyPressed == true || keyBindManager.KeyBinds[keyBindID].KeyHeld == true)) {
                return true;
            }
            return false;
        }

        public bool KeyBindWasPressed(string keyBindID) {
            RegisterKeyPresses();
            if (keyBindManager.KeyBinds.ContainsKey(keyBindID) && keyBindManager.KeyBinds[keyBindID].KeyPressed == true) {
                return true;
            }
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
                if (!Input.GetMouseButton(1) && !Input.GetMouseButtonDown(1)) {
                    SystemEventManager.TriggerEvent("OnDisallowMouseMovement", eventParam);
                }
                SystemEventManager.TriggerEvent("OnLeftMouseButtonUp", eventParam);
                if (leftMouseButtonDown) {
                    leftMouseButtonUpPosition = Input.mousePosition;
                    leftMouseButtonUp = true;
                    //Debug.Log("down mouse position: " + leftMouseButtonDownPosition.ToString() + " up mouse position: " + leftMouseButtonUpPosition.ToString());
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
                if (!Input.GetMouseButton(0) && !Input.GetMouseButtonDown(0)) {
                    SystemEventManager.TriggerEvent("OnDisallowMouseMovement", eventParam);
                }
                if (rightMouseButtonDown) {
                    rightMouseButtonUpPosition = Input.mousePosition;
                    rightMouseButtonUp = true;
                    //Debug.Log("down mouse position: " + rightMouseButtonDownPosition.ToString() + " up mouse position: " + rightMouseButtonUpPosition.ToString());
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
                //Debug.Log("down mouse position: " + rightMouseButtonDownPosition.ToString() + " up mouse position: " + rightMouseButtonUpPosition.ToString());
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
                if (EventSystem.current.IsPointerOverGameObject() && (namePlateManager != null ? !namePlateManager.MouseOverNamePlate() : true)) {
                    rightMouseButtonClickedOverUI = true;
                }
                if (!rightMouseButtonClickedOverUI) {
                    SystemEventManager.TriggerEvent("OnAllowMouseMovement", eventParam);
                }
                SystemEventManager.TriggerEvent("OnRightMouseButtonDown", eventParam);

            }

            // track left mouse button up and down events to determine difference in click vs drag
            if (Input.GetMouseButtonDown(0)) {
                leftMouseButtonDown = true;
                leftMouseButtonDownPosition = Input.mousePosition;
                if (EventSystem.current.IsPointerOverGameObject() && (namePlateManager != null ? !namePlateManager.MouseOverNamePlate() : true)) {
                    leftMouseButtonClickedOverUI = true;
                }
                if (!leftMouseButtonClickedOverUI) {
                    SystemEventManager.TriggerEvent("OnAllowMouseMovement", eventParam);
                }
                SystemEventManager.TriggerEvent("OnLeftMouseButtonDown", eventParam);
            }

            if (Input.GetMouseButtonDown(2)) {
                middleMouseButtonDown = true;
                middleMouseButtonDownPosition = Input.mousePosition;
                if (EventSystem.current.IsPointerOverGameObject() && (namePlateManager != null ? !namePlateManager.MouseOverNamePlate() : true)) {
                    middleMouseButtonClickedOverUI = true;
                }
            }

            float mouseWheel = Input.GetAxis("Mouse ScrollWheel");
            if (mouseWheel != 0) {
                mouseScrolled = true;
            }

        }
    }

}