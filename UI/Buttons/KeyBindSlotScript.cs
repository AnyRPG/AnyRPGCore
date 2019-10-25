using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
public class KeyBindSlotScript : MonoBehaviour {

    // a unique string that represents the dictionary key for this keybind throughout the game
    [SerializeField]
    private string keyBindID;

    [SerializeField]
    private Text slotLabel;

    [SerializeField]
    private Text buttonLabel;

    private KeyBindNode keyBindNode;

    public void Initialize(KeyBindNode keyBindNode) {
        this.keyBindNode = keyBindNode;
        this.keyBindID = keyBindNode.MyKeyBindID;
        this.slotLabel.text = keyBindNode.MyLabel;
        this.buttonLabel.text = (keyBindNode.MyControl ? "ctrl+" : "") + (keyBindNode.MyShift ? "shift+" : "") + keyBindNode.MyKeyCode.ToString();
    }

    public void SetKeyBind() {
        Debug.Log("KeyBindSlotScript.SetKeyBind()");
        KeyBindManager.MyInstance.BeginKeyBind(keyBindID);
    }
}

}