using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    public class KeyBindManager : ConfiguredMonoBehaviour {

        [SerializeField]
        private Dictionary<string, KeyBindNode> keyBinds = new Dictionary<string, KeyBindNode>();

        private string bindName = string.Empty;

        private InputDeviceType inputDeviceType;

        // game manager references
        UIManager uIManager = null;

        public Dictionary<string, KeyBindNode> KeyBinds { get => keyBinds; set => keyBinds = value; }

        private Dictionary<string, KeyCode> xBoxKeys = new Dictionary<string, KeyCode>();
        private Dictionary<string, KeyCode> invectorKeys = new Dictionary<string, KeyCode>();

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

            invectorKeys.Add("JUMP", KeyCode.X);
            invectorKeys.Add("CROUCH", KeyCode.Y);

            xBoxKeys.Add("CANCEL", KeyCode.JoystickButton1);
            xBoxKeys.Add("ACCEPT", KeyCode.JoystickButton0);
            xBoxKeys.Add("MAINMENU", KeyCode.JoystickButton7);
            xBoxKeys.Add("JUMP", KeyCode.JoystickButton3);
            xBoxKeys.Add("CROUCH", KeyCode.JoystickButton2);
            xBoxKeys.Add("JOYSTICKBUTTON0", KeyCode.JoystickButton0);
            xBoxKeys.Add("JOYSTICKBUTTON1", KeyCode.JoystickButton1);
            xBoxKeys.Add("JOYSTICKBUTTON2", KeyCode.JoystickButton2);
            xBoxKeys.Add("JOYSTICKBUTTON3", KeyCode.JoystickButton3);
            xBoxKeys.Add("JOYSTICKBUTTON4", KeyCode.JoystickButton4);
            xBoxKeys.Add("JOYSTICKBUTTON5", KeyCode.JoystickButton5);

            InitializeKey("FORWARD", KeyCode.W, KeyCode.W, KeyCode.W, "Forward", KeyBindType.Normal);
            InitializeKey("BACK", KeyCode.S, KeyCode.S, KeyCode.S, "Backward", KeyBindType.Normal);
            InitializeKey("STRAFELEFT", KeyCode.A, KeyCode.A, KeyCode.A, "Strafe Left", KeyBindType.Normal);
            InitializeKey("STRAFERIGHT", KeyCode.D, KeyCode.D, KeyCode.D, "Strafe Right", KeyBindType.Normal);
            InitializeKey("TURNLEFT", KeyCode.Q, KeyCode.Q, KeyCode.Q, "Turn Left", KeyBindType.Normal);
            InitializeKey("TURNRIGHT", KeyCode.E, KeyCode.E, KeyCode.E, "Turn Right", KeyBindType.Normal);
            InitializeKey("JUMP", KeyCode.Space, xBoxKeys["JUMP"], KeyCode.X, "Jump", KeyBindType.Normal);
            InitializeKey("CROUCH", KeyCode.X, xBoxKeys["CROUCH"], KeyCode.Y, "Crouch", KeyBindType.Normal);
            InitializeKey("ROLL", KeyCode.R, KeyCode.B, KeyCode.B, "Roll", KeyBindType.Normal);
            InitializeKey("TOGGLERUN", KeyCode.KeypadDivide, KeyCode.None, KeyCode.None, "Toggle Run", KeyBindType.Normal);
            InitializeKey("TOGGLEAUTORUN", KeyCode.KeypadMultiply, KeyCode.JoystickButton8, KeyCode.None, "Toggle Autorun", KeyBindType.Normal);
            InitializeKey("TOGGLESTRAFE", KeyCode.T, KeyCode.JoystickButton9, KeyCode.JoystickButton9, "Toggle Strafe", KeyBindType.Normal);

            InitializeKey("ACCEPT", KeyCode.KeypadEnter, xBoxKeys["ACCEPT"], KeyCode.None, "Accept", KeyBindType.Constant);
            InitializeKey("CANCEL", KeyCode.Backspace, KeyCode.None, KeyCode.None, "Cancel", KeyBindType.Constant);
            InitializeKey("CANCELALL", KeyCode.Escape, KeyCode.None, KeyCode.None, "Cancel All", KeyBindType.Constant);
            InitializeKey("MAINMENU", KeyCode.F12, xBoxKeys["MAINMENU"], KeyCode.None, "Main Menu", KeyBindType.Constant);
            InitializeKey("JOYSTICKBUTTON0", KeyCode.None, xBoxKeys["JOYSTICKBUTTON0"], KeyCode.None, "Joystick Button 0", KeyBindType.Constant);
            InitializeKey("JOYSTICKBUTTON1", KeyCode.None, xBoxKeys["JOYSTICKBUTTON1"], KeyCode.None, "Joystick Button 1", KeyBindType.Constant);
            InitializeKey("JOYSTICKBUTTON2", KeyCode.None, xBoxKeys["JOYSTICKBUTTON2"], KeyCode.None, "Joystick Button 2", KeyBindType.Constant);
            InitializeKey("JOYSTICKBUTTON3", KeyCode.None, xBoxKeys["JOYSTICKBUTTON3"], KeyCode.None, "Joystick Button 3", KeyBindType.Constant);
            InitializeKey("JOYSTICKBUTTON4", KeyCode.None, xBoxKeys["JOYSTICKBUTTON4"], KeyCode.None, "Joystick Button 4", KeyBindType.Constant);
            InitializeKey("JOYSTICKBUTTON5", KeyCode.None, xBoxKeys["JOYSTICKBUTTON5"], KeyCode.None, "Joystick Button 5", KeyBindType.Constant);
            InitializeKey("QUESTLOG", KeyCode.L, KeyCode.None, KeyCode.None, "Quest Log", KeyBindType.System);
            InitializeKey("CHARACTERPANEL", KeyCode.C, KeyCode.None, KeyCode.None, "Character Panel", KeyBindType.System);
            InitializeKey("CURRENCYPANEL", KeyCode.I, KeyCode.None, KeyCode.None, "Currency Panel", KeyBindType.System);
            InitializeKey("ABILITYBOOK", KeyCode.P, KeyCode.None, KeyCode.None, "Ability Book", KeyBindType.System);
            InitializeKey("SKILLBOOK", KeyCode.K, KeyCode.None, KeyCode.None, "Skill Book", KeyBindType.System);
            InitializeKey("ACHIEVEMENTBOOK", KeyCode.Y, KeyCode.None, KeyCode.None, "Achievement Book", KeyBindType.System);
            InitializeKey("REPUTATIONBOOK", KeyCode.U, KeyCode.None, KeyCode.None, "Reputation Book", KeyBindType.System);
            InitializeKey("INVENTORY", KeyCode.B, KeyCode.None, KeyCode.None, "Inventory", KeyBindType.System);
            InitializeKey("MAINMAP", KeyCode.M, KeyCode.None, KeyCode.None, "Map", KeyBindType.System);
            InitializeKey("NEXTTARGET", KeyCode.Tab, KeyCode.None, KeyCode.None, "Next Target", KeyBindType.System);
            InitializeKey("UNEQUIPALL", KeyCode.N, KeyCode.None, KeyCode.None, "Unequip All", KeyBindType.System);
            InitializeKey("HIDEUI", KeyCode.Period, KeyCode.None, KeyCode.None, "Hide UI", KeyBindType.System);

            InitializeKey("ACT1", KeyCode.Alpha1, KeyCode.None, KeyCode.None, "Action Button 1", KeyBindType.Action);
            InitializeKey("ACT2", KeyCode.Alpha2, KeyCode.None, KeyCode.None, "Action Button 2", KeyBindType.Action);
            InitializeKey("ACT3", KeyCode.Alpha3, KeyCode.None, KeyCode.None, "Action Button 3", KeyBindType.Action);
            InitializeKey("ACT4", KeyCode.Alpha4, KeyCode.None, KeyCode.None, "Action Button 4", KeyBindType.Action);
            InitializeKey("ACT5", KeyCode.Alpha5, KeyCode.None, KeyCode.None, "Action Button 5", KeyBindType.Action);
            InitializeKey("ACT6", KeyCode.Alpha6, KeyCode.None, KeyCode.None, "Action Button 6", KeyBindType.Action);
            InitializeKey("ACT7", KeyCode.Alpha7, KeyCode.None, KeyCode.None, "Action Button 7", KeyBindType.Action);
            InitializeKey("ACT8", KeyCode.Alpha8, KeyCode.None, KeyCode.None, "Action Button 8", KeyBindType.Action);
            InitializeKey("ACT9", KeyCode.Alpha9, KeyCode.None, KeyCode.None, "Action Button 9", KeyBindType.Action);
            InitializeKey("ACT10", KeyCode.Alpha0, KeyCode.None, KeyCode.None, "Action Button 10", KeyBindType.Action);

            InitializeKey("ACT11", KeyCode.Alpha1, KeyCode.None, KeyCode.None, "Action Button 11", KeyBindType.Action, false, true);
            InitializeKey("ACT12", KeyCode.Alpha2, KeyCode.None, KeyCode.None, "Action Button 12", KeyBindType.Action, false, true);
            InitializeKey("ACT13", KeyCode.Alpha3, KeyCode.None, KeyCode.None, "Action Button 13", KeyBindType.Action, false, true);
            InitializeKey("ACT14", KeyCode.Alpha4, KeyCode.None, KeyCode.None, "Action Button 14", KeyBindType.Action, false, true);
            InitializeKey("ACT15", KeyCode.Alpha5, KeyCode.None, KeyCode.None, "Action Button 15", KeyBindType.Action, false, true);
            InitializeKey("ACT16", KeyCode.Alpha6, KeyCode.None, KeyCode.None, "Action Button 16", KeyBindType.Action, false, true);
            InitializeKey("ACT17", KeyCode.Alpha7, KeyCode.None, KeyCode.None, "Action Button 17", KeyBindType.Action, false, true);
            InitializeKey("ACT18", KeyCode.Alpha8, KeyCode.None, KeyCode.None, "Action Button 18", KeyBindType.Action, false, true);
            InitializeKey("ACT19", KeyCode.Alpha9, KeyCode.None, KeyCode.None, "Action Button 19", KeyBindType.Action, false, true);
            InitializeKey("ACT20", KeyCode.Alpha0, KeyCode.None, KeyCode.None, "Action Button 20", KeyBindType.Action, false, true);
                                                   
            InitializeKey("ACT21", KeyCode.Alpha1, KeyCode.None, KeyCode.None, "Action Button 21", KeyBindType.Action, true);
            InitializeKey("ACT22", KeyCode.Alpha2, KeyCode.None, KeyCode.None, "Action Button 22", KeyBindType.Action, true);
            InitializeKey("ACT23", KeyCode.Alpha3, KeyCode.None, KeyCode.None, "Action Button 23", KeyBindType.Action, true);
            InitializeKey("ACT24", KeyCode.Alpha4, KeyCode.None, KeyCode.None, "Action Button 24", KeyBindType.Action, true);
            InitializeKey("ACT25", KeyCode.Alpha5, KeyCode.None, KeyCode.None, "Action Button 25", KeyBindType.Action, true);
            InitializeKey("ACT26", KeyCode.Alpha6, KeyCode.None, KeyCode.None, "Action Button 26", KeyBindType.Action, true);
            InitializeKey("ACT27", KeyCode.Alpha7, KeyCode.None, KeyCode.None, "Action Button 27", KeyBindType.Action, true);
            InitializeKey("ACT28", KeyCode.Alpha8, KeyCode.None, KeyCode.None, "Action Button 28", KeyBindType.Action, true);
            InitializeKey("ACT29", KeyCode.Alpha9, KeyCode.None, KeyCode.None, "Action Button 29", KeyBindType.Action, true);
            InitializeKey("ACT30", KeyCode.Alpha0, KeyCode.None, KeyCode.None, "Action Button 30", KeyBindType.Action, true);

            InitializeKey("ACT31", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 31", KeyBindType.Action);
            InitializeKey("ACT32", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 32", KeyBindType.Action);
            InitializeKey("ACT33", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 33", KeyBindType.Action);
            InitializeKey("ACT34", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 34", KeyBindType.Action);
            InitializeKey("ACT35", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 35", KeyBindType.Action);
            InitializeKey("ACT36", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 36", KeyBindType.Action);
            InitializeKey("ACT37", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 37", KeyBindType.Action);
            InitializeKey("ACT38", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 38", KeyBindType.Action);
            InitializeKey("ACT39", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 39", KeyBindType.Action);
            InitializeKey("ACT40", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 40", KeyBindType.Action);
                                                 
            InitializeKey("ACT41", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 41", KeyBindType.Action);
            InitializeKey("ACT42", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 42", KeyBindType.Action);
            InitializeKey("ACT43", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 43", KeyBindType.Action);
            InitializeKey("ACT44", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 44", KeyBindType.Action);
            InitializeKey("ACT45", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 45", KeyBindType.Action);
            InitializeKey("ACT46", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 46", KeyBindType.Action);
            InitializeKey("ACT47", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 47", KeyBindType.Action);
            InitializeKey("ACT48", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 48", KeyBindType.Action);
            InitializeKey("ACT49", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 49", KeyBindType.Action);
            InitializeKey("ACT50", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 50", KeyBindType.Action);
                                                 
            InitializeKey("ACT51", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 51", KeyBindType.Action);
            InitializeKey("ACT52", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 52", KeyBindType.Action);
            InitializeKey("ACT53", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 53", KeyBindType.Action);
            InitializeKey("ACT54", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 54", KeyBindType.Action);
            InitializeKey("ACT55", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 55", KeyBindType.Action);
            InitializeKey("ACT56", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 56", KeyBindType.Action);
            InitializeKey("ACT57", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 57", KeyBindType.Action);
            InitializeKey("ACT58", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 58", KeyBindType.Action);
            InitializeKey("ACT59", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 59", KeyBindType.Action);
            InitializeKey("ACT60", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 60", KeyBindType.Action);
                                                 
            InitializeKey("ACT61", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 61", KeyBindType.Action);
            InitializeKey("ACT62", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 62", KeyBindType.Action);
            InitializeKey("ACT63", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 63", KeyBindType.Action);
            InitializeKey("ACT64", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 64", KeyBindType.Action);
            InitializeKey("ACT65", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 65", KeyBindType.Action);
            InitializeKey("ACT66", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 66", KeyBindType.Action);
            InitializeKey("ACT67", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 67", KeyBindType.Action);
            InitializeKey("ACT68", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 68", KeyBindType.Action);
            InitializeKey("ACT69", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 69", KeyBindType.Action);
            InitializeKey("ACT70", KeyCode.None, KeyCode.None, KeyCode.None, "Action Button 70", KeyBindType.Action);
        }

        private void InitializeKey(string key, KeyCode keyCode, KeyCode joystickKeyCode, KeyCode mobileKeyCode, string label, KeyBindType keyBindType, bool control = false, bool shift = false) {
            //Debug.Log("KeyBindManager.InitializeKey(" + key + ", " + control + ")");
            KeyBindNode keyBindNode = new KeyBindNode(key, keyCode, joystickKeyCode, mobileKeyCode, label, keyBindType, control, shift);
            keyBinds.Add(key, keyBindNode);
            EventParamProperties eventParamProperties = new EventParamProperties();
            SimpleParamNode simpleParamNode = new SimpleParamNode();
            simpleParamNode.ParamType = SimpleParamType.stringType;
            simpleParamNode.SimpleParams.StringParam = keyCode.ToString();
            eventParamProperties.objectParam.MySimpleParams.Add(simpleParamNode);
            simpleParamNode = new SimpleParamNode();
            simpleParamNode.ParamType = SimpleParamType.stringType;
            simpleParamNode.SimpleParams.StringParam = joystickKeyCode.ToString();
            eventParamProperties.objectParam.MySimpleParams.Add(simpleParamNode);
            simpleParamNode = new SimpleParamNode();
            simpleParamNode.ParamType = SimpleParamType.stringType;
            simpleParamNode.SimpleParams.StringParam = mobileKeyCode.ToString();
            eventParamProperties.objectParam.MySimpleParams.Add(simpleParamNode);
            SystemEventManager.TriggerEvent("OnBindKey" + key, eventParamProperties);
        }

        public void BindKey(string key, InputDeviceType inputDeviceType, KeyCode keyCode, bool control, bool shift) {
            Debug.Log("KeyBindManager.BindKey(" + key + ", " + keyCode.ToString() + ")");

            // since the key cannot control 2 actions, if it already exists, unbind it from that action
            UnbindKeyCode(keyBinds, inputDeviceType, keyCode, control, shift);

            //keyBinds[key].MyKeyCode = keyCode;
            keyBinds[key].UpdateKeyCode(inputDeviceType, keyCode, control, shift);
            bindName = string.Empty;
            uIManager.keyBindConfirmWindow.CloseWindow();
        }

        private void UnbindKeyCode(Dictionary<string, KeyBindNode> currentDictionary, InputDeviceType inputDeviceType, KeyCode keyCode, bool control, bool shift) {
            //Debug.Log("KeyBindManager.UnbindKeyCode()");
            foreach (KeyBindNode keyBindNode in currentDictionary.Values) {
                if (inputDeviceType == InputDeviceType.Keyboard) {
                    if (keyBindNode.KeyboardKeyCode == keyCode && keyBindNode.Shift == shift && keyBindNode.Control == control) {
                        keyBindNode.KeyboardKeyCode = KeyCode.None;
                        keyBindNode.Shift = false;
                        keyBindNode.Control = false;
                        return;
                    }
                } else if(inputDeviceType == InputDeviceType.Joystick) {
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
            }
        }

        public void SendKeyBindEvents() {
            //Debug.Log("KeyBindManager.UnbindKeyCode()");
            foreach (KeyBindNode keyBindNode in keyBinds.Values) {
                keyBindNode.SendKeyBindEvent();
            }
        }

        /*
        private bool ContainsKeyCode(Dictionary<string, KeyBindNode> currentDictionary, KeyCode keyCode) {
            foreach (KeyBindNode keyBindNode in currentDictionary.Values) {
                if (keyBindNode.MyKeyCode == keyCode) {
                    return true;
                }
            }
            return false;
        }
        */

        public void BeginKeyBind(string key, InputDeviceType inputDeviceType) {
            Debug.Log("KeyBindManager.BeginKeyBind(" + key + ", " + inputDeviceType.ToString() + ")");
            this.bindName = key;
            this.inputDeviceType = inputDeviceType;
            uIManager.keyBindConfirmWindow.OpenWindow();
        }

        private void OnGUI() {
            //Debug.Log("KeyBindManager.OnGUI()");
            if (bindName != string.Empty) {
                //Debug.Log("KeyBindManager.OnGUI(): bindName: " + bindName);
                Event e = Event.current;
                if (e.isKey && e.keyCode != KeyCode.LeftShift && e.keyCode != KeyCode.RightShift && e.keyCode != KeyCode.LeftControl && e.keyCode != KeyCode.RightControl) {
                    //Debug.Log("KeyBindManager.OnGUI(): the bind was a key");
                    BindKey(bindName, inputDeviceType, e.keyCode, e.control, e.shift);
                    return;
                }
                for (int i = 0; i < 20; i++) {
                    if (Input.GetKey("joystick button " + i.ToString())) {
                        //System.Enum.Parse(typeof(KeyCode), "JoystickButton" + i)
                        Debug.Log("joystick button " + i);
                        BindKey(bindName, inputDeviceType, (KeyCode)System.Enum.Parse(typeof(KeyCode), "JoystickButton" + i), false, false);
                        return;
                    }
                }
            }
        }

        public void CancelKeyBind() {
            Debug.Log("KeyBindManager.CancelKeyBind()");
            uIManager.keyBindConfirmWindow.CloseWindow();
            bindName = string.Empty;
        }

    }

    public enum KeyBindType { Normal, Action, Constant, System }
}