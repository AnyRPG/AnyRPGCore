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
        private List<string> prefabNames = new List<string>();

        private List<PrefabProfile> prefabProfileList = new List<PrefabProfile>();

        /*
        [SerializeField]
        protected GameObject abilityEffectPrefab = null;
        */

        [SerializeField]
        protected PrefabSpawnLocation prefabSpawnLocation;

        /*
        [SerializeField]
        protected string prefabSourceBone;

        [SerializeField]
        protected Vector3 prefabOffset = Vector3.zero;

        [SerializeField]
        protected Vector3 prefabRotation = Vector3.zero;
        */

        // a delay after the effect ends to destroy the spell effect prefab
        [SerializeField]
        protected float prefabDestroyDelay = 0f;

        /// <summary>
        /// the reference to the gameobject spawned by this ability
        /// </summary>
        //protected GameObject abilityEffectObject = null;

        protected Dictionary<PrefabProfile, GameObject> prefabObjects = new Dictionary<PrefabProfile, GameObject>();

        // every <tickRate> seconds, the Tick() will occur
        [SerializeField]
        protected float tickRate;

        // do we cast an immediate tick at zero seconds
        [SerializeField]
        protected bool castZeroTick;

        // any abilities to cast every tick
        [SerializeField]
        protected List<string> tickAbilityEffectNames = new List<string>();

        //[SerializeField]
        protected List<AbilityEffect> tickAbilityEffectList = new List<AbilityEffect>();

        //private float nextTickTime;
        protected DateTime nextTickTime;

        // any abilities to cast when the effect completes
        [SerializeField]
        protected List<string> completeAbilityEffectNames = new List<string>();

        //[SerializeField]
        protected List<AbilityEffect> completeAbilityEffectList = new List<AbilityEffect>();

        public List<AbilityEffect> MyTickAbilityEffectList { get => tickAbilityEffectList; set => tickAbilityEffectList = value; }
        public List<AbilityEffect> MyCompleteAbilityEffectList { get => completeAbilityEffectList; set => completeAbilityEffectList = value; }
        public float MyTickRate { get => tickRate; set => tickRate = value; }
        public DateTime MyNextTickTime { get => nextTickTime; set => nextTickTime = value; }
        public float MyPrefabDestroyDelay { get => prefabDestroyDelay; set => prefabDestroyDelay = value; }
        public PrefabSpawnLocation MyPrefabSpawnLocation { get => prefabSpawnLocation; set => prefabSpawnLocation = value; }
        //public GameObject MyAbilityEffectPrefab { get => abilityEffectPrefab; set => abilityEffectPrefab = value; }
        public bool MyCastZeroTick { get => castZeroTick; set => castZeroTick = value; }
        protected Dictionary<PrefabProfile, GameObject> MyPrefabObjects { get => prefabObjects; set => prefabObjects = value; }

        public override Dictionary<PrefabProfile, GameObject> Cast(BaseCharacter source, GameObject target, GameObject originalTarget, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log(MyName + ".LengthEffect.Cast(" + (source == null ? "null" : source.name) + ", " + (target == null ? "null" : target.name) + ")");
            
            base.Cast(source, target, originalTarget, abilityEffectInput);
            if (prefabProfileList != null) {
                foreach (PrefabProfile prefabProfile in prefabProfileList) {
                    Vector3 spawnLocation = Vector3.zero;
                    Transform prefabParent = null;
                    if (prefabSpawnLocation == PrefabSpawnLocation.Point) {
                        //Debug.Log(resourceName + ".AbilityEffect.Cast(): prefabspawnlocation: point; abilityEffectInput.prefabLocation: " + abilityEffectInput.prefabLocation);
                        //spawnLocation = source.GetComponent<Collider>().bounds.center;
                        spawnLocation = abilityEffectInput.prefabLocation;
                        prefabParent = null;
                    }
                    if (prefabSpawnLocation == PrefabSpawnLocation.Caster) {
                        //Debug.Log(MyName + ".LengthEffect.Cast(): PrefabSpawnLocation is Caster");
                        //spawnLocation = source.GetComponent<Collider>().bounds.center;
                        spawnLocation = source.MyCharacterUnit.transform.position;
                        prefabParent = source.MyCharacterUnit.transform;
                        Transform usedPrefabSourceBone = null;
                        if (prefabProfile.MyTargetBone != null && prefabProfile.MyTargetBone != string.Empty) {
                            usedPrefabSourceBone = prefabParent.FindChildByRecursive(prefabProfile.MyTargetBone);
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
                    if (prefabSpawnLocation != PrefabSpawnLocation.None && (target != null || prefabSpawnLocation == PrefabSpawnLocation.Point || requiresTarget == false)) {
                        //float finalX = (prefabParent == null ? prefabOffset.x : prefabParent.TransformPoint(prefabOffset).x);
                        //float finalY = (prefabParent == null ? prefabOffset.y : prefabParent.TransformPoint(prefabOffset).x);
                        //float finalZ = (prefabParent == null ? prefabOffset.z : prefabParent.TransformPoint(prefabOffset).z);
                        float finalX = (prefabParent == null ? spawnLocation.x + prefabProfile.MyPosition.x : prefabParent.TransformPoint(prefabProfile.MyPosition).x);
                        float finalY = (prefabParent == null ? spawnLocation.y + prefabProfile.MyPosition.y : prefabParent.TransformPoint(prefabProfile.MyPosition).y);
                        float finalZ = (prefabParent == null ? spawnLocation.z + prefabProfile.MyPosition.z : prefabParent.TransformPoint(prefabProfile.MyPosition).z);
                        //Vector3 finalSpawnLocation = new Vector3(spawnLocation.x + finalX, spawnLocation.y + prefabOffset.y, spawnLocation.z + finalZ);
                        Vector3 finalSpawnLocation = new Vector3(finalX, finalY, finalZ);
                        //Debug.Log("Instantiating Ability Effect Prefab for: " + MyName + " at " + finalSpawnLocation + "; prefabParent: " + (prefabParent == null ? "null " : prefabParent.name) + ";");
                        // CORRECT WAY BELOW
                        //abilityEffectObject = Instantiate(abilityEffectPrefab, finalSpawnLocation, Quaternion.LookRotation(source.MyCharacterUnit.transform.forward) * Quaternion.Euler(prefabRotation), PlayerManager.MyInstance.MyEffectPrefabParent.transform);
                        GameObject prefabObject = Instantiate(prefabProfile.MyPrefab, finalSpawnLocation, Quaternion.LookRotation(source.MyCharacterUnit.transform.forward) * Quaternion.Euler(prefabProfile.MyRotation), prefabParent);
                        if (prefabObject == null) {
                            //Debug.Log(MyName + ".LengthEffect.Cast(): prefabObject = null");
                        } else {
                            //Debug.Log(MyName + ".LengthEffect.Cast(): PREFAB SPAWNED PROPERLY AND IS NAMED: " + prefabObject.name);
                        }
                        prefabObjects[prefabProfile] = prefabObject;
                        //abilityEffectObject =
                    }

                }
                BeginMonitoring(prefabObjects, source, target, abilityEffectInput);
            }
            return prefabObjects;
        }

        public void CreateAbilityObject() {

        }

        public virtual void CastTick(BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log(abilityEffectName + ".AbilityEffect.CastTick(" + source.name + ", " + (target ? target.name : "null") + ")");
        }

        public virtual void CastComplete(BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log(abilityEffectName + ".AbilityEffect.CastComplete(" + source.name + ", " + (target ? target.name : "null") + ")");
        }

        protected virtual void BeginMonitoring(Dictionary<PrefabProfile, GameObject> abilityEffectObjects, BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput) {
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
            if (prefabObjects != null) {
                foreach (GameObject go in prefabObjects.Values) {
                    Destroy(go, prefabDestroyDelay);
                }
                // give slight delay to allow for graphic effects to finish
            }

        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            tickAbilityEffectList = new List<AbilityEffect>();
            if (tickAbilityEffectNames != null) {
                foreach (string abilityEffectName in tickAbilityEffectNames) {
                    AbilityEffect abilityEffect = SystemAbilityEffectManager.MyInstance.GetResource(abilityEffectName);
                    if (abilityEffect != null) {
                        tickAbilityEffectList.Add(abilityEffect);
                    } else {
                        Debug.LogError("LengthEffect.SetupScriptableObjects(): Could not find ability effect: " + abilityEffectName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                    }
                }
            }

            completeAbilityEffectList = new List<AbilityEffect>();
            if (completeAbilityEffectNames != null) {
                foreach (string abilityEffectName in completeAbilityEffectNames) {
                    AbilityEffect abilityEffect = SystemAbilityEffectManager.MyInstance.GetResource(abilityEffectName);
                    if (abilityEffect != null) {
                        completeAbilityEffectList.Add(abilityEffect);
                    } else {
                        Debug.LogError("LengthEffect.SetupScriptableObjects(): Could not find ability effect: " + abilityEffectName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                    }
                }
            }

            prefabProfileList = new List<PrefabProfile>();
            if (prefabNames != null) {
                if (prefabNames.Count > 0 && prefabSpawnLocation == PrefabSpawnLocation.None) {
                    Debug.LogError("LengthEffect.SetupScriptableObjects(): prefabnames is not null but PrefabSpawnLocation is none while inititalizing " + MyName + ".  CHECK INSPECTOR BECAUSE OBJECTS WILL NEVER SPAWN");
                }
                foreach (string prefabName in prefabNames) {
                    PrefabProfile prefabProfile = SystemPrefabProfileManager.MyInstance.GetResource(prefabName);
                    if (prefabProfile != null) {
                        prefabProfileList.Add(prefabProfile);
                    } else {
                        Debug.LogError("LengthEffect.SetupScriptableObjects(): Could not find prefab Profile : " + prefabName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                    }
                }
            }

        }

    }
}