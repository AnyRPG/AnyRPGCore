using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class ClassChangeManagerClient : InteractableOptionManager {

        private ClassChangeProps classChangeProps = null;
        private ClassChangeComponent classChangeComponent = null;

        public ClassChangeProps ClassChangeProps { get => classChangeProps; set => classChangeProps = value; }
        public ClassChangeComponent ClassChangeComponent { get => classChangeComponent; set => classChangeComponent = value; }

        public void SetProps(ClassChangeProps classChangeProps, ClassChangeComponent classChangeComponent, int componentIndex, int choiceIndex) {
            //Debug.Log("VendorManager.SetProps()");
            this.classChangeProps = classChangeProps;
            this.classChangeComponent = classChangeComponent;
            BeginInteraction(classChangeComponent, componentIndex, choiceIndex);
        }

        public void RequestChangeCharacterClass(UnitController sourceUnitController) {
            
            if (systemGameManager.GameMode == GameMode.Local) {
                classChangeComponent.ChangeCharacterClass(sourceUnitController);
            } else {
                networkManagerClient.RequestSetPlayerCharacterClass(classChangeComponent.Interactable, componentIndex);
            }
        }

        public override void EndInteraction() {
            base.EndInteraction();

            classChangeProps = null;
            classChangeComponent = null;
        }

    }

}