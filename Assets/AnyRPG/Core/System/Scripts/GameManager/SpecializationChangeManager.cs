using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class SpecializationChangeManager : ConfiguredMonoBehaviour {

        public event System.Action OnConfirmAction = delegate { };
        public event System.Action OnEndInteraction = delegate { };

        private ClassSpecialization classSpecialization = null;

        // game manager references
        private PlayerManager playerManager = null;

        public ClassSpecialization ClassSpecialization { get => classSpecialization; set => classSpecialization = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
        }

        public void ChangeClassSpecialization() {
            playerManager.SetPlayerCharacterSpecialization(classSpecialization);

            OnConfirmAction();
        }

        public void EndInteraction() {
            OnEndInteraction();
        }

        public void SetDisplaySpecialization(ClassSpecialization classSpecialization) {
            //Debug.Log("SpecializationChangeManager.SetDisplaySpecialization(" + classSpecialization + ")");
            this.classSpecialization = classSpecialization;
        }


    }

}