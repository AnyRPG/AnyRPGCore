using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class NewGameAbilityButton : TransparencyButton {

        private ILearnable ability = null;

        [SerializeField]
        private Image icon = null;

        [SerializeField]
        private TextMeshProUGUI spellName = null;

        [SerializeField]
        private TextMeshProUGUI description = null;

        public void AddAbility(ILearnable ability) {
            this.ability = ability;
            icon.sprite = this.ability.Icon;
            icon.color = Color.white;
            spellName.text = ability.DisplayName;
            description.text = ability.GetShortDescription() + "\nLearned at level " + Mathf.Clamp(ability.RequiredLevel, 1, 1000);
        }

        public void ClearAbility() {
            icon.sprite = null;
            icon.color = new Color32(0, 0, 0, 0);
            spellName.text = string.Empty;
            description.text = string.Empty;
        }

    }

}