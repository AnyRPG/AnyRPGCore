using AnyRPG;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class TargetProps {

        [Header("Standard Target")]

        [Tooltip("If true, the character must have a target selected to cast this ability.")]
        [SerializeField]
        private bool requiresTarget = false;

        [Tooltip("If true, the ability source must have an uninterrupted line of sight to the target.")]
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
        private bool canCastOnSelf = false;

        [Tooltip("Can the character cast this ability on others?")]
        [SerializeField]
        private bool canCastOnOthers = false;

        [Tooltip("Can the character cast this ability on a character belonging to an enemy faction?")]
        [SerializeField]
        private bool canCastOnEnemy = false;

        [Tooltip("Can the character cast this ability on a character with no relationship?")]
        [SerializeField]
        private bool canCastOnNeutral = false;

        [Tooltip("Can the character cast this ability on a character belonging to a friendly faction?")]
        [SerializeField]
        private bool canCastOnFriendly = false;

        [Tooltip("If no target is given, automatically cast on the caster")]
        [SerializeField]
        private bool autoSelfCast = false;

        [Header("Range")]

        [Tooltip("If true, the target must be within melee range (within hitbox) to cast this ability.")]
        [SerializeField]
        private bool useMeleeRange = false;

        [Tooltip("If melee range is not used, this ability can be cast on targets this many meters away.")]
        [SerializeField]
        private int maxRange;

        public bool RequiresTarget { get => requiresTarget; set => requiresTarget = value; }
        public bool RequireLineOfSight { get => requireLineOfSight; set => requireLineOfSight = value; }
        public bool RequiresLiveTarget { get => requiresLiveTarget; set => requiresLiveTarget = value; }
        public bool RequireDeadTarget { get => requireDeadTarget; set => requireDeadTarget = value; }
        public bool CanCastOnSelf { get => canCastOnSelf; set => canCastOnSelf = value; }
        public bool CanCastOnOthers { get => canCastOnOthers; set => canCastOnOthers = value; }
        public bool CanCastOnEnemy { get => canCastOnEnemy; set => canCastOnEnemy = value; }
        public bool CanCastOnNeutral { get => canCastOnNeutral; set => canCastOnNeutral = value; }
        public bool CanCastOnFriendly { get => canCastOnFriendly; set => canCastOnFriendly = value; }
        public bool AutoSelfCast { get => autoSelfCast; set => autoSelfCast = value; }
        public bool UseMeleeRange { get => useMeleeRange; set => useMeleeRange = value; }
        public int MaxRange { get => maxRange; set => maxRange = value; }
        public virtual LineOfSightSourceLocation LineOfSightSourceLocation { get => LineOfSightSourceLocation.Caster; }
        public virtual TargetRangeSourceLocation TargetRangeSourceLocation { get => TargetRangeSourceLocation.Caster; }
        public virtual bool RequiresGroundTarget { get => false; }

        public static bool CanUseOn(ITargetable targetable, Interactable target, IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectContext = null, bool playerInitiated = false) {
            // create target booleans
            bool targetIsSelf = false;
            CharacterUnit targetCharacterUnit = null;

            // special case for ground targeted spells cast by AI since AI currently has to cast a ground targeted spell on its current target
            if (targetable.TargetOptions.RequiresGroundTarget == true
                && targetable.TargetOptions.MaxRange > 0
                && target != null
                && ((sourceCharacter as BaseCharacter) is BaseCharacter)
                && (sourceCharacter as BaseCharacter).UnitController.UnitControllerMode == UnitControllerMode.AI && Vector3.Distance(sourceCharacter.AbilityManager.UnitGameObject.transform.position, target.transform.position) > targetable.TargetOptions.MaxRange) {
                return false;
            }

            // if this ability requires no target, then we can always cast it
            if (targetable.TargetOptions.RequiresTarget == false) {
                return true;
            }

            // if we got here, we require a target, therefore if we don't have one, we can't cast
            if (target == null) {
                if (playerInitiated && !targetable.TargetOptions.CanCastOnSelf && !targetable.TargetOptions.AutoSelfCast) {
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
                if (targetable.TargetOptions.CanCastOnSelf == false) {
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
            if (targetable.TargetOptions.CanCastOnOthers == false) {
                if (playerInitiated && !targetable.TargetOptions.CanCastOnSelf && !targetable.TargetOptions.AutoSelfCast) {
                    sourceCharacter.AbilityManager.ReceiveCombatMessage(targetable.DisplayName + " cannot be cast on others");
                }
                return false;
            }

            targetCharacterUnit = CharacterUnit.GetCharacterUnit(target);
            if (targetCharacterUnit != null) {

                // liveness checks
                if (targetCharacterUnit.BaseCharacter.CharacterStats.IsAlive == false && targetable.TargetOptions.RequiresLiveTarget == true) {
                    //Debug.Log("This ability requires a live target");
                    if (playerInitiated && !targetable.TargetOptions.CanCastOnSelf && !targetable.TargetOptions.AutoSelfCast) {
                        sourceCharacter.AbilityManager.ReceiveCombatMessage(targetable.DisplayName + " requires a live target!");
                    }
                    return false;
                }
                if (targetCharacterUnit.BaseCharacter.CharacterStats.IsAlive == true && targetable.TargetOptions.RequireDeadTarget == true) {
                    //Debug.Log("This ability requires a dead target");
                    if (playerInitiated && !targetable.TargetOptions.CanCastOnSelf && !targetable.TargetOptions.AutoSelfCast) {
                        sourceCharacter.AbilityManager.ReceiveCombatMessage(targetable.DisplayName + " requires a dead target!");
                    }
                    return false;
                }

                if (!sourceCharacter.AbilityManager.PerformFactionCheck(targetable, targetCharacterUnit, targetIsSelf)) {
                    if (playerInitiated && !targetable.TargetOptions.CanCastOnSelf && !targetable.TargetOptions.AutoSelfCast) {
                        sourceCharacter.AbilityManager.ReceiveCombatMessage("Cannot cast " + targetable.DisplayName + " on target. Faction reputation requirement not met!");
                    }
                    return false;
                }

            } else {
                if (targetable.TargetOptions.RequiresLiveTarget == true || targetable.TargetOptions.RequireDeadTarget == true) {
                    // something that is not a character unit cannot satisfy the alive or dead conditions because it is inanimate
                    if (playerInitiated && !targetable.TargetOptions.CanCastOnSelf && !targetable.TargetOptions.AutoSelfCast) {
                        sourceCharacter.AbilityManager.ReceiveCombatMessage(targetable.DisplayName + " requires an animate target!");
                    }
                    return false;
                }
                if (targetable.TargetOptions.CanCastOnFriendly == true || targetable.TargetOptions.CanCastOnNeutral == true || targetable.TargetOptions.CanCastOnEnemy == true) {
                    // something that is not a character unit cannot satisfy the relationship conditions because it is inanimate
                    if (playerInitiated && !targetable.TargetOptions.CanCastOnSelf && !targetable.TargetOptions.AutoSelfCast) {
                        sourceCharacter.AbilityManager.ReceiveCombatMessage(targetable.DisplayName + " requires an animate target!");
                    }
                    return false;
                }
            }

            // if we made it this far we passed liveness and relationship checks.
            // since the target is not ourself, and it is valid, we should perform a range check

            if (!sourceCharacter.AbilityManager.IsTargetInRange(target, targetable, abilityEffectContext)) {
                //Debug.Log(DisplayName + ".BaseAbility.CanUseOn(): returning false: NOT IN RANGE");
                if (playerInitiated && !targetable.TargetOptions.CanCastOnSelf && !targetable.TargetOptions.AutoSelfCast) {
                    sourceCharacter.AbilityManager.ReceiveCombatMessage("Cannot cast " + targetable.DisplayName + ". target is not in range!");
                }
                return false;
            }

            //Debug.Log(DisplayName + ".BaseAbility.CanUseOn(): returning true");
            return true;
        }

    }
}