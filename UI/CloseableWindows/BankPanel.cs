using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BankPanel : BagPanel {

    public override event Action<ICloseableWindowContents> OnOpenWindowHandler;

    [SerializeField]
    protected BagBarController bagBarController;

    public BagBarController MyBagBarController { get => bagBarController; set => bagBarController = value; }

    public override void OnOpenWindow() {
        base.OnOpenWindow();
        InventoryManager.MyInstance.OpenBank();
    }

    public override void OnCloseWindow() {
        base.OnCloseWindow();
        InventoryManager.MyInstance.CloseBank();
    }

}
