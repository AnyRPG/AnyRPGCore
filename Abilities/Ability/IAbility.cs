using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public interface IAbility {
        bool MyAutoLearn { get; }
        Sprite MyIcon { get; }
        string MyName { get; }
        int MyRequiredLevel { get; }
        bool MyUseableWithoutLearning { get; }
        bool MyIgnoreGlobalCoolDown { get; }
        bool CanSimultaneousCast { get; }
        int MyAbilityManaCost { get; set; }
        float MyAbilityCastingTime { get; set; }
        bool MyRequiresTarget { get; set; }
        bool MyRequiresGroundTarget { get; set; }
        Color MyGroundTargetColor { get; set; }
        bool CanCastOnEnemy { get; }
        bool MyCanCastOnSelf { get; }
        bool CanCastOnFriendly { get; }
        //AnimationClip MyAnimationClip { get; set; }
        AnimationClip MyCastingAnimationClip { get; }
        List<PrefabProfile> MyHoldableObjects { get; set; }
        AudioClip MyCastingAudioClip { get; }
        bool MyAnimatorCreatePrefabs { get; set; }
        bool MyRequireOutOfCombat { get; set; }


        bool CanUseOn(GameObject target, IAbilityCaster source);
        bool Cast(IAbilityCaster source, GameObject target, Vector3 GroundTarget);
        string GetDescription();
        string GetSummary();
        GameObject ReturnTarget(IAbilityCaster source, GameObject target);
        bool Use();
        void StartCasting(IAbilityCaster source);
        float OnCastTimeChanged(float currentCastTime, float nextTickTime, IAbilityCaster source, GameObject target);
        //void HandleCastStop(BaseCharacter source);

    }
}