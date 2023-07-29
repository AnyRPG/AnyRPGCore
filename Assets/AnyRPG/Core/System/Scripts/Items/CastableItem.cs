using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    //[CreateAssetMenu(fileName = "New Scroll",menuName = "AnyRPG/Inventory/Items/Scroll", order = 1)]
    public abstract class CastableItem : Item, IUseable {

        /*
        [SerializeField]
        [ResourceSelector(resourceType = typeof(BaseAbility))]
        protected string abilityName = string.Empty;
        */

        [Header("Castable item")]

        [Tooltip("The Use: hint that will appear in the tooltip")]
        [TextArea(5, 10)]
        [SerializeField]
        private string toolTip = string.Empty;

        public abstract BaseAbilityProperties Ability { get; }

        // game manager references
        protected SystemAbilityController systemAbilityController = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemAbilityController = systemGameManager.SystemAbilityController;
        }

        public override bool Use() {
            //Debug.Log("CastableItem.Use()");
            if (Ability == null) {
                Debug.LogError(ResourceName + ".CastableItem.Use(): ability is null.  Please set it in the inspector!");
                return false;
            }
            bool returnValue = base.Use();
            if (returnValue == false) {
                return false;
            }
            if (playerManager.UnitController.CharacterAbilityManager.BeginAbility(Ability)) {
                Remove();
            }
            return returnValue;
        }

        public override bool HadSpecialIcon(ActionButton actionButton) {
            if (Ability != null) {
                Ability.UpdateActionButtonVisual(actionButton);
                return true;
            }
            return base.HadSpecialIcon(actionButton);
        }

        public override Coroutine ChooseMonitorCoroutine(ActionButton actionButton) {
            //Debug.Log(DisplayName + ".CastableItem.ChooseMonitorCoroutine()");
            if (Ability == null) {
                return null;
            }
            return systemAbilityController.StartCoroutine(actionButton.MonitorAbility(Ability.DisplayName));
        }

        public override string GetDescription(ItemQuality usedItemQuality) {
            //Debug.Log(DisplayName + ".CastableItem.GetSummary()");
            return base.GetDescription(usedItemQuality) + GetCastableInformation() + GetCooldownString();
        }

        public virtual string GetCastableInformation() {
            string returnString = string.Empty;
            if (Ability != null) {
                returnString += string.Format("\n\n<color=green>Use: {0}</color>", toolTip);
            }
            return returnString;
        }

        public string GetCooldownString() {
            string coolDownString = string.Empty;
            if (Ability != null) {
                coolDownString = Ability.GetCooldownString();
            }
            return coolDownString;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);
            /*
            ability = null;
            if (abilityName != null) {
                BaseAbility baseAbility = systemDataFactory.GetResource<BaseAbility>(abilityName);
                if (baseAbility != null) {
                    ability = baseAbility.AbilityProperties;
                } else {
                    Debug.LogError("CastableItem.SetupScriptableObjects(): Could not find ability : " + abilityName + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
                }
            }
            */
        }


    }

}