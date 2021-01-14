using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class NewGameFactionButton : HighlightButton {

        [SerializeField]
        private Faction faction = null;

        [SerializeField]
        private Image icon = null;

        [SerializeField]
        private TextMeshProUGUI factionName = null;

        [SerializeField]
        private TextMeshProUGUI description = null;

        public Faction Faction { get => faction; set => faction = value; }

        public void AddFaction(Faction newFaction) {
            faction = newFaction;
            icon.sprite = this.faction.Icon;
            icon.color = Color.white;
            factionName.text = faction.DisplayName;
            //description.text = this.faction.GetSummary();
            description.text = faction.GetSummary();

        }

        public void ClearFaction() {
            icon.sprite = null;
            icon.color = new Color32(0, 0, 0, 0);
            factionName.text = string.Empty;
            description.text = string.Empty;
        }

        public void CommonSelect() {
            NewGamePanel.MyInstance.ShowFaction(this);
        }

        public void RawSelect() {
            CommonSelect();
        }

        public override void Select() {
            CommonSelect();
            base.Select();
        }



    }

}