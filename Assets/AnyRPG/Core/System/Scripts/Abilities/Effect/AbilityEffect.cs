using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New AbilityEffect", menuName = "AnyRPG/Abilities/Effects/AbilityEffect")]
    public class AbilityEffect : DescribableResource {

        /*
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
        private float materialChangeDuration = 2f;

        [Header("Audio")]

        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        private List<string> onHitAudioProfileNames = new List<string>();

        [Tooltip("whether to play all audio profiles or just one random one")]
        [SerializeField]
        private bool randomAudioProfiles = false;

        //protected AudioProfile onHitAudioProfile;
        protected List<AudioProfile> onHitAudioProfiles = new List<AudioProfile>();

        [Header("Hit")]
        [Tooltip("any abilities to cast immediately on hit")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AbilityEffect))]
        private List<string> hitAbilityEffectNames = new List<string>();

        //[SerializeField]
        protected List<AbilityEffect> hitAbilityEffectList = new List<AbilityEffect>();

        [Tooltip("amount to multiply inputs by when adding their amount to this effect")]
        public float inputMultiplier = 0f;

        [SerializeField]
        protected float threatMultiplier = 1f;

        public List<AbilityEffect> HitAbilityEffectList { get => hitAbilityEffectList; set => hitAbilityEffectList = value; }
        public float ThreatMultiplier { get => threatMultiplier; set => threatMultiplier = value; }
        public float ChanceToCast { get => chanceToCast; set => chanceToCast = value; }
        public AbilityEffectTargetProps TargetOptions { get => targetOptions; set => targetOptions = value; }
        public string EffectMaterialName { get => effectMaterialName; set => effectMaterialName = value; }
        public float MaterialChangeDuration { get => materialChangeDuration; set => materialChangeDuration = value; }
        public List<string> OnHitAudioProfileNames { get => onHitAudioProfileNames; set => onHitAudioProfileNames = value; }
        public bool RandomAudioProfiles { get => randomAudioProfiles; set => randomAudioProfiles = value; }
        public List<string> HitAbilityEffectNames { get => hitAbilityEffectNames; set => hitAbilityEffectNames = value; }

        */

        public virtual AbilityEffectProperties AbilityEffectProperties { get => null; }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            AbilityEffectProperties.SetupScriptableObjects(systemGameManager);
        }
    }
}