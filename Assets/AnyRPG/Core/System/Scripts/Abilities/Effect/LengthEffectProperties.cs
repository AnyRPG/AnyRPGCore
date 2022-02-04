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
        public bool CastZeroTick { get => castZeroTick; set => castZeroTick = value; }
        public bool RandomPrefabs { get => randomPrefabs; set => randomPrefabs = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            objectPooler = systemGameManager.ObjectPooler;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager, string displayName) {
            base.SetupScriptableObjects(systemGameManager, displayName);

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