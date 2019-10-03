using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bank : InteractableOption {

    public override event System.Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

    public override Sprite MyIcon { get => (SystemConfigurationManager.MyInstance.MyBankInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyBankInteractionPanelImage : base.MyIcon); }
    public override Sprite MyNamePlateImage { get => (SystemConfigurationManager.MyInstance.MyBankNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyBankNamePlateImage : base.MyNamePlateImage); }

    protected override void Start() {
        base.Start();
        interactionPanelTitle = "Bank";
    }

    public override bool Interact(CharacterUnit source) {
        //Debug.Log(gameObject.name + ".Bank.Interact(" + (source == null ? "null" : source.name) +")");
        base.Interact(source);
        PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
        if (!PopupWindowManager.MyInstance.bankWindow.IsOpen) {
            PopupWindowManager.MyInstance.bankWindow.OpenWindow();
            return true;
        }
        return false;
    }

    public override void StopInteract() {
        base.StopInteract();
        PopupWindowManager.MyInstance.bankWindow.CloseWindow();
    }

    public override void HandlePrerequisiteUpdates() {
        //Debug.Log(gameObject.name + ".Bank.HandldePrerequisiteUpdates()");
        base.HandlePrerequisiteUpdates();
        MiniMapStatusUpdateHandler(this);
    }
}
