using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    float MyRemainingCoolDown { get; set; }
    AnimationClip MyAnimationClip { get; set; }
    AnimationClip MyCastingAnimationClip { get; set; }
    string MyHoldableObjectName { get; set; }
    AudioClip MyCastingAudioClip { get; set; }


    bool CanUseOn(GameObject target, BaseCharacter source);
    bool Cast(BaseCharacter source, GameObject target, Vector3 GroundTarget);
    string GetDescription();
    string GetSummary();
    GameObject ReturnTarget(BaseCharacter source, GameObject target);
    void Use();
    void StartCasting(BaseCharacter source);
    void OnCastTimeChanged(float currentCastTime, BaseCharacter source, GameObject target);
    void HandleCastStop(BaseCharacter source);

}