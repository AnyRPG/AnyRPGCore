using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace AnyRPG {
    public class InputActionNode {

        private string actionName;
        private InputAction inputAction;
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

        public InputActionNode(string actionName, InputAction inputAction, string label, KeyBindType keyBindType) {
            //Debug.Log("KeyBindNode(" + keyBindID + ")");
            this.actionName = actionName;
            this.inputAction = inputAction;
            this.label = label;
            this.keyBindType = keyBindType;
        }

        public string ActionName { get => actionName; set => actionName = value; }
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
        public bool KeyPressed { get => keyPressed; }
        public bool KeyHeld { get => keyHeld; }
        public bool KeyUp { get => keyUp; }

        private string FormatActionButtonLabel() {
            //Debug.Log("KeyBindNode.FormatActionButtonLabel() : " + KeyboardKeyCode.ToString());
            /*
            if (KeyboardKey.ToString() == "None") {
                return string.Empty;
            }
            return (controlModifier ? "c" : "") + (shiftModifier ? "s" : "") + ReplaceSpecialCharacters(KeyboardKey.ToString());
            */
            return string.Empty;
        }

        public void UpdateInputAction(bool control, bool shift, string nativePath) {
            if (string.IsNullOrEmpty(nativePath)) return;

            inputAction.ApplyBindingOverride(new InputBinding {
                overridePath = nativePath
            });

            //Debug.Log($"Successfully rebound {inputAction.name} to native path: {nativePath}");
        }

        public string ReplaceSpecialCharacters(string inputString) {
            inputString = inputString.Replace("Digit", "");
            inputString = inputString.Replace("Period", ".");
            inputString = inputString.Replace("Minus", "-");
            inputString = inputString.Replace("Equals", "=");
            return inputString;
        }

        public void SetSlotScript(KeyBindSlotScript keyBindSlotScript) {
            this.keyBindSlotScript = keyBindSlotScript;
        }

        public void RegisterKeyPress() {
            if (keyLocked == false) {
                keyPressed = true;
                keyLocked = true;
                //OnKeyPressedHandler();
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