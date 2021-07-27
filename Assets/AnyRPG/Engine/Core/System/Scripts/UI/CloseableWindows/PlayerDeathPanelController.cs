using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PlayerDeathPanelController : WindowContentController {

        //public override event Action<ICloseableWindowContents> OnOpenWindow;

        public void RespawnPlayer() {
            SystemWindowManager.Instance.playerOptionsMenuWindow.CloseWindow();
            PlayerManager.Instance.RespawnPlayer();
        }

        public void RevivePlayer() {
            //Debug.Log("PlayerOptionsController.RevivePlayer()");
            SystemWindowManager.Instance.playerOptionsMenuWindow.CloseWindow();
            PlayerManager.Instance.RevivePlayerUnit();
        }

    }

}