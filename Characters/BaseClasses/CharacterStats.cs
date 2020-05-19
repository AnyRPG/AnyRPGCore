using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterStats : MonoBehaviour {
        //public static event Action<CharacterStats> OnCharacterStatsAdded = delegate { };
        //public static event Action<CharacterStats> OnCharacterStatsRemoved = delegate { };

        public event System.Action<int, int> OnHealthChanged = delegate { };
        public event System.Action<int, int> OnPrimaryResourceAmountChanged = delegate { };
        public event System.Action<CharacterStats> OnDie = delegate { };
        public event System.Action<CharacterStats> BeforeDie = delegate { };
        public event System.Action OnReviveBegin = delegate { };
        public event System.Action OnReviveComplete = delegate { };
        public event System.Action<StatusEffectNode> OnStatusEffectAdd = delegate { };
        public event System.Action OnStatChanged = delegate { };
        public event System.Action<int> OnLevelChanged = delegate { };

        // starting level
        [SerializeField]
        private int level = 0;

        // a stat multiplier to make creatures more difficult
        [SerializeField]
        protected string toughness = string.Empty;

        [SerializeField]
        protected float sprintSpeedModifier = 1.5f;

        protected UnitToughness unitToughness;

        // keep track of current level
        private int currentLevel;

        private int stamina;
        private int intellect;
        private int strength;
        private int agility;
        protected int currentStamina;
        protected int currentIntellect;
        protected int currentStrength;
        protected int currentAgility;

        protected float walkSpeed = 1f;
        protected float runSpeed = 7f;

        protected float currentRunSpeed = 0f;
        protected float currentSprintSpeed = 0f;

        public int currentHealth { get; private set; }

        private Dictionary<PowerResource, PowerResourceNode> powerResourceDictionary = new Dictionary<PowerResource, PowerResourceNode>();


        //public int currentMana { get; private set; }

        // hitbox is now a function of character unit collider height
        //private float hitBox = 1.5f;

        protected Stat meleeDamageModifiers = new Stat();
        protected Stat armorModifiers = new Stat();

        protected float healthMultiplier = 1f;
        protected float manaMultiplier = 1f;

        protected Dictionary<StatBuffType, Stat> primaryStatModifiers = new Dictionary<StatBuffType, Stat>();
        //protected List<StatusEffect> statusEffects = new List<StatusEffect>();
        protected Dictionary<string, StatusEffectNode> statusEffects = new Dictionary<string, StatusEffectNode>();
        protected BaseCharacter baseCharacter;

        private bool isReviving = false;
        private bool isAlive = true;
        private int currentXP = 0;

        protected bool eventSubscriptionsInitialized = false;

        public float MyPhysicalDamage { get => meleeDamageModifiers.GetValue(); }
        public float MyArmor { get => armorModifiers.GetValue(); }
        public int MyBaseStamina { get => stamina; }
        public int MyStamina { get => currentStamina; }
        public int MyBaseStrength { get => strength; }
        public int MyStrength { get => currentStrength; }
        public int MyBaseIntellect { get => intellect; }
        public int MyIntellect { get => currentIntellect; }
        public float MyWalkSpeed { get => walkSpeed; }
        public float MyRunSpeed { get => currentRunSpeed; }
        public float MySprintSpeed { get => currentSprintSpeed; }
        public int MyBaseAgility { get => agility; }
        //public int MyAgility { get => (int)((agility + GetAddModifiers(StatBuffType.Agility)) * GetMultiplyModifiers(StatBuffType.Agility)); }
        public int MyAgility { get => currentAgility; }
        //public float MyHitBox { get => hitBox; }
        public bool IsAlive { get => isAlive; }
        public BaseCharacter MyBaseCharacter { get => baseCharacter; set => baseCharacter = value; }

        public int Level { get => currentLevel; }
        public int MyCurrentXP { get => currentXP; set => currentXP = value; }
        public int MyMaxHealth { get => (int)(MyStamina * 10 * healthMultiplier); }
        public PowerResource PrimaryResource {
            get {
                if (baseCharacter != null && baseCharacter.MyCharacterClass != null) {
                    if (baseCharacter.MyCharacterClass.PowerResourceList.Count > 0) {
                        return baseCharacter.MyCharacterClass.PowerResourceList[0];
                    }
                }
                return null;
            }
        }
        public int MaxPrimaryResource {
            get {
                if (PrimaryResource != null) {
                    return (int)baseCharacter.MyCharacterClass.GetResourceMaximum(baseCharacter.MyCharacterClass.PowerResourceList[0], this);
                }
                return 0;
            }
        }
        public int CurrentPrimaryResource {
            get {
                if (PrimaryResource != null) {
                    if (powerResourceDictionary.Count > 0) {
                        return (int)powerResourceDictionary[baseCharacter.MyCharacterClass.PowerResourceList[0]].currentValue;
                    }
                }
                return 0;
            }
        }

        public Dictionary<StatBuffType, Stat> MyPrimaryStatModifiers { get => primaryStatModifiers; }
        public Dictionary<string, StatusEffectNode> MyStatusEffects { get => statusEffects; }
        public UnitToughness MyToughness { get => unitToughness; set => unitToughness = value; }
        public bool MyIsReviving { get => isReviving; set => isReviving = value; }
        public Dictionary<PowerResource, PowerResourceNode> PowerResourceDictionary { get => powerResourceDictionary; set => powerResourceDictionary = value; }

        protected virtual void Awake() {
            //Debug.Log(gameObject.name + ".CharacterStats.Awake()");
        }

        public void OrchestratorSetLevel() {
            if (currentLevel == 0) {
                // if it is not zero, we have probably been initialized some other way, and don't need to do this
                SetLevel(level);
            } else {
                CalculatePrimaryStats();
            }
            TrySpawnDead();
        }

        public void OrchestratorStart() {
            GetComponentReferences();
            SetPrimaryStatModifiers();
            CreateEventSubscriptions();
            SetupScriptableObjects();
        }

        public void OrchestratorFinish() {
            CreateLateSubscriptions();
        }

        public void CalculatePrimaryStats() {
            CalculateRunSpeed();
            CalculateAgility();
            CalculateStamina();
            CalculateStrength();
            CalculateIntellect();
        }

        public void GetComponentReferences() {
            baseCharacter = GetComponent<BaseCharacter>();
        }

        public void SetCharacterClass(CharacterClass characterClass) {
            //Debug.Log(gameObject.name + ".CharacterStats.SetCharacterClass(" + (characterClass == null ? "null" : characterClass.MyName) + ")");
            powerResourceDictionary = new Dictionary<PowerResource, PowerResourceNode>();
            foreach (PowerResource powerResource in characterClass.PowerResourceList) {
                powerResourceDictionary.Add(powerResource, new PowerResourceNode());
            }
        }

        public virtual bool PerformPowerResourceCheck(IAbility ability, float resourceCost) {
            //Debug.Log(gameObject.name + ".CharacterStats.PerformPowerResourceCheck(" + (ability == null ? "null" : ability.MyName) + ", " + resourceCost + ")");
            if (resourceCost == 0f || (ability != null & ability.PowerResource == null)) {
                return true;
            }
            if (PowerResourceDictionary == null) {
                return false;
            }
            if (powerResourceDictionary.ContainsKey(ability.PowerResource) && (powerResourceDictionary[ability.PowerResource].currentValue >= resourceCost)) {
                return true;
            }
            return false;
        }

        public void SetPrimaryStatModifiers() {
            foreach (StatBuffType statBuffType in Enum.GetValues(typeof(StatBuffType))) {
                primaryStatModifiers.Add(statBuffType, new Stat());
            }
            primaryStatModifiers[StatBuffType.Stamina].OnModifierUpdate += HandleStaminaUpdate;
            primaryStatModifiers[StatBuffType.Intellect].OnModifierUpdate += HandleIntellectUpdate;
            primaryStatModifiers[StatBuffType.Strength].OnModifierUpdate += HandleStrengthUpdate;
            primaryStatModifiers[StatBuffType.Agility].OnModifierUpdate += HandleAgilityUpdate;
            primaryStatModifiers[StatBuffType.MovementSpeed].OnModifierUpdate += HandleMovementSpeedUpdate;
        }

        public virtual void CreateLateSubscriptions() {

        }

        public virtual void CreateEventSubscriptions() {
            //Debug.Log(gameObject.name + ".CharacterStats.CreateEventSubscriptions()");
            if (baseCharacter != null && baseCharacter.CharacterEquipmentManager != null) {
                //Debug.Log(gameObject.name + ".CharacterStats.CreateEventSubscriptions(): subscribing to onequipmentchanged event");
                baseCharacter.CharacterEquipmentManager.OnEquipmentChanged += OnEquipmentChanged;
            } else {
                //Debug.Log(gameObject.name + ".CharacterStats.CreateEventSubscriptions(): could not subscribe to onequipmentchanged event");
            }
        }

        public virtual void CleanupEventSubscriptions() {
            if (baseCharacter != null && baseCharacter.CharacterEquipmentManager != null) {
                baseCharacter.CharacterEquipmentManager.OnEquipmentChanged -= OnEquipmentChanged;
            }

            ClearStatusEffects();
        }

        public virtual void OnDisable() {
            //Debug.Log(gameObject.name + ".CharacterStats.OnDisable()");
            CleanupEventSubscriptions();
            //ClearStatusEffects();
        }

        public virtual void OnDestroy() {
            //Debug.Log(gameObject.name + ".CharacterStats.OnDestroy()");

            //ClearStatusEffects();
        }

        public virtual void CalculateStrength() {
            currentStrength = (int)((strength + GetAddModifiers(StatBuffType.Strength)) * GetMultiplyModifiers(StatBuffType.Strength));
        }

        public virtual void CalculateStamina() {
            currentStamina = (int)((stamina + GetAddModifiers(StatBuffType.Stamina)) * GetMultiplyModifiers(StatBuffType.Stamina));
        }

        public virtual void CalculateAgility() {
            currentAgility = (int)((agility + GetAddModifiers(StatBuffType.Agility)) * GetMultiplyModifiers(StatBuffType.Agility));
        }

        public virtual void CalculateIntellect() {
            currentIntellect = (int)((intellect + GetAddModifiers(StatBuffType.Intellect)) * GetMultiplyModifiers(StatBuffType.Intellect));
        }

        public virtual void CalculateRunSpeed() {
            currentRunSpeed = (runSpeed + GetAddModifiers(StatBuffType.MovementSpeed)) * GetMultiplyModifiers(StatBuffType.MovementSpeed);
            currentSprintSpeed = currentRunSpeed * sprintSpeedModifier;
        }

        public virtual void HandleMovementSpeedUpdate() {
            CalculateRunSpeed();
        }

        public virtual void HandleStrengthUpdate() {
            CalculateStrength();
        }

        public virtual void HandleAgilityUpdate() {
            CalculateAgility();
        }

        void OnEquipmentChanged(Equipment newItem, Equipment oldItem) {
            //Debug.Log(gameObject.name + ".CharacterStats.OnEquipmentChanged(" + (newItem != null ? newItem.MyName : "null") + ", " + (oldItem != null ? oldItem.MyName : "null") + ")");

            if (newItem != null) {
                armorModifiers.AddModifier(newItem.MyArmorModifier);
                meleeDamageModifiers.AddModifier(newItem.MyDamageModifier);
                primaryStatModifiers[StatBuffType.Stamina].AddModifier(newItem.MyStaminaModifier(Level, baseCharacter));
                primaryStatModifiers[StatBuffType.Intellect].AddModifier(newItem.MyIntellectModifier(Level, baseCharacter));
                primaryStatModifiers[StatBuffType.Strength].AddModifier(newItem.MyStrengthModifier(Level, baseCharacter));
                primaryStatModifiers[StatBuffType.Agility].AddModifier(newItem.MyAgilityModifier(Level, baseCharacter));
            }

            if (oldItem != null) {
                armorModifiers.RemoveModifier(oldItem.MyArmorModifier);
                meleeDamageModifiers.RemoveModifier(oldItem.MyDamageModifier);
                primaryStatModifiers[StatBuffType.Stamina].RemoveModifier(oldItem.MyStaminaModifier(Level, baseCharacter));
                primaryStatModifiers[StatBuffType.Intellect].RemoveModifier(oldItem.MyIntellectModifier(Level, baseCharacter));
                primaryStatModifiers[StatBuffType.Strength].RemoveModifier(oldItem.MyStrengthModifier(Level, baseCharacter));
                primaryStatModifiers[StatBuffType.Agility].RemoveModifier(oldItem.MyAgilityModifier(Level, baseCharacter));
            }

            PrimaryResourceAmountChangedNotificationHandler();
            HealthChangedNotificationHandler();
        }


        protected virtual float GetAddModifiers(StatBuffType statBuffType) {
            //Debug.Log(gameObject.name + ".CharacterStats.GetAddModifiers(" + statBuffType.ToString() + ")");
            float returnValue = 0;
            foreach (StatusEffectNode statusEffectNode in MyStatusEffects.Values) {
                if (statusEffectNode.MyStatusEffect.MyStatBuffTypes.Contains(statBuffType)) {
                    returnValue += statusEffectNode.MyStatusEffect.MyCurrentStacks * statusEffectNode.MyStatusEffect.MyStatAmount;
                }
            }
            if (primaryStatModifiers.ContainsKey(statBuffType)) {
                returnValue += primaryStatModifiers[statBuffType].GetValue();
            }
            return returnValue;
        }

        protected virtual float GetMultiplyModifiers(StatBuffType statBuffType) {
            //Debug.Log(gameObject.name + ".CharacterStats.GetMultiplyModifiers(" + statBuffType.ToString() + ")");
            float returnValue = 1f;
            foreach (StatusEffectNode statusEffectNode in MyStatusEffects.Values) {
                if (statusEffectNode.MyStatusEffect.MyStatBuffTypes.Contains(statBuffType)) {
                    returnValue *= statusEffectNode.MyStatusEffect.MyCurrentStacks * statusEffectNode.MyStatusEffect.MyStatMultiplier;
                }
            }
            return returnValue;
        }

        public virtual float GetIncomingDamageModifiers() {
            //Debug.Log("CharacterStats.GetDamageModifiers()");
            float returnValue = 1f;
            foreach (StatusEffectNode statusEffectNode in MyStatusEffects.Values) {
                //Debug.Log("CharacterStats.GetDamageModifiers(): looping through status effects");
                if (statusEffectNode.MyStatusEffect != null) {
                    if (statusEffectNode.MyStatusEffect.IncomingDamageMultiplier != 1) {
                        //Debug.Log("CharacterStats.GetDamageModifiers(): looping through status effects: ");
                        returnValue *= statusEffectNode.MyStatusEffect.MyCurrentStacks * statusEffectNode.MyStatusEffect.IncomingDamageMultiplier;
                    }
                } else {
                    Debug.Log(gameObject.name + "CharacterStats.GetIncomingDamageModifiers(): statusEffectNode.MyStatusEffect is null!!!");
                }
            }
            //Debug.Log("CharacterStats.GetDamageModifiers() returning: " + returnValue);
            return returnValue;
        }

        public virtual float GetSpeedModifiers() {
            //Debug.Log("CharacterStats.GetDamageModifiers()");
            float returnValue = 1f;
            foreach (StatusEffectNode statusEffectNode in MyStatusEffects.Values) {
                //Debug.Log("CharacterStats.GetDamageModifiers(): looping through status effects");
                if (statusEffectNode.MyStatusEffect.MySpeedMultiplier != 1) {
                    //Debug.Log("CharacterStats.GetDamageModifiers(): looping through status effects: ");
                    returnValue *= (float)statusEffectNode.MyStatusEffect.MyCurrentStacks * statusEffectNode.MyStatusEffect.MySpeedMultiplier;
                }
            }
            //Debug.Log("CharacterStats.GetDamageModifiers() returning: " + returnValue);
            return returnValue;
        }

        public virtual float GetAccuracyModifiers() {
            //Debug.Log("CharacterStats.GetDamageModifiers()");
            float returnValue = 1f;
            foreach (StatusEffectNode statusEffectNode in MyStatusEffects.Values) {
                //Debug.Log("CharacterStats.GetDamageModifiers(): looping through status effects");
                if (statusEffectNode.MyStatusEffect.MyAccuracyMultiplier != 1) {
                    //Debug.Log("CharacterStats.GetDamageModifiers(): looping through status effects: ");
                    returnValue *= (float)statusEffectNode.MyStatusEffect.MyCurrentStacks * statusEffectNode.MyStatusEffect.MyAccuracyMultiplier;
                }
            }
            //Debug.Log(gameObject.name + ".CharacterStats.GetAccuracyModifiers() returning: " + returnValue);
            return returnValue;
        }

        public virtual bool HasFreezeImmunity() {
            //Debug.Log("CharacterStats.GetDamageModifiers()");
            foreach (StatusEffectNode statusEffectNode in MyStatusEffects.Values) {
                if (statusEffectNode.MyStatusEffect.MyImmuneDisableAnimator == true) {
                    return true;
                }
            }
            return false;
        }

        public virtual bool HasStunImmunity() {
            //Debug.Log("CharacterStats.GetDamageModifiers()");
            foreach (StatusEffectNode statusEffectNode in MyStatusEffects.Values) {
                if (statusEffectNode.MyStatusEffect.MyImmuneStun == true) {
                    return true;
                }
            }
            return false;
        }

        public virtual bool HasLevitateImmunity() {
            //Debug.Log("CharacterStats.GetDamageModifiers()");
            foreach (StatusEffectNode statusEffectNode in MyStatusEffects.Values) {
                if (statusEffectNode.MyStatusEffect.MyImmuneLevitate == true) {
                    return true;
                }
            }
            return false;
        }


        public virtual float GetThreatModifiers() {
            //Debug.Log("CharacterStats.GetDamageModifiers()");
            float returnValue = 1f;
            foreach (StatusEffectNode statusEffectNode in MyStatusEffects.Values) {
                //Debug.Log("CharacterStats.GetDamageModifiers(): looping through status effects");
                if (statusEffectNode.MyStatusEffect.ThreatMultiplier != 1) {
                    //Debug.Log("CharacterStats.GetDamageModifiers(): looping through status effects: ");
                    returnValue *= (float)statusEffectNode.MyStatusEffect.MyCurrentStacks * statusEffectNode.MyStatusEffect.ThreatMultiplier;
                }
            }
            //Debug.Log("CharacterStats.GetDamageModifiers() returning: " + returnValue);
            return returnValue;
        }

        public virtual float GetOutGoingDamageModifiers() {
            //Debug.Log("CharacterStats.GetDamageModifiers()");
            float returnValue = 1f;
            foreach (StatusEffectNode statusEffectNode in MyStatusEffects.Values) {
                //Debug.Log("CharacterStats.GetDamageModifiers(): looping through status effects");
                if (statusEffectNode.MyStatusEffect.MyOutgoingDamageMultiplier != 1) {
                    //Debug.Log("CharacterStats.GetDamageModifiers(): looping through status effects: ");
                    returnValue *= (float)statusEffectNode.MyStatusEffect.MyCurrentStacks * statusEffectNode.MyStatusEffect.MyOutgoingDamageMultiplier;
                }
            }
            //Debug.Log("CharacterStats.GetDamageModifiers() returning: " + returnValue);
            return returnValue;
        }

        public virtual float GetCriticalStrikeModifiers() {
            //Debug.Log("CharacterStats.GetDamageModifiers()");
            float returnValue = 0f;
            foreach (StatusEffectNode statusEffectNode in MyStatusEffects.Values) {
                //Debug.Log("CharacterStats.GetDamageModifiers(): looping through status effects");
                if (statusEffectNode.MyStatusEffect.MyExtraCriticalStrikePercent != 1) {
                    //Debug.Log("CharacterStats.GetDamageModifiers(): looping through status effects: ");
                    returnValue += (float)statusEffectNode.MyStatusEffect.MyCurrentStacks * statusEffectNode.MyStatusEffect.MyExtraCriticalStrikePercent;
                }
            }
            //Debug.Log("CharacterStats.GetDamageModifiers() returning: " + returnValue);
            return returnValue;
        }

        /// <summary>
        /// attempt to pull target into combat
        /// </summary>
        /// <param name="sourceCharacter"></param>
        /// <param name="target"></param>
        public virtual void AttemptAgro(IAbilityCaster sourceCharacter, CharacterUnit target) {
            if (target != null && (sourceCharacter as CharacterAbilityManager) is CharacterAbilityManager) {
                CharacterUnit targetCharacterUnit = target.GetComponent<CharacterUnit>();
                if (targetCharacterUnit != null && targetCharacterUnit.MyBaseCharacter != null) {
                    if (Faction.RelationWith(targetCharacterUnit.MyBaseCharacter, (sourceCharacter as CharacterAbilityManager).BaseCharacter) <= -1) {
                        if (targetCharacterUnit.MyBaseCharacter.CharacterCombat != null) {
                            // agro includes a liveness check, so casting necromancy on a dead enemy unit should not pull it into combat with us if we haven't applied a faction or master control buff yet
                            targetCharacterUnit.MyBaseCharacter.CharacterController.Agro((sourceCharacter as CharacterAbilityManager).BaseCharacter.CharacterUnit);
                        }
                    }
                }
            }
        }

        public virtual bool WasImmuneToFreeze(StatusEffect statusEffect, IAbilityCaster sourceCharacter) {
            if (statusEffect.MyDisableAnimator == true && baseCharacter.CharacterStats.HasFreezeImmunity()) {
                if (sourceCharacter == (PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager as IAbilityCaster)) {
                    CombatTextManager.MyInstance.SpawnCombatText(baseCharacter.CharacterUnit.gameObject, 0, CombatTextType.immune, CombatMagnitude.normal);
                }
                return true;
            }
            return false;
        }

        public virtual bool WasImmuneToStun(StatusEffect statusEffect, IAbilityCaster sourceCharacter) {
            // check for stun
            if (statusEffect.MyStun == true && baseCharacter.CharacterStats.HasStunImmunity()) {
                if (sourceCharacter == (PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager as IAbilityCaster)) {
                    CombatTextManager.MyInstance.SpawnCombatText(baseCharacter.CharacterUnit.gameObject, 0, CombatTextType.immune, CombatMagnitude.normal);
                }
                return true;
            }
            return false;
        }

        public virtual bool WasImmuneToLevitate(StatusEffect statusEffect, IAbilityCaster sourceCharacter) {
            // check for levitate
            if (statusEffect.MyLevitate == true && baseCharacter.CharacterStats.HasLevitateImmunity()) {
                if (sourceCharacter == (PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager as IAbilityCaster)) {
                    CombatTextManager.MyInstance.SpawnCombatText(baseCharacter.CharacterUnit.gameObject, 0, CombatTextType.immune, CombatMagnitude.normal);
                }
                return true;
            }
            return false;
        }

        public virtual StatusEffectNode ApplyStatusEffect(StatusEffect statusEffect, IAbilityCaster sourceCharacter, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log(gameObject.name + ".CharacterStats.ApplyStatusEffect(" + statusEffect.MyAbilityEffectName + ", " + source.name + ", " + (target == null ? "null" : target.name) + ")");
            if (IsAlive == false && statusEffect.RequiresLiveTarget == true) {
                //Debug.Log("Cannot apply status effect to dead character. return null.");
                return null;
            }
            if (IsAlive == true && statusEffect.RequireDeadTarget == true) {
                //Debug.Log("Cannot apply status effect to dead character. return null.");
                return null;
            }
            if (statusEffect == null) {
                //Debug.Log("CharacterStats.ApplyAbilityEffect() abilityEffect is null");
            }
            if (sourceCharacter == null) {
                //Debug.Log("CharacterStats.ApplyAbilityEffect() source is null");
            }
            AttemptAgro(sourceCharacter, baseCharacter.CharacterUnit);

            // check for frozen
            if (WasImmuneToFreeze(statusEffect, sourceCharacter)) {
                return null;
            }

            // check for stun
            if (WasImmuneToStun(statusEffect, sourceCharacter)) {
                return null;
            }

            // check for levitate
            if (WasImmuneToLevitate(statusEffect, sourceCharacter)) {
                return null;
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

                // add to effect list since it was not in there
                StatusEffect _statusEffect = SystemAbilityEffectManager.MyInstance.GetNewResource(statusEffect.MyName) as StatusEffect;
                if (_statusEffect == null) {
                    Debug.LogError(gameObject.name + ".CharacterStats.ApplyStatusEffect(): Could not get status effect " + statusEffect.MyName);
                    return null;
                }
                StatusEffectNode newStatusEffectNode = new StatusEffectNode();

                statusEffects.Add(SystemResourceManager.prepareStringForMatch(_statusEffect.MyName), newStatusEffectNode);

                _statusEffect.Initialize(sourceCharacter, baseCharacter, abilityEffectInput);
                newStatusEffectNode.Setup(this, _statusEffect);
                Coroutine newCoroutine = StartCoroutine(Tick(sourceCharacter, abilityEffectInput, _statusEffect));
                newStatusEffectNode.MyMonitorCoroutine = newCoroutine;
                //newStatusEffectNode.Setup(this, _statusEffect, newCoroutine);

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
                PrimaryResourceAmountChangedNotificationHandler();
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
                CalculateRunSpeed();
                StatChangedNotificationHandler();
            }

            if (statusEffect.MyFactionModifiers.Count > 0) {
                //Debug.Log(gameObject.name + ".CharacterStats.HandleChangedNOtifications(" + statusEffect.MyName + "): NOTIFYING REPUTATION CHANGED");
                if (SystemEventManager.MyInstance != null) {
                    SystemEventManager.TriggerEvent("OnReputationChange", new EventParamProperties());
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

        public virtual void GainXP(int xp) {
            //Debug.Log(gameObject.name + ": GainXP(" + xp + ")");
            currentXP += xp;
            int overflowXP = 0;
            while (currentXP - LevelEquations.GetXPNeededForLevel(currentLevel) >= 0) {
                overflowXP = currentXP - LevelEquations.GetXPNeededForLevel(currentLevel);
                GainLevel();
                currentXP = overflowXP;
            }

            CombatTextManager.MyInstance.SpawnCombatText(baseCharacter.CharacterUnit.gameObject, xp, CombatTextType.gainXP, CombatMagnitude.normal);

            SystemEventManager.MyInstance.NotifyOnXPGained();
        }

        public virtual void GainLevel() {
            // make gain level sound and graphic
            SetLevel(currentLevel + 1);
            OnLevelChanged(currentLevel);
        }

        public virtual void SetLevel(int newLevel) {
            //Debug.Log(gameObject.name + ".CharacterStats.SetLevel(" + newLevel + ")");
            // arbitrary toughness cap of 5 for now.  add this as system configuration option later maybe
            //int usedToughNess = (int)Mathf.Clamp((int)toughness, 1, 5);
            float usedStaminaMultiplier = 1f;
            float usedIntellectMultiplier = 1f;
            float usedAgilityMultiplier = 1f;
            float usedStrengthMultiplier = 1f;
            currentLevel = newLevel;
            if (unitToughness != null) {
                usedStaminaMultiplier = unitToughness.MyStaminaMultiplier;
                usedIntellectMultiplier = unitToughness.MyIntellectMultiplier;
                usedAgilityMultiplier = unitToughness.MyAgilityMultiplier;
                usedStrengthMultiplier = unitToughness.MyStrengthMultiplier;
                healthMultiplier = unitToughness.MyHealthMultiplier;
                manaMultiplier = unitToughness.MyManaMultiplier;
            }

            //Debug.Log(gameObject.name + ".CharacterStats.SetLevel(" + newLevel + "): stamina before: " + stamina + "; currentlevel: " + currentLevel + "; usedstaminamultiplier: " + usedStaminaMultiplier);
            stamina = (int)(currentLevel * LevelEquations.GetStaminaForLevel(currentLevel, baseCharacter.MyCharacterClass) * usedStaminaMultiplier);
            //Debug.Log(gameObject.name + ".CharacterStats.SetLevel(" + newLevel + "): stamina after: " + stamina);
            intellect = (int)(currentLevel * LevelEquations.GetIntellectForLevel(currentLevel, baseCharacter.MyCharacterClass) * usedIntellectMultiplier);
            strength = (int)(currentLevel * LevelEquations.GetStrengthForLevel(currentLevel, baseCharacter.MyCharacterClass) * usedStrengthMultiplier);
            agility = (int)(currentLevel * LevelEquations.GetAgilityForLevel(currentLevel, baseCharacter.MyCharacterClass) * usedAgilityMultiplier);

            CalculatePrimaryStats();

            ResetHealth();
            ResetPrimaryResourceAmount();
        }

        public Vector3 GetTransFormPosition() {
            return transform.position;
        }

        public virtual void UsePowerResource(PowerResource powerResource, int usedResourceAmount) {
            usedResourceAmount = Mathf.Clamp(usedResourceAmount, 0, int.MaxValue);
            if (powerResourceDictionary.ContainsKey(powerResource)) {
                powerResourceDictionary[powerResource].currentValue -= usedResourceAmount;
                powerResourceDictionary[powerResource].currentValue = Mathf.Clamp(powerResourceDictionary[powerResource].currentValue, 0, int.MaxValue);
                OnPrimaryResourceAmountChanged(MaxPrimaryResource, CurrentPrimaryResource);
            }
        }

        public void SetResourceAmount(string resourceName, float newAmount) {
            //Debug.Log(gameObject.name + ".CharacterStats.SetResourceAmount(" + resourceName + ", " + newAmount + "): current " + CurrentPrimaryResource);
            newAmount = Mathf.Clamp(newAmount, 0, int.MaxValue);
            PowerResource tmpPowerResource = SystemPowerResourceManager.MyInstance.GetResource(resourceName);

            if (tmpPowerResource != null && powerResourceDictionary.ContainsKey(tmpPowerResource)) {
                powerResourceDictionary[tmpPowerResource].currentValue = newAmount;
                powerResourceDictionary[tmpPowerResource].currentValue = Mathf.Clamp(
                    powerResourceDictionary[tmpPowerResource].currentValue,
                    0,
                    (int)baseCharacter.MyCharacterClass.GetResourceMaximum(tmpPowerResource, this));
                OnPrimaryResourceAmountChanged(MaxPrimaryResource, CurrentPrimaryResource);
                //Debug.Log(gameObject.name + ".CharacterStats.SetResourceAmount(" + resourceName + ", " + newAmount + "): current " + CurrentPrimaryResource);
            }
        }

        public void AddResourceAmount(string resourceName, float newAmount) {
            //Debug.Log(gameObject.name + ".CharacterStats.SetResourceAmount(" + resourceName + ", " + newAmount + "): current " + CurrentPrimaryResource);
            newAmount = Mathf.Clamp(newAmount, 0, int.MaxValue);
            PowerResource tmpPowerResource = SystemPowerResourceManager.MyInstance.GetResource(resourceName);

            if (tmpPowerResource != null && powerResourceDictionary.ContainsKey(tmpPowerResource)) {
                powerResourceDictionary[tmpPowerResource].currentValue += newAmount;
                powerResourceDictionary[tmpPowerResource].currentValue = Mathf.Clamp(
                    powerResourceDictionary[tmpPowerResource].currentValue,
                    0,
                    (int)baseCharacter.MyCharacterClass.GetResourceMaximum(tmpPowerResource, this));
                OnPrimaryResourceAmountChanged(MaxPrimaryResource, CurrentPrimaryResource);
                //Debug.Log(gameObject.name + ".CharacterStats.SetResourceAmount(" + resourceName + ", " + newAmount + "): current " + CurrentPrimaryResource);
            }
        }

        public void SetPrimaryResourceAmount(int newAmount) {
            //Debug.Log(gameObject.name + ": setting mana: " + mana.ToString());
            newAmount = Mathf.Clamp(newAmount, 0, int.MaxValue);
            if (PrimaryResource != null && powerResourceDictionary.ContainsKey(PrimaryResource)) {
                powerResourceDictionary[PrimaryResource].currentValue += newAmount;
                powerResourceDictionary[PrimaryResource].currentValue = Mathf.Clamp(powerResourceDictionary[PrimaryResource].currentValue, 0, MaxPrimaryResource);
                OnPrimaryResourceAmountChanged(MaxPrimaryResource, CurrentPrimaryResource);
            }
        }

        public void RecoverPrimaryResource(int mana, IAbilityCaster source, bool showCombatText = true, CombatMagnitude combatMagnitude = CombatMagnitude.normal) {

            SetPrimaryResourceAmount(mana);
            if (showCombatText && (baseCharacter.CharacterUnit.gameObject == PlayerManager.MyInstance.MyPlayerUnitObject || source.UnitGameObject == PlayerManager.MyInstance.MyCharacter.CharacterUnit.gameObject)) {
                // spawn text over the player
                CombatTextManager.MyInstance.SpawnCombatText(baseCharacter.CharacterUnit.gameObject, mana, CombatTextType.gainMana, combatMagnitude);
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

        public void RecoverHealth(int health, IAbilityCaster source, bool showCombatText = true, CombatMagnitude combatMagnitude = CombatMagnitude.normal) {
            //Debug.Log(gameObject.name + ".CharacterStats.RecoverHealth(" + health + ", " + (source != null && source.MyDisplayName != null ? source.MyDisplayName : null) + ", " + showCombatText + ")");
            SetHealth(health);

            if (showCombatText && (baseCharacter.CharacterUnit.gameObject == PlayerManager.MyInstance.MyPlayerUnitObject || source.UnitGameObject == PlayerManager.MyInstance.MyCharacter.CharacterUnit.gameObject)) {
                // spawn text over the player
                CombatTextManager.MyInstance.SpawnCombatText(baseCharacter.CharacterUnit.gameObject, health, CombatTextType.gainHealth, combatMagnitude);
            }
        }

        public void HandleIntellectUpdate() {
            CalculateIntellect();
            PrimaryResourceAmountChangedNotificationHandler();
        }

        /// <summary>
        /// allows to cap the resource amount if the resource cap has been lowered due to stat debuff etc
        /// </summary>
        public void PrimaryResourceAmountChangedNotificationHandler() {

            if (PrimaryResource != null && powerResourceDictionary.ContainsKey(PrimaryResource)) {
                powerResourceDictionary[PrimaryResource].currentValue = Mathf.Clamp(powerResourceDictionary[PrimaryResource].currentValue, 0, MaxPrimaryResource);
                OnPrimaryResourceAmountChanged(MaxPrimaryResource, CurrentPrimaryResource);
            }
        }

        public void HandleStaminaUpdate() {
            CalculateStamina();
            HealthChangedNotificationHandler();
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
            //Debug.Log(gameObject.name + ".CharacterStats.ResetHealth() : broadcasting OnHealthChanged; maxhealth: " + MyMaxHealth);
            currentHealth = MyMaxHealth;

            // notify subscribers that our health has changed
            OnHealthChanged(MyMaxHealth, currentHealth);
        }

        /// <summary>
        /// Set mana to maximum
        /// </summary>
        public void ResetPrimaryResourceAmount() {
            if (PrimaryResource != null && powerResourceDictionary.ContainsKey(PrimaryResource)) {
                powerResourceDictionary[PrimaryResource].currentValue = MaxPrimaryResource;
                OnPrimaryResourceAmountChanged(MaxPrimaryResource, CurrentPrimaryResource);
            }
        }

        public virtual void TrySpawnDead() {
            //Debug.Log(gameObject.name + ".CharacterStats.TrySpawnDead()");
            if (baseCharacter != null && baseCharacter.MySpawnDead == true) {
                //Debug.Log(gameObject.name + ".CharacterStats.TrySpawnDead(): spawning with no health");
                isAlive = false;
                currentHealth = 0;

                // notify subscribers that our health has changed
                OnHealthChanged(MyMaxHealth, currentHealth);
            }
        }

        public virtual void Die() {
            //Debug.Log(gameObject.name + ".CharacterStats.Die()");
            if (isAlive) {
                isAlive = false;
                ClearStatusEffects(false);
                ClearPowerAmounts();
                BeforeDie(this);
                OnDie(this);
            }
        }

        public virtual void Revive() {
            //Debug.Log(MyBaseCharacter.MyCharacterName + "Triggering Revive Animation");
            if (isReviving) {
                //Debug.Log(MyBaseCharacter.MyCharacterName + " is already reviving.  Doing nothing");
                return;
            }
            if (baseCharacter != null && baseCharacter.AnimatedUnit != null && baseCharacter.AnimatedUnit.MyCharacterAnimator != null) {
                baseCharacter.AnimatedUnit.MyCharacterAnimator.EnableAnimator();
            }
            isReviving = true;
            //baseCharacter.MyCharacterUnit.DisableCollider();
            OnReviveBegin();
        }

        public virtual void ReviveComplete() {
            //Debug.Log(MyBaseCharacter.MyCharacterName + ".CharacterStats.ReviveComplete() Recieved Revive Complete Signal. Resetting Character Stats.");
            ReviveRaw();
            OnReviveComplete();
        }

        public virtual void ReviveRaw() {
            //Debug.Log(MyBaseCharacter.MyCharacterName + ".CharacterStats.ReviveRaw()");
            isReviving = false;
            baseCharacter.CharacterUnit.DisableCollider();
            baseCharacter.CharacterUnit.EnableCollider();
            isAlive = true;
            ClearInvalidStatusEffects();
            ResetHealth();
            //ResetPrimaryResourceAmount();
        }

        protected virtual void ClearStatusEffects(bool clearAll = true) {
            //Debug.Log(gameObject.name + ".CharacterStatus.ClearStatusEffects()");
            List<StatusEffectNode> statusEffectNodes = new List<StatusEffectNode>();
            foreach (StatusEffectNode statusEffectNode in statusEffects.Values) {
                if (clearAll == true || statusEffectNode.MyStatusEffect.MyClassTrait == false) {
                    statusEffectNodes.Add(statusEffectNode);
                }
            }
            foreach (StatusEffectNode statusEffectNode in statusEffectNodes) {
                statusEffectNode.CancelStatusEffect();
                statusEffects.Remove(SystemResourceManager.prepareStringForMatch(statusEffectNode.MyStatusEffect.MyName));
            }
            //statusEffects.Clear();
        }

        protected virtual void ClearInvalidStatusEffects() {
            //Debug.Log(gameObject.name + ".CharacterStatus.ClearInvalidStatusEffects()");
            //List<string> RemoveList = new List<string>();
            List<StatusEffectNode> statusEffectNodes = new List<StatusEffectNode>();
            foreach (StatusEffectNode statusEffectNode in MyStatusEffects.Values) {
                //Debug.Log(gameObject.name + ".CharacterStatus.ClearInvalidStatusEffects(): checking statusEffectNode: " + statusEffectNode.MyStatusEffect.MyName);
                if (statusEffectNode.MyStatusEffect.RequireDeadTarget == true) {
                    //Debug.Log(gameObject.name + ".CharacterStatus.ClearInvalidStatusEffects(): checking statusEffectNode: " + statusEffectNode.MyStatusEffect.MyName + " requires dead target");
                    statusEffectNodes.Add(statusEffectNode);
                }
            }
            foreach (StatusEffectNode statusEffectNode in statusEffectNodes) {
                statusEffectNode.CancelStatusEffect();
            }

        }

        public IEnumerator Tick(IAbilityCaster characterSource, AbilityEffectOutput abilityEffectInput, StatusEffect statusEffect) {
            //Debug.Log(gameObject.name + ".StatusEffect.Tick() start");
            float elapsedTime = 0f;

            statusEffect.ApplyControlEffects(baseCharacter);
            if (abilityEffectInput.overrideDuration != 0) {
                statusEffect.SetRemainingDuration(abilityEffectInput.overrideDuration);
            } else {
                statusEffect.SetRemainingDuration(statusEffect.MyDuration);
            }
            //Debug.Log("duration: " + duration);
            //nextTickTime = remainingDuration - tickRate;
            //DateTime nextTickTime2 = System.DateTime.Now + tickRate;
            //int milliseconds = (int)((statusEffect.MyTickRate - (int)statusEffect.MyTickRate) * 1000);
            //Debug.Log(abilityEffectName + ".StatusEffect.Tick() milliseconds: " + milliseconds);
            //TimeSpan tickRateTimeSpan = new TimeSpan(0, 0, 0, (statusEffect.MyTickRate == 0f ? (int)statusEffect.MyDuration + 1 : (int)statusEffect.MyTickRate), milliseconds);
            //Debug.Log(abilityEffectName + ".StatusEffect.Tick() tickRateTimeSpan: " + tickRateTimeSpan);
            if (statusEffect.MyCastZeroTick) {
                if (baseCharacter != null && baseCharacter.CharacterUnit != null && characterSource != null) {
                    statusEffect.CastTick(characterSource, baseCharacter.CharacterUnit.gameObject, abilityEffectInput);
                }
            }
            //Debug.Log(abilityEffectName + ".StatusEffect.Tick() nextTickTime: " + nextTickTime);

            while ((statusEffect.MyLimitedDuration == false || statusEffect.MyClassTrait == true || statusEffect.GetRemainingDuration() > 0f) && baseCharacter != null) {
                yield return null;
                //Debug.Log(gameObject.name + ".CharacterStats.Tick(): statusEffect: " + statusEffect.MyName + "; remaining: " + statusEffect.GetRemainingDuration());
                statusEffect.SetRemainingDuration(statusEffect.GetRemainingDuration() - Time.deltaTime);
                elapsedTime += Time.deltaTime;
                // check for tick first so we can do final tick;

                if (elapsedTime >= statusEffect.TickRate && statusEffect.TickRate != 0) {
                    //Debug.Log(MyName + ".StatusEffect.Tick() TickTime!");
                    if (baseCharacter != null && baseCharacter.CharacterUnit != null && characterSource != null) {
                        statusEffect.CastTick(characterSource, baseCharacter.CharacterUnit.gameObject, abilityEffectInput);
                        elapsedTime -= statusEffect.TickRate;
                    }
                }
                statusEffect.UpdateStatusNode();
            }
            //Debug.Log(gameObject.name + ".CharacterStats.Tick(): statusEffect: " + statusEffect.MyName + "; remaining: " + statusEffect.GetRemainingDuration());
            if (baseCharacter != null) {
                if (characterSource != null & baseCharacter.CharacterUnit != null) {
                    statusEffect.CastComplete(characterSource, baseCharacter.CharacterUnit.gameObject, abilityEffectInput);
                }
            }

            if (statusEffects.ContainsKey(SystemResourceManager.prepareStringForMatch(statusEffect.MyName))) {
                //Debug.Log(gameObject.name + ".CharacterStats.Tick(): statusEffect: " + statusEffect.MyName + "; cancelling ");
                statusEffects[SystemResourceManager.prepareStringForMatch(statusEffect.MyName)].CancelStatusEffect();
            }
        }

        public void SetupScriptableObjects() {
            //Debug.Log(gameObject.name + ".CharacterStats.SetupScriptableObjects(): looking for unit toughness");
            if (unitToughness == null && toughness != null && toughness != string.Empty) {
                UnitToughness tmpToughness = SystemUnitToughnessManager.MyInstance.GetResource(toughness);
                if (tmpToughness != null) {
                    //Debug.Log(gameObject.name + ".CharacterStats.SetupScriptableObjects(): looking for unit toughness: found unit toughness");
                    unitToughness = tmpToughness;
                } else {
                    Debug.LogError(gameObject.name + "; Unit Toughness: " + toughness + " not found while initializing character stats.  Check Inspector!");
                }
            }

        }

        public void Update() {
            PerformResourceRegen();
        }

        public float GetPowerResourceAmount(PowerResource powerResource) {
            if (powerResourceDictionary.ContainsKey(powerResource)) {
                return powerResourceDictionary[powerResource].currentValue;
            }
            return 0f;
        }

        public float GetPowerResourceMaxAmount(PowerResource powerResource) {
            if (powerResourceDictionary.ContainsKey(powerResource)) {
                return baseCharacter.MyCharacterClass.GetResourceMaximum(powerResource, this);
            }
            return 0f;
        }

        protected virtual void ClearPowerAmounts() {
            foreach (PowerResource powerResource in powerResourceDictionary.Keys) {
                SetResourceAmount(powerResource.MyName, 0f);
            }
        }

        protected virtual void PerformResourceRegen() {
            if (baseCharacter == null || baseCharacter.CharacterUnit == null || isAlive == false) {
                // if the character is not spawned, we should not be regenerating their resources.
                return;
            }
            foreach (PowerResource powerResource in powerResourceDictionary.Keys) {
                powerResourceDictionary[powerResource].elapsedTime += Time.deltaTime;
                //Debug.Log(gameObject.name + ".CharacterStats.PerformResourceRegen(): powerResource: " + powerResource.MyName + "; value: " + powerResourceDictionary[powerResource].currentValue + "; max: " + baseCharacter.MyCharacterClass.GetResourceMaximum(powerResource, this));

                if (powerResourceDictionary[powerResource].elapsedTime >= powerResource.TickRate) {
                    powerResourceDictionary[powerResource].elapsedTime -= powerResource.TickRate;
                    float usedRegenAmount = 0f;
                    if (baseCharacter != null && baseCharacter.CharacterCombat != null && baseCharacter.CharacterCombat.GetInCombat() == true) {
                        // perform combat regen
                        if (powerResource.CombatRegenIsPercent) {
                            usedRegenAmount = GetPowerResourceMaxAmount(powerResource) * (powerResource.CombatRegenPerTick / 100);
                        } else {
                            usedRegenAmount = powerResource.CombatRegenPerTick;
                        }
                    } else {
                        // perform out of combat regen
                        if (powerResource.RegenIsPercent) {
                            usedRegenAmount = GetPowerResourceMaxAmount(powerResource) * (powerResource.RegenPerTick / 100);
                        } else {
                            usedRegenAmount = powerResource.RegenPerTick;
                        }
                    }
                    powerResourceDictionary[powerResource].currentValue += usedRegenAmount;
                    powerResourceDictionary[powerResource].currentValue = Mathf.Clamp(
                        powerResourceDictionary[powerResource].currentValue,
                        0,
                        (int)baseCharacter.MyCharacterClass.GetResourceMaximum(powerResource, this));

                    // this is notifying on primary resource, but for now, we don't have multiples, so its ok
                    // this will need to be fixed when we add secondary resources
                    OnPrimaryResourceAmountChanged(MaxPrimaryResource, CurrentPrimaryResource);

                }
            }
        }
    }

    [System.Serializable]
    public class PowerResourceNode {

        /// <summary>
        /// elapsed time per tick
        /// </summary>
        public float elapsedTime = 0f;

        /// <summary>
        /// the value of the power resource
        /// </summary>
        public float currentValue = 0f;

    }

}