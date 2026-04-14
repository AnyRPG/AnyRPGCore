using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class StorageContainerManagerClient : InteractableOptionManager {

        private StorageContainerProps storageContainerProps = null;
        private StorageContainerComponent storageContainerComponent = null;

        public StorageContainerProps StorageContainerProps { get => storageContainerProps; set => storageContainerProps = value; }
        public StorageContainerComponent StorageContainerComponent { get => storageContainerComponent; set => storageContainerComponent = value; }

        public void SetProps(StorageContainerProps storageContainerProps, StorageContainerComponent storageContainerComponent, int componentIndex, int choiceIndex) {
            //Debug.Log("VendorManager.SetProps()");
            this.storageContainerProps = storageContainerProps;
            this.storageContainerComponent = storageContainerComponent;
            BeginInteraction(storageContainerComponent, componentIndex, choiceIndex);
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


    }

}