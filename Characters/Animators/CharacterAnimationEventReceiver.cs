using AnyRPG;
using UnityEngine;

namespace AnyRPG {
    public class CharacterAnimationEventReceiver : MonoBehaviour {
        [SerializeField]
        private CharacterUnit characterUnit;

        private Animator animator;

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
            if (characterUnit != null && characterUnit.MyCharacter != null && characterUnit.MyCharacter.MyCharacterCombat != null) {
                characterUnit.MyCharacter.MyCharacterCombat.AttackHit_AnimationEvent();
            }
        }

        public void Hit() {
            //Debug.Log(gameObject.name + ".CharacterAnimationEventReceiver.Hit()");
            AttackHitEvent();
        }

        public void AnimationHit() {
            if (characterUnit != null && characterUnit.MyCharacter != null && characterUnit.MyCharacter.MyCharacterAbilityManager != null) {
                characterUnit.MyCharacter.MyCharacterAbilityManager.AnimationHitAnimationEvent();
            }
        }

        public void AnimationPrefabCreate() {
            //Debug.Log(gameObject.name + ".CharacterAnimationEventReceiver.AnimationPrefabCreate()");
            if (characterUnit != null && characterUnit.MyCharacter != null && characterUnit.MyCharacter.MyCharacterAbilityManager != null) {
                characterUnit.MyCharacter.MyCharacterAbilityManager.SpawnAbilityObjects();
            }
        }

        public void AnimationPrefabCreateByIndex(int animationIndex) {
            //Debug.Log(gameObject.name + ".CharacterAnimationEventReceiver.AnimationPrefabCreateByIndex(" + animationIndex + ")");
            if (characterUnit != null && characterUnit.MyCharacter != null && characterUnit.MyCharacter.MyCharacterAbilityManager != null) {
                characterUnit.MyCharacter.MyCharacterAbilityManager.SpawnAbilityObjects(animationIndex);
            }
        }

        public void AnimationPrefabDestroy() {
            //Debug.Log(gameObject.name + ".CharacterAnimationEventReceiver.AnimationPrefabDestroy()");
            if (characterUnit != null && characterUnit.MyCharacter != null && characterUnit.MyCharacter.MyCharacterEquipmentManager != null) {
                characterUnit.MyCharacter.MyCharacterEquipmentManager.DespawnAbilityObjects();
            }
        }

        public void OnAnimatorMove() {
            if (animator != null && characterUnit != null && characterUnit.MyCharacter != null && characterUnit.MyCharacter.MyAnimatedUnit != null && characterUnit.MyCharacter.MyAnimatedUnit.MyCharacterMotor != null) {
                characterUnit.MyCharacter.MyAnimatedUnit.MyCharacterMotor.ReceiveAnimatorMovment(animator.deltaPosition);
            }
        }

        public void PlayStep() {

        }

        public void Shoot() {
        }

        public void FootR() {
        }

        public void FootL() {
        }

        public void Land() {
        }

    }

}