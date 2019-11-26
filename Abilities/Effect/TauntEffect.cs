using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Taunt Effect", menuName = "AnyRPG/Abilities/Effects/TauntEffect")]
    public class TauntEffect : StatusEffect {

        // extra threat from the taunt
        private float extraThreat = 100f;

        public override void CancelEffect(BaseCharacter targetCharacter) {
            //Debug.Log("MountEffect.CancelEffect(" + (targetCharacter != null ? targetCharacter.name : "null") + ")");
            if (targetCharacter != null && targetCharacter.MyCharacterCombat != null && targetCharacter.MyCharacterCombat.MyAggroTable != null) {
                targetCharacter.MyCharacterCombat.MyAggroTable.UnLockAgro();
            }
            base.CancelEffect(targetCharacter);
        }

        /*
        // bypass the creation of the status effect and just make its visual prefab
        public void RawCast(BaseCharacter source, GameObject target, GameObject originalTarget, AbilityEffectOutput abilityEffectInput) {
            base.Cast(source, target, originalTarget, abilityEffectInput);
        }
        */

        public override GameObject Cast(BaseCharacter source, GameObject target, GameObject originalTarget, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log("StatusEffect.Cast(" + source.name + ", " + (target? target.name : "null") + ")");
            if (!CanUseOn(target, source)) {
                //Debug.Log("StatusEffect.Cast(" + source.name + ", " + (target ? target.name : "null") + ") CANNOT USE RETURNING");
                return null;
            }
            GameObject returnObject = base.Cast(source, target, originalTarget, abilityEffectInput);
            // make ourselves the top threat in his threat table
            CharacterUnit targetCharacterUnit = target.GetComponent<CharacterUnit>();
            if (targetCharacterUnit != null) {
                if (targetCharacterUnit.MyCharacter != null && targetCharacterUnit.MyCharacter.MyCharacterCombat != null && targetCharacterUnit.MyCharacter.MyCharacterCombat.MyAggroTable != null) {
                    //Debug.Log("StatusEffect.Cast(" + source.name + ", " + (target ? target.name : "null") + ") CHARACTER COMBAT IS NOT NULL");
                    AggroNode AgroNode = targetCharacterUnit.MyCharacter.MyCharacterCombat.MyAggroTable.MyTopAgroNode;
                    float usedAgroValue = 0f;
                    if (AgroNode != null) {
                        usedAgroValue = AgroNode.aggroValue;
                    }
                    if (source != null && source.MyCharacterUnit != null) {
                        targetCharacterUnit.MyCharacter.MyCharacterCombat.MyAggroTable.AddToAggroTable(source.MyCharacterUnit, (int)(usedAgroValue + extraThreat));
                        AgroNode = targetCharacterUnit.MyCharacter.MyCharacterCombat.MyAggroTable.MyTopAgroNode;
                        //Debug.Log("StatusEffect.Cast(" + source.name + ", " + (target ? target.name : "null") + ") topNode agro value: " + AgroNode.aggroValue + "; target: " + AgroNode.aggroTarget.MyName);
                        targetCharacterUnit.MyCharacter.MyCharacterCombat.MyAggroTable.LockAgro();
                    }
                }

            }

            // override aggro checks

            return returnObject;
        }





    }
}
