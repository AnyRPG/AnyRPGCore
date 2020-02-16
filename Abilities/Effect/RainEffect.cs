using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New RainEffect", menuName = "AnyRPG/Abilities/Effects/RainEffect")]
    public class RainEffect : AOEEffect {


        protected override List<AOETargetNode> GetValidTargets(BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput, List<AbilityEffect> abilityEffectList) {
            //Debug.Log(MyName + ".RainEffect.GetValidTargets()");
            // we are intentionally not calling the base class

            // max targets determines how many objects to spawn

            Vector3 aoeSpawnCenter = Vector3.zero;
            if (prefabSpawnLocation == PrefabSpawnLocation.Target && target != null) {
                //Debug.Log("AOEEffect.Cast(): Setting AOE center to target");
                aoeSpawnCenter = target.transform.position;
            } else if (prefabSpawnLocation == PrefabSpawnLocation.Caster) {
                //Debug.Log("AOEEffect.Cast(): Setting AOE center to caster");
                aoeSpawnCenter = source.MyCharacterUnit.transform.position;
            } else if (prefabSpawnLocation == PrefabSpawnLocation.Point) {
                //Debug.Log("AOEEffect.Cast(): Setting AOE center to groundTarget at: " + abilityEffectInput.prefabLocation);
                aoeSpawnCenter = abilityEffectInput.prefabLocation;
            } else {
                //Debug.Log("AOEEffect.Cast(): Setting AOE center to vector3.zero!!! was prefab spawn location not set or target despawned?");
            }
            //aoeSpawnCenter += source.MyCharacterUnit.transform.TransformDirection(aoeCenter);

            //Debug.Log("AOEEffect.Cast(): Casting OverlapSphere with radius: " + aoeRadius);
            List<AOETargetNode> validTargets = new List<AOETargetNode>();
            // for loop max targets 
            for (int i = 0; i < maxTargets; i++) {
                AOETargetNode validTargetNode = new AOETargetNode();
                validTargetNode.targetGameObject = null;
                //abilityEffectInput.prefabLocation = new Vector3(aoeSpawnCenter.x + Random.Range(-aoeRadius, aoeRadius), aoeSpawnCenter.y + aoeCenter.y, aoeSpawnCenter.z + Random.Range(-aoeRadius, aoeRadius));
                validTargetNode.abilityEffectInput = new AbilityEffectOutput();
                validTargetNode.abilityEffectInput.healthAmount = abilityEffectInput.healthAmount;
                validTargetNode.abilityEffectInput.manaAmount = abilityEffectInput.manaAmount;
                validTargetNode.abilityEffectInput.overrideDuration = abilityEffectInput.overrideDuration;
                validTargetNode.abilityEffectInput.savedEffect = abilityEffectInput.savedEffect;
                validTargetNode.abilityEffectInput.castTimeMultipler = abilityEffectInput.castTimeMultipler;
                validTargetNode.abilityEffectInput.spellDamageMultiplier = abilityEffectInput.spellDamageMultiplier;
                //validTargetNode.abilityEffectInput.prefabLocation = abilityEffectInput.prefabLocation;
                //validTargetNode.abilityEffectInput = abilityEffectInput;
                validTargetNode.abilityEffectInput.prefabLocation = new Vector3(aoeSpawnCenter.x + Random.Range(-aoeRadius, aoeRadius), aoeSpawnCenter.y + aoeCenter.y, aoeSpawnCenter.z + Random.Range(-aoeRadius, aoeRadius));
                //Debug.Log(MyName + ".RainEffect.GetValidTargets(). prefabLocation: " + validTargetNode.abilityEffectInput.prefabLocation);
                validTargets.Add(validTargetNode);
            }

            //Debug.Log(MyName + ".RainEffect.GetValidTargets(). Valid targets count: " + validTargets.Count);
            return validTargets;
        }

    }
}