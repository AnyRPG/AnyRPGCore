using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class VendorProps : InteractableOptionProps {

        [Header("Vendor")]

        [SerializeField]
        private List<string> vendorCollectionNames = new List<string>();

        private List<VendorCollection> vendorCollections = new List<VendorCollection>();

        public override Sprite Icon { get => (SystemConfigurationManager.MyInstance.MyVendorInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyVendorInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemConfigurationManager.MyInstance.MyVendorNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyVendorNamePlateImage : base.NamePlateImage); }
        public List<VendorCollection> VendorCollections { get => vendorCollections; set => vendorCollections = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable) {
            return new VendorComponent(interactable, this);
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            if (vendorCollectionNames != null && vendorCollectionNames.Count > 0) {
                foreach (string vendorCollectionName in vendorCollectionNames) {
                    VendorCollection tmpVendorCollection = SystemVendorCollectionManager.MyInstance.GetResource(vendorCollectionName);
                    if (tmpVendorCollection != null) {
                        vendorCollections.Add(tmpVendorCollection);
                    } else {
                        Debug.LogError("VendorProps.SetupScriptableObjects(): Could not find vendor collection : " + vendorCollectionName + " while inititalizing.  CHECK INSPECTOR");
                    }
                }
            }

        }
    }

}