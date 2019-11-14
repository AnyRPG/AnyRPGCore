using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New LengthEffect", menuName = "AnyRPG/Abilities/Effects/LengthEffect")]
    public class LengthEffect : AbilityEffect {

        [SerializeField]
        protected GameObject abilityEffectPrefab = null;

        [SerializeField]
        protected PrefabSpawnLocation prefabSpawnLocation;

        [SerializeField]
        protected string prefabSourceBone;

        [SerializeField]
        protected Vector3 prefabOffset = Vector3.zero;

        [SerializeField]
        protected Vector3 prefabRotation = Vector3.zero;

        // a delay after the effect ends to destroy the spell effect prefab
        [SerializeField]
        protected float prefabDestroyDelay = 0f;

        /// <summary>
        /// the reference to the gameobject spawned by this ability
        /// </summary>
        protected GameObject abilityEffectObject = null;

        // every <tickRate> seconds, the Tick() will occur
        [SerializeField]
        protected float tickRate;

        // any abilities to cast every tick
        [SerializeField]
        protected List<AbilityEffect> tickAbilityEffectList = new List<AbilityEffect>();

        //private float nextTickTime;
        protected DateTime nextTickTime;

        // any abilities to cast when the effect completes
        [SerializeField]
        protected List<AbilityEffect> completeAbilityEffectList = new List<AbilityEffect>();

        public List<AbilityEffect> MyTickAbilityEffectList { get => tickAbilityEffectList; set => tickAbilityEffectList = value; }
        public List<AbilityEffect> MyCompleteAbilityEffectList { get => completeAbilityEffectList; set => completeAbilityEffectList = value; }
        public float MyTickRate { get => tickRate; set => tickRate = value; }
        public DateTime MyNextTickTime { get => nextTickTime; set => nextTickTime = value; }
        public float MyPrefabDestroyDelay { get => prefabDestroyDelay; set => prefabDestroyDelay = value; }
        public PrefabSpawnLocation MyPrefabSpawnLocation { get => prefabSpawnLocation; set => prefabSpawnLocation = value; }
        public GameObject MyAbilityEffectPrefab { get => abilityEffectPrefab; set => abilityEffectPrefab = value; }

        public override void Cast(BaseCharacter source, GameObject target, GameObject originalTarget, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log(MyName + ".LengthEffect.Cast(" + source.name + ")");
            Vector3 spawnLocation = Vector3.zero;
            Transform prefabParent = null;
            if (prefabSpawnLocation == PrefabSpawnLocation.Point) {
                //Debug.Log(resourceName + ".AbilityEffect.Cast(): prefabspawnlocation: point; abilityEffectInput.prefabLocation: " + abilityEffectInput.prefabLocation);
                //spawnLocation = source.GetComponent<Collider>().bounds.center;
                spawnLocation = abilityEffectInput.prefabLocation;
                prefabParent = null;
            }
            if (prefabSpawnLocation == PrefabSpawnLocation.Caster) {
                //Debug.Log("PrefabSpawnLocation is Caster");
                //spawnLocation = source.GetComponent<Collider>().bounds.center;
                spawnLocation = source.MyCharacterUnit.transform.position;
                prefabParent = source.MyCharacterUnit.transform;
                Transform usedPrefabSourceBone = null;
                if (prefabSourceBone != null && prefabSourceBone != string.Empty) {
                    usedPrefabSourceBone = prefabParent.FindChildByRecursive(prefabSourceBone);
                }
                if (usedPrefabSourceBone != null) {
                    prefabParent = usedPrefabSourceBone;
                }
            }
            if (prefabSpawnLocation == PrefabSpawnLocation.Target && target != null) {
                //spawnLocation = target.GetComponent<Collider>().bounds.center;
                spawnLocation = target.transform.position;
                prefabParent = target.transform;
            }
            if (prefabSpawnLocation == PrefabSpawnLocation.OriginalTarget && target != null) {
                //spawnLocation = target.GetComponent<Collider>().bounds.center;
                spawnLocation = originalTarget.transform.position;
                prefabParent = originalTarget.transform;
            }
            base.Cast(source, target, originalTarget, abilityEffectInput);
            if (abilityEffectPrefab != null && prefabSpawnLocation != PrefabSpawnLocation.None && (target != null || prefabSpawnLocation == PrefabSpawnLocation.Point || requiresTarget == false)) {
                //float finalX = (prefabParent == null ? prefabOffset.x : prefabParent.TransformPoint(prefabOffset).x);
                //float finalY = (prefabParent == null ? prefabOffset.y : prefabParent.TransformPoint(prefabOffset).x);
                //float finalZ = (prefabParent == null ? prefabOffset.z : prefabParent.TransformPoint(prefabOffset).z);
                float finalX = (prefabParent == null ? spawnLocation.x + prefabOffset.x : prefabParent.TransformPoint(prefabOffset).x);
                float finalY = (prefabParent == null ? spawnLocation.y + prefabOffset.y : prefabParent.TransformPoint(prefabOffset).y);
                float finalZ = (prefabParent == null ? spawnLocation.z + prefabOffset.z : prefabParent.TransformPoint(prefabOffset).z);
                //Vector3 finalSpawnLocation = new Vector3(spawnLocation.x + finalX, spawnLocation.y + prefabOffset.y, spawnLocation.z + finalZ);
                Vector3 finalSpawnLocation = new Vector3(finalX, finalY, finalZ);
                //Debug.Log("Instantiating Ability Effect Prefab for: " + MyName + " at " + finalSpawnLocation + "; prefabParent: " + (prefabParent == null ? "null " : prefabParent.name) + "; abilityEffectPrefab: " + abilityEffectPrefab);
                //abilityEffectObject = Instantiate(abilityEffectPrefab, finalSpawnLocation, Quaternion.Euler(prefabRotation), prefabParent);
                //abilityEffectObject = Instantiate(abilityEffectPrefab, finalSpawnLocation, source.MyCharacterUnit.transform.rotation * Quaternion.Euler(prefabRotation), prefabParent);
                //abilityEffectObject = Instantiate(abilityEffectPrefab, finalSpawnLocation, source.MyCharacterUnit.transform.rotation * Quaternion.LookRotation(prefabRotation), prefabParent);
                // CORRECT WAY BELOW
                //abilityEffectObject = Instantiate(abilityEffectPrefab, finalSpawnLocation, Quaternion.LookRotation(source.MyCharacterUnit.transform.forward) * Quaternion.Euler(prefabRotation), PlayerManager.MyInstance.MyEffectPrefabParent.transform);
                abilityEffectObject = Instantiate(abilityEffectPrefab, finalSpawnLocation, Quaternion.LookRotation(source.MyCharacterUnit.transform.forward) * Quaternion.Euler(prefabRotation), prefabParent);
                /*
                abilityEffectObject = Instantiate(abilityEffectPrefab, prefabParent, true);
                abilityEffectObject.transform.position = finalSpawnLocation;
                abilityEffectObject.transform.rotation = Quaternion.LookRotation(source.MyCharacterUnit.transform.forward) * Quaternion.Euler(prefabRotation);
                */
                BeginMonitoring(abilityEffectObject, source, target, abilityEffectInput);
            }
        }

        public void CreateAbilityObject() {

        }

        public virtual void CastTick(BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log(abilityEffectName + ".AbilityEffect.CastTick(" + source.name + ", " + (target ? target.name : "null") + ")");
        }

        public virtual void CastComplete(BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log(abilityEffectName + ".AbilityEffect.CastComplete(" + source.name + ", " + (target ? target.name : "null") + ")");
        }

        protected virtual void BeginMonitoring(GameObject abilityEffectObject, BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log(abilityEffectName + ".AbilityEffect.BeginMonitoring(" + source.name + ", " + (target == null ? "null" : target.name) + ")");
            // overwrite me
        }

        public virtual void PerformAbilityTickEffects(BaseCharacter source, GameObject target, AbilityEffectOutput effectOutput) {
            PerformAbilityEffects(source, target, effectOutput, tickAbilityEffectList);
        }

        public virtual void PerformAbilityCompleteEffects(BaseCharacter source, GameObject target, AbilityEffectOutput effectOutput) {
            PerformAbilityEffects(source, target, effectOutput, completeAbilityEffectList);
        }

        public virtual void PerformAbilityTick(BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log(abilityEffectName + ".AbilityEffect.PerformAbilityTick(" + source.name + ", " + (target == null ? "null" : target.name) + ")");
            PerformAbilityTickEffects(source, target, abilityEffectInput);
        }

        public virtual void PerformAbilityComplete(BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log(abilityEffectName + ".AbilityEffect.PerformAbilityComplete(" + source.name + ", " + (target == null ? "null" : target.name) + ")");
            PerformAbilityCompleteEffects(source, target, abilityEffectInput);
        }

        public virtual void CancelEffect(BaseCharacter targetCharacter) {
            if (abilityEffectObject != null) {
                // give slight delay to allow for graphic effects to finish
                Destroy(abilityEffectObject, prefabDestroyDelay);
            }

        }


    }
}