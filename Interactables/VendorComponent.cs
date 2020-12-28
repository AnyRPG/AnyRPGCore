using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class VendorComponent : InteractableOptionComponent {

        public override event System.Action<InteractableOptionComponent> MiniMapStatusUpdateHandler = delegate { };

        public VendorProps Props { get => interactableOptionProps as VendorProps; }

        public VendorComponent(Interactable interactable, VendorProps interactableOptionProps) : base(interactable, interactableOptionProps) {
            interactableOptionProps.InteractionPanelTitle = "Purchase Items";
        }

        /*
        protected override void AddUnitProfileSettings() {
            if (unitProfile != null) {
                interactableOptionProps = unitProfile.VendorProps;
            }
            HandlePrerequisiteUpdates();
        }
        */

        public void InitWindow(ICloseableWindowContents vendorUI) {
            (vendorUI as VendorUI).PopulateDropDownList(Props.VendorCollections);
        }

        public override bool Interact(CharacterUnit source) {
            base.Interact(source);
            //Debug.Log(source + " attempting to interact with " + gameObject.name);
            if (!PopupWindowManager.MyInstance.vendorWindow.IsOpen) {
                //Debug.Log(source + " interacting with " + gameObject.name);
                PopupWindowManager.MyInstance.vendorWindow.MyCloseableWindowContents.OnOpenWindow += InitWindow;
                PopupWindowManager.MyInstance.vendorWindow.OpenWindow();
                return true;
            }
            return false;
        }

        public override void StopInteract() {
            base.StopInteract();
            PopupWindowManager.MyInstance.vendorWindow.CloseWindow();
            PopupWindowManager.MyInstance.vendorWindow.MyCloseableWindowContents.OnOpenWindow -= InitWindow;
        }

        public override void CallMiniMapStatusUpdateHandler() {
            MiniMapStatusUpdateHandler(this);
        }

        public override void HandlePlayerUnitSpawn() {
            base.HandlePlayerUnitSpawn();
            MiniMapStatusUpdateHandler(this);
        }

    }

}