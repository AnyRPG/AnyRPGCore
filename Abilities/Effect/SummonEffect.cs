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

        public override Dictionary<PrefabProfile, GameObject> Cast(BaseCharacter source, GameObject target, GameObject originalTarget, AbilityEffectOutput abilityEffectInput) {
            Debug.Log(MyName + ".SummonEffect.Cast()");
            base.Cast(source, target, originalTarget, abilityEffectInput);
            Dictionary<PrefabProfile, GameObject> returnObjects = Spawn(source);
            return returnObjects;
        }

        private Dictionary<PrefabProfile, GameObject> Spawn(BaseCharacter source) {
            Debug.Log(MyName + ".SummonEffect.Spawn(): prefabObjects.count: " + prefabObjects.Count);
            foreach (KeyValuePair<PrefabProfile, GameObject> tmpPair in prefabObjects) {
                Debug.Log(MyName + ".SummonEffect.Spawn(): looping through prefabObjects");
                //GameObject spawnReference = Instantiate(summonObject, PlayerManager.MyInstance.MyAIUnitParent.transform, true);
                tmpPair.Value.transform.parent = null;
                //Debug.Log("UnitSpawnNode.Spawn(): gameObject spawned at: " + spawnReference.transform.position);
                //Vector3 newSpawnLocation = GetSpawnLocation();
                Vector3 newSpawnLocation = source.MyCharacterUnit.transform.position;
                //Debug.Log("UnitSpawnNode.Spawn(): newSpawnLocation: " + newSpawnLocation);
                NavMeshAgent navMeshAgent = tmpPair.Value.GetComponent<NavMeshAgent>();
                AIController aIController = tmpPair.Value.GetComponent<AIController>();
                aIController.MyStartPosition = newSpawnLocation;
                //Debug.Log("UnitSpawnNode.Spawn(): navhaspath: " + navMeshAgent.hasPath + "; isOnNavMesh: " + navMeshAgent.isOnNavMesh + "; isOnOffMeshLink: " + navMeshAgent.isOnOffMeshLink + "; pathpending: " + navMeshAgent.pathPending + "; warping now!");
                //spawnReference.transform.position = newSpawnLocation;
                navMeshAgent.Warp(newSpawnLocation);
                //Debug.Log("UnitSpawnNode.Spawn(): afterMove: navhaspath: " + navMeshAgent.hasPath + "; isOnNavMesh: " + navMeshAgent.isOnNavMesh + "; pathpending: " + navMeshAgent.pathPending);
                CharacterUnit _characterUnit = tmpPair.Value.GetComponent<CharacterUnit>();
                /*
                if (_characterUnit != null) {
                    _characterUnit.OnDespawn += HandleDespawn;
                }
                */
                //int _unitLevel = (dynamicLevel ? PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel : unitLevel) + extraLevels;
                int _unitLevel = source.MyCharacterStats.MyLevel;
                _characterUnit.MyCharacter.MyCharacterStats.SetLevel(_unitLevel);
                (_characterUnit.MyCharacter.MyCharacterStats as AIStats).ApplyControlEffects(source);
                //spawnReferences.Add(spawnReference);
            }

            return prefabObjects;
        }

        protected override void CheckDestroyObjects(Dictionary<PrefabProfile, GameObject> abilityEffectObjects, BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput) {
            // intentionally not calling base to avoid getting our pet destroyed
        }


    }

}
