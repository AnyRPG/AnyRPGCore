using AnyRPG;
using UnityEngine;

namespace AnyRPG {
    public class UnitAnimationEventReceiver : MonoBehaviour {

        private UnitController unitController = null;

        public void Setup(UnitController unitController) {
            //Debug.Log($"{gameObject.name}.UnitAnimationEventReceiver.Setup(" + (unitController == null ? "null" : unitController.gameObject.name) + ")");
            this.unitController = unitController;
        }

        public void AttackHitEvent() {
            //Debug.Log($"{gameObject.name}.CharacterAnimationEventReceiver.AttackHitEvent()");
            unitController.CharacterCombat.AttackHitAnimationEvent();
        }

        public void Hit() {
            //Debug.Log($"{gameObject.name}.CharacterAnimationEventReceiver.Hit()");
            AttackHitEvent();
        }

        public void AnimationHit() {
            unitController.CharacterAbilityManager.AnimationHitAnimationEvent();
        }

        public void StartAudio() {
            unitController.CharacterAbilityManager.StartAudioAnimationEvent();
        }

        public void StopAudio() {
            unitController.CharacterAbilityManager.StopAudioAnimationEvent();
        }

        public void AnimationPrefabCreate() {
            //Debug.Log($"{gameObject.name}.CharacterAnimationEventReceiver.AnimationPrefabCreate()");
            unitController.CharacterAbilityManager.SpawnAbilityObjects();
        }

        public void AnimationPrefabCreateByIndex(int animationIndex) {
            //Debug.Log($"{gameObject.name}.CharacterAnimationEventReceiver.AnimationPrefabCreateByIndex(" + animationIndex + ")");
            unitController.CharacterAbilityManager.SpawnAbilityObjects(animationIndex);
        }

        public void AnimationPrefabDestroy() {
            //Debug.Log($"{gameObject.name}.CharacterAnimationEventReceiver.AnimationPrefabDestroy()");
            unitController.CharacterAbilityManager.DespawnAbilityObjects();
        }

        // for root motion
        public void OnAnimatorMove() {
            if (unitController.UnitMotor != null) {
                unitController.UnitMotor.ReceiveAnimatorMovement();
            }
        }

        public void PlayStep() {
            PlayFootStep();
        }

        public void FootStep() {
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
            //Debug.Log($"{gameObject.name}.HandleMovementAudio(): " + apparentVelocity);
            if (unitController == null) {
                Debug.Log(gameObject.name + ".UnitAnimationEventReceiver.PlayFootStep() unitController is null!!!");
            }

            unitController.PlayFootStep();
        }

        public void PlaySwimSound() {
            //Debug.Log($"{gameObject.name}.HandleMovementAudio(): " + apparentVelocity);
            if (unitController == null) {
                Debug.Log(gameObject.name + ".UnitAnimationEventReceiver.PlayFootStep() unitController is null!!!");
                return;
            }

            unitController.PlaySwimSound();

        }

    }

}