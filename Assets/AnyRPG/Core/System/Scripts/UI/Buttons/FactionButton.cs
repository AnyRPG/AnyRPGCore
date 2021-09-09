using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class FactionButton : TransparencyButton {

        [SerializeField]
        private Faction faction = null;

        [SerializeField]
        private Image icon = null;

        [SerializeField]
        private TextMeshProUGUI factionName = null;

        [SerializeField]
        private TextMeshProUGUI description = null;

        public void AddFaction(Faction newFaction) {
            this.faction = newFaction;
            icon.sprite = this.faction.Icon;
            icon.color = Color.white;
            factionName.text = faction.DisplayName;
            //description.text = this.faction.GetSummary();
            description.text = faction.GetReputationSummary(faction);

        }

        public void ClearFaction() {
            icon.sprite = null;
            icon.color = new Color32(0, 0, 0, 0);
            factionName.text = string.Empty;
            description.text = string.Empty;
        }


    }

}