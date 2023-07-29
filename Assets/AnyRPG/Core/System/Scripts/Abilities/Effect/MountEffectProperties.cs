using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


namespace AnyRPG {

    [System.Serializable]
    public class MountEffectProperties : StatusEffectProperties {

        [Header("Mount")]

        [Tooltip("Unit Prefab Profile to use for the mount object")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(UnitProfile))]
        private string unitProfileName = string.Empty;

        // reference to actual unitProfile
        private UnitProfile unitProfile = null;

        /*
        public void GetMountEffectProperties(MountEffect effect) {

            unitProfileName = effect.UnitProfileName;

            GetStatusEffectProperties(effect);
        }
        */

        public override void CancelEffect(UnitController targetUnitController) {
            //Debug.Log(displayName +  ".MountEffect.CancelEffect(" + (targetCharacter != null ? targetCharacter.name : "null") + ")");
            if (SystemGameManager.IsShuttingDown == true) {
                // game is in the middle of exiting
                return;
            }
            if (targetUnitController == null) {
                return;
            }
            targetUnitController.DeActivateMountedState();
            
            base.CancelEffect(targetUnitController);
        }

        public override bool CanCast() {
            if (levelManager.GetActiveSceneNode()?.AllowMount == false) {
                //Debug.Log(DisplayName + ".MountEffect.CanCast(): scene does not allow mount");
                return false;
            }
            return base.CanCast();
        }

        public override Dictionary<PrefabProfile, List<GameObject>> Cast(IAbilityCaster source, Interactable target, Interactable originalTarget, AbilityEffectContext abilityEffectInput) {
            //Debug.Log("StatusEffect.Cast(" + source.name + ", " + (target? target.name : "null") + ")");
            if (!CanUseOn(target, source)) {
                return null;
            }
            if (target == null) {
                // we can't mount anything if there is no target
                return null;
            }
            Dictionary<PrefabProfile, List<GameObject>> returnObjects = base.Cast(source, target, originalTarget, abilityEffectInput);

            source.AbilityManager.SetMountedState(unitProfile);
            
            return returnObjects;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager, IDescribable describable) {
            base.SetupScriptableObjects(systemGameManager, describable);

            if (unitProfileName != null && unitProfileName != string.Empty) {
                UnitProfile tmpUnitProfile = systemDataFactory.GetResource<UnitProfile>(unitProfileName);
                if (tmpUnitProfile != null) {
                    unitProfile = tmpUnitProfile;
                } else {
                    Debug.LogError("MountEffect.SetupScriptableObjects(): Could not find prefab Profile : " + unitProfileName + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
                }
            } else {
                Debug.LogError("MountEffect.SetupScriptableObjects(): Mount effect requires a unit prefab profile but non was configured while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
            }

        }



    }
}
