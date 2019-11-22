using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class Vendor : InteractableOption {

        public override event System.Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        public override Sprite MyIcon { get => (SystemConfigurationManager.MyInstance.MyVendorInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyVendorInteractionPanelImage : base.MyIcon); }
        public override Sprite MyNamePlateImage { get => (SystemConfigurationManager.MyInstance.MyVendorNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyVendorNamePlateImage : base.MyNamePlateImage); }

        [SerializeField]
        private VendorItem[] items;

        [SerializeField]
        private List<VendorCollection> vendorCollections = new List<VendorCollection>();

        protected override void Start() {
            base.Start();
            interactionPanelTitle = "Purchase Items";
        }

        public void InitWindow(ICloseableWindowContents vendorUI) {
            (vendorUI as VendorUI).PopulateDropDownList(vendorCollections);
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

        public override void HandlePrerequisiteUpdates() {
            base.HandlePrerequisiteUpdates();
            MiniMapStatusUpdateHandler(this);
        }
    }

}