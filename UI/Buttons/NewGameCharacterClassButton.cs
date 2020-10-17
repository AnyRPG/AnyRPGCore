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
        private CharacterClass characterClass = null;

        [SerializeField]
        private Image icon = null;

        [SerializeField]
        private TextMeshProUGUI characterClassName = null;

        [SerializeField]
        private TextMeshProUGUI description = null;

        public CharacterClass CharacterClass { get => characterClass; set => characterClass = value; }

        public void AddCharacterClass(CharacterClass newCharacterClass) {
            characterClass = newCharacterClass;
            icon.sprite = this.characterClass.MyIcon;
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
            NewGamePanel.MyInstance.ShowCharacterClass(this);
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