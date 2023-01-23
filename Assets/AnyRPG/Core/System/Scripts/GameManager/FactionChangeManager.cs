using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class FactionChangeManager : InteractableOptionManager {

        private Faction faction = null;

        // game manager references
        private PlayerManager playerManager = null;

        public Faction Faction { get => faction; set => faction = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            playerManager = systemGameManager.PlayerManager;
        }

        public void SetDisplayFaction(Faction faction, InteractableOptionComponent interactableOptionComponent) {
            //Debug.Log("FactionChangeChangeManager.SetDisplayFaction(" + faction.DisplayName + ")");

            this.faction = faction;

            BeginInteraction(interactableOptionComponent);
        }

        public void ChangePlayerFaction() {
            playerManager.SetPlayerFaction(faction);
            
            ConfirmAction();
        }

        public override void EndInteraction() {
            base.EndInteraction();

            faction = null;
        }

    }

}