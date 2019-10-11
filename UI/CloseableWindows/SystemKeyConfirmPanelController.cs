using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemKeyConfirmPanelController : WindowContentController {

    public override event Action<ICloseableWindowContents> OnOpenWindow;

    private void Start() {
        //Debug.Log("KeyConfirmPanelController.Start()");
    }

    public void CancelBind() {
        KeyBindManager.MyInstance.CancelKeyBind();
    }

}
