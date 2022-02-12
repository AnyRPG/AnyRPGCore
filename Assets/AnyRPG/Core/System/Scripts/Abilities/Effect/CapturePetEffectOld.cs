using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Capture Pet Effect", menuName = "AnyRPG/Abilities/Effects/CapturePetEffect")]
    public class CapturePetEffectOld : InstantEffectOld {

        [SerializeField]
        [ResourceSelector(resourceType = typeof(UnitType))]
        protected List<string> unitTypeRestrictions = new List<string>();

        protected List<UnitType> unitTypeRestrictionList = new List<UnitType>();

        public override bool CanUseOn(Interactable target, IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectContext = null, bool playerInitiated = false, bool performRangeCheck = true) {
            //Debug.Log(DisplayName + ".CapturePetEffect.CanUseOn()");
            if (target == null) {
                // capture pet effect requires a target under all circumstances
                if (playerInitiated) {
                    sourceCharacter.AbilityManager.ReceiveCombatMessage("Cannot cast " + resourceName + ". Target required");
                }
                return false;
            }
            BaseCharacter targetCharacter = target.GetComponent<BaseCharacter>();
            if (targetCharacter == null) {
                // if there is no target character, it cannot possibly match a unit type
                //Debug.Log(DisplayName + ".CapturePetEffect.CanUseOn(): no target character");
                if (playerInitiated) {
                    sourceCharacter.AbilityManager.ReceiveCombatMessage("Cannot cast " + resourceName + ". target must be a character");
                }
                return false;
            }
            UnitController unitController = target as UnitController;
            if (unitController?.UnitProfile == null || unitController.UnitProfile.IsPet == false) {
                // has to be the right unit type plus needs to be capturable specifically
                //Debug.Log(DisplayName + ".CapturePetEffect.CanUseOn(): pet was not capturable ");
                if (playerInitiated) {
                    sourceCharacter.AbilityManager.ReceiveCombatMessage("Cannot cast " + resourceName + ". Target is not a capturable pet");
                }
                return false;
            }
            if (unitTypeRestrictionList != null && unitTypeRestrictionList.Count > 0) {
                if (targetCharacter.UnitType == null || !unitTypeRestrictionList.Contains(targetCharacter.UnitType)) {
                    //Debug.Log(MyDisplayName + ".CapturePetEffect.CanUseOn(): pet was not allowed by your restrictions ");
                    if (playerInitiated) {
                        sourceCharacter.AbilityManager.ReceiveCombatMessage("Cannot cast " + resourceName + ". pet was not allowed by your restrictions");
                    }
                    return false;
                }
            }
            bool returnValue = base.CanUseOn(target, sourceCharacter, abilityEffectContext, playerInitiated, performRangeCheck);
            //Debug.Log(DisplayName + ".CapturePetEffect.CanUseOn(): returning: " + returnValue);
            return returnValue;
        }

        public override Dictionary<PrefabProfile, GameObject> Cast(IAbilityCaster source, Interactable target, Interactable originalTarget, AbilityEffectContext abilityEffectInput) {
            if (target == null) {
                //Debug.Log(DisplayName + ".CapturePetEffect.Cast(): target is null, returning");
                return null;
            }

            Dictionary<PrefabProfile, GameObject> returnValue = base.Cast(source, target, originalTarget, abilityEffectInput);

            UnitController targetUnitController = target.GetComponent<UnitController>();
            if (targetUnitController != null) {
                //Debug.Log(DisplayName + ".CapturePetEffect.Cast(): applying control effects");
                source.AbilityManager.CapturePet(targetUnitController);
            }

            return returnValue;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);
            if (unitTypeRestrictions != null && unitTypeRestrictions.Count > 0) {
                foreach (string unitTypeRestriction in unitTypeRestrictions) {
                    //Debug.Log(DisplayName + ".CapturePetEffect.SetupScriptableObjects(): looping through restrictions: " + unitTypeRestriction);
                    UnitType tmpUnitType = systemDataFactory.GetResource<UnitType>(unitTypeRestriction);
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