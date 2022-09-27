using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class TeleportEffectProperties : InstantEffectProperties {

        [Header("Teleport")]

        [Tooltip("The name of the scene to load")]
        [SerializeField]
        private string levelName = string.Empty;

        [Tooltip("If this is set, the player will spawn at the location of the object in the scene with this tag, instead of the default spawn location for the scene.")]
        [SerializeField]
        protected string locationTag = string.Empty;

        [Tooltip("If true, the player will spawn at the Vector location set in the Spawn Location field below.")]
        [SerializeField]
        private bool overrideSpawnLocation = false;

        [Tooltip("The world space position to spawn at. Only used if Override Spawn Location box is checked")]
        [SerializeField]
        private Vector3 spawnLocation = Vector3.zero;

        [Tooltip("If true, the player will spawn facing the world space direction specified in the Spawn Forward Direction field")]
        [SerializeField]
        private bool overrideSpawnDirection = false;

        [Tooltip("The world space forward direction to face when spawning.  Only used if Override Spawn Direction box is checked")]
        [SerializeField]
        private Vector3 spawnForwardDirection = Vector3.zero;

        /*

    public void GetTeleportEffectProperties(TeleportEffect effect) {
        levelName = effect.LevelName;
        locationTag = effect.LocationTag;
        overrideSpawnLocation = effect.OverrideSpawnLocation;
        spawnLocation = effect.SpawnLocation;
        overrideSpawnDirection = effect.SpawnDirection;
        spawnForwardDirection = effect.SpawnForwardDirection;
        GetInstantEffectProperties(effect);
        }
        */

        // game manager references
        protected LevelManager levelManager = null;

        public string LevelName { get => levelName; set => levelName = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            levelManager = systemGameManager.LevelManager;
        }

        public override Dictionary<PrefabProfile, GameObject> Cast(IAbilityCaster source, Interactable target, Interactable originalTarget, AbilityEffectContext abilityEffectInput) {
            Dictionary<PrefabProfile, GameObject> returnObjects = base.Cast(source, target, originalTarget, abilityEffectInput);
            if (levelName != null) {
                if (overrideSpawnDirection == true) {
                    levelManager.SetSpawnRotationOverride(spawnForwardDirection);
                }
                if (overrideSpawnLocation == true) {
                    levelManager.LoadLevel(levelName, spawnLocation);
                } else {
                    if (locationTag != null && locationTag != string.Empty) {
                        levelManager.OverrideSpawnLocationTag = locationTag;
                    }
                    levelManager.LoadLevel(levelName);
                }
            }
            return returnObjects;
        }

    }

}
