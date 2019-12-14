using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class CharacterClassButton : TransparencyButton {

        [SerializeField]
        private CharacterClass characterClass;

        [SerializeField]
        private Image icon;

        [SerializeField]
        private Text characterClassName;

        [SerializeField]
        private Text description;

        public void AddCharacterClass(CharacterClass newCharacterClass) {
            characterClass = newCharacterClass;
            icon.sprite = this.characterClass.MyIcon;
            icon.color = Color.white;
            characterClassName.text = characterClass.MyName;
            //description.text = this.faction.GetSummary();
            description.text = characterClass.GetSummary();

        }

        public void ClearCharacterClass() {
            icon.sprite = null;
            icon.color = new Color32(0, 0, 0, 0);
            characterClassName.text = string.Empty;
            description.text = string.Empty;
        }


    }

}