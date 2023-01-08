using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class VendorComponent : InteractableOptionComponent {

        // game manager references
        private VendorManager vendorManager = null;

        public VendorProps Props { get => interactableOptionProps as VendorProps; }

        public VendorComponent(Interactable interactable, VendorProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            interactableOptionProps.InteractionPanelTitle = "Purchase Items";
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            vendorManager = systemGameManager.VendorManager;
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
            if (!uIManager.vendorWindow.IsOpen) {
                //Debug.Log(source + " interacting with " + gameObject.name);

                vendorManager.SetProps(Props, this);
                uIManager.vendorWindow.OpenWindow();
                
                return true;
            }
            return false;
        }

        public override void StopInteract() {
            base.StopInteract();
            uIManager.vendorWindow.CloseWindow();
        }

        public override bool PlayInteractionSound() {
            return true;
        }

        public override AudioClip GetInteractionSound(VoiceProps voiceProps) {
            return voiceProps.RandomStartVendorInteract;
        }


    }

}