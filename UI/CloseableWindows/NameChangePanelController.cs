using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NameChangePanelController : WindowContentController {

    public InputField textInput;
    public event System.Action OnConfirmAction = delegate { };
    public override event Action<ICloseableWindowContents> OnCloseWindowHandler = delegate { };


    public void CancelAction() {
        Debug.Log("NameChangePanelController.CancelAction()");
        SystemWindowManager.MyInstance.nameChangeWindow.CloseWindow();
    }

    public void ConfirmAction() {
        Debug.Log("NameChangePanelController.ConfirmAction()");
        if (textInput.text != null && textInput.text != string.Empty) {
            PlayerManager.MyInstance.SetPlayerName(textInput.text);
            OnConfirmAction();
            SystemWindowManager.MyInstance.nameChangeWindow.CloseWindow();
        }
    }

    public override void OnOpenWindow() {
        //Debug.Log("NameChangePanelController.OnOpenWindow()");
        base.OnOpenWindow();
        textInput.text = PlayerManager.MyInstance.MyCharacter.MyCharacterName;
    }

    public override void OnCloseWindow() {
        base.OnCloseWindow();
        OnCloseWindowHandler(this);
    }
}
