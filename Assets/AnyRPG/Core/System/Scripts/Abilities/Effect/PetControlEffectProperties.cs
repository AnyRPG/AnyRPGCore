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

        private List<SummonEffectProperties> petEffectList = new List<SummonEffectProperties>();

        /*
        public void GetPetControlEffectProperties(PetControlEffect effect) {

            petEffectNames = effect.PetEffectNames;

            GetStatusEffectProperties(effect);
        }
        */

        public override void CastTick(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(DisplayName + ".PetControlEffect.CastTick()");
            base.CastTick(source, target, abilityEffectContext);

            CheckPetSpawn(source, target, abilityEffectContext);
        }

        public void CheckPetSpawn(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(DisplayName + ".PetEffect.CheckPetSpawn()");
            CharacterPetManager characterPetManager = null;
            if ((source as BaseCharacter) is BaseCharacter) {
                characterPetManager = (source as BaseCharacter).CharacterPetManager;
            } else {
                //Debug.Log(DisplayName + ".PetEffect.CheckPetSpawn(): source is not basecharacter");
                return;
            }

            List<AbilityEffectProperties> castList = new List<AbilityEffectProperties>();
            foreach (SummonEffectProperties petEffect in petEffectList) {
                if (SystemDataFactory.MatchResource(petEffect.DisplayName, DisplayName)) {
                    Debug.LogError(DisplayName + ".PerformAbilityEffects(): circular reference detected.  Tried to cast self.  CHECK INSPECTOR AND FIX ABILITY EFFECT CONFIGURATION!!!");
                } else {
                    //Debug.Log(DisplayName + ".PetEffect.CheckPetSpawn(): adding to cast list");
                    if (petEffect.UnitProfile != null
                        && characterPetManager.ActiveUnitProfiles.ContainsKey(petEffect.UnitProfile) == false) {
                        //Debug.Log(DisplayName + ".PetEffect.CheckPetSpawn(): adding cast:" + petEffect.DisplayName);
                        castList.Add(petEffect);
                    }
                }
            }
            if (castList.Count > 0) {
                //Debug.Log(DisplayName + ".PetEffect.CheckPetSpawn(): castlist.count: " + castList.Count);
                // commented next line because it appeared to have no references
                //Dictionary<PrefabProfile, GameObject> rawObjectList = PerformAbilityEffects(source, target, abilityEffectInput, castList);
                PerformAbilityEffects(source, target, abilityEffectInput, castList);
            }

        }

        public override void CancelEffect(BaseCharacter targetCharacter) {
            //Debug.Log("PetControlEffect.CancelEffect(" + (targetCharacter != null ? targetCharacter.name : "null") + ")");

            foreach (SummonEffectProperties petEffect in petEffectList) {
                //Debug.Log(DisplayName + ".PetEffect.CheckPetSpawn(): adding to cast list");
                if (petEffect.UnitProfile != null
                    && targetCharacter.CharacterPetManager.ActiveUnitProfiles.ContainsKey(petEffect.UnitProfile) == true) {
                    targetCharacter.CharacterPetManager.DespawnPet(petEffect.UnitProfile);
                }
            }

            base.CancelEffect(targetCharacter);
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager, IDescribable describable) {
            base.SetupScriptableObjects(systemGameManager, describable);

            if (petEffectNames != null) {
                foreach (string petEffectName in petEffectNames) {
                    AbilityEffect abilityEffect = systemDataFactory.GetResource<AbilityEffect>(petEffectName);
                    if (abilityEffect != null && ((abilityEffect as SummonEffect) is SummonEffect)) {
                        petEffectList.Add(abilityEffect.AbilityEffectProperties as SummonEffectProperties);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability effect : " + petEffectName + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
                    }
                }
            }


        }

    }
}
