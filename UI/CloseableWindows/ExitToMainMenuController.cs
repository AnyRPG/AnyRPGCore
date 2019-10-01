using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitToMainMenuController : WindowContentController {

    public void CancelExit() {
        //Debug.Log("ExitMenuController.CancelExit()");
        SystemWindowManager.MyInstance.exitToMainMenuWindow.CloseWindow();
    }

    public void ConfirmExit() {
        //Debug.Log("ExitMenuController.ConfirmExit()");
        SystemWindowManager.MyInstance.exitToMainMenuWindow.CloseWindow();
        LevelManager.MyInstance.LoadMainMenu();
    }

}
