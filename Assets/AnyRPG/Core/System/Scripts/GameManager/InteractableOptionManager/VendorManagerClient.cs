using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class VendorManagerClient : InteractableOptionManager {

        private VendorProps vendorProps = null;
        private VendorComponent vendorComponent = null;

        public VendorProps VendorProps { get => vendorProps; set => vendorProps = value; }
        public VendorComponent VendorComponent { get => vendorComponent; set => vendorComponent = value; }

        public void SetProps(VendorProps vendorProps, VendorComponent vendorComponent, int componentIndex, int choiceIndex) {
            //Debug.Log("VendorManager.SetProps()");
            this.vendorProps = vendorProps;
            this.vendorComponent = vendorComponent;
            BeginInteraction(vendorComponent, componentIndex, choiceIndex);
        }

        public override void EndInteraction() {
            base.EndInteraction();

            vendorProps = null;
        }

        public void RequestSellItemToVendor(UnitController sourceUnitController, InstantiatedItem instantiatedItem) {
            if (systemGameManager.GameMode == GameMode.Local) {
                vendorComponent.SellItemToVendor(sourceUnitController, componentIndex, instantiatedItem);
            } else {
                networkManagerClient.SellItemToVendor(vendorComponent.Interactable, componentIndex, instantiatedItem.InstanceId);
            }
        }

        public void RequestBuyItemFromVendor(UnitController sourceUnitController, VendorItem vendorItem, int collectionIndex, int itemIndex) {
            //Debug.Log($"VendorManager.BuyItemFromVendor({sourceUnitController.gameObject.name}, {vendorItem.Item.ResourceName}, {collectionIndex}, {itemIndex})");

            if (systemGameManager.GameMode == GameMode.Local) {
                vendorComponent.BuyItemFromVendor(sourceUnitController, componentIndex, vendorItem, collectionIndex, itemIndex);
            } else {
                networkManagerClient.BuyItemFromVendor(vendorComponent.Interactable, componentIndex, collectionIndex, itemIndex, vendorItem.Item.ResourceName);
            }
        }

    }

}