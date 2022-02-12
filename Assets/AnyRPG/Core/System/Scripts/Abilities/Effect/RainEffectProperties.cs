using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class RainEffectProperties : AOEEffectProperties {


        protected override List<AOETargetNode> GetValidTargets(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectInput, List<AbilityEffect> abilityEffectList) {
            //Debug.Log(DisplayName + ".RainEffect.GetValidTargets()");
            // we are intentionally not calling the base class

            // max targets determines how many objects to spawn

            Vector3 aoeSpawnCenter = Vector3.zero;
            if (prefabSpawnLocation == PrefabSpawnLocation.Target && target != null) {
                //Debug.Log("AOEEffect.Cast(): Setting AOE center to target");
                aoeSpawnCenter = target.transform.position;
            } else if (prefabSpawnLocation == PrefabSpawnLocation.Caster || prefabSpawnLocation == PrefabSpawnLocation.CasterPoint) {
                //Debug.Log("AOEEffect.Cast(): Setting AOE center to caster");
                aoeSpawnCenter = source.AbilityManager.UnitGameObject.transform.position;
            } else if (prefabSpawnLocation == PrefabSpawnLocation.GroundTarget) {
                //Debug.Log("AOEEffect.Cast(): Setting AOE center to groundTarget at: " + abilityEffectInput.prefabLocation);
                aoeSpawnCenter = abilityEffectInput.groundTargetLocation;
            } else {
                //Debug.Log("AOEEffect.Cast(): Setting AOE center to vector3.zero!!! was prefab spawn location not set or target despawned?");
            }
            //aoeSpawnCenter += source.AbilityManager.UnitGameObject.transform.TransformDirection(aoeCenter);

            //Debug.Log("AOEEffect.Cast(): Casting OverlapSphere with radius: " + aoeRadius);
            List<AOETargetNode> validTargets = new List<AOETargetNode>();
            // for loop max targets 
            for (int i = 0; i < maxTargets; i++) {
                AOETargetNode validTargetNode = new AOETargetNode();
                validTargetNode.targetGameObject = null;
                //abilityEffectInput.prefabLocation = new Vector3(aoeSpawnCenter.x + Random.Range(-aoeRadius, aoeRadius), aoeSpawnCenter.y + aoeCenter.y, aoeSpawnCenter.z + Random.Range(-aoeRadius, aoeRadius));
                // testing make copy instead
                //validTargetNode.abilityEffectInput = new AbilityEffectContext();
                validTargetNode.abilityEffectInput = abilityEffectInput.GetCopy(); ;
                /*
                foreach (ResourceInputAmountNode resourceInputAmountNode in abilityEffectInput.resourceAmounts) {
                    validTargetNode.abilityEffectInput.AddResourceAmount(resourceInputAmountNode.resourceName, (int)resourceInputAmountNode.amount);
                }

                validTargetNode.abilityEffectInput.overrideDuration = abilityEffectInput.overrideDuration;
                validTargetNode.abilityEffectInput.savedEffect = abilityEffectInput.savedEffect;
                validTargetNode.abilityEffectInput.castTimeMultiplier = abilityEffectInput.castTimeMultiplier;
                validTargetNode.abilityEffectInput.spellDamageMultiplier = abilityEffectInput.spellDamageMultiplier;
                //validTargetNode.abilityEffectInput.prefabLocation = abilityEffectInput.prefabLocation;
                //validTargetNode.abilityEffectInput = abilityEffectInput;
                validTargetNode.abilityEffectInput.groundTargetLocation = new Vector3(aoeSpawnCenter.x + Random.Range(-aoeRadius, aoeRadius), aoeSpawnCenter.y + aoeCenter.y, aoeSpawnCenter.z + Random.Range(-aoeRadius, aoeRadius));
                */
                validTargetNode.abilityEffectInput.groundTargetLocation = new Vector3(aoeSpawnCenter.x + Random.Range(-aoeRadius, aoeRadius), aoeSpawnCenter.y + aoeCenter.y, aoeSpawnCenter.z + Random.Range(-aoeRadius, aoeRadius));

                //Debug.Log(DisplayName + ".RainEffect.GetValidTargets(). prefabLocation: " + validTargetNode.abilityEffectInput.prefabLocation);
                validTargets.Add(validTargetNode);
            }

            //Debug.Log(DisplayName + ".RainEffect.GetValidTargets(). Valid targets count: " + validTargets.Count);
            return validTargets;
        }

    }
}