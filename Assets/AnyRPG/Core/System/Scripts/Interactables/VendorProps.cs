using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class VendorProps : InteractableOptionProps {

        [Header("Vendor")]

        [SerializeField]
        [ResourceSelector(resourceType = typeof(VendorCollection))]
        private List<string> vendorCollectionNames = new List<string>();

        private List<VendorCollection> vendorCollections = new List<VendorCollection>();

        public override Sprite Icon { get => (systemConfigurationManager.VendorInteractionPanelImage != null ? systemConfigurationManager.VendorInteractionPanelImage : base.Icon); }
        public override Sprite NameplateImage { get => (systemConfigurationManager.VendorNameplateImage != null ? systemConfigurationManager.VendorNameplateImage : base.NameplateImage); }
        public List<VendorCollection> VendorCollections { get => vendorCollections; set => vendorCollections = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new VendorComponent(interactable, this, systemGameManager);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;

        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            if (vendorCollectionNames != null && vendorCollectionNames.Count > 0) {
                foreach (string vendorCollectionName in vendorCollectionNames) {
                    VendorCollection tmpVendorCollection = systemDataFactory.GetResource<VendorCollection>(vendorCollectionName);
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