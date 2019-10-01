using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InstantEffectAbility : BaseAbility {

    public override void Cast(BaseCharacter source, GameObject target, Vector3 groundTarget) {
        //Debug.Log(abilityName + ".InstantEffectAbility.Cast(" + source.name + ", " + (target == null ? "null" : target.name) + ", " + groundTarget + ")");
        PerformAbilityEffects(source, target, groundTarget);
        base.Cast(source, target, groundTarget);
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
