using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {

    public class NewGameRaceButton : HighlightButton {

        [SerializeField]
        protected CharacterRace characterRace = null;

        [SerializeField]
        protected Image icon = null;

        [SerializeField]
        protected TextMeshProUGUI characterRaceName = null;

        [SerializeField]
        protected TextMeshProUGUI description = null;

        // game manager references
        protected NewGameManager newGameManager = null;

        public CharacterRace CharacterRace { get => characterRace; set => characterRace = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            newGameManager = systemGameManager.NewGameManager;
        }

        public void AddCharacterRace(CharacterRace characterRace) {
            this.characterRace = characterRace;
            icon.sprite = this.characterRace.Icon;
            icon.color = Color.white;
            characterRaceName.text = this.characterRace.DisplayName;
            //description.text = this.faction.GetSummary();
            description.text = this.characterRace.GetDescription();

        }

        public void ClearCharacterRace() {
            icon.sprite = null;
            icon.color = new Color32(0, 0, 0, 0);
            characterRaceName.text = string.Empty;
            description.text = string.Empty;
        }

        public void CommonSelect() {
            newGameManager.ChooseNewCharacterRace(characterRace);
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