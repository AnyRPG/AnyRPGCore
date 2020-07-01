using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterStats : MonoBehaviour {

        public event System.Action<int, int> OnPrimaryResourceAmountChanged = delegate { };
        public event System.Action<PowerResource, int, int> OnResourceAmountChanged = delegate { };
        public event System.Action<CharacterStats> OnDie = delegate { };
        public event System.Action<CharacterStats> BeforeDie = delegate { };
        public event System.Action OnReviveBegin = delegate { };
        public event System.Action OnReviveComplete = delegate { };
        public event System.Action<StatusEffectNode> OnStatusEffectAdd = delegate { };
        public event System.Action OnStatChanged = delegate { };
        public event System.Action<int> OnLevelChanged = delegate { };

        private int level = 1;

        protected float sprintSpeedModifier = 1.5f;

        // keep track of current level
        private int currentLevel;

        // primary stat names for this character, and their values
        protected Dictionary<string, Stat> primaryStats = new Dictionary<string, Stat>();

        // secondary stats for this character, and their values
        protected Dictionary<SecondaryStatType, Stat> secondaryStats = new Dictionary<SecondaryStatType, Stat>();

        // power resources for this character, and their values
        protected Dictionary<PowerResource, PowerResourceNode> powerResourceDictionary = new Dictionary<PowerResource, PowerResourceNode>();

        // keep track of resource multipliers from unit toughness to directly multiply resource values without multiplying underlying stats
        private Dictionary<string, float> resourceMultipliers = new Dictionary<string, float>();

        protected float walkSpeed = 1f;
        protected float runSpeed = 7f;

        protected float currentRunSpeed = 0f;
        protected float currentSprintSpeed = 0f;

        protected Dictionary<string, StatusEffectNode> statusEffects = new Dictionary<string, StatusEffectNode>();
        protected BaseCharacter baseCharacter;

        private bool isReviving = false;
        private bool isAlive = true;
        private int currentXP = 0;

        private List<PowerResource> powerResourceList = new List<PowerResource>();

        protected bool eventSubscriptionsInitialized = false;

        public float WalkSpeed { get => walkSpeed; }
        public float RunSpeed { get => currentRunSpeed; }
        public float SprintSpeed { get => currentSprintSpeed; }
        //public float MyHitBox { get => hitBox; }
        public bool IsAlive { get => isAlive; }
        public BaseCharacter BaseCharacter { get => baseCharacter; set => baseCharacter = value; }

        public int Level { get => currentLevel; }
        public int CurrentXP { get => currentXP; set => currentXP = value; }

        public List<PowerResource> PowerResourceList {
            get {
                return powerResourceList;
            }
        }

        public void HandleLevelLoad(string eventName, EventParamProperties eventParamProperties) {

            // remove scene specific status effects that are not valid in this scene
            List<StatusEffectNode> removeNodes = new List<StatusEffectNode>();
            foreach (StatusEffectNode statusEffectNode in statusEffects.Values) {
                if (statusEffectNode.StatusEffect.SceneNames.Count > 0) {
                    bool sceneFound = false;
                    foreach (string sceneName in statusEffectNode.StatusEffect.SceneNames) {
                        if (SystemResourceManager.prepareStringForMatch(sceneName) == SystemResourceManager.prepareStringForMatch(LevelManager.MyInstance.GetActiveSceneNode().SceneName)) {
                            sceneFound = true;
                        }
                    }
                    if (!sceneFound) {
                        removeNodes.Add(statusEffectNode);
                    }
                }
            }
            foreach (StatusEffectNode statusEffectNode in removeNodes) {
                statusEffectNode.CancelStatusEffect();
            }
        }

        public void UpdatePowerResourceList() {

            // since this is just a list and contains no values, it is safe to overwrite
            powerResourceList = new List<PowerResource>();

            // add from system
            powerResourceList.AddRange(SystemConfigurationManager.MyInstance.PowerResourceList);

            if (baseCharacter == null || baseCharacter.StatProviders == null) {
                return;
            }

            foreach (IStatProvider statProvider in baseCharacter.StatProviders) {
                if (statProvider != null) {
                    foreach (PowerResource powerResource in statProvider.PowerResourceList) {
                        if (!powerResourceList.Contains(powerResource)) {
                            powerResourceList.Add(powerResource);
                        }
                    }
                }
            }

        }

        public PowerResource PrimaryResource {
            get {
                if (PowerResourceList.Count > 0) {
                    return PowerResourceList[0];
                }
                return null;
            }
        }

        public bool HasHealthResource {
            get {
                if (PowerResourceDictionary.Count > 0) {
                    foreach (PowerResource powerResource in powerResourceDictionary.Keys) {
                        if (powerResource.IsHealth == true) {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public bool HasPrimaryResource {
            get {
                if (PowerResourceDictionary.Count > 0) {
                    return true;
                }
                return false;
            }
        }

        public bool HasSecondaryResource {
            get {
                if (PowerResourceDictionary.Count > 1) {
                    return true;
                }
                return false;
            }
        }

        public int MaxPrimaryResource {
            get {
                if (PrimaryResource != null) {
                    if (PowerResourceList.Count > 0) {
                        return (int)GetPowerResourceMaxAmount(PowerResourceList[0]);
                    }
                }
                return 0;
            }
        }

        public int CurrentPrimaryResource {
            get {
                if (PrimaryResource != null) {
                    if (powerResourceDictionary.Count > 0) {
                        if (PowerResourceList.Count > 0) {
                            return (int)powerResourceDictionary[PowerResourceList[0]].currentValue;
                        }
                    }
                }
                return 0;
            }
        }

        public Dictionary<string, StatusEffectNode> StatusEffects { get => statusEffects; }
        public UnitToughness Toughness {
            get {
                if (baseCharacter != null) {
                    return baseCharacter.UnitToughness;
                }
                return null;
            }
        }
        public bool IsReviving { get => isReviving; set => isReviving = value; }
        public Dictionary<PowerResource, PowerResourceNode> PowerResourceDictionary { get => powerResourceDictionary; set => powerResourceDictionary = value; }
        public Dictionary<string, Stat> PrimaryStats { get => primaryStats; set => primaryStats = value; }
        public Dictionary<SecondaryStatType, Stat> SecondaryStats { get => secondaryStats; set => secondaryStats = value; }

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
            //temporarily disabled since unit profile will still be null at this point
            SetPrimaryStatModifiers();
            InitializeSecondaryStats();
            CreateEventSubscriptions();
            SetupScriptableObjects();
        }

        public void OrchestratorFinish() {
            CreateLateSubscriptions();
        }

        public virtual void CalculatePrimaryStats() {
            CalculateRunSpeed();
            foreach (string statName in primaryStats.Keys) {
                CalculateStat(statName);
            }
            CalculateSecondaryStats();
        }

        public virtual void CalculateSecondaryStats() {
            //Debug.Log(gameObject.name + ".CharacterStats.CalculateSecondaryStats()");

            // update base values
            foreach (SecondaryStatType secondaryStatType in secondaryStats.Keys) {
                secondaryStats[secondaryStatType].BaseValue = (int)LevelEquations.GetBaseSecondaryStatForCharacter(secondaryStatType, baseCharacter);
            }

            // calculate values that include base values plus modifiers
            foreach (SecondaryStatType secondaryStatType in secondaryStats.Keys) {
                CalculateSecondaryStat(secondaryStatType);
            }
        }

        public virtual void GetComponentReferences() {
            baseCharacter = GetComponent<BaseCharacter>();
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

        public void HandleUpdateStatProviders() {
            //Debug.Log(gameObject.name + ".CharacterStats.HandleSetUnitProfile()");
            SetPrimaryStatModifiers();
        }

        public void AddPrimaryStatModifiers(List<StatScalingNode> primaryStatList, bool updatePowerResourceDictionary = true) {
            if (primaryStatList != null) {
                foreach (StatScalingNode statScalingNode in primaryStatList) {
                    if (!primaryStats.ContainsKey(statScalingNode.StatName)) {
                        //Debug.Log(gameObject.name + ".CharacterStats.AddUnitProfileModifiers(): adding stat: " + statScalingNode.StatName);
                        primaryStats.Add(statScalingNode.StatName, new Stat(statScalingNode.StatName));
                        primaryStats[statScalingNode.StatName].OnModifierUpdate += HandleStatUpdateCommon;
                    }
                }
            }

            if (updatePowerResourceDictionary) {
                UpdatePowerResourceDictionary();
            }

        }

        /// <summary>
        /// add power resources which are needed.  remove power resources which are no longer available
        /// </summary>
        public void UpdatePowerResourceDictionary() {
            //Debug.Log(gameObject.name + ".CharacterStats.UpdatePowerResourceDictionary()");

            UpdatePowerResourceList();

            // remove power resources which are no longer available
            List<PowerResource> removeList = new List<PowerResource>();
            foreach (PowerResource powerResource in powerResourceDictionary.Keys) {
                if (!PowerResourceList.Contains(powerResource)) {
                    removeList.Add(powerResource);
                }
            }
            foreach (PowerResource powerResource in removeList) {
                powerResourceDictionary.Remove(powerResource);
            }

            // add power resources that need to be added
            foreach (PowerResource powerResource in PowerResourceList) {
                if (!powerResourceDictionary.ContainsKey(powerResource)) {
                    //Debug.Log(gameObject.name + ".CharacterStats.AddUnitProfileModifiers(): adding resource: " + powerResource.MyDisplayName);
                    powerResourceDictionary.Add(powerResource, new PowerResourceNode());
                }
            }

        }

        /// <summary>
        /// setup the dictionaries that keep track of the current values for stats and resources
        /// </summary>
        public void SetPrimaryStatModifiers() {
            //Debug.Log(gameObject.name + ".CharacterStats.SetPrimaryStatModifiers()");
            // setup the primary stats dictionary with system defined stats
            AddPrimaryStatModifiers(SystemConfigurationManager.MyInstance.PrimaryStats, false);

            if (baseCharacter != null && baseCharacter.StatProviders != null) {
                foreach (IStatProvider statProvider in baseCharacter.StatProviders) {
                    if (statProvider != null && statProvider.PrimaryStats != null) {
                        AddPrimaryStatModifiers(statProvider.PrimaryStats, false);
                    }
                }
            }

            UpdatePowerResourceDictionary();
        }

        public void InitializeSecondaryStats() { 
            // setup the secondary stats from the system defined enum
            foreach (SecondaryStatType secondaryStatType in Enum.GetValues(typeof(SecondaryStatType))) {
                if (!secondaryStats.ContainsKey(secondaryStatType)) {
                    secondaryStats.Add(secondaryStatType, new Stat(secondaryStatType.ToString()));
                }
            }

            // accuracy and speed are percentages so they need 100 as their base value
            secondaryStats[SecondaryStatType.Accuracy].DefaultAddValue = 100;
            secondaryStats[SecondaryStatType.Speed].DefaultAddValue = 100;

            // movement speed is a special case for now and needs to be passed onto thiry party controllers
            secondaryStats[SecondaryStatType.MovementSpeed].OnModifierUpdate += HandleMovementSpeedUpdate;
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
            SystemEventManager.StartListening("OnLevelLoad", HandleLevelLoad);
        }

        public virtual void CleanupEventSubscriptions() {
            if (baseCharacter != null && baseCharacter.CharacterEquipmentManager != null) {
                baseCharacter.CharacterEquipmentManager.OnEquipmentChanged -= OnEquipmentChanged;
            }
            SystemEventManager.StopListening("OnLevelLoad", HandleLevelLoad);
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

        public virtual void CalculateRunSpeed() {
            currentRunSpeed = (runSpeed + GetSecondaryAddModifiers(SecondaryStatType.MovementSpeed)) * GetSecondaryMultiplyModifiers(SecondaryStatType.MovementSpeed);
            currentSprintSpeed = currentRunSpeed * sprintSpeedModifier;
            //Debug.Log(gameObject.name + ".CharacterStats.CalculateRunSpeed(): runSpeed: " + runSpeed + "; current: " + currentRunSpeed);
        }

        public virtual void HandleMovementSpeedUpdate(string secondaryStatName) {
            CalculateRunSpeed();
        }

        public virtual void HandleStatUpdateCommon(string statName) {
            //Debug.Log(gameObject.name + ".CharacterStats.HandleStatUpdateCommon(" + statName + ")");

            // check if the stat that was just updated contributes to any resource in any way
            // if it does, throw out a resource changed notification handler

            if (baseCharacter != null && baseCharacter.StatProviders != null) {

                // make a list since each provider could contribute and we want to avoid multiple notifications for the same resource
                List<PowerResource> affectedResources = new List<PowerResource>();

                foreach (IStatProvider statProvider in baseCharacter.StatProviders) {
                    if (statProvider != null && statProvider.PrimaryStats != null) {
                        foreach (StatScalingNode statScalingNode in statProvider.PrimaryStats) {
                            foreach (CharacterStatToResourceNode characterStatToResourceNode in statScalingNode.PrimaryToResourceConversion) {
                                if (statScalingNode.StatName == statName) {
                                    if (!affectedResources.Contains(characterStatToResourceNode.PowerResource)) {
                                        affectedResources.Add(characterStatToResourceNode.PowerResource);
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (PowerResource powerResource in affectedResources) {
                    ResourceAmountChangedNotificationHandler(powerResource);
                }
            }
        }

        public virtual void HandleStatUpdate(string statName, bool calculateStat = true) {
            if (calculateStat == true) {
                CalculateStat(statName);

                // in the future, loop and only calculate secondary stats that this primary stat affects
                CalculateSecondaryStats();
            }
            HandleStatUpdateCommon(statName);
        }

        private void CalculateEquipmentChanged(Equipment newItem, Equipment oldItem, bool recalculate = true) {
            if (newItem != null) {

                foreach (ItemPrimaryStatNode itemPrimaryStatNode in newItem.PrimaryStats) {
                    if (primaryStats.ContainsKey(itemPrimaryStatNode.StatName)) {
                        primaryStats[itemPrimaryStatNode.StatName].AddModifier(newItem.GetPrimaryStatModifier(itemPrimaryStatNode.StatName, Level, baseCharacter));
                    }
                }

                // armor is special because it can come from a base value and from secondary stats
                // here we add the base value
                secondaryStats[SecondaryStatType.Armor].AddModifier(newItem.GetArmorModifier(Level));

                foreach (ItemSecondaryStatNode itemSecondaryStatNode in newItem.SecondaryStats) {
                    secondaryStats[itemSecondaryStatNode.SecondaryStat].AddModifier(newItem.GetSecondaryStatAddModifier(itemSecondaryStatNode.SecondaryStat, Level));
                    secondaryStats[itemSecondaryStatNode.SecondaryStat].AddMultiplyModifier(newItem.GetSecondaryStatMultiplyModifier(itemSecondaryStatNode.SecondaryStat));
                }

            }

            // theres a bug here ?
            // if you equip an item at one level, then remove it at another level, it will remove the higher level value for the stat, leaving you with
            // less stats than you should have
            // to fix this, we need to recalculate the modifiers for equipment, every time you level up

            if (oldItem != null) {
                secondaryStats[SecondaryStatType.Armor].RemoveModifier(oldItem.GetArmorModifier(Level));

                foreach (ItemSecondaryStatNode itemSecondaryStatNode in oldItem.SecondaryStats) {
                    secondaryStats[itemSecondaryStatNode.SecondaryStat].RemoveModifier(oldItem.GetSecondaryStatAddModifier(itemSecondaryStatNode.SecondaryStat, Level));
                    secondaryStats[itemSecondaryStatNode.SecondaryStat].RemoveMultiplyModifier(oldItem.GetSecondaryStatMultiplyModifier(itemSecondaryStatNode.SecondaryStat));
                }

                foreach (ItemPrimaryStatNode itemPrimaryStatNode in oldItem.PrimaryStats) {
                    primaryStats[itemPrimaryStatNode.StatName].RemoveModifier(oldItem.GetPrimaryStatModifier(itemPrimaryStatNode.StatName, Level, baseCharacter));
                }
            }

            if (recalculate == true) {
                CalculatePrimaryStats();

                foreach (PowerResource _powerResource in PowerResourceDictionary.Keys) {
                    ResourceAmountChangedNotificationHandler(_powerResource);
                }
            }
        }

        private void OnEquipmentChanged(Equipment newItem, Equipment oldItem) {
            //Debug.Log(gameObject.name + ".CharacterStats.OnEquipmentChanged(" + (newItem != null ? newItem.MyName : "null") + ", " + (oldItem != null ? oldItem.MyName : "null") + ")");

            CalculateEquipmentChanged(newItem, oldItem);

            
        }

        public virtual void CalculateStat(string statName) {
            if (primaryStats.ContainsKey(statName)) {
                primaryStats[statName].CurrentValue = (int)((primaryStats[statName].BaseValue + GetAddModifiers(statName)) * GetMultiplyModifiers(statName));
            }
        }

        public virtual void CalculateSecondaryStat(SecondaryStatType secondaryStatType) {
            //Debug.Log(gameObject.name + ".CharacterStats.CalculateSecondaryStat(" + secondaryStatType.ToString() + ")");

            if (secondaryStats.ContainsKey(secondaryStatType)) {
                secondaryStats[secondaryStatType].CurrentValue = (int)((secondaryStats[secondaryStatType].BaseValue + GetSecondaryAddModifiers(secondaryStatType)) * GetSecondaryMultiplyModifiers(secondaryStatType));
            }
        }

        public virtual float GetSecondaryAddModifiers(SecondaryStatType secondaryStatType) {
            //Debug.Log(gameObject.name + ".CharacterStats.GetAddModifiers(" + statBuffType.ToString() + ")");
            float returnValue = 0;
            foreach (StatusEffectNode statusEffectNode in StatusEffects.Values) {
                if (statusEffectNode.StatusEffect.SecondaryStatBuffsTypes.Contains(secondaryStatType)) {
                    returnValue += statusEffectNode.StatusEffect.MyCurrentStacks * statusEffectNode.StatusEffect.SecondaryStatAmount;
                }
            }
            if (secondaryStats.ContainsKey(secondaryStatType)) {
                returnValue += secondaryStats[secondaryStatType].GetAddValue();
            }
            return returnValue;
        }

        public virtual float GetSecondaryMultiplyModifiers(SecondaryStatType secondaryStatType) {
            //Debug.Log(gameObject.name + ".CharacterStats.GetMultiplyModifiers(" + secondaryStatType.ToString() + ")");
            float returnValue = 1f;
            foreach (StatusEffectNode statusEffectNode in StatusEffects.Values) {
                if (statusEffectNode.StatusEffect.SecondaryStatBuffsTypes.Contains(secondaryStatType)) {
                    returnValue *= (float)statusEffectNode.StatusEffect.MyCurrentStacks * statusEffectNode.StatusEffect.SecondaryStatMultiplier;
                    //Debug.Log(gameObject.name + ".CharacterStats.GetMultiplyModifiers(" + secondaryStatType.ToString() + "): return: " + returnValue + "; stack: " + statusEffectNode.MyStatusEffect.MyCurrentStacks + "; multiplier: " + statusEffectNode.MyStatusEffect.MyStatMultiplier);
                }
            }
            //Debug.Log(gameObject.name + ".CharacterStats.GetMultiplyModifiers(" + secondaryStatType.ToString() + "): return: " + returnValue);
            if (secondaryStats.ContainsKey(secondaryStatType)) {
                returnValue *= secondaryStats[secondaryStatType].GetMultiplyValue();
            }
            //Debug.Log(gameObject.name + ".CharacterStats.GetMultiplyModifiers(" + secondaryStatType.ToString() + "): return: " + returnValue);
            return returnValue;
        }

        /// <summary>
        /// Add together additive stat modifiers from status effects and gear
        /// </summary>
        /// <param name="statName"></param>
        /// <returns></returns>
        protected virtual float GetAddModifiers(string statName) {
            //Debug.Log(gameObject.name + ".CharacterStats.GetAddModifiers(" + statBuffType.ToString() + ")");
            float returnValue = 0;
            foreach (StatusEffectNode statusEffectNode in StatusEffects.Values) {
                if (statusEffectNode.StatusEffect.StatBuffTypeNames.Contains(statName)) {
                    returnValue += statusEffectNode.StatusEffect.MyCurrentStacks * statusEffectNode.StatusEffect.MyStatAmount;
                }
            }
            if (primaryStats.ContainsKey(statName)) {
                returnValue += primaryStats[statName].GetAddValue();
            }
            return returnValue;
        }

        /// <summary>
        /// Multiply together multiplicative stats modifers from status effects and gear
        /// </summary>
        /// <param name="statName"></param>
        /// <returns></returns>
        protected virtual float GetMultiplyModifiers(string statName) {
            //Debug.Log(gameObject.name + ".CharacterStats.GetMultiplyModifiers(" + statBuffType.ToString() + ")");
            float returnValue = 1f;
            foreach (StatusEffectNode statusEffectNode in StatusEffects.Values) {
                if (statusEffectNode.StatusEffect.StatBuffTypeNames.Contains(statName)) {
                    returnValue *= statusEffectNode.StatusEffect.MyCurrentStacks * statusEffectNode.StatusEffect.MyStatMultiplier;
                }
            }
            if (primaryStats.ContainsKey(statName)) {
                returnValue *= primaryStats[statName].GetMultiplyValue();
            }

            return returnValue;
        }

        public virtual float GetIncomingDamageModifiers() {
            //Debug.Log("CharacterStats.GetDamageModifiers()");
            float returnValue = 1f;
            foreach (StatusEffectNode statusEffectNode in StatusEffects.Values) {
                //Debug.Log("CharacterStats.GetDamageModifiers(): looping through status effects");
                if (statusEffectNode.StatusEffect != null) {
                    if (statusEffectNode.StatusEffect.IncomingDamageMultiplier != 1) {
                        //Debug.Log("CharacterStats.GetDamageModifiers(): looping through status effects: ");
                        returnValue *= statusEffectNode.StatusEffect.MyCurrentStacks * statusEffectNode.StatusEffect.IncomingDamageMultiplier;
                    }
                } else {
                    Debug.Log(gameObject.name + "CharacterStats.GetIncomingDamageModifiers(): statusEffectNode.MyStatusEffect is null!!!");
                }
            }
            //Debug.Log("CharacterStats.GetDamageModifiers() returning: " + returnValue);
            return returnValue;
        }

        public virtual float GetSpeedModifiers() {
            //Debug.Log("CharacterStats.GetSpeedModifiers(): ");
            return secondaryStats[SecondaryStatType.Speed].CurrentValue;
        }

        public virtual float GetAccuracyModifiers() {
            //Debug.Log("CharacterStats.GetAccuracyModifiers()");
            return secondaryStats[SecondaryStatType.Accuracy].CurrentValue;
        }

        public virtual bool HasFreezeImmunity() {
            //Debug.Log("CharacterStats.GetDamageModifiers()");
            foreach (StatusEffectNode statusEffectNode in StatusEffects.Values) {
                if (statusEffectNode.StatusEffect.MyImmuneDisableAnimator == true) {
                    return true;
                }
            }
            return false;
        }

        public virtual bool HasStunImmunity() {
            //Debug.Log("CharacterStats.GetDamageModifiers()");
            foreach (StatusEffectNode statusEffectNode in StatusEffects.Values) {
                if (statusEffectNode.StatusEffect.MyImmuneStun == true) {
                    return true;
                }
            }
            return false;
        }

        public virtual bool HasLevitateImmunity() {
            //Debug.Log("CharacterStats.GetDamageModifiers()");
            foreach (StatusEffectNode statusEffectNode in StatusEffects.Values) {
                if (statusEffectNode.StatusEffect.MyImmuneLevitate == true) {
                    return true;
                }
            }
            return false;
        }


        public virtual float GetThreatModifiers() {
            //Debug.Log("CharacterStats.GetDamageModifiers()");
            float returnValue = 1f;
            foreach (StatusEffectNode statusEffectNode in StatusEffects.Values) {
                //Debug.Log("CharacterStats.GetDamageModifiers(): looping through status effects");
                if (statusEffectNode.StatusEffect.ThreatMultiplier != 1) {
                    //Debug.Log("CharacterStats.GetDamageModifiers(): looping through status effects: ");
                    returnValue *= (float)statusEffectNode.StatusEffect.MyCurrentStacks * statusEffectNode.StatusEffect.ThreatMultiplier;
                }
            }
            //Debug.Log("CharacterStats.GetDamageModifiers() returning: " + returnValue);
            return returnValue;
        }

        public virtual float GetOutGoingDamageModifiers() {
            //Debug.Log("CharacterStats.GetDamageModifiers()");
            float returnValue = 1f;
            foreach (StatusEffectNode statusEffectNode in StatusEffects.Values) {
                //Debug.Log("CharacterStats.GetDamageModifiers(): looping through status effects");
                if (statusEffectNode.StatusEffect.MyOutgoingDamageMultiplier != 1) {
                    //Debug.Log("CharacterStats.GetDamageModifiers(): looping through status effects: ");
                    returnValue *= (float)statusEffectNode.StatusEffect.MyCurrentStacks * statusEffectNode.StatusEffect.MyOutgoingDamageMultiplier;
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
                if (targetCharacterUnit != null && targetCharacterUnit.BaseCharacter != null) {
                    if (Faction.RelationWith(targetCharacterUnit.BaseCharacter, (sourceCharacter as CharacterAbilityManager).BaseCharacter) <= -1) {
                        if (targetCharacterUnit.BaseCharacter.CharacterCombat != null) {
                            // agro includes a liveness check, so casting necromancy on a dead enemy unit should not pull it into combat with us if we haven't applied a faction or master control buff yet
                            targetCharacterUnit.BaseCharacter.CharacterController.Agro((sourceCharacter as CharacterAbilityManager).BaseCharacter.CharacterUnit);
                        }
                    }
                }
            }
        }

        public virtual bool WasImmuneToDamageType(PowerResource powerResource, IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectContext) {
            if (!powerResourceDictionary.ContainsKey(powerResource)) {
                if (sourceCharacter == (PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager as IAbilityCaster)) {
                    CombatTextManager.MyInstance.SpawnCombatText(baseCharacter.CharacterUnit.gameObject, 0, CombatTextType.immune, CombatMagnitude.normal, abilityEffectContext);
                }
                return true;
            }
            return false;
        }

        public virtual bool WasImmuneToFreeze(StatusEffect statusEffect, IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectContext) {
            if (statusEffect.MyDisableAnimator == true && baseCharacter.CharacterStats.HasFreezeImmunity()) {
                if (sourceCharacter == (PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager as IAbilityCaster)) {
                    CombatTextManager.MyInstance.SpawnCombatText(baseCharacter.CharacterUnit.gameObject, 0, CombatTextType.immune, CombatMagnitude.normal, abilityEffectContext);
                }
                return true;
            }
            return false;
        }

        public virtual bool WasImmuneToStun(StatusEffect statusEffect, IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectContext) {
            // check for stun
            if (statusEffect.MyStun == true && baseCharacter.CharacterStats.HasStunImmunity()) {
                if (sourceCharacter == (PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager as IAbilityCaster)) {
                    CombatTextManager.MyInstance.SpawnCombatText(baseCharacter.CharacterUnit.gameObject, 0, CombatTextType.immune, CombatMagnitude.normal, abilityEffectContext);
                }
                return true;
            }
            return false;
        }

        public virtual bool WasImmuneToLevitate(StatusEffect statusEffect, IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectContext) {
            // check for levitate
            if (statusEffect.MyLevitate == true && baseCharacter.CharacterStats.HasLevitateImmunity()) {
                if (sourceCharacter == (PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager as IAbilityCaster)) {
                    CombatTextManager.MyInstance.SpawnCombatText(baseCharacter.CharacterUnit.gameObject, 0, CombatTextType.immune, CombatMagnitude.normal, abilityEffectContext);
                }
                return true;
            }
            return false;
        }

        public virtual StatusEffectNode ApplyStatusEffect(StatusEffect statusEffect, IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(gameObject.name + ".CharacterStats.ApplyStatusEffect(" + statusEffect.MyDisplayName + ", " + sourceCharacter.Name + ")");
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
            if (WasImmuneToFreeze(statusEffect, sourceCharacter, abilityEffectInput)) {
                return null;
            }

            // check for stun
            if (WasImmuneToStun(statusEffect, sourceCharacter, abilityEffectInput)) {
                return null;
            }

            // check for levitate
            if (WasImmuneToLevitate(statusEffect, sourceCharacter, abilityEffectInput)) {
                return null;
            }
            //Debug.Log("CharacterStats.ApplyStatusEffect(" + statusEffect.ToString() + ", " + source.name + ", " + target.name + ")");
            //Debug.Log("statuseffects count: " + statusEffects.Count);

            StatusEffect comparedStatusEffect = null;
            string peparedString = SystemResourceManager.prepareStringForMatch(statusEffect.MyDisplayName);
            if (statusEffects.ContainsKey(peparedString)) {
                comparedStatusEffect = statusEffects[peparedString].StatusEffect;
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
                StatusEffect _statusEffect = SystemAbilityEffectManager.MyInstance.GetNewResource(statusEffect.MyDisplayName) as StatusEffect;
                if (_statusEffect == null) {
                    Debug.LogError(gameObject.name + ".CharacterStats.ApplyStatusEffect(): Could not get status effect " + statusEffect.MyDisplayName);
                    return null;
                }
                StatusEffectNode newStatusEffectNode = new StatusEffectNode();

                statusEffects.Add(SystemResourceManager.prepareStringForMatch(_statusEffect.MyDisplayName), newStatusEffectNode);

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
            //Debug.Log(gameObject.name + ".CharacterStats.HandleChangedNotifications(" + statusEffectNode.StatusEffect.MyName + "): NOTIFYING STATUS EFFECT UPDATE");
            OnStatusEffectAdd(statusEffectNode);
            HandleChangedNotifications(statusEffectNode.StatusEffect);
        }

        public void HandleChangedNotifications(StatusEffect statusEffect) {
            //Debug.Log(gameObject.name + ".CharacterStats.HandleChangedNotifications(" + (statusEffect == null ? "null" : statusEffect.MyName) + ")");

            //statusEffect.StatBuffTypeNames
            if (statusEffect.StatBuffTypeNames.Count > 0) {
                foreach (string statName in statusEffect.StatBuffTypeNames) {
                    //HandleStatUpdate(statName, false);
                    HandleStatUpdate(statName, true);
                }
                StatChangedNotificationHandler();
            }

            if (statusEffect.SecondaryStatBuffsTypes.Contains(SecondaryStatType.MovementSpeed)) {
                CalculateRunSpeed();
            }
            if (statusEffect.SecondaryStatBuffsTypes.Count > 0) {
                CalculateSecondaryStats();
                StatChangedNotificationHandler();
            }

            if (statusEffect.MyFactionModifiers.Count > 0) {
                if (SystemEventManager.MyInstance != null) {
                    SystemEventManager.TriggerEvent("OnReputationChange", new EventParamProperties());
                }
            }
        }

        public void HandleStatusEffectRemoval(StatusEffect statusEffect) {
            //Debug.Log("CharacterStats.HandleStatusEffectRemoval(" + statusEffect.name + ")");
            string preparedString = SystemResourceManager.prepareStringForMatch(statusEffect.MyDisplayName);
            if (statusEffects.ContainsKey(preparedString)) {
                if (statusEffects[preparedString].MyMonitorCoroutine != null) {
                    StopCoroutine(statusEffects[preparedString].MyMonitorCoroutine);
                }
                statusEffects.Remove(preparedString);
            }

            // should reset resources back down after buff expires
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

            CombatTextManager.MyInstance.SpawnCombatText(baseCharacter.CharacterUnit.gameObject, xp, CombatTextType.gainXP, CombatMagnitude.normal, null);

            SystemEventManager.MyInstance.NotifyOnXPGained();
        }

        public virtual void GainLevel() {
            // make gain level sound and graphic
            SetLevel(currentLevel + 1);
            OnLevelChanged(currentLevel);
        }

        public virtual void SetLevel(int newLevel) {
            //Debug.Log(gameObject.name + ".CharacterStats.SetLevel(" + newLevel + ")");

            Dictionary<string, float> multiplierValues = new Dictionary<string, float>();
            foreach (string statName in primaryStats.Keys) {
                multiplierValues.Add(statName, 1f);
            }

            currentLevel = newLevel;
            if (baseCharacter != null && baseCharacter.UnitToughness != null) {
                foreach (primaryStatMultiplierNode primaryStatMultiplierNode in baseCharacter.UnitToughness.PrimaryStatMultipliers) {
                    multiplierValues[primaryStatMultiplierNode.StatName] = primaryStatMultiplierNode.StatMultiplier;
                }
                resourceMultipliers = new Dictionary<string, float>();
                foreach (primaryStatMultiplierNode primaryStatMultiplierNode in baseCharacter.UnitToughness.PrimaryStatMultipliers) {
                    resourceMultipliers.Add(primaryStatMultiplierNode.StatName, primaryStatMultiplierNode.StatMultiplier);
                }
            }

            // calculate base values independent of any modifiers
            foreach (string statName in primaryStats.Keys) {
                primaryStats[statName].BaseValue = (int)(currentLevel * LevelEquations.GetPrimaryStatForLevel(statName, currentLevel, baseCharacter) * multiplierValues[statName]);
            }

            // reset any amounts from equipment to deal with item level scaling before performing the calculations that include those equipment stat values
            CalculateEquipmentStats();

            // calculate current values that include modifiers
            CalculatePrimaryStats();

            ResetResourceAmounts();
        }

        public void CalculateEquipmentStats() {

            // reset all primary stat equipment modifiers
            foreach (Stat stat in primaryStats.Values) {
                stat.ClearAddModifiers();
                stat.ClearMultiplyModifiers();
            }

            // reset secondary stats
            foreach (Stat stat in secondaryStats.Values) {
                stat.ClearAddModifiers();
                stat.ClearMultiplyModifiers();
            }

            if (baseCharacter.CharacterEquipmentManager != null) {
                foreach (Equipment equipment in baseCharacter.CharacterEquipmentManager.CurrentEquipment.Values) {
                    CalculateEquipmentChanged(equipment, null, false);
                }
            }
        }


        public Vector3 GetTransFormPosition() {
            return transform.position;
        }


        public virtual void ReducePowerResource(PowerResource powerResource, int usedResourceAmount) {
            UsePowerResource(powerResource, usedResourceAmount);
        }

        public void PerformDeathCheck() {
            bool shouldLive = false;
            foreach (PowerResource powerResource in powerResourceDictionary.Keys) {
                if (powerResourceDictionary[powerResource].currentValue > 0f && powerResource.IsHealth == true) {
                    shouldLive = true;
                }
            }
            if (shouldLive == false) {
                Die();
            }
        }

        public virtual void UsePowerResource(PowerResource powerResource, int usedResourceAmount) {
            usedResourceAmount = Mathf.Clamp(usedResourceAmount, 0, int.MaxValue);
            if (powerResourceDictionary.ContainsKey(powerResource)) {
                powerResourceDictionary[powerResource].currentValue -= usedResourceAmount;
                powerResourceDictionary[powerResource].currentValue = Mathf.Clamp(powerResourceDictionary[powerResource].currentValue, 0, int.MaxValue);
                OnResourceAmountChanged(powerResource, (int)GetPowerResourceMaxAmount(powerResource), (int)powerResourceDictionary[powerResource].currentValue);
            }
            if (powerResource.IsHealth == true) {
                PerformDeathCheck();
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
                    (int)GetPowerResourceMaxAmount(tmpPowerResource));
                OnResourceAmountChanged(tmpPowerResource, (int)GetPowerResourceMaxAmount(tmpPowerResource), (int)powerResourceDictionary[tmpPowerResource].currentValue);
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
                    (int)GetPowerResourceMaxAmount(tmpPowerResource));
                OnResourceAmountChanged(tmpPowerResource, (int)GetPowerResourceMaxAmount(tmpPowerResource), (int)powerResourceDictionary[tmpPowerResource].currentValue);
                //Debug.Log(gameObject.name + ".CharacterStats.SetResourceAmount(" + resourceName + ", " + newAmount + "): current " + CurrentPrimaryResource);
            }
        }

        public virtual void RecoverResource(AbilityEffectContext abilityEffectContext, PowerResource powerResource, int amount, IAbilityCaster source, bool showCombatText = true, CombatMagnitude combatMagnitude = CombatMagnitude.normal) {

            AddResourceAmount(powerResource.MyDisplayName, amount);
            if (showCombatText && (baseCharacter.CharacterUnit.gameObject == PlayerManager.MyInstance.MyPlayerUnitObject || source.UnitGameObject == PlayerManager.MyInstance.MyCharacter.CharacterUnit.gameObject)) {
                // spawn text over the player
                CombatTextManager.MyInstance.SpawnCombatText(baseCharacter.CharacterUnit.gameObject, amount, CombatTextType.gainResource, combatMagnitude, abilityEffectContext);
            }
        }

        /// <summary>
        /// allows to cap the resource amount if the resource cap has been lowered due to stat debuff etc
        /// </summary>
        public void ResourceAmountChangedNotificationHandler(PowerResource powerResource) {
            //Debug.Log(gameObject.name + ".CharacterStats.ResourceAmountChangedNotificationHandler()");

            if (powerResource != null && powerResourceDictionary.ContainsKey(powerResource)) {
                powerResourceDictionary[powerResource].currentValue = Mathf.Clamp(powerResourceDictionary[powerResource].currentValue, 0, GetPowerResourceMaxAmount(powerResource));
                OnResourceAmountChanged(powerResource, (int)GetPowerResourceMaxAmount(powerResource), (int)powerResourceDictionary[powerResource].currentValue);
            }
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

        public void StatChangedNotificationHandler() {
            OnStatChanged();
        }

        /// <summary>
        /// Set resources to maximum
        /// </summary>
        public void ResetResourceAmounts() {
            //Debug.Log(gameObject.name + ".CharacterStats.ResetResourceAmounts()");

            if (PowerResourceList == null) {
                return;
            }

            // loop through and update the resources.
            foreach (PowerResource _powerResource in PowerResourceList) {
                if (_powerResource != null && powerResourceDictionary.ContainsKey(_powerResource)) {
                    if (_powerResource.FillOnReset == true) {
                        powerResourceDictionary[_powerResource].currentValue = GetPowerResourceMaxAmount(_powerResource);
                    }
                }
                OnResourceAmountChanged(_powerResource, (int)GetPowerResourceMaxAmount(_powerResource), (int)PowerResourceDictionary[_powerResource].currentValue);
            }

            
        }

        public virtual void TrySpawnDead() {
            //Debug.Log(gameObject.name + ".CharacterStats.TrySpawnDead()");
            if (baseCharacter != null && baseCharacter.MySpawnDead == true) {
                //Debug.Log(gameObject.name + ".CharacterStats.TrySpawnDead(): spawning with no health");
                isAlive = false;

                SetResourceAmount(PrimaryResource.MyDisplayName, 0f);

                // notify subscribers that our health has changed
                OnResourceAmountChanged(PrimaryResource, MaxPrimaryResource, CurrentPrimaryResource);
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

            ResetResourceAmounts();
        }

        protected virtual void ClearStatusEffects(bool clearAll = true) {
            //Debug.Log(gameObject.name + ".CharacterStatus.ClearStatusEffects()");
            List<StatusEffectNode> statusEffectNodes = new List<StatusEffectNode>();
            foreach (StatusEffectNode statusEffectNode in statusEffects.Values) {
                if (clearAll == true || statusEffectNode.StatusEffect.MyClassTrait == false) {
                    statusEffectNodes.Add(statusEffectNode);
                }
            }
            foreach (StatusEffectNode statusEffectNode in statusEffectNodes) {
                statusEffectNode.CancelStatusEffect();
                statusEffects.Remove(SystemResourceManager.prepareStringForMatch(statusEffectNode.StatusEffect.MyDisplayName));
            }
            //statusEffects.Clear();
        }

        protected virtual void ClearInvalidStatusEffects() {
            //Debug.Log(gameObject.name + ".CharacterStatus.ClearInvalidStatusEffects()");
            //List<string> RemoveList = new List<string>();
            List<StatusEffectNode> statusEffectNodes = new List<StatusEffectNode>();
            foreach (StatusEffectNode statusEffectNode in StatusEffects.Values) {
                //Debug.Log(gameObject.name + ".CharacterStatus.ClearInvalidStatusEffects(): checking statusEffectNode: " + statusEffectNode.MyStatusEffect.MyName);
                if (statusEffectNode.StatusEffect.RequireDeadTarget == true) {
                    //Debug.Log(gameObject.name + ".CharacterStatus.ClearInvalidStatusEffects(): checking statusEffectNode: " + statusEffectNode.MyStatusEffect.MyName + " requires dead target");
                    statusEffectNodes.Add(statusEffectNode);
                }
            }
            foreach (StatusEffectNode statusEffectNode in statusEffectNodes) {
                statusEffectNode.CancelStatusEffect();
            }

        }

        public IEnumerator Tick(IAbilityCaster characterSource, AbilityEffectContext abilityEffectInput, StatusEffect statusEffect) {
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

            if (statusEffects.ContainsKey(SystemResourceManager.prepareStringForMatch(statusEffect.MyDisplayName))) {
                //Debug.Log(gameObject.name + ".CharacterStats.Tick(): statusEffect: " + statusEffect.MyName + "; cancelling ");
                statusEffects[SystemResourceManager.prepareStringForMatch(statusEffect.MyDisplayName)].CancelStatusEffect();
            }
        }

        public void SetupScriptableObjects() {
            //Debug.Log(gameObject.name + ".CharacterStats.SetupScriptableObjects(): looking for unit toughness");
            

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

        /// <summary>
        /// Return the maximum value for a power resource
        /// </summary>
        /// <param name="powerResource"></param>
        /// <param name="characterStats"></param>
        /// <returns></returns>
        public float GetResourceMaximum(PowerResource powerResource, IStatProvider statProvider) {

            float returnValue = 0f;

            foreach (StatScalingNode statScalingNode in statProvider.PrimaryStats) {
                if (PrimaryStats.ContainsKey(statScalingNode.StatName)) {
                    foreach (CharacterStatToResourceNode characterStatToResourceNode in statScalingNode.PrimaryToResourceConversion) {
                        if (characterStatToResourceNode.PowerResource == powerResource) {
                            returnValue += (characterStatToResourceNode.ResourcePerPoint * PrimaryStats[statScalingNode.StatName].CurrentValue);
                        }
                    }
                }
            }
            return returnValue;
        }

        public float GetPowerResourceMaxAmount(PowerResource powerResource) {
            float returnValue = 0f;
            if (powerResourceDictionary.ContainsKey(powerResource)) {
                if (baseCharacter != null) {
                    returnValue += powerResource.MaximumAmount;
                    foreach (IStatProvider statProvider in baseCharacter.StatProviders) {
                        if (statProvider != null) {
                            returnValue += GetResourceMaximum(powerResource, statProvider);
                        }
                    }
                }
            }
            if (resourceMultipliers.ContainsKey(powerResource.MyDisplayName)) {
                returnValue *= resourceMultipliers[powerResource.MyDisplayName];
            }
            return returnValue;
        }

        protected virtual void ClearPowerAmounts() {
            foreach (PowerResource powerResource in powerResourceDictionary.Keys) {
                SetResourceAmount(powerResource.MyDisplayName, 0f);
            }
        }

        protected virtual void PerformResourceRegen() {
            if (baseCharacter == null || baseCharacter.CharacterUnit == null || isAlive == false) {
                // if the character is not spawned, we should not be regenerating their resources.
                return;
            }
            foreach (PowerResource powerResource in powerResourceDictionary.Keys) {
                powerResourceDictionary[powerResource].elapsedTime += Time.deltaTime;

                if (powerResourceDictionary[powerResource].elapsedTime >= powerResource.TickRate) {
                    powerResourceDictionary[powerResource].elapsedTime -= powerResource.TickRate;
                    if ((powerResource.RegenPerTick != 0f || powerResource.CombatRegenPerTick != 0f) && powerResourceDictionary[powerResource].currentValue < GetPowerResourceMaxAmount(powerResource)) {
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
                            (int)GetPowerResourceMaxAmount(powerResource));

                        // this is notifying on primary resource, but for now, we don't have multiples, so its ok
                        // this will need to be fixed when we add secondary resources
                        if (usedRegenAmount > 0) {
                            OnResourceAmountChanged(powerResource, (int)GetPowerResourceMaxAmount(powerResource), (int)powerResourceDictionary[powerResource].currentValue);
                        }
                    }
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