namespace AnyRPG {
    public class VendorManagerClient : InteractableOptionManager {

        private VendorProps vendorProps = null;
        private VendorComponent vendorComponent = null;

        InstantiatedItem instantiatedItem = null;

        // game manager references
        private PlayerManagerClient playerManagerClient = null;

        public VendorProps VendorProps { get => vendorProps; set => vendorProps = value; }
        public VendorComponent VendorComponent { get => vendorComponent; set => vendorComponent = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManagerClient = systemGameManager.PlayerManagerClient;
        }

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

        public void SetSellItem(InstantiatedItem instantiatedItem) {
            //Debug.Log($"VendorManagerClient.SetSellItem({instantiatedItem.DisplayName})");
            this.instantiatedItem = instantiatedItem;
        }

        public void RequestSellItemToVendor(InstantiatedItem instantiatedItem) {
            if (systemGameManager.GameMode == GameMode.Local) {
                vendorComponent.SellItemToVendor(playerManagerClient.UnitController, componentIndex, instantiatedItem);
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

        public void RequestSellItemToVendor() {
            RequestSellItemToVendor(instantiatedItem);
        }
    }

}