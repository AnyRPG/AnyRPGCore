using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New ProjectileEffect", menuName = "AnyRPG/Abilities/Effects/ProjectileEffect")]
    public class ProjectileEffect : DirectEffect {

        /*
        [Header("Projectile")]

        [SerializeField]
        protected float projectileSpeed = 0;

        [Header("Flight Audio")]

        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        protected List<string> flightAudioProfileNames = new List<string>();

        [Tooltip("whether to play all audio profiles or just one random one")]
        [SerializeField]
        protected bool randomFlightAudioProfiles = false;

        //protected AudioProfile onHitAudioProfile;
        protected List<AudioProfile> flightAudioProfiles = new List<AudioProfile>();

        // game manager references
        protected PlayerManager playerManager = null;

        public float ProjectileSpeed { get => projectileSpeed; set => projectileSpeed = value; }
        public List<string> FlightAudioProfileNames { get => flightAudioProfileNames; set => flightAudioProfileNames = value; }
        public bool RandomFlightAudioProfiles { get => randomFlightAudioProfiles; set => randomFlightAudioProfiles = value; }
        */

        [SerializeField]
        private ProjectileEffectProperties projectileEffectProperties = new ProjectileEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => projectileEffectProperties; }


    }
}