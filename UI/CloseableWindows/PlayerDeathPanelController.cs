using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeathPanelController : WindowContentController {

    public override event Action<ICloseableWindowContents> OnOpenWindowHandler;

    public void SpawnPlayer() {
        SystemWindowManager.MyInstance.playerOptionsMenuWindow.CloseWindow();
        PlayerManager.MyInstance.SpawnPlayerUnit();
    }

    public void RevivePlayer() {
        Debug.Log("PlayerOptionsController.RevivePlayer()");
        SystemWindowManager.MyInstance.playerOptionsMenuWindow.CloseWindow();
        PlayerManager.MyInstance.RevivePlayerUnit();
    }

}
