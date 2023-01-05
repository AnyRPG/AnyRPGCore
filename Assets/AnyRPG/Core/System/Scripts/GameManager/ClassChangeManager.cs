using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class ClassChangeManager : ConfiguredMonoBehaviour {

        public event System.Action OnConfirmAction = delegate { };
        public event System.Action OnEndInteraction = delegate { };

        private CharacterClass characterClass = null;

        // game manager references
        private PlayerManager playerManager = null;

        public CharacterClass CharacterClass { get => characterClass; set => characterClass = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
        }

        public void ChangeCharacterClass() {
            playerManager.SetPlayerCharacterClass(characterClass);
            OnConfirmAction();
        }

        public void EndInteraction() {
            OnEndInteraction();
        }

        public void SetDisplayClass(CharacterClass characterClass) {
            //Debug.Log("ClassChangeManager.SetDisplayClass(" + characterClass + ")");
            this.characterClass = characterClass;
        }


    }

}