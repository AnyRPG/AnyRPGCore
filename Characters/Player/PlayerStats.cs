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
                HandlePlayerUnitSpawn();
            }
            CreateStartEventReferences();
        }

        public void CreateStartEventReferences() {
            // TRYING DO THIS HERE TO GIVE IT TIME TO INITIALIZE IN AWAKE
            PlayerManager.MyInstance.MyCharacter.MyCharacterCombat.OnKillEvent += OnKillEventHandler;
        }

        public override void CreateEventSubscriptions() {
            //Debug.Log(gameObject.name + ".PlayerStats.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            base.CreateEventSubscriptions();
            //SystemEventManager.MyInstance.OnEquipmentChanged += OnEquipmentChanged;
            SystemEventManager.MyInstance.OnLevelChanged += LevelUpHandler;
            SystemEventManager.MyInstance.OnPlayerUnitSpawn += HandlePlayerUnitSpawn;
            SystemEventManager.MyInstance.OnPlayerUnitDespawn += HandlePlayerUnitDespawn;
            eventSubscriptionsInitialized = true;
        }

        public override void CleanupEventSubscriptions() {
            if (!eventSubscriptionsInitialized) {
                return;
            }
            base.CleanupEventSubscriptions();
            if (PlayerManager.MyInstance != null) {
                if (PlayerManager.MyInstance.MyCharacter != null && PlayerManager.MyInstance.MyCharacter.MyCharacterCombat != null) {
                    PlayerManager.MyInstance.MyCharacter.MyCharacterCombat.OnKillEvent -= OnKillEventHandler;
                }
            }
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnLevelChanged -= LevelUpHandler;
                //SystemEventManager.MyInstance.OnEquipmentChanged -= OnEquipmentChanged;
                SystemEventManager.MyInstance.OnPlayerUnitSpawn -= HandlePlayerUnitSpawn;
                SystemEventManager.MyInstance.OnPlayerUnitDespawn -= HandlePlayerUnitDespawn;
            }
            eventSubscriptionsInitialized = false;
        }

        public override void OnDisable() {
            base.OnDisable();
            CleanupEventSubscriptions();
        }

        public void OnKillEventHandler(BaseCharacter sourceCharacter, float creditPercent) {
            if (creditPercent == 0) {
                return;
            }
            //Debug.Log(gameObject.name + ": About to gain xp from kill with creditPercent: " + creditPercent);
            GainXP((int)(LevelEquations.GetXPAmountForKill(MyLevel, sourceCharacter.MyCharacterStats.MyLevel) * creditPercent));
        }

        public override void Die() {
            base.Die();
            // Kill the player
            SystemEventManager.TriggerEvent("OnPlayerDeath", new EventParam());
        }

        public override void CalculateRunSpeed() {
            float oldRunSpeed = currentRunSpeed;
            float oldSprintSpeed = currentSprintSpeed;
            base.CalculateRunSpeed();
            if (currentRunSpeed != oldRunSpeed) {
                EventParam eventParam = new EventParam();
                eventParam.FloatParam = currentRunSpeed;
                SystemEventManager.TriggerEvent("OnSetRunSpeed", eventParam);
                eventParam.FloatParam = currentSprintSpeed;
                SystemEventManager.TriggerEvent("OnSetSprintSpeed", eventParam);
            }
            if (currentSprintSpeed != oldSprintSpeed) {
                EventParam eventParam = new EventParam();
                eventParam.FloatParam = currentSprintSpeed;
                SystemEventManager.TriggerEvent("OnSetSprintSpeed", eventParam);
            }
        }


        public void LevelUpHandler(int NewLevel) {
            MessageFeedManager.MyInstance.WriteMessage(string.Format("YOU HAVE REACHED LEVEL {0}!", NewLevel.ToString()));
        }

        public override StatusEffectNode ApplyStatusEffect(StatusEffect statusEffect, BaseCharacter source, CharacterUnit target, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log("Playerstats.ApplyStatusEffect()");
            if (statusEffect == null) {
                //Debug.Log("Playerstats.ApplyStatusEffect(): statusEffect is null!");
            }
            StatusEffectNode _statusEffectNode = base.ApplyStatusEffect(statusEffect, source, target, abilityEffectInput);
            if (statusEffect.MyClassTrait == false) {
                if (_statusEffectNode != null) {
                    UIManager.MyInstance.MyStatusEffectPanelController.SpawnStatusNode(_statusEffectNode, target);
                    if (abilityEffectInput.savedEffect == false) {
                        if (target != null) {
                            CombatTextManager.MyInstance.SpawnCombatText(target.gameObject, statusEffect, true);
                        }
                    }
                }
            }
            return _statusEffectNode;
        }

        public void HandlePlayerUnitSpawn() {
            //Debug.Log("PlayerStats.HandlePlayerUnitSpawn()");
            if (MyBaseCharacter != null && MyBaseCharacter.MyAnimatedUnit != null && MyBaseCharacter.MyAnimatedUnit.MyCharacterAnimator != null) {
                MyBaseCharacter.MyAnimatedUnit.MyCharacterAnimator.OnReviveComplete += ReviveComplete;
            }

            //code to re-apply visual effects when the player loads into a new level
            foreach (StatusEffectNode statusEffectNode in MyStatusEffects.Values) {
                //Debug.Log("PlayerStats.HandlePlayerUnitSpawn(): re-applying effect object for: " + statusEffectNode.MyStatusEffect.MyName);
                statusEffectNode.MyStatusEffect.RawCast(MyBaseCharacter as BaseCharacter, MyBaseCharacter.MyCharacterUnit.gameObject, MyBaseCharacter.MyCharacterUnit.gameObject, new AbilityEffectOutput());
            }
        }

        public void HandlePlayerUnitDespawn() {
            //Debug.Log("PlayerStats.HandlePlayerUnitDespawn()");
            if (MyBaseCharacter != null && MyBaseCharacter.MyAnimatedUnit != null && MyBaseCharacter.MyAnimatedUnit.MyCharacterAnimator != null) {
                MyBaseCharacter.MyAnimatedUnit.MyCharacterAnimator.OnReviveComplete -= ReviveComplete;
            }
        }

        public override void GainLevel() {
            base.GainLevel();
            SystemEventManager.MyInstance.NotifyOnLevelChanged(MyLevel);
        }

        public override void SetLevel(int newLevel) {
            //Debug.Log(gameObject.name + ".PlayerStats.SetLevel(" + newLevel + ")");
            base.SetLevel(newLevel);
            // moving to GainLevel to avoid notifications on character load
            //SystemEventManager.MyInstance.NotifyOnLevelChanged(newLevel);
        }

    }

}