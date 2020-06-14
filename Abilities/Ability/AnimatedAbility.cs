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
        private bool isAutoAttack = false;

        [Tooltip("This option is only valid if this is not an auto attack ability.  If true, it will use the current auto-attack animations so it looks good with any weapon.")]
        [SerializeField]
        private bool useAutoAttackAnimations = false;

        [Tooltip("If true, the current weapon default hit sound will be played when this ability hits an enemy.")]
        [SerializeField]
        private bool useWeaponHitSound = false;

        public override float GetAbilityCastingTime(IAbilityCaster abilityCaster) {
            return 0f;
        }

        public bool IsAutoAttack { get => isAutoAttack; set => isAutoAttack = value; }
        public bool UseWeaponHitSound { get => useWeaponHitSound; set => useWeaponHitSound = value; }

        /// <summary>
        /// weapon hit sound
        /// </summary>
        /// <param name="abilityCaster"></param>
        /// <returns></returns>
        public override AudioClip GetHitSound(IAbilityCaster abilityCaster) {
            if (useWeaponHitSound == true) {
                return abilityCaster.GetAnimatedAbilityHitSound();
            }
            return base.GetHitSound(abilityCaster);
        }

        public List<AnimationClip> GetAnimationClips(IAbilityCaster sourceCharacter) {
            List<AnimationClip> animationClips = new List<AnimationClip>();
            if (useAutoAttackAnimations == true) {
                animationClips = sourceCharacter.GetDefaultAttackAnimations();
            } else {
                animationClips = AnimationClips;
            }
            return animationClips;
        }

        public override bool Cast(IAbilityCaster sourceCharacter, GameObject target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(MyName + ".AnimatedAbility.Cast(" + sourceCharacter.MyName + ")");
            if (base.Cast(sourceCharacter, target, abilityEffectContext)) {
                List<AnimationClip> usedAnimationClips = GetAnimationClips(sourceCharacter);
                if (usedAnimationClips.Count > 0) {
                    //Debug.Log("AnimatedAbility.Cast(): animationClip is not null, setting animator");

                    CharacterUnit targetCharacterUnit = null;
                    if (target != null) {
                        targetCharacterUnit = target.GetComponent<CharacterUnit>();
                    }
                    BaseCharacter targetBaseCharacter = null;
                    if (targetCharacterUnit != null) {
                        targetBaseCharacter = targetCharacterUnit.BaseCharacter;
                    }

                    int attackIndex = UnityEngine.Random.Range(0, usedAnimationClips.Count);
                    if (usedAnimationClips[attackIndex] != null) {
                        // perform the actual animation
                        float animationLength = sourceCharacter.PerformAnimatedAbility(usedAnimationClips[attackIndex], this, targetBaseCharacter, abilityEffectContext);

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

        public bool HandleAbilityHit(IAbilityCaster source, GameObject target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(MyName + ".AnimatedAbility.HandleAbilityHit()");
            bool returnResult = PerformAbilityEffects(source, target, abilityEffectContext);
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

        public override bool CanUseOn(GameObject target, IAbilityCaster source, bool performCooldownChecks = true) {
            //Debug.Log("AnimatedAbility.CanUseOn(" + (target == null ? "null" : target.name) + ", " + source.MyCharacterName + ")");
            if (performCooldownChecks && !source.PerformAnimatedAbilityCheck(this)) {
                return false;
            }
            
            //Debug.Log("AnimatedAbility.CanUseOn(" + (target == null ? "null" : target.name) + ", " + source.MyCharacterName + ") returning base");
            return base.CanUseOn(target, source, performCooldownChecks);
        }

        public override void ProcessGCDAuto(IAbilityCaster sourceCharacter) {
            //Debug.Log(MyName + "AnimatedAbility.ProcessGCDAuto()");
            //intentionally do nothing
        }


    }

}