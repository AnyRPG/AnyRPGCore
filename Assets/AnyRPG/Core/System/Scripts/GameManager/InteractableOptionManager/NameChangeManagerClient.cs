using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class NameChangeManagerClient : InteractableOptionManager {

        private NameChangeComponent nameChangeComponent = null;

        public NameChangeComponent NameChangeComponent { get => nameChangeComponent; set => nameChangeComponent = value; }

        public void SetProps(NameChangeComponent nameChangeComponent, int componentIndex, int choiceIndex) {
            this.nameChangeComponent = nameChangeComponent;

            BeginInteraction(nameChangeComponent, componentIndex, choiceIndex);
        }

        public void RequestChangePlayerName(UnitController sourceUnitController, string newName) {
            //Debug.Log($"NameChangeManager.RequestChangePlayerName({newName})");

            if (systemGameManager.GameMode == GameMode.Local) {
                nameChangeComponent.SetPlayerName(sourceUnitController, newName);
            } else {
                networkManagerClient.RequestChangePlayerName(nameChangeComponent.Interactable, componentIndex, newName);
            }
        }

        public override void EndInteraction() {
            base.EndInteraction();
            nameChangeComponent = null;
        }

    }

}