using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace AnyRPG {
    public class InputActionNode {

        private string actionName;
        private InputAction inputAction;
        private KeyBindType keyBindType;
        // the label to use in the keybind manager
        private string bindKeyLabel = string.Empty;
        
        private string actionButtonString = string.Empty;
        private string keyboardString = string.Empty;

        private KeyBindSlotScript keyBindSlotScript = null;

        private ActionButton actionButton = null;

        // tracker to see if the key was pressed this frame
        private bool keyPressed = false;

        private bool keyHeld = false;

        private bool keyUp = false;

        // prevent multiple triggers
        private bool keyLocked = false;

        public InputActionNode(string actionName, InputAction inputAction, string bindKeyLabel, KeyBindType keyBindType) {
            this.actionName = actionName;
            this.inputAction = inputAction;
            this.bindKeyLabel = bindKeyLabel;
            this.keyBindType = keyBindType;
            FormatActionButtonLabel();
        }

        public string ActionName { get => actionName; set => actionName = value; }
        public string Label { get => bindKeyLabel; set => bindKeyLabel = value; }

        public ActionButton ActionButton {
            get => actionButton;
            set {
                actionButton = value;
                actionButton.UpdateKeybindText(actionButtonString);
            }
        }

        public KeyBindSlotScript KeyBindSlotScript { get => keyBindSlotScript; set => keyBindSlotScript = value; }
        public KeyBindType KeyBindType { get => keyBindType; set => keyBindType = value; }
        public bool KeyPressed { get => keyPressed; }
        public bool KeyHeld { get => keyHeld; }
        public bool KeyUp { get => keyUp; }
        public InputAction InputAction { get => inputAction; }
        public string KeyboardString { get => keyboardString; }

        public void FormatActionButtonLabel() {
            Debug.Log($"InputActionNode.FormatActionButtonLabel() called for action: {actionName}");

            if (inputAction == null || inputAction.bindings.Count == 0) {
                Debug.Log($"No bindings found for action '{actionName}'");
                return;
            }

            string keyboardPath = string.Empty;
            bool hasCtrlModifier = false;
            bool hasShiftModifier = false;

            // 1. Scan the bindings to locate the active keyboard path and check for modifiers
            for (int i = 0; i < inputAction.bindings.Count; i++) {
                InputBinding binding = inputAction.bindings[i];
                string activePath = !string.IsNullOrEmpty(binding.overridePath) ? binding.overridePath : binding.path;

                if (activePath.Contains("Keyboard")) {
                    // If it is part of a composite binding structure, look for modifier parts
                    if (binding.isPartOfComposite) {
                        if (binding.name == "modifier" || binding.name == "modifier1" || binding.name == "modifier2") {
                            if (activePath.Contains("ctrl") || activePath.Contains("control")) hasCtrlModifier = true;
                            if (activePath.Contains("shift")) hasShiftModifier = true;
                        } else if (binding.name == "binding") {
                            keyboardPath = activePath;
                        }
                    } else {
                        // Standalone keyboard key found
                        keyboardPath = activePath;
                    }
                } else {
                    Debug.Log($"activepath '{activePath}' for binding index {i} is not a keyboard path, skipping.");
                }
            }

            // 2. Return an empty string if no keyboard key is bound to this action
            if (string.IsNullOrEmpty(keyboardPath)) {
                Debug.Log($"No keyboard key bound for action '{actionName}'");
                return;
            }

            // 3. Convert the native path directly into a clean, player-facing string (e.g., "=" or "1")
            string readableKeyName = InputControlPath.ToHumanReadableString(
                keyboardPath,
                InputControlPath.HumanReadableStringOptions.OmitDevice
            ).Replace("\"", "");

            if (readableKeyName.Length == 1) {
                readableKeyName = readableKeyName.ToUpper();
            }

            // 4. Construct and return your clean short-hand label format (e.g., "cs1")
            actionButtonString = (hasCtrlModifier ? "c" : "") + (hasShiftModifier ? "s" : "") + readableKeyName;
            keyboardString = (hasCtrlModifier ? "ctrl+" : "") + (hasShiftModifier ? "shift+" : "") + readableKeyName;
            
            Debug.Log($"Formatted action button label for action '{actionName}': '{actionButtonString}' (keyboard string: '{keyboardString}') readableKeyName: {readableKeyName}");
        }

        public void UpdateInputAction(bool control, bool shift, string nativePath) {
            Debug.Log($"InputActionNode.UpdateInputAction() called for action: {actionName} with control={control}, shift={shift}, nativePath='{nativePath}'");

            if (string.IsNullOrEmpty(nativePath)) return;
            if (inputAction == null) return;

            // --- STEP 1: CLEANUP EXISTING KEYBOARD BINDINGS ---
            // We must strip out old keyboard tracks first so they don't stack up layout duplicates.
            for (int i = inputAction.bindings.Count - 1; i >= 0; i--) {
                InputBinding b = inputAction.bindings[i];
                string activePath = !string.IsNullOrEmpty(b.overridePath) ? b.overridePath : b.path;

                // If it belongs to a keyboard path, clear the override or delete the composite track
                if (activePath.StartsWith("<Keyboard>") || b.path.StartsWith("<Keyboard>")) {
                    if (b.isPartOfComposite) {
                        // Find the parent composite header row and wipe its group structure override cleanly
                        int checkIndex = i;
                        while (checkIndex >= 0 && !inputAction.bindings[checkIndex].isComposite) {
                            checkIndex--;
                        }
                        if (checkIndex >= 0) {
                            inputAction.RemoveBindingOverride(checkIndex);
                        }
                    } else {
                        inputAction.RemoveBindingOverride(i);
                    }
                }
            }

            // --- STEP 2: APPLY THE NEW BINDING LAYOUT ---
            // Case A: Standalone Key (No modifier keys held down)
            if (!control && !shift) {
                // Find the first non-composite keyboard binding slot to overwrite, or append a fresh one
                int targetIndex = inputAction.bindings.IndexOf(b => b.path.StartsWith("<Keyboard>") && !b.isPartOfComposite);

                if (targetIndex != -1) {
                    inputAction.ApplyBindingOverride(targetIndex, new InputBinding { overridePath = nativePath });
                } else {
                    // Append safely to the end of the binding collection array
                    inputAction.AddBinding(nativePath);
                }
                Debug.Log($"Successfully bound standalone key to path: {nativePath}");
                ProcessUpdateAction();

                return;
            }

            // Case B: Composite Modifier Binding (Ctrl, Shift, or Both are active)
            string compositeType = (control && shift) ? "TwoModifiers" : "OneModifier";

            // Programmatically initialize a runtime composite binding string construct
            var compositeBuilder = inputAction.AddCompositeBinding(compositeType);

            // Inject the structural modifier parameters based on the layout state
            if (control && shift) {
                compositeBuilder.With("modifier1", "<Keyboard>/ctrl")
                                .With("modifier2", "<Keyboard>/shift")
                                .With("binding", nativePath);
            } else if (control) {
                compositeBuilder.With("modifier", "<Keyboard>/ctrl")
                                .With("binding", nativePath);
            } else if (shift) {
                compositeBuilder.With("modifier", "<Keyboard>/shift")
                                .With("binding", nativePath);
            }
            Debug.Log($"Successfully built '{compositeType}' composite for {inputAction.name} linking {nativePath}");

            ProcessUpdateAction();
        }

        /*
        public string ReplaceSpecialCharacters(string inputString) {
            inputString = inputString.Replace("Digit", "");
            inputString = inputString.Replace("Period", ".");
            inputString = inputString.Replace("Minus", "-");
            inputString = inputString.Replace("Equals", "=");
            return inputString;
        }
        */

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

        public void ProcessUpdateAction() {
            Debug.Log($"InputActionNode.ProcessUpdateAction() called for action: {actionName}");

            FormatActionButtonLabel();
            if (keyBindSlotScript != null) {
                keyBindSlotScript.UpdateLabel();
            }
            if (actionButton != null) {
                actionButton.UpdateKeybindText(actionButtonString);
            }
        }
    }

    public enum InputDeviceType { Keyboard, Joystick, Mobile }

}