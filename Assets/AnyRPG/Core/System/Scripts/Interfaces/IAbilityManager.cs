using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public interface IAbilityManager {

        string Name { get; }

        GameObject UnitGameObject { get; }

        int Level { get; }

        /// <summary>
        /// True if casting or performing animated ability
        /// </summary>
        bool PerformingAbility { get; }

        bool ControlLocked { get; }

        bool IsDead { get; }

        void GeneratePower(BaseAbility ability);

        bool BeginAbility(string abilityName);

        bool CanCastAbility(BaseAbility ability, bool playerInitiated = false);

        Dictionary<string, BaseAbility> RawAbilityList { get; }

        AudioClip GetAnimatedAbilityHitSound();

        //Interactable ReturnTarget(AbilityEffect abilityEffect, Interactable target);
        float PerformAnimatedAbility(AnimationClip animationClip, AnimatedAbility animatedAbility, BaseCharacter targetBaseCharacter, AbilityEffectContext abilityEffectContext);

        void SetMountedState(UnitController mountUnitController, UnitProfile mountUnitProfile);

        AttachmentPointNode GetHeldAttachmentPointNode(AbilityAttachmentNode attachmentNode);

        void AddAbilityObject(AbilityAttachmentNode abilityAttachmentNode, GameObject go);

        /// <summary>
        /// return a list of weapon ability objects
        /// </summary>
        /// <returns></returns>
        List<AbilityAttachmentNode> GetWeaponAbilityObjectList();

        /// <summary>
        /// return a list of auto-attack on hit effects for the currently equipped weapon
        /// </summary>
        /// <returns></returns>
        List<AbilityEffect> GetDefaultHitEffects();

        /// <summary>
        /// return a list of auto-attack animations for the currently equipped weapon
        /// </summary>
        /// <returns></returns>
        List<AnimationClip> GetDefaultAttackAnimations();

        /// <summary>
        /// return a list of auto-attack animations for the current unit
        /// </summary>
        /// <returns></returns>
        List<AnimationClip> GetUnitAttackAnimations();

        /// <summary>
        /// return an AnimationProps object for the unit
        /// </summary>
        /// <returns></returns>
        AnimationProps GetUnitAnimationProps();

        /// <summary>
        /// return a list of cast animations for the current unit
        /// </summary>
        /// <returns></returns>
        List<AnimationClip> GetUnitCastAnimations();

        /// <summary>
        /// false if the caster does not know that ability
        /// </summary>
        /// <param name="baseAbility"></param>
        /// <returns></returns>
        bool HasAbility(BaseAbility baseAbility);

        /// <summary>
        /// True if the target is in line of sight of the caster
        /// </summary>
        /// <param name="target"></param>
        /// <param name="targetable"></param>
        /// <returns></returns>
        bool PerformLOSCheck(Interactable target, ITargetable targetable, AbilityEffectContext abilityEffectContext = null);

        float GetMeleeRange();

        void PerformCastingAnimation(AnimationClip animationClip, BaseAbility baseAbility);

        /// <summary>
        /// give a chance to cast any onhit abilities from the equipped weapon
        /// </summary>
        /// <param name="attackEffect"></param>
        void ProcessWeaponHitEffects(AttackEffect attackEffect, Interactable target, AbilityEffectContext abilityEffectOutput);

        /// <summary>
        /// return a float that increases damage by the animation time to ensure long cast abilities get the same benefit from dps increases
        /// </summary>
        /// <returns></returns>
        float GetAnimationLengthMultiplier();

        /// <summary>
        /// any damage multipliers from status effects
        /// </summary>
        /// <returns></returns>
        float GetOutgoingDamageModifiers();

        /// <summary>
        /// The gear +damage stat
        /// </summary>
        /// <returns></returns>
        float GetPhysicalDamage();

        /// <summary>
        /// return the amount of damage to add to physical abilities
        /// </summary>
        /// <returns></returns>
        float GetPhysicalPower();

        /// <summary>
        /// return the amount of damage to add to spell abilities
        /// </summary>
        /// <returns></returns>
        float GetSpellPower();

        /// <summary>
        /// the percentage chance for any ability to do extra damage
        /// </summary>
        /// <returns></returns>
        float GetCritChance();

        /// <summary>
        /// return the casting speed multiplier. 1 is normal speed
        /// </summary>
        /// <returns></returns>
        float GetSpeed();

        /// <summary>
        /// True if the faction requirements of the caster and target are satisfied
        /// </summary>
        /// <returns></returns>
        bool PerformFactionCheck(ITargetable targetableEffect, CharacterUnit targetCharacterUnit, bool targetIsSelf);

        /// <summary>
        /// True if the target is in physical striking distance
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        bool IsTargetInMeleeRange(Interactable target);

        /*
        /// <summary>
        /// True if the target is in the correct range for the ability
        /// </summary>
        /// <returns></returns>
        bool IsTargetInAbilityEffectRange(AbilityEffect abilityEffect, Interactable target, AbilityEffectContext abilityEffectContext = null);

        /// <summary>
        /// True if the target is in the correct range for the ability
        /// </summary>
        /// <returns></returns>
        bool IsTargetInAbilityRange(BaseAbility baseAbility, Interactable target, AbilityEffectContext abilityEffectContext = null, bool notify = false);
        */

        /// <summary>
        /// True if the target is in the correct range for the targetable
        /// </summary>
        /// <returns></returns>
        bool IsTargetInRange(Interactable target, ITargetable targetable, AbilityEffectContext abilityEffectContext = null);

        /// <summary>
        /// Put an ability on cooldown and prevent it from being cast for x seconds
        /// </summary>
        /// <param name="baseAbility"></param>
        /// <param name="coolDownLength"></param>
        void BeginAbilityCoolDown(BaseAbility baseAbility, float coolDownLength = -1f);

        /// <summary>
        /// Start a global cooldown to prevent all spells from being cast
        /// </summary>
        /// <param name="usedCoolDown"></param>
        void InitiateGlobalCooldown(float coolDownToUse);

        /// <summary>
        /// start cooldowns for gcd and ability on animated abilities
        /// </summary>
        /// <param name="baseAbility"></param>
        /// <param name="animationLength"></param>
        /// <param name="abilityCoolDown"></param>
        void ProcessAbilityCoolDowns(AnimatedAbility baseAbility, float animationLength, float abilityCoolDown);

        /// <summary>
        /// despawn the ability objects
        /// </summary>
        void DespawnAbilityObjects();

        /// <summary>
        /// Give the caster a chance to activate or deactivate auto-attack
        /// </summary>
        /// <param name="target"></param>
        /// <param name="deactivateAutoAttack"></param>
        /// <returns></returns>
        bool ProcessAnimatedAbilityHit(Interactable target, bool deactivateAutoAttack);

        /// <summary>
        /// True if the caster has the weapon equipped required to cast the ability
        /// </summary>
        /// <param name="baseAbility"></param>
        /// <returns></returns>
        bool PerformWeaponAffinityCheck(BaseAbility baseAbility, bool playerInitiated = false);

        /// <summary>
        /// True if an animated ability can be performed
        /// </summary>
        /// <param name="animatedAbility"></param>
        /// <returns></returns>
        bool PerformAnimatedAbilityCheck(AnimatedAbility animatedAbility);

        /// <summary>
        /// True if the ability hit after hit/miss check
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        bool AbilityHit(Interactable target, AbilityEffectContext abilityEffectContext);

        void AddPet(CharacterUnit target);

        void AddTemporaryPet(UnitProfile unitProfile, UnitController unitController);

        /// <summary>
        /// give a chance to add the pet to the pet journal and set the current target as a currently active pet
        /// </summary>
        /// <param name="unitProfile"></param>
        /// <param name="target"></param>
        void CapturePet(UnitController targetUnitController);

        /// <summary>
        /// channeled effects have a delay
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="abilityEffectInput"></param>
        /// <param name="channeledEffect"></param>
        void BeginPerformAbilityHitDelay(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectInput, ChanneledEffect channeledEffect);

        /// <summary>
        /// Destroy ability effect objects after a certain amount of time
        /// </summary>
        /// <param name="abilityEffectObjects"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="timer"></param>
        /// <param name="abilityEffectInput"></param>
        /// <param name="fixedLengthEffect"></param>
        //void BeginDestroyAbilityEffectObject(Dictionary<PrefabProfile, GameObject> abilityEffectObjects, IAbilityCaster source, GameObject target, float timer, AbilityEffectOutput abilityEffectInput, FixedLengthEffect fixedLengthEffect);
        // moved to systemabilitycontroller

        /// <summary>
        /// If combat enabled, generate agro on the target and lock agro
        /// </summary>
        void GenerateAgro(CharacterUnit targetCharacterUnit, int usedAgroValue);

        /// <summary>
        /// if combat enabled, generate agro on the source and return true if it was a new entry
        /// </summary>
        /// <param name="targetCharacterUnit"></param>
        /// <param name="usedAgroValue"></param>
        /// <returns></returns>
        //bool AddToAggroTable(CharacterUnit targetCharacterUnit, int usedAgroValue);

        /// <summary>
        /// return any threat multipliers
        /// </summary>
        /// <returns></returns>
        float GetThreatModifiers();

        /// <summary>
        /// True if this caster is controlled by the player
        /// </summary>
        /// <returns></returns>
        bool IsPlayerControlled();

        /// <summary>
        /// pass a message to be emitted as an event
        /// </summary>
        void ReceiveCombatMessage(string messageText);

        /// <summary>
        /// pass a message to be emitted as an event
        /// </summary>
        void ReceiveMessageFeedMessage(string messageText);

    }

}