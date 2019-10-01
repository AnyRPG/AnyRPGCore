using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacterAbilityManager {
    ICharacter MyBaseCharacter { get; set; }

    event Action<BaseCharacter> OnCastStop;
    event Action<IAbility, float> OnCastTimeChanged;

    
    Dictionary<string, IAbility> MyAbilityList { get; }
    bool MyWaitingForAnimatedAbility { get; set; }
    void BeginAbility(IAbility ability);
    void BeginAbility(IAbility ability, GameObject target);
    void OnManualMovement();
    void PerformAbility(IAbility ability, GameObject target, Vector3 groundTarget);
    void BeginPerformAbilityHitDelay(BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput, ChanneledEffect channeledEffect);
    void BeginDestroyAbilityEffectObject(GameObject abilityEffectObject, BaseCharacter source, GameObject target, float timer, AbilityEffectOutput abilityEffectInput, FixedLengthEffect fixedLengthEffect);
    IEnumerator PerformAbilityCast(IAbility ability, GameObject target);
    void StopCasting();
    bool LearnAbility(string abilityName);
    void UnlearnAbility(string abilityName);
    void ActivateTargettingMode(Color groundTargetColor);
    void DeActivateTargettingMode();
    bool HasAbility(string abilityName);
    bool MyIsCasting { get; set; }
}