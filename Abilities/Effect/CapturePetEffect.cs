using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Capture Pet Effect", menuName = "AnyRPG/Abilities/Effects/CapturePetEffect")]
    public class CapturePetEffect : InstantEffect {

        [SerializeField]
        protected List<string> unitTypeRestrictions = new List<string>();

        protected List<UnitType> unitTypeRestrictionList = new List<UnitType>();

        public override bool CanUseOn(GameObject target, BaseCharacter sourceCharacter) {
            if (unitTypeRestrictionList != null && unitTypeRestrictionList.Count > 0) {
                BaseCharacter targetCharacter = target.GetComponent<BaseCharacter>();
                if (targetCharacter == null) {
                    // if there is no target character, it cannot possibly match a unit type
                    Debug.Log(MyName + ".CapturePetEffect.CanUseOn(): no target character");
                    return false;
                }
                if (targetCharacter.MyUnitType == null || !unitTypeRestrictionList.Contains(targetCharacter.MyUnitType)) {
                    Debug.Log(MyName + ".CapturePetEffect.CanUseOn(): pet was not allowed by your restrictions ");
                    return false;
                }
                if (targetCharacter.MyUnitProfile == null || targetCharacter.MyUnitProfile.MyIsPet == false) {
                    // has to be the right unit type plus needs to be capturable specifically
                    Debug.Log(MyName + ".CapturePetEffect.CanUseOn(): pet was not capturable ");
                    return false;
                }
            }
            bool returnValue = base.CanUseOn(target, sourceCharacter);
            Debug.Log(MyName + ".CapturePetEffect.CanUseOn(): returning: " + returnValue);
            return returnValue;
        }

        public override Dictionary<PrefabProfile, GameObject> Cast(BaseCharacter source, GameObject target, GameObject originalTarget, AbilityEffectOutput abilityEffectInput) {
            if (target == null) {
                Debug.Log(MyName + ".CapturePetEffect.Cast(): target is null, returning");
                return null;
            }

            Dictionary<PrefabProfile, GameObject> returnValue = base.Cast(source, target, originalTarget, abilityEffectInput);

            BaseCharacter targetCharacter = target.GetComponent<BaseCharacter>();
            if (targetCharacter != null && targetCharacter.MyCharacterStats != null && targetCharacter.MyCharacterStats is AIStats) {
                Debug.Log(MyName + ".CapturePetEffect.Cast(): applying control effects");
                (targetCharacter.MyCharacterStats as AIStats).ApplyControlEffects(source);
            }
            
            if (source.MyCharacterPetManager != null && targetCharacter != null && targetCharacter.MyUnitProfile != null) {
                Debug.Log(MyName + ".CapturePetEffect.Cast(): adding to pet manager");
                source.MyCharacterPetManager.AddPet(targetCharacter.MyUnitProfile);
                source.MyCharacterPetManager.MyActiveUnitProfiles.Add(targetCharacter.MyUnitProfile, target);
            }

            return returnValue;
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            if (unitTypeRestrictions != null && unitTypeRestrictions.Count > 0) {
                foreach (string unitTypeRestriction in unitTypeRestrictions) {
                    //Debug.Log(MyName + ".CapturePetEffect.SetupScriptableObjects(): looping through restrictions: " + unitTypeRestriction);
                    UnitType tmpUnitType = SystemUnitTypeManager.MyInstance.GetResource(unitTypeRestriction);
                    if (tmpUnitType != null) {
                        unitTypeRestrictionList.Add(tmpUnitType);
                    } else {
                        Debug.LogError("CapturePetEffect.SetupScriptableObjects(): Could not find unitTypeRestriction: " + unitTypeRestriction + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                    }
                }
            }
        }

    }
}