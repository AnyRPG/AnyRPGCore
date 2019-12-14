using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class FactionButton : TransparencyButton {

        [SerializeField]
        private Faction faction;

        [SerializeField]
        private Image icon;

        [SerializeField]
        private Text factionName;

        [SerializeField]
        private Text description;

        public void AddFaction(Faction newFaction) {
            this.faction = newFaction;
            icon.sprite = this.faction.MyIcon;
            icon.color = Color.white;
            factionName.text = faction.MyName;
            //description.text = this.faction.GetSummary();
            description.text = faction.GetExtendedSummary(faction);

        }

        public void ClearFaction() {
            icon.sprite = null;
            icon.color = new Color32(0, 0, 0, 0);
            factionName.text = string.Empty;
            description.text = string.Empty;
        }


    }

}