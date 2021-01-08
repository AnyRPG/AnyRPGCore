using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class InstantEffectAbility : BaseAbility {

        public override bool Cast(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {

            // this code could lead to a situation where an instanteffect was allowed to perform its ability effects even if the wrong weapon was equipped.
            // need to change cast to a bool to pass that success or not up the casting stack
            bool castResult = base.Cast(source, target, abilityEffectContext);
            if (castResult) {
                PerformAbilityEffects(source, target, abilityEffectContext);
            }
            return castResult;
        }

        public override bool CanUseOn(Interactable target, IAbilityCaster source, bool performCooldownChecks = true, AbilityEffectContext abilityEffectContext = null, bool playerInitiated = false, bool performRangeChecks = true) {
            //Debug.Log("DirectAbility.CanUseOn(" + (target != null ? target.name : "null") + ")");
            if (!base.CanUseOn(target, source, performCooldownChecks, abilityEffectContext, playerInitiated, performRangeChecks)) {
                return false;
            }
            if (!CanSimultaneousCast) {
                if (source.AbilityManager.PerformingAbility) {
                    if (playerInitiated) {
                        source.AbilityManager.ReceiveCombatMessage("Cannot cast " + resourceName + ". another case is in progress");
                    }
                    return false;
                }
            }
            return true;
        }


    }

}