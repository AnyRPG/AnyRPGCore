using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Vendor Config", menuName = "AnyRPG/Interactable/VendorConfig")]
    [System.Serializable]
    public class VendorConfig : InteractableOptionConfig {

        [Header("Vendor")]

        [SerializeField]
        private List<string> vendorCollectionNames = new List<string>();

        public override Sprite Icon { get => (SystemConfigurationManager.MyInstance.MyVendorInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyVendorInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemConfigurationManager.MyInstance.MyVendorNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyVendorNamePlateImage : base.NamePlateImage); }
        public List<string> VendorCollectionNames { get => vendorCollectionNames; set => vendorCollectionNames = value; }

        public InteractableOption GetInteractableOption(Interactable interactable) {
            return new Vendor(interactable, this);
        }
    }

}