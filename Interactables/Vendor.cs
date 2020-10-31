using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class Vendor : InteractableOption {

        [SerializeField]
        private VendorProps vendorProps = new VendorProps();

        [Header("Vendor")]

        [SerializeField]
        private List<string> vendorCollectionNames = new List<string>();

        public override InteractableOptionProps InteractableOptionProps { get => vendorProps; }
    }

}