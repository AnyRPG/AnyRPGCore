using AnyRPG;
﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
public class KeyBindNode
{
    public event System.Action OnKeyPressedHandler = delegate { };

    private string keyBindID;

    private KeyCode keyCode;

    private bool control = false;

    private bool shift = false;

    private KeyBindType keyBindType;

    // the label to use in the keybind manager
    private string label;

    private KeyBindSlotScript keyBindSlotScript = null;

    private ActionButton actionButton = null;

    public KeyBindNode(string keyBindID, KeyCode keyCode, string label, KeyBindType keyBindType, bool control = false, bool shift = false) {
        //Debug.Log("KeyBindNode(" + keyBindID + ", " + control + ")");
        this.keyBindID = keyBindID;
        this.label = label;
        this.keyBindType = keyBindType;
        this.control = control;
        this.shift = shift;
        this.MyKeyCode = keyCode;
    }

    public string MyKeyBindID { get => keyBindID; set => keyBindID = value; }

    public KeyCode MyKeyCode {
        get => keyCode;
        set {
            //Debug.Log("KeyBindNode.SetMyKeyCode");
            keyCode = value;
            if (MyActionButton != null) {
                MyActionButton.MyKeyBindText.text = FormatActionButtonLabel();
            }
            if (MyKeyBindSlotScript != null) {
                MyKeyBindSlotScript.Initialize(this);
            }
        }
    }

    public string MyLabel { get => label; set => label = value; }

    public ActionButton MyActionButton {
        get => actionButton;
        set {
            actionButton = value;
            actionButton.MyKeyBindText.text = FormatActionButtonLabel();
        }
    }

    public KeyBindSlotScript MyKeyBindSlotScript { get => keyBindSlotScript; set => keyBindSlotScript = value; }
    public KeyBindType MyKeyBindType { get => keyBindType; set => keyBindType = value; }
    public bool MyControl { get => control; set => control = value; }
    public bool MyShift { get => shift; set => shift = value; }

    private string FormatActionButtonLabel() {
        if (MyKeyCode.ToString() == "None") {
            return string.Empty;
        }
        return (control ? "c" : "") + (shift ? "s" : "") + ReplaceSpecialCharacters(MyKeyCode.ToString());
    }

    public string ReplaceSpecialCharacters(string inputString) {
        inputString = inputString.Replace("Alpha", "");
        inputString = inputString.Replace("Period", ".");
        return inputString;
    }

    public void SetSlotScript(KeyBindSlotScript keyBindSlotScript) {
        this.keyBindSlotScript = keyBindSlotScript;
    }

    public void UpdateKeyCode(KeyCode keyCode, bool control, bool shift) {
        Debug.Log("KeyBindNode.UpdateKeyCode(" + keyCode + ", " + control + ", " + shift + ")");
        this.control = control;
        this.shift = shift;
        this.MyKeyCode = keyCode;
    }
}

}