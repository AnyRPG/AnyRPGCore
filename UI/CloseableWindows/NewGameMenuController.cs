using AnyRPG;
﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
public class NewGameMenuController : WindowContentController {

    public void CancelAction() {
        //Debug.Log("NewGameMenuController.CancelAction()");
        SystemWindowManager.MyInstance.newGameMenuWindow.CloseWindow();
    }

    public void ConfirmAction() {
        //Debug.Log("NewGameMenuController.ConfirmAction()");
        SystemWindowManager.MyInstance.newGameMenuWindow.CloseWindow();
        SystemWindowManager.MyInstance.loadGameWindow.CloseWindow();
        SaveManager.MyInstance.TryNewGame();
    }

}

}