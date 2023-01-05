using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class FactionChangeManager : ConfiguredMonoBehaviour {

        public event System.Action OnConfirmAction = delegate { };
        public event System.Action OnEndInteraction = delegate { };

        private Faction faction = null;

        // game manager references
        private PlayerManager playerManager = null;

        public Faction Faction { get => faction; set => faction = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
        }

        public void ChangePlayerFaction() {
            playerManager.SetPlayerFaction(faction);
            OnConfirmAction();
        }

        public void EndInteraction() {
            OnEndInteraction();
        }

        public void SetDisplayFaction(Faction faction) {
            //Debug.Log("FactionChangeChangeManager.SetDisplayFaction(" + faction + ")");
            this.faction = faction;
        }


    }

}