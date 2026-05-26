using UnityEngine;

namespace AnyRPG {
    public class Vendor : InteractableOption {

        [SerializeField]
        private VendorProps vendorProps = new VendorProps();

        public override InteractableOptionProps InteractableOptionProps { get => vendorProps; }
    }

}