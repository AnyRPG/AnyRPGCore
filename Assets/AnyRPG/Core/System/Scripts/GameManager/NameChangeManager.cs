using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class NameChangeManager : InteractableOptionManager {

        // game manager references
        private PlayerManager playerManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            playerManager = systemGameManager.PlayerManager;
        }

        public void ChangePlayerName(string newName) {
            playerManager.SetPlayerName(newName);
            ConfirmAction();
        }

    }

}