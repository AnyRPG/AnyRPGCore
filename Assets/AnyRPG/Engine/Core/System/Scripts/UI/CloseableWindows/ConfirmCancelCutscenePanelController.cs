using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ConfirmCancelCutscenePanelController : WindowContentController {


        // game manager references
        private UIManager uIManager = null;
        private CutSceneBarController cutSceneBarController = null;

        public override void Init(SystemGameManager systemGameManager) {
            base.Init(systemGameManager);

            uIManager = systemGameManager.UIManager;
            cutSceneBarController = uIManager.CutSceneBarController;
        }

        public void CancelAction() {
            //Debug.Log("ConfirmCancelCutScenePanelController.CancelAction()");
            uIManager.confirmCancelCutsceneMenuWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("ConfirmCancelCutScenePanelController.ConfirmAction()");
            // might be something better to deactivate cutscene camera
            cutSceneBarController.EndCutScene();
            //SystemGameManager.Instance.UIManager.ActivateInGameUI();
            uIManager.confirmCancelCutsceneMenuWindow.CloseWindow();
        }

    }

}