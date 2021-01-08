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

        [Tooltip("If true, a random animation from the unit attack animations will be used")]
        [SerializeField]
        private bool useUnitAttackAnimations = true;

        [Tooltip("This option is only valid if this is not an auto attack ability.  If true, it will use the current auto-attack animations so it looks good with any weapon.")]
        [SerializeField]
        private bool useAutoAttackAnimations = false;

        [Tooltip("If true, the current weapon default hit sound will be played when this ability hits an enemy.")]
        [SerializeField]
        private bool useWeaponHitSound = false;


        public bool IsAutoAttack { get => isAutoAttack; set => isAutoAttack = value; }
        public bool UseWeaponHitSound { get => useWeaponHitSound; set => useWeaponHitSound = value; }

        public override List<AbilityEffect> GetAbilityEffects(IAbilityCaster abilityCaster) {
            if (isAutoAttack) {
                List<AbilityEffect> weaponAbilityList = abilityCaster.AbilityManager.GetDefaultHitEffects();
                if (weaponAbilityList != null && weaponAbilityList.Count > 0) {
                    return weaponAbilityList;
                }
            }
            return base.GetAbilityEffects(abilityCaster);
        }

        public override float GetAbilityCastingTime(IAbilityCaster abilityCaster) {
            return 0f;
        }

        public override List<AbilityAttachmentNode> GetHoldableObjectList(IAbilityCaster abilityCaster) {
            if (abilityPrefabSource == AbilityPrefabSource.Both) {
                List<AbilityAttachmentNode> returnList = new List<AbilityAttachmentNode>();
                returnList.AddRange(base.GetHoldableObjectList(abilityCaster));
                returnList.AddRange(abilityCaster.AbilityManager.GetWeaponAbilityObjectList());
                return returnList;
            }
            if (abilityPrefabSource == AbilityPrefabSource.Weapon) {
                return abilityCaster.AbilityManager.GetWeaponAbilityObjectList();
            }

            // abilityPrefabSource is AbilityPrefabSource.Ability since there are only 3 options
            return base.GetHoldableObjectList(abilityCaster);
        }


        /// <summary>
        /// weapon hit sound
        /// </summary>
        /// <param name="abilityCaster"></param>
        /// <returns></returns>
        public override AudioClip GetHitSound(IAbilityCaster abilityCaster) {
            //Debug.Log(MyName + ".AnimatedAbility.GetHitSound(" + abilityCaster.Name + ")");
            if (useWeaponHitSound == true) {
                //Debug.Log(MyName + ".AnimatedAbility.GetHitSound(" + abilityCaster.Name + "): using weapon hit sound");
                return abilityCaster.AbilityManager.GetAnimatedAbilityHitSound();
            }
            //Debug.Log(MyName + ".AnimatedAbility.GetHitSound(" + abilityCaster.Name + "): not using weapon hit sound");
            return base.GetHitSound(abilityCaster);
        }

        public List<AnimationClip> GetAnimationClips(IAbilityCaster sourceCharacter) {
            List<AnimationClip> animationClips = new List<AnimationClip>();
            if (useUnitAttackAnimations == true) {
                animationClips = sourceCharacter.AbilityManager.GetUnitAttackAnimations();
            } else if (useAutoAttackAnimations == true) {
                animationClips = sourceCharacter.AbilityManager.GetDefaultAttackAnimations();
            } else {
                animationClips = AttackClips;
            }
            return animationClips;
        }

        public override bool Cast(IAbilityCaster sourceCharacter, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(DisplayName + ".AnimatedAbility.Cast(" + sourceCharacter.AbilityManager.Name + ")");
            if (base.Cast(sourceCharacter, target, abilityEffectContext)) {
                List<AnimationClip> usedAnimationClips = GetAnimationClips(sourceCharacter);
                if (usedAnimationClips != null && usedAnimationClips.Count > 0) {
                    //Debug.Log("AnimatedAbility.Cast(): animationClip is not null, setting animator");

                    CharacterUnit targetCharacterUnit = null;
                    if (target != null) {
                        targetCharacterUnit =  CharacterUnit.GetCharacterUnit(target);
                    }
                    BaseCharacter targetBaseCharacter = null;
                    if (targetCharacterUnit != null) {
                        targetBaseCharacter = targetCharacterUnit.BaseCharacter;
                    }

                    int attackIndex = UnityEngine.Random.Range(0, usedAnimationClips.Count);
                    if (usedAnimationClips[attackIndex] != null) {
                        // perform the actual animation
                        float animationLength = sourceCharacter.AbilityManager.PerformAnimatedAbility(usedAnimationClips[attackIndex], this, targetBaseCharacter, abilityEffectContext);

                        sourceCharacter.AbilityManager.ProcessAbilityCoolDowns(this, animationLength, abilityCoolDown);
                    }

                } else {
                    Debug.LogError(DisplayName + "AnimatedAbility.Cast(): no animation clips returned");
                }
                return true;
            } else {
                //Debug.Log(DisplayName + ".AnimatedAbility.Cast(): COULD NOT CAST ABILITY: sourceCharacter: " + sourceCharacter);
            }
            //Debug.Log(DisplayName + ".AnimatedAbility.Cast(): COULD NOT CAST ABILITY (RETURN FALSE): sourceCharacter: " + sourceCharacter);
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

        public bool HandleAbilityHit(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(MyName + ".AnimatedAbility.HandleAbilityHit()");
            bool returnResult = true;

            // perform a check that includes range to target
            bool rangeResult = base.CanUseOn(target, source);
            bool deactivateAutoAttack = false;
            if (rangeResult == false) {
                returnResult = false;
                // if the range to target check failed, perform another check without range
                // if that passes, do not deactivate auto-attack.  target just moved away mid swing and didn't die/change faction etc
                if (!base.CanUseOn(target, source, true, null, false, false)) {
                    deactivateAutoAttack = true;
                }
            }
            source.AbilityManager.ProcessAnimatedAbilityHit(target, deactivateAutoAttack);

            // if the range check passed, see if the ability hit or missed
            if (returnResult == true) {
                bool missResult = PerformAbilityEffects(source, target, abilityEffectContext);
                if (!missResult) {
                    returnResult = false;
                }
            }

            return returnResult;

        }

        public override string GetSummary() {
            string returnString = base.GetSummary();
            return returnString;

        }

        public override bool CanUseOn(Interactable target, IAbilityCaster source, bool performCooldownChecks = true, AbilityEffectContext abilityEffectContext = null, bool playerInitiated = false, bool performRangeCheck = true) {
            //Debug.Log(DisplayName + ".AnimatedAbility.CanUseOn(" + (target == null ? "null" : target.gameObject.name) + ", " + (source == null ? "null" : source.gameObject.name) + ")");
            if (performCooldownChecks && !source.AbilityManager.PerformAnimatedAbilityCheck(this)) {
                return false;
            }

            //Debug.Log(DisplayName + ".AnimatedAbility.CanUseOn(" + (target == null ? "null" : target.gameObject.name) + ", " + (source == null ? "null" : source.gameObject.name) + "): returning base");
            return base.CanUseOn(target, source, performCooldownChecks, abilityEffectContext, playerInitiated, performRangeCheck);
        }

        public override void ProcessGCDAuto(IAbilityCaster sourceCharacter) {
            //Debug.Log(MyName + "AnimatedAbility.ProcessGCDAuto()");
            //intentionally do nothing
        }


    }

}