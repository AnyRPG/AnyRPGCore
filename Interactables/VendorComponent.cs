using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class VendorComponent : InteractableOptionComponent {

        public override event System.Action<InteractableOptionComponent> MiniMapStatusUpdateHandler = delegate { };

        private VendorProps interactableOptionProps = null;

        public override Sprite Icon { get => interactableOptionProps.Icon; }
        public override Sprite NamePlateImage { get => interactableOptionProps.NamePlateImage; }

        private List<VendorCollection> vendorCollections = new List<VendorCollection>();

        public VendorComponent(Interactable interactable, VendorProps interactableOptionProps) : base(interactable) {
            this.interactableOptionProps = interactableOptionProps;
            interactionPanelTitle = "Purchase Items";
        }

        protected override void AddUnitProfileSettings() {
            if (unitProfile != null) {
                interactableOptionProps = unitProfile.VendorProps;
            }
            HandlePrerequisiteUpdates();
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

        public override void HandlePlayerUnitSpawn() {
            base.HandlePlayerUnitSpawn();
            MiniMapStatusUpdateHandler(this);
        }


        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            if (interactableOptionProps.VendorCollectionNames != null && interactableOptionProps.VendorCollectionNames.Count > 0) {
                foreach (string vendorCollectionName in interactableOptionProps.VendorCollectionNames) {
                    VendorCollection tmpVendorCollection = SystemVendorCollectionManager.MyInstance.GetResource(vendorCollectionName);
                    if (tmpVendorCollection != null) {
                        vendorCollections.Add(tmpVendorCollection);
                    } else {
                        Debug.LogError("Vendor.SetupScriptableObjects(): Could not find vendor collection : " + vendorCollectionName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

        }


    }

}