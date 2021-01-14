using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ConfirmCancelCutscenePanelController : WindowContentController {

        public void CancelAction() {
            //Debug.Log("ConfirmCancelCutScenePanelController.CancelAction()");
            SystemWindowManager.MyInstance.confirmCancelCutsceneMenuWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("ConfirmCancelCutScenePanelController.ConfirmAction()");
            // might be something better to deactivate cutscene camera
            UIManager.MyInstance.MyCutSceneBarController.EndCutScene();
            //UIManager.MyInstance.ActivateInGameUI();
            SystemWindowManager.MyInstance.confirmCancelCutsceneMenuWindow.CloseWindow();
        }

    }

}