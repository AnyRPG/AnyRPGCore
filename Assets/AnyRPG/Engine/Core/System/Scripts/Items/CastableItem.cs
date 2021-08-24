using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    //[CreateAssetMenu(fileName = "New Scroll",menuName = "AnyRPG/Inventory/Items/Scroll", order = 1)]
    public abstract class CastableItem : Item, IUseable {

        [SerializeField]
        [ResourceSelector(resourceType = typeof(BaseAbility))]
        protected string abilityName = string.Empty;

        //[SerializeField]
        protected BaseAbility ability = null;

        // game manager references
        protected SystemAbilityController systemAbilityController = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemAbilityController = systemGameManager.SystemAbilityController;
        }

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
            if (playerManager.MyCharacter.CharacterAbilityManager.BeginAbility(ability)) {
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
            return systemAbilityController.StartCoroutine(actionButton.MonitorAbility(ability));
        }

        public override string GetSummary(ItemQuality usedItemQuality) {
            //Debug.Log(DisplayName + ".CastableItem.GetSummary()");
            return base.GetSummary(usedItemQuality) + GetCastableInformation() + GetCooldownString();
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

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);
            ability = null;
            if (abilityName != null) {
                BaseAbility baseAbility = systemDataFactory.GetResource<BaseAbility>(abilityName);
                if (baseAbility != null) {
                    ability = baseAbility;
                } else {
                    Debug.LogError("CastableItem.SetupScriptableObjects(): Could not find ability : " + abilityName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }
        }


    }

}