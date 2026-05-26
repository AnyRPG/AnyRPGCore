namespace AnyRPG {
    public class StorageContainerManagerClient : InteractableOptionManager {

        private StorageContainerProps storageContainerProps = null;
        private StorageContainerComponent storageContainerComponent = null;

        // game manager references
        private PlayerManagerClient playerManagerClient = null;

        public StorageContainerProps StorageContainerProps { get => storageContainerProps; set => storageContainerProps = value; }
        public StorageContainerComponent StorageContainerComponent { get => storageContainerComponent; set => storageContainerComponent = value; }

        public void SetProps(StorageContainerProps storageContainerProps, StorageContainerComponent storageContainerComponent, int componentIndex, int choiceIndex) {
            //Debug.Log("VendorManager.SetProps()");
            this.storageContainerProps = storageContainerProps;
            this.storageContainerComponent = storageContainerComponent;
            BeginInteraction(storageContainerComponent, componentIndex, choiceIndex);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManagerClient = systemGameManager.PlayerManagerClient;
        }

        /*
        public void RequestChangeCharacterFaction(UnitController sourceUnitController) {

            if (systemGameManager.GameMode == GameMode.Local) {
                storageContainerComponent.ChangeCharacterFaction(sourceUnitController);
            } else {
                networkManagerClient.RequestSetPlayerFaction(storageContainerComponent.Interactable, componentIndex);

            }
        }
        */

        public override void EndInteraction() {
            base.EndInteraction();

            storageContainerProps = null;
            storageContainerComponent = null;
        }

        /*
        public void MoveItemFromCharacterToStorageContainer(int toSlotIndex, InventorySlot inventorySlot, bool isBankSlot) {
            playerManagerClient.UnitController.CharacterInventoryManager.RequestMoveItemToStorageContainer(storageContainerComponent, toSlotIndex, inventorySlot, isBankSlot);
        }
        */
    }

}