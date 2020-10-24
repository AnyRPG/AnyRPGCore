using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public interface IAbility {
        bool AutoLearn { get; }
        Sprite Icon { get; }
        string DisplayName { get; }
        int RequiredLevel { get; }
        bool UseableWithoutLearning { get; }
        bool IgnoreGlobalCoolDown { get; }
        bool CanSimultaneousCast { get; }
        float BaseAbilityCastingTime { get; }
        bool RequiresTarget { get; set; }
        bool RequiresGroundTarget { get; set; }
        Color GroundTargetColor { get; set; }
        bool CanCastOnEnemy { get; }
        bool CanCastWhileMoving { get; }
        bool CanCastOnSelf { get; }
        bool CanCastOnFriendly { get; }
        //AnimationClip MyAnimationClip { get; set; }
        AnimationClip CastingAnimationClip { get; }
        List<AbilityAttachmentNode> HoldableObjectList { get; set; }
        AudioClip CastingAudioClip { get; }
        bool AnimatorCreatePrefabs { get; set; }
        bool RequireOutOfCombat { get; set; }
        PowerResource PowerResource { get; }
        PowerResource GeneratePowerResource { get; set; }
        int BaseResourceGain { get; set; }
        int ResourceGainPerLevel { get; set; }
        float SpendDelay { get; set; }
        bool UseSpeedMultipliers { get; set; }
        List<CharacterClass> CharacterClassRequirementList { get; set; }

        float GetAbilityCastingTime(IAbilityCaster abilityCaster);


        bool CanUseOn(GameObject target, IAbilityCaster source, bool performCooldownChecks, AbilityEffectContext abilityEffectContext = null);
        bool Cast(IAbilityCaster source, GameObject target, AbilityEffectContext abilityEffectContext);
        string GetDescription();
        string GetShortDescription();
        string GetSummary();
        GameObject ReturnTarget(IAbilityCaster source, GameObject target, bool performCooldownChecks = true, AbilityEffectContext abilityEffectContext = null);
        bool Use();
        void StartCasting(IAbilityCaster source);
        float OnCastTimeChanged(float currentCastTime, float nextTickTime, IAbilityCaster source, GameObject target, AbilityEffectContext abilityEffectContext);
        //void HandleCastStop(BaseCharacter source);
        //bool PerformLOSCheck(IAbilityCaster source, GameObject target);
        bool RequirementsAreMet();
        float GetResourceCost(IAbilityCaster abilityCaster);
        float GetResourceGain(IAbilityCaster abilityCaster);
    }
}