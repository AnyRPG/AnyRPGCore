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
        protected Faction faction = null;

        [SerializeField]
        protected Image icon = null;

        [SerializeField]
        protected TextMeshProUGUI factionName = null;

        [SerializeField]
        protected TextMeshProUGUI description = null;

        // game manager references
        protected NewGameManager newGameManager = null;

        public Faction Faction { get => faction; set => faction = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            newGameManager = systemGameManager.NewGameManager;
        }

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
            newGameManager.ShowFaction(this);
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