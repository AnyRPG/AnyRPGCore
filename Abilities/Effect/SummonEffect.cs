using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New SummonEffect", menuName = "AnyRPG/Abilities/Effects/SummonEffect")]
    public class SummonEffect : InstantEffect {

        [Header("Summon")]

        [Tooltip("Unit Prefab Profile to use for the summon pet")]
        [SerializeField]
        private string unitProfileName = string.Empty;

        // reference to actual unitProfile
        private UnitProfile unitProfile = null;

        // reference to spawned object UnitController
        private UnitController petUnitController;

        public override Dictionary<PrefabProfile, GameObject> Cast(IAbilityCaster source, GameObject target, GameObject originalTarget, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(MyName + ".SummonEffect.Cast()");
            base.Cast(source, target, originalTarget, abilityEffectInput);
            Dictionary<PrefabProfile, GameObject> returnObjects = base.Cast(source, target, originalTarget, abilityEffectInput);
            (source.AbilityManager as CharacterAbilityManager).BaseCharacter.MyCharacterPetManager.SpawnPet(unitProfile);
            return returnObjects;
        }
        /*
        private void Spawn(BaseCharacter source) {
            //Debug.Log(MyName + ".SummonEffect.Spawn(): prefabObjects.count: " + prefabObjects.Count);

            UnitController unitController = unitProfile.SpawnUnitPrefab(source.UnitController.transform.parent);
            if (unitController != null) {
                petUnitController = unitController;
                petUnitController.SetUnitControllerMode(UnitControllerMode.Pet);
                source.MyCharacterPetManager.HandlePetSpawn(petUnitController);
            }
        }
        */

        /*
        public override void CancelEffect(BaseCharacter targetCharacter) {
            base.CancelEffect(targetCharacter);
            petUnitController.
        }
        */

        protected override void CheckDestroyObjects(Dictionary<PrefabProfile, GameObject> abilityEffectObjects, IAbilityCaster source, GameObject target, AbilityEffectContext abilityEffectInput) {
            // intentionally not calling base to avoid getting our pet destroyed
        }


    }

}
