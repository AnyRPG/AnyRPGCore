using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class VendorComponent : InteractableOptionComponent {

        public VendorProps Props { get => interactableOptionProps as VendorProps; }

        public VendorComponent(Interactable interactable, VendorProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            interactableOptionProps.InteractionPanelTitle = "Purchase Items";
        }

        /*
        protected override void AddUnitProfileSettings() {
            if (unitProfile != null) {
                interactableOptionProps = unitProfile.VendorProps;
            }
            HandlePrerequisiteUpdates();
        }
        */

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            base.Interact(source, optionIndex);
            //Debug.Log(source + " attempting to interact with " + gameObject.name);
            if (!SystemGameManager.Instance.UIManager.vendorWindow.IsOpen) {
                //Debug.Log(source + " interacting with " + gameObject.name);
                SystemGameManager.Instance.UIManager.vendorWindow.OpenWindow();
                (SystemGameManager.Instance.UIManager.vendorWindow.CloseableWindowContents as VendorUI).PopulateDropDownList(Props.VendorCollections);
                return true;
            }
            return false;
        }

        public override void StopInteract() {
            base.StopInteract();
            SystemGameManager.Instance.UIManager.vendorWindow.CloseWindow();
        }

    }

}