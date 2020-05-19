using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PlayerStats : CharacterStats {
        protected override void Awake() {
            //Debug.Log(gameObject.name + ".PlayerStats.Awake()");
            base.Awake();
        }

        public void Start() {
            //Debug.Log(gameObject.name + ".PlayerStats.Start()");
            if (PlayerManager.MyInstance.MyPlayerUnitSpawned) {
                //Debug.Log("PlayerStats.Start(): Player Unit is already spawned");
                ProcessPlayerUnitSpawn();
            }
            CreateStartEventReferences();
        }

        public void CreateStartEventReferences() {
            // TRYING DO THIS HERE TO GIVE IT TIME TO INITIALIZE IN AWAKE
            PlayerManager.MyInstance.MyCharacter.CharacterCombat.OnKillEvent += OnKillEventHandler;
        }

        public override void CreateEventSubscriptions() {
            //Debug.Log(gameObject.name + ".PlayerStats.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            base.CreateEventSubscriptions();
            //SystemEventManager.MyInstance.OnEquipmentChanged += OnEquipmentChanged;
            SystemEventManager.MyInstance.OnLevelChanged += LevelUpHandler;
            SystemEventManager.StartListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            SystemEventManager.MyInstance.OnPlayerUnitDespawn += HandlePlayerUnitDespawn;
            eventSubscriptionsInitialized = true;
        }

        public override void CleanupEventSubscriptions() {
            if (!eventSubscriptionsInitialized) {
                return;
            }
            base.CleanupEventSubscriptions();
            if (PlayerManager.MyInstance != null) {
                if (PlayerManager.MyInstance.MyCharacter != null && PlayerManager.MyInstance.MyCharacter.CharacterCombat != null) {
                    PlayerManager.MyInstance.MyCharacter.CharacterCombat.OnKillEvent -= OnKillEventHandler;
                }
            }
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnLevelChanged -= LevelUpHandler;
                //SystemEventManager.MyInstance.OnEquipmentChanged -= OnEquipmentChanged;
                SystemEventManager.StopListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
                SystemEventManager.MyInstance.OnPlayerUnitDespawn -= HandlePlayerUnitDespawn;
            }
            eventSubscriptionsInitialized = false;
        }

        public void HandlePlayerUnitSpawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log(gameObject.name + ".InanimateUnit.HandlePlayerUnitSpawn()");
            ProcessPlayerUnitSpawn();
        }


        public override void OnDisable() {
            base.OnDisable();
            CleanupEventSubscriptions();
        }

        public override bool WasImmuneToFreeze(StatusEffect statusEffect, IAbilityCaster sourceCharacter) {
            bool returnValue = base.WasImmuneToFreeze(statusEffect, sourceCharacter);
            if (returnValue == true) {
                CombatTextManager.MyInstance.SpawnCombatText(baseCharacter.CharacterUnit.gameObject, 0, CombatTextType.immune, CombatMagnitude.normal);
            }
            return false;
        }

        public override bool WasImmuneToStun(StatusEffect statusEffect, IAbilityCaster sourceCharacter) {
            bool returnValue = base.WasImmuneToStun(statusEffect, sourceCharacter);
            if (returnValue == true) {
                CombatTextManager.MyInstance.SpawnCombatText(baseCharacter.CharacterUnit.gameObject, 0, CombatTextType.immune, CombatMagnitude.normal);
            }
            return false;
        }

        public override bool WasImmuneToLevitate(StatusEffect statusEffect, IAbilityCaster sourceCharacter) {
            bool returnValue = base.WasImmuneToLevitate(statusEffect, sourceCharacter);
            if (returnValue == true) {
                CombatTextManager.MyInstance.SpawnCombatText(baseCharacter.CharacterUnit.gameObject, 0, CombatTextType.immune, CombatMagnitude.normal);
            }
            return false;
        }

        public void OnKillEventHandler(BaseCharacter sourceCharacter, float creditPercent) {
            if (creditPercent == 0) {
                return;
            }
            //Debug.Log(gameObject.name + ": About to gain xp from kill with creditPercent: " + creditPercent);
            GainXP((int)(LevelEquations.GetXPAmountForKill(Level, sourceCharacter.CharacterStats.Level) * creditPercent));
        }

        public override void Die() {
            base.Die();
            // Kill the player
            SystemEventManager.TriggerEvent("OnPlayerDeath", new EventParamProperties());
        }

        public override void CalculateRunSpeed() {
            float oldRunSpeed = currentRunSpeed;
            float oldSprintSpeed = currentSprintSpeed;
            base.CalculateRunSpeed();
            if (currentRunSpeed != oldRunSpeed) {
                EventParamProperties eventParam = new EventParamProperties();
                eventParam.simpleParams.FloatParam = currentRunSpeed;
                SystemEventManager.TriggerEvent("OnSetRunSpeed", eventParam);
                eventParam.simpleParams.FloatParam = currentSprintSpeed;
                SystemEventManager.TriggerEvent("OnSetSprintSpeed", eventParam);
            }
            if (currentSprintSpeed != oldSprintSpeed) {
                EventParamProperties eventParam = new EventParamProperties();
                eventParam.simpleParams.FloatParam = currentSprintSpeed;
                SystemEventManager.TriggerEvent("OnSetSprintSpeed", eventParam);
            }
        }


        public void LevelUpHandler(int NewLevel) {
            MessageFeedManager.MyInstance.WriteMessage(string.Format("YOU HAVE REACHED LEVEL {0}!", NewLevel.ToString()));
        }

        public override StatusEffectNode ApplyStatusEffect(StatusEffect statusEffect, IAbilityCaster source, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log("Playerstats.ApplyStatusEffect()");
            if (statusEffect == null) {
                //Debug.Log("Playerstats.ApplyStatusEffect(): statusEffect is null!");
            }
            StatusEffectNode _statusEffectNode = base.ApplyStatusEffect(statusEffect, source, abilityEffectInput);
            if (statusEffect.MyClassTrait == false) {
                if (_statusEffectNode != null) {
                    UIManager.MyInstance.MyStatusEffectPanelController.SpawnStatusNode(_statusEffectNode, baseCharacter.CharacterUnit);
                    if (abilityEffectInput.savedEffect == false) {
                        if (baseCharacter.CharacterUnit != null) {
                            CombatTextManager.MyInstance.SpawnCombatText(baseCharacter.CharacterUnit.gameObject, statusEffect, true);
                        }
                    }
                }
            }
            return _statusEffectNode;
        }

        public void ProcessPlayerUnitSpawn() {
            //Debug.Log("PlayerStats.HandlePlayerUnitSpawn()");
            if (MyBaseCharacter != null && MyBaseCharacter.AnimatedUnit != null && MyBaseCharacter.AnimatedUnit.MyCharacterAnimator != null) {
                MyBaseCharacter.AnimatedUnit.MyCharacterAnimator.OnReviveComplete += ReviveComplete;
            }

            //code to re-apply visual effects when the player loads into a new level
            foreach (StatusEffectNode statusEffectNode in MyStatusEffects.Values) {
                //Debug.Log("PlayerStats.HandlePlayerUnitSpawn(): re-applying effect object for: " + statusEffectNode.MyStatusEffect.MyName);
                statusEffectNode.MyStatusEffect.RawCast(MyBaseCharacter.CharacterAbilityManager, MyBaseCharacter.CharacterUnit.gameObject, MyBaseCharacter.CharacterUnit.gameObject, new AbilityEffectOutput());
            }
        }

        public void HandlePlayerUnitDespawn() {
            //Debug.Log("PlayerStats.HandlePlayerUnitDespawn()");
            if (MyBaseCharacter != null && MyBaseCharacter.AnimatedUnit != null && MyBaseCharacter.AnimatedUnit.MyCharacterAnimator != null) {
                MyBaseCharacter.AnimatedUnit.MyCharacterAnimator.OnReviveComplete -= ReviveComplete;
            }
        }

        public override void GainLevel() {
            base.GainLevel();
            SystemEventManager.MyInstance.NotifyOnLevelChanged(Level);
        }

        public override void SetLevel(int newLevel) {
            //Debug.Log(gameObject.name + ".PlayerStats.SetLevel(" + newLevel + ")");
            base.SetLevel(newLevel);
            // moving to GainLevel to avoid notifications on character load
            //SystemEventManager.MyInstance.NotifyOnLevelChanged(newLevel);
        }

        public override void ReviveComplete() {
            base.ReviveComplete();
            SystemEventManager.TriggerEvent("OnReviveComplete", new EventParamProperties());
        }

        protected override void PerformResourceRegen() {
            //Debug.Log(gameObject.name + ".PlayerStats.PerformResourceRegen()");
            base.PerformResourceRegen();
        }

    }

}