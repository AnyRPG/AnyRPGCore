using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New LengthEffect", menuName = "AnyRPG/Abilities/Effects/LengthEffect")]
    public class LengthEffect : AbilityEffect {

        [Header("Prefab")]

        [Tooltip("Ability: use ability prefabs, Both: use weapon and ability prefabs, Weapon: use only weapon prefabs")]
        [SerializeField]
        protected AbilityPrefabSource abilityPrefabSource = AbilityPrefabSource.Ability;

        //[SerializeField]
        //private List<string> prefabNames = new List<string>();

        [Tooltip("randomly select a prefab instead of spawning all of them")]
        [SerializeField]
        private bool randomPrefabs = false;

        [Tooltip("Physical prefabs to attach to bones on the character unit when this weapon is being used during an attack.  This could be arrows, special spell or glow effects, etc")]
        [SerializeField]
        private List<AbilityAttachmentNode> abilityObjectList = new List<AbilityAttachmentNode>();

        //private List<PrefabProfile> prefabProfileList = new List<PrefabProfile>();

        [SerializeField]
        protected PrefabSpawnLocation prefabSpawnLocation;

        [Tooltip("a delay after the effect ends to destroy the spell effect prefab")]
        [SerializeField]
        protected float prefabDestroyDelay = 0f;

        [Tooltip("If true, the prefab will be destroyed when casting ends, regardless of prefab lifetime")]
        [SerializeField]
        protected bool destroyOnEndCast = false;

        [Header("Tick")]

        [Tooltip("every <tickRate> seconds, the Tick() will occur")]
        [SerializeField]
        protected float tickRate;

        [Tooltip("do we cast an immediate tick at zero seconds")]
        [SerializeField]
        protected bool castZeroTick;

        [Tooltip("any abilities to cast every tick")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AbilityEffect))]
        protected List<string> tickAbilityEffectNames = new List<string>();

        //[SerializeField]
        protected List<AbilityEffect> tickAbilityEffectList = new List<AbilityEffect>();

        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        protected List<string> onTickAudioProfileNames = new List<string>();

        [Tooltip("whether to play all audio profiles or just one random one")]
        [SerializeField]
        protected bool randomTickAudioProfiles = false;

        //protected AudioProfile onHitAudioProfile;
        protected List<AudioProfile> onTickAudioProfiles = new List<AudioProfile>();

        [Header("Complete")]

        [Tooltip("any abilities to cast when the effect completes")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AbilityEffect))]
        protected List<string> completeAbilityEffectNames = new List<string>();

        //[SerializeField]
        protected List<AbilityEffect> completeAbilityEffectList = new List<AbilityEffect>();

        // game manager references
        protected ObjectPooler objectPooler = null;

        public List<AbilityEffect> MyTickAbilityEffectList { get => tickAbilityEffectList; set => tickAbilityEffectList = value; }
        public List<AbilityEffect> MyCompleteAbilityEffectList { get => completeAbilityEffectList; set => completeAbilityEffectList = value; }
        public float TickRate { get => tickRate; set => tickRate = value; }
        public float PrefabDestroyDelay { get => prefabDestroyDelay; set => prefabDestroyDelay = value; }
        public PrefabSpawnLocation PrefabSpawnLocation { get => prefabSpawnLocation; set => prefabSpawnLocation = value; }
        //public GameObject MyAbilityEffectPrefab { get => abilityEffectPrefab; set => abilityEffectPrefab = value; }
        public bool CastZeroTick { get => castZeroTick; set => castZeroTick = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            objectPooler = systemGameManager.ObjectPooler;
        }

        public List<AbilityAttachmentNode> GetPrefabProfileList(IAbilityCaster abilityCaster) {
            if (abilityPrefabSource == AbilityPrefabSource.Both) {
                List<AbilityAttachmentNode> returnList = new List<AbilityAttachmentNode>();
                returnList.AddRange(abilityObjectList);
                foreach (AbilityAttachmentNode abilityAttachmentNode in abilityCaster.AbilityManager.GetWeaponAbilityObjectList()) {
                    if (abilityAttachmentNode.HoldableObject != null) {
                        returnList.Add(abilityAttachmentNode);
                    }
                }
                return returnList;
            }
            if (abilityPrefabSource == AbilityPrefabSource.Weapon) {
                List<AbilityAttachmentNode> returnList = new List<AbilityAttachmentNode>();
                foreach (AbilityAttachmentNode abilityAttachmentNode in abilityCaster.AbilityManager.GetWeaponAbilityObjectList()) {
                    if (abilityAttachmentNode.HoldableObject != null) {
                        returnList.Add(abilityAttachmentNode);
                    }
                }
                return returnList;
            }

            // abilityPrefabSource is AbilityPrefabSource.Ability since there are only 3 options
            return abilityObjectList;

        }

        public override Dictionary<PrefabProfile, GameObject> Cast(IAbilityCaster source, Interactable target, Interactable originalTarget, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(DisplayName + ".LengthEffect.Cast(" + (source == null ? "null" :source.AbilityManager.Name) + ", " + (target == null ? "null" : target.gameObject.name) + ", " + (originalTarget == null ? "null" : originalTarget.name) + ")");

            Dictionary<PrefabProfile, GameObject> prefabObjects = base.Cast(source, target, originalTarget, abilityEffectInput);
            if (GetPrefabProfileList(source) != null) {
                List<AbilityAttachmentNode> usedAbilityAttachmentNodeList = new List<AbilityAttachmentNode>();
                if (randomPrefabs == false) {
                    usedAbilityAttachmentNodeList = GetPrefabProfileList(source);
                } else {
                    //PrefabProfile copyProfile = prefabProfileList[UnityEngine.Random.Range(0, prefabProfileList.Count -1)];
                    usedAbilityAttachmentNodeList.Add(GetPrefabProfileList(source)[UnityEngine.Random.Range(0, GetPrefabProfileList(source).Count)]);
                }
                foreach (AbilityAttachmentNode abilityAttachmentNode in usedAbilityAttachmentNodeList) {
                    if (abilityAttachmentNode.HoldableObject != null && abilityAttachmentNode.HoldableObject.Prefab != null) {
                        Vector3 spawnLocation = Vector3.zero;
                        Transform prefabParent = null;
                        Vector3 nodePosition = abilityAttachmentNode.HoldableObject.Position;
                        Vector3 nodeRotation = abilityAttachmentNode.HoldableObject.Rotation;
                        Vector3 nodeScale = abilityAttachmentNode.HoldableObject.Scale;
                        if (prefabSpawnLocation == PrefabSpawnLocation.GroundTarget) {
                            //Debug.Log(resourceName + ".LengthEffect.Cast(): prefabspawnlocation: point; abilityEffectInput.prefabLocation: " + abilityEffectInput.prefabLocation);
                            //spawnLocation =source.AbilityManager.GetComponent<Collider>().bounds.center;
                            spawnLocation = abilityEffectInput.groundTargetLocation;
                            prefabParent = null;
                        }
                        if (prefabSpawnLocation == PrefabSpawnLocation.TargetPoint && target != null) {
                            //Debug.Log(resourceName + ".LengthEffect.Cast(): prefabspawnlocation: point; abilityEffectInput.prefabLocation: " + abilityEffectInput.prefabLocation);
                            //spawnLocation =source.AbilityManager.GetComponent<Collider>().bounds.center;
                            spawnLocation = target.transform.position;
                            prefabParent = null;
                        }
                        if ((prefabSpawnLocation == PrefabSpawnLocation.Caster || prefabSpawnLocation == PrefabSpawnLocation.CasterPoint) && target != null) {
                            //Debug.Log(MyName + ".LengthEffect.Cast(): PrefabSpawnLocation is Caster");
                            //spawnLocation =source.AbilityManager.GetComponent<Collider>().bounds.center;
                            AttachmentPointNode attachmentPointNode = source.AbilityManager.GetHeldAttachmentPointNode(abilityAttachmentNode);
                            nodePosition = attachmentPointNode.Position;
                            nodeRotation = attachmentPointNode.Rotation;
                            nodeScale = attachmentPointNode.Scale;
                            spawnLocation = source.AbilityManager.UnitGameObject.transform.position;
                            if (prefabSpawnLocation == PrefabSpawnLocation.CasterPoint) {
                                prefabParent = null;
                            } else {
                                prefabParent = source.AbilityManager.UnitGameObject.transform;
                                Transform usedPrefabSourceBone = null;
                                if (attachmentPointNode.TargetBone != null && attachmentPointNode.TargetBone != string.Empty) {
                                    usedPrefabSourceBone = prefabParent.FindChildByRecursive(attachmentPointNode.TargetBone);
                                }
                                if (usedPrefabSourceBone != null) {
                                    prefabParent = usedPrefabSourceBone;
                                }
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
                        if (prefabSpawnLocation != PrefabSpawnLocation.None &&
                            (target != null || prefabSpawnLocation == PrefabSpawnLocation.GroundTarget || GetTargetOptions(source).RequireTarget == false)) {
                            float finalX = (prefabParent == null ? spawnLocation.x + nodePosition.x : prefabParent.TransformPoint(nodePosition).x);
                            float finalY = (prefabParent == null ? spawnLocation.y + nodePosition.y : prefabParent.TransformPoint(nodePosition).y);
                            float finalZ = (prefabParent == null ? spawnLocation.z + nodePosition.z : prefabParent.TransformPoint(nodePosition).z);
                            //Vector3 finalSpawnLocation = new Vector3(spawnLocation.x + finalX, spawnLocation.y + prefabOffset.y, spawnLocation.z + finalZ);
                            Vector3 finalSpawnLocation = new Vector3(finalX, finalY, finalZ);
                            //Debug.Log("Instantiating Ability Effect Prefab for: " + MyName + " at " + finalSpawnLocation + "; prefabParent: " + (prefabParent == null ? "null " : prefabParent.name) + ";");
                            Vector3 usedForwardDirection = Vector3.forward;
                            if (source != null && source.AbilityManager.UnitGameObject != null) {
                                usedForwardDirection = source.AbilityManager.UnitGameObject.transform.forward;
                            }
                            if (prefabParent != null) {
                                usedForwardDirection = prefabParent.transform.forward;
                            }
                            GameObject prefabObject = objectPooler.GetPooledObject(abilityAttachmentNode.HoldableObject.Prefab,
                                finalSpawnLocation,
                                Quaternion.LookRotation(usedForwardDirection) * Quaternion.Euler(nodeRotation),
                                prefabParent);
                            prefabObject.transform.localScale = nodeScale;
                            prefabObjects[abilityAttachmentNode.HoldableObject] = prefabObject;
                            if (destroyOnEndCast) {
                                source.AbilityManager.AddAbilityObject(abilityAttachmentNode, prefabObject);
                            }
                        }
                    }
                }
                BeginMonitoring(prefabObjects, source, target, abilityEffectInput);
            }
            abilityEffectInput.PrefabObjects = prefabObjects;
            return prefabObjects;
        }

        public virtual void CastTick(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(DisplayName + ".AbilityEffect.CastTick(" +source.AbilityManager.Name + ", " + (target ? target.name : "null") + ")");
            // play tick audio effects
            PlayAudioEffects(onTickAudioProfiles, target, abilityEffectContext);
        }

        public virtual void CastComplete(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(abilityEffectName + ".AbilityEffect.CastComplete(" +source.AbilityManager.name + ", " + (target ? target.name : "null") + ")");
        }

        protected virtual void BeginMonitoring(Dictionary<PrefabProfile, GameObject> abilityEffectObjects, IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(MyName + ".LengthEffect.BeginMonitoring(" +source.AbilityManager.name + ", " + (target == null ? "null" : target.name) + ")");
            // overwrite me
        }

        public virtual void PerformAbilityTickEffects(IAbilityCaster source, Interactable target, AbilityEffectContext effectOutput) {
            PerformAbilityEffects(source, target, effectOutput, tickAbilityEffectList);
        }

        public virtual void PerformAbilityCompleteEffects(IAbilityCaster source, Interactable target, AbilityEffectContext effectOutput) {
            PerformAbilityEffects(source, target, effectOutput, completeAbilityEffectList);
        }

        public virtual void PerformAbilityTick(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(abilityEffectName + ".AbilityEffect.PerformAbilityTick(" +source.AbilityManager.name + ", " + (target == null ? "null" : target.name) + ")");
            PerformAbilityTickEffects(source, target, abilityEffectInput);
        }

        public virtual void PerformAbilityComplete(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(abilityEffectName + ".AbilityEffect.PerformAbilityComplete(" +source.AbilityManager.name + ", " + (target == null ? "null" : target.name) + ")");
            PerformAbilityCompleteEffects(source, target, abilityEffectInput);
        }

        public virtual void CancelEffect(BaseCharacter targetCharacter) {
            //Debug.Log(DisplayName + ".LengthEffect.CancelEffect(" + targetCharacter.MyName + ")");
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);
            tickAbilityEffectList = new List<AbilityEffect>();
            if (tickAbilityEffectNames != null) {
                foreach (string abilityEffectName in tickAbilityEffectNames) {
                    AbilityEffect abilityEffect = systemDataFactory.GetResource<AbilityEffect>(abilityEffectName);
                    if (abilityEffect != null) {
                        tickAbilityEffectList.Add(abilityEffect);
                    } else {
                        Debug.LogError("LengthEffect.SetupScriptableObjects(): Could not find ability effect: " + abilityEffectName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            completeAbilityEffectList = new List<AbilityEffect>();
            if (completeAbilityEffectNames != null) {
                foreach (string abilityEffectName in completeAbilityEffectNames) {
                    AbilityEffect abilityEffect = systemDataFactory.GetResource<AbilityEffect>(abilityEffectName);
                    if (abilityEffect != null) {
                        completeAbilityEffectList.Add(abilityEffect);
                    } else {
                        Debug.LogError("LengthEffect.SetupScriptableObjects(): Could not find ability effect: " + abilityEffectName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            /*
            if (prefabNames != null) {
                if (prefabNames.Count > 0 && prefabSpawnLocation == PrefabSpawnLocation.None) {
                    Debug.LogError("LengthEffect.SetupScriptableObjects(): prefabnames is not null but PrefabSpawnLocation is none while inititalizing " + DisplayName + ".  CHECK INSPECTOR BECAUSE OBJECTS WILL NEVER SPAWN");
                }
                foreach (string prefabName in prefabNames) {
                    PrefabProfile prefabProfile = systemDataFactory.GetResource<PrefabProfile>(prefabName);
                    if (prefabProfile != null) {
                        prefabProfileList.Add(prefabProfile);
                    } else {
                        Debug.LogError("LengthEffect.SetupScriptableObjects(): Could not find prefab Profile : " + prefabName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }
            */

            onTickAudioProfiles = new List<AudioProfile>();
            if (onTickAudioProfileNames != null) {
                foreach (string audioProfileName in onTickAudioProfileNames) {
                    AudioProfile audioProfile = systemDataFactory.GetResource<AudioProfile>(audioProfileName);
                    if (audioProfile != null) {
                        onTickAudioProfiles.Add(audioProfile);
                    } else {
                        Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find audio profile: " + audioProfileName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            if (abilityObjectList != null) {
                foreach (AbilityAttachmentNode abilityAttachmentNode in abilityObjectList) {
                    if (abilityAttachmentNode != null) {
                        abilityAttachmentNode.SetupScriptableObjects(DisplayName, systemGameManager);
                    }
                }
            }


        }

    }
}