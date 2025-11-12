using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class VendorManagerServer : InteractableOptionManager {

        public void SellItemToVendor(UnitController sourceUnitController, Interactable interactable, int componentIndex, InstantiatedItem instantiatedItem) {
            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(sourceUnitController);
            if (currentInteractables[componentIndex] is VendorComponent) {
                (currentInteractables[componentIndex] as VendorComponent).SellItemToVendor(sourceUnitController, componentIndex, instantiatedItem);
            }
        }

        public void BuyItemFromVendor(UnitController sourceUnitController, Interactable interactable, int componentIndex, int collectionIndex, int itemIndex, string resourceName, int accountId) {
            //Debug.Log($"VendorManager.BuyItemFromVendorServer({sourceUnitController.gameObject.name}, {interactable.gameObject.name}, {componentIndex}, {collectionIndex}, {itemIndex}, {resourceName}, {accountId})");

            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(sourceUnitController);
            if (currentInteractables[componentIndex] is VendorComponent) {
                VendorComponent vendorComponent = (currentInteractables[componentIndex] as VendorComponent);
                List<VendorCollection> localVendorCollections = vendorComponent.GetVendorCollections(accountId);
                if (localVendorCollections.Count > collectionIndex && localVendorCollections[collectionIndex].VendorItems.Count > itemIndex) {
                    VendorItem vendorItem = localVendorCollections[collectionIndex].VendorItems[itemIndex];
                    if (vendorItem.Item.ResourceName == resourceName) {
                        vendorComponent.BuyItemFromVendor(sourceUnitController, componentIndex, vendorItem, collectionIndex, itemIndex);
                    }
                }
            }

        }
    }

}