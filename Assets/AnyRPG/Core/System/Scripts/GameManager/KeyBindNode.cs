using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace AnyRPG {
    public class KeyBindNode {
        public event System.Action OnKeyPressedHandler = delegate { };

        private string keyBindID;

        private Key keyboardKey;
        private GamepadButton gamepadButton;
        //private KeyCode mobileKeyCode;

        // A clean, readable reference for your "None" state
        public const GamepadButton GamepadNone = (GamepadButton)(-1);

        private bool controlModifier = false;

        private bool shiftModifier = false;

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

        public KeyBindNode(string keyBindID, Key keyboardKey, GamepadButton gamepadButton, /*KeyCode mobileKeyCode,*/ string label, KeyBindType keyBindType, bool control = false, bool shift = false) {
            //Debug.Log("KeyBindNode(" + keyBindID + ")");
            this.keyBindID = keyBindID;
            this.label = label;
            this.keyBindType = keyBindType;
            this.controlModifier = control;
            this.shiftModifier = shift;
            this.gamepadButton = gamepadButton;
            //this.mobileKeyCode = mobileKeyCode;
            this.KeyboardKey = keyboardKey;
        }

        public string KeyBindID { get => keyBindID; set => keyBindID = value; }

        public Key KeyboardKey {
            get => keyboardKey;
            set {
                //Debug.Log("KeyBindNode.SetKeyboardKeyCode: " + value);
                keyboardKey = value;
                if (ActionButton != null) {
                    //Debug.Log("KeyBindNode.SetKeyboardKeyCode : actionbutton is not null");
                    ActionButton.KeyBindText.text = FormatActionButtonLabel();
                }
                if (KeyBindSlotScript != null) {
                    KeyBindSlotScript.Initialize(this);
                }
            }
        }

        public GamepadButton GamepadButton {
            get => gamepadButton;
            set {
                //Debug.Log("KeyBindNode.SetMyKeyCode");
                gamepadButton = value;
                /*
                if (MyActionButton != null) {
                    MyActionButton.MyKeyBindText.text = FormatActionButtonLabel();
                }
                */
                if (KeyBindSlotScript != null) {
                    KeyBindSlotScript.Initialize(this);
                }
            }
        }

        /*
        public KeyCode MobileKeyCode {
            get => mobileKeyCode;
            set {
                //Debug.Log("KeyBindNode.SetMyKeyCode");
                mobileKeyCode = value;
                
                //if (MyActionButton != null) {
                  //  MyActionButton.MyKeyBindText.text = FormatActionButtonLabel();
                //}
                if (KeyBindSlotScript != null) {
                    KeyBindSlotScript.Initialize(this);
                }
            }
        }
*/

        public string Label { get => label; set => label = value; }

        public ActionButton ActionButton {
            get => actionButton;
            set {
                //Debug.Log("KeyBindNode.SetActionButton: " + (value == null ? "null" : value.GetInstanceID().ToString()) + "keybindID: " + keyBindID);
                actionButton = value;
                actionButton.KeyBindText.text = FormatActionButtonLabel();
            }
        }

        public KeyBindSlotScript KeyBindSlotScript { get => keyBindSlotScript; set => keyBindSlotScript = value; }
        public KeyBindType KeyBindType { get => keyBindType; set => keyBindType = value; }
        public bool Control { get => controlModifier; set => controlModifier = value; }
        public bool Shift { get => shiftModifier; set => shiftModifier = value; }
        public bool KeyPressed { get => keyPressed; }
        public bool KeyHeld { get => keyHeld; }
        public bool KeyUp { get => keyUp; }

        private string FormatActionButtonLabel() {
            //Debug.Log("KeyBindNode.FormatActionButtonLabel() : " + KeyboardKeyCode.ToString());
            if (KeyboardKey.ToString() == "None") {
                return string.Empty;
            }
            return (controlModifier ? "c" : "") + (shiftModifier ? "s" : "") + ReplaceSpecialCharacters(KeyboardKey.ToString());
            //return keyBindID;
        }

        public string ReplaceSpecialCharacters(string inputString) {
            inputString = inputString.Replace("Alpha", "");
            inputString = inputString.Replace("Period", ".");
            inputString = inputString.Replace("Minus", "-");
            inputString = inputString.Replace("Equals", "=");
            return inputString;
        }

        public void SetSlotScript(KeyBindSlotScript keyBindSlotScript) {
            this.keyBindSlotScript = keyBindSlotScript;
        }

        public void UpdateKeyCode(InputDeviceType inputDeviceType, Key keyboardKey, GamepadButton gamepadButton, bool control, bool shift) {
            //Debug.Log("KeyBindNode.UpdateKeyCode(" + inputDeviceType + ", " + keyCode + ", " + control + ", " + shift + ")");

            if (inputDeviceType == InputDeviceType.Keyboard) {
                this.KeyboardKey = keyboardKey;
                this.controlModifier = control;
                this.shiftModifier = shift;
            } else if (inputDeviceType == InputDeviceType.Joystick) {
                this.GamepadButton = gamepadButton;
            }/* else if (inputDeviceType == InputDeviceType.Mobile) {
                this.MobileKeyCode = keyCode;
            }*/
        }

        public void RegisterKeyPress() {
            if (keyLocked == false) {
                keyPressed = true;
                keyLocked = true;
                OnKeyPressedHandler();
            }
        }

        public void UnRegisterKeyPress(bool unlock = false) {
            keyPressed = false;
            if (unlock == true) {
                keyLocked = false;
            }
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