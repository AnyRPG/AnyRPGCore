using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
public class AIStats : CharacterStats {

    protected override void Awake() {
        base.Awake();
        baseCharacter = GetComponent<AICharacter>() as ICharacter;
    }

    public override void Start() {
        base.Start();
        if (baseCharacter.MyCharacterUnit.MyCharacterAnimator != null) {
            baseCharacter.MyCharacterUnit.MyCharacterAnimator.OnReviveComplete += ReviveComplete;
        }
    }

    public override StatusEffectNode ApplyStatusEffect(StatusEffect statusEffect, BaseCharacter source, CharacterUnit target, AbilityEffectOutput abilityEffectInput) {
        //Debug.Log("AISats.ApplyStatusEffect()");
        if (statusEffect == null) {
            Debug.Log("AIStats.ApplyStatusEffect(): statusEffect is null!");
        }
        StatusEffectNode _statusEffectNode = base.ApplyStatusEffect(statusEffect, source, target, abilityEffectInput);
        if (_statusEffectNode != null && _statusEffectNode.MyStatusEffect.MyControlTarget == true) {
            ApplyControlEffects(source);
        }
        return _statusEffectNode;
    }

    public void ApplyControlEffects (BaseCharacter source) {
        (baseCharacter.MyCharacterController as AIController).ApplyControlEffects(source);
    }

}

}