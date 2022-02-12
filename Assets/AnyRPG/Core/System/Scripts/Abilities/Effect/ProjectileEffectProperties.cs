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

        // game manager references
        protected PlayerManager playerManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
        }

        public override Dictionary<PrefabProfile, GameObject> Cast(IAbilityCaster source, Interactable target, Interactable originalTarget, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(DisplayName + ".ProjectileEffect.Cast(" + source.AbilityManager.Name + ", " + (target == null ? "null" : target.name) + ")");
            Dictionary<PrefabProfile, GameObject> returnObjects = base.Cast(source, target, originalTarget, abilityEffectContext);
            if (returnObjects != null) {
                foreach (GameObject go in returnObjects.Values) {
                    //Debug.Log(DisplayName + ".ProjectileEffect.Cast(): found gameobject: " + go.name);
                    go.transform.parent = playerManager.EffectPrefabParent.transform;
                    ProjectileScript projectileScript = go.GetComponentInChildren<ProjectileScript>();
                    if (projectileScript != null) {
                        //Debug.Log(DisplayName + ".ProjectileEffect.Cast(): found gameobject: " + go.name + " and it has projectile script");
                        abilityEffectContext = ApplyInputMultiplier(abilityEffectContext);
                        projectileScript.Initialize(projectileSpeed, source, target, new Vector3(0, 1, 0), go, abilityEffectContext);
                        if (flightAudioProfiles != null && flightAudioProfiles.Count > 0) {
                            projectileScript.PlayFlightAudio(flightAudioProfiles, randomFlightAudioProfiles);
                        }
                        projectileScript.OnCollission += HandleCollission;
                    }
                }
            }
            return returnObjects;
        }

        public void HandleCollission(IAbilityCaster source, Interactable target, GameObject _abilityEffectObject, AbilityEffectContext abilityEffectInput, ProjectileScript projectileScript) {
            //Debug.Log(DisplayName + ".ProjectileEffect.HandleCollission()");
            PerformAbilityHit(source, target, abilityEffectInput);
            projectileScript.OnCollission -= HandleCollission;
            objectPooler.ReturnObjectToPool(_abilityEffectObject);
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);
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