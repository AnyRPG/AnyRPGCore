using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KeyBindManager : MonoBehaviour {

    #region Singleton
    private static KeyBindManager instance;

    public static KeyBindManager MyInstance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<KeyBindManager>();
            }

            return instance;
        }
    }

    #endregion

    [SerializeField]
    private Dictionary<string, KeyBindNode> keyBinds = new Dictionary<string, KeyBindNode>();

    private string bindName = string.Empty;

    public Dictionary<string, KeyBindNode> MyKeyBinds { get => keyBinds; set => keyBinds = value; }

    public string MyBindName { get => bindName; set => bindName = value; }

    // Start is called before the first frame update
    void Awake() {
        //Debug.Log("KeyBindManager.Awake()");
        InitializeKeys();
    }

    private void InitializeKeys() {
        //Debug.Log("KeyBindManager.InitializeKeys()");

        InitializeKey("FORWARD", KeyCode.W, "Forward", KeyBindType.Normal);
        InitializeKey("BACK", KeyCode.S, "Backward", KeyBindType.Normal);
        InitializeKey("STRAFELEFT", KeyCode.A, "Strafe Left", KeyBindType.Normal);
        InitializeKey("STRAFERIGHT", KeyCode.D, "Strafe Right", KeyBindType.Normal);
        InitializeKey("TURNLEFT", KeyCode.Q, "Turn Left", KeyBindType.Normal);
        InitializeKey("TURNRIGHT", KeyCode.E, "Turn Right", KeyBindType.Normal);
        InitializeKey("JUMP", KeyCode.Space, "Jump", KeyBindType.Normal);
        InitializeKey("TOGGLERUN", KeyCode.KeypadDivide, "Toggle Run", KeyBindType.Normal);

        InitializeKey("CANCEL", KeyCode.Escape, "Cancel", KeyBindType.Constant);
        InitializeKey("MAINMENU", KeyCode.F12, "Main Menu", KeyBindType.Constant);
        InitializeKey("QUESTLOG", KeyCode.L, "Quest Log", KeyBindType.System);
        InitializeKey("CHARACTERPANEL", KeyCode.C, "Character Panel", KeyBindType.System);
        InitializeKey("CURRENCYPANEL", KeyCode.I, "Currency Panel", KeyBindType.System);
        InitializeKey("ABILITYBOOK", KeyCode.P, "Ability Book", KeyBindType.System);
        InitializeKey("SKILLBOOK", KeyCode.K, "Skill Book", KeyBindType.System);
        InitializeKey("ACHIEVEMENTBOOK", KeyCode.Y, "Achievement Book", KeyBindType.System);
        InitializeKey("REPUTATIONBOOK", KeyCode.U, "Reputation Book", KeyBindType.System);
        InitializeKey("INVENTORY", KeyCode.B, "Inventory", KeyBindType.System);
        InitializeKey("MAINMAP", KeyCode.M, "Map", KeyBindType.System);
        InitializeKey("NEXTTARGET", KeyCode.Tab, "Next Target", KeyBindType.System);
        InitializeKey("UNEQUIPALL", KeyCode.N, "Unequip All", KeyBindType.System);
        InitializeKey("HIDEUI", KeyCode.Period, "Hide UI", KeyBindType.System);

        InitializeKey("ACT1", KeyCode.Alpha1, "Action Button 1", KeyBindType.Action);
        InitializeKey("ACT2", KeyCode.Alpha2, "Action Button 2", KeyBindType.Action);
        InitializeKey("ACT3", KeyCode.Alpha3, "Action Button 3", KeyBindType.Action);
        InitializeKey("ACT4", KeyCode.Alpha4, "Action Button 4", KeyBindType.Action);
        InitializeKey("ACT5", KeyCode.Alpha5, "Action Button 5", KeyBindType.Action);
        InitializeKey("ACT6", KeyCode.Alpha6, "Action Button 6", KeyBindType.Action);
        InitializeKey("ACT7", KeyCode.Alpha7, "Action Button 7", KeyBindType.Action);
        InitializeKey("ACT8", KeyCode.Alpha8, "Action Button 8", KeyBindType.Action);
        InitializeKey("ACT9", KeyCode.Alpha9, "Action Button 9", KeyBindType.Action);
        InitializeKey("ACT10", KeyCode.Alpha0, "Action Button 10", KeyBindType.Action);

        InitializeKey("ACT11", KeyCode.Alpha1, "Action Button 11", KeyBindType.Action, false, true);
        InitializeKey("ACT12", KeyCode.Alpha2, "Action Button 12", KeyBindType.Action, false, true);
        InitializeKey("ACT13", KeyCode.Alpha3, "Action Button 13", KeyBindType.Action, false, true);
        InitializeKey("ACT14", KeyCode.Alpha4, "Action Button 14", KeyBindType.Action, false, true);
        InitializeKey("ACT15", KeyCode.Alpha5, "Action Button 15", KeyBindType.Action, false, true);
        InitializeKey("ACT16", KeyCode.Alpha6, "Action Button 16", KeyBindType.Action, false, true);
        InitializeKey("ACT17", KeyCode.Alpha7, "Action Button 17", KeyBindType.Action, false, true);
        InitializeKey("ACT18", KeyCode.Alpha8, "Action Button 18", KeyBindType.Action, false, true);
        InitializeKey("ACT19", KeyCode.Alpha9, "Action Button 19", KeyBindType.Action, false, true);
        InitializeKey("ACT20", KeyCode.Alpha0, "Action Button 20", KeyBindType.Action, false, true);

        InitializeKey("ACT21", KeyCode.Alpha1, "Action Button 21", KeyBindType.Action, true);
        InitializeKey("ACT22", KeyCode.Alpha2, "Action Button 22", KeyBindType.Action, true);
        InitializeKey("ACT23", KeyCode.Alpha3, "Action Button 23", KeyBindType.Action, true);
        InitializeKey("ACT24", KeyCode.Alpha4, "Action Button 24", KeyBindType.Action, true);
        InitializeKey("ACT25", KeyCode.Alpha5, "Action Button 25", KeyBindType.Action, true);
        InitializeKey("ACT26", KeyCode.Alpha6, "Action Button 26", KeyBindType.Action, true);
        InitializeKey("ACT27", KeyCode.Alpha7, "Action Button 27", KeyBindType.Action, true);
        InitializeKey("ACT28", KeyCode.Alpha8, "Action Button 28", KeyBindType.Action, true);
        InitializeKey("ACT29", KeyCode.Alpha9, "Action Button 29", KeyBindType.Action, true);
        InitializeKey("ACT30", KeyCode.Alpha0, "Action Button 30", KeyBindType.Action, true);

        InitializeKey("ACT31", KeyCode.None, "Action Button 31", KeyBindType.Action);
        InitializeKey("ACT32", KeyCode.None, "Action Button 32", KeyBindType.Action);
        InitializeKey("ACT33", KeyCode.None, "Action Button 33", KeyBindType.Action);
        InitializeKey("ACT34", KeyCode.None, "Action Button 34", KeyBindType.Action);
        InitializeKey("ACT35", KeyCode.None, "Action Button 35", KeyBindType.Action);
        InitializeKey("ACT36", KeyCode.None, "Action Button 36", KeyBindType.Action);
        InitializeKey("ACT37", KeyCode.None, "Action Button 37", KeyBindType.Action);
        InitializeKey("ACT38", KeyCode.None, "Action Button 38", KeyBindType.Action);
        InitializeKey("ACT39", KeyCode.None, "Action Button 39", KeyBindType.Action);
        InitializeKey("ACT40", KeyCode.None, "Action Button 40", KeyBindType.Action);

        InitializeKey("ACT41", KeyCode.None, "Action Button 41", KeyBindType.Action);
        InitializeKey("ACT42", KeyCode.None, "Action Button 42", KeyBindType.Action);
        InitializeKey("ACT43", KeyCode.None, "Action Button 43", KeyBindType.Action);
        InitializeKey("ACT44", KeyCode.None, "Action Button 44", KeyBindType.Action);
        InitializeKey("ACT45", KeyCode.None, "Action Button 45", KeyBindType.Action);
        InitializeKey("ACT46", KeyCode.None, "Action Button 46", KeyBindType.Action);
        InitializeKey("ACT47", KeyCode.None, "Action Button 47", KeyBindType.Action);
        InitializeKey("ACT48", KeyCode.None, "Action Button 48", KeyBindType.Action);
        InitializeKey("ACT49", KeyCode.None, "Action Button 49", KeyBindType.Action);
        InitializeKey("ACT50", KeyCode.None, "Action Button 50", KeyBindType.Action);

        InitializeKey("ACT51", KeyCode.None, "Action Button 51", KeyBindType.Action);
        InitializeKey("ACT52", KeyCode.None, "Action Button 52", KeyBindType.Action);
        InitializeKey("ACT53", KeyCode.None, "Action Button 53", KeyBindType.Action);
        InitializeKey("ACT54", KeyCode.None, "Action Button 54", KeyBindType.Action);
        InitializeKey("ACT55", KeyCode.None, "Action Button 55", KeyBindType.Action);
        InitializeKey("ACT56", KeyCode.None, "Action Button 56", KeyBindType.Action);
        InitializeKey("ACT57", KeyCode.None, "Action Button 57", KeyBindType.Action);
        InitializeKey("ACT58", KeyCode.None, "Action Button 58", KeyBindType.Action);
        InitializeKey("ACT59", KeyCode.None, "Action Button 59", KeyBindType.Action);
        InitializeKey("ACT60", KeyCode.None, "Action Button 60", KeyBindType.Action);

        InitializeKey("ACT61", KeyCode.None, "Action Button 61", KeyBindType.Action);
        InitializeKey("ACT62", KeyCode.None, "Action Button 62", KeyBindType.Action);
        InitializeKey("ACT63", KeyCode.None, "Action Button 63", KeyBindType.Action);
        InitializeKey("ACT64", KeyCode.None, "Action Button 64", KeyBindType.Action);
        InitializeKey("ACT65", KeyCode.None, "Action Button 65", KeyBindType.Action);
        InitializeKey("ACT66", KeyCode.None, "Action Button 66", KeyBindType.Action);
        InitializeKey("ACT67", KeyCode.None, "Action Button 67", KeyBindType.Action);
        InitializeKey("ACT68", KeyCode.None, "Action Button 68", KeyBindType.Action);
        InitializeKey("ACT69", KeyCode.None, "Action Button 69", KeyBindType.Action);
        InitializeKey("ACT70", KeyCode.None, "Action Button 70", KeyBindType.Action);
    }

    private void InitializeKey(string key, KeyCode keyCode, string label, KeyBindType keyBindType, bool control = false, bool shift = false) {
        //Debug.Log("KeyBindManager.InitializeKey(" + key + ", " + control + ")");
        KeyBindNode keyBindNode = new KeyBindNode(key, keyCode, label, keyBindType, control, shift);
        keyBinds.Add(key, keyBindNode);
    }

    public void BindKey(string key, KeyCode keyCode, bool control, bool shift) {
        //Debug.Log("KeyBindManager.BindKey(" + key + ", " + keyCode.ToString() + ")");

        // since the key cannot control 2 actions, if it already exists, unbind it from that action
        UnbindKeyCode(keyBinds, keyCode, control, shift);

        //keyBinds[key].MyKeyCode = keyCode;
        keyBinds[key].UpdateKeyCode(keyCode, control, shift);
        //(PopupWindowManager.MyInstance.keyBindMenuWindow.MyCloseableWindowContents as KeyBindMenuController).UpdateKeyText(bindName, keyCode);
        bindName = string.Empty;
        SystemWindowManager.MyInstance.keyBindConfirmWindow.CloseWindow();
    }

    private void UnbindKeyCode(Dictionary<string, KeyBindNode> currentDictionary, KeyCode keyCode, bool control, bool shift) {
        //Debug.Log("KeyBindManager.UnbindKeyCode()");
        foreach (KeyBindNode keyBindNode in currentDictionary.Values) {
            if (keyBindNode.MyKeyCode == keyCode && keyBindNode.MyShift == shift && keyBindNode.MyControl == control) {
                keyBindNode.MyKeyCode = KeyCode.None;
                keyBindNode.MyShift = false;
                keyBindNode.MyControl = false;
                return;
            }
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

    public void BeginKeyBind(string key) {
        //Debug.Log("KeyBindManager.BeginKeyBind(" + key + ")");
        this.bindName = key;
        SystemWindowManager.MyInstance.keyBindConfirmWindow.OpenWindow();
    }

    private void OnGUI() {
        //Debug.Log("KeyBindManager.OnGUI()");
        if (bindName != string.Empty) {
            //Debug.Log("KeyBindManager.OnGUI(): bindName: " + bindName);
            Event e = Event.current;
            if (e.isKey && e.keyCode != KeyCode.LeftShift && e.keyCode != KeyCode.RightShift && e.keyCode != KeyCode.LeftControl && e.keyCode != KeyCode.RightControl) {
                //Debug.Log("KeyBindManager.OnGUI(): the bind was a key");
                BindKey(bindName, e.keyCode, e.control, e.shift);
            }
        }
    }

    public void CancelKeyBind() {
        SystemWindowManager.MyInstance.keyBindConfirmWindow.CloseWindow();
        bindName = string.Empty;
    }

}

public enum KeyBindType { Normal, Action, Constant, System }