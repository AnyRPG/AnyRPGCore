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

        public void AddAbility(IAbility ability) {
            this.ability = ability as BaseAbility;
            icon.sprite = this.ability.MyIcon;
            icon.color = Color.white;
            spellName.text = ability.DisplayName;
            description.text = ability.GetSummary();
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
                HandScript.MyInstance.TakeMoveable(ability);
            }
            if (eventData.button == PointerEventData.InputButton.Right) {
                //Debug.Log("AbilityButton.OnPointerClick(): right click");
                PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.BeginAbility(ability);
            }
        }
    }

}