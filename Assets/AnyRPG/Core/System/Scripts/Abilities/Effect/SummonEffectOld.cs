using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New SummonEffect", menuName = "AnyRPG/Abilities/Effects/SummonEffect")]
    public class SummonEffectOld : InstantEffectOld {

        [Header("Summon")]

        [Tooltip("Unit Prefab Profile to use for the summon pet")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(UnitProfile))]
        private string unitProfileName = string.Empty;

        // reference to actual unitProfile
        private UnitProfile unitProfile = null;

        // reference to spawned object UnitController
        private UnitController petUnitController;

        public UnitProfile UnitProfile { get => unitProfile; set => unitProfile = value; }

        public override Dictionary<PrefabProfile, GameObject> Cast(IAbilityCaster source, Interactable target, Interactable originalTarget, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(DisplayName + ".SummonEffect.Cast()");
            base.Cast(source, target, originalTarget, abilityEffectInput);
            Dictionary<PrefabProfile, GameObject> returnObjects = base.Cast(source, target, originalTarget, abilityEffectInput);
            (source.AbilityManager as CharacterAbilityManager).BaseCharacter.CharacterPetManager.SpawnPet(unitProfile);
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

        protected override void CheckDestroyObjects(Dictionary<PrefabProfile, GameObject> abilityEffectObjects, IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectInput) {
            // intentionally not calling base to avoid getting our pet destroyed
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);
            if (unitProfileName != null && unitProfileName != string.Empty) {
                UnitProfile tmpUnitProfile = systemDataFactory.GetResource<UnitProfile>(unitProfileName);
                if (tmpUnitProfile != null) {
                    unitProfile = tmpUnitProfile;
                } else {
                    Debug.LogError("SummonEffect.SetupScriptableObjects(): Could not find unitProfile : " + unitProfileName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }
        }


    }

}
