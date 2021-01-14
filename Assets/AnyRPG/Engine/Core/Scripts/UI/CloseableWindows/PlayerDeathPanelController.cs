using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PlayerDeathPanelController : WindowContentController {

        //public override event Action<ICloseableWindowContents> OnOpenWindow;

        public void RespawnPlayer() {
            SystemWindowManager.MyInstance.playerOptionsMenuWindow.CloseWindow();
            PlayerManager.MyInstance.RespawnPlayer();
        }

        public void RevivePlayer() {
            //Debug.Log("PlayerOptionsController.RevivePlayer()");
            SystemWindowManager.MyInstance.playerOptionsMenuWindow.CloseWindow();
            PlayerManager.MyInstance.RevivePlayerUnit();
        }

    }

}