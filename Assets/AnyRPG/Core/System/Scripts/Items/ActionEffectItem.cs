using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "ActionEffectItem", menuName = "AnyRPG/Inventory/Items/ActionEffectItem", order = 1)]
    public class ActionEffectItem : ActionItem {

        [Header("Effect")]

        [Tooltip("If true, use the inline effect instead of named effect")]
        [SerializeField]
        private bool useInlineEffect = false;

        [Tooltip("The resources to affect, and the amounts of the effects")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AbilityEffect))]
        private string effectName = string.Empty;

        [Tooltip("The resources to affect, and the amounts of the effects")]
        [SerializeReference]
        [SerializeReferenceButton]
        private AbilityEffectConfig inlineEffect = null;



        private AbilityEffectProperties abilityEffectProperties = null;

        public override bool Use() {
            //Debug.Log(DisplayName + ".ActionEffectItem.Use()");

            bool returnValue = base.Use();
            if (returnValue == false) {
                return false;
            }

            // perform heal effect
            abilityEffectProperties.Cast(playerManager.ActiveCharacter, playerManager.UnitController, null, null);

            return returnValue;

        }

        public override string GetCastableInformation() {
            //Debug.Log(DisplayName + ".PowerResourcePotion.GetCastableInformation()");
            string returnString = string.Empty;
            //if (ability != null) {
                returnString += string.Format("\n<color=green>Use: {0}</color>", description);
            //}
            return returnString;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            if (useInlineEffect) {
                abilityEffectProperties = inlineEffect.AbilityEffectProperties;
                abilityEffectProperties.SetupScriptableObjects(systemGameManager, this);
            } else {
                if (effectName != null && effectName != string.Empty) {
                    AbilityEffect tmpEffect = systemDataFactory.GetResource<AbilityEffect>(effectName);
                    if (tmpEffect != null) {
                        abilityEffectProperties = tmpEffect.AbilityEffectProperties;
                    } else {
                        Debug.LogError("PowerResourcePotion.SetupScriptableObjects(): Could not find ability effect : " + effectName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }



        }
    }

}