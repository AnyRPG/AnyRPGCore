using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class NameChangeManager : ConfiguredMonoBehaviour {

        public event System.Action OnConfirmAction = delegate { };
        public event System.Action OnEndInteraction = delegate { };

        // game manager references
        private PlayerManager playerManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
        }

        public void ChangePlayerName(string newName) {
            playerManager.SetPlayerName(newName);
            OnConfirmAction();
        }

        public void EndInteraction() {
            OnEndInteraction();
        }


    }

}