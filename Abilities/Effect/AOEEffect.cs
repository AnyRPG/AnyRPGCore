using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New AOEEffect", menuName = "Abilities/Effects/AOEEffect")]
public class AOEEffect : FixedLengthEffect {

    [SerializeField]
    private float aoeRadius;

    [SerializeField]
    private bool useRadius = true;

    [SerializeField]
    private bool useExtents = false;

    [SerializeField]
    private Vector3 aoeCenter;

    [SerializeField]
    private Vector3 aoeExtents;

    [SerializeField]
    private float maxTargets = 0;

    /// <summary>
    /// Does the actual work of hitting the target with an ability
    /// </summary>
    /// <param name="ability"></param>
    /// <param name="source"></param>
    /// <param name="target"></param>
    public override void Cast(BaseCharacter source, GameObject target, GameObject originalTarget, AbilityEffectOutput abilityEffectInput) {
        Debug.Log(MyName + ".AOEEffect.Cast(" + (source == null ? "null" : source.name) + ", " + (target == null ? "null" : target.name) + ")");
        if (abilityEffectInput == null) {
            abilityEffectInput = new AbilityEffectOutput();
        }
        base.Cast(source, target, originalTarget, abilityEffectInput);
        TargetAOEHit(source, target, abilityEffectInput);
    }

    public override void CastTick(BaseCharacter source, GameObject target, AbilityEffectOutput abilityAffectInput) {
        //Debug.Log(resourceName + ".AOEEffect.CastTick(" + (source == null ? "null" : source.name) + ", " + (target == null ? "null" : target.name) + ")");
        TargetAOETick(source, target, abilityAffectInput);
    }

    public override void CastComplete(BaseCharacter source, GameObject target, AbilityEffectOutput abilityAffectInput) {
        //Debug.Log(resourceName + ".AOEEffect.CastComplete(" + (source == null ? "null" : source.name) + ", " + (target == null ? "null" : target.name) + ")");
        TargetAOEComplete(source, target, abilityAffectInput);
    }

    private void TargetAOEHit(BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput) {
        Debug.Log(MyName + "AOEEffect.TargetAOEHit(" + (source == null ? "null" : source.name) + ", " + (target == null ? "null" : target.name) + ")");
        List<GameObject> validTargets = GetValidTargets(source, target, abilityEffectInput, hitAbilityEffectList);
        foreach (GameObject validTarget in validTargets) {
            PerformAOEHit(source, validTarget, 1f / validTargets.Count, abilityEffectInput);
        }
    }

    private void TargetAOETick(BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput) {
        List<GameObject> validTargets = GetValidTargets(source, target, abilityEffectInput, tickAbilityEffectList);
        foreach (GameObject validTarget in validTargets) {
            PerformAOETick(source, validTarget, 1f / validTargets.Count, abilityEffectInput);
        }
    }

    private void TargetAOEComplete(BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput) {
        List<GameObject> validTargets = GetValidTargets(source, target, abilityEffectInput, completeAbilityEffectList);
        foreach (GameObject validTarget in validTargets) {
            PerformAOEComplete(source, validTarget, 1f / validTargets.Count, abilityEffectInput);
        }
    }

    private List<GameObject> GetValidTargets(BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput, List<AbilityEffect> abilityEffectList) {
        Debug.Log(MyName + "AOEEffect.GetValidTargets()");

        Vector3 aoeSpawnCenter = Vector3.zero;
        if (prefabSpawnLocation == PrefabSpawnLocation.Target && target != null) {
            Debug.Log("AOEEffect.Cast(): Setting AOE center to target");
            aoeSpawnCenter = target.transform.position;
        } else if (prefabSpawnLocation == PrefabSpawnLocation.Caster) {
            //Debug.Log("AOEEffect.Cast(): Setting AOE center to caster");
            aoeSpawnCenter = source.MyCharacterUnit.transform.position;
        } else if (prefabSpawnLocation == PrefabSpawnLocation.Point) {
            Debug.Log("AOEEffect.Cast(): Setting AOE center to groundTarget at: " + abilityEffectInput.prefabLocation);
            aoeSpawnCenter = abilityEffectInput.prefabLocation;
        } else {
            Debug.Log("AOEEffect.Cast(): Setting AOE center to vector3.zero!!! was prefab spawn location not set or target despawned?");
        }
        aoeSpawnCenter += source.MyCharacterUnit.transform.TransformDirection(aoeCenter);
        Collider[] colliders = new Collider[0];
        if (useRadius) {
            colliders = Physics.OverlapSphere(aoeSpawnCenter, aoeRadius);
        }
        if (useExtents) {
            colliders = Physics.OverlapBox(aoeSpawnCenter, aoeExtents / 2f, source.MyCharacterUnit.transform.rotation);
        }
        //Debug.Log("AOEEffect.Cast(): Casting OverlapSphere with radius: " + aoeRadius);
        List<GameObject> validTargets = new List<GameObject>();
        foreach (Collider collider in colliders) {
            //Debug.Log("AOEEffect.Cast() hit: " + collider.gameObject.name + "; " + collider.gameObject.layer);
            bool canAdd = true;
            foreach (AbilityEffect abilityEffect in abilityEffectList) {
                if (abilityEffect.CanUseOn(collider.gameObject, source) == false) {
                    canAdd = false;
                }
            }
            //if (CanUseOn(collider.gameObject, source)) {
            // next line was preventing aoe from hitting current target
            //if (collider.gameObject != target && CanUseOn(collider.gameObject, source)) {
            //Debug.Log("performing AOE ability: " + MyAbilityEffectName + " on " + collider.gameObject);
            if (canAdd) {
                validTargets.Add(collider.gameObject);
            }
            if (maxTargets > 0) {
                //Debug.Log("AOEEffect.GetValidTargets(). maxTargets: " + maxTargets + "; validTargets.Count: " + validTargets.Count);
                while (validTargets.Count > maxTargets) {
                    int randomNumber = Random.Range(0, validTargets.Count);
                    //Debug.Log("AOEEffect.GetValidTargets(). maxTargets: " + maxTargets + "; validTargets.Count: " + validTargets.Count + "; randomNumber: " + randomNumber);
                    validTargets.RemoveAt(randomNumber);
                }
            }
            //}
        }
        //Debug.Log(abilityEffectName + ".AOEEffect.Cast(). Valid targets count: " + validTargets.Count);
        return validTargets;
    }

    private AbilityEffectOutput GetSharedOutput(float outputShare, AbilityEffectOutput abilityEffectInput) {
        AbilityEffectOutput modifiedOutput = new AbilityEffectOutput();
        modifiedOutput.healthAmount = (int)(abilityEffectInput.healthAmount * outputShare);
        modifiedOutput.manaAmount = (int)(abilityEffectInput.manaAmount * outputShare);
        modifiedOutput.prefabLocation = abilityEffectInput.prefabLocation;
        return modifiedOutput;
    }

    public void PerformAOEHit(BaseCharacter source, GameObject target, float outputShare, AbilityEffectOutput abilityEffectInput) {
        //Debug.Log(abilityEffectName + ".AOEEffect.PerformAbilityEffect(): abilityEffectInput.healthAmount: " + (abilityEffectInput == null ? "null" : abilityEffectInput.healthAmount.ToString()) + "; outputShare: " + outputShare);
        AbilityEffectOutput modifiedOutput = GetSharedOutput(outputShare, abilityEffectInput);
        PerformAbilityHit(source, target, modifiedOutput);
    }

    public void PerformAOETick(BaseCharacter source, GameObject target, float outputShare, AbilityEffectOutput abilityEffectInput) {
        //Debug.Log(resourceName + ".AOEEffect.PerformAbilityEffect(): abilityEffectInput.healthAmount: " + (abilityEffectInput == null ? "null" : abilityEffectInput.healthAmount.ToString()) + "; outputShare: " + outputShare);
        AbilityEffectOutput modifiedOutput = GetSharedOutput(outputShare, abilityEffectInput);
        PerformAbilityTick(source, target, modifiedOutput);
    }

    public void PerformAOEComplete(BaseCharacter source, GameObject target, float outputShare, AbilityEffectOutput abilityEffectInput) {
        //Debug.Log(abilityEffectName + ".AOEEffect.PerformAbilityEffect(): abilityEffectInput.healthAmount: " + (abilityEffectInput == null ? "null" : abilityEffectInput.healthAmount.ToString()) + "; outputShare: " + outputShare);
        AbilityEffectOutput modifiedOutput = GetSharedOutput(outputShare, abilityEffectInput);
        PerformAbilityComplete(source, target, modifiedOutput);
    }

}