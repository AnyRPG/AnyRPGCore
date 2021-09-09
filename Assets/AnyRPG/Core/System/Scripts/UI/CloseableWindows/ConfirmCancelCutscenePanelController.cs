using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ConfirmCancelCutscenePanelController : WindowContentController {

        [SerializeField]
        private HighlightButton noButton = null;

        [SerializeField]
        private HighlightButton yesButton = null;

        // game manager references
        private UIManager uIManager = null;
        private CutSceneBarController cutSceneBarController = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            noButton.Configure(systemGameManager);
            yesButton.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
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