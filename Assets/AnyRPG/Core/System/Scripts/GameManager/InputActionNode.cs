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
            //Debug.Log($"InputActionNode.FormatActionButtonLabel() called for action: {actionName}");

            if (inputAction == null || inputAction.bindings.Count == 0) {
                //Debug.Log($"No bindings found for action '{actionName}'");
                return;
            }

            string keyboardPath = string.Empty;
            bool hasCtrlModifier = false;
            bool hasShiftModifier = false;

            // 1. Scan the bindings to locate the active keyboard path and check for modifiers
            for (int i = 0; i < inputAction.bindings.Count; i++) {
                InputBinding binding = inputAction.bindings[i];
                string activePath = !string.IsNullOrEmpty(binding.overridePath) ? binding.overridePath : binding.path;
                //Debug.Log($"Checking binding index {i} with active path '{activePath}' for action '{actionName}' overridepath '{binding.overridePath}', path '{binding.path}'");

                // FIXED: Also process the row if it's a composite header anchor (e.g., "ButtonWithOneModifier")
                if (activePath.Contains("Keyboard") || binding.isComposite) {

                    // If it is part of a composite binding structure, look for modifier parts
                    if (binding.isPartOfComposite) {
                        // Unity 6 name fields can be "modifier", "modifier1", "modifier2"
                        //Debug.Log($"Processing composite part '{binding.name}' with active path '{activePath}' for binding index {i}");
                        if (binding.name.Contains("modifier")) {
                            if (activePath.ToLower().Contains("ctrl") || activePath.ToLower().Contains("control")) hasCtrlModifier = true;
                            if (activePath.ToLower().Contains("shift")) hasShiftModifier = true;
                        } else if (binding.name == "button" || binding.name == "binding") {
                            // FIXED: Checks for both "button" and "binding" names safely
                            keyboardPath = activePath;
                        }
                    } else if (!binding.isComposite) {
                        // Standalone keyboard key found (only assign if it's not the header itself)
                        keyboardPath = activePath;
                    }
                } else {
                    //Debug.Log($"activepath '{activePath}' for binding index {i} is not a keyboard path, skipping.");
                }
            }

            // 2. Catch empty states, unbound paths, and "<Keyboard>/none" explicit configurations
            string initialReadableCheck = string.Empty;
            if (!string.IsNullOrEmpty(keyboardPath)) {
                initialReadableCheck = InputControlPath.ToHumanReadableString(
                    keyboardPath,
                    InputControlPath.HumanReadableStringOptions.OmitDevice
                ).Trim();
            }

            if (string.IsNullOrEmpty(keyboardPath) || initialReadableCheck.Equals("None", System.StringComparison.OrdinalIgnoreCase)) {
                //Debug.Log($"Unbound or 'none' keyboard state detected for action '{actionName}'");
                actionButtonString = string.Empty;
                keyboardString = "Click To Bind";
                return;
            }

            // 3. Convert the native path directly into a clean, player-facing string (e.g., "=" or "1")
            string readableKeyName = initialReadableCheck.Replace("\"", "");

            if (readableKeyName.Length == 1) {
                readableKeyName = readableKeyName.ToUpper();
            }

            // 4. Construct and return your clean short-hand label format (e.g., "cs1")
            actionButtonString = (hasCtrlModifier ? "c" : "") + (hasShiftModifier ? "s" : "") + readableKeyName;
            keyboardString = (hasCtrlModifier ? "ctrl+" : "") + (hasShiftModifier ? "shift+" : "") + readableKeyName;

            //Debug.Log($"Formatted action button label for action '{actionName}': '{actionButtonString}' (keyboard string: '{keyboardString}') readableKeyName: {readableKeyName}");
        }


        public void UpdateInputAction(bool control, bool shift, string nativePath) {
            //Debug.Log($"InputActionNode.UpdateInputAction() called for action: {actionName} with control={control}, shift={shift}, nativePath='{nativePath}'");

            if (string.IsNullOrEmpty(nativePath)) return;
            if (inputAction == null) return;

            // --- STEP 1: SCAN FOR FLEXIBLE MODIFIER ARCHITECTURE ---
            // Look for our custom "FlexibleModifier" composite header in this action's bindings list
            int flexCompositeIdx = inputAction.bindings.IndexOf(b => b.action == inputAction.name && b.isComposite && b.path.Contains("FlexibleModifier"));

            if (flexCompositeIdx != -1) {
                // --- STRATEGY A: ADVANCED FLEXIBLE MODIFIER LAYOUT ---
                // Offset array positions based on the FlexibleModifier struct blueprint:
                // Index + 1 = Modifier 1, Index + 2 = Modifier 2, Index + 3 = Primary Button

                string mod1Path = "";
                string mod2Path = "";

                // Determine modifier states and assign appropriate Unity device paths
                if (control && shift) {
                    mod1Path = "<Keyboard>/leftCtrl";
                    mod2Path = "<Keyboard>/leftShift";
                } else if (control) {
                    mod1Path = "<Keyboard>/leftCtrl";
                } else if (shift) {
                    mod1Path = "<Keyboard>/leftShift";
                }

                // Apply string path overrides directly onto the pre-existing composite structure
                inputAction.ApplyBindingOverride(flexCompositeIdx + 1, mod1Path);
                inputAction.ApplyBindingOverride(flexCompositeIdx + 2, mod2Path);
                inputAction.ApplyBindingOverride(flexCompositeIdx + 3, nativePath);

                //Debug.Log($"Successfully updated FlexibleModifier for {inputAction.name}: Mod1='{mod1Path}', Mod2='{mod2Path}', Button='{nativePath}'");

                ProcessUpdateAction();
                return;
            }

            // --- STRATEGY B: LEGACY/BACKWARD-COMPATIBLE FALLBACK ---
            // If the action asset doesn't use the custom composite template, run your original cleanup and binding routine safely.

            // Cleanup existing keyboard overrides
            for (int i = inputAction.bindings.Count - 1; i >= 0; i--) {
                InputBinding b = inputAction.bindings[i];
                string activePath = !string.IsNullOrEmpty(b.overridePath) ? b.overridePath : b.path;

                if (activePath.StartsWith("<Keyboard>") || b.path.StartsWith("<Keyboard>")) {
                    if (b.isPartOfComposite) {
                        int checkIndex = i;
                        while (checkIndex >= 0 && !inputAction.bindings[checkIndex].isComposite) {
                            checkIndex--;
                        }
                        if (checkIndex >= 0) {
                            inputAction.RemoveBindingOverride(checkIndex);
                        }
                    } else {
                        inputAction.ApplyBindingOverride(i, "<Keyboard>/none");
                    }
                }
            }

            // Fallback binding mapping layout for basic single-key nodes
            if (!control && !shift) {
                int targetIndex = inputAction.bindings.IndexOf(b => b.path.StartsWith("<Keyboard>") && !b.isPartOfComposite && !b.isComposite);

                if (targetIndex != -1) {
                    inputAction.ApplyBindingOverride(targetIndex, nativePath);
                    //Debug.Log($"Successfully bound fallback standalone key to path: {nativePath}");
                } else {
                    inputAction.AddBinding(nativePath);
                }
            } else {
                // Handle native composite injection fallback
                string compositeType = (control && shift) ? "ButtonWithTwoModifiers" : "ButtonWithOneModifier";
                var compositeBuilder = inputAction.AddCompositeBinding(compositeType);

                if (control && shift) {
                    compositeBuilder.With("modifier1", "<Keyboard>/ctrl")
                                    .With("modifier2", "<Keyboard>/shift")
                                    .With("button", nativePath);
                } else if (control) {
                    compositeBuilder.With("modifier", "<Keyboard>/ctrl")
                                    .With("button", nativePath);
                } else if (shift) {
                    compositeBuilder.With("modifier", "<Keyboard>/shift")
                                    .With("button", nativePath);
                }
                //Debug.Log($"Injected legacy layout fallback composite '{compositeType}' for {inputAction.name}");
            }

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
            //Debug.Log($"InputActionNode.ProcessUpdateAction() called for action: {actionName}");

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