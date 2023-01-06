using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class VendorManager : InteractableOptionManager {

        private VendorProps vendorProps = null;

        public VendorProps VendorProps { get => vendorProps; set => vendorProps = value; }

        public void SetProps(VendorProps vendorProps, InteractableOptionComponent interactableOptionComponent) {
            //Debug.Log("VendorManager.SetProps()");
            this.vendorProps = vendorProps;
            BeginInteraction(interactableOptionComponent);
        }

        public override void EndInteraction() {
            base.EndInteraction();

            vendorProps = null;
        }
    }

}