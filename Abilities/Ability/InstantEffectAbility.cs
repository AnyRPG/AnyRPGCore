using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class InstantEffectAbility : BaseAbility {


        public override bool Cast(IAbilityCaster source, GameObject target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(MyName + ".InstantEffectAbility.Cast(" + source.name + ", " + (target == null ? "null" : target.name) + ", " + groundTarget + ")");

            // this code could lead to a situation where an instanteffect was allowed to perform its ability effects even if the wrong weapon was equipped.
            // need to change cast to a bool to pass that success or not up the casting stack
            bool castResult = base.Cast(source, target, abilityEffectContext);
            if (castResult) {
                PerformAbilityEffects(source, target, abilityEffectContext);
            }
            return castResult;
        }

        public override bool CanUseOn(GameObject target, IAbilityCaster source, bool performCooldownChecks = true, AbilityEffectContext abilityEffectContext = null) {
            //Debug.Log("DirectAbility.CanUseOn(" + (target != null ? target.name : "null") + ")");
            if (!base.CanUseOn(target, source, performCooldownChecks, abilityEffectContext)) {
                return false;
            }
            if (!CanSimultaneousCast) {
                if (source.PerformingAbility) {
                    return false;
                }
            }
            return true;
        }


    }

}