using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    //[CreateAssetMenu(fileName = "New Scroll",menuName = "AnyRPG/Inventory/Items/Scroll", order = 1)]
    public abstract class CastableItem : Item, IUseable {

        [SerializeField]
        protected string abilityName = string.Empty;

        //[SerializeField]
        protected BaseAbility ability = null;

        public override bool Use() {
            //Debug.Log("CastableItem.Use()");
            if (ability == null) {
                Debug.LogError(DisplayName + ".CastableItem.Use(): ability is null.  Please set it in the inspector!");
                return false;
            }
            bool returnValue = base.Use();
            if (returnValue == false) {
                return false;
            }
            if (PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.BeginAbility(ability)) {
                Remove();
            }
            return returnValue;
        }

        public override bool HadSpecialIcon(ActionButton actionButton) {
            if (ability != null) {
                ability.UpdateActionButtonVisual(actionButton);
                return true;
            }
            return base.HadSpecialIcon(actionButton);
        }

        public override Coroutine ChooseMonitorCoroutine(ActionButton actionButton) {
            //Debug.Log(DisplayName + ".CastableItem.ChooseMonitorCoroutine()");
            if (ability == null) {
                return null;
            }
            return SystemAbilityManager.MyInstance.StartCoroutine(actionButton.MonitorAbility(ability));
        }

        public override string GetSummary() {

            return base.GetSummary() + GetCastableInformation() + GetCooldownString();
        }

        public virtual string GetCastableInformation() {
            return string.Empty;
        }

        public string GetCooldownString() {
            string coolDownString = string.Empty;
            if (ability != null) {
                coolDownString = ability.GetCooldownString();
            }
            return coolDownString;
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            ability = null;
            if (abilityName != null) {
                BaseAbility baseAbility = SystemAbilityManager.MyInstance.GetResource(abilityName);
                if (baseAbility != null) {
                    ability = baseAbility;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find ability : " + abilityName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }
        }


    }

}