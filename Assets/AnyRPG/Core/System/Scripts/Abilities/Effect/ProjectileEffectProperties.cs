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

        public float ProjectileSpeed { get => projectileSpeed; }
        public bool RandomFlightAudioProfiles { get => randomFlightAudioProfiles; }
        public List<AudioProfile> FlightAudioProfiles { get => flightAudioProfiles; }

        /*
        public void GetProjectileEffectProperties(ProjectileEffect effect) {

            projectileSpeed = effect.ProjectileSpeed;
            flightAudioProfileNames = effect.FlightAudioProfileNames;
            randomFlightAudioProfiles = effect.RandomFlightAudioProfiles;

            GetDirectEffectProperties(effect);
        }
        */

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
        }

        public override Dictionary<PrefabProfile, List<GameObject>> Cast(IAbilityCaster source, Interactable target, Interactable originalTarget, AbilityEffectContext abilityEffectContext) {
            //Debug.Log($"{ResourceName}.ProjectileEffect.Cast({source.AbilityManager.Name}, {(target == null ? "null" : target.name)})");

            Dictionary<PrefabProfile, List<GameObject>> returnObjects = source.AbilityManager.SpawnProjectileEffectPrefabs(target, originalTarget, this, abilityEffectContext);
            return returnObjects;
        }

        protected override void CheckDestroyObjects(Dictionary<PrefabProfile, List<GameObject>> abilityEffectObjects, IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectInput) {
            // intentionally not calling base to avoid pool recycled projectiles getting despawned mid-flight
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager, IDescribable describable) {
            base.SetupScriptableObjects(systemGameManager, describable);

            if (flightAudioProfileNames != null) {
                foreach (string audioProfileName in flightAudioProfileNames) {
                    AudioProfile audioProfile = systemDataFactory.GetResource<AudioProfile>(audioProfileName);
                    if (audioProfile != null) {
                        flightAudioProfiles.Add(audioProfile);
                    } else {
                        Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find audio profile: " + audioProfileName + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
                    }
                }
            }
        }

    }
}