using AnyRPG;
using UnityEngine;

namespace AnyRPG {
    public class CharacterAnimationEventReceiver : MonoBehaviour {
        [SerializeField]
        private CharacterUnit characterUnit;

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
            if (characterUnit != null && characterUnit.MyCharacter != null && characterUnit.MyCharacter.MyCharacterAbilityManager != null) {
                characterUnit.MyCharacter.MyCharacterAbilityManager.SpawnAbilityObjects();
            }
        }

        public void AnimationPrefabDestroy() {
            if (characterUnit != null && characterUnit.MyCharacter != null && characterUnit.MyCharacter.MyCharacterEquipmentManager != null) {
                characterUnit.MyCharacter.MyCharacterEquipmentManager.DespawnAbilityObjects();
            }
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