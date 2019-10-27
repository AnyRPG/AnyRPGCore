using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class InstantEffectAbility : BaseAbility {

        public override bool Cast(BaseCharacter source, GameObject target, Vector3 groundTarget) {
            //Debug.Log(MyName + ".InstantEffectAbility.Cast(" + source.name + ", " + (target == null ? "null" : target.name) + ", " + groundTarget + ")");

            // this code could lead to a situation where an instanteffect was allowed to perform its ability effects even if the wrong weapon was equipped.
            // need to change cast to a bool to pass that success or not up the casting stack
            bool castResult = base.Cast(source, target, groundTarget);
            if (castResult) {
                // these effects may include things which do not do damage (status debuffs) or take a while to do damage (slow ticks).
                // We need to pull enemies into combat at this point
                if (target != null) {
                    CharacterUnit targetCharacterUnit = target.GetComponent<CharacterUnit>();
                    if (targetCharacterUnit != null && targetCharacterUnit.MyBaseCharacter != null) {
                        if (Faction.RelationWith(targetCharacterUnit.MyBaseCharacter, source) <= -1) {
                            if (targetCharacterUnit.MyBaseCharacter.MyCharacterCombat != null) {
                                // agro includes a liveness check, so casting necromancy on a dead enemy unit should not pull it into combat with us if we haven't applied a faction or master control buff yet
                                targetCharacterUnit.MyBaseCharacter.MyCharacterController.Agro(source.MyCharacterUnit);
                            }
                        }
                    }
                }
                PerformAbilityEffects(source, target, groundTarget);
            }
            return castResult;
        }

        public override bool CanUseOn(GameObject target, BaseCharacter source) {
            //Debug.Log("DirectAbility.CanUseOn(" + (target ? target.name : "null") + ")");
            if (!base.CanUseOn(target, source)) {
                return false;
            }
            if (source.MyCharacterAbilityManager.MyWaitingForAnimatedAbility == true && !MyCanSimultaneousCast) {
                //Debug.Log("DirectAbility.CanUseOn(" + (target ? target.name : "null") + "): waiting for an animated ability. can't cast");
                return false;
            }
            if (source.MyCharacterAbilityManager.MyIsCasting && !MyCanSimultaneousCast) {
                //Debug.Log("DirectAbility.CanUseOn(" + (target ? target.name : "null") + "): already casting. can't cast");
                return false;
            }
            return true;
        }



    }

}