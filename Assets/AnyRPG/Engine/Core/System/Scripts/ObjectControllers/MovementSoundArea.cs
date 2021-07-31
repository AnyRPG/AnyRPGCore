using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class MovementSoundArea : MonoBehaviour {

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

        public AudioProfile MovementLoopProfile { get => movementLoopProfile; set => movementLoopProfile = value; }
        public AudioProfile MovementHitProfile { get => movementHitProfile; set => movementHitProfile = value; }

        private void Awake() {
            //Debug.Log(gameObject.name + ".EnvironmentalEffectArea.Awake()");
            GetComponentReferences();
            SetupScriptableObjects();
        }

        public void GetComponentReferences() {
        }

        public void OnTriggerEnter(Collider other) {
            //Debug.Log(gameObject.name + ".MovementSoundArea.OnTriggerEnter()");
            UnitController unitController = other.gameObject.GetComponent<UnitController>();
            // stop playing sound in case movement sounds will change
            if (unitController != null) {
                unitController.UnitComponentController.StopMovement();
                unitController.SetMovementSoundArea(this);
            }
        }

        public void OnTriggerExit(Collider other) {
            UnitController unitController = other.gameObject.GetComponent<UnitController>();
            if (unitController != null) {
                unitController.UnsetMovementSoundArea(this);
            }
            // stop playing sound in case movement sounds will change
            unitController.UnitComponentController.StopMovement();
        }

        private void SetupScriptableObjects() {
            //Debug.Log(gameObject.name + ".EnvironmentalEffectArea.SetupScriptableObjects()");
            if (SystemAbilityEffectManager.Instance == null) {
                Debug.LogError(gameObject.name + ": SystemAbilityEffectManager not found.  Is the GameManager in the scene?");
                return;
            }

            if (movementLoopProfileName != null && movementLoopProfileName != string.Empty) {
                AudioProfile tmpMovementLoop = SystemAudioProfileManager.Instance.GetResource(movementLoopProfileName);
                if (tmpMovementLoop != null) {
                    movementLoopProfile = tmpMovementLoop;
                } else {
                    Debug.LogError("MovementSoundArea.SetupScriptableObjects(): Could not find audio profile : " + movementLoopProfileName + " while inititalizing " + gameObject.name + ".  CHECK INSPECTOR");
                }
            }

            if (movementHitProfileName != null && movementHitProfileName != string.Empty) {
                AudioProfile tmpMovementHit = SystemAudioProfileManager.Instance.GetResource(movementHitProfileName);
                if (tmpMovementHit != null) {
                    movementHitProfile = tmpMovementHit;
                } else {
                    Debug.LogError("MovementSoundArea.SetupScriptableObjects(): Could not find audio profile : " + movementHitProfileName + " while inititalizing " + gameObject.name + ".  CHECK INSPECTOR");
                }
            }
        }

    }

}
