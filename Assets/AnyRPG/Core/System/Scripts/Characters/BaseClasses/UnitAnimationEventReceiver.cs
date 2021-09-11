using AnyRPG;
using UnityEngine;

namespace AnyRPG {
    public class UnitAnimationEventReceiver : MonoBehaviour {

        private UnitController unitController = null;

        private int stepIndex = 0;

        public void Setup(UnitController unitController) {
            //Debug.Log(gameObject.name + ".UnitAnimationEventReceiver.Setup(" + (unitController == null ? "null" : unitController.gameObject.name) + ")");
            this.unitController = unitController;
        }

        public void AttackHitEvent() {
            //Debug.Log(gameObject.name + ".CharacterAnimationEventReceiver.AttackHitEvent()");
            if (unitController?.CharacterUnit?.BaseCharacter?.CharacterCombat != null) {
                unitController.CharacterUnit.BaseCharacter.CharacterCombat.AttackHit_AnimationEvent();
            }
        }

        public void Hit() {
            //Debug.Log(gameObject.name + ".CharacterAnimationEventReceiver.Hit()");
            AttackHitEvent();
        }

        public void AnimationHit() {
            if (unitController?.CharacterUnit?.BaseCharacter?.CharacterAbilityManager != null) {
                unitController.CharacterUnit.BaseCharacter.CharacterAbilityManager.AnimationHitAnimationEvent();
            }
        }

        public void AnimationPrefabCreate() {
            //Debug.Log(gameObject.name + ".CharacterAnimationEventReceiver.AnimationPrefabCreate()");
            if (unitController?.CharacterUnit?.BaseCharacter?.CharacterAbilityManager != null) {
                unitController.CharacterUnit.BaseCharacter.CharacterAbilityManager.SpawnAbilityObjects();
            }
        }

        public void AnimationPrefabCreateByIndex(int animationIndex) {
            //Debug.Log(gameObject.name + ".CharacterAnimationEventReceiver.AnimationPrefabCreateByIndex(" + animationIndex + ")");
            if (unitController != null && unitController.CharacterUnit.BaseCharacter != null && unitController.CharacterUnit.BaseCharacter.CharacterAbilityManager != null) {
                unitController.CharacterUnit.BaseCharacter.CharacterAbilityManager.SpawnAbilityObjects(animationIndex);
            }
        }

        public void AnimationPrefabDestroy() {
            //Debug.Log(gameObject.name + ".CharacterAnimationEventReceiver.AnimationPrefabDestroy()");
            if (unitController != null && unitController.CharacterUnit.BaseCharacter != null && unitController.CharacterUnit.BaseCharacter.CharacterEquipmentManager != null) {
                unitController.CharacterUnit.BaseCharacter.CharacterAbilityManager.DespawnAbilityObjects();
            }
        }

        // for root motion
        public void OnAnimatorMove() {
            if (unitController != null && unitController.UnitMotor != null) {
                unitController.UnitMotor.ReceiveAnimatorMovement();
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
            if (unitController == null) {
                Debug.Log(gameObject.name + ".UnitAnimationEventReceiver.PlayFootStep() unitController is null!!!");
            }

            if ((unitController?.MovementHitProfile  == null ||
                unitController.MovementHitProfile?.AudioClips == null || 
                unitController?.MovementHitProfile.AudioClips.Count == 0 ||
                unitController?.UnitProfile.PlayOnFootstep == false)
                //&& unitController?.MovementSoundArea?.MovementLoopProfile == null
                && unitController?.MovementSoundArea?.MovementHitProfile == null) {
                //Debug.Log(gameObject.name + ".HandleMovementAudio(): nothing to do, returning");
                return;
            }

            //Debug.Log(gameObject.name + ".HandleMovementAudio(): up to run speed");
            if (stepIndex >= unitController.MovementHitProfile.AudioClips.Count) {
                stepIndex = 0;
            }
            unitController.PlayMovementSound(unitController.MovementHitProfile.AudioClips[stepIndex], false);
            stepIndex++;
            if (stepIndex >= unitController.MovementHitProfile.AudioClips.Count) {
                stepIndex = 0;
            }
        }

    }

}