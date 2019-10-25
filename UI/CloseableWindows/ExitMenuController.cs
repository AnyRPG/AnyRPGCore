using AnyRPG;
﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
public class ExitMenuController : WindowContentController {

    public void CancelExit() {
        //Debug.Log("ExitMenuController.CancelExit()");
        SystemWindowManager.MyInstance.exitMenuWindow.CloseWindow();
    }

    public void ConfirmExit() {
        //Debug.Log("ExitMenuController.ConfirmExit()");
        // save any game data here
        #if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

}

}