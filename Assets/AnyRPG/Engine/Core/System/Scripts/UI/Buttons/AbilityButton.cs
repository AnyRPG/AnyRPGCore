using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class AbilityButton : TransparencyButton, IPointerClickHandler {

        [SerializeField]
        private BaseAbility ability = null;

        [SerializeField]
        private Image icon = null;

        [SerializeField]
        private TextMeshProUGUI spellName = null;

        [SerializeField]
        private TextMeshProUGUI description = null;

        public void AddAbility(BaseAbility ability) {
            this.ability = ability;
            icon.sprite = this.ability.Icon;
            icon.color = Color.white;
            spellName.text = ability.DisplayName;
            description.text = ability.GetSummary();
            description.text = ability.GetShortDescription();
        }

        public void ClearAbility() {
            icon.sprite = null;
            icon.color = new Color32(0, 0, 0, 0);
            spellName.text = string.Empty;
            description.text = string.Empty;
        }

        public void OnPointerClick(PointerEventData eventData) {
            //Debug.Log("AbilityButton.OnPointerClick()");
            if (eventData.button == PointerEventData.InputButton.Left) {
                //Debug.Log("AbilityButton.OnPointerClick(): left click");
                HandScript.Instance.TakeMoveable(ability);
            }
            if (eventData.button == PointerEventData.InputButton.Right) {
                //Debug.Log("AbilityButton.OnPointerClick(): right click");
                PlayerManager.Instance.MyCharacter.CharacterAbilityManager.BeginAbility(ability);
            }
        }
    }

}