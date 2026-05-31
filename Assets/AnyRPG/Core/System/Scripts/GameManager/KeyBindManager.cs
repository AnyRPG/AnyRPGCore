using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace AnyRPG {
    public class KeyBindManager : ConfiguredClass {

        [SerializeField]
        private Dictionary<string, KeyBindNode> keyBinds = new Dictionary<string, KeyBindNode>();

        private string bindName = string.Empty;

        // Store the active subscription token
        private IDisposable rebindToken;

        private InputDeviceType inputDeviceType;

        // game manager references
        UIManager uIManager = null;

        public Dictionary<string, KeyBindNode> KeyBinds { get => keyBinds; set => keyBinds = value; }

        private Dictionary<string, GamepadButton> xBoxKeys = new Dictionary<string, GamepadButton>();

        public string BindName { get => bindName; set => bindName = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            InitializeKeys();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
        }

        private void InitializeKeys() {
            //Debug.Log("KeyBindManager.InitializeKeys()");

            xBoxKeys.Add("CANCEL", GamepadButton.B);
            xBoxKeys.Add("ACCEPT", GamepadButton.A);
            xBoxKeys.Add("MAINMENU", GamepadButton.Start);
            xBoxKeys.Add("JUMP", GamepadButton.X);
            xBoxKeys.Add("CROUCH", GamepadButton.Y);
            xBoxKeys.Add("GAMEPADBUTTONA", GamepadButton.A); // A
            xBoxKeys.Add("GAMEPADBUTTONB", GamepadButton.B); // B
            xBoxKeys.Add("GAMEPADBUTTONX", GamepadButton.X); // X
            xBoxKeys.Add("GAMEPADBUTTONY", GamepadButton.Y); // Y
            xBoxKeys.Add("GAMEPADBUTTONLEFTSHOULDER", GamepadButton.LeftShoulder); // LB
            xBoxKeys.Add("GAMEPADBUTTONRIGHTSHOULDER", GamepadButton.RightShoulder); // RB
            xBoxKeys.Add("GAMEPADBUTTONSELECT", GamepadButton.Select); // Window
            xBoxKeys.Add("GAMEPADBUTTONSTART", GamepadButton.Start); // Menu
            xBoxKeys.Add("GAMEPADBUTTONLEFTSTICK", GamepadButton.LeftStick); // Left Analog
            xBoxKeys.Add("GAMEPADBUTTONRIGHTSTICK", GamepadButton.RightStick); // Right Analog

            InitializeKey("FORWARD", Key.W, KeyBindNode.GamepadNone, "Forward", KeyBindType.Normal);
            InitializeKey("BACK", Key.S, KeyBindNode.GamepadNone, "Backward", KeyBindType.Normal);
            InitializeKey("STRAFELEFT", Key.A, KeyBindNode.GamepadNone, "Strafe Left", KeyBindType.Normal);
            InitializeKey("STRAFERIGHT", Key.D, KeyBindNode.GamepadNone, "Strafe Right", KeyBindType.Normal);
            InitializeKey("TURNLEFT", Key.Q, KeyBindNode.GamepadNone, "Turn Left", KeyBindType.Normal);
            InitializeKey("TURNRIGHT", Key.E, KeyBindNode.GamepadNone, "Turn Right", KeyBindType.Normal);
            InitializeKey("JUMP", Key.Space, xBoxKeys["JUMP"], "Jump", KeyBindType.Normal);
            InitializeKey("CROUCH", Key.X, xBoxKeys["CROUCH"], "Crouch", KeyBindType.Normal);
            InitializeKey("ROLL", Key.R, KeyBindNode.GamepadNone, "Roll", KeyBindType.Normal);
            InitializeKey("TOGGLERUN", Key.NumpadDivide, KeyBindNode.GamepadNone, "Toggle Run", KeyBindType.Normal);
            InitializeKey("TOGGLEAUTORUN", Key.NumpadMultiply, KeyBindNode.GamepadNone, "Toggle Autorun", KeyBindType.Normal);
            InitializeKey("TOGGLESTRAFE", Key.T, GamepadButton.RightStick, "Toggle Strafe", KeyBindType.Normal);
            InitializeKey("TOGGLEMOUSELOOK", Key.V, KeyBindNode.GamepadNone, "Toggle Mouse Look", KeyBindType.Normal);

            InitializeKey("ACCEPT", Key.NumpadEnter, xBoxKeys["ACCEPT"], "Accept", KeyBindType.Constant);
            //InitializeKey("CANCEL", KeyCode.Backspace, KeyCode.None, KeyCode.None, "Cancel", KeyBindType.Constant);
            InitializeKey("CANCELALL", Key.Escape, KeyBindNode.GamepadNone, "Cancel All", KeyBindType.Constant);
            InitializeKey("MAINMENU", Key.F12, xBoxKeys["MAINMENU"], "Main Menu", KeyBindType.Constant);
            InitializeKey("BEGINCHATCOMMAND", Key.Slash, KeyBindNode.GamepadNone, "Chat Command", KeyBindType.Constant);

            InitializeKey("GAMEPADBUTTONA", Key.None, xBoxKeys["GAMEPADBUTTONA"], "Gamepad Button A", KeyBindType.Hidden);
            InitializeKey("GAMEPADBUTTONB", Key.None, xBoxKeys["GAMEPADBUTTONB"], "Gamepad Button B", KeyBindType.Hidden);
            InitializeKey("GAMEPADBUTTONX", Key.None, xBoxKeys["GAMEPADBUTTONX"], "Gamepad Button X", KeyBindType.Hidden);
            InitializeKey("GAMEPADBUTTONY", Key.None, xBoxKeys["GAMEPADBUTTONY"], "Gamepad Button Y", KeyBindType.Hidden);
            InitializeKey("GAMEPADBUTTONLEFTSHOULDER", Key.None, xBoxKeys["GAMEPADBUTTONLEFTSHOULDER"], "Gamepad Button Left Shoulder", KeyBindType.Hidden);
            InitializeKey("GAMEPADBUTTONRIGHTSHOULDER", Key.None, xBoxKeys["GAMEPADBUTTONRIGHTSHOULDER"], "Gamepad Button Right Shoulder", KeyBindType.Hidden);
            InitializeKey("GAMEPADBUTTONSELECT", Key.None, xBoxKeys["GAMEPADBUTTONSELECT"], "Gamepad Button Select", KeyBindType.Hidden);
            InitializeKey("GAMEPADBUTTONSTART", Key.None, xBoxKeys["GAMEPADBUTTONSTART"], "Gamepad Button Start", KeyBindType.Hidden);
            InitializeKey("GAMEPADBUTTONLEFTSTICK", Key.None, xBoxKeys["GAMEPADBUTTONLEFTSTICK"], "Gamepad Button Left Stick", KeyBindType.Hidden);
            InitializeKey("GAMEPADBUTTONRIGHTSTICK", Key.None, xBoxKeys["GAMEPADBUTTONRIGHTSTICK"], "Gamepad Button Right Stick", KeyBindType.Hidden);

            InitializeKey("QUESTLOG", Key.L, KeyBindNode.GamepadNone, "Quest Log", KeyBindType.System);
            InitializeKey("CHARACTERPANEL", Key.C, KeyBindNode.GamepadNone, "Character Panel", KeyBindType.System);
            InitializeKey("CURRENCYPANEL", Key.I, KeyBindNode.GamepadNone, "Currency Panel", KeyBindType.System);
            InitializeKey("ABILITYBOOK", Key.P, KeyBindNode.GamepadNone, "Ability Book", KeyBindType.System);
            InitializeKey("SKILLBOOK", Key.K, KeyBindNode.GamepadNone, "Skill Book", KeyBindType.System);
            InitializeKey("ACHIEVEMENTBOOK", Key.Y, KeyBindNode.GamepadNone, "Achievement Book", KeyBindType.System);
            InitializeKey("REPUTATIONBOOK", Key.U, KeyBindNode.GamepadNone, "Reputation Book", KeyBindType.System);
            InitializeKey("INVENTORY", Key.B, KeyBindNode.GamepadNone, "Inventory", KeyBindType.System);
            InitializeKey("SOCIAL", Key.O, KeyBindNode.GamepadNone, "Social", KeyBindType.System);
            InitializeKey("MAINMAP", Key.M, KeyBindNode.GamepadNone, "Map", KeyBindType.System);
            InitializeKey("NEXTTARGET", Key.Tab, KeyBindNode.GamepadNone, "Next Target", KeyBindType.System);
            InitializeKey("UNEQUIPALL", Key.N, KeyBindNode.GamepadNone, "Unequip All", KeyBindType.System);
            InitializeKey("HIDEUI", Key.Period, KeyBindNode.GamepadNone, "Hide UI", KeyBindType.System);

            InitializeKey("ACT1", Key.Digit1, KeyBindNode.GamepadNone, "Action Button 1", KeyBindType.Action);
            InitializeKey("ACT2", Key.Digit2, KeyBindNode.GamepadNone, "Action Button 2", KeyBindType.Action);
            InitializeKey("ACT3", Key.Digit3, KeyBindNode.GamepadNone, "Action Button 3", KeyBindType.Action);
            InitializeKey("ACT4", Key.Digit4, KeyBindNode.GamepadNone, "Action Button 4", KeyBindType.Action);
            InitializeKey("ACT5", Key.Digit5, KeyBindNode.GamepadNone, "Action Button 5", KeyBindType.Action);
            InitializeKey("ACT6", Key.Digit6, KeyBindNode.GamepadNone, "Action Button 6", KeyBindType.Action);
            InitializeKey("ACT7", Key.Digit7, KeyBindNode.GamepadNone, "Action Button 7", KeyBindType.Action);
            InitializeKey("ACT8", Key.Digit8, KeyBindNode.GamepadNone, "Action Button 8", KeyBindType.Action);
            InitializeKey("ACT9", Key.Digit9, KeyBindNode.GamepadNone, "Action Button 9", KeyBindType.Action);
            InitializeKey("ACT10", Key.Digit0, KeyBindNode.GamepadNone, "Action Button 10", KeyBindType.Action);
            InitializeKey("ACT11", Key.Minus, KeyBindNode.GamepadNone, "Action Button 11", KeyBindType.Action);
            InitializeKey("ACT12", Key.Equals, KeyBindNode.GamepadNone, "Action Button 12", KeyBindType.Action);

            InitializeKey("ACT13", Key.Digit1, KeyBindNode.GamepadNone, "Action Button 13", KeyBindType.Action, false, true);
            InitializeKey("ACT14", Key.Digit2, KeyBindNode.GamepadNone, "Action Button 14", KeyBindType.Action, false, true);
            InitializeKey("ACT15", Key.Digit3, KeyBindNode.GamepadNone, "Action Button 15", KeyBindType.Action, false, true);
            InitializeKey("ACT16", Key.Digit4, KeyBindNode.GamepadNone, "Action Button 16", KeyBindType.Action, false, true);
            InitializeKey("ACT17", Key.Digit5, KeyBindNode.GamepadNone, "Action Button 17", KeyBindType.Action, false, true);
            InitializeKey("ACT18", Key.Digit6, KeyBindNode.GamepadNone, "Action Button 18", KeyBindType.Action, false, true);
            InitializeKey("ACT19", Key.Digit7, KeyBindNode.GamepadNone, "Action Button 19", KeyBindType.Action, false, true);
            InitializeKey("ACT20", Key.Digit8, KeyBindNode.GamepadNone, "Action Button 20", KeyBindType.Action, false, true);
            InitializeKey("ACT21", Key.Digit9, KeyBindNode.GamepadNone, "Action Button 21", KeyBindType.Action, false, true);
            InitializeKey("ACT22", Key.Digit0, KeyBindNode.GamepadNone, "Action Button 22", KeyBindType.Action, false, true);
            InitializeKey("ACT23", Key.Minus, KeyBindNode.GamepadNone, "Action Button 23", KeyBindType.Action, false, true);
            InitializeKey("ACT24", Key.Equals, KeyBindNode.GamepadNone, "Action Button 24", KeyBindType.Action, false, true);
            InitializeKey("ACT25", Key.Digit1, KeyBindNode.GamepadNone, "Action Button 25", KeyBindType.Action, true);
            InitializeKey("ACT26", Key.Digit2, KeyBindNode.GamepadNone, "Action Button 26", KeyBindType.Action, true);
            InitializeKey("ACT27", Key.Digit3, KeyBindNode.GamepadNone, "Action Button 27", KeyBindType.Action, true);
            InitializeKey("ACT28", Key.Digit4, KeyBindNode.GamepadNone, "Action Button 28", KeyBindType.Action, true);
            InitializeKey("ACT29", Key.Digit5, KeyBindNode.GamepadNone, "Action Button 29", KeyBindType.Action, true);
            InitializeKey("ACT30", Key.Digit6, KeyBindNode.GamepadNone, "Action Button 30", KeyBindType.Action, true);
            InitializeKey("ACT31", Key.Digit7, KeyBindNode.GamepadNone, "Action Button 31", KeyBindType.Action, true);
            InitializeKey("ACT32", Key.Digit8, KeyBindNode.GamepadNone, "Action Button 32", KeyBindType.Action, true);
            InitializeKey("ACT33", Key.Digit9, KeyBindNode.GamepadNone, "Action Button 33", KeyBindType.Action, true);
            InitializeKey("ACT34", Key.Digit0, KeyBindNode.GamepadNone, "Action Button 34", KeyBindType.Action, true);
            InitializeKey("ACT35", Key.Minus, KeyBindNode.GamepadNone, "Action Button 35", KeyBindType.Action, true);
            InitializeKey("ACT36", Key.Equals, KeyBindNode.GamepadNone, "Action Button 36", KeyBindType.Action, true);

            InitializeKey("ACT37", Key.None, KeyBindNode.GamepadNone, "Action Button 37", KeyBindType.Action);
            InitializeKey("ACT38", Key.None, KeyBindNode.GamepadNone, "Action Button 38", KeyBindType.Action);
            InitializeKey("ACT39", Key.None, KeyBindNode.GamepadNone, "Action Button 39", KeyBindType.Action);
            InitializeKey("ACT40", Key.None, KeyBindNode.GamepadNone, "Action Button 40", KeyBindType.Action);
            InitializeKey("ACT41", Key.None, KeyBindNode.GamepadNone, "Action Button 41", KeyBindType.Action);
            InitializeKey("ACT42", Key.None, KeyBindNode.GamepadNone, "Action Button 42", KeyBindType.Action);
            InitializeKey("ACT43", Key.None, KeyBindNode.GamepadNone, "Action Button 43", KeyBindType.Action);
            InitializeKey("ACT44", Key.None, KeyBindNode.GamepadNone, "Action Button 44", KeyBindType.Action);
            InitializeKey("ACT45", Key.None, KeyBindNode.GamepadNone, "Action Button 45", KeyBindType.Action);
            InitializeKey("ACT46", Key.None, KeyBindNode.GamepadNone, "Action Button 46", KeyBindType.Action);
            InitializeKey("ACT47", Key.None, KeyBindNode.GamepadNone, "Action Button 47", KeyBindType.Action);
            InitializeKey("ACT48", Key.None, KeyBindNode.GamepadNone, "Action Button 48", KeyBindType.Action);

            InitializeKey("ACT49", Key.None, KeyBindNode.GamepadNone, "Action Button 49", KeyBindType.Action);
            InitializeKey("ACT50", Key.None, KeyBindNode.GamepadNone, "Action Button 50", KeyBindType.Action);
            InitializeKey("ACT51", Key.None, KeyBindNode.GamepadNone, "Action Button 51", KeyBindType.Action);
            InitializeKey("ACT52", Key.None, KeyBindNode.GamepadNone, "Action Button 52", KeyBindType.Action);
            InitializeKey("ACT53", Key.None, KeyBindNode.GamepadNone, "Action Button 53", KeyBindType.Action);
            InitializeKey("ACT54", Key.None, KeyBindNode.GamepadNone, "Action Button 54", KeyBindType.Action);
            InitializeKey("ACT55", Key.None, KeyBindNode.GamepadNone, "Action Button 55", KeyBindType.Action);
            InitializeKey("ACT56", Key.None, KeyBindNode.GamepadNone, "Action Button 56", KeyBindType.Action);
            InitializeKey("ACT57", Key.None, KeyBindNode.GamepadNone, "Action Button 57", KeyBindType.Action);
            InitializeKey("ACT58", Key.None, KeyBindNode.GamepadNone, "Action Button 58", KeyBindType.Action);
            InitializeKey("ACT59", Key.None, KeyBindNode.GamepadNone, "Action Button 59", KeyBindType.Action);
            InitializeKey("ACT60", Key.None, KeyBindNode.GamepadNone, "Action Button 60", KeyBindType.Action);
                                                 
            InitializeKey("ACT61", Key.None, KeyBindNode.GamepadNone, "Action Button 61", KeyBindType.Action);
            InitializeKey("ACT62", Key.None, KeyBindNode.GamepadNone, "Action Button 62", KeyBindType.Action);
            InitializeKey("ACT63", Key.None, KeyBindNode.GamepadNone, "Action Button 63", KeyBindType.Action);
            InitializeKey("ACT64", Key.None, KeyBindNode.GamepadNone, "Action Button 64", KeyBindType.Action);
            InitializeKey("ACT65", Key.None, KeyBindNode.GamepadNone, "Action Button 65", KeyBindType.Action);
            InitializeKey("ACT66", Key.None, KeyBindNode.GamepadNone, "Action Button 66", KeyBindType.Action);
            InitializeKey("ACT67", Key.None, KeyBindNode.GamepadNone, "Action Button 67", KeyBindType.Action);
            InitializeKey("ACT68", Key.None, KeyBindNode.GamepadNone, "Action Button 68", KeyBindType.Action);
            InitializeKey("ACT69", Key.None, KeyBindNode.GamepadNone, "Action Button 69", KeyBindType.Action);
            InitializeKey("ACT70", Key.None, KeyBindNode.GamepadNone, "Action Button 70", KeyBindType.Action);
            InitializeKey("ACT71", Key.None, KeyBindNode.GamepadNone, "Action Button 71", KeyBindType.Action);
            InitializeKey("ACT72", Key.None, KeyBindNode.GamepadNone, "Action Button 72", KeyBindType.Action);

            InitializeKey("ACT73", Key.None, KeyBindNode.GamepadNone, "Action Button 73", KeyBindType.Action);
            InitializeKey("ACT74", Key.None, KeyBindNode.GamepadNone, "Action Button 74", KeyBindType.Action);
            InitializeKey("ACT75", Key.None, KeyBindNode.GamepadNone, "Action Button 75", KeyBindType.Action);
            InitializeKey("ACT76", Key.None, KeyBindNode.GamepadNone, "Action Button 76", KeyBindType.Action);
            InitializeKey("ACT77", Key.None, KeyBindNode.GamepadNone, "Action Button 77", KeyBindType.Action);
            InitializeKey("ACT78", Key.None, KeyBindNode.GamepadNone, "Action Button 78", KeyBindType.Action);
            InitializeKey("ACT79", Key.None, KeyBindNode.GamepadNone, "Action Button 79", KeyBindType.Action);
            InitializeKey("ACT80", Key.None, KeyBindNode.GamepadNone, "Action Button 80", KeyBindType.Action);
            InitializeKey("ACT81", Key.None, KeyBindNode.GamepadNone, "Action Button 81", KeyBindType.Action);
            InitializeKey("ACT82", Key.None, KeyBindNode.GamepadNone, "Action Button 82", KeyBindType.Action);
            InitializeKey("ACT83", Key.None, KeyBindNode.GamepadNone, "Action Button 83", KeyBindType.Action);
            InitializeKey("ACT84", Key.None, KeyBindNode.GamepadNone, "Action Button 84", KeyBindType.Action);
        }

        private void InitializeKey(string key, Key KeyboardKey, GamepadButton gamepadButton, /*KeyCode mobileKeyCode,*/ string label, KeyBindType keyBindType, bool control = false, bool shift = false) {
            KeyBindNode keyBindNode = new KeyBindNode(key, KeyboardKey, gamepadButton, /*mobileKeyCode,*/ label, keyBindType, control, shift);
            keyBinds.Add(key, keyBindNode);
        }

        public void BindKey(string key, InputDeviceType inputDeviceType, Key keyboardKey, bool control, bool shift) {
            
            // since the key cannot control 2 actions, if it already exists, unbind it from that action
            UnbindKeyCode(keyBinds, inputDeviceType, keyboardKey, control, shift);

            keyBinds[key].UpdateKeyCode(inputDeviceType, keyboardKey, KeyBindNode.GamepadNone, control, shift);
            bindName = string.Empty;
            uIManager.keyBindConfirmWindow.CloseWindow();
            StopListeningForRebind();
        }

        private void UnbindKeyCode(Dictionary<string, KeyBindNode> currentDictionary, InputDeviceType inputDeviceType, Key keyboardKey, bool control, bool shift) {
            //Debug.Log("KeyBindManager.UnbindKeyCode()");
            foreach (KeyBindNode keyBindNode in currentDictionary.Values) {
                if (inputDeviceType == InputDeviceType.Keyboard) {
                    if (keyBindNode.KeyboardKey == keyboardKey && keyBindNode.Shift == shift && keyBindNode.Control == control) {
                        keyBindNode.KeyboardKey = Key.None;
                        keyBindNode.Shift = false;
                        keyBindNode.Control = false;
                        return;
                    }
                }/* else if(inputDeviceType == InputDeviceType.Joystick) {
                    if (keyBindNode.JoystickKeyCode == keyCode) {
                        keyBindNode.JoystickKeyCode = KeyCode.None;
                        return;
                    }

                } else if (inputDeviceType == InputDeviceType.Mobile) {
                    if (keyBindNode.MobileKeyCode == keyCode) {
                        keyBindNode.MobileKeyCode = KeyCode.None;
                        return;
                    }

                }
                */
            }
        }

        public void BeginKeyBind(string key, InputDeviceType inputDeviceType) {
            //Debug.Log("KeyBindManager.BeginKeyBind(" + key + ", " + inputDeviceType.ToString() + ")");
            this.bindName = key;
            this.inputDeviceType = inputDeviceType;
            uIManager.keyBindConfirmWindow.OpenWindow();
            StartListeningForRebind();
        }

        public void StartListeningForRebind() {
            if (string.IsNullOrEmpty(bindName)) return;

            // Clean safeguard: Always ensure any active token is disposed first
            StopListeningForRebind();

            // Subscribe natively using .Call() and capture the disposable token structure
            rebindToken = InputSystem.onAnyButtonPress.Call(OnInputCaptured);
        }

        private void OnInputCaptured(InputControl control) {
            // 1. Cast the generic control into a ButtonControl to access frame state properties
            if (control is ButtonControl buttonControl) {

                // Use .wasPressedThisFrame safely now that the compiler knows it is a button
                if (!buttonControl.wasPressedThisFrame) return;

                // --- CASE A: KEYBOARD PRESS ---
                if (control is KeyControl keyControl) {
                    Key pressedKey = keyControl.keyCode;

                    if (pressedKey != Key.None &&
                        pressedKey != Key.LeftShift && pressedKey != Key.RightShift &&
                        pressedKey != Key.LeftCtrl && pressedKey != Key.RightCtrl) {

                        StopListeningForRebind();

                        bool isCtrlHeld = Keyboard.current.ctrlKey.isPressed;
                        bool isShiftHeld = Keyboard.current.shiftKey.isPressed;

                        BindKey(bindName, InputDeviceType.Keyboard, pressedKey, isCtrlHeld, isShiftHeld);
                    }
                    return;
                }

                // --- CASE B: GAMEPAD/JOYSTICK PRESS ---
                /*
                if (inputDeviceType == InputDeviceType.Joystick && control.device is Gamepad) {
                    // Get the structural hardware identifier name (e.g., "buttonSouth", "leftShoulder")
                    string controlName = buttonControl.name;

                    if (System.Enum.TryParse(controlName, true, out GamepadButton pressedButton)) {

                        StopListeningForRebind();

                        BindKey(bindName, inputDeviceType, Key.None, pressedButton, false, false);
                    }
                }
                */
            }
        }


        public void StopListeningForRebind() {
            // If an active token exists, disposing it unhooks the InputSystem listener instantly
            if (rebindToken != null) {
                rebindToken.Dispose();
                rebindToken = null;
            }
        }

        // Safety clear to prevent memory leaks if the window disappears or player quits mid-rebind
        private void OnDisable() {
            StopListeningForRebind();
        }

        /*
        private void OnGUI() {
            //Debug.Log("KeyBindManager.OnGUI()");
            if (bindName != string.Empty) {
                //Debug.Log("KeyBindManager.OnGUI(): bindName: " + bindName);
                Event e = Event.current;

                if (e != null && e.isKey && e.key != KeyCode.None) {

                    // Use Unity's native Keyboard layout tool to convert the event's raw code 
                    // directly into the modern Key enum. This seamlessly handles Alpha -> Digit renames.
                    Key modernKey = Keyboard.current != null
                        ? Keyboard.current.FindKeyOnCurrentLayout(e.keyCode.ToString())
                        : Key.None;

                    // Fallback: If layout mapping fails for a specific hardware setup, fallback to safe manual handling
                    if (modernKey == Key.None) {
                        string keyName = e.keyCode.ToString();
                        if (keyName.StartsWith("Alpha")) keyName = keyName.Replace("Alpha", "Digit");
                        if (keyName == "LeftControl") keyName = "LeftCtrl";
                        if (keyName == "RightControl") keyName = "RightCtrl";

                        System.Enum.TryParse(keyName, true, out modernKey);
                    }

                    // Apply filtering logic using the verified modern Key enum
                    if (modernKey != Key.None &&
                        modernKey != Key.LeftShift && modernKey != Key.RightShift &&
                        modernKey != Key.LeftCtrl && modernKey != Key.RightCtrl) {

                        BindKey(bindName, inputDeviceType, modernKey, e.control, e.shift);
                        return;
                    }
                }
                / *
                if (e.isKey && e.keyCode != KeyCode.LeftShift && e.keyCode != KeyCode.RightShift && e.keyCode != KeyCode.LeftControl && e.keyCode != KeyCode.RightControl) {
                    //Debug.Log("KeyBindManager.OnGUI(): the bind was a key");
                    BindKey(bindName, inputDeviceType, e.keyCode, e.control, e.shift);
                    return;
                }
                * /
            }
        }
    */

        public void CancelKeyBind() {
            //Debug.Log("KeyBindManager.CancelKeyBind()");

            uIManager.keyBindConfirmWindow.CloseWindow();
            bindName = string.Empty;
            StopListeningForRebind();
        }

    }

    public enum KeyBindType { Normal, Action, Constant, System, Hidden }
}