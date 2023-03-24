using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class MovementSoundArea : AutoConfiguredMonoBehaviour {

        [Header("Audio")]

        [Tooltip("This audio will override the movement sound loop for a character in this zone")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        private string movementLoopProfileName = string.Empty;

        private AudioProfile movementLoopProfile;

        [Tooltip("This audio will override the movement hit (footstep) sound for a character in this zone")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        private string movementHitProfileName = string.Empty;

        private AudioProfile movementHitProfile;

        // game manager references

        private SystemDataFactory systemDataFactory = null;

        public AudioProfile MovementLoopProfile { get => movementLoopProfile; set => movementLoopProfile = value; }
        public AudioProfile MovementHitProfile { get => movementHitProfile; set => movementHitProfile = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            SetupScriptableObjects();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            systemDataFactory = systemGameManager.SystemDataFactory;
        }

        public void OnTriggerEnter(Collider other) {
            //Debug.Log($"{gameObject.name}.MovementSoundArea.OnTriggerEnter()");
            UnitController unitController = other.gameObject.GetComponent<UnitController>();
            // stop playing sound in case movement sounds will change
            if (unitController != null) {
                unitController.StopMovementSound();
                unitController.SetMovementSoundArea(this);
            }
        }

        public void OnTriggerExit(Collider other) {
            UnitController unitController = other.gameObject.GetComponent<UnitController>();
            if (unitController != null) {
                // stop playing sound in case movement sounds will change
                unitController.StopMovementSound();

                unitController.UnsetMovementSoundArea(this);
            }
        }

        private void SetupScriptableObjects() {
            //Debug.Log($"{gameObject.name}.EnvironmentalEffectArea.SetupScriptableObjects()");
            if (systemGameManager == null) {
                Debug.LogError(gameObject.name + ": SystemGameManager not found.  Is the GameManager in the scene?");
                return;
            }

            if (movementLoopProfileName != null && movementLoopProfileName != string.Empty) {
                AudioProfile tmpMovementLoop = systemDataFactory.GetResource<AudioProfile>(movementLoopProfileName);
                if (tmpMovementLoop != null) {
                    movementLoopProfile = tmpMovementLoop;
                } else {
                    Debug.LogError("MovementSoundArea.SetupScriptableObjects(): Could not find audio profile : " + movementLoopProfileName + " while inititalizing " + gameObject.name + ".  CHECK INSPECTOR");
                }
            }

            if (movementHitProfileName != null && movementHitProfileName != string.Empty) {
                AudioProfile tmpMovementHit = systemDataFactory.GetResource<AudioProfile>(movementHitProfileName);
                if (tmpMovementHit != null) {
                    movementHitProfile = tmpMovementHit;
                } else {
                    Debug.LogError("MovementSoundArea.SetupScriptableObjects(): Could not find audio profile : " + movementHitProfileName + " while inititalizing " + gameObject.name + ".  CHECK INSPECTOR");
                }
            }
        }

    }

}
