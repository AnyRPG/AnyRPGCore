using AnyRPG;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnyRPG {

    [System.Serializable]
    public class TargetProps {

        [Header("Standard Target")]

        [Tooltip("If true, the character must have a target selected to cast this ability.")]
        [FormerlySerializedAs("requiresTarget")]
        [SerializeField]
        private bool requireTarget = false;

        [Tooltip("If true, the ability source must have an uninterrupted line of sight to the target.")]
        [SerializeField]
        private bool requireLineOfSight = false;

        [Tooltip("If true, the target must be a character and must be alive.")]
        [FormerlySerializedAs("requiresLiveTarget")]
        [SerializeField]
        private bool requireLiveTarget = true;

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

        [SerializeField]
        [ResourceSelector(resourceType = typeof(UnitType))]
        protected List<string> unitTypeRestrictions = new List<string>();

        //protected List<UnitType> unitTypeRestrictionList = new List<UnitType>();


        [Header("Range")]

        [Tooltip("If true, the target must be within melee range (within hitbox) to cast this ability.")]
        [SerializeField]
        private bool useMeleeRange = false;

        [Tooltip("If melee range is not used, this ability can be cast on targets this many meters away.")]
        [SerializeField]
        private int maxRange;

        public bool RequireTarget { get => requireTarget; set => requireTarget = value; }
        public bool RequireLineOfSight { get => requireLineOfSight; set => requireLineOfSight = value; }
        public bool RequireLiveTarget { get => requireLiveTarget; set => requireLiveTarget = value; }
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

        public virtual bool CanUseOn(ITargetable targetable, Interactable target, IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectContext = null, bool playerInitiated = false, bool performRangeCheck = true) {
            //Debug.Log("TargetProps.CanUseOn()");
            // create target booleans
            bool targetIsSelf = false;
            CharacterUnit targetCharacterUnit = null;

            // special case for ground targeted spells cast by AI since AI currently has to cast a ground targeted spell on its current target
            if (targetable.GetTargetOptions(sourceCharacter).RequiresGroundTarget == true
                && targetable.GetTargetOptions(sourceCharacter).MaxRange > 0
                && target != null
                && ((sourceCharacter as UnitController) is UnitController)
                && ((sourceCharacter as UnitController).UnitControllerMode == UnitControllerMode.AI || (sourceCharacter as UnitController).UnitControllerMode == UnitControllerMode.Pet)
                && Vector3.Distance(sourceCharacter.AbilityManager.UnitGameObject.transform.position, target.transform.position) > targetable.GetTargetOptions(sourceCharacter).MaxRange) {
                return false;
            }

            // if this ability requires no target, then we can always cast it
            if (targetable.GetTargetOptions(sourceCharacter).RequireTarget == false) {
                return true;
            }

            // if we got here, we require a target, therefore if we don't have one, we can't cast
            if (target == null) {
                if (playerInitiated && !targetable.GetTargetOptions(sourceCharacter).CanCastOnSelf && !targetable.GetTargetOptions(sourceCharacter).AutoSelfCast) {
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
                if (targetable.GetTargetOptions(sourceCharacter).CanCastOnSelf == false) {
                    if (playerInitiated) {
                        sourceCharacter.AbilityManager.ReceiveCombatMessage(targetable.DisplayName + " cannot be cast on self!");
                    }
                    return false;
                } else {
                    return true;
                }
            }

            // if we made it this far, the target is not ourself

            targetCharacterUnit = CharacterUnit.GetCharacterUnit(target);
            if (targetCharacterUnit != null) {

                // the target is another unit, but this ability cannot be cast on others
                // this check moved to inside character unit checks because others is really only meant for abilities that target characters
                // if crafting or gathering, just checking requires target should be enough and the ability will take care of any other checks
                if (targetable.GetTargetOptions(sourceCharacter).CanCastOnOthers == false) {
                    if (playerInitiated && !targetable.GetTargetOptions(sourceCharacter).CanCastOnSelf && !targetable.GetTargetOptions(sourceCharacter).AutoSelfCast) {
                        sourceCharacter.AbilityManager.ReceiveCombatMessage(targetable.DisplayName + " cannot be cast on others");
                    }
                    return false;
                }

                // liveness checks
                if (targetCharacterUnit.UnitController.CharacterStats.IsAlive == false && targetable.GetTargetOptions(sourceCharacter).RequireLiveTarget == true && targetable.GetTargetOptions(sourceCharacter).RequireDeadTarget == false) {
                    if (playerInitiated && !targetable.GetTargetOptions(sourceCharacter).CanCastOnSelf && !targetable.GetTargetOptions(sourceCharacter).AutoSelfCast) {
                        sourceCharacter.AbilityManager.ReceiveCombatMessage(targetable.DisplayName + " requires a live target!");
                    }
                    return false;
                }
                if (targetCharacterUnit.UnitController.CharacterStats.IsAlive == true && targetable.GetTargetOptions(sourceCharacter).RequireDeadTarget == true && targetable.GetTargetOptions(sourceCharacter).RequireLiveTarget == false) {
                    if (playerInitiated && !targetable.GetTargetOptions(sourceCharacter).CanCastOnSelf && !targetable.GetTargetOptions(sourceCharacter).AutoSelfCast) {
                        sourceCharacter.AbilityManager.ReceiveCombatMessage(targetable.DisplayName + " requires a dead target!");
                    }
                    return false;
                }

                if (!sourceCharacter.AbilityManager.PerformFactionCheck(targetable, targetCharacterUnit, targetIsSelf)) {
                    if (playerInitiated && !targetable.GetTargetOptions(sourceCharacter).CanCastOnSelf && !targetable.GetTargetOptions(sourceCharacter).AutoSelfCast) {
                        sourceCharacter.AbilityManager.ReceiveCombatMessage("Cannot cast " + targetable.DisplayName + " on target. Faction reputation requirement not met!");
                    }
                    return false;
                }

                // check unit type restrictions
                if (unitTypeRestrictions.Count > 0) {
                    if (targetCharacterUnit.UnitController.BaseCharacter.UnitType == null || !unitTypeRestrictions.Contains(targetCharacterUnit.UnitController.BaseCharacter.UnitType.ResourceName)) {
                        //Debug.Log(MyDisplayName + ".CapturePetEffect.CanUseOn(): pet was not allowed by your restrictions ");
                        if (playerInitiated) {
                            sourceCharacter.AbilityManager.ReceiveCombatMessage("Cannot cast " + targetable.DisplayName + " on target. Pet type was not allowed");
                        }
                        return false;
                    }
                }

            } else {
                if (targetable.GetTargetOptions(sourceCharacter).RequireLiveTarget == true || targetable.GetTargetOptions(sourceCharacter).RequireDeadTarget == true) {
                    // something that is not a character unit cannot satisfy the alive or dead conditions because it is inanimate
                    if (playerInitiated && !targetable.GetTargetOptions(sourceCharacter).CanCastOnSelf && !targetable.GetTargetOptions(sourceCharacter).AutoSelfCast) {
                        sourceCharacter.AbilityManager.ReceiveCombatMessage(targetable.DisplayName + " requires an animate target!");
                    }
                    return false;
                }
                if (targetable.GetTargetOptions(sourceCharacter).CanCastOnFriendly == true || targetable.GetTargetOptions(sourceCharacter).CanCastOnNeutral == true || targetable.GetTargetOptions(sourceCharacter).CanCastOnEnemy == true) {
                    // something that is not a character unit cannot satisfy the relationship conditions because it is inanimate
                    if (playerInitiated && !targetable.GetTargetOptions(sourceCharacter).CanCastOnSelf && !targetable.GetTargetOptions(sourceCharacter).AutoSelfCast) {
                        sourceCharacter.AbilityManager.ReceiveCombatMessage(targetable.DisplayName + " requires an animate target!");
                    }
                    return false;
                }
                if (unitTypeRestrictions.Count != 0) {
                    // something that is not a character unit cannot have a unit type
                    return false;
                }
            }

            // if we made it this far we passed liveness and relationship checks.
            // since the target is not ourself, and it is valid, we should perform a range check

            if (performRangeCheck == true && !sourceCharacter.AbilityManager.IsTargetInRange(target, targetable, abilityEffectContext)) {
                if (playerInitiated && !targetable.GetTargetOptions(sourceCharacter).CanCastOnSelf && !targetable.GetTargetOptions(sourceCharacter).AutoSelfCast) {
                    sourceCharacter.AbilityManager.ReceiveCombatMessage("Cannot cast " + targetable.DisplayName + ". target is not in range!");
                }
                return false;
            }

            //Debug.Log(targetable.DisplayName + ".CanUseOn(): return true");
            return true;
        }

    }
}