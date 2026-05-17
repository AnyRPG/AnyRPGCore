using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class LengthEffectProperties : AbilityEffectProperties {
        /*
        [Header("Prefab")]

        [Tooltip("Ability: use ability prefabs, Both: use weapon and ability prefabs, Weapon: use only weapon prefabs")]
        [SerializeField]
        protected AbilityPrefabSource abilityPrefabSource = AbilityPrefabSource.Ability;

        //[SerializeField]
        //private List<string> prefabNames = new List<string>();

        [Tooltip("randomly select a prefab instead of spawning all of them")]
        [SerializeField]
        protected bool randomPrefabs = false;

        [Tooltip("Prefabs to spawn when this effect is cast")]
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
        */

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
        protected List<AbilityEffectProperties> tickAbilityEffectList = new List<AbilityEffectProperties>();

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
        protected List<AbilityEffectProperties> completeAbilityEffectList = new List<AbilityEffectProperties>();

        // game manager references
        protected ObjectPooler objectPooler = null;

        public List<AbilityEffectProperties> TickAbilityEffectList { get => tickAbilityEffectList; set => tickAbilityEffectList = value; }
        public List<AbilityEffectProperties> CompleteAbilityEffectList { get => completeAbilityEffectList; set => completeAbilityEffectList = value; }
        public float TickRate { get => tickRate; set => tickRate = value; }
        public bool CastZeroTick { get => castZeroTick; set => castZeroTick = value; }

        /*
        public void GetLengthEffectProperties(LengthEffect effect) {

            abilityPrefabSource = effect.AbilityPrefabSource;
            randomPrefabs = effect.RandomPrefabs;
            abilityObjectList = effect.AbilityObjectList;
            prefabSpawnLocation = effect.PrefabSpawnLocation;
            prefabDestroyDelay = effect.PrefabDestroyDelay;
            destroyOnEndCast = effect.DestroyOnEndCast;
            tickRate = effect.TickRate;
            castZeroTick = effect.CastZeroTick;
            tickAbilityEffectNames = effect.TickAbilityEffectNames;
            onTickAudioProfileNames = effect.OnTickAudioProfileNames;
            randomTickAudioProfiles = effect.RandomTickAudioProfiles;
            completeAbilityEffectNames = effect.CompleteAbilityEffectNames;

            GetAbilityEffectProperties(effect);
        }
        */

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            objectPooler = systemGameManager.ObjectPooler;
        }


        public virtual void CastTick(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(DisplayName + ".AbilityEffect.CastTick(" +source.AbilityManager.Name + ", " + (target ? target.name : "null") + ")");
            // play tick audio effects
            PlayAudioEffects(onTickAudioProfiles, target, abilityEffectContext);
        }

        public virtual void CastComplete(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(abilityEffectName + ".AbilityEffect.CastComplete(" +source.AbilityManager.name + ", " + (target ? target.name : "null") + ")");
        }

        public virtual void BeginMonitoring(Dictionary<PrefabProfile, List<GameObject>> abilityEffectObjects, IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(DisplayName + ".LengthEffect.BeginMonitoring(" +source.AbilityManager.name + ", " + (target == null ? "null" : target.name) + ")");
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

        public virtual void CancelEffect(UnitController unitController) {
            //Debug.Log(DisplayName + ".LengthEffect.CancelEffect(" + targetCharacter.DisplayName + ")");
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager, IDescribable describable) {
            base.SetupScriptableObjects(systemGameManager, describable);
            tickAbilityEffectList = new List<AbilityEffectProperties>();
            if (tickAbilityEffectNames != null) {
                foreach (string abilityEffectName in tickAbilityEffectNames) {
                    AbilityEffect abilityEffect = systemDataFactory.GetResource<AbilityEffect>(abilityEffectName);
                    if (abilityEffect != null) {
                        tickAbilityEffectList.Add(abilityEffect.AbilityEffectProperties);
                    } else {
                        Debug.LogError("LengthEffect.SetupScriptableObjects(): Could not find ability effect: " + abilityEffectName + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
                    }
                }
            }

            completeAbilityEffectList = new List<AbilityEffectProperties>();
            if (completeAbilityEffectNames != null) {
                foreach (string abilityEffectName in completeAbilityEffectNames) {
                    AbilityEffect abilityEffect = systemDataFactory.GetResource<AbilityEffect>(abilityEffectName);
                    if (abilityEffect != null) {
                        completeAbilityEffectList.Add(abilityEffect.AbilityEffectProperties);
                    } else {
                        Debug.LogError("LengthEffect.SetupScriptableObjects(): Could not find ability effect: " + abilityEffectName + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
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
                        Debug.LogError("LengthEffect.SetupScriptableObjects(): Could not find prefab Profile : " + prefabName + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
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
                        Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find audio profile: " + audioProfileName + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
                    }
                }
            }


        }

    }
}