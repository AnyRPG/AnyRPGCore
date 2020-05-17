using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "NewAnimatedAbility",menuName = "AnyRPG/Abilities/AnimatedAbility")]
    public class AnimatedAbility : BaseAbility {

        [Header("Animated Ability")]

        [Tooltip("Is this an auto attack ability")]
        [SerializeField]
        private bool isAutoAttack;

        [Tooltip("This option is only valid if this is not an auto attack ability.  If true, it will use the current auto-attack animations so it looks good with any weapon.")]
        [SerializeField]
        private bool useAutoAttackAnimations;

        public override float MyAbilityCastingTime {
            get {
                return 0f;
            }
            set => abilityCastingTime = value;
        }

        public bool IsAutoAttack { get => isAutoAttack; set => isAutoAttack = value; }

        public override bool Cast(IAbilityCaster sourceCharacter, GameObject target, Vector3 groundTarget) {
            //Debug.Log(MyName + ".AnimatedAbility.Cast(" + sourceCharacter.MyName + ")");
            if (base.Cast(sourceCharacter, target, groundTarget)) {
                if (MyAnimationClips.Count > 0) {
                    //Debug.Log("AnimatedAbility.Cast(): animationClip is not null, setting animator");

                    

                    CharacterUnit targetCharacterUnit = null;
                    if (target != null) {
                        targetCharacterUnit = target.GetComponent<CharacterUnit>();
                    }
                    BaseCharacter targetBaseCharacter = null;
                    if (targetCharacterUnit != null) {
                        targetBaseCharacter = targetCharacterUnit.MyBaseCharacter;
                    }

                    int attackIndex = UnityEngine.Random.Range(0, MyAnimationClips.Count);
                    if (MyAnimationClips[attackIndex] != null) {
                        // perform the actual animation
                        float animationLength = sourceCharacter.PerformAnimatedAbility(MyAnimationClips[attackIndex], this, targetBaseCharacter);

                        //sourceCharacter.MyCharacterUnit.MyCharacter.MyCharacterCombat.OnHitEvent += HandleAbilityHit;
                        if (SystemConfigurationManager.MyInstance.MyAllowAutoAttack == false || !isAutoAttack) {
                            //Debug.Log(MyName + ".Cast(): Setting GCD for length: " + animationLength);
                            ProcessGCDManual(sourceCharacter, Mathf.Min(animationLength, abilityCoolDown));
                            base.BeginAbilityCoolDown(sourceCharacter, Mathf.Max(animationLength, abilityCoolDown));
                        }
                    }

                }
                return true;
            } else {
                //Debug.Log(MyName + ".AnimatedAbility.Cast(): COULD NOT CAST ABILITY: sourceCharacter: " + sourceCharacter);
            }
            return false;
        }

        public override void BeginAbilityCoolDown(IAbilityCaster sourceCharacter, float animationLength = -1) {
            // intentionally do nothing, we will call this method manually here and pass in a time
            //base.BeginAbilityCoolDown(sourceCharacter);
        }

        public override void ProcessAbilityPrefabs(IAbilityCaster sourceCharacter) {
            //Debug.Log(MyName + ".AnimatedAbility.ProcessAbilityPrefabs()");
            //base.ProcessAbilityPrefabs(sourceCharacter);
            // do nothing intentionally, we will clean these up at the end of the ability
        }

        public void CleanupEventSubscriptions(BaseCharacter source) {
            //source.MyCharacterCombat.OnHitEvent -= HandleAbilityHit;
        }

        public bool HandleAbilityHit(IAbilityCaster source, GameObject target) {
            //Debug.Log(MyName + ".AnimatedAbility.HandleAbilityHit()");
            bool returnResult = PerformAbilityEffects(source, target, Vector3.zero);
            if (!returnResult) {
                return false;
            }

            if (!source.ProcessAnimatedAbilityHit(target, !base.CanUseOn(target, source))) {
                return false;
            }

            return true;

        }

        public override string GetSummary() {
            string returnString = base.GetSummary();
            return returnString;

        }

        public override bool CanUseOn(GameObject target, IAbilityCaster source) {
            //Debug.Log("AnimatedAbility.CanUseOn(" + (target == null ? "null" : target.name) + ", " + source.MyCharacterName + ")");
            if (!source.PerformAnimatedAbilityCheck(this)) {
                return false;
            }
            
            //Debug.Log("AnimatedAbility.CanUseOn(" + (target == null ? "null" : target.name) + ", " + source.MyCharacterName + ") returning base");
            return base.CanUseOn(target, source);
        }

        public override void ProcessGCDAuto(IAbilityCaster sourceCharacter) {
            //Debug.Log(MyName + "AnimatedAbility.ProcessGCDAuto()");
            //intentionally do nothing
        }


    }

}