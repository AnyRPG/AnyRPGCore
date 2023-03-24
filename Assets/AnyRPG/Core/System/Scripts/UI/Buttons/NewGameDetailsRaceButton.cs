using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class NewGameDetailsRaceButton : HighlightButton {

        [SerializeField]
        protected CharacterRace race = null;

        [SerializeField]
        protected Image icon = null;

        [SerializeField]
        protected TextMeshProUGUI raceName = null;

        [SerializeField]
        protected TextMeshProUGUI description = null;

        // game manager references
        protected NewGameManager newGameManager = null;

        public CharacterRace Race { get => race; set => race = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            newGameManager = systemGameManager.NewGameManager;
        }

        public void AddRace(CharacterRace newRace) {
            race = newRace;
            icon.sprite = this.race.Icon;
            icon.color = Color.white;
            raceName.text = race.DisplayName;
            //description.text = this.faction.GetSummary();
            description.text = race.GetDescription();

        }

        public void ClearRace() {
            icon.sprite = null;
            icon.color = new Color32(0, 0, 0, 0);
            raceName.text = string.Empty;
            description.text = string.Empty;
        }


    }

}