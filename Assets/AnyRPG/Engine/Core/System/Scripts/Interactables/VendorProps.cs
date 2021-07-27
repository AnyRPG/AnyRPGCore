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

        public override Sprite Icon { get => (SystemConfigurationManager.Instance.VendorInteractionPanelImage != null ? SystemConfigurationManager.Instance.VendorInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemConfigurationManager.Instance.VendorNamePlateImage != null ? SystemConfigurationManager.Instance.VendorNamePlateImage : base.NamePlateImage); }
        public List<VendorCollection> VendorCollections { get => vendorCollections; set => vendorCollections = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new VendorComponent(interactable, this);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;

        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            if (vendorCollectionNames != null && vendorCollectionNames.Count > 0) {
                foreach (string vendorCollectionName in vendorCollectionNames) {
                    VendorCollection tmpVendorCollection = SystemVendorCollectionManager.Instance.GetResource(vendorCollectionName);
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