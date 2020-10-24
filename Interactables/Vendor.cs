using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class Vendor : InteractableOption {

        public override event System.Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        private VendorConfig vendorConfig = null;

        public override Sprite Icon { get => vendorConfig.Icon; }
        public override Sprite NamePlateImage { get => vendorConfig.NamePlateImage; }

        [Header("Vendor")]

        [SerializeField]
        private List<string> vendorCollectionNames = new List<string>();

        private List<VendorCollection> vendorCollections = new List<VendorCollection>();

        public Vendor(Interactable interactable, VendorConfig interactableConfig) : base(interactable) {
            this.vendorConfig = interactableConfig;
            interactionPanelTitle = "Purchase Items";
        }

        protected override void Start() {
            base.Start();

            AddUnitProfileSettings();

        }

        public void AddUnitProfileSettings() {
            CharacterUnit characterUnit = GetComponent<CharacterUnit>();
            if (characterUnit != null && characterUnit.BaseCharacter != null && characterUnit.BaseCharacter.UnitProfile != null) {
                if (characterUnit.BaseCharacter.UnitProfile.VendorCollections != null) {
                    vendorCollections.AddRange(characterUnit.BaseCharacter.UnitProfile.VendorCollections);
                }
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

            if (vendorCollectionNames != null && vendorCollectionNames.Count > 0) {
                foreach (string vendorCollectionName in vendorCollectionNames) {
                    VendorCollection tmpVendorCollection = SystemVendorCollectionManager.MyInstance.GetResource(vendorCollectionName);
                    if (tmpVendorCollection != null) {
                        vendorCollections.Add(tmpVendorCollection);
                    } else {
                        Debug.LogError(gameObject.name + ".Vendor.SetupScriptableObjects(): Could not find vendor collection : " + vendorCollectionName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

        }


    }

}