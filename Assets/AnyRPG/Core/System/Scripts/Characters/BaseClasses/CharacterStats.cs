using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterStats : ConfiguredClass {

        //private int level = 1;

        protected float sprintSpeedModifier = 1.5f;

        // keep track of current level
        private int currentLevel = 0;

        // primary stat names for this character, and their values
        protected Dictionary<string, Stat> primaryStats = new Dictionary<string, Stat>();

        // secondary stats for this character, and their values
        protected Dictionary<SecondaryStatType, Stat> secondaryStats = new Dictionary<SecondaryStatType, Stat>();

        // power resource regen
        protected Dictionary<string, PowerResourceRegenProperty> powerResourceRegenDictionary = new Dictionary<string, PowerResourceRegenProperty>();

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
        protected UnitController unitController = null;

        private bool isReviving = false;
        private bool isAlive = true;
        private int currentXP = 0;

        private List<PowerResource> powerResourceList = new List<PowerResource>();

        protected bool eventSubscriptionsInitialized = false;

        private Coroutine resurrectionCoroutine = null;

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
        //public BaseCharacter BaseCharacter { get => unitController; set => unitController = value; }

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
        public UnitController UnitController { get => unitController; set => unitController = value; }

        public CharacterStats(UnitController unitController, SystemGameManager systemGameManager) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterStats()");
            this.unitController = unitController;
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

        /*
               public void ApplyControlEffects(IAbilityCaster source) {
                   baseCharacter.UnitController.ApplyControlEffects((source as BaseCharacter));
               }
               */

        // this code only needs to be run if baseCharacters are persistent between scene loads
        // if the character is unloaded and then re-spawned on level load, this should not be necessary
        // because the status effects would be re-applied, and likely fail to apply due to scene restrictions
        /*
        public void ProcessLevelLoad() {
            // remove scene specific status effects that are not valid in this scene
            List<StatusEffectNode> removeNodes = new List<StatusEffectNode>();
            foreach (StatusEffectNode statusEffectNode in statusEffects.Values) {
                if (statusEffectNode.StatusEffect.SceneNames.Count > 0) {
                    bool sceneFound = false;
                    foreach (string sceneName in statusEffectNode.StatusEffect.SceneNames) {
                        if (SystemDataUtility.PrepareStringForMatch(sceneName) == levelManager.GetActiveSceneNode().ResourceName) {
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
        */

        public void SetSpawnDead() {
            isAlive = false;
        }

        public void UpdatePowerResourceList() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterStats.UpdatePowerResourceList()");

            // since this is just a list and contains no values, it is safe to overwrite
            powerResourceList = new List<PowerResource>();

            // add from system
            powerResourceList.AddRange(systemConfigurationManager.PowerResourceList);

            if (unitController.BaseCharacter.StatProviders == null) {
                return;
            }

            foreach (IStatProvider statProvider in unitController.BaseCharacter.StatProviders) {
                if (statProvider != null) {
                    foreach (PowerResource powerResource in statProvider.PowerResourceList) {
                        if (!powerResourceList.Contains(powerResource)) {
                            //Debug.Log($"{unitController.gameObject.name}.CharacterStats.UpdatePowerResourceList(): adding resource: {powerResource.DisplayName}");
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
            foreach (string statName in primaryStats.Keys) {
                CalculateStat(statName);
            }
            CalculateSecondaryStats();
            CalculateRunSpeed();
            CalculateRegen();
        }

        public void CalculateRegen() {
            // clear dictionary before recalculation
            powerResourceRegenDictionary.Clear();

            // add base power resource regen amounts
            foreach (PowerResource powerResource in powerResourceDictionary.Keys) {
                if (powerResourceRegenDictionary.ContainsKey(powerResource.ResourceName) == false) {
                    powerResourceRegenDictionary.Add(powerResource.ResourceName, new PowerResourceRegenProperty());
                }
                powerResourceRegenDictionary[powerResource.ResourceName].PercentPerTick += powerResource.PercentPerTick;
                powerResourceRegenDictionary[powerResource.ResourceName].AmountPerTick += powerResource.AmountPerTick;
                powerResourceRegenDictionary[powerResource.ResourceName].CombatPercentPerTick += powerResource.CombatPercentPerTick;
                powerResourceRegenDictionary[powerResource.ResourceName].CombatAmountPerTick += powerResource.CombatAmountPerTick;
            }

            // loop through all stat providers
            foreach (IStatProvider statProvider in unitController.BaseCharacter.StatProviders) {

                if (statProvider != null && statProvider.PrimaryStats != null) {

                    // loop through all stats
                    foreach (StatScalingNode statScalingNode in statProvider.PrimaryStats) {
                        foreach (PowerResourceRegenProperty powerResourceRegenProperty in statScalingNode.Regen) {

                            // add a resource regen key if one does not exist
                            if (powerResourceRegenDictionary.ContainsKey(powerResourceRegenProperty.PowerResource) == false) {
                                powerResourceRegenDictionary.Add(powerResourceRegenProperty.PowerResource, new PowerResourceRegenProperty());
                            }

                            // ensure the character actually has the stat before trying to access it
                            if (primaryStats.ContainsKey(statScalingNode.StatName)) {

                                // add out of combat amount per tick multiplied by stat total to total
                                powerResourceRegenDictionary[powerResourceRegenProperty.PowerResource].AmountPerTick += (powerResourceRegenProperty.AmountPerTick * primaryStats[statScalingNode.StatName].CurrentValue);
                                
                                // add out of combat percent per tick multiplied by stat total to total
                                powerResourceRegenDictionary[powerResourceRegenProperty.PowerResource].PercentPerTick += (powerResourceRegenProperty.PercentPerTick * primaryStats[statScalingNode.StatName].CurrentValue);

                                // add out of combat amount per tick multiplied by stat total to total
                                powerResourceRegenDictionary[powerResourceRegenProperty.PowerResource].CombatAmountPerTick += (powerResourceRegenProperty.CombatAmountPerTick * primaryStats[statScalingNode.StatName].CurrentValue);

                                // add out of combat percent per tick multiplied by stat total to total
                                powerResourceRegenDictionary[powerResourceRegenProperty.PowerResource].CombatPercentPerTick += (powerResourceRegenProperty.CombatPercentPerTick * primaryStats[statScalingNode.StatName].CurrentValue);

                            }
                        }
                    }
                }
            }
        }

        public void CalculateSecondaryStats() {
            //Debug.Log($"{gameObject.name}.CharacterStats.CalculateSecondaryStats()");

            // update base values
            foreach (SecondaryStatType secondaryStatType in secondaryStats.Keys) {
                secondaryStats[secondaryStatType].BaseValue = (int)LevelEquations.GetBaseSecondaryStatForCharacter(secondaryStatType, unitController);
            }

            // calculate values that include base values plus modifiers
            foreach (SecondaryStatType secondaryStatType in secondaryStats.Keys) {
                CalculateSecondaryStat(secondaryStatType);
            }
        }

        public bool PerformPowerResourceCheck(AbilityProperties ability, float resourceCost) {
            //Debug.Log($"{gameObject.name}.CharacterStats.PerformPowerResourceCheck(" + (ability == null ? "null" : ability.DisplayName) + ", " + resourceCost + ")");
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

        /// <summary>
        /// spawn the physical prefabs associated with any status effects on this character
        /// </summary>
        public void HandleCharacterUnitSpawn() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterStats.HandleCharacterUnitSpawn()");

            SpawnStatusEffectPrefabs();
        }

        public void SpawnStatusEffectPrefabs() {
            foreach (StatusEffectNode statusEffectNode in StatusEffects.Values) {
                if (statusEffectNode.StatusEffect.StatusEffectObjectList.Count > 0
                    && (statusEffectNode.PrefabObjects == null || statusEffectNode.PrefabObjects.Count == 0)) {
                    unitController.UnitModelController.ProcessAddStatusEffect(statusEffectNode, statusEffectNode.StatusEffect, statusEffectNode.AbilityEffectContext);
                }
            }
        }

        public void HandleUpdateStatProviders() {
            //Debug.Log($"{gameObject.name}.CharacterStats.HandleSetUnitProfile()");
            SetPrimaryStatModifiers();
        }

        public void AddPrimaryStatModifiers(List<StatScalingNode> primaryStatList, bool updatePowerResourceDictionary = true) {
            if (primaryStatList != null) {
                foreach (StatScalingNode statScalingNode in primaryStatList) {
                    if (!primaryStats.ContainsKey(statScalingNode.StatName)) {
                        //Debug.Log($"{gameObject.name}.CharacterStats.AddUnitProfileModifiers(): adding stat: " + statScalingNode.StatName);
                        primaryStats.Add(statScalingNode.StatName, new Stat(statScalingNode.StatName));
                        //primaryStats[statScalingNode.StatName].OnModifierUpdate += HandleStatUpdateCommon;
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
            //Debug.Log($"{gameObject.name}.CharacterStats.UpdatePowerResourceDictionary()");

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
                    //Debug.Log($"{gameObject.name}.CharacterStats.AddUnitProfileModifiers(): adding resource: " + powerResource.MyDisplayName);
                    powerResourceDictionary.Add(powerResource, new PowerResourceNode());
                }
            }

        }

        /// <summary>
        /// setup the dictionaries that keep track of the current values for stats and resources
        /// </summary>
        public void SetPrimaryStatModifiers() {
            //Debug.Log($"{gameObject.name}.CharacterStats.SetPrimaryStatModifiers()");
            // setup the primary stats dictionary with system defined stats
            AddPrimaryStatModifiers(systemConfigurationManager.PrimaryStats, false);

            if (unitController.BaseCharacter.StatProviders != null) {
                foreach (IStatProvider statProvider in unitController.BaseCharacter.StatProviders) {
                    if (statProvider != null && statProvider.PrimaryStats != null) {
                        AddPrimaryStatModifiers(statProvider.PrimaryStats, false);
                    }
                }
            }

            UpdatePowerResourceDictionary();
        }

        public void InitializeSecondaryStats() {
            //Debug.Log($"{gameObject.name}.CharacterStats.InitializeSecondaryStats()");

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
            float usedRunSpeed = runSpeed;
            if (unitController.RiderUnitController != null) {
                usedRunSpeed = unitController.RiderUnitController.CharacterStats.RunSpeed;
            }
            currentRunSpeed = (usedRunSpeed + GetSecondaryAddModifiers(SecondaryStatType.MovementSpeed)) * GetSecondaryMultiplyModifiers(SecondaryStatType.MovementSpeed);
            currentSprintSpeed = currentRunSpeed * sprintSpeedModifier;
            unitController.UnitEventController.NotifyOnCalculateRunSpeed(oldRunSpeed, currentRunSpeed, oldSprintSpeed, currentSprintSpeed);
            unitController.HandleMovementSpeedUpdate();
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterStats.CalculateRunSpeed(): runSpeed: " + runSpeed + "; current: " + currentRunSpeed + "; old: " + oldRunSpeed);
        }

        public void HandleMovementSpeedUpdate(string secondaryStatName) {
            CalculateRunSpeed();
        }

        /// <summary>
        /// Notify for power resource recalculation if a primary stat change affected any power resources
        /// </summary>
        /// <param name="statName"></param>
        public void NotifyResourceAmountsChanged(string statName) {
            //Debug.Log($"{gameObject.name}.CharacterStats.HandleStatUpdateCommon(" + statName + ")");

            // check if the stat that was just updated contributes to any resource in any way
            // if it does, throw out a resource changed notification handler

            if (unitController.BaseCharacter.StatProviders != null) {

                // make a list since each provider could contribute and we want to avoid multiple notifications for the same resource
                List<PowerResource> affectedResources = new List<PowerResource>();

                foreach (IStatProvider statProvider in unitController.BaseCharacter.StatProviders) {
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

                //CalculateSecondaryStats();
            }
            NotifyResourceAmountsChanged(statName);
        }

        private void CalculateEquipmentChanged(InstantiatedEquipment newInstantiatedEquipment, InstantiatedEquipment oldInstantiatedEquipment) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterStats.CalculateEquipmentChanged(" + (newItem != null ? newItem.DisplayName : "null") + ", " + (oldItem != null ? oldItem.DisplayName : "null") + ", " + recalculate + ")");

            // add modifiers for new item
            if (newInstantiatedEquipment != null) {

                foreach (ItemPrimaryStatNode itemPrimaryStatNode in newInstantiatedEquipment.Equipment.PrimaryStats) {
                    if (primaryStats.ContainsKey(itemPrimaryStatNode.StatName)) {
                        primaryStats[itemPrimaryStatNode.StatName].AddModifier(newInstantiatedEquipment.Equipment.GetPrimaryStatModifier(itemPrimaryStatNode.StatName, Level, unitController.BaseCharacter));
                    }
                }

                // armor is special because it can come from a base value and from secondary stats
                // here we add the base value
                secondaryStats[SecondaryStatType.Armor].AddModifier(newInstantiatedEquipment.Equipment.GetArmorModifier(Level));

                foreach (ItemSecondaryStatNode itemSecondaryStatNode in newInstantiatedEquipment.SecondaryStats) {
                    secondaryStats[itemSecondaryStatNode.SecondaryStat].AddModifier(newInstantiatedEquipment.Equipment.GetSecondaryStatAddModifier(newInstantiatedEquipment.SecondaryStats, itemSecondaryStatNode.SecondaryStat, Level));
                    secondaryStats[itemSecondaryStatNode.SecondaryStat].AddMultiplyModifier(newInstantiatedEquipment.Equipment.GetSecondaryStatMultiplyModifier(newInstantiatedEquipment.SecondaryStats, itemSecondaryStatNode.SecondaryStat));
                }

            }

            // remove modifiers for old item
            if (oldInstantiatedEquipment != null) {
                secondaryStats[SecondaryStatType.Armor].RemoveModifier(oldInstantiatedEquipment.Equipment.GetArmorModifier(Level));

                foreach (ItemSecondaryStatNode itemSecondaryStatNode in oldInstantiatedEquipment.SecondaryStats) {
                    secondaryStats[itemSecondaryStatNode.SecondaryStat].RemoveModifier(oldInstantiatedEquipment.Equipment.GetSecondaryStatAddModifier(oldInstantiatedEquipment.SecondaryStats, itemSecondaryStatNode.SecondaryStat, Level));
                    secondaryStats[itemSecondaryStatNode.SecondaryStat].RemoveMultiplyModifier(oldInstantiatedEquipment.Equipment.GetSecondaryStatMultiplyModifier(oldInstantiatedEquipment.SecondaryStats, itemSecondaryStatNode.SecondaryStat));
                }

                foreach (ItemPrimaryStatNode itemPrimaryStatNode in oldInstantiatedEquipment.Equipment.PrimaryStats) {
                    if (primaryStats.ContainsKey(itemPrimaryStatNode.StatName)) {
                        primaryStats[itemPrimaryStatNode.StatName].RemoveModifier(oldInstantiatedEquipment.Equipment.GetPrimaryStatModifier(itemPrimaryStatNode.StatName, Level, unitController.BaseCharacter));
                    }
                }
            }

        }

        public void HandleEquipmentChanged(InstantiatedEquipment newItem, InstantiatedEquipment oldItem, int slotIndex) {
            //Debug.Log($"{gameObject.name}.CharacterStats.OnEquipmentChanged(" + (newItem != null ? newItem.DisplayName : "null") + ", " + (oldItem != null ? oldItem.DisplayName : "null") + ")");

            CalculateEquipmentChanged(newItem, oldItem);

            CalculatePrimaryStats();

            foreach (PowerResource _powerResource in PowerResourceDictionary.Keys) {
                ResourceAmountChangedNotificationHandler(_powerResource);
            }
        }

        public void CalculateStat(string statName) {
            if (primaryStats.ContainsKey(statName)) {
                primaryStats[statName].CurrentValue = (int)((primaryStats[statName].BaseValue + GetAddModifiers(statName)) * GetMultiplyModifiers(statName));
            }
        }

        public void CalculateSecondaryStat(SecondaryStatType secondaryStatType) {
            //Debug.Log($"{gameObject.name}.CharacterStats.CalculateSecondaryStat(" + secondaryStatType.ToString() + ")");

            if (secondaryStats.ContainsKey(secondaryStatType)) {
                secondaryStats[secondaryStatType].CurrentValue = (int)((secondaryStats[secondaryStatType].BaseValue + GetSecondaryAddModifiers(secondaryStatType)) * GetSecondaryMultiplyModifiers(secondaryStatType));
            }
        }

        public float GetSecondaryAddModifiers(SecondaryStatType secondaryStatType) {
            //Debug.Log($"{gameObject.name}.CharacterStats.GetAddModifiers(" + statBuffType.ToString() + ")");
            float returnValue = 0;

            // get modifiers from status effects
            foreach (StatusEffectNode statusEffectNode in StatusEffects.Values) {
                if (statusEffectNode.StatusEffect.SecondaryStatBuffsTypes.Contains(secondaryStatType)) {
                    returnValue += statusEffectNode.CurrentStacks * statusEffectNode.StatusEffect.SecondaryStatAmount;
                }
            }

            // get modifiers from equipment
            if (secondaryStats.ContainsKey(secondaryStatType)) {
                returnValue += secondaryStats[secondaryStatType].GetAddValue();
            }
            return returnValue;
        }

        public float GetSecondaryMultiplyModifiers(SecondaryStatType secondaryStatType) {
            //Debug.Log($"{gameObject.name}.CharacterStats.GetMultiplyModifiers(" + secondaryStatType.ToString() + ")");
            float returnValue = 1f;

            // get modifiers from status effects
            foreach (StatusEffectNode statusEffectNode in StatusEffects.Values) {
                if (statusEffectNode.StatusEffect.SecondaryStatBuffsTypes.Contains(secondaryStatType)) {
                    returnValue *= (float)statusEffectNode.CurrentStacks * statusEffectNode.StatusEffect.SecondaryStatMultiplier;
                    //Debug.Log($"{gameObject.name}.CharacterStats.GetMultiplyModifiers(" + secondaryStatType.ToString() + "): return: " + returnValue + "; stack: " + statusEffectNode.MyStatusEffect.MyCurrentStacks + "; multiplier: " + statusEffectNode.MyStatusEffect.MyStatMultiplier);
                }
            }
            
            // get modifiers from equipment
            //Debug.Log($"{gameObject.name}.CharacterStats.GetMultiplyModifiers(" + secondaryStatType.ToString() + "): return: " + returnValue);
            if (secondaryStats.ContainsKey(secondaryStatType)) {
                returnValue *= secondaryStats[secondaryStatType].GetMultiplyValue();
            }
            //Debug.Log($"{gameObject.name}.CharacterStats.GetMultiplyModifiers(" + secondaryStatType.ToString() + "): return: " + returnValue);
            return returnValue;
        }

        /// <summary>
        /// Add together additive stat modifiers from status effects and gear
        /// </summary>
        /// <param name="statName"></param>
        /// <returns></returns>
        protected float GetAddModifiers(string statName) {
            //Debug.Log($"{gameObject.name}.CharacterStats.GetAddModifiers(" + statBuffType.ToString() + ")");
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
            //Debug.Log($"{gameObject.name}.CharacterStats.GetMultiplyModifiers(" + statBuffType.ToString() + ")");
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
        public void AttemptAgro(IAbilityCaster sourceCharacter, UnitController targetUnitController) {
            if (targetUnitController == null) {
                return;
            }
            if ((sourceCharacter.AbilityManager as CharacterAbilityManager) is CharacterAbilityManager) {
                if (Faction.RelationWith(targetUnitController, (sourceCharacter.AbilityManager as CharacterAbilityManager).UnitController) <= -1) {
                    if (targetUnitController.CharacterCombat != null) {
                        // agro includes a liveness check, so casting necromancy on a dead enemy unit should not pull it into combat with us if we haven't applied a faction or master control buff yet
                        targetUnitController.Aggro((sourceCharacter as UnitController).CharacterUnit);
                    }
                }
            }
        }

        public bool WasImmuneToDamageType(PowerResource powerResource, IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectContext) {
            if (!powerResourceDictionary.ContainsKey(powerResource)) {
                if (sourceCharacter.gameObject != unitController.gameObject) {
                    sourceCharacter.AbilityManager.ReceiveCombatTextEvent(unitController, 0, CombatTextType.immune, CombatMagnitude.normal, abilityEffectContext);
                }
                unitController.UnitEventController.NotifyOnImmuneToEffect(abilityEffectContext);
                return true;
            }
            return false;
        }

        public bool WasImmuneToFreeze(StatusEffectProperties statusEffect, IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectContext) {
            if (statusEffect.DisableAnimator == true && unitController.CharacterStats.HasFreezeImmunity()) {
                if (sourceCharacter.gameObject != unitController.gameObject) {
                    sourceCharacter.AbilityManager.ReceiveCombatTextEvent(unitController, 0, CombatTextType.immune, CombatMagnitude.normal, abilityEffectContext);
                }
                unitController.UnitEventController.NotifyOnImmuneToEffect(abilityEffectContext);
                return true;
            }
            return false;
        }

        public bool WasImmuneToStun(StatusEffectProperties statusEffect, IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectContext) {
            // check for stun
            if (statusEffect.Stun == true && unitController.CharacterStats.HasStunImmunity()) {
                if (sourceCharacter.gameObject != unitController.gameObject) {
                    sourceCharacter.AbilityManager.ReceiveCombatTextEvent(unitController, 0, CombatTextType.immune, CombatMagnitude.normal, abilityEffectContext);
                }
                unitController.UnitEventController.NotifyOnImmuneToEffect(abilityEffectContext);
                return true;
            }
            return false;
        }

        public bool WasImmuneToLevitate(StatusEffectProperties statusEffect, IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectContext) {
            // check for levitate
            if (statusEffect.Levitate == true && unitController.CharacterStats.HasLevitateImmunity()) {
                if (sourceCharacter.gameObject != unitController.gameObject) {
                    sourceCharacter.AbilityManager.ReceiveCombatTextEvent(unitController, 0, CombatTextType.immune, CombatMagnitude.normal, abilityEffectContext);
                }
                unitController.UnitEventController.NotifyOnImmuneToEffect(abilityEffectContext);
                return true;
            }
            return false;
        }

        public StatusEffectNode ApplyStatusEffect(StatusEffectProperties statusEffect, IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectContext) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterStats.ApplyStatusEffect({statusEffect.ResourceName}, {sourceCharacter.AbilityManager.Name})");

            // add to effect list since it was not in there
            if (statusEffect == null) {
                Debug.LogError("CharacterStats.ApplyStatusEffect(): Could not get status effect " + statusEffect.DisplayName);
                return null;
            }

            if (IsAlive == false
                && statusEffect.GetTargetOptions(sourceCharacter).RequireLiveTarget == true
                && statusEffect.GetTargetOptions(sourceCharacter).RequireDeadTarget == false
                && statusEffect.KeepOnDeath == false
                && abilityEffectContext.savedEffect == false) {
                    //Debug.Log("Cannot apply status effect to dead character. return null.");
                    return null;
            }
            if (IsAlive == true && statusEffect.GetTargetOptions(sourceCharacter).RequireDeadTarget == true && statusEffect.GetTargetOptions(sourceCharacter).RequireLiveTarget == false) {
                //Debug.Log("Cannot apply status effect to dead character. return null.");
                return null;
            }

            // no need to agro if this is being applied from a character load
            AttemptAgro(sourceCharacter, unitController);

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

            // attempt to add a stack to an existing effect
            if (AddStatusEffectStack(statusEffect)) { 
                return null;
            }

            StatusEffectNode _statusEffectNode = AddNewStatusEffect(statusEffect, sourceCharacter, abilityEffectContext);
            return _statusEffectNode;
        }

        public StatusEffectNode AddNewStatusEffect(StatusEffectProperties statusEffectProperties, IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectContext) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterStats.AddNewStatusEffect({statusEffectProperties.ResourceName}, {sourceCharacter.AbilityManager.Name})");

            StatusEffectNode newStatusEffectNode = new StatusEffectNode(systemGameManager);
            statusEffects.Add(statusEffectProperties.ResourceName, newStatusEffectNode);

            // set base ability to null so that all damage taken by a status effect tick is considered ability damage for combat text purposes
            abilityEffectContext.baseAbility = null;

            newStatusEffectNode.Setup(unitController, statusEffectProperties, abilityEffectContext);
            Coroutine newCoroutine = unitController.StartCoroutine(Tick(sourceCharacter, abilityEffectContext, statusEffectProperties, newStatusEffectNode));
            newStatusEffectNode.MonitorCoroutine = newCoroutine;
            //newStatusEffectNode.Setup(this, _statusEffect, newCoroutine);

            HandleAddNotifications(newStatusEffectNode);

            if (newStatusEffectNode.StatusEffect.ControlTarget == true) {
                sourceCharacter.AbilityManager.AddTemporaryPet(unitController.UnitProfile, unitController);

                // any control effect will add the pet to the pet journal if this is used.  This is already done in capture pet effect so should not be needed
                // see if leaving it commented out breaks anything
                //sourceCharacter.AbilityManager.AddPet(baseCharacter.CharacterUnit);

            }

            if (newStatusEffectNode != null) {
                unitController.UnitModelController.ProcessAddStatusEffect(newStatusEffectNode, statusEffectProperties, abilityEffectContext);
            }

            return newStatusEffectNode;
        }

        public bool AddStatusEffectStack(StatusEffectProperties statusEffect) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterStats.AddStatusEffectStack({statusEffect.ResourceName})");

            // check if status effect already exists on target
            StatusEffectProperties comparedStatusEffect = null;
            if (statusEffects.ContainsKey(statusEffect.ResourceName)) {
                comparedStatusEffect = statusEffects[statusEffect.ResourceName].StatusEffect;
            }

            //Debug.Log("comparedStatusEffect: " + comparedStatusEffect);
            if (comparedStatusEffect != null) {
                if (!statusEffects[statusEffect.ResourceName].AddStack()) {
                    //Debug.Log("Could not apply " + statusEffect.MyAbilityEffectName + ".  Max stack reached");
                } else {
                    //AddStatusEffectModifiers(statusEffect);
                    ProcessStatusEffectChanges(comparedStatusEffect);
                }
                return true;
            }

            return false;
        }

        public bool HasStatusEffect(StatusEffectProperties statusEffect) {
            return HasStatusEffect(statusEffect.ResourceName);
        }

        public bool HasStatusEffect(string statusEffectName) {
            if (statusEffects.ContainsKey(statusEffectName)) {
                return true;
            }
            return false;
        }

        public StatusEffectNode GetStatusEffectNode(StatusEffectProperties statusEffect) {
            if (statusEffects.ContainsKey(statusEffect.DisplayName)) {
                return StatusEffects[statusEffect.DisplayName];
            }
            return null;
        }

        private void HandleAddNotifications(StatusEffectNode statusEffectNode) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterStats.HandleChangedNotifications(" + statusEffectNode.StatusEffect.DisplayName + "): NOTIFYING STATUS EFFECT UPDATE");
            ProcessStatusEffectChanges(statusEffectNode.StatusEffect);
            unitController.UnitEventController.NotifyOnStatusEffectAdd(statusEffectNode);
        }

        public void ProcessStatusEffectChanges(StatusEffectProperties statusEffect) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterStats.ProcessStatusEffectChanges({statusEffect.ResourceName})");

            //statusEffect.StatBuffTypeNames
            if (statusEffect.StatBuffTypeNames.Count > 0) {
                foreach (string statName in statusEffect.StatBuffTypeNames) {
                    //HandleStatUpdate(statName, false);
                    HandleStatUpdate(statName, true);
                }
                //StatChangedNotificationHandler();
                
                CalculateRegen();
            }

            
            if (statusEffect.SecondaryStatBuffsTypes.Count > 0 || statusEffect.StatBuffTypeNames.Count > 0) {
                CalculateSecondaryStats();
                StatChangedNotificationHandler();


            }

            if (statusEffect.SecondaryStatBuffsTypes.Contains(SecondaryStatType.MovementSpeed)) {
                CalculateRunSpeed();
                // if unit is currently moving, the motor must be informed of any movement speed change as it is usually only informed on state change
            }

            if (statusEffect.CanFly == true) {
                if (HasFlight() == false) {
                    unitController.CanFlyOverride = false;
                }
            }
            if (statusEffect.CanGlide == true) {
                if (HasGlide() == false) {
                    unitController.CanGlideOverride = false;
                }
            }

            if (statusEffect.Stealth == true) {
                if (IsStealthed == false) {
                    unitController.DeactivateStealth();
                }
            }


            if (statusEffect.FactionModifiers.Count > 0) {
                unitController.UnitEventController.NotifyOnReputationChange();
            }
        }

        public void HandleStatusEffectRemoval(StatusEffectProperties statusEffect) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterStats.HandleStatusEffectRemoval({statusEffect.ResourceName})");

            if (statusEffects.ContainsKey(statusEffect.ResourceName)) {
                if (statusEffects[statusEffect.ResourceName].MonitorCoroutine != null) {
                    unitController.StopCoroutine(statusEffects[statusEffect.ResourceName].MonitorCoroutine);
                }
                statusEffects.Remove(statusEffect.ResourceName);
            }

            // should reset resources back down after buff expires
            ProcessStatusEffectChanges(statusEffect);
        }

        public void GainXP(int xp) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterStats.GainXP({xp})");
            currentXP += xp;
            int overflowXP = 0;
            int initialLevel = currentLevel;
            int levelGains = 0;
            //adjust current xp before sending notification
            while (currentXP - LevelEquations.GetXPNeededForLevel(initialLevel + levelGains, systemConfigurationManager) >= 0) {
                overflowXP = currentXP - LevelEquations.GetXPNeededForLevel(initialLevel + levelGains, systemConfigurationManager);
                levelGains++;
                currentXP = overflowXP;
            }
            unitController.UnitEventController.NotifyOnGainXP(xp, currentXP);
            while (levelGains > 0) {
                GainLevel();
                levelGains--;
            }
        }

        public void SetXP(int xp) {
            currentXP = xp;
        }

        public void GainLevel() {
            // make gain level sound and graphic
            SetLevelInternal(currentLevel + 1);
            unitController.UnitEventController.NotifyOnLevelChanged(currentLevel);
            SetResourceAmountsToMaximum();
        }

        public void SetLevel(int newLevel) {
            SetLevelInternal(newLevel);
            unitController.UnitEventController.NotifyOnLevelChanged(currentLevel);
        }

        public void LoadLevel(int level) {
            currentLevel = level;
        }

        public void SetLevelInternal(int newLevel) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterStats.SetLevel(" + newLevel + ")");

            Dictionary<string, float> multiplierValues = new Dictionary<string, float>();
            foreach (string statName in primaryStats.Keys) {
                multiplierValues.Add(statName, 1f);
            }

            currentLevel = newLevel;

            unitController.CharacterSkillManager.UpdateSkillList(currentLevel);
            unitController.CharacterAbilityManager.UpdateAbilityList(currentLevel);
            unitController.CharacterAbilityManager.UpdateTraitList(currentLevel);
            unitController.CharacterRecipeManager.UpdateRecipeList(currentLevel);
            unitController.CharacterPetManager.UpdatePetList(currentLevel);

            float primaryStatMultiplier = 1;
            if (unitController.BaseCharacter.UnitToughness != null) {
                primaryStatMultiplier = unitController.BaseCharacter.UnitToughness.DefaultPrimaryStatMultiplier;

                foreach (PrimaryStatMultiplierNode primaryStatMultiplierNode in unitController.BaseCharacter.UnitToughness.PrimaryStatMultipliers) {
                    multiplierValues[primaryStatMultiplierNode.StatName] = primaryStatMultiplierNode.StatMultiplier;
                }
                resourceMultipliers = new Dictionary<string, float>();
                if (unitController.BaseCharacter.UnitToughness.DefaultResourceMultiplier != 1f) {
                    foreach (PowerResource powerResource in powerResourceDictionary.Keys) {
                        resourceMultipliers.Add(powerResource.ResourceName, unitController.BaseCharacter.UnitToughness.DefaultResourceMultiplier);
                    }
                }
                foreach (ResourceMultiplierNode resourceMultiplierNode in unitController.BaseCharacter.UnitToughness.ResourceMultipliers) {
                    if (resourceMultipliers.ContainsKey(resourceMultiplierNode.ResourceName) == false) {
                        resourceMultipliers.Add(resourceMultiplierNode.ResourceName, resourceMultiplierNode.ValueMultiplier);
                    } else {
                        resourceMultipliers[resourceMultiplierNode.ResourceName] = resourceMultipliers[resourceMultiplierNode.ResourceName] * resourceMultiplierNode.ValueMultiplier;
                    }
                }

            }

            // calculate base values independent of any modifiers
            foreach (string statName in primaryStats.Keys) {
                primaryStats[statName].BaseValue = (int)(currentLevel * LevelEquations.GetPrimaryStatForLevel(statName, currentLevel, unitController.BaseCharacter, systemConfigurationManager) * multiplierValues[statName] * primaryStatMultiplier);
            }

            // reset any amounts from equipment to deal with item level scaling before performing the calculations that include those equipment stat values
            CalculateEquipmentStats();

            // calculate current values that include modifiers
            CalculatePrimaryStats();
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

            if (unitController.CharacterEquipmentManager != null) {
                foreach (EquipmentInventorySlot equipmentInventorySlot in unitController.CharacterEquipmentManager.CurrentEquipment.Values) {
                    //CalculateEquipmentChanged(equipment, null, false);
                    CalculateEquipmentChanged(equipmentInventorySlot.InstantiatedEquipment, null);
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
            unitController.UnitEventController.NotifyOnTakeFallDamage();
        }

        public void PerformDeathCheck() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterStats.PerformDeathCheck()");

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
            unitController.UnitEventController.NotifyOnResourceAmountChanged(powerResource, maxValue, currentValue);
        }

        public void SetResourceAmount(string resourceName, float newAmount) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterStats.SetResourceAmount(" + resourceName + ", " + newAmount + "): current " + CurrentPrimaryResource);
            newAmount = Mathf.Clamp(newAmount, 0, int.MaxValue);
            PowerResource tmpPowerResource = systemDataFactory.GetResource<PowerResource>(resourceName);

            if (tmpPowerResource != null && powerResourceDictionary.ContainsKey(tmpPowerResource)) {
                int oldAmount = (int)powerResourceDictionary[tmpPowerResource].currentValue;
                powerResourceDictionary[tmpPowerResource].currentValue = newAmount;
                powerResourceDictionary[tmpPowerResource].currentValue = Mathf.Clamp(
                    powerResourceDictionary[tmpPowerResource].currentValue,
                    0,
                    (int)GetPowerResourceMaxAmount(tmpPowerResource));
                NotifyOnResourceAmountChanged(tmpPowerResource, (int)GetPowerResourceMaxAmount(tmpPowerResource), (int)powerResourceDictionary[tmpPowerResource].currentValue);
                //Debug.Log($"{gameObject.name}.CharacterStats.SetResourceAmount(" + resourceName + ", " + newAmount + "): current " + CurrentPrimaryResource);
            }
        }

        public void LoadResourceAmount(string resourceName, float newAmount) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterStats.SetResourceAmount(" + resourceName + ", " + newAmount + "): current " + CurrentPrimaryResource);
            newAmount = Mathf.Clamp(newAmount, 0, int.MaxValue);
            PowerResource tmpPowerResource = systemDataFactory.GetResource<PowerResource>(resourceName);

            if (tmpPowerResource != null && powerResourceDictionary.ContainsKey(tmpPowerResource)) {
                powerResourceDictionary[tmpPowerResource].currentValue = newAmount;
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
                //Debug.Log($"{gameObject.name}.CharacterStats.SetResourceAmount(" + resourceName + ", " + newAmount + "): current " + CurrentPrimaryResource);
            }
            return returnValue;
        }

        public bool RecoverResource(AbilityEffectContext abilityEffectContext, PowerResource powerResource, int amount, IAbilityCaster source, CombatMagnitude combatMagnitude = CombatMagnitude.normal) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterStats.RecoverResource({powerResource.ResourceName}, {amount})");

            bool returnValue = AddResourceAmount(powerResource.ResourceName, amount);
            if (returnValue == false) {
                return false;
            }
            if (source.gameObject != unitController.gameObject) {
                source.AbilityManager.ReceiveCombatTextEvent(unitController, amount, CombatTextType.gainResource, combatMagnitude, abilityEffectContext);
            }
            unitController.UnitEventController.NotifyOnRecoverResource(powerResource, amount, combatMagnitude, abilityEffectContext);
            return true;
        }

        /// <summary>
        /// allows to cap the resource amount if the resource cap has been lowered due to stat debuff etc
        /// </summary>
        public void ResourceAmountChangedNotificationHandler(PowerResource powerResource) {
            //Debug.Log($"{gameObject.name}.CharacterStats.ResourceAmountChangedNotificationHandler()");

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
                unitController.UnitEventController.NotifyOnPrimaryResourceAmountChanged(MaxPrimaryResource, CurrentPrimaryResource);
            }
        }

        public void StatChangedNotificationHandler() {
            unitController.UnitEventController.NotifyOnStatChanged();
        }

        /// <summary>
        /// Set resources to maximum
        /// </summary>
        public void SetResourceAmountsToMaximum() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterStats.SetResourceAmountsToMaximum()");

            if (PowerResourceList == null) {
                return;
            }

            // loop through and update the resources.
            foreach (PowerResource _powerResource in PowerResourceList) {
                if (_powerResource != null && powerResourceDictionary.ContainsKey(_powerResource)) {
                    if (_powerResource.FillOnReset == true) {
                        powerResourceDictionary[_powerResource].currentValue = GetPowerResourceMaxAmount(_powerResource);
                        //Debug.Log($"{unitController.gameObject.name}.CharacterStats.SetResourceAmountsToMaximum(): Setting {_powerResource.ResourceName} to {GetPowerResourceMaxAmount(_powerResource)}");
                    }
                }
                NotifyOnResourceAmountChanged(_powerResource, (int)GetPowerResourceMaxAmount(_powerResource), (int)PowerResourceDictionary[_powerResource].currentValue);
            }
        }

        public void ClipResourceAmounts() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterStats.ClipResourceAmounts()");

            if (PowerResourceList == null) {
                return;
            }

            // loop through and update the resources.
            foreach (PowerResource _powerResource in PowerResourceList) {
                if (_powerResource != null && powerResourceDictionary.ContainsKey(_powerResource)) {
                    if (powerResourceDictionary[_powerResource].currentValue > GetPowerResourceMaxAmount(_powerResource)) {
                        //Debug.Log($"{unitController.gameObject.name}.CharacterStats.SetResourceAmountsToMaximum(): Lowering {_powerResource.ResourceName} from {powerResourceDictionary[_powerResource].currentValue} to {GetPowerResourceMaxAmount(_powerResource)}");
                        powerResourceDictionary[_powerResource].currentValue = GetPowerResourceMaxAmount(_powerResource);
                    }
                }
                NotifyOnResourceAmountChanged(_powerResource, (int)GetPowerResourceMaxAmount(_powerResource), (int)PowerResourceDictionary[_powerResource].currentValue);
            }
        }


        /*
        public void TrySpawnDead() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterStats.TrySpawnDead()");
            if (unitController.BaseCharacter.SpawnDead == true) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterStats.TrySpawnDead(): spawning with no health");
                isAlive = false;

                SetResourceAmount(PrimaryResource.ResourceName, 0f);

                // notify subscribers that our health has changed
                NotifyOnResourceAmountChanged(PrimaryResource, MaxPrimaryResource, CurrentPrimaryResource);
            }
        }
        */

        public void Die() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterStats.Die()");

            if (isAlive) {
                isAlive = false;
                if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive || levelManager.IsCutscene()) {
                    // should only be done on server
                    ClearStatusEffects(false);
                    // should only be done on server
                    ClearPowerAmounts();
                    // should only be done on server
                    unitController.CharacterAbilityManager.HandleDie(this);
                }

                // because this results in loot roll, it needs to be done after status effects are cleared, but before aggro table is cleared
                unitController.UnitEventController.NotifyOnBeforeDie(unitController);

                if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive || levelManager.IsCutscene()) {
                    // should only be done on server
                    unitController.CharacterPetManager.HandleDie();
                    // should only be done on server
                    unitController.CharacterCombat.HandleDie();
                }
                if (systemGameManager.GameMode == GameMode.Local
                    || unitController.IsOwner
                    || (networkManagerServer.ServerModeActive && unitController.UnitControllerMode != UnitControllerMode.Player)
                    || levelManager.IsCutscene()) { 
                    // should only be done on server or authoritative client
                    unitController.FreezePositionXZ();
                    // should only be done on server or authoritative client
                    unitController.UnitAnimator.HandleDie();
                }
                if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == false || levelManager.IsCutscene()) {
                    // should be done on client
                    unitController.RemoveNamePlate();
                    // should be done on client
                    unitController.CharacterUnit.HandleDie(this);
                }

                unitController.UnitEventController.NotifyOnAfterDie(this);
            }
        }

        public void StatusEffectRevive() {
            bool canRevive = false;
            List<StatusEffectNode> cancelNodes = new List<StatusEffectNode>();
            foreach (StatusEffectNode statusEffectNode in statusEffects.Values) {
                // find the first matching status effect that allows revive and cancel it before reviving
                if (statusEffectNode.StatusEffect.AllowRevive == true) {
                    statusEffectNode.CancelStatusEffect();
                    canRevive = true;
                    break;
                }
            }
            if (canRevive == false) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterStats.StatusEffectRevive(): no status effects allow revive");
                return;
            }
            Revive();

        }

        public void Revive() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterStats.Revive()");

            if (isReviving) {
                //Debug.Log(BaseCharacter.MyCharacterName + " is already reviving.  Doing nothing");
                return;
            }
            unitController.UnitAnimator.EnableAnimator();
            isReviving = true;
            if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive || levelManager.IsCutscene()) {
                // should only be done on server
                unitController.CancelDespawnDelay();
            }
            float reviveTime = unitController.UnitAnimator.GetReviveAnimationLength();
            unitController.UnitEventController.NotifyOnReviveBegin(reviveTime);
            if (systemGameManager.GameMode == GameMode.Local
                || unitController.IsOwner
                || (networkManagerServer.ServerModeActive && unitController.UnitControllerMode != UnitControllerMode.Player)
                || levelManager.IsCutscene()) {
                unitController.UnitAnimator.HandleReviveBegin();
            }
            if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true || levelManager.IsCutscene()) {
                resurrectionCoroutine = unitController.StartCoroutine(WaitForResurrection(reviveTime));
            }
        }

        public IEnumerator WaitForResurrection(float animationLength) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterStats.WaitForResurrection({animationLength})");

            float remainingTime = animationLength;
            while (remainingTime > 0f) {
                yield return null;
                remainingTime -= Time.deltaTime;
            }
            //Debug.Log($"{gameObject.name}Setting waitingforhits to false after countdown down");
            unitController.CharacterStats.ReviveComplete();
            resurrectionCoroutine = null;
        }


        public void ReviveComplete() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterStats.ReviveComplete()");

            if (systemGameManager.GameMode == GameMode.Local
                || unitController.IsOwner
                || (networkManagerServer.ServerModeActive && unitController.UnitControllerMode != UnitControllerMode.Player)
                || levelManager.IsCutscene()) {
                // should only be done on server or authoritative client
                unitController.UnitAnimator.HandleReviveComplete();
            }
            isReviving = false;
            isAlive = true;
            if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true || levelManager.IsCutscene()) {
                // should only be done on server
                ReviveRaw();
            }
            unitController.FreezeRotation();
            if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == false || levelManager.IsCutscene()) {
                // should be done on client
                unitController.InitializeNamePlate();
                // minimap updates
                unitController.CharacterUnit.HandleReviveComplete();
                unitController.UnitComponentController.HighlightController.UpdateColors();
            }
            unitController.UnitEventController.NotifyOnReviveComplete();
        }

        public void ReviveRaw() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterStats.ReviveRaw()");

            unitController.DisableCollider();
            unitController.EnableCollider();
            ClearInvalidStatusEffects();

            SetResourceAmountsToMaximum();
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
            //Debug.Log($"{gameObject.name}.CharacterStatus.ClearStatusEffects()");
            List<StatusEffectNode> statusEffectNodes = new List<StatusEffectNode>();
            foreach (StatusEffectNode statusEffectNode in statusEffects.Values) {
                if (clearAll == true || (statusEffectNode.StatusEffect.ClassTrait == false && statusEffectNode.StatusEffect.KeepOnDeath == false)) {
                    statusEffectNodes.Add(statusEffectNode);
                }
            }
            foreach (StatusEffectNode statusEffectNode in statusEffectNodes) {
                statusEffectNode.CancelStatusEffect();
                statusEffects.Remove(statusEffectNode.StatusEffect.ResourceName);
            }
            //statusEffects.Clear();
        }

        protected void ClearInvalidStatusEffects() {
            //Debug.Log($"{gameObject.name}.CharacterStatus.ClearInvalidStatusEffects()");
            //List<string> RemoveList = new List<string>();
            List<StatusEffectNode> statusEffectNodes = new List<StatusEffectNode>();
            foreach (StatusEffectNode statusEffectNode in StatusEffects.Values) {
                // TODO : pass in the original source caster of the status effect for more accurate character based check
                if (statusEffectNode.StatusEffect.GetTargetOptions(unitController).RequireDeadTarget == true && statusEffectNode.StatusEffect.GetTargetOptions(unitController).RequireLiveTarget == false) {
                    statusEffectNodes.Add(statusEffectNode);
                }
            }
            foreach (StatusEffectNode statusEffectNode in statusEffectNodes) {
                statusEffectNode.CancelStatusEffect();
            }

        }

        public IEnumerator Tick(IAbilityCaster characterSource, AbilityEffectContext abilityEffectContext, StatusEffectProperties statusEffect, StatusEffectNode statusEffectNode) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterStats.Tick({statusEffect.ResourceName})");

            float elapsedTime = 0f;

            statusEffect.ApplyControlEffects(unitController);
            if (abilityEffectContext.overrideDuration != 0) {
                statusEffectNode.SetRemainingDuration(abilityEffectContext.overrideDuration);
            } else {
                statusEffectNode.SetRemainingDuration(statusEffect.Duration);
            }
            if (statusEffect.CastZeroTick) {
                if (characterSource?.AbilityManager != null && (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive || levelManager.IsCutscene())) {
                    statusEffect.CastTick(characterSource, unitController, abilityEffectContext);
                }
            }

            while ((statusEffect.LimitedDuration == false || statusEffect.ClassTrait == true || statusEffectNode.GetRemainingDuration() > 0f) && unitController != null && characterSource?.AbilityManager != null) {
                yield return null;
                if (statusEffect.LimitedDuration == true && statusEffect.ClassTrait == false) {
                    statusEffectNode.SetRemainingDuration(statusEffectNode.GetRemainingDuration() - Time.deltaTime);
                    unitController.CharacterSaveManager.SaveStatusEffectData();
                }
                elapsedTime += Time.deltaTime;
                // check for tick first so we can do final tick;

                if (elapsedTime >= statusEffect.TickRate && statusEffect.TickRate != 0) {
                    if (characterSource?.AbilityManager != null) {
                        if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive || levelManager.IsCutscene()) {
                            statusEffect.CastTick(characterSource, unitController, abilityEffectContext);
                        }
                    }
                    elapsedTime -= statusEffect.TickRate;
                }
                statusEffectNode.UpdateStatusNode();
            }
            if (characterSource?.AbilityManager != null && (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive || levelManager.IsCutscene())) {
                statusEffect.CastComplete(characterSource, unitController, abilityEffectContext);
            }

            if (statusEffects.ContainsKey(statusEffect.ResourceName) && (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive || levelManager.IsCutscene())) {
                statusEffects[statusEffect.ResourceName].CancelStatusEffect();
            }
        }

        public void Update() {
            if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true || levelManager.IsCutscene()) {
                PerformResourceRegen();
            }
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
                if (unitController != null) {
                    returnValue += powerResource.MaximumAmount;

                    // add base power resource
                    returnValue += powerResource.BaseAmount;

                    // add level scaled amount
                    returnValue += powerResource.AmountPerLevel * currentLevel;

                    foreach (IStatProvider statProvider in unitController.BaseCharacter.StatProviders) {
                        if (statProvider != null) {
                            returnValue += GetResourceMaximum(powerResource, statProvider);
                        }
                    }
                }
            }
            if (resourceMultipliers.ContainsKey(powerResource.ResourceName)) {
                returnValue *= resourceMultipliers[powerResource.ResourceName];
            }
            return returnValue;
        }

        protected void ClearPowerAmounts() {
            foreach (PowerResource powerResource in powerResourceDictionary.Keys) {
                SetResourceAmount(powerResource.ResourceName, 0f);
            }
        }

        protected void PerformResourceRegen() {
            //Debug.Log("CharacterStats.PerformResourceRegen()");
            if (isAlive == false) {
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
                        (
                         (powerResourceRegenDictionary[powerResource.ResourceName].AmountPerTick > 0f
                            || powerResourceRegenDictionary[powerResource.ResourceName].PercentPerTick > 0f
                            || powerResourceRegenDictionary[powerResource.ResourceName].CombatAmountPerTick > 0f
                            || powerResourceRegenDictionary[powerResource.ResourceName].CombatPercentPerTick > 0f)
                         && (powerResourceDictionary[powerResource].currentValue < GetPowerResourceMaxAmount(powerResource))
                        )
                        || (
                         (powerResourceRegenDictionary[powerResource.ResourceName].AmountPerTick < 0f
                         || powerResourceRegenDictionary[powerResource.ResourceName].PercentPerTick < 0f
                         || powerResourceRegenDictionary[powerResource.ResourceName].CombatAmountPerTick < 0f
                         || powerResourceRegenDictionary[powerResource.ResourceName].CombatPercentPerTick < 0f)
                        && (powerResourceDictionary[powerResource].currentValue > 0f))
                       ) {
                        float usedRegenAmount = 0f;
                        if (unitController?.CharacterCombat != null && unitController.CharacterCombat.GetInCombat() == true) {
                            // perform combat regen
                            usedRegenAmount += GetPowerResourceMaxAmount(powerResource) * (powerResourceRegenDictionary[powerResource.ResourceName].CombatPercentPerTick / 100f);
                            usedRegenAmount += powerResourceRegenDictionary[powerResource.ResourceName].CombatAmountPerTick;
                        } else {
                            // perform out of combat regen
                            usedRegenAmount += GetPowerResourceMaxAmount(powerResource) * (powerResourceRegenDictionary[powerResource.ResourceName].PercentPerTick / 100f);
                            usedRegenAmount += powerResourceRegenDictionary[powerResource.ResourceName].AmountPerTick;
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

        public void CancelStatusEffect(StatusEffectProperties statusEffect) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterStats.CancelStatusEffect({statusEffect.ResourceName})");

            if (statusEffects.ContainsKey(statusEffect.ResourceName)) {
                statusEffects[statusEffect.ResourceName].CancelStatusEffect();
            }
        }

        public void RequestCancelStatusEffect(StatusEffectNode statusEffectNode) {
            if (statusEffectNode.StatusEffect.StatusEffectAlignment == StatusEffectAlignment.Harmful) {
                return;
            }
            if (systemGameManager.GameMode == GameMode.Local) {
                CancelStatusEffect(statusEffectNode.StatusEffect);
            } else {
                unitController.UnitEventController.NotifyOnRequestCancelStatusEffect(statusEffectNode.StatusEffect);
            }
        }

        public bool CanRevive() {
            // check status effects for revive property
            foreach (StatusEffectNode statusEffectNode in statusEffects.Values) {
                if (statusEffectNode.StatusEffect.AllowRevive == true) {
                    return true;
                }
            }
            return false;
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