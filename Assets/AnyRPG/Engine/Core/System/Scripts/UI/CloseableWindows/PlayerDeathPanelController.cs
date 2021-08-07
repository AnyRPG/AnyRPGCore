using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PlayerDeathPanelController : WindowContentController {

        //public override event Action<ICloseableWindowContents> OnOpenWindow;

        // game manager references
        private UIManager uIManager = null;
        private PlayerManager playerManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            uIManager = systemGameManager.UIManager;
            playerManager = systemGameManager.PlayerManager;
        }

        public void RespawnPlayer() {
            uIManager.playerOptionsMenuWindow.CloseWindow();
            playerManager.RespawnPlayer();
        }

        public void RevivePlayer() {
            //Debug.Log("PlayerOptionsController.RevivePlayer()");
            uIManager.playerOptionsMenuWindow.CloseWindow();
            playerManager.RevivePlayerUnit();
        }

    }

}