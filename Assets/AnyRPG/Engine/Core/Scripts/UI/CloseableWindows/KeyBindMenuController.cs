using AnyRPG;
﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
public class KeyBindMenuController : WindowContentController {

    [SerializeField]
    private GameObject otherKeyParent = null;

    [SerializeField]
    private GameObject actionKeyParent = null;

    [SerializeField]
    private GameObject systemKeyParent = null;

    [SerializeField]
    private GameObject keyBindButtonPrefab = null;

    //public override event Action<ICloseableWindowContents> OnOpenWindow;

    private void Start() {
        //Debug.Log("KeyBindMenuController.Start()");
        InitializeKeys();
    }

    private void InitializeKeys() {
        //Debug.Log("KeyBindMenuController.InitializeKeys()");
        foreach (KeyBindNode keyBindNode in KeyBindManager.MyInstance.MyKeyBinds.Values) {
            Transform nodeParent = null;
            if (keyBindNode.MyKeyBindType == KeyBindType.Action) {
                nodeParent = actionKeyParent.transform;
            } else if (keyBindNode.MyKeyBindType == KeyBindType.Normal) {
                nodeParent = otherKeyParent.transform;
            } else if (keyBindNode.MyKeyBindType == KeyBindType.Constant) {
                nodeParent = systemKeyParent.transform;
            }
            KeyBindSlotScript keyBindSlotScript = Instantiate(keyBindButtonPrefab, nodeParent).GetComponent<KeyBindSlotScript>();
            keyBindSlotScript.Initialize(keyBindNode);
            keyBindNode.SetSlotScript(keyBindSlotScript);
        }
    }

}

}