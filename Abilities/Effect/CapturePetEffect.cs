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

        public override bool CanUseOn(Interactable target, IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectContext = null) {
            if (unitTypeRestrictionList != null && unitTypeRestrictionList.Count > 0) {
                BaseCharacter targetCharacter = target.GetComponent<BaseCharacter>();
                if (targetCharacter == null) {
                    // if there is no target character, it cannot possibly match a unit type
                    Debug.Log(DisplayName + ".CapturePetEffect.CanUseOn(): no target character");
                    return false;
                }
                if (targetCharacter.UnitType == null || !unitTypeRestrictionList.Contains(targetCharacter.UnitType)) {
                    //Debug.Log(MyDisplayName + ".CapturePetEffect.CanUseOn(): pet was not allowed by your restrictions ");
                    return false;
                }
                if (target == null || target.UnitController == null ||  target.UnitController.UnitProfile == null || target.UnitController.UnitProfile.IsPet == false) {
                    // has to be the right unit type plus needs to be capturable specifically
                    Debug.Log(DisplayName + ".CapturePetEffect.CanUseOn(): pet was not capturable ");
                    return false;
                }
            }
            bool returnValue = base.CanUseOn(target, sourceCharacter, abilityEffectContext);
            Debug.Log(DisplayName + ".CapturePetEffect.CanUseOn(): returning: " + returnValue);
            return returnValue;
        }

        public override Dictionary<PrefabProfile, GameObject> Cast(IAbilityCaster source, Interactable target, Interactable originalTarget, AbilityEffectContext abilityEffectInput) {
            if (target == null) {
                Debug.Log(DisplayName + ".CapturePetEffect.Cast(): target is null, returning");
                return null;
            }

            Dictionary<PrefabProfile, GameObject> returnValue = base.Cast(source, target, originalTarget, abilityEffectInput);

            UnitController targetUnitController = target.GetComponent<UnitController>();
            if (targetUnitController != null) {
                //Debug.Log(DisplayName + ".CapturePetEffect.Cast(): applying control effects");
                targetUnitController.SetPetMode((source.AbilityManager as CharacterAbilityManager).BaseCharacter);
                source.AbilityManager.CapturePet(targetUnitController);
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
                        Debug.LogError("CapturePetEffect.SetupScriptableObjects(): Could not find unitTypeRestriction: " + unitTypeRestriction + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }
        }

    }
}