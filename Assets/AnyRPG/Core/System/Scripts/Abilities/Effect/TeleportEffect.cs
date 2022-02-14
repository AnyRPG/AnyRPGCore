using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New TeleportEffect", menuName = "AnyRPG/Abilities/Effects/TeleportEffect")]
    public class TeleportEffect : InstantEffect {

        [SerializeField]
        private TeleportEffectProperties teleportEffectProperties = new TeleportEffectProperties();

        public AbilityEffectProperties EffectProperties { get => teleportEffectProperties; }

        public override void Convert() {
            //petEffectProperties.GetPetEffectProperties(this);
        }

        /*

        // game manager references
        protected LevelManager levelManager = null;

        public string LevelName { get => teleportEffectProperties.LevelName; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            levelManager = systemGameManager.LevelManager;
        }


        public override Dictionary<PrefabProfile, GameObject> Cast(IAbilityCaster source, Interactable target, Interactable originalTarget, AbilityEffectContext abilityEffectInput) {
            Dictionary<PrefabProfile, GameObject> returnObjects = base.Cast(source, target, originalTarget, abilityEffectInput);
            if (teleportEffectProperties.EffectlevelName != null) {
                if (teleportEffectProperties.OverrideSpawnDirection == true) {
                    levelManager.SetSpawnRotationOverride(teleportEffectProperties.SpawnForwardDirection);
                }
                if (teleportEffectProperties.OverrideSpawnLocation == true) {
                    levelManager.LoadLevel(teleportEffectProperties.LevelName, teleportEffectProperties.SpawnLocation);
                } else {
                    if (teleportEffectProperties.LocationTag != null && teleportEffectProperties.LocationTag != string.Empty) {
                        levelManager.OverrideSpawnLocationTag = teleportEffectProperties.LocationTag;
                    }
                    levelManager.LoadLevel(levelName);
                }
            }
            return returnObjects;
        }
        */

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            teleportEffectProperties.SetupScriptableObjects(systemGameManager);
        }

    }

}
