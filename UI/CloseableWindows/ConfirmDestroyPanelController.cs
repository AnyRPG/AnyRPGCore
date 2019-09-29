using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfirmDestroyPanelController : WindowContentController {

    public void CancelAction() {
        Debug.Log("NewGameMenuController.CancelAction()");
        SystemWindowManager.MyInstance.confirmDestroyMenuWindow.CloseWindow();
    }

    public void ConfirmAction() {
        Debug.Log("NewGameMenuController.ConfirmAction()");
        HandScript.MyInstance.DeleteItem(); ;
        SystemWindowManager.MyInstance.confirmDestroyMenuWindow.CloseWindow();
    }

}
