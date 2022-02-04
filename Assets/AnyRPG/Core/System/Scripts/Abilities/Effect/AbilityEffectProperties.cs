using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public abstract class AbilityEffectProperties : ConfiguredClass {

        [Header("Target Properties")]

        [SerializeField]
        private AbilityEffectTargetProps targetOptions = new AbilityEffectTargetProps();

        [Tooltip("The chance this ability will be cast.  100 = 100%")]
        [SerializeField]
        private float chanceToCast = 100f;

        [Header("Material Changes")]

        [SerializeField]
        [ResourceSelector(resourceType = typeof(MaterialProfile))]
        private string effectMaterialName = string.Empty;

        // a material to temporarily assign to the target we hit
        //[SerializeField]
        private Material effectMaterial;

        [Tooltip("The length, in seconds, that any material change (such as ice freeze) should last.")]
        [SerializeField]
        protected float materialChangeDuration = 2f;

        [Header("Audio")]

        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        protected List<string> onHitAudioProfileNames = new List<string>();

        [Tooltip("whether to play all audio profiles or just one random one")]
        [SerializeField]
        protected bool randomAudioProfiles = false;

        //protected AudioProfile onHitAudioProfile;
        protected List<AudioProfile> onHitAudioProfiles = new List<AudioProfile>();

        [Header("Hit")]
        [Tooltip("any abilities to cast immediately on hit")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AbilityEffect))]
        protected List<string> hitAbilityEffectNames = new List<string>();

        //[SerializeField]
        protected List<AbilityEffect> hitAbilityEffectList = new List<AbilityEffect>();

        [Tooltip("amount to multiply inputs by when adding their amount to this effect")]
        public float inputMultiplier = 0f;

        [SerializeField]
        protected float threatMultiplier = 1f;

        protected string displayName;

        public List<AbilityEffect> MyHitAbilityEffectList { get => hitAbilityEffectList; set => hitAbilityEffectList = value; }
        public float ThreatMultiplier { get => threatMultiplier; set => threatMultiplier = value; }
        public float ChanceToCast { get => chanceToCast; set => chanceToCast = value; }
        public string DisplayName { get => displayName; set => displayName = value; }
        public float MaterialChangeDuration { get => materialChangeDuration; set => materialChangeDuration = value; }

        public virtual void SetupScriptableObjects(SystemGameManager systemGameManager, string displayName) {
            //Debug.Log(DisplayName + ".AbilityEffect.SetupscriptableObjects()");

            this.displayName = displayName;
            Configure(systemGameManager);

            hitAbilityEffectList = new List<AbilityEffect>();
            if (hitAbilityEffectNames != null) {
                foreach (string abilityEffectName in hitAbilityEffectNames) {
                    AbilityEffect abilityEffect = systemDataFactory.GetResource<AbilityEffect>(abilityEffectName);
                    if (abilityEffect != null) {
                        hitAbilityEffectList.Add(abilityEffect);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability effect: " + abilityEffectName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            onHitAudioProfiles = new List<AudioProfile>();
            if (onHitAudioProfileNames != null) {
                foreach (string audioProfileName in onHitAudioProfileNames) {
                    AudioProfile audioProfile = systemDataFactory.GetResource<AudioProfile>(audioProfileName);
                    if (audioProfile != null) {
                        onHitAudioProfiles.Add(audioProfile);
                    } else {
                        Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find audio profile: " + audioProfileName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            if (effectMaterialName != null && effectMaterialName != string.Empty) {
                effectMaterial = null;
                MaterialProfile tmpMaterialProfile = systemDataFactory.GetResource<MaterialProfile>(effectMaterialName);
                if (tmpMaterialProfile != null) {
                    effectMaterial = tmpMaterialProfile.MyEffectMaterial;
                } else {
                    Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find material profile: " + effectMaterialName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

        }

    }

    public enum LineOfSightSourceLocation { Caster, GroundTarget, OriginalTarget }

    public enum TargetRangeSourceLocation { Caster, GroundTarget, OriginalTarget }
}