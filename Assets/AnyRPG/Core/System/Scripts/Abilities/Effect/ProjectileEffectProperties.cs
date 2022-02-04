using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class ProjectileEffectProperties : DirectEffectProperties {

        [Header("Projectile")]

        [SerializeField]
        private float projectileSpeed = 0;

        [Header("Flight Audio")]

        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        protected List<string> flightAudioProfileNames = new List<string>();

        [Tooltip("whether to play all audio profiles or just one random one")]
        [SerializeField]
        protected bool randomFlightAudioProfiles = false;

        //protected AudioProfile onHitAudioProfile;
        protected List<AudioProfile> flightAudioProfiles = new List<AudioProfile>();

        public float ProjectileSpeed { get => projectileSpeed; set => projectileSpeed = value; }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager, string displayName) {
            base.SetupScriptableObjects(systemGameManager, displayName);

            if (flightAudioProfileNames != null) {
                foreach (string audioProfileName in flightAudioProfileNames) {
                    AudioProfile audioProfile = systemDataFactory.GetResource<AudioProfile>(audioProfileName);
                    if (audioProfile != null) {
                        flightAudioProfiles.Add(audioProfile);
                    } else {
                        Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find audio profile: " + audioProfileName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }
        }

    }
}