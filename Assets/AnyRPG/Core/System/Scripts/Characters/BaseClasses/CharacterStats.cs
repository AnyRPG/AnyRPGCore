using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterStats : ConfiguredClass {

        public event System.Action<int, int> OnPrimaryResourceAmountChanged = delegate { };
        public event System.Action<PowerResource, int, int> OnResourceAmountChanged = delegate { };
        public event System.Action<CharacterStats> OnDie = delegate { };
        public event System.Action<CharacterStats> BeforeDie = delegate { };
        public event System.Action OnReviveBegin = delegate { };
        public event System.Action OnReviveComplete = delegate { };
        public event System.Action<StatusEffectNode> OnStatusEffectAdd = delegate { };
        public event System.Action OnStatChanged = delegate { };
        public event System.Action OnEnterStealth = delegate { };
        public event System.Action OnLeaveStealth = delegate { };
        public event System.Action<int> OnLevelChanged = delegate { };
        public event System.Action<AbilityEffectContext> OnImmuneToEffect = delegate { };
        public event System.Action<int> OnGainXP = delegate { };
        public event System.Action<PowerResource, int> OnRecoverResource = delegate { };
        public event System.Action<float, float, float, float> OnCalculateRunSpeed = delegate { };

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
        protected float swimSpeed = 2f;
        protected float flySpeed = 20f;
        protected float glideSpeed = 5f;
        protected float glideFallSpeed = 2f;

        protected float currentRunSpeed = 0f;
        protected float currentSprintSpeed = 0f;
        protected float currentSwimSpeed = 2f;
        protected float currentFlySpeed = 20f;
        protected float currentGlideSpeed = 5f;
        protected float currentGlideFallSpeed = 2f;

        protected Dictionary<string, StatusEffectNode> statusEffects = new Dictionary<string, StatusEffectNode>();
        protected BaseCharacter baseCharacter = null;

        private bool isReviving = false;
        private bool isAlive = true;
        private int currentXP = 0;

        private List<PowerResource> powerResourceList = new List<PowerResource>();

        protected bool eventSubscriptionsInitialized = false;

        // game manager references
        protected LevelManager levelManager = null;
        protected PlayerManager playerManager = null;
        protected CombatTextManager combatTextManager = null;

        public float WalkSpeed { get => walkSpeed; }
        public float RunSpeed { get => currentRunSpeed; }
        public float SprintSpeed { get => currentSprintSpeed; }
        public float SwimSpeed { get => currentSwimSpeed; }
        public float FlySpeed { get => currentFlySpeed; }
        public float GlideSpeed { get => currentGlideSpeed; }
        public float GlideFallSpeed { get => currentGlideFallSpeed; }
        public bool IsAlive { get => isAlive; }
        public BaseCharacter BaseCharacter { get => baseCharacter; set => baseCharacter = value; }

        public int Level { get => currentLevel; }
        public int CurrentXP { get => currentXP; set => currentXP = value; }

        public List<PowerResource> PowerResourceList {
            get {
                return powerResourceList;
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
        public bool IsStealthed {
            get {
                foreach (StatusEffectNode statusEffectNode in statusEffects.Values) {
                    if (statusEffectNode.StatusEffect.Stealth == true) {
                        return true;
                    }
                }
                return false;
            }
        }
        public Dictionary<PowerResource, PowerResourceNode> PowerResourceDictionary { get => powerResourceDictionary; set => powerResourceDictionary = value; }
        public Dictionary<string, Stat> PrimaryStats { get => primaryStats; set => primaryStats = value; }
        public Dictionary<SecondaryStatType, Stat> SecondaryStats { get => secondaryStats; set => secondaryStats = value; }

        public CharacterStats(BaseCharacter baseCharacter, SystemGameManager systemGameManager) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterStats()");
            this.baseCharacter = baseCharacter;
            Configure(systemGameManager);
            SetPrimaryStatModifiers();
            InitializeSecondaryStats();

            walkSpeed = systemConfigurationManager.WalkSpeed;
            runSpeed = systemConfigurationManager.RunSpeed;
            swimSpeed = systemConfigurationManager.SwimSpeed;
            flySpeed = systemConfigurationManager.FlySpeed;
            glideSpeed = systemConfigurationManager.GlideSpeed;
            glideFallSpeed = systemConfigurationManager.GlideFallSpeed;
            currentSwimSpeed = swimSpeed;
            currentFlySpeed = flySpeed;
            currentGlideSpeed = glideSpeed;
            currentGlideFallSpeed = glideFallSpeed;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            levelManager = systemGameManager.LevelManager;
            playerManager = systemGameManager.PlayerManager;
            combatTextManager = systemGameManager.UIManager.CombatTextManager;
        }

        public void Init() {
            SetLevel(level);
            //TrySpawnDead();
        }

        /*
               public void ApplyControlEffects(IAbilityCaster source) {
                   baseCharacter.UnitController.ApplyControlEffects((source as BaseCharacter));
               }
               */

        public void ProcessLevelLoad() {
            // remove scene specific status effects that are not valid in this scene
            List<StatusEffectNode> removeNodes = new List<StatusEffectNode>();
            foreach (StatusEffectNode statusEffectNode in statusEffects.Values) {
                if (statusEffectNode.StatusEffect.SceneNames.Count > 0) {
                    bool sceneFound = false;
                    foreach (string sceneName in statusEffectNode.StatusEffect.SceneNames) {
                        if (SystemDataFactory.PrepareStringForMatch(sceneName) == SystemDataFactory.PrepareStringForMatch(levelManager.GetActiveSceneNode().DisplayName)) {
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
            powerResourceList.AddRange(systemConfigurationManager.PowerResourceList);

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

        public void CancelNonCombatEffects() {

            List<StatusEffectNode> cancelEffects = new List<StatusEffectNode>();

            foreach (StatusEffectNode statusEffectNode in statusEffects.Values) {
                if (statusEffectNode.StatusEffect.RequireOutOfCombat == true) {
                    cancelEffects.Add(statusEffectNode);
                }
            }

            foreach (StatusEffectNode statusEffectNode in cancelEffects) {
                statusEffectNode.CancelStatusEffect();
            }

        }

        public void CalculatePrimaryStats() {
            CalculateRunSpeed();
            foreach (string statName in primaryStats.Keys) {
                CalculateStat(statName);
            }
            CalculateSecondaryStats();
        }

        public void CalculateSecondaryStats() {
            //Debug.Log(gameObject.name + ".CharacterStats.CalculateSecondaryStats()");

            // update base values
            foreach (SecondaryStatType secondaryStatType in secondaryStats.Keys) {
                secondaryStats[secondaryStatType].BaseValue = (int)LevelEquations.GetBaseSecondaryStatForCharacter(secondaryStatType, this);
            }

            // calculate values that include base values plus modifiers
            foreach (SecondaryStatType secondaryStatType in secondaryStats.Keys) {
                CalculateSecondaryStat(secondaryStatType);
            }
        }

        public bool PerformPowerResourceCheck(BaseAbilityProperties ability, float resourceCost) {
            //Debug.Log(gameObject.name + ".CharacterStats.PerformPowerResourceCheck(" + (ability == null ? "null" : ability.DisplayName) + ", " + resourceCost + ")");
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

        public void HandleCharacterUnitSpawn() {
            foreach (StatusEffectNode statusEffectNode in StatusEffects.Values) {
                if (statusEffectNode.StatusEffect.GetPrefabProfileList(baseCharacter).Count > 0
                    && (statusEffectNode.PrefabObjects == null || statusEffectNode.PrefabObjects.Count == 0)) {
                    statusEffectNode.PrefabObjects = statusEffectNode.StatusEffect.RawCast(baseCharacter, baseCharacter.UnitController, baseCharacter.UnitController, new AbilityEffectContext(baseCharacter));
                }
            }
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
            AddPrimaryStatModifiers(systemConfigurationManager.PrimaryStats, false);

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
            //Debug.Log(gameObject.name + ".CharacterStats.InitializeSecondaryStats()");

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

        /*
        public void ProcessLevelUnload() {
            ClearStatusEffectPrefabs();
        }
        */

        public void CalculateRunSpeed() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterStats.CalculateRunSpeed() current: " + currentRunSpeed);
            float oldRunSpeed = currentRunSpeed;
            float oldSprintSpeed = currentSprintSpeed;
            currentRunSpeed = (runSpeed + GetSecondaryAddModifiers(SecondaryStatType.MovementSpeed)) * GetSecondaryMultiplyModifiers(SecondaryStatType.MovementSpeed);
            currentSprintSpeed = currentRunSpeed * sprintSpeedModifier;
            OnCalculateRunSpeed(oldRunSpeed, currentRunSpeed, oldSprintSpeed, currentSprintSpeed);
            if (baseCharacter?.UnitController != null) {
                baseCharacter.UnitController.HandleMovementSpeedUpdate();
            }
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterStats.CalculateRunSpeed(): runSpeed: " + runSpeed + "; current: " + currentRunSpeed + "; old: " + oldRunSpeed);
        }

        public void HandleMovementSpeedUpdate(string secondaryStatName) {
            CalculateRunSpeed();
        }

        public void HandleStatUpdateCommon(string statName) {
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

        public void HandleStatUpdate(string statName, bool calculateStat = true) {
            if (calculateStat == true) {
                CalculateStat(statName);

                // in the future, loop and only calculate secondary stats that this primary stat affects
                CalculateSecondaryStats();
            }
            HandleStatUpdateCommon(statName);
        }

        private void CalculateEquipmentChanged(Equipment newItem, Equipment oldItem, bool recalculate = true) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterStats.CalculateEquipmentChanged(" + (newItem != null ? newItem.DisplayName : "null") + ", " + (oldItem != null ? oldItem.DisplayName : "null") + ", " + recalculate + ")");
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
                    if (primaryStats.ContainsKey(itemPrimaryStatNode.StatName)) {
                        primaryStats[itemPrimaryStatNode.StatName].RemoveModifier(oldItem.GetPrimaryStatModifier(itemPrimaryStatNode.StatName, Level, baseCharacter));
                    }
                }
            }

            if (recalculate == true) {
                CalculatePrimaryStats();

                foreach (PowerResource _powerResource in PowerResourceDictionary.Keys) {
                    ResourceAmountChangedNotificationHandler(_powerResource);
                }
            }
        }

        public void HandleEquipmentChanged(Equipment newItem, Equipment oldItem, int slotIndex) {
            //Debug.Log(gameObject.name + ".CharacterStats.OnEquipmentChanged(" + (newItem != null ? newItem.DisplayName : "null") + ", " + (oldItem != null ? oldItem.DisplayName : "null") + ")");

            CalculateEquipmentChanged(newItem, oldItem);
        }

        public void CalculateStat(string statName) {
            if (primaryStats.ContainsKey(statName)) {
                primaryStats[statName].CurrentValue = (int)((primaryStats[statName].BaseValue + GetAddModifiers(statName)) * GetMultiplyModifiers(statName));
            }
        }

        public void CalculateSecondaryStat(SecondaryStatType secondaryStatType) {
            //Debug.Log(gameObject.name + ".CharacterStats.CalculateSecondaryStat(" + secondaryStatType.ToString() + ")");

            if (secondaryStats.ContainsKey(secondaryStatType)) {
                secondaryStats[secondaryStatType].CurrentValue = (int)((secondaryStats[secondaryStatType].BaseValue + GetSecondaryAddModifiers(secondaryStatType)) * GetSecondaryMultiplyModifiers(secondaryStatType));
            }
        }

        public float GetSecondaryAddModifiers(SecondaryStatType secondaryStatType) {
            //Debug.Log(gameObject.name + ".CharacterStats.GetAddModifiers(" + statBuffType.ToString() + ")");
            float returnValue = 0;
            foreach (StatusEffectNode statusEffectNode in StatusEffects.Values) {
                if (statusEffectNode.StatusEffect.SecondaryStatBuffsTypes.Contains(secondaryStatType)) {
                    returnValue += statusEffectNode.CurrentStacks * statusEffectNode.StatusEffect.SecondaryStatAmount;
                }
            }
            if (secondaryStats.ContainsKey(secondaryStatType)) {
                returnValue += secondaryStats[secondaryStatType].GetAddValue();
            }
            return returnValue;
        }

        public float GetSecondaryMultiplyModifiers(SecondaryStatType secondaryStatType) {
            //Debug.Log(gameObject.name + ".CharacterStats.GetMultiplyModifiers(" + secondaryStatType.ToString() + ")");
            float returnValue = 1f;
            foreach (StatusEffectNode statusEffectNode in StatusEffects.Values) {
                if (statusEffectNode.StatusEffect.SecondaryStatBuffsTypes.Contains(secondaryStatType)) {
                    returnValue *= (float)statusEffectNode.CurrentStacks * statusEffectNode.StatusEffect.SecondaryStatMultiplier;
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
        protected float GetAddModifiers(string statName) {
            //Debug.Log(gameObject.name + ".CharacterStats.GetAddModifiers(" + statBuffType.ToString() + ")");
            float returnValue = 0;
            foreach (StatusEffectNode statusEffectNode in StatusEffects.Values) {
                if (statusEffectNode.StatusEffect.StatBuffTypeNames.Contains(statName)) {
                    returnValue += statusEffectNode.CurrentStacks * statusEffectNode.StatusEffect.StatAmount;
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
        protected float GetMultiplyModifiers(string statName) {
            //Debug.Log(gameObject.name + ".CharacterStats.GetMultiplyModifiers(" + statBuffType.ToString() + ")");
            float returnValue = 1f;
            foreach (StatusEffectNode statusEffectNode in StatusEffects.Values) {
                if (statusEffectNode.StatusEffect.StatBuffTypeNames.Contains(statName)) {
                    returnValue *= statusEffectNode.CurrentStacks * statusEffectNode.StatusEffect.StatMultiplier;
                }
            }
            if (primaryStats.ContainsKey(statName)) {
                returnValue *= primaryStats[statName].GetMultiplyValue();
            }

            return returnValue;
        }

        public float GetIncomingDamageModifiers() {
            //Debug.Log("CharacterStats.GetDamageModifiers()");
            float returnValue = 1f;
            foreach (StatusEffectNode statusEffectNode in StatusEffects.Values) {
                //Debug.Log("CharacterStats.GetDamageModifiers(): looping through status effects");
                if (statusEffectNode.StatusEffect != null) {
                    if (statusEffectNode.StatusEffect.IncomingDamageMultiplier != 1) {
                        //Debug.Log("CharacterStats.GetDamageModifiers(): looping through status effects: ");
                        returnValue *= statusEffectNode.CurrentStacks * statusEffectNode.StatusEffect.IncomingDamageMultiplier;
                    }
                }
            }
            //Debug.Log("CharacterStats.GetDamageModifiers() returning: " + returnValue);
            return returnValue;
        }

        public float GetSpeedModifiers() {
            //Debug.Log("CharacterStats.GetSpeedModifiers(): ");
            return secondaryStats[SecondaryStatType.Speed].CurrentValue;
        }

        public float GetAccuracyModifiers() {
            //Debug.Log("CharacterStats.GetAccuracyModifiers()");
            return secondaryStats[SecondaryStatType.Accuracy].CurrentValue;
        }

        public bool HasFlight() {
            //Debug.Log("CharacterStats.GetDamageModifiers()");
            foreach (StatusEffectNode statusEffectNode in StatusEffects.Values) {
                if (statusEffectNode.StatusEffect.CanFly == true) {
                    return true;
                }
            }
            return false;
        }

        public bool HasGlide() {
            //Debug.Log("CharacterStats.GetDamageModifiers()");
            foreach (StatusEffectNode statusEffectNode in StatusEffects.Values) {
                if (statusEffectNode.StatusEffect.CanGlide == true) {
                    return true;
                }
            }
            return false;
        }

        public bool HasFreezeImmunity() {
            //Debug.Log("CharacterStats.GetDamageModifiers()");
            foreach (StatusEffectNode statusEffectNode in StatusEffects.Values) {
                if (statusEffectNode.StatusEffect.ImmuneDisableAnimator == true) {
                    return true;
                }
            }
            return false;
        }

        public bool HasStunImmunity() {
            //Debug.Log("CharacterStats.GetDamageModifiers()");
            foreach (StatusEffectNode statusEffectNode in StatusEffects.Values) {
                if (statusEffectNode.StatusEffect.ImmuneStun == true) {
                    return true;
                }
            }
            return false;
        }

        public bool HasLevitateImmunity() {
            //Debug.Log("CharacterStats.GetDamageModifiers()");
            foreach (StatusEffectNode statusEffectNode in StatusEffects.Values) {
                if (statusEffectNode.StatusEffect.ImmuneLevitate == true) {
                    return true;
                }
            }
            return false;
        }


        public float GetThreatModifiers() {
            //Debug.Log("CharacterStats.GetDamageModifiers()");
            float returnValue = 1f;
            foreach (StatusEffectNode statusEffectNode in StatusEffects.Values) {
                //Debug.Log("CharacterStats.GetDamageModifiers(): looping through status effects");
                if (statusEffectNode.StatusEffect.ThreatMultiplier != 1) {
                    //Debug.Log("CharacterStats.GetDamageModifiers(): looping through status effects: ");
                    returnValue *= (float)statusEffectNode.CurrentStacks * statusEffectNode.StatusEffect.ThreatMultiplier;
                }
            }
            //Debug.Log("CharacterStats.GetDamageModifiers() returning: " + returnValue);
            return returnValue;
        }

        public float GetOutGoingDamageModifiers() {
            //Debug.Log("CharacterStats.GetDamageModifiers()");
            float returnValue = 1f;
            foreach (StatusEffectNode statusEffectNode in StatusEffects.Values) {
                //Debug.Log("CharacterStats.GetDamageModifiers(): looping through status effects");
                if (statusEffectNode.StatusEffect.OutgoingDamageMultiplier != 1) {
                    //Debug.Log("CharacterStats.GetDamageModifiers(): looping through status effects: " + statusEffectNode.StatusEffect.DisplayName);
                    returnValue *= (float)statusEffectNode.CurrentStacks * statusEffectNode.StatusEffect.OutgoingDamageMultiplier;
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
        public void AttemptAgro(IAbilityCaster sourceCharacter, CharacterUnit target) {
            if (target != null && (sourceCharacter.AbilityManager as CharacterAbilityManager) is CharacterAbilityManager) {
                if (target.BaseCharacter != null) {
                    if (Faction.RelationWith(target.BaseCharacter, (sourceCharacter.AbilityManager as CharacterAbilityManager).BaseCharacter) <= -1) {
                        if (target.BaseCharacter.CharacterCombat != null) {
                            // agro includes a liveness check, so casting necromancy on a dead enemy unit should not pull it into combat with us if we haven't applied a faction or master control buff yet
                            target.BaseCharacter.UnitController.Agro((sourceCharacter.AbilityManager as CharacterAbilityManager).BaseCharacter.UnitController.CharacterUnit);
                        }
                    }
                }
            }
        }

        public bool WasImmuneToDamageType(PowerResource powerResource, IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectContext) {
            if (!powerResourceDictionary.ContainsKey(powerResource)) {
                if (sourceCharacter == (playerManager.MyCharacter as IAbilityCaster)) {
                    combatTextManager.SpawnCombatText(baseCharacter.UnitController, 0, CombatTextType.immune, CombatMagnitude.normal, abilityEffectContext);
                }
                OnImmuneToEffect(abilityEffectContext);
                return true;
            }
            return false;
        }

        public bool WasImmuneToFreeze(StatusEffectProperties statusEffect, IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectContext) {
            if (statusEffect.DisableAnimator == true && baseCharacter.CharacterStats.HasFreezeImmunity()) {
                if (sourceCharacter == (playerManager.MyCharacter as IAbilityCaster)) {
                    combatTextManager.SpawnCombatText(baseCharacter.UnitController, 0, CombatTextType.immune, CombatMagnitude.normal, abilityEffectContext);
                }
                OnImmuneToEffect(abilityEffectContext);
                return true;
            }
            return false;
        }

        public bool WasImmuneToStun(StatusEffectProperties statusEffect, IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectContext) {
            // check for stun
            if (statusEffect.Stun == true && baseCharacter.CharacterStats.HasStunImmunity()) {
                if (sourceCharacter == (playerManager.MyCharacter as IAbilityCaster)) {
                    combatTextManager.SpawnCombatText(baseCharacter.UnitController, 0, CombatTextType.immune, CombatMagnitude.normal, abilityEffectContext);
                }
                OnImmuneToEffect(abilityEffectContext);
                return true;
            }
            return false;
        }

        public bool WasImmuneToLevitate(StatusEffectProperties statusEffect, IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectContext) {
            // check for levitate
            if (statusEffect.Levitate == true && baseCharacter.CharacterStats.HasLevitateImmunity()) {
                if (sourceCharacter == (playerManager.MyCharacter as IAbilityCaster)) {
                    combatTextManager.SpawnCombatText(baseCharacter.UnitController, 0, CombatTextType.immune, CombatMagnitude.normal, abilityEffectContext);
                }
                OnImmuneToEffect(abilityEffectContext);
                return true;
            }
            return false;
        }

        public StatusEffectNode ApplyStatusEffect(StatusEffectProperties statusEffect, IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterStats.ApplyStatusEffect(" + statusEffect.DisplayName + ", " + sourceCharacter.AbilityManager.Name + ")");
            if (IsAlive == false && statusEffect.GetTargetOptions(sourceCharacter).RequireLiveTarget == true && statusEffect.GetTargetOptions(sourceCharacter).RequireDeadTarget == false) {
                //Debug.Log("Cannot apply status effect to dead character. return null.");
                return null;
            }
            if (IsAlive == true && statusEffect.GetTargetOptions(sourceCharacter).RequireDeadTarget == true && statusEffect.GetTargetOptions(sourceCharacter).RequireLiveTarget == false) {
                //Debug.Log("Cannot apply status effect to dead character. return null.");
                return null;
            }

            if (baseCharacter.UnitController != null) {
                // no need to agro if this is being applied from a character load
                AttemptAgro(sourceCharacter, baseCharacter.UnitController.CharacterUnit);
            }

            // check for frozen
            if (WasImmuneToFreeze(statusEffect, sourceCharacter, abilityEffectContext)) {
                return null;
            }

            // check for stun
            if (WasImmuneToStun(statusEffect, sourceCharacter, abilityEffectContext)) {
                return null;
            }

            // check for levitate
            if (WasImmuneToLevitate(statusEffect, sourceCharacter, abilityEffectContext)) {
                return null;
            }
            //Debug.Log("CharacterStats.ApplyStatusEffect(" + statusEffect.ToString() + ", " + source.name + ", " + target.name + ")");
            //Debug.Log("statuseffects count: " + statusEffects.Count);

            // check if another effect from the same status effect group already exists on the target
            if (statusEffect.StatusEffectGroup != null) {

                // keep a list of status effects to overwrite
                List<StatusEffectNode> removeNodes = new List<StatusEffectNode>();

                foreach (StatusEffectNode statusEffectNode in statusEffects.Values) {
                    if (statusEffectNode.StatusEffect.StatusEffectGroup == statusEffect.StatusEffectGroup) {
                        // another effect of this group type exists
                        if (statusEffectNode.StatusEffect.StatusEffectGroup.ExclusiveOption == StatusEffectGroupOption.First) {
                            // the first status effect cannot be overwritten, do nothing
                            sourceCharacter.AbilityManager.ReceiveMessageFeedMessage("Another effect of this type already exists and cannot be overwritten");
                            return null;
                        } else if (statusEffectNode.StatusEffect.StatusEffectGroup.ExclusiveOption == StatusEffectGroupOption.Last) {
                            // the first status effect should be overwritten, add to removal list
                            removeNodes.Add(statusEffectNode);
                        }
                    }
                }

                // remove any status effects that were flagged for removal
                foreach (StatusEffectNode removeNode in removeNodes) {
                    removeNode.CancelStatusEffect();
                }
            }


            // check if status effect already exists on target
            StatusEffectProperties comparedStatusEffect = null;
            string peparedString = SystemDataFactory.PrepareStringForMatch(statusEffect.DisplayName);
            if (statusEffects.ContainsKey(peparedString)) {
                comparedStatusEffect = statusEffects[peparedString].StatusEffect;
            }

            //Debug.Log("comparedStatusEffect: " + comparedStatusEffect);
            if (comparedStatusEffect != null) {
                if (!statusEffects[peparedString].AddStack()) {
                    //Debug.Log("Could not apply " + statusEffect.MyAbilityEffectName + ".  Max stack reached");
                } else {
                    //AddStatusEffectModifiers(statusEffect);
                    HandleChangedNotifications(comparedStatusEffect);
                }
                return null;
            } else {

                // add to effect list since it was not in there
                if (statusEffect == null) {
                    Debug.LogError("CharacterStats.ApplyStatusEffect(): Could not get status effect " + statusEffect.DisplayName);
                    return null;
                }
                StatusEffectNode newStatusEffectNode = new StatusEffectNode(systemGameManager);
                statusEffects.Add(SystemDataFactory.PrepareStringForMatch(statusEffect.DisplayName), newStatusEffectNode);

                // set base ability to null so that all damage taken by a status effect tick is considered ability damage for combat text purposes
                abilityEffectContext.baseAbility = null;

                newStatusEffectNode.Setup(this, statusEffect, abilityEffectContext);
                Coroutine newCoroutine = baseCharacter.StartCoroutine(Tick(sourceCharacter, abilityEffectContext, statusEffect, newStatusEffectNode));
                newStatusEffectNode.MyMonitorCoroutine = newCoroutine;
                //newStatusEffectNode.Setup(this, _statusEffect, newCoroutine);

                HandleAddNotifications(newStatusEffectNode);

                if (newStatusEffectNode.StatusEffect.ControlTarget == true) {
                    if (baseCharacter.UnitController != null) {
                        // control effects really shouldn't be on saved characters, but just in case, check if no unit is spawned yet
                        sourceCharacter.AbilityManager.AddTemporaryPet(baseCharacter.UnitProfile, baseCharacter.UnitController);
                    }

                    // any control effect will add the pet to the pet journal if this is used.  This is already done in capture pet effect so should not be needed
                    // see if leaving it commented out breaks anything
                    //sourceCharacter.AbilityManager.AddPet(baseCharacter.CharacterUnit);

                }

                return newStatusEffectNode;
            }
        }

        public bool HasStatusEffect(StatusEffectProperties statusEffect) {
            return HasStatusEffect(statusEffect.DisplayName);
        }

        public bool HasStatusEffect(string statusEffectName) {
            if (statusEffects.ContainsKey(SystemDataFactory.PrepareStringForMatch(statusEffectName))) {
                return true;
            }
            return false;
        }

        public StatusEffectNode GetStatusEffectNode(StatusEffectProperties statusEffect) {
            if (statusEffects.ContainsKey(SystemDataFactory.PrepareStringForMatch(statusEffect.DisplayName))) {
                return StatusEffects[SystemDataFactory.PrepareStringForMatch(statusEffect.DisplayName)];
            }
            return null;
        }

        private void HandleAddNotifications(StatusEffectNode statusEffectNode) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterStats.HandleChangedNotifications(" + statusEffectNode.StatusEffect.DisplayName + "): NOTIFYING STATUS EFFECT UPDATE");
            OnStatusEffectAdd(statusEffectNode);
            HandleChangedNotifications(statusEffectNode.StatusEffect);
            if (baseCharacter.UnitController != null) {
                baseCharacter.UnitController.NotifyOnStatusEffectAdd(statusEffectNode);
            }
        }

        public void HandleChangedNotifications(StatusEffectProperties statusEffect) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterStats.HandleChangedNotifications(" + (statusEffect == null ? "null" : statusEffect.DisplayName) + ")");

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
                // if unit is currently moving, the motor must be informed of any movement speed change as it is usually only informed on state change
            }
            if (statusEffect.SecondaryStatBuffsTypes.Count > 0) {
                CalculateSecondaryStats();
                StatChangedNotificationHandler();
            }

            if (statusEffect.CanFly == true) {
                if (HasFlight() == false) {
                    if (baseCharacter.UnitController != null) {
                        baseCharacter.UnitController.CanFlyOverride = false;
                    }
                }
            }
            if (statusEffect.CanGlide == true) {
                if (HasGlide() == false) {
                    if (baseCharacter.UnitController != null) {
                        baseCharacter.UnitController.CanGlideOverride = false;
                    }
                }
            }

            if (statusEffect.Stealth == true) {
                if (IsStealthed == false) {
                    DeactivateStealth();
                }
            }


            if (statusEffect.FactionModifiers.Count > 0) {
                SystemEventManager.TriggerEvent("OnReputationChange", new EventParamProperties());
            }
        }

        public void HandleStatusEffectRemoval(StatusEffectProperties statusEffect) {
            //Debug.Log("CharacterStats.HandleStatusEffectRemoval(" + statusEffect.name + ")");
            string preparedString = SystemDataFactory.PrepareStringForMatch(statusEffect.DisplayName);
            if (statusEffects.ContainsKey(preparedString)) {
                if (statusEffects[preparedString].MyMonitorCoroutine != null) {
                    baseCharacter.StopCoroutine(statusEffects[preparedString].MyMonitorCoroutine);
                }
                statusEffects.Remove(preparedString);
            }

            // should reset resources back down after buff expires
            HandleChangedNotifications(statusEffect);
        }

        public void GainXP(int xp) {
            //Debug.Log(gameObject.name + ": GainXP(" + xp + ")");
            currentXP += xp;
            int overflowXP = 0;
            while (currentXP - LevelEquations.GetXPNeededForLevel(currentLevel, systemConfigurationManager) >= 0) {
                overflowXP = currentXP - LevelEquations.GetXPNeededForLevel(currentLevel, systemConfigurationManager);
                GainLevel();
                currentXP = overflowXP;
            }
            OnGainXP(xp);
        }

        public void GainLevel() {
            // make gain level sound and graphic
            SetLevel(currentLevel + 1);
            OnLevelChanged(currentLevel);
            baseCharacter.CharacterSkillManager.UpdateSkillList(currentLevel);
            baseCharacter.CharacterAbilityManager.UpdateAbilityList(currentLevel);
            baseCharacter.CharacterRecipeManager.UpdateRecipeList(currentLevel);
            if (baseCharacter.UnitController != null) {
                baseCharacter.UnitController.NotifyOnLevelChanged(currentLevel);
            }
        }

        public void SetLevel(int newLevel) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterStats.SetLevel(" + newLevel + ")");

            Dictionary<string, float> multiplierValues = new Dictionary<string, float>();
            foreach (string statName in primaryStats.Keys) {
                multiplierValues.Add(statName, 1f);
            }

            currentLevel = newLevel;
            float primaryStatMultiplier = 1;
            if (baseCharacter?.UnitToughness != null) {
                primaryStatMultiplier = baseCharacter.UnitToughness.DefaultPrimaryStatMultiplier;

                foreach (PrimaryStatMultiplierNode primaryStatMultiplierNode in baseCharacter.UnitToughness.PrimaryStatMultipliers) {
                    multiplierValues[primaryStatMultiplierNode.StatName] = primaryStatMultiplierNode.StatMultiplier;
                }
                resourceMultipliers = new Dictionary<string, float>();
                if (baseCharacter.UnitToughness.DefaultResourceMultiplier != 1f) {
                    foreach (PowerResource powerResource in powerResourceDictionary.Keys) {
                        resourceMultipliers.Add(powerResource.ResourceName, baseCharacter.UnitToughness.DefaultResourceMultiplier);
                    }
                }
                foreach (ResourceMultiplierNode resourceMultiplierNode in baseCharacter.UnitToughness.ResourceMultipliers) {
                    if (resourceMultipliers.ContainsKey(resourceMultiplierNode.ResourceName) == false) {
                        resourceMultipliers.Add(resourceMultiplierNode.ResourceName, resourceMultiplierNode.ValueMultiplier);
                    } else {
                        resourceMultipliers[resourceMultiplierNode.ResourceName] = resourceMultipliers[resourceMultiplierNode.ResourceName] * resourceMultiplierNode.ValueMultiplier;
                    }
                }

            }

            // calculate base values independent of any modifiers
            foreach (string statName in primaryStats.Keys) {
                primaryStats[statName].BaseValue = (int)(currentLevel * LevelEquations.GetPrimaryStatForLevel(statName, currentLevel, baseCharacter, systemConfigurationManager) * multiplierValues[statName] * primaryStatMultiplier);
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

        public void ReducePowerResource(PowerResource powerResource, int usedResourceAmount) {
            UsePowerResource(powerResource, usedResourceAmount);
        }

        public void TakeFallDamage(float damagePercent) {
            foreach (PowerResource powerResource in powerResourceDictionary.Keys) {
                if (powerResource.IsHealth == true) {
                    ReducePowerResource(powerResource, (int)((damagePercent / 100f) * GetPowerResourceMaxAmount(powerResource)));
                }
            }
            if (systemConfigurationManager.FallDamageAudioClip != null) {
                baseCharacter.UnitController.UnitComponentController.PlayEffectSound(systemConfigurationManager.FallDamageAudioClip);
            }
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

        public void UsePowerResource(PowerResource powerResource, int usedResourceAmount) {
            usedResourceAmount = Mathf.Clamp(usedResourceAmount, 0, int.MaxValue);
            if (powerResourceDictionary.ContainsKey(powerResource)) {
                powerResourceDictionary[powerResource].currentValue -= usedResourceAmount;
                powerResourceDictionary[powerResource].currentValue = Mathf.Clamp(powerResourceDictionary[powerResource].currentValue, 0, int.MaxValue);
                NotifyOnResourceAmountChanged(powerResource, (int)GetPowerResourceMaxAmount(powerResource), (int)powerResourceDictionary[powerResource].currentValue);
            }
            if (powerResource.IsHealth == true) {
                PerformDeathCheck();
            }
        }

        public void NotifyOnResourceAmountChanged(PowerResource powerResource, int maxValue, int currentValue) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterStats.NotifyOnResourceAmountChanged(" + powerResource.DisplayName + ", " + maxValue + ", " + currentValue + ")");
            OnResourceAmountChanged(powerResource, maxValue, currentValue);
            if (baseCharacter.UnitController != null) {
                baseCharacter.UnitController.NotifyOnResourceAmountChanged(powerResource, maxValue, currentValue);
            }

        }

        public void SetResourceAmount(string resourceName, float newAmount) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterStats.SetResourceAmount(" + resourceName + ", " + newAmount + "): current " + CurrentPrimaryResource);
            newAmount = Mathf.Clamp(newAmount, 0, int.MaxValue);
            PowerResource tmpPowerResource = systemDataFactory.GetResource<PowerResource>(resourceName);

            if (tmpPowerResource != null && powerResourceDictionary.ContainsKey(tmpPowerResource)) {
                powerResourceDictionary[tmpPowerResource].currentValue = newAmount;
                powerResourceDictionary[tmpPowerResource].currentValue = Mathf.Clamp(
                    powerResourceDictionary[tmpPowerResource].currentValue,
                    0,
                    (int)GetPowerResourceMaxAmount(tmpPowerResource));
                NotifyOnResourceAmountChanged(tmpPowerResource, (int)GetPowerResourceMaxAmount(tmpPowerResource), (int)powerResourceDictionary[tmpPowerResource].currentValue);
                //Debug.Log(gameObject.name + ".CharacterStats.SetResourceAmount(" + resourceName + ", " + newAmount + "): current " + CurrentPrimaryResource);
            }
        }

        /// <summary>
        /// return true if resource could be added, false if not
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="newAmount"></param>
        /// <returns></returns>
        public bool AddResourceAmount(string resourceName, float newAmount) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterStats.AddResourceAmount(" + resourceName + ", " + newAmount + ")");
            newAmount = Mathf.Clamp(newAmount, 0, int.MaxValue);
            PowerResource tmpPowerResource = systemDataFactory.GetResource<PowerResource>(resourceName);

            bool returnValue = false;
            if (tmpPowerResource != null && powerResourceDictionary.ContainsKey(tmpPowerResource)) {
                powerResourceDictionary[tmpPowerResource].currentValue += newAmount;
                powerResourceDictionary[tmpPowerResource].currentValue = Mathf.Clamp(
                    powerResourceDictionary[tmpPowerResource].currentValue,
                    0,
                    (int)GetPowerResourceMaxAmount(tmpPowerResource));
                NotifyOnResourceAmountChanged(tmpPowerResource, (int)GetPowerResourceMaxAmount(tmpPowerResource), (int)powerResourceDictionary[tmpPowerResource].currentValue);
                returnValue = true;
                //Debug.Log(gameObject.name + ".CharacterStats.SetResourceAmount(" + resourceName + ", " + newAmount + "): current " + CurrentPrimaryResource);
            }
            return returnValue;
        }

        public bool RecoverResource(AbilityEffectContext abilityEffectContext, PowerResource powerResource, int amount, IAbilityCaster source, bool showCombatText = true, CombatMagnitude combatMagnitude = CombatMagnitude.normal) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterStats.RecoverResource(" + powerResource.DisplayName + ", " + amount + ")");

            bool returnValue = AddResourceAmount(powerResource.DisplayName, amount);
            if (returnValue == false) {
                return false;
            }
            if (playerManager.PlayerUnitSpawned == true
                && showCombatText
                && (baseCharacter.UnitController.gameObject == playerManager.UnitController.gameObject || source.AbilityManager.UnitGameObject == playerManager.UnitController.gameObject)) {
                // spawn text over the player
                combatTextManager.SpawnCombatText(baseCharacter.UnitController, amount, CombatTextType.gainResource, combatMagnitude, abilityEffectContext);
            }
            OnRecoverResource(powerResource, amount);
            return true;
        }

        /// <summary>
        /// allows to cap the resource amount if the resource cap has been lowered due to stat debuff etc
        /// </summary>
        public void ResourceAmountChangedNotificationHandler(PowerResource powerResource) {
            //Debug.Log(gameObject.name + ".CharacterStats.ResourceAmountChangedNotificationHandler()");

            if (powerResource != null && powerResourceDictionary.ContainsKey(powerResource)) {
                powerResourceDictionary[powerResource].currentValue = Mathf.Clamp(powerResourceDictionary[powerResource].currentValue, 0, GetPowerResourceMaxAmount(powerResource));
                NotifyOnResourceAmountChanged(powerResource, (int)GetPowerResourceMaxAmount(powerResource), (int)powerResourceDictionary[powerResource].currentValue);
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

        public void ActivateStealth() {
            baseCharacter.UnitController.UnitMaterialController.ActivateStealth();
            OnEnterStealth();
        }

        public void DeactivateStealth() {
            //Debug.Log(baseCharacter.gameObject.name + "CharacterStats.DeactivateStealth()");

            baseCharacter.UnitController.UnitMaterialController.DeactivateStealth();
            OnLeaveStealth();

            // to ensure the character gets agrod if close to enemies, the collider must be cycled
            baseCharacter.UnitController.CharacterUnit.DisableCollider();
            baseCharacter.UnitController.CharacterUnit.EnableCollider();
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
                NotifyOnResourceAmountChanged(_powerResource, (int)GetPowerResourceMaxAmount(_powerResource), (int)PowerResourceDictionary[_powerResource].currentValue);
            }


        }

        public void TrySpawnDead() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterStats.TrySpawnDead()");
            if (baseCharacter != null && baseCharacter.SpawnDead == true) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterStats.TrySpawnDead(): spawning with no health");
                isAlive = false;

                SetResourceAmount(PrimaryResource.DisplayName, 0f);

                // notify subscribers that our health has changed
                NotifyOnResourceAmountChanged(PrimaryResource, MaxPrimaryResource, CurrentPrimaryResource);
            }
        }

        public void Die() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterStats.Die()");
            if (isAlive) {
                isAlive = false;
                ClearStatusEffects(false);
                ClearPowerAmounts();
                BeforeDie(this);
                baseCharacter.UnitController.NotifyOnBeforeDie(this);
                OnDie(this);
                baseCharacter.CharacterPetManager.HandleDie();
                baseCharacter.CharacterCombat.HandleDie();
                baseCharacter.CharacterAbilityManager.HandleDie(this);
                baseCharacter.UnitController.FreezePositionXZ();
                baseCharacter.UnitController.UnitAnimator.HandleDie(this);
                baseCharacter.UnitController.RemoveNamePlate();
                baseCharacter.UnitController.CharacterUnit.HandleDie(this);
                baseCharacter.UnitController.NotifyOnAfterDie(this);
            }
        }

        public void Revive() {
            //Debug.Log(BaseCharacter.MyCharacterName + "Triggering Revive Animation");
            if (isReviving) {
                //Debug.Log(BaseCharacter.MyCharacterName + " is already reviving.  Doing nothing");
                return;
            }
            if (baseCharacter != null && baseCharacter.UnitController != null && baseCharacter.UnitController.UnitAnimator != null) {
                baseCharacter.UnitController.UnitAnimator.EnableAnimator();
            }
            isReviving = true;
            baseCharacter.UnitController?.CharacterUnit?.CancelDespawnDelay();
            //baseCharacter.MyCharacterUnit.DisableCollider();
            OnReviveBegin();
            baseCharacter.UnitController.UnitAnimator.HandleReviveBegin();

        }

        public void ReviveComplete() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterStats.ReviveComplete() Recieved Revive Complete Signal. Resetting Character Stats.");
            ReviveRaw();
            OnReviveComplete();
            if (baseCharacter.UnitController != null) {
                baseCharacter.UnitController.NotifyOnReviveComplete();
            }
        }

        public void ReviveRaw() {
            //Debug.Log(BaseCharacter.gameObject.name + ".CharacterStats.ReviveRaw()");
            isReviving = false;
            baseCharacter.UnitController.CharacterUnit.DisableCollider();
            baseCharacter.UnitController.CharacterUnit.EnableCollider();
            isAlive = true;
            ClearInvalidStatusEffects();

            ResetResourceAmounts();
        }

        /// <summary>
        /// when a body despawns, all effect prefabs should be cleared
        /// </summary>
        public void ClearStatusEffectPrefabs() {
            foreach (StatusEffectNode statusEffectNode in statusEffects.Values) {
                statusEffectNode.ClearEffectPrefabs();
            }
        }


        public void HandleCharacterUnitDespawn() {
            ClearStatusEffectPrefabs();
        }

        public void ClearStatusEffects(bool clearAll = true) {
            //Debug.Log(gameObject.name + ".CharacterStatus.ClearStatusEffects()");
            List<StatusEffectNode> statusEffectNodes = new List<StatusEffectNode>();
            foreach (StatusEffectNode statusEffectNode in statusEffects.Values) {
                if (clearAll == true || statusEffectNode.StatusEffect.ClassTrait == false) {
                    statusEffectNodes.Add(statusEffectNode);
                }
            }
            foreach (StatusEffectNode statusEffectNode in statusEffectNodes) {
                statusEffectNode.CancelStatusEffect();
                statusEffects.Remove(SystemDataFactory.PrepareStringForMatch(statusEffectNode.StatusEffect.DisplayName));
            }
            //statusEffects.Clear();
        }

        protected void ClearInvalidStatusEffects() {
            //Debug.Log(gameObject.name + ".CharacterStatus.ClearInvalidStatusEffects()");
            //List<string> RemoveList = new List<string>();
            List<StatusEffectNode> statusEffectNodes = new List<StatusEffectNode>();
            foreach (StatusEffectNode statusEffectNode in StatusEffects.Values) {
                // TODO : pass in the original source caster of the status effect for more accurate character based check
                if (statusEffectNode.StatusEffect.GetTargetOptions(baseCharacter).RequireDeadTarget == true && statusEffectNode.StatusEffect.GetTargetOptions(baseCharacter).RequireLiveTarget == false) {
                    statusEffectNodes.Add(statusEffectNode);
                }
            }
            foreach (StatusEffectNode statusEffectNode in statusEffectNodes) {
                statusEffectNode.CancelStatusEffect();
            }

        }

        public IEnumerator Tick(IAbilityCaster characterSource, AbilityEffectContext abilityEffectContext, StatusEffectProperties statusEffect, StatusEffectNode statusEffectNode) {
            //Debug.Log(gameObject.name + ".StatusEffect.Tick() start");
            float elapsedTime = 0f;

            statusEffect.ApplyControlEffects(baseCharacter);
            if (abilityEffectContext.overrideDuration != 0) {
                statusEffectNode.SetRemainingDuration(abilityEffectContext.overrideDuration);
            } else {
                statusEffectNode.SetRemainingDuration(statusEffect.Duration);
            }
            if (statusEffect.CastZeroTick) {
                if (baseCharacter != null && baseCharacter.UnitController != null && characterSource != null) {
                    statusEffect.CastTick(characterSource, baseCharacter.UnitController, abilityEffectContext);
                }
            }

            while ((statusEffect.LimitedDuration == false || statusEffect.ClassTrait == true || statusEffectNode.GetRemainingDuration() > 0f) && baseCharacter != null) {
                yield return null;
                statusEffectNode.SetRemainingDuration(statusEffectNode.GetRemainingDuration() - Time.deltaTime);
                elapsedTime += Time.deltaTime;
                // check for tick first so we can do final tick;

                if (elapsedTime >= statusEffect.TickRate && statusEffect.TickRate != 0) {
                    if (baseCharacter != null && baseCharacter.UnitController != null && characterSource != null) {
                        statusEffect.CastTick(characterSource, baseCharacter.UnitController, abilityEffectContext);
                        elapsedTime -= statusEffect.TickRate;
                    }
                }
                statusEffectNode.UpdateStatusNode();
            }
            if (baseCharacter != null) {
                if (characterSource != null & baseCharacter.UnitController != null) {
                    statusEffect.CastComplete(characterSource, baseCharacter.UnitController, abilityEffectContext);
                }
            }

            if (statusEffects.ContainsKey(SystemDataFactory.PrepareStringForMatch(statusEffect.DisplayName))) {
                statusEffects[SystemDataFactory.PrepareStringForMatch(statusEffect.DisplayName)].CancelStatusEffect();
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

        /// <summary>
        /// Return the maximum value for a power resource
        /// </summary>
        /// <param name="powerResource"></param>
        /// <param name="characterStats"></param>
        /// <returns></returns>
        public float GetResourceMaximum(PowerResource powerResource, IStatProvider statProvider) {

            float returnValue = 0f;

            // add power resource from stats conversions
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

                    // add base power resource
                    returnValue += powerResource.BaseAmount;

                    // add level scaled amount
                    returnValue += powerResource.AmountPerLevel * currentLevel;

                    foreach (IStatProvider statProvider in baseCharacter.StatProviders) {
                        if (statProvider != null) {
                            returnValue += GetResourceMaximum(powerResource, statProvider);
                        }
                    }
                }
            }
            if (resourceMultipliers.ContainsKey(powerResource.DisplayName)) {
                returnValue *= resourceMultipliers[powerResource.DisplayName];
            }
            return returnValue;
        }

        protected void ClearPowerAmounts() {
            foreach (PowerResource powerResource in powerResourceDictionary.Keys) {
                SetResourceAmount(powerResource.DisplayName, 0f);
            }
        }

        protected void PerformResourceRegen() {
            //Debug.Log("CharacterStats.PerformResourceRegen()");
            if (baseCharacter == null || baseCharacter.UnitController == null || isAlive == false) {
                // if the character is not spawned, we should not be regenerating their resources.
                //Debug.Log("CharacterStats.PerformResourceRegen(): NULL! baseCharacter: " + (baseCharacter == null ? "null" : baseCharacter.gameObject.name) + "; characterunit: " + (baseCharacter.UnitController == null ? "null" : baseCharacter.UnitController.DisplayName));
                return;
            }
            //Debug.Log("CharacterStats.PerformResourceRegen(): NOT NULL! baseCharacter: " + (baseCharacter == null ? "null" : baseCharacter.gameObject.name) + "; characterunit: " + (baseCharacter.UnitController == null ? "null" : baseCharacter.UnitController.DisplayName));
            foreach (PowerResource powerResource in powerResourceDictionary.Keys) {
                powerResourceDictionary[powerResource].elapsedTime += Time.deltaTime;

                if (powerResourceDictionary[powerResource].elapsedTime >= powerResource.TickRate) {
                    powerResourceDictionary[powerResource].elapsedTime -= powerResource.TickRate;
                    if (
                        ((powerResource.RegenPerTick > 0f || powerResource.CombatRegenPerTick > 0f) && (powerResourceDictionary[powerResource].currentValue < GetPowerResourceMaxAmount(powerResource)))
                        || ((powerResource.RegenPerTick < 0f || powerResource.CombatRegenPerTick < 0f) && (powerResourceDictionary[powerResource].currentValue > 0f))
                        ) {
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
                        if (usedRegenAmount != 0f) {
                            //Debug.Log("CharacterStats.PerformResourceRegen(): Trigger OnResourceAmountChanged() regen: " + usedRegenAmount);
                            NotifyOnResourceAmountChanged(powerResource, (int)GetPowerResourceMaxAmount(powerResource), (int)powerResourceDictionary[powerResource].currentValue);
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