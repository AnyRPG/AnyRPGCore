using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class AbilityButton : TransparencyButton {

        [SerializeField]
        protected BaseAbility ability = null;

        [SerializeField]
        protected Image icon = null;

        [SerializeField]
        protected TextMeshProUGUI spellName = null;

        [SerializeField]
        protected TextMeshProUGUI description = null;

        // game manager references
        protected PlayerManager playerManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            playerManager = systemGameManager.PlayerManager;
        }

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

        public override void OnPointerClick(PointerEventData eventData) {
            //Debug.Log("AbilityButton.OnPointerClick()");
            base.OnPointerClick(eventData);
            if (eventData.button == PointerEventData.InputButton.Left) {
                //Debug.Log("AbilityButton.OnPointerClick(): left click");
                uIManager.HandScript.TakeMoveable(ability);
            }
            if (eventData.button == PointerEventData.InputButton.Right) {
                //Debug.Log("AbilityButton.OnPointerClick(): right click");
                playerManager.MyCharacter.CharacterAbilityManager.BeginAbility(ability);
            }
        }

        public override void Select() {
            Debug.Log("AbilityButton.Select()");
            base.Select();
            if (owner != null) {
                owner.SetControllerHints("Cast", "Add To Action Bars", "", "");
            }
        }

        public override void DeSelect() {
            Debug.Log("AbilityButton.DeSelect()");
            base.DeSelect();
            if (owner != null) {
                owner.HideControllerHints();
            }
        }

        public override void Accept() {
            Debug.Log("AbilityButton.Accept()");
            base.Accept();
            if (ability.CanCast(playerManager.MyCharacter, true)) {
                playerManager.MyCharacter.CharacterAbilityManager.BeginAbility(ability);
            }
        }

        public override void JoystickButton2() {
            Debug.Log("AbilityButton.JoystickButton2()");
            base.JoystickButton2();
        }

    }

}