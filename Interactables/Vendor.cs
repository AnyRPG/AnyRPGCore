using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Vendor : InteractableOption 
{
    public override event System.Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

    [SerializeField]
    private VendorItem[] items;

    protected override void Start() {
        base.Start();
        interactionPanelTitle = "Purchase Items";
    }

    public void InitWindow(ICloseableWindowContents vendorUI) {
        (vendorUI as VendorUI).CreatePages(items);
    }

    public override bool Interact(CharacterUnit source) {
        base.Interact(source);
        //Debug.Log(source + " attempting to interact with " + gameObject.name);
        if (!PopupWindowManager.MyInstance.vendorWindow.IsOpen) {
            //Debug.Log(source + " interacting with " + gameObject.name);
            PopupWindowManager.MyInstance.vendorWindow.MyCloseableWindowContents.OnOpenWindowHandler += InitWindow;
            PopupWindowManager.MyInstance.vendorWindow.OpenWindow();
            return true;
        }
        return false;
    }

    public override void StopInteract() {
        base.StopInteract();
        PopupWindowManager.MyInstance.vendorWindow.CloseWindow();
        PopupWindowManager.MyInstance.vendorWindow.MyCloseableWindowContents.OnOpenWindowHandler -= InitWindow;
    }

}
