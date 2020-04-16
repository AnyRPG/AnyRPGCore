using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AnyRPG {

    public class InputManager : MonoBehaviour {

        #region Singleton
        private static InputManager instance;

        public static InputManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<InputManager>();
                }

                return instance;
            }
        }

        #endregion

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

        void Update() {
            if (KeyBindManager.MyInstance.MyBindName != string.Empty) {
                // we are binding a key.  discard all input
                //Debug.Log("Key Binding in progress.  returning.");
                return;
            }

            RegisterMouseActions();

        }

        public bool KeyBindWasPressed(string keyBindID) {
            if (SystemWindowManager.MyInstance.nameChangeWindow.IsOpen) {
                //Debug.Log("Not allowing registration of keystrokes to keybinds during name change");
                return false;
            }
            bool control = (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl) || Input.GetKey(KeyCode.RightControl));
            bool shift = (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift) || Input.GetKey(KeyCode.RightShift));
            foreach (KeyBindNode keyBindNode in KeyBindManager.MyInstance.MyKeyBinds.Values) {
                // normal should eventually changed to movement, but there is only one other key (toggle run) that is normal for now, so normal is ok until more keys are added
                if (Input.GetKeyDown(keyBindNode.MyKeyCode) && keyBindNode.MyKeyBindID == keyBindID && (keyBindNode.MyKeyBindType == KeyBindType.Normal || ((control == keyBindNode.MyControl) && (shift == keyBindNode.MyShift)))) {
                    //Debug.Log(keyBindNode.MyKeyCode + " true!");
                    return true;
                }
            }
            return false;
        }

        public bool KeyBindWasPressedOrHeld(string keyBindID) {
            if (SystemWindowManager.MyInstance.nameChangeWindow.IsOpen) {
                //Debug.Log("Not allowing registration of keystrokes to keybinds during name change");
                return false;
            }
            bool control = (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl) || Input.GetKey(KeyCode.RightControl));
            bool shift = (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift) || Input.GetKey(KeyCode.RightShift));
            foreach (KeyBindNode keyBindNode in KeyBindManager.MyInstance.MyKeyBinds.Values) {
                if (keyBindNode.MyKeyBindID == keyBindID) {
                    if (Input.GetKeyDown(keyBindNode.MyKeyCode) && (keyBindNode.MyKeyBindType == KeyBindType.Normal || (control == keyBindNode.MyControl) && (shift == keyBindNode.MyShift))) {
                        //Debug.Log(keyBindNode.MyKeyCode + " true!");
                        return true;
                    }
                    if (Input.GetKey(keyBindNode.MyKeyCode) && (keyBindNode.MyKeyBindType == KeyBindType.Normal || (control == keyBindNode.MyControl) && (shift == keyBindNode.MyShift))) {
                        //Debug.Log(keyBindNode.MyKeyCode + " true!");
                        return true;
                    }
                }
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
            EventParam eventParam = new EventParam();


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
                if (EventSystem.current.IsPointerOverGameObject() && (NamePlateManager.MyInstance != null ? !NamePlateManager.MyInstance.MouseOverNamePlate() : true)) {
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
                if (EventSystem.current.IsPointerOverGameObject() && (NamePlateManager.MyInstance != null ? !NamePlateManager.MyInstance.MouseOverNamePlate() : true)) {
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
                if (EventSystem.current.IsPointerOverGameObject() && (NamePlateManager.MyInstance != null ? !NamePlateManager.MyInstance.MouseOverNamePlate() : true)) {
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