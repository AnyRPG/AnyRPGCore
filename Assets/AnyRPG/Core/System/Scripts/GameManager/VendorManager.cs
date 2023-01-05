using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class VendorManager : ConfiguredMonoBehaviour {

        public event System.Action OnConfirmAction = delegate { };
        public event System.Action OnEndInteraction = delegate { };

        private VendorProps vendorProps = null;
        private InteractableOptionComponent interactableOptionComponent = null;

        public VendorProps VendorProps { get => vendorProps; set => vendorProps = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
        }

        public void EndInteraction() {
            interactableOptionComponent.ProcessStopInteract();
            interactableOptionComponent = null;
            OnEndInteraction();
        }

        public void SetProps(VendorProps vendorProps, InteractableOptionComponent interactableOptionComponent) {
            //Debug.Log("VendorManager.SetProps()");
            this.vendorProps = vendorProps;
            this.interactableOptionComponent = interactableOptionComponent;
            interactableOptionComponent.ProcessStartInteract();
        }


    }

}