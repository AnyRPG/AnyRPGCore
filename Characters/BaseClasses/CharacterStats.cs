using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour, ICharacterStats {
    //public static event Action<CharacterStats> OnCharacterStatsAdded = delegate { };
    //public static event Action<CharacterStats> OnCharacterStatsRemoved = delegate { };

    public event System.Action<int, int> OnHealthChanged = delegate { };
    public event System.Action<int, int> OnManaChanged = delegate { };
    public event System.Action<CharacterStats> OnDie = delegate { };
    public event System.Action<CharacterStats> BeforeDie = delegate { };
    public event System.Action OnReviveBegin = delegate { };
    public event System.Action OnReviveComplete = delegate { };
    public event System.Action<StatusEffectNode> OnStatusEffectAdd = delegate { };
    public event System.Action OnStatChanged = delegate { };

    // starting level
    [SerializeField]
    private int level;

    // a stat multiplier to make creatures more difficult
    [SerializeField]
    protected int toughness;

    // keep track of current level
    private int currentLevel;

    private int stamina;
    private int intellect;
    private int strength;
    private int agility;

    protected float walkSpeed = 1f;
    protected float runSpeed = 3.5f;

    public int currentHealth { get; private set; }
    public int currentMana { get; private set; }

    private float hitBox = 1.5f;

    protected Stat meleeDamageModifiers = new Stat();
    protected Stat armorModifiers = new Stat();

    protected Dictionary<StatBuffType, Stat> primaryStatModifiers = new Dictionary<StatBuffType, Stat>();
    //protected List<StatusEffect> statusEffects = new List<StatusEffect>();
    protected Dictionary<string, StatusEffectNode> statusEffects = new Dictionary<string, StatusEffectNode>();
    protected ICharacter baseCharacter;

    private bool isAlive = true;
    private int currentXP = 0;

    protected bool startHasRun = false;
    protected bool eventReferencesInitialized = false;

    public int MyBaseMeleeDamage { get => (MyStrength / 2); }
    public int MyMeleeDamage { get => MyBaseMeleeDamage + meleeDamageModifiers.GetValue(); }
    public int MySpellPower { get => MyIntellect / 2; }
    public int MyArmor { get => armorModifiers.GetValue(); }
    public int MyBaseStamina { get => stamina; }
    //public int MyStamina { get => (int)((stamina + primaryStatModifiers[StatBuffType.Stamina].GetValue()) * primaryStatModifiers [StatBuffType.Stamina].GetMultiplyValue()); }
    public int MyStamina { get => (int)((stamina + GetAddModifiers(StatBuffType.Stamina)) * GetMultiplyModifiers(StatBuffType.Stamina)); }
    public int MyBaseStrength { get => strength; }
    public int MyStrength { get => (int)((strength + GetAddModifiers(StatBuffType.Strength)) * GetMultiplyModifiers(StatBuffType.Strength)); }
    public int MyBaseIntellect { get => intellect; }
    public int MyIntellect { get => (int)((intellect + GetAddModifiers(StatBuffType.Intellect)) * GetMultiplyModifiers(StatBuffType.Intellect)); }
    public float MyWalkSpeed { get => walkSpeed; }
    public float MyMovementSpeed { get => (runSpeed + GetAddModifiers(StatBuffType.MovementSpeed)) * GetMultiplyModifiers(StatBuffType.MovementSpeed); }
    public int MyBaseAgility { get => agility; }
    public int MyAgility { get => (int)((agility + GetAddModifiers(StatBuffType.Agility)) * GetMultiplyModifiers(StatBuffType.Agility)); }
    public float MyHitBox { get => hitBox; }
    public bool IsAlive { get => isAlive; }
    public ICharacter MyBaseCharacter { get => baseCharacter; set => baseCharacter = value; }

    public int MyLevel { get => currentLevel; }
    public int MyCurrentXP { get => currentXP; set => currentXP = value; }
    public int MyMaxHealth { get => MyStamina * 10; }
    public int MyMaxMana { get => MyIntellect * 10; }

    public Dictionary<StatBuffType, Stat> MyPrimaryStatModifiers { get => primaryStatModifiers; }
    public Dictionary<string, StatusEffectNode> MyStatusEffects { get => statusEffects; }
    public int MyToughness { get => toughness; set => toughness = value; }

    protected virtual void Awake() {
        //Debug.Log(gameObject.name + ".CharacterStats.Awake()");
        foreach (StatBuffType statBuffType in Enum.GetValues(typeof(StatBuffType))) {
            primaryStatModifiers.Add(statBuffType, new Stat());
        }
        primaryStatModifiers[StatBuffType.Stamina].OnModifierUpdate += HealthChangedNotificationHandler;
        primaryStatModifiers[StatBuffType.Intellect].OnModifierUpdate += ManaChangedNotificationHandler;
    }

    public virtual void Start() {
        //Debug.Log(gameObject.name + ".CharacterStats.Start()");
        if (currentLevel == 0) {
            // if it is not zero, we have probably been initialized some other way, and don't need to do this
            SetLevel(level);
        }
        startHasRun = true;
        CreateEventReferences();
    }

    public virtual void CreateEventReferences() {

    }

    public virtual void CleanupEventReferences() {
        ClearStatusEffects();
    }

    public virtual void OnDisable() {
        //Debug.Log(gameObject.name + ".CharacterStats.OnDisable()");
        CleanupEventReferences();
        //ClearStatusEffects();
    }

    public virtual void OnDestroy() {
        //Debug.Log(gameObject.name + ".CharacterStats.OnDestroy()");

        //ClearStatusEffects();
    }


    protected virtual int GetAddModifiers(StatBuffType statBuffType) {
        //Debug.Log(gameObject.name + ".CharacterStats.GetAddModifiers(" + statBuffType.ToString() + ")");
        int returnValue = 0;
        foreach (StatusEffectNode statusEffectNode in MyStatusEffects.Values) {
            if (statusEffectNode.MyStatusEffect.MyStatBuffTypes.Contains(statBuffType)) {
                returnValue += statusEffectNode.MyStatusEffect.MyCurrentStacks * statusEffectNode.MyStatusEffect.MyStatAmount;
            }
        }
        returnValue += primaryStatModifiers[statBuffType].GetValue();
        return returnValue;
    }

    public virtual float GetDamageModifiers() {
        //Debug.Log("CharacterStats.GetDamageModifiers()");
        float returnValue = 1f;
        foreach (StatusEffectNode statusEffectNode in MyStatusEffects.Values) {
            //Debug.Log("CharacterStats.GetDamageModifiers(): looping through status effects");
            if (statusEffectNode.MyStatusEffect.MyIncomingDamageMultiplier != 1) {
                //Debug.Log("CharacterStats.GetDamageModifiers(): looping through status effects: ");
                returnValue *= statusEffectNode.MyStatusEffect.MyCurrentStacks * statusEffectNode.MyStatusEffect.MyIncomingDamageMultiplier;
            }
        }
        //Debug.Log("CharacterStats.GetDamageModifiers() returning: " + returnValue);
        return returnValue;
    }

    protected virtual float GetMultiplyModifiers(StatBuffType statBuffType) {
        float returnValue = 1f;
        foreach (StatusEffectNode statusEffectNode in MyStatusEffects.Values) {
            if (statusEffectNode.MyStatusEffect.MyStatBuffTypes.Contains(statBuffType)) {
                returnValue *= statusEffectNode.MyStatusEffect.MyCurrentStacks * statusEffectNode.MyStatusEffect.MyStatMultiplier;
            }
        }
        return returnValue;
    }

    public virtual StatusEffectNode ApplyStatusEffect(StatusEffect statusEffect, BaseCharacter source, CharacterUnit target, AbilityEffectOutput abilityEffectInput) {
        //Debug.Log(gameObject.name + ".CharacterStats.ApplyStatusEffect(" + statusEffect.MyAbilityEffectName + ", " + source.name + ", " + (target == null ? "null" : target.name) + ")");
        if (IsAlive == false && statusEffect.MyRequiresLiveTarget == true) {
            Debug.Log("Cannot apply status effect to dead character. return null.");
            return null;
        }
        if (IsAlive == true && statusEffect.MyRequireDeadTarget == true) {
            Debug.Log("Cannot apply status effect to dead character. return null.");
            return null;
        }
        if (statusEffect == null) {
            Debug.Log("CharacterStats.ApplyAbilityEffect() abilityEffect is null");
        }
        if (source == null) {
            Debug.Log("CharacterStats.ApplyAbilityEffect() source is null");
        }
        if (target == null) {
            Debug.Log("CharacterStats.ApplyAbilityEffect() target is null");
        }
        //Debug.Log("CharacterStats.ApplyStatusEffect(" + statusEffect.ToString() + ", " + source.name + ", " + target.name + ")");
        //Debug.Log("statuseffects count: " + statusEffects.Count);

        StatusEffect comparedStatusEffect = null;
        string peparedString = SystemResourceManager.prepareStringForMatch(statusEffect.MyName);
        if (statusEffects.ContainsKey(peparedString)) {
            comparedStatusEffect = statusEffects[peparedString].MyStatusEffect;
        }

        //Debug.Log("comparedStatusEffect: " + comparedStatusEffect);
        if (comparedStatusEffect != null) {
            if (!comparedStatusEffect.AddStack()) {
                //Debug.Log("Could not apply " + statusEffect.MyAbilityEffectName + ".  Max stack reached");
            } else {
                //AddStatusEffectModifiers(statusEffect);
                HandleChangedNotifications(comparedStatusEffect);
            }
            return null;
        } else {
            // maybe resource id to see if it's an original uninstantiated one?

            // add to effect list since it was not in there
            StatusEffect _statusEffect = SystemAbilityEffectManager.MyInstance.GetNewResource(statusEffect.MyName) as StatusEffect;
            StatusEffectNode newStatusEffectNode = new StatusEffectNode();
            statusEffects.Add(SystemResourceManager.prepareStringForMatch(_statusEffect.MyName), newStatusEffectNode);
            _statusEffect.Initialize(source, target.GetComponent<CharacterUnit>().MyCharacter, abilityEffectInput);
            Coroutine newCoroutine = StartCoroutine(Tick(source, abilityEffectInput, target.GetComponent<CharacterUnit>().MyCharacter, _statusEffect));
            newStatusEffectNode.Setup(this, _statusEffect, newCoroutine);
            HandleAddNotifications(newStatusEffectNode);
            return newStatusEffectNode;
        }
    }

    private void HandleAddNotifications(StatusEffectNode statusEffectNode) {
        //Debug.Log(gameObject.name + ".CharacterStats.HandleChangedNOtifications(" + statusEffect.MyName + "): NOTIFYING STATUS EFFECT UPDATE");
        OnStatusEffectAdd(statusEffectNode);
        HandleChangedNotifications(statusEffectNode.MyStatusEffect);
    }

    public void HandleChangedNotifications(StatusEffect statusEffect) {
        //Debug.Log(gameObject.name + ".CharacterStats.HandleChangedNotifications(" + (statusEffect == null ? "null" : statusEffect.MyName) + ")");
        if (statusEffect.MyStatBuffTypes.Contains(StatBuffType.Intellect)) {
            //Debug.Log(gameObject.name + ".CharacterStats.HandleChangedNOtifications(" + statusEffect.MyName + "): NOTIFYING MANA CHANGED");
            ManaChangedNotificationHandler();
            StatChangedNotificationHandler();
        }
        if (statusEffect.MyStatBuffTypes.Contains(StatBuffType.Stamina)) {
            //Debug.Log(gameObject.name + ".CharacterStats.HandleChangedNOtifications(" + statusEffect.MyName + "): NOTIFYING HEALTH CHANGED");
            HealthChangedNotificationHandler();
            StatChangedNotificationHandler();
        }
        if (statusEffect.MyStatBuffTypes.Contains(StatBuffType.Strength)) {
            //Debug.Log(gameObject.name + ".CharacterStats.HandleChangedNOtifications(" + statusEffect.MyName + "): NOTIFYING HEALTH CHANGED");
            StatChangedNotificationHandler();
        }
        if (statusEffect.MyStatBuffTypes.Contains(StatBuffType.Agility)) {
            //Debug.Log(gameObject.name + ".CharacterStats.HandleChangedNOtifications(" + statusEffect.MyName + "): NOTIFYING HEALTH CHANGED");
            StatChangedNotificationHandler();
        }
        if (statusEffect.MyStatBuffTypes.Contains(StatBuffType.MovementSpeed)) {
            //Debug.Log(gameObject.name + ".CharacterStats.HandleChangedNOtifications(" + statusEffect.MyName + "): NOTIFYING HEALTH CHANGED");
            StatChangedNotificationHandler();
        }

        if (statusEffect.MyFactionModifiers.Count > 0) {
            //Debug.Log(gameObject.name + ".CharacterStats.HandleChangedNOtifications(" + statusEffect.MyName + "): NOTIFYING REPUTATION CHANGED");
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.NotifyOnReputationChange();
            }
        }
    }

    private void AddStatusEffectModifiers(StatusEffect statusEffect) {
        //Debug.Log(gameObject.name + ".CharacterStats.AddStatusEffectModifiers()");
        foreach (StatBuffType statBuffType in statusEffect.MyStatBuffTypes) {
            //Debug.Log(gameObject.name + ".CharacterStats.AddStatusEffectModifiers() statBuffType: " + statBuffType);
            primaryStatModifiers[statBuffType].AddModifier(statusEffect.MyStatAmount);
            primaryStatModifiers[statBuffType].AddMultiplyModifier(statusEffect.MyStatMultiplier);
        }
    }

    public void HandleStatusEffectRemoval(StatusEffect statusEffect) {
        //Debug.Log("CharacterStats.HandleStatusEffectRemoval(" + statusEffect.name + ")");
        string preparedString = SystemResourceManager.prepareStringForMatch(statusEffect.MyName);
        if (statusEffects.ContainsKey(preparedString)) {
            if (statusEffects[preparedString].MyMonitorCoroutine != null) {
                StopCoroutine(statusEffects[preparedString].MyMonitorCoroutine);
            }
            statusEffects.Remove(preparedString);
        }

        // should reset health back down after buff expires
        HandleChangedNotifications(statusEffect);
    }

    public void GainXP(int xp) {
        //Debug.Log(gameObject.name + ": GainXP(" + xp + ")");
        currentXP += xp;
        int overflowXP = 0;
        while (currentXP - LevelEquations.GetXPNeededForLevel(currentLevel) >= 0) {
            overflowXP = currentXP - LevelEquations.GetXPNeededForLevel(currentLevel);
            GainLevel();
            currentXP = overflowXP;
        }
        
        CombatTextManager.MyInstance.SpawnCombatText(baseCharacter.MyCharacterUnit.gameObject, xp, CombatType.gainXP, CombatMagnitude.normal);

        SystemEventManager.MyInstance.NotifyOnXPGained();
    }

    public virtual void GainLevel() {
        // make gain level sound and graphic
        SetLevel(currentLevel + 1);
    }

    public virtual void SetLevel(int newLevel) {
        //Debug.Log(gameObject.name + ".CharacterStats.SetLevel(" + newLevel + ")");
        // arbitrary toughness cap of 5 for now.  add this as system configuration option later maybe
        int usedToughNess = (int)Mathf.Clamp(toughness, 1, 5);
        currentLevel = newLevel;
        stamina = currentLevel * 10 * usedToughNess;
        intellect = currentLevel * 10 * usedToughNess;
        strength = currentLevel * 10 * usedToughNess;
        agility = currentLevel * 10 * usedToughNess;
        ResetHealth();
        ResetMana();
    }

    public Vector3 GetTransFormPosition() {
        return transform.position;
    }

    public virtual void UseMana(int usedMana) {
        usedMana = Mathf.Clamp(usedMana, 0, int.MaxValue);
        currentMana -= usedMana;
        currentMana = Mathf.Clamp(currentMana, 0, int.MaxValue);
        OnManaChanged(MyMaxMana, currentMana);
    }

    public void SetMana(int mana) {
        //Debug.Log(gameObject.name + ": setting mana: " + mana.ToString());
        mana = Mathf.Clamp(mana, 0, int.MaxValue);
        currentMana += mana;
        currentMana = Mathf.Clamp(currentMana, 0, MyMaxMana);

        // notify subscribers that our mana has changed
        OnManaChanged(MyMaxMana, currentMana);
    }

    public void RecoverMana(int mana, BaseCharacter source, bool showCombatText = true) {

        SetMana(mana);
        if (showCombatText && (baseCharacter.MyCharacterUnit.gameObject == PlayerManager.MyInstance.MyPlayerUnitObject || source == PlayerManager.MyInstance.MyCharacter.MyCharacterUnit)) {
            // spawn text over the player
            CombatTextManager.MyInstance.SpawnCombatText(baseCharacter.MyCharacterUnit.gameObject, mana, CombatType.gainMana, CombatMagnitude.normal);
        }
    }


    public void ReduceHealth(int amount) {
        //Debug.Log(gameObject.name + ".CharacterStats.ReduceHealth(" + amount + ")");
        // clamp in case we receive a negative amount
        amount = Mathf.Clamp(amount, 0, int.MaxValue);

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, int.MaxValue);
        //Debug.Log(transform.name + " takes " + amount + " damage.");

        OnHealthChanged(MyMaxHealth, currentHealth);

        if (currentHealth <= 0) {
            Die();
        }
    }

    public void SetHealth(int health) {
        //Debug.Log(gameObject.name + ": setting health: " + health.ToString());
        health = Mathf.Clamp(health, 0, int.MaxValue);
        currentHealth += health;
        currentHealth = Mathf.Clamp(currentHealth, 0, MyMaxHealth);

        // notify subscribers that our health has changed
        OnHealthChanged(MyMaxHealth, currentHealth);
    }

    public void RecoverHealth(int health, BaseCharacter source, bool showCombatText = true) {
        //Debug.Log(gameObject.name + ".CharacterStats.RecoverHealth(" + health + ", " + (source != null && source.MyDisplayName != null ? source.MyDisplayName : null) + ", " + showCombatText + ")");
        SetHealth(health);

        if (showCombatText && (baseCharacter.MyCharacterUnit.gameObject == PlayerManager.MyInstance.MyPlayerUnitObject || source == PlayerManager.MyInstance.MyCharacter)) {
            // spawn text over the player
            CombatTextManager.MyInstance.SpawnCombatText(baseCharacter.MyCharacterUnit.gameObject, health, CombatType.gainHealth, CombatMagnitude.normal);
        }
    }

    public void ManaChangedNotificationHandler() {
        currentMana = Mathf.Clamp(currentMana, 0, MyMaxMana);
        OnManaChanged(MyMaxMana, currentMana);
    }

    public void HealthChangedNotificationHandler() {
        currentHealth = Mathf.Clamp(currentHealth, 0, MyMaxHealth);
        OnHealthChanged(MyMaxHealth, currentHealth);
    }

    public void StatChangedNotificationHandler() {
        OnStatChanged();
    }

    /// <summary>
    /// Set health to maximum
    /// </summary>
    public void ResetHealth() {
        //Debug.Log(gameObject.name + ".CharacterStats.ResetHealth() : broadcasting OnHealthChanged");
        currentHealth = MyMaxHealth;

        // notify subscribers that our health has changed
        OnHealthChanged(MyMaxHealth, currentHealth);
    }

    /// <summary>
    /// Set mana to maximum
    /// </summary>
    public void ResetMana() {
        currentMana = MyMaxMana;

        // notify subscribers that our health has changed
        OnManaChanged(MyMaxMana, currentMana);
    }

    public virtual void Die () {
        //Debug.Log(gameObject.name + ".CharacterStats.Die()");
        if (isAlive) {
            isAlive = false;
            ClearStatusEffects();
            BeforeDie(this);
            OnDie(this);
        }
    }

    public virtual void Revive() {
        //Debug.Log(MyBaseCharacter.MyCharacterName + "Triggering Revive Animation");
        OnReviveBegin();
    }

    public virtual void ReviveComplete() {
        //Debug.Log(MyBaseCharacter.MyCharacterName + ": Recieved Revive Complete Signal. Resetting Character Stats.");
        ReviveRaw();
        OnReviveComplete();
    }

    public virtual void ReviveRaw() {
        isAlive = true;
        ClearInvalidStatusEffects();
        ResetHealth();
        ResetMana();
    }

    protected virtual void ClearStatusEffects() {
        //Debug.Log(gameObject.name + ".CharacterStatus.ClearStatusEffects()");
        List<StatusEffectNode> statusEffectNodes = new List<StatusEffectNode>();
        foreach (StatusEffectNode statusEffectNode in statusEffects.Values) {
            statusEffectNodes.Add(statusEffectNode);
        }
        foreach (StatusEffectNode statusEffectNode in statusEffectNodes) {
            statusEffectNode.CancelStatusEffect();
        }
        statusEffects.Clear();
    }

    protected virtual void ClearInvalidStatusEffects() {
        //Debug.Log(gameObject.name + ".CharacterStatus.ClearInvalidStatusEffects()");
        //List<string> RemoveList = new List<string>();
        List<StatusEffectNode> statusEffectNodes = new List<StatusEffectNode>();
        foreach (StatusEffectNode statusEffectNode in MyStatusEffects.Values) {
            //Debug.Log(gameObject.name + ".CharacterStatus.ClearInvalidStatusEffects(): checking statusEffectNode: " + statusEffectNode.MyStatusEffect.MyName);
            if (statusEffectNode.MyStatusEffect.MyRequireDeadTarget == true) {
                //Debug.Log(gameObject.name + ".CharacterStatus.ClearInvalidStatusEffects(): checking statusEffectNode: " + statusEffectNode.MyStatusEffect.MyName + " requires dead target");
                statusEffectNodes.Add(statusEffectNode);
            }
        }
        foreach (StatusEffectNode statusEffectNode in statusEffectNodes) {
            statusEffectNode.CancelStatusEffect();
        }

    }

    public IEnumerator Tick(BaseCharacter source, AbilityEffectOutput abilityEffectInput, BaseCharacter target, StatusEffect statusEffect) {
        //Debug.Log(MyName + ".StatusEffect.Tick() start");
        BaseCharacter characterSource = source;
        statusEffect.ApplyControlEffects(target);
        statusEffect.SetRemainingDuration(statusEffect.MyDuration);
        //Debug.Log("duration: " + duration);
        //nextTickTime = remainingDuration - tickRate;
        //DateTime nextTickTime2 = System.DateTime.Now + tickRate;
        int milliseconds = (int)((statusEffect.MyTickRate - (int)statusEffect.MyTickRate) * 1000);
        //Debug.Log(abilityEffectName + ".StatusEffect.Tick() milliseconds: " + milliseconds);
        TimeSpan tickRateTimeSpan = new TimeSpan(0, 0, 0, (statusEffect.MyTickRate == 0f ? (int)statusEffect.MyDuration + 1 : (int)statusEffect.MyTickRate), milliseconds);
        //Debug.Log(abilityEffectName + ".StatusEffect.Tick() tickRateTimeSpan: " + tickRateTimeSpan);
        statusEffect.MyNextTickTime = System.DateTime.Now + tickRateTimeSpan;
        //Debug.Log(abilityEffectName + ".StatusEffect.Tick() nextTickTime: " + nextTickTime);

        while (statusEffect.GetRemainingDuration() >= 0 && target != null) {
            //Debug.Log(gameObject.name + ".CharacterStats.Tick(): statusEffect: " + statusEffect.MyName + "; remaining: " + statusEffect.GetRemainingDuration());
            statusEffect.SetRemainingDuration(statusEffect.GetRemainingDuration() - Time.deltaTime);

            // check for tick first so we can do final tick;
            if (System.DateTime.Now > statusEffect.MyNextTickTime && statusEffect.MyTickRate != 0) {
                //Debug.Log(MyName + ".StatusEffect.Tick() TickTime!");
                if (target != null && target.MyCharacterUnit != null && characterSource != null) {
                    statusEffect.CastTick(characterSource, target.MyCharacterUnit.gameObject, abilityEffectInput);
                    statusEffect.MyNextTickTime += tickRateTimeSpan;
                }
            }
            statusEffect.UpdateStatusNode();
            yield return null;
        }
        if (target != null) {
            if (source != null & target.MyCharacterUnit != null) {
                statusEffect.CastComplete(source, target.MyCharacterUnit.gameObject, abilityEffectInput);
            }
        }

        statusEffects[SystemResourceManager.prepareStringForMatch(statusEffect.MyName)].CancelStatusEffect();
    }
}
