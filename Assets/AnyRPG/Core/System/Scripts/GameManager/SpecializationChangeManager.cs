using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class SpecializationChangeManager : InteractableOptionManager {

        private ClassSpecialization classSpecialization = null;

        // game manager references
        private PlayerManager playerManager = null;

        public ClassSpecialization ClassSpecialization { get => classSpecialization; set => classSpecialization = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            playerManager = systemGameManager.PlayerManager;
        }

        public void SetDisplaySpecialization(ClassSpecialization classSpecialization, InteractableOptionComponent interactableOptionComponent) {
            //Debug.Log("SpecializationChangeManager.SetDisplaySpecialization(" + classSpecialization + ")");

            this.classSpecialization = classSpecialization;

            BeginInteraction(interactableOptionComponent);
        }

        public void ChangeClassSpecialization() {
            playerManager.SetPlayerCharacterSpecialization(classSpecialization);

            ConfirmAction();
        }

        public override void EndInteraction() {
            base.EndInteraction();

            classSpecialization = null;
        }

    }

}