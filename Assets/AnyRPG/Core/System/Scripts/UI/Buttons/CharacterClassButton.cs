using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class CharacterClassButton : TransparencyButton {

        [SerializeField]
        protected CharacterClass characterClass = null;

        [SerializeField]
        protected Image icon = null;

        [SerializeField]
        protected TextMeshProUGUI characterClassName = null;

        [SerializeField]
        protected TextMeshProUGUI description = null;

        public void AddCharacterClass(CharacterClass newCharacterClass) {
            characterClass = newCharacterClass;
            icon.sprite = this.characterClass.Icon;
            icon.color = Color.white;
            characterClassName.text = characterClass.DisplayName;
            //description.text = this.faction.GetSummary();
            description.text = characterClass.GetDescription();

        }

        public void ClearCharacterClass() {
            icon.sprite = null;
            icon.color = new Color32(0, 0, 0, 0);
            characterClassName.text = string.Empty;
            description.text = string.Empty;
        }


    }

}