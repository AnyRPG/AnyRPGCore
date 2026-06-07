using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace AnyRPG {
    public class KeyBindManager : ConfiguredClass {

        private string bindName = string.Empty;

        // Store the active subscription token
        private IDisposable rebindToken;

        private InputDeviceType inputDeviceType;

        // game manager references
        private UIManager uIManager = null;
        private InputManager inputManager = null;

        public static bool SkipInputProcessingThisFrame { get; private set; }

        public string BindName { get => bindName; set => bindName = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            //InitializeKeys();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            inputManager = systemGameManager.InputManager;
        }

        public void BeginKeyBind(string key, InputDeviceType inputDeviceType) {
            //Debug.Log($"KeyBindManager.BeginKeyBind({key}, {inputDeviceType})");

            this.bindName = key;
            this.inputDeviceType = inputDeviceType;
            uIManager.keyBindConfirmWindow.OpenWindow();
            StartListeningForRebind();
        }

        public void StartListeningForRebind() {
            //Debug.Log("KeyBindManager.StartListeningForRebind()");

            if (string.IsNullOrEmpty(bindName)) return;

            // Clean safeguard: Always ensure any active token is disposed first
            StopListeningForRebind();

            // Subscribe natively using .Call() and capture the disposable token structure
            rebindToken = InputSystem.onAnyButtonPress.Call(OnInputCaptured);
        }

        private void OnInputCaptured(InputControl control) {
            //Debug.Log($"KeyBindManager.OnInputCaptured(): Event fired for control: {control.name}");

            // --- CASE A: KEYBOARD PRESS ---
            if (control is KeyControl keyControl) {
                Key pressedKey = keyControl.keyCode;

                // Strictly ignore Escape and other modifier keys
                if (pressedKey == Key.Escape) {
                    EndBindCleanup();
                    systemGameManager.StartCoroutine(ClearInputBlockRoutine());
                    return;
                }

                if (pressedKey != Key.None &&
                    pressedKey != Key.LeftShift && pressedKey != Key.RightShift &&
                    pressedKey != Key.LeftCtrl && pressedKey != Key.RightCtrl) {

                    StopListeningForRebind();

                    bool isCtrlHeld = Keyboard.current.ctrlKey.isPressed;
                    bool isShiftHeld = Keyboard.current.shiftKey.isPressed;

                    // NATIVE PATH SHORTCUT: control.path gives you exactly what Unity wants (e.g., "<Keyboard>/equals")
                    //string nativePath = control.path;
                    string nativePath = $"<Keyboard>/{control.name}";

                    // Update your BindKey signature to accept this clean nativePath string
                    BindInputAction(bindName, InputDeviceType.Keyboard, isCtrlHeld, isShiftHeld, nativePath);
                    systemGameManager.StartCoroutine(ClearInputBlockRoutine());
                }
                return;
            }

            // --- CASE B: GAMEPAD/JOYSTICK PRESS ---
            if (inputDeviceType == InputDeviceType.Joystick && control.device is Gamepad) {
                string controlName = control.name;

                if (System.Enum.TryParse(controlName, true, out GamepadButton pressedButton)) {
                    StopListeningForRebind();

                    // NATIVE PATH SHORTCUT: Automatically gives "<Gamepad>/buttonEast", "<Gamepad>/leftShoulder", etc.
                    string nativePath = control.path;

                    BindInputAction(bindName, inputDeviceType, false, false, nativePath);
                }
            }
        }

        private IEnumerator ClearInputBlockRoutine() {
            SkipInputProcessingThisFrame = true;

            // Wait until the current frame (1299) is completely finished processing
            yield return new WaitForEndOfFrame();

            SkipInputProcessingThisFrame = false;
        }

        public void BindInputAction(string actionName, InputDeviceType inputDeviceType, bool control, bool shift, string nativePath) {
            //Debug.Log($"KeyBindManager.BindKey(): Binding key: {actionName} with inputDeviceType: {inputDeviceType}, control: {control}, shift: {shift}, nativePath: {nativePath}");

            // since the key cannot control 2 actions, if it already exists, unbind it from that action
            inputManager.UnbindInputAction(actionName, control, shift, nativePath);

            inputManager.UpdateInputAction(actionName, control, shift, nativePath);
            EndBindCleanup();
        }

        private void EndBindCleanup() {
            bindName = string.Empty;
            uIManager.keyBindConfirmWindow.CloseWindow();
            StopListeningForRebind();
        }

        public void StopListeningForRebind() {
            //Debug.Log($"KeyBindManager.StopListeningForRebind() frame: {Time.frameCount}");

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

        public void CancelKeyBind() {
            //Debug.Log("KeyBindManager.CancelKeyBind()");

            uIManager.keyBindConfirmWindow.CloseWindow();
            bindName = string.Empty;
            StopListeningForRebind();
        }

    }

    public enum KeyBindType { Normal, Action, Constant, System, Hidden }
}