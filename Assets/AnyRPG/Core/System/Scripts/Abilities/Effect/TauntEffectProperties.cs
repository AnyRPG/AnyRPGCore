using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class TauntEffectProperties : StatusEffectProperties {

        // extra threat from the taunt
        private float extraThreat = 100f;

        public void GetTauntEffectProperties(TauntEffect effect) {

            //extraThreat = effect.ExtraThreat;

            GetStatusEffectProperties(effect);
        }

        public override void CancelEffect(BaseCharacter targetCharacter) {
            //Debug.Log("MountEffect.CancelEffect(" + (targetCharacter != null ? targetCharacter.name : "null") + ")");
            if (targetCharacter != null && targetCharacter.CharacterCombat != null && targetCharacter.CharacterCombat.AggroTable != null) {
                targetCharacter.CharacterCombat.AggroTable.UnLockAgro();
            }
            base.CancelEffect(targetCharacter);
        }

        /*
        // bypass the creation of the status effect and just make its visual prefab
        public void RawCast(BaseCharacter source, GameObject target, GameObject originalTarget, AbilityEffectOutput abilityEffectInput) {
            base.Cast(source, target, originalTarget, abilityEffectInput);
        }
        */

        public override Dictionary<PrefabProfile, GameObject> Cast(IAbilityCaster source, Interactable target, Interactable originalTarget, AbilityEffectContext abilityEffectInput) {
            //Debug.Log("StatusEffect.Cast(" + source.name + ", " + (target? target.name : "null") + ")");
            if (!CanUseOn(target, source)) {
                //Debug.Log("StatusEffect.Cast(" + source.name + ", " + (target ? target.name : "null") + ") CANNOT USE RETURNING");
                return null;
            }
            Dictionary<PrefabProfile, GameObject> returnObjects = base.Cast(source, target, originalTarget, abilityEffectInput);
            // make ourselves the top threat in his threat table
            CharacterUnit targetCharacterUnit = CharacterUnit.GetCharacterUnit(target);
            if (targetCharacterUnit != null) {
                if (targetCharacterUnit.BaseCharacter != null && targetCharacterUnit.BaseCharacter.CharacterCombat != null && targetCharacterUnit.BaseCharacter.CharacterCombat.AggroTable != null) {
                    //Debug.Log("StatusEffect.Cast(" + source.name + ", " + (target ? target.name : "null") + ") CHARACTER COMBAT IS NOT NULL");
                    AggroNode AgroNode = targetCharacterUnit.BaseCharacter.CharacterCombat.AggroTable.TopAgroNode;
                    float usedAgroValue = 0f;
                    if (AgroNode != null) {
                        usedAgroValue = AgroNode.aggroValue;
                    }
                    if (source != null) {
                        source.AbilityManager.GenerateAgro(targetCharacterUnit, (int)(usedAgroValue + extraThreat));
                    }
                }

            }

            // override aggro checks

            return returnObjects;
        }





    }
}
