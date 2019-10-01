using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Direct Ability", menuName = "Abilities/DirectAbility")]
public class DirectAbility : InstantEffectAbility {

    public override void Cast(BaseCharacter source, GameObject target, Vector3 groundTarget) {
        //Debug.Log("DirectAbility.Cast(" + (target ? target.name : "null") + ")");
        base.Cast(source, target, groundTarget);
    }

    public override bool CanUseOn(GameObject target, BaseCharacter source) {
        //Debug.Log("DirectAbility.CanUseOn(" + (target ? target.name : "null") + ")");
        if (!base.CanUseOn(target, source)) {
            return false;
        }
        return true;
    }

    public override void StartCasting(BaseCharacter source) {
        //Debug.Log("DirectAbilty.OnCastStart()");
        base.StartCasting(source);
    }

    public override void OnCastTimeChanged(float currentCastTime, BaseCharacter source, GameObject target) {
        //Debug.Log("DirectAbility.OnCastTimeChanged()");
        base.OnCastTimeChanged(currentCastTime, source, target);
    }

}
