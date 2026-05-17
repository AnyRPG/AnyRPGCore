using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    //[CreateAssetMenu(fileName = "New Scroll",menuName = "AnyRPG/Inventory/Items/Scroll", order = 1)]
    public abstract class CastableItem : Item {

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

        public abstract AbilityProperties Ability { get; }

        // game manager references
        protected SystemAbilityController systemAbilityController = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemAbilityController = systemGameManager.SystemAbilityController;
        }

        public override bool HadSpecialIcon(ActionButton actionButton) {
            if (Ability != null) {
                Ability.UpdateActionButtonVisual(actionButton);
                return true;
            }
            return base.HadSpecialIcon(actionButton);
        }

        public override string GetDescription(ItemQuality usedItemQuality, int usedItemLevel) {
            //Debug.Log(DisplayName + ".CastableItem.GetSummary()");
            return base.GetDescription(usedItemQuality, usedItemLevel) + GetCastableItemDescription();
        }

        public string GetCastableItemDescription() {
            //Debug.Log(DisplayName + ".CastableItem.GetSummary()");
            return GetCastableInformation() + GetCooldownString();
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