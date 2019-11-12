using AnyRPG;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace AnyRPG {
    public interface ICharacterStats {

        event System.Action<StatusEffectNode> OnStatusEffectAdd;
        int currentHealth { get; }
        int currentMana { get; }
        bool IsAlive { get; }
        int MyAgility { get; }
        int MyArmor { get; }
        int MyBaseAgility { get; }
        ICharacter MyBaseCharacter { get; set; }
        int MyBaseIntellect { get; }
        int MyBaseStamina { get; }
        int MyBaseStrength { get; }
        int MyCurrentXP { get; }
        float MyHitBox { get; }
        int MyIntellect { get; }
        int MyLevel { get; }
        int MyMaxHealth { get; }
        int MyMaxMana { get; }
        Dictionary<string, StatusEffectNode> MyStatusEffects { get; }

        int MyMeleeDamage { get; }
        int MySpellPower { get; }
        int MyStamina { get; }
        int MyStrength { get; }
        float MyWalkSpeed { get; }
        float MyMovementSpeed { get; }
        Dictionary<StatBuffType, Stat> MyPrimaryStatModifiers { get; }


        event Action<int, int> OnHealthChanged;
        event Action<int, int> OnManaChanged;
        event Action<CharacterStats> OnDie;
        event Action<CharacterStats> BeforeDie;
        event Action OnReviveBegin;
        event Action OnReviveComplete;

        void Die();
        void Revive();
        void GainLevel();
        void GainXP(int xp);
        float GetDamageModifiers();
        Vector3 GetTransFormPosition();
        void RecoverHealth(int health, BaseCharacter source, bool showCombatText = true);
        void RecoverMana(int mana, BaseCharacter source, bool showCombatText = true);
        void ReduceHealth(int amount);
        void ResetHealth();
        void SetHealth(int health);
        void SetLevel(int newLevel);
        void SetMana(int mana);
        void Start();
        void UseMana(int usedMana);
        StatusEffectNode ApplyStatusEffect(StatusEffect statusEffect, BaseCharacter source, CharacterUnit target, AbilityEffectOutput abilityEffectInput);
        void CreateEventSubscriptions();
        void OrchestratorSetLevel();
    }
}