using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ConfirmCancelCutscenePanelController : WindowContentController {

        public void CancelAction() {
            //Debug.Log("ConfirmCancelCutScenePanelController.CancelAction()");
            SystemWindowManager.Instance.confirmCancelCutsceneMenuWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("ConfirmCancelCutScenePanelController.ConfirmAction()");
            // might be something better to deactivate cutscene camera
            UIManager.Instance.CutSceneBarController.EndCutScene();
            //UIManager.Instance.ActivateInGameUI();
            SystemWindowManager.Instance.confirmCancelCutsceneMenuWindow.CloseWindow();
        }

    }

}