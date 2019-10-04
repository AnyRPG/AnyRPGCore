﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// NOTE: DIRECTEFFECT WILL CAST TICK AND COMPLETE, BUT NEVER HIT.  HIT MUST BE CAST BY PROJECTILE, AOE, OR CHANNELED
[CreateAssetMenu(fileName = "New DirectEffect", menuName = "Abilities/Effects/DirectEffect")]
public class DirectEffect : FixedLengthEffect {

    public override void Cast(BaseCharacter source, GameObject target, GameObject originalTarget, AbilityEffectOutput abilityEffectInput) {
        //Debug.Log(MyName + ".DirectEffect.Cast()");
        base.Cast(source, target, originalTarget, abilityEffectInput);
    }

    public override void CastTick(BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput) {
        //Debug.Log(abilityEffectName + ".DirectEffect.CastTick()");
        base.CastTick(source, target, abilityEffectInput);
        PerformAbilityTick(source, target, abilityEffectInput);
    }

    public override void CastComplete(BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput) {
        //Debug.Log(abilityEffectName + ".DirectEffect.CastComplete()");
        base.CastComplete(source, target, abilityEffectInput);
        PerformAbilityComplete(source, target, abilityEffectInput);
    }

}
