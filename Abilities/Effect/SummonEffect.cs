using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New SummonEffect", menuName = "AnyRPG/Abilities/Effects/SummonEffect")]
    public class SummonEffect : InstantEffect {

        /*
        // The prefab to summon
        [SerializeField]
        private GameObject summonObject;
        */

        /*
        [SerializeField]
        private Vector3 spawnLocation = Vector3.zero;
        */

        public override Dictionary<PrefabProfile, GameObject> Cast(IAbilityCaster source, GameObject target, GameObject originalTarget, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log(MyName + ".SummonEffect.Cast()");
            base.Cast(source, target, originalTarget, abilityEffectInput);
            Dictionary<PrefabProfile, GameObject> returnObjects = Spawn((source as CharacterAbilityManager).BaseCharacter);
            return returnObjects;
        }

        private Dictionary<PrefabProfile, GameObject> Spawn(BaseCharacter source) {
            //Debug.Log(MyName + ".SummonEffect.Spawn(): prefabObjects.count: " + prefabObjects.Count);
            foreach (KeyValuePair<PrefabProfile, GameObject> tmpPair in prefabObjects) {
                //Debug.Log(MyName + ".SummonEffect.Spawn(): looping through prefabObjects");
                //GameObject spawnReference = Instantiate(summonObject, PlayerManager.MyInstance.MyAIUnitParent.transform, true);
                GameObject go = tmpPair.Value;
                if (source.MyCharacterPetManager != null) {
                    source.MyCharacterPetManager.HandlePetSpawn(go);
                }
                //spawnReferences.Add(spawnReference);
            }

            return prefabObjects;
        }

        protected override void CheckDestroyObjects(Dictionary<PrefabProfile, GameObject> abilityEffectObjects, IAbilityCaster source, GameObject target, AbilityEffectOutput abilityEffectInput) {
            // intentionally not calling base to avoid getting our pet destroyed
        }


    }

}
