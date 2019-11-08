using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PlayerStats : CharacterStats {
        protected override void Awake() {
            //Debug.Log(gameObject.name + ".PlayerStats.Awake()");
            base.Awake();
            baseCharacter = GetComponent<PlayerCharacter>() as ICharacter;
            // DO THIS HERE SO WE CAN CATCH EQUIPMENT EVENTS
            CreateEventReferences();
        }

        public override void Start() {
            //Debug.Log(gameObject.name + ".PlayerStats.Start()");
            base.Start();
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

        public override void CreateEventReferences() {
            //Debug.Log(gameObject.name + ".PlayerStats.CreateEventReferences()");
            if (eventReferencesInitialized) {
                //if (eventReferencesInitialized || !startHasRun) {
                return;
            }
            base.CreateEventReferences();
            //SystemEventManager.MyInstance.OnEquipmentChanged += OnEquipmentChanged;
            SystemEventManager.MyInstance.OnLevelChanged += LevelUpHandler;
            SystemEventManager.MyInstance.OnPlayerUnitSpawn += HandlePlayerUnitSpawn;
            SystemEventManager.MyInstance.OnPlayerUnitDespawn += HandlePlayerUnitDespawn;
            eventReferencesInitialized = true;
        }

        public override void CleanupEventReferences() {
            if (!eventReferencesInitialized) {
                return;
            }
            base.CleanupEventReferences();
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
            eventReferencesInitialized = false;
        }

        public override void OnDisable() {
            base.OnDisable();
            CleanupEventReferences();
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
            SystemEventManager.MyInstance.NotifyOnPlayerDeath();
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
            if (_statusEffectNode != null) {
                UIManager.MyInstance.MyStatusEffectPanelController.SpawnStatusNode(_statusEffectNode, target);
                CombatTextManager.MyInstance.SpawnCombatText(target.gameObject, statusEffect, true);
            }
            return _statusEffectNode;
        }

        public void HandlePlayerUnitSpawn() {
            //Debug.Log("PlayerStats.HandlePlayerUnitSpawn()");
            MyBaseCharacter.MyCharacterUnit.MyCharacterAnimator.OnReviveComplete += ReviveComplete;

            //code to re-apply visual effects when the player loads into a new level
            foreach (StatusEffectNode statusEffectNode in MyStatusEffects.Values) {
                //Debug.Log("PlayerStats.HandlePlayerUnitSpawn(): re-applying effect object for: " + statusEffect.MyName);
                statusEffectNode.MyStatusEffect.RawCast(MyBaseCharacter as BaseCharacter, MyBaseCharacter.MyCharacterUnit.gameObject, MyBaseCharacter.MyCharacterUnit.gameObject, new AbilityEffectOutput());
            }
        }

        public void HandlePlayerUnitDespawn() {
            //Debug.Log("PlayerStats.HandlePlayerUnitDespawn()");
            MyBaseCharacter.MyCharacterUnit.MyCharacterAnimator.OnReviveComplete -= ReviveComplete;
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