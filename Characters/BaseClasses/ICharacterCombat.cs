using System;
using UnityEngine;

public interface ICharacterCombat {

    // events
    event Action<BaseCharacter> OnAttack;
    event Action OnDropCombat;
    event Action<BaseCharacter, float> OnKillEvent;
    event System.Action<BaseCharacter, GameObject> OnHitEvent;

    AggroTable MyAggroTable { get; }
    ICharacter MyBaseCharacter { get; set; }
    AudioClip MyDefaultHitSoundEffect { get; set; }
    AudioClip MyOverrideHitSoundEffect { get; set; }


    bool MyWaitingForAutoAttack { get; set; }

    void ActivateAutoAttack();
    void Attack(BaseCharacter target);
    void AttackHitEvent();
    bool AttackHit_AnimationEvent();
    void BroadcastCharacterDeath();
    void DeActivateAutoAttack();
    bool EnterCombat(BaseCharacter target);
    bool GetInCombat();
    void OnKillConfirmed(BaseCharacter sourceCharacter, float creditPercent);
    void OnTakeDamage(int damage, BaseCharacter target, CombatType combatType, CombatMagnitude combatMagnitude, string abilityName);
    void Start();
    void TakeDamage(int damage, Vector3 sourcePosition, BaseCharacter source, CombatType combatType, CombatMagnitude combatMagnitude, string abilityName);
    void TryToDropCombat();
    void ResetAttackCoolDown();
    void SetWaitingForAutoAttack(bool newValue);
}