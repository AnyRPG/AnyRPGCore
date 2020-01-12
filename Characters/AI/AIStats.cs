using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class AIStats : CharacterStats {

        public override void CreateEventSubscriptions() {
            if (baseCharacter.MyAnimatedUnit.MyCharacterAnimator != null) {
                baseCharacter.MyAnimatedUnit.MyCharacterAnimator.OnReviveComplete += ReviveComplete;
            }
        }

        public override StatusEffectNode ApplyStatusEffect(StatusEffect statusEffect, BaseCharacter sourceCharacter, CharacterUnit target, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log(gameObject + ".AISats.ApplyStatusEffect()");
            StatusEffectNode _statusEffectNode = base.ApplyStatusEffect(statusEffect, sourceCharacter, target, abilityEffectInput);
            if (_statusEffectNode != null && _statusEffectNode.MyStatusEffect.MyControlTarget == true) {
                //Debug.Log(gameObject + ".AISats.ApplyStatusEffect() : disabling ai patrol");
                if ((baseCharacter.MyCharacterController as AIController).MyAiPatrol != null) {
                    (baseCharacter.MyCharacterController as AIController).MyAiPatrol.enabled = false;
                }
                (baseCharacter.MyCharacterController as AIController).ChangeState(new IdleState());
                ApplyControlEffects(sourceCharacter);
                if (sourceCharacter.MyCharacterPetManager != null && target.MyCharacter != null && target.MyCharacter.MyUnitProfile != null) {
                    sourceCharacter.MyCharacterPetManager.AddPet(target.MyCharacter.MyUnitProfile);
                }

            }
            return _statusEffectNode;
        }

        public void ApplyControlEffects(BaseCharacter source) {
            (baseCharacter.MyCharacterController as AIController).ApplyControlEffects(source);
        }

    }

}