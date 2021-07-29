using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ConfirmCancelCutscenePanelController : WindowContentController {

        public void CancelAction() {
            //Debug.Log("ConfirmCancelCutScenePanelController.CancelAction()");
            SystemGameManager.Instance.UIManager.SystemWindowManager.confirmCancelCutsceneMenuWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("ConfirmCancelCutScenePanelController.ConfirmAction()");
            // might be something better to deactivate cutscene camera
            SystemGameManager.Instance.UIManager.CutSceneBarController.EndCutScene();
            //SystemGameManager.Instance.UIManager.ActivateInGameUI();
            SystemGameManager.Instance.UIManager.SystemWindowManager.confirmCancelCutsceneMenuWindow.CloseWindow();
        }

    }

}