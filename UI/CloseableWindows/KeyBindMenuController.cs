using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyBindMenuController : WindowContentController {

    [SerializeField]
    private GameObject otherKeyParent;

    [SerializeField]
    private GameObject actionKeyParent;

    [SerializeField]
    private GameObject systemKeyParent;

    [SerializeField]
    private GameObject keyBindButtonPrefab;

    public override event Action<ICloseableWindowContents> OnOpenWindow;

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
