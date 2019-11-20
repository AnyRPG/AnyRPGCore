using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "NewAnimatedAbility",menuName = "AnyRPG/Abilities/AnimatedAbility")]
    public class AnimatedAbility : BaseAbility {

        // is this an auto attack ability
        [SerializeField]
        private bool isAutoAttack;

        public override float MyAbilityCastingTime {
            get {
                return 0f;
            }
            set => abilityCastingTime = value;
        }

        public bool MyIsAutoAttack { get => isAutoAttack; set => isAutoAttack = value; }

        public override bool Cast(BaseCharacter source, GameObject target, Vector3 groundTarget) {
            //Debug.Log(MyName + ".AnimatedAbility.Cast()");
            if (base.Cast(source, target, groundTarget)) {
                if (animationClips.Count > 0) {
                    //Debug.Log("AnimatedAbility.Cast(): animationClip is not null, setting animator");

                    // this type of ability is allowed to interrupt other types of animations, so clear them all
                    source.MyAnimatedUnit.MyCharacterAnimator.ClearAnimationBlockers();

                    // now block further animations of other types from starting
                    source.MyCharacterAbilityManager.MyWaitingForAnimatedAbility = true;
                    CharacterUnit targetCharacterUnit = null;
                    if (target != null) {
                        targetCharacterUnit = target.GetComponent<CharacterUnit>();
                    }
                    BaseCharacter targetBaseCharacter = null;
                    if (targetCharacterUnit != null) {
                        targetBaseCharacter = targetCharacterUnit.MyBaseCharacter;
                    }

                    int attackIndex = UnityEngine.Random.Range(0, animationClips.Count);
                    if (animationClips[attackIndex] != null) {
                        // perform the actual animation
                        source.MyAnimatedUnit.MyCharacterAnimator.HandleAbility(animationClips[attackIndex], this, targetBaseCharacter);

                        // unblock 
                        source.MyCharacterUnit.MyCharacter.MyCharacterCombat.OnHitEvent += HandleAbilityHit;

                        ProcessGCDManual(source, animationClips[attackIndex].length);
                    }

                }
                return true;
            } else {
                //Debug.Log(MyName + ".AnimatedAbility.Cast(): COULD NOT CAST ABILITY");
            }
            return false;
        }

        public override void ProcessAbilityPrefabs(BaseCharacter sourceCharacter) {
            //Debug.Log(MyName + ".AnimatedAbility.ProcessAbilityPrefabs()");
            //base.ProcessAbilityPrefabs(sourceCharacter);
            // do nothing intentionally, we will clean these up at the end of the ability
        }

        public void CleanupEventSubscriptions(BaseCharacter source) {
            source.MyCharacterCombat.OnHitEvent -= HandleAbilityHit;
        }

        public void HandleAbilityHit(BaseCharacter source, GameObject target) {
            //Debug.Log(MyName + ".AnimatedAbility.HandleAbilityHit()");
            PerformAbilityEffects(source, target, Vector3.zero);

            // we can now continue because everything beyond this point is single target oriented and it's ok if we cancel attacking due to lack of alive/unfriendly target
            // check for friendly target in case it somehow turned friendly mid swing
            if (target == null || !CanUseOn(target, source)) {
                source.MyCharacterCombat.DeActivateAutoAttack();
                return;
            }

            if (isAutoAttack) {
                if (source.MyCharacterCombat.MyAutoAttackActive == false) {
                    //Debug.Log(gameObject.name + ".CharacterCombat.AttackHit_AnimationEvent(): activating auto-attack");
                    source.MyCharacterCombat.ActivateAutoAttack();
                }
            }

        }

        public override string GetSummary() {
            string returnString = base.GetSummary();
            return returnString;

        }

        public override bool CanUseOn(GameObject target, BaseCharacter source) {
            //Debug.Log("AnimatedAbility.CanUseOn(" + (target == null ? "null" : target.name) + ", " + source.MyCharacterName + ")");
            if (source.MyCharacterAbilityManager.MyWaitingForAnimatedAbility == true) {
                //Debug.Log("AnimatedAbility.CanUseOn(" + (target == null ? "null" : target.name) + ", " + source.MyCharacterName + ") FAILING.  ALREADY IN PROGRESS");
                if (PlayerManager.MyInstance.MyPlayerUnitSpawned == true && source == PlayerManager.MyInstance.MyCharacter && CombatLogUI.MyInstance != null) {
                    CombatLogUI.MyInstance.WriteCombatMessage("Cannot use " + (MyName == null ? "null" : MyName) + ". Waiting for another ability to finish.");
                }
                return false;
            }
            //Debug.Log("AnimatedAbility.CanUseOn(" + (target == null ? "null" : target.name) + ", " + source.MyCharacterName + ") returning base");
            return base.CanUseOn(target, source);
        }

        public override void ProcessGCDAuto(BaseCharacter sourceCharacter) {
            //intentionally do nothing
        }


    }

    public enum AnyRPGWeaponAffinity { Unarmed, Sword2H, Sword1H, Staff, Shield, Mace2H, Mace1H, Bow }
}