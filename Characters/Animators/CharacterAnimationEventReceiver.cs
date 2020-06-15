using AnyRPG;
using UnityEngine;

namespace AnyRPG {
    public class CharacterAnimationEventReceiver : MonoBehaviour {

        [SerializeField]
        private CharacterUnit characterUnit;

        private Animator animator;

        private int stepIndex = 0;

        private void Awake() {
            if (characterUnit == null) {
                characterUnit = GetComponent<CharacterUnit>();
            }
            if (characterUnit == null) {
                characterUnit = GetComponentInParent<CharacterUnit>();
            }
            if (characterUnit == null) {
                //Debug.Log(gameObject.name + ".CharacterAnimationEventReceiver.Awake(): could not find character unit!");
            }
            if (animator == null) {
                animator = GetComponent<Animator>();
            }
        }

        public void AttackHitEvent() {
            //Debug.Log(gameObject.name + ".CharacterAnimationEventReceiver.AttackHitEvent()");
            if (characterUnit != null && characterUnit.MyCharacter != null && characterUnit.MyCharacter.CharacterCombat != null) {
                characterUnit.MyCharacter.CharacterCombat.AttackHit_AnimationEvent();
            }
        }

        public void Hit() {
            //Debug.Log(gameObject.name + ".CharacterAnimationEventReceiver.Hit()");
            AttackHitEvent();
        }

        public void AnimationHit() {
            if (characterUnit != null && characterUnit.MyCharacter != null && characterUnit.MyCharacter.CharacterAbilityManager != null) {
                characterUnit.MyCharacter.CharacterAbilityManager.AnimationHitAnimationEvent();
            }
        }

        public void AnimationPrefabCreate() {
            //Debug.Log(gameObject.name + ".CharacterAnimationEventReceiver.AnimationPrefabCreate()");
            if (characterUnit != null && characterUnit.MyCharacter != null && characterUnit.MyCharacter.CharacterAbilityManager != null) {
                characterUnit.MyCharacter.CharacterAbilityManager.SpawnAbilityObjects();
            }
        }

        public void AnimationPrefabCreateByIndex(int animationIndex) {
            //Debug.Log(gameObject.name + ".CharacterAnimationEventReceiver.AnimationPrefabCreateByIndex(" + animationIndex + ")");
            if (characterUnit != null && characterUnit.MyCharacter != null && characterUnit.MyCharacter.CharacterAbilityManager != null) {
                characterUnit.MyCharacter.CharacterAbilityManager.SpawnAbilityObjects(animationIndex);
            }
        }

        public void AnimationPrefabDestroy() {
            //Debug.Log(gameObject.name + ".CharacterAnimationEventReceiver.AnimationPrefabDestroy()");
            if (characterUnit != null && characterUnit.MyCharacter != null && characterUnit.MyCharacter.CharacterEquipmentManager != null) {
                characterUnit.MyCharacter.CharacterEquipmentManager.DespawnAbilityObjects();
            }
        }

        // for root motion
        public void OnAnimatorMove() {
            if (animator != null && characterUnit != null && characterUnit.MyCharacter != null && characterUnit.MyCharacter.AnimatedUnit != null && characterUnit.MyCharacter.AnimatedUnit.MyCharacterMotor != null) {
                characterUnit.MyCharacter.AnimatedUnit.MyCharacterMotor.ReceiveAnimatorMovment(animator.deltaPosition);
            }
        }

        public void PlayStep() {
            PlayFootStep();
        }

        public void Shoot() {
        }

        public void FootR() {
            PlayFootStep();
        }

        public void FootL() {
            PlayFootStep();
        }

        public void Land() {
        }

        private void PlayFootStep() {
            //Debug.Log(gameObject.name + ".HandleMovementAudio(): " + apparentVelocity);
            if (characterUnit.BaseCharacter.UnitProfile == null ||
                characterUnit.MovementHitProfile == null ||
                characterUnit.MovementHitProfile.AudioClips == null || 
                characterUnit.MovementHitProfile.AudioClips.Count == 0 ||
                characterUnit.BaseCharacter.UnitProfile.PlayOnFootstep == false) {
                //Debug.Log(gameObject.name + ".HandleMovementAudio(): nothing to do, returning");
                return;
            }

            //Debug.Log(gameObject.name + ".HandleMovementAudio(): up to run speed");
            if (stepIndex >= characterUnit.MovementHitProfile.AudioClips.Count) {
                stepIndex = 0;
            }
            characterUnit.UnitAudio.PlayMovement(characterUnit.MovementHitProfile.AudioClips[stepIndex], false);
            stepIndex++;
            if (stepIndex >= characterUnit.MovementHitProfile.AudioClips.Count) {
                stepIndex = 0;
            }
        }

    }

}