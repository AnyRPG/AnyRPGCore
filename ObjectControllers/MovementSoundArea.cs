using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class MovementSoundArea : MonoBehaviour {

        [Header("Audio")]

        [Tooltip("This audio will override the movement sound loop for a character in this zone")]
        [SerializeField]
        private string movementLoopProfileName = string.Empty;

        private AudioProfile movementLoopProfile;

        [Tooltip("This audio will override the movement hit (footstep) sound for a character in this zone")]
        [SerializeField]
        private string movementHitProfileName = string.Empty;

        private AudioProfile movementHitProfile;

        private BoxCollider boxCollider = null;

        public AudioProfile MovementLoopProfile { get => movementLoopProfile; set => movementLoopProfile = value; }
        public AudioProfile MovementHitProfile { get => movementHitProfile; set => movementHitProfile = value; }

        private void Awake() {
            //Debug.Log(gameObject.name + ".EnvironmentalEffectArea.Awake()");
            GetComponentReferences();
            SetupScriptableObjects();
        }

        public void GetComponentReferences() {
            boxCollider = GetComponent<BoxCollider>();
        }

        protected virtual List<AOETargetNode> GetValidTargets() {
            //Debug.Log(gameObject.name + ".EnvironmentalEffectArea.GetValidTargets()");

            Vector3 aoeSpawnCenter = transform.position;

            Collider[] colliders = new Collider[0];
            int playerMask = 1 << LayerMask.NameToLayer("Player");
            int characterMask = 1 << LayerMask.NameToLayer("CharacterUnit");
            int validMask = (playerMask | characterMask);

            //Debug.Log(MyName + ".AOEEffect.GetValidTargets(): using aoeSpawnCenter: " + aoeSpawnCenter + ", extents: " + aoeExtents);
            colliders = Physics.OverlapBox(aoeSpawnCenter, boxCollider.bounds.extents, Quaternion.identity, validMask);

            //Debug.Log("AOEEffect.Cast(): Casting OverlapSphere with radius: " + aoeRadius);
            List<AOETargetNode> validTargets = new List<AOETargetNode>();
            foreach (Collider collider in colliders) {
                //Debug.Log(gameObject.name + ".EnvironmentalEffectArea.GetValidTargets() hit: " + collider.gameObject.name + "; layer: " + collider.gameObject.layer);

                bool canAdd = true;
                if (collider.gameObject.GetComponent<CharacterUnit>() == null) {
                    canAdd = false;
                }
                /*
                foreach (AbilityEffect abilityEffect in abilityEffects) {
                    if (abilityEffect.CanUseOn(collider.gameObject, source) == false) {
                        canAdd = false;
                    }
                }
                */
                //Debug.Log(MyName + "performing AOE ability  on " + collider.gameObject);
                if (canAdd) {
                    AOETargetNode validTargetNode = new AOETargetNode();
                    validTargetNode.targetGameObject = collider.gameObject;
                    validTargets.Add(validTargetNode);
                }
            }
            //Debug.Log(gameObject.name + ".EnvironmentalEffectArea.GetValidTargets(). Valid targets count: " + validTargets.Count);
            return validTargets;
        }

        public void OnTriggerEnter(Collider other) {
            CharacterUnit characterUnit = other.gameObject.GetComponent<CharacterUnit>();
            if (characterUnit != null) {
                characterUnit.SetMovementSoundArea(this);
            }
        }

        public void OnTriggerExit(Collider other) {
            CharacterUnit characterUnit = other.gameObject.GetComponent<CharacterUnit>();
            if (characterUnit != null) {
                characterUnit.UnsetMovementSoundArea(this);
            }
        }

        private void SetupScriptableObjects() {
            //Debug.Log(gameObject.name + ".EnvironmentalEffectArea.SetupScriptableObjects()");
            if (SystemAbilityEffectManager.MyInstance == null) {
                Debug.LogError(gameObject.name + ": SystemAbilityEffectManager not found.  Is the GameManager in the scene?");
                return;
            }

            if (movementLoopProfileName != null && movementLoopProfileName != string.Empty) {
                AudioProfile tmpMovementLoop = SystemAudioProfileManager.MyInstance.GetResource(movementLoopProfileName);
                if (tmpMovementLoop != null) {
                    movementLoopProfile = tmpMovementLoop;
                } else {
                    Debug.LogError("MovementSoundArea.SetupScriptableObjects(): Could not find audio profile : " + movementLoopProfileName + " while inititalizing " + gameObject.name + ".  CHECK INSPECTOR");
                }
            }

            if (movementHitProfileName != null && movementHitProfileName != string.Empty) {
                AudioProfile tmpMovementHit = SystemAudioProfileManager.MyInstance.GetResource(movementHitProfileName);
                if (tmpMovementHit != null) {
                    movementHitProfile = tmpMovementHit;
                } else {
                    Debug.LogError("MovementSoundArea.SetupScriptableObjects(): Could not find audio profile : " + movementHitProfileName + " while inititalizing " + gameObject.name + ".  CHECK INSPECTOR");
                }
            }
        }

    }

}
