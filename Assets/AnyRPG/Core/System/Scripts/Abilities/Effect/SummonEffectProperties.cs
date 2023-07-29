using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class SummonEffectProperties : InstantEffectProperties {

        [Header("Summon")]

        [Tooltip("Unit Profile to use for the summon pet")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(UnitProfile))]
        private string unitProfileName = string.Empty;

        // reference to actual unitProfile
        private UnitProfile unitProfile = null;

        /*
        // reference to spawned object UnitController
        private UnitController petUnitController;
        */

        public UnitProfile UnitProfile { get => unitProfile; set => unitProfile = value; }

        /*
        public void GetSummonEffectProperties(SummonEffect effect) {

            unitProfileName = effect.UnitProfileName;

            GetInstantEffectProperties(effect);
        }
        */

        public override Dictionary<PrefabProfile, List<GameObject>> Cast(IAbilityCaster source, Interactable target, Interactable originalTarget, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(DisplayName + ".SummonEffect.Cast()");
            // cast twice?  is this intentional?
            base.Cast(source, target, originalTarget, abilityEffectInput);
            Dictionary<PrefabProfile, List<GameObject>> returnObjects = base.Cast(source, target, originalTarget, abilityEffectInput);
            (source as UnitController).CharacterPetManager.SpawnPet(unitProfile);
            return returnObjects;
        }
        /*
        private void Spawn(BaseCharacter source) {
            //Debug.Log(DisplayName + ".SummonEffect.Spawn(): prefabObjects.count: " + prefabObjects.Count);

            UnitController unitController = unitProfile.SpawnUnitPrefab(source.UnitController.transform.parent);
            if (unitController != null) {
                petUnitController = unitController;
                petUnitController.SetUnitControllerMode(UnitControllerMode.Pet);
                source.MyCharacterPetManager.HandlePetSpawn(petUnitController);
            }
        }
        */

        protected override void CheckDestroyObjects(Dictionary<PrefabProfile, List<GameObject>> abilityEffectObjects, IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectInput) {
            // intentionally not calling base to avoid getting our pet destroyed
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager, IDescribable describable) {
            base.SetupScriptableObjects(systemGameManager, describable);

            if (unitProfileName != null && unitProfileName != string.Empty) {
                UnitProfile tmpUnitProfile = systemDataFactory.GetResource<UnitProfile>(unitProfileName);
                if (tmpUnitProfile != null) {
                    unitProfile = tmpUnitProfile;
                } else {
                    Debug.LogError("SummonEffect.SetupScriptableObjects(): Could not find unitProfile : " + unitProfileName + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
                }
            }
        }


    }

}
