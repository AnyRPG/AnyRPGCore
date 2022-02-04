using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class PetControlEffectProperties : StatusEffectProperties {

        [SerializeField]
        [ResourceSelector(resourceType = typeof(SummonEffect))]
        private List<string> petEffectNames = new List<string>();

        private List<SummonEffect> petEffectList = new List<SummonEffect>();

        public override void SetupScriptableObjects(SystemGameManager systemGameManager, string displayName) {
            base.SetupScriptableObjects(systemGameManager, displayName);

            if (petEffectNames != null) {
                foreach (string petEffectName in petEffectNames) {
                    AbilityEffect abilityEffect = systemDataFactory.GetResource<AbilityEffect>(petEffectName);
                    if (abilityEffect != null && ((abilityEffect as SummonEffect) is SummonEffect)) {
                        petEffectList.Add(abilityEffect as SummonEffect);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability effect : " + petEffectName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }


        }

    }
}
