using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class FactionChangeManagerClient : InteractableOptionManager {

        private FactionChangeProps factionChangeProps = null;
        private FactionChangeComponent factionChangeComponent = null;

        public FactionChangeProps FactionChangeProps { get => factionChangeProps; set => factionChangeProps = value; }
        public FactionChangeComponent FactionChangeComponent { get => factionChangeComponent; set => factionChangeComponent = value; }

        public void SetProps(FactionChangeProps factionChangeProps, FactionChangeComponent factionChangeComponent, int componentIndex, int choiceIndex) {
            //Debug.Log("VendorManager.SetProps()");
            this.factionChangeProps = factionChangeProps;
            this.factionChangeComponent = factionChangeComponent;
            BeginInteraction(factionChangeComponent, componentIndex, choiceIndex);
        }

        public void RequestChangeCharacterFaction(UnitController sourceUnitController) {

            if (systemGameManager.GameMode == GameMode.Local) {
                factionChangeComponent.ChangeCharacterFaction(sourceUnitController);
            } else {
                networkManagerClient.RequestSetPlayerFaction(factionChangeComponent.Interactable, componentIndex);

            }
        }

        public override void EndInteraction() {
            base.EndInteraction();

            factionChangeProps = null;
            factionChangeComponent = null;
        }


    }

}