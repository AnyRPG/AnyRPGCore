using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class KeyBindNode {
        public event System.Action OnKeyPressedHandler = delegate { };

        private string keyBindID;

        private KeyCode keyCode;
        private KeyCode joystickKeyCode;
        private KeyCode mobileKeyCode;

        private bool control = false;

        private bool shift = false;

        private KeyBindType keyBindType;

        // the label to use in the keybind manager
        private string label;

        private KeyBindSlotScript keyBindSlotScript = null;

        private ActionButton actionButton = null;

        // tracker to see if the key was pressed this frame
        private bool keyPressed = false;

        private bool keyHeld = false;

        private bool keyUp = false;

        // prevent multiple triggers
        private bool keyLocked = false;

        public KeyBindNode(string keyBindID, KeyCode keyCode, KeyCode joystickKeyCode, KeyCode mobileKeyCode, string label, KeyBindType keyBindType, bool control = false, bool shift = false) {
            //Debug.Log("KeyBindNode(" + keyBindID + ")");
            this.keyBindID = keyBindID;
            this.label = label;
            this.keyBindType = keyBindType;
            this.control = control;
            this.shift = shift;
            this.joystickKeyCode = joystickKeyCode;
            this.mobileKeyCode = mobileKeyCode;
            this.MyKeyCode = keyCode;
        }

        public string MyKeyBindID { get => keyBindID; set => keyBindID = value; }

        public KeyCode MyKeyCode {
            get => keyCode;
            set {
                //Debug.Log("KeyBindNode.SetMyKeyCode");
                keyCode = value;
                if (MyActionButton != null) {
                    MyActionButton.KeyBindText.text = FormatActionButtonLabel();
                }
                if (MyKeyBindSlotScript != null) {
                    MyKeyBindSlotScript.Initialize(this);
                }
            }
        }

        public KeyCode MyJoystickKeyCode {
            get => joystickKeyCode;
            set {
                //Debug.Log("KeyBindNode.SetMyKeyCode");
                joystickKeyCode = value;
                /*
                if (MyActionButton != null) {
                    MyActionButton.MyKeyBindText.text = FormatActionButtonLabel();
                }
                */
                if (MyKeyBindSlotScript != null) {
                    MyKeyBindSlotScript.Initialize(this);
                }
            }
        }

        public KeyCode MyMobileKeyCode {
            get => mobileKeyCode;
            set {
                //Debug.Log("KeyBindNode.SetMyKeyCode");
                mobileKeyCode = value;
                /*
                if (MyActionButton != null) {
                    MyActionButton.MyKeyBindText.text = FormatActionButtonLabel();
                }
                */
                if (MyKeyBindSlotScript != null) {
                    MyKeyBindSlotScript.Initialize(this);
                }
            }
        }

        public string MyLabel { get => label; set => label = value; }

        public ActionButton MyActionButton {
            get => actionButton;
            set {
                //Debug.Log("KeyBindNode.SetMyActionButton: " + (value == null ? "null" : value.GetInstanceID().ToString()) + "keybindID: " + keyBindID);
                actionButton = value;
                actionButton.KeyBindText.text = FormatActionButtonLabel();
            }
        }

        public KeyBindSlotScript MyKeyBindSlotScript { get => keyBindSlotScript; set => keyBindSlotScript = value; }
        public KeyBindType MyKeyBindType { get => keyBindType; set => keyBindType = value; }
        public bool MyControl { get => control; set => control = value; }
        public bool MyShift { get => shift; set => shift = value; }
        public bool KeyPressed { get => keyPressed; }
        public bool KeyHeld { get => keyHeld; }
        public bool KeyUp { get => keyUp; }

        private string FormatActionButtonLabel() {
            if (MyKeyCode.ToString() == "None") {
                return string.Empty;
            }
            return (control ? "c" : "") + (shift ? "s" : "") + ReplaceSpecialCharacters(MyKeyCode.ToString());
            //return keyBindID;
        }

        public string ReplaceSpecialCharacters(string inputString) {
            inputString = inputString.Replace("Alpha", "");
            inputString = inputString.Replace("Period", ".");
            return inputString;
        }

        public void SetSlotScript(KeyBindSlotScript keyBindSlotScript) {
            this.keyBindSlotScript = keyBindSlotScript;
        }

        public void UpdateKeyCode(InputDeviceType inputDeviceType, KeyCode keyCode, bool control, bool shift) {
            //Debug.Log("KeyBindNode.UpdateKeyCode(" + inputDeviceType + ", " + keyCode + ", " + control + ", " + shift + ")");
            if (inputDeviceType == InputDeviceType.Keyboard) {
                this.MyKeyCode = keyCode;
                this.control = control;
                this.shift = shift;
            } else if (inputDeviceType == InputDeviceType.Joystick) {
                this.MyJoystickKeyCode = keyCode;
            } else if (inputDeviceType == InputDeviceType.Mobile) {
                this.MyMobileKeyCode = keyCode;
            }
            SendKeyBindEvent();
        }

        public void SendKeyBindEvent() {
            EventParamProperties eventParamProperties = new EventParamProperties();
            SimpleParamNode simpleParamNode = new SimpleParamNode();
            simpleParamNode.MyParamType = SimpleParamType.stringType;
            simpleParamNode.MySimpleParams.StringParam = this.keyCode.ToString();
            eventParamProperties.objectParam.MySimpleParams.Add(simpleParamNode);
            simpleParamNode = new SimpleParamNode();
            simpleParamNode.MyParamType = SimpleParamType.stringType;
            simpleParamNode.MySimpleParams.StringParam = this.joystickKeyCode.ToString();
            eventParamProperties.objectParam.MySimpleParams.Add(simpleParamNode);
            simpleParamNode = new SimpleParamNode();
            simpleParamNode.MyParamType = SimpleParamType.stringType;
            simpleParamNode.MySimpleParams.StringParam = this.mobileKeyCode.ToString();
            eventParamProperties.objectParam.MySimpleParams.Add(simpleParamNode);
            SystemEventManager.TriggerEvent("OnBindKey" + keyBindID, eventParamProperties);
        }

        /*
        public void UpdateKeyCode(KeyCode keyCode, bool control, bool shift) {
            //Debug.Log("KeyBindNode.UpdateKeyCode(" + keyCode + ", " + control + ", " + shift + ")");
            this.control = control;
            this.shift = shift;
            this.MyKeyCode = keyCode;
        }
        */

        public void RegisterKeyPress() {
            if (keyLocked == false) {
                keyPressed = true;
                keyLocked = true;
                OnKeyPressedHandler();
            }
        }

        public void UnRegisterKeyPress() {
            keyPressed = false;
        }

        public void RegisterKeyHeld() {
            keyHeld = true;
        }

        public void UnRegisterKeyHeld() {
            keyHeld = false;
        }

        public void RegisterKeyUp() {
            keyUp = true;
            keyLocked = false;
        }

        public void UnRegisterKeyUp() {
            keyUp = false;
        }
    }

    public enum InputDeviceType { Keyboard, Joystick, Mobile }

}