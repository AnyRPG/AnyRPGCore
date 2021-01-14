using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class Vendor : InteractableOption {

        [SerializeField]
        private VendorProps vendorProps = new VendorProps();

        public override InteractableOptionProps InteractableOptionProps { get => vendorProps; }
    }

}