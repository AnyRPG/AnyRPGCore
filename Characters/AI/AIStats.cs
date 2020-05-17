using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class AIStats : CharacterStats {

        public override void CreateLateSubscriptions() {
            base.CreateEventSubscriptions();
            if (baseCharacter != null && baseCharacter.AnimatedUnit != null && baseCharacter.AnimatedUnit.MyCharacterAnimator != null) {
                baseCharacter.AnimatedUnit.MyCharacterAnimator.OnReviveComplete += ReviveComplete;
            }
        }

        public override StatusEffectNode ApplyStatusEffect(StatusEffect statusEffect, IAbilityCaster sourceCharacter, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log(gameObject + ".AISats.ApplyStatusEffect()");
            StatusEffectNode _statusEffectNode = base.ApplyStatusEffect(statusEffect, sourceCharacter, abilityEffectInput);
            if (_statusEffectNode != null && _statusEffectNode.MyStatusEffect.MyControlTarget == true) {
                //Debug.Log(gameObject + ".AISats.ApplyStatusEffect() : disabling ai patrol");
                if ((baseCharacter.CharacterController as AIController).MyAiPatrol != null) {
                    (baseCharacter.CharacterController as AIController).MyAiPatrol.enabled = false;
                }
                (baseCharacter.CharacterController as AIController).ChangeState(new IdleState());
                ApplyControlEffects(sourceCharacter);

                sourceCharacter.AddPet(baseCharacter.CharacterUnit);

            }
            return _statusEffectNode;
        }

        public void ApplyControlEffects(IAbilityCaster source) {
            (baseCharacter.CharacterController as AIController).ApplyControlEffects((source as CharacterAbilityManager).BaseCharacter);
        }

    }

}