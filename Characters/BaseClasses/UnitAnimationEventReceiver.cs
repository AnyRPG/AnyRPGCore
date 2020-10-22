using AnyRPG;
using UnityEngine;

namespace AnyRPG {
    public class UnitAnimationEventReceiver : MonoBehaviour {

        [SerializeField]
        private UnitController unitController;

        private int stepIndex = 0;

        public void Setup(UnitController unitController) {
            this.unitController = unitController;
        }

        public void AttackHitEvent() {
            //Debug.Log(gameObject.name + ".CharacterAnimationEventReceiver.AttackHitEvent()");
            if (unitController != null && unitController.BaseCharacter != null && unitController.BaseCharacter.CharacterCombat != null) {
                unitController.BaseCharacter.CharacterCombat.AttackHit_AnimationEvent();
            }
        }

        public void Hit() {
            //Debug.Log(gameObject.name + ".CharacterAnimationEventReceiver.Hit()");
            AttackHitEvent();
        }

        public void AnimationHit() {
            if (unitController != null && unitController.BaseCharacter != null && unitController.BaseCharacter.CharacterAbilityManager != null) {
                unitController.BaseCharacter.CharacterAbilityManager.AnimationHitAnimationEvent();
            }
        }

        public void AnimationPrefabCreate() {
            //Debug.Log(gameObject.name + ".CharacterAnimationEventReceiver.AnimationPrefabCreate()");
            if (unitController != null && unitController.BaseCharacter != null && unitController.BaseCharacter.CharacterAbilityManager != null) {
                unitController.BaseCharacter.CharacterAbilityManager.SpawnAbilityObjects();
            }
        }

        public void AnimationPrefabCreateByIndex(int animationIndex) {
            //Debug.Log(gameObject.name + ".CharacterAnimationEventReceiver.AnimationPrefabCreateByIndex(" + animationIndex + ")");
            if (unitController != null && unitController.BaseCharacter != null && unitController.BaseCharacter.CharacterAbilityManager != null) {
                unitController.BaseCharacter.CharacterAbilityManager.SpawnAbilityObjects(animationIndex);
            }
        }

        public void AnimationPrefabDestroy() {
            //Debug.Log(gameObject.name + ".CharacterAnimationEventReceiver.AnimationPrefabDestroy()");
            if (unitController != null && unitController.BaseCharacter != null && unitController.BaseCharacter.CharacterEquipmentManager != null) {
                unitController.BaseCharacter.CharacterAbilityManager.DespawnAbilityObjects();
            }
        }

        // for root motion
        public void OnAnimatorMove() {
            if (unitController != null && unitController.UnitMotor != null) {
                unitController.UnitMotor.ReceiveAnimatorMovment();
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

        public void PlayFootStep() {
            //Debug.Log(gameObject.name + ".HandleMovementAudio(): " + apparentVelocity);
            if (unitController.MovementHitProfile == null ||
                unitController.MovementHitProfile.AudioClips == null || 
                unitController.MovementHitProfile.AudioClips.Count == 0 ||
                unitController.BaseCharacter.UnitProfile.PlayOnFootstep == false) {
                //Debug.Log(gameObject.name + ".HandleMovementAudio(): nothing to do, returning");
                return;
            }

            //Debug.Log(gameObject.name + ".HandleMovementAudio(): up to run speed");
            if (stepIndex >= unitController.MovementHitProfile.AudioClips.Count) {
                stepIndex = 0;
            }
            unitController.UnitComponentController.PlayMovement(unitController.MovementHitProfile.AudioClips[stepIndex], false);
            stepIndex++;
            if (stepIndex >= unitController.MovementHitProfile.AudioClips.Count) {
                stepIndex = 0;
            }
        }

    }

}