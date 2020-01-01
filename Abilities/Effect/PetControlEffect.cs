using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Pet Control Effect", menuName = "AnyRPG/Abilities/Effects/PetControlEffect")]
    public class PetControlEffect : StatusEffect {

        [SerializeField]
        private List<string> petEffectNames = new List<string>();

        /*
        [SerializeField]
        private List<string> petEffectList = new List<string>();
        */

        private List<AbilityEffect> realPetEffectList = new List<AbilityEffect>();

        private List<CharacterUnit> petUnits = new List<CharacterUnit>();

        public override void CastTick(BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log(MyName + ".PetEffect.CastTick()");
            base.CastTick(source, target, abilityEffectInput);
            CheckPetSpawn(source, target, abilityEffectInput);
        }

        public void CheckPetSpawn(BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log(MyName + ".PetEffect.CheckPetSpawn()");
            List<CharacterUnit> unitsToRemove = new List<CharacterUnit>();
            foreach (CharacterUnit characterUnit in petUnits) {
                //if (characterUnit != null) {
                    if (characterUnit.MyCharacter.MyCharacterStats.IsAlive == false) {
                    //Debug.Log(MyName + ".PetEffect.CheckPetSpawn(): ADDING DEAD PET TO REMOVE LIST");
                    unitsToRemove.Add(characterUnit);
                    }
                //}
            }
            foreach (CharacterUnit characterUnit in unitsToRemove) {
                //Debug.Log(MyName + ".PetEffect.CheckPetSpawn(): REMOVING DEAD PET");
                petUnits.Remove(characterUnit);
            }
            if (petUnits.Count == 0) {
                //Debug.Log(MyName + ".PetEffect.CheckPetSpawn(): SPAWNING PETS");
                // spawn pet
                List<AbilityEffect> castList = new List<AbilityEffect>();
                foreach (AbilityEffect petEffect in realPetEffectList) {
                    //if (source.MyCharacterAbilityManager.HasAbility(petAbilityName)) {
                        if (SystemResourceManager.MatchResource(petEffect.MyName, MyName)) {
                            Debug.LogError(MyName + ".PerformAbilityEffects(): circular reference detected.  Tried to cast self.  CHECK INSPECTOR AND FIX ABILITY EFFECT CONFIGURATION!!!");
                        } else {
                            //Debug.Log(MyName + ".PetEffect.CheckPetSpawn(): adding to cast list");
                            castList.Add(petEffect);
                        }
                    //}
                }
                if (castList.Count > 0) {
                    //Debug.Log(MyName + ".PetEffect.CheckPetSpawn(): castlist.count: " + castList.Count);
                    Dictionary<PrefabProfile, GameObject> rawObjectList = PerformAbilityEffects(source, target, abilityEffectInput, castList);
                    foreach (KeyValuePair<PrefabProfile, GameObject> tmpObject in rawObjectList) {
                        //Debug.Log(MyName + ".PetEffect.CheckPetSpawn(): LOOPING THROUGH RAW OBJECT LIST ");
                        CharacterUnit _characterUnit = tmpObject.Value.GetComponent<CharacterUnit>();
                        if (_characterUnit != null) {
                            //Debug.Log(MyName + ".PetEffect.CheckPetSpawn(): ADDING PET TO UNIT LIST");
                            petUnits.Add(_characterUnit);
                        }
                    }
                }
            }
        }

        public override void CancelEffect(BaseCharacter targetCharacter) {
            //Debug.Log("MountEffect.CancelEffect(" + (targetCharacter != null ? targetCharacter.name : "null") + ")");
            foreach (CharacterUnit characterUnit in petUnits) {
                if (characterUnit != null) {
                    characterUnit.Despawn(0, false, true);
                }
            }

            base.CancelEffect(targetCharacter);
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            realPetEffectList = new List<AbilityEffect>();
            if (petEffectNames != null) {
                foreach (string petEffectName in petEffectNames) {
                    AbilityEffect abilityEffect = SystemAbilityEffectManager.MyInstance.GetResource(petEffectName);
                    if (abilityEffect != null) {
                        realPetEffectList.Add(abilityEffect);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability effect : " + petEffectName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                    }
                }
            }


        }

    }
}
