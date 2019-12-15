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
        bool MyCanSimultaneousCast { get; }
        int MyAbilityManaCost { get; set; }
        float MyAbilityCastingTime { get; set; }
        bool MyRequiresTarget { get; set; }
        bool MyRequiresGroundTarget { get; set; }
        Color MyGroundTargetColor { get; set; }
        bool MyCanCastOnEnemy { get; }
        bool MyCanCastOnSelf { get; }
        bool MyCanCastOnFriendly { get; }
        //AnimationClip MyAnimationClip { get; set; }
        AnimationClip MyCastingAnimationClip { get; set; }
        List<PrefabProfile> MyHoldableObjects { get; set; }
        AudioClip MyCastingAudioClip { get; set; }
        bool MyAnimatorCreatePrefabs { get; set; }
        bool MyRequireOutOfCombat { get; set; }


        bool CanUseOn(GameObject target, BaseCharacter source);
        bool Cast(BaseCharacter source, GameObject target, Vector3 GroundTarget);
        string GetDescription();
        string GetSummary();
        GameObject ReturnTarget(BaseCharacter source, GameObject target);
        bool Use();
        void StartCasting(BaseCharacter source);
        void OnCastTimeChanged(float currentCastTime, BaseCharacter source, GameObject target);
        void HandleCastStop(BaseCharacter source);

    }
}