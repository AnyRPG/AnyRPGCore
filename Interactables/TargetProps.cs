using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class TargetProps {

        // in the future, this will likely be directly serialized on ability and effect to avoid duplication effort of nearly identical settings
        /*

        [Header("Ground Target")]

        [Tooltip("If true, casting this spell will require choosing a target on the ground, instead of a target character.")]
        [SerializeField]
        private bool requiresGroundTarget = false;

        [Tooltip("If this is a ground targeted spell, tint it with this color.")]
        [SerializeField]
        protected Color groundTargetColor = new Color32(255, 255, 255, 255);

        [Tooltip("How big should the projector be on the ground if this is ground targeted. Used to show accurate effect size.")]
        [SerializeField]
        protected float groundTargetRadius = 0f;

        [Header("Standard Target")]

        [Tooltip("Ignore requireTarget and canCast variables and use the check from the first ability effect instead")]
        [SerializeField]
        private bool useAbilityEffectTargetting = false;

        [Tooltip("If true, the character must have a target selected to cast this ability.")]
        [SerializeField]
        private bool requiresTarget = false;

        [Tooltip("If true, the character must have an uninterrupted line of sight to the target.")]
        [SerializeField]
        private bool requireLineOfSight = false;

        [Tooltip("If true, the target must be a character and must be alive.")]
        [SerializeField]
        private bool requiresLiveTarget = true;

        [Tooltip("If true, the target must be a character and must be dead.")]
        [SerializeField]
        private bool requireDeadTarget = false;

        [Tooltip("Can the character cast this ability on itself?")]
        [SerializeField]
        protected bool canCastOnSelf = false;

        [Tooltip("Can the character cast this ability on others?")]
        [SerializeField]
        protected bool canCastOnOthers = false;

        [Tooltip("Can the character cast this ability on a character belonging to an enemy faction?")]
        [SerializeField]
        protected bool canCastOnEnemy = false;

        [Tooltip("Can the character cast this ability on a character with no relationship?")]
        [SerializeField]
        protected bool canCastOnNeutral = false;

        [Tooltip("Can the character cast this ability on a character belonging to a friendly faction?")]
        [SerializeField]
        protected bool canCastOnFriendly = false;

        [Tooltip("If no target is given, automatically cast on the caster")]
        [SerializeField]
        private bool autoSelfCast = false;

        [Header("Range")]

        [Tooltip("If true, the target must be within melee range (within hitbox) to cast this ability.")]
        [SerializeField]
        protected bool useMeleeRange = false;

        [Tooltip("If melee range is not used, this ability can be cast on targets this many meters away.")]
        [SerializeField]
        protected int maxRange;
        */

        public static bool CanUseOn(ITargetable targetable, Interactable target, IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectContext = null, bool playerInitiated = false) {
            // create target booleans
            bool targetIsSelf = false;
            CharacterUnit targetCharacterUnit = null;

            // special case for ground targeted spells cast by AI since AI currently has to cast a ground targeted spell on its current target
            if (targetable.RequiresGroundTarget == true
                && targetable.MaxRange > 0
                && target != null
                && ((sourceCharacter as BaseCharacter) is BaseCharacter)
                && (sourceCharacter as BaseCharacter).UnitController.UnitControllerMode == UnitControllerMode.AI && Vector3.Distance(sourceCharacter.AbilityManager.UnitGameObject.transform.position, target.transform.position) > targetable.MaxRange) {
                return false;
            }

            // if this ability requires no target, then we can always cast it
            if (targetable.RequiresTarget == false) {
                return true;
            }

            // if we got here, we require a target, therefore if we don't have one, we can't cast
            if (target == null) {
                if (playerInitiated && !targetable.CanCastOnSelf && !targetable.AutoSelfCast) {
                    sourceCharacter.AbilityManager.ReceiveCombatMessage(targetable.DisplayName + " requires a target!");
                }
                return false;
            }

            // determine if we are casting on ourself
            if (target.gameObject == sourceCharacter.AbilityManager.UnitGameObject) {
                targetIsSelf = true;
            }

            // first check if the target is ourself
            if (targetIsSelf == true) {
                if (targetable.CanCastOnSelf == false) {
                    if (playerInitiated) {
                        sourceCharacter.AbilityManager.ReceiveCombatMessage(targetable.DisplayName + " cannot be cast on self!");
                    }
                    return false;
                } else {
                    return true;
                }
            }

            // if we made it this far, the target is not ourself

            // the target is another unit, but this ability cannot be cast on others
            if (targetable.CanCastOnOthers == false) {
                if (playerInitiated && !targetable.CanCastOnSelf && !targetable.AutoSelfCast) {
                    sourceCharacter.AbilityManager.ReceiveCombatMessage(targetable.DisplayName + " cannot be cast on others");
                }
                return false;
            }

            targetCharacterUnit = CharacterUnit.GetCharacterUnit(target);
            if (targetCharacterUnit != null) {

                // liveness checks
                if (targetCharacterUnit.BaseCharacter.CharacterStats.IsAlive == false && targetable.RequiresLiveTarget == true) {
                    //Debug.Log("This ability requires a live target");
                    if (playerInitiated && !targetable.CanCastOnSelf && !targetable.AutoSelfCast) {
                        sourceCharacter.AbilityManager.ReceiveCombatMessage(targetable.DisplayName + " requires a live target!");
                    }
                    return false;
                }
                if (targetCharacterUnit.BaseCharacter.CharacterStats.IsAlive == true && targetable.RequireDeadTarget == true) {
                    //Debug.Log("This ability requires a dead target");
                    if (playerInitiated && !targetable.CanCastOnSelf && !targetable.AutoSelfCast) {
                        sourceCharacter.AbilityManager.ReceiveCombatMessage(targetable.DisplayName + " requires a dead target!");
                    }
                    return false;
                }

                if (!sourceCharacter.AbilityManager.PerformFactionCheck(targetable, targetCharacterUnit, targetIsSelf)) {
                    if (playerInitiated && !targetable.CanCastOnSelf && !targetable.AutoSelfCast) {
                        sourceCharacter.AbilityManager.ReceiveCombatMessage("Cannot cast " + targetable.DisplayName + " on target. Faction reputation requirement not met!");
                    }
                    return false;
                }

            } else {
                if (targetable.RequiresLiveTarget == true || targetable.RequireDeadTarget == true) {
                    // something that is not a character unit cannot satisfy the alive or dead conditions because it is inanimate
                    if (playerInitiated && !targetable.CanCastOnSelf && !targetable.AutoSelfCast) {
                        sourceCharacter.AbilityManager.ReceiveCombatMessage(targetable.DisplayName + " requires an animate target!");
                    }
                    return false;
                }
                if (targetable.CanCastOnFriendly == true || targetable.CanCastOnNeutral == true || targetable.CanCastOnEnemy == true) {
                    // something that is not a character unit cannot satisfy the relationship conditions because it is inanimate
                    if (playerInitiated && !targetable.CanCastOnSelf && !targetable.AutoSelfCast) {
                        sourceCharacter.AbilityManager.ReceiveCombatMessage(targetable.DisplayName + " requires an animate target!");
                    }
                    return false;
                }
            }

            // if we made it this far we passed liveness and relationship checks.
            // since the target is not ourself, and it is valid, we should perform a range check

            if (!sourceCharacter.AbilityManager.IsTargetInRange(target, targetable, abilityEffectContext)) {
                //Debug.Log(DisplayName + ".BaseAbility.CanUseOn(): returning false: NOT IN RANGE");
                if (playerInitiated && !targetable.CanCastOnSelf && !targetable.AutoSelfCast) {
                    sourceCharacter.AbilityManager.ReceiveCombatMessage("Cannot cast " + targetable.DisplayName + ". target is not in range!");
                }
                return false;
            }

            //Debug.Log(DisplayName + ".BaseAbility.CanUseOn(): returning true");
            return true;
        }

    }
}