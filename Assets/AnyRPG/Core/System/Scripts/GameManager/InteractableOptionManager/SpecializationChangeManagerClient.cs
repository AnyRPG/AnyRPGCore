using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SpecializationChangeManagerClient : InteractableOptionManager {

        private SpecializationChangeProps specializationChangeProps = null;
        private SpecializationChangeComponent specializationChangeComponent = null;

        public SpecializationChangeProps SpecializationChangeProps { get => specializationChangeProps; set => specializationChangeProps = value; }
        public SpecializationChangeComponent SpecializationChangeComponent { get => specializationChangeComponent; set => specializationChangeComponent = value; }

        public void SetProps(SpecializationChangeProps specializationChangeProps, SpecializationChangeComponent specializationChangeComponent, int componentIndex, int choiceIndex) {
            //Debug.Log("VendorManager.SetProps()");
            this.specializationChangeProps = specializationChangeProps;
            this.specializationChangeComponent = specializationChangeComponent;
            BeginInteraction(specializationChangeComponent, componentIndex, choiceIndex);
        }

        public void RequestChangeCharacterSpecialization(UnitController sourceUnitController) {

            if (systemGameManager.GameMode == GameMode.Local) {
                specializationChangeComponent.ChangeCharacterSpecialization(sourceUnitController);
            } else {
                networkManagerClient.SetPlayerCharacterSpecialization(specializationChangeComponent.Interactable, componentIndex);

            }
        }

        public override void EndInteraction() {
            base.EndInteraction();

            specializationChangeProps = null;
            specializationChangeComponent = null;
        }


    }

}