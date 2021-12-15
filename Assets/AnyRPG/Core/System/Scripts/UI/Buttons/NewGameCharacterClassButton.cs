using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class NewGameCharacterClassButton : HighlightButton {

        [SerializeField]
        protected CharacterClass characterClass = null;

        [SerializeField]
        protected Image icon = null;

        [SerializeField]
        protected TextMeshProUGUI characterClassName = null;

        [SerializeField]
        protected TextMeshProUGUI description = null;

        // game manager references
        protected NewGameManager newGameManager = null;

        public CharacterClass CharacterClass { get => characterClass; set => characterClass = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            newGameManager = systemGameManager.NewGameManager;
        }

        public void AddCharacterClass(CharacterClass newCharacterClass) {
            characterClass = newCharacterClass;
            icon.sprite = this.characterClass.Icon;
            icon.color = Color.white;
            characterClassName.text = characterClass.DisplayName;
            //description.text = this.faction.GetSummary();
            description.text = characterClass.GetSummary();

        }

        public void ClearCharacterClass() {
            icon.sprite = null;
            icon.color = new Color32(0, 0, 0, 0);
            characterClassName.text = string.Empty;
            description.text = string.Empty;
        }

        public void CommonSelect() {
            newGameManager.SetCharacterClass(characterClass);
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