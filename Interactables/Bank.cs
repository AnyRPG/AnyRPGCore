using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bank : InteractableOption
{
    public override event System.Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

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
        base.HandlePrerequisiteUpdates();
        MiniMapStatusUpdateHandler(this);
    }
}
