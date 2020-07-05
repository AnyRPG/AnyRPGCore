using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PlayerAbilityManager : CharacterAbilityManager {

        public System.Action<IAbility> OnPerformAbility = delegate { };
        public System.Action<IAbility> OnAttemptPerformAbility = delegate { };

        public override void CreateEventSubscriptions() {
            //Debug.Log(gameObject.name + ".PlayerAbilityManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            base.CreateEventSubscriptions();
            SystemEventManager.MyInstance.OnLevelChanged += UpdateAbilityList;
            SystemEventManager.StartListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            SystemEventManager.MyInstance.OnPlayerUnitDespawn += HandleCharacterUnitDespawn;
            if (PlayerManager.MyInstance.MyPlayerUnitSpawned) {
                //Debug.Log(gameObject.name + ".PlayerAbilityManager.CreateEventSubscriptions() Player is already spawned");
                ProcessCharacterUnitSpawn();
            }
            if (KeyBindManager.MyInstance != null && KeyBindManager.MyInstance.MyKeyBinds != null && KeyBindManager.MyInstance.MyKeyBinds.ContainsKey("CANCEL")) {
                KeyBindManager.MyInstance.MyKeyBinds["CANCEL"].OnKeyPressedHandler += OnEscapeKeyPressedHandler;
            }

        }

        public override void ActivateTargettingMode(BaseAbility baseAbility, GameObject target) {
            //Debug.Log("CharacterAbilityManager.ActivateTargettingMode()");

            base.ActivateTargettingMode(baseAbility, target);

            targettingModeActive = true;
            CastTargettingManager.MyInstance.EnableProjector(baseAbility);
        }

        public override void CleanupEventSubscriptions() {
            if (!eventSubscriptionsInitialized) {
                return;
            }
            base.CleanupEventSubscriptions();
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnLevelChanged -= UpdateAbilityList;
                SystemEventManager.StopListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
                SystemEventManager.MyInstance.OnPlayerUnitDespawn -= HandleCharacterUnitDespawn;
            }
            // that next code would have never been necessary because that handler was never set : TEST THAT ESCAPE CANCELS SPELLCASTING - THAT METHOD IS NEVER SET
            if (KeyBindManager.MyInstance != null && KeyBindManager.MyInstance.MyKeyBinds != null && KeyBindManager.MyInstance.MyKeyBinds.ContainsKey("CANCEL")) {
                KeyBindManager.MyInstance.MyKeyBinds["CANCEL"].OnKeyPressedHandler -= OnEscapeKeyPressedHandler;
            }

        }

        public void HandlePlayerUnitSpawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log(gameObject.name + ".InanimateUnit.HandlePlayerUnitSpawn()");
            ProcessCharacterUnitSpawn();
        }


        /// <summary>
        /// Stop casting if the escape key is pressed
        /// </summary>
        public void OnEscapeKeyPressedHandler() {
            //Debug.Log("Received Escape Key Pressed Handler");
            baseCharacter.CharacterAbilityManager.StopCasting();

        }


        public override void OnDisable() {
            base.OnDisable();
            CleanupEventSubscriptions();
        }

        public override bool PerformAnimatedAbilityCheck(AnimatedAbility animatedAbility) {
            bool returnresult = base.PerformAnimatedAbilityCheck(animatedAbility);
            if (!returnresult) {
                if (PlayerManager.MyInstance.MyPlayerUnitSpawned == true && CombatLogUI.MyInstance != null) {
                    CombatLogUI.MyInstance.WriteCombatMessage("Cannot use " + (animatedAbility.DisplayName == null ? "null" : animatedAbility.DisplayName) + ". Waiting for another ability to finish.");
                }
            }
            return returnresult;
        }

        public void AbilityLearnedHandler(BaseAbility newAbility) {
            //Debug.Log("PlayerAbilityManager.AbilityLearnedHandler()");
            if (MessageFeedManager.MyInstance != null) {
                MessageFeedManager.MyInstance.WriteMessage(string.Format("Learned New Ability: {0}", newAbility.DisplayName));
            }
        }

        public override bool IsTargetInAbilityRange(BaseAbility baseAbility, GameObject target, AbilityEffectContext abilityEffectContext = null) {
            bool returnResult = base.IsTargetInAbilityRange(baseAbility, target, abilityEffectContext);
            if (!returnResult && abilityEffectContext != null && abilityEffectContext.baseAbility != null) {
                if (CombatLogUI.MyInstance != null) {
                    CombatLogUI.MyInstance.WriteCombatMessage(target.name + " is out of range of " + (baseAbility.DisplayName == null ? "null" : baseAbility.DisplayName));
                }
            }
            return returnResult;
        }

        public override bool IsTargetInMaxRange(GameObject target, float maxRange, ITargetable targetable, AbilityEffectContext abilityEffectContext) {
            bool returnResult = base.IsTargetInMaxRange(target, maxRange, targetable, abilityEffectContext);
            return returnResult;
        }

        public override bool PerformCombatCheck(IAbility ability) {
            bool returnResult = base.PerformCombatCheck(ability);
            if (!returnResult) {
                CombatLogUI.MyInstance.WriteCombatMessage("The ability " + ability.DisplayName + " can only be cast while out of combat");
                //Debug.Log("The ability " + ability.MyName + " can only be cast while out of combat");
            }
            return returnResult;
        }

        public override bool PerformCooldownCheck(IAbility ability) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.CanCastAbility(" + ability.MyName + "): current GCD: " + MyRemainingGlobalCoolDown);
            bool returnResult = base.PerformCooldownCheck(ability);
            if (!returnResult) {
                //CombatLogUI.MyInstance.WriteCombatMessage("The ability " + ability.MyName + " is still on cooldown or there is an active global cooldown");
                //Debug.Log("The ability " + ability.MyName + " is still on cooldown or there is an active global cooldown");
                //CombatLogUI.MyInstance.WriteCombatMessage(ability.MyName + " is on cooldown: " + SystemAbilityManager.MyInstance.GetResource(ability.MyName).MyRemainingCoolDown);
                // write some common notify method here that only has content in it in playerabilitymanager to show messages so don't get spammed with npc messages
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.CanCastAbility(" + ability.MyName + "): gcd: " + MyRemainingGlobalCoolDown + "; key in dictionary: " + abilityCoolDownDictionary.ContainsKey(ability.MyName));
                /*
                if (abilityCoolDownDictionary.ContainsKey(ability.MyName)) {
                    Debug.Log(abilityCoolDownDictionary[ability.MyName].MyRemainingCoolDown);
                }
                */
            }
            return returnResult;
        }

        public override bool PerformLearnedCheck(IAbility ability) {
            bool returnResult = base.PerformLearnedCheck(ability);
            if (!returnResult) {
                CombatLogUI.MyInstance.WriteCombatMessage("You have not learned the ability " + ability.DisplayName + " yet");
                //Debug.Log("You have not learned the ability " + ability.MyName + " yet");
                //Debug.Log("ability.MyUseableWithoutLearning: " + ability.MyUseableWithoutLearning + "; abilityList.Contains(" + keyName + "): " + abilityList.ContainsKey(keyName));
            }
            return returnResult;
        }

        public override bool PerformPowerResourceCheck(IAbility ability) {
            bool returnResult = base.PerformPowerResourceCheck(ability);
            if (!returnResult) {
                CombatLogUI.MyInstance.WriteCombatMessage("Not enough " + ability.PowerResource.DisplayName + " to perform " + ability.DisplayName + " at a cost of " + ability.GetResourceCost(this));
            }
            return returnResult;
        }

        public override void UnLearnClassAbilities(CharacterClass characterClass, bool updateActionBars = false) {
            
            // to avoid a bunch of unnecessary loops of update action bars as abilities are unlearned, suppress the update
            base.UnLearnClassAbilities(characterClass, updateActionBars);

            // now perform a single action bar update
            UIManager.MyInstance.MyActionBarManager.UpdateVisuals(true);
        }

        public override void UnlearnAbility(BaseAbility oldAbility, bool updateActionBars = true) {
            base.UnlearnAbility(oldAbility);
            if (updateActionBars) {
                // attemp to remove from bars
                UIManager.MyInstance.MyActionBarManager.UpdateVisuals(true);
            }
        }

        public override bool LearnAbility(BaseAbility newAbility) {
            //Debug.Log(gameObject.name + "PlayerAbilityManager.LearnAbility()");
            bool returnValue = base.LearnAbility(newAbility);
            if (returnValue) {
                //Debug.Log(gameObject.name + "PlayerAbilityManager.LearnAbility() returnvalue is true");
                SystemEventManager.MyInstance.NotifyOnAbilityListChanged(newAbility);
                newAbility.NotifyOnLearn();
            } else {
                //Debug.Log(gameObject.name + "PlayerAbilityManager.LearnAbility() returnvalue was false");
            }
            return returnValue;
        }

        public override void BeginAbilityCoolDown(BaseAbility baseAbility, float coolDownLength = -1) {
            //Debug.Log("PlayerAbilityManager.BeginAbilityCoolDown(" + baseAbility.MyName + ", " + coolDownLength + ")");
            base.BeginAbilityCoolDown(baseAbility, coolDownLength);
        }

        public void LoadAbility(string abilityName) {
            //Debug.Log(gameObject.name + ".PlayerAbilityManager.LoadAbility(" + abilityName + ")");
            IAbility ability = SystemAbilityManager.MyInstance.GetResource(abilityName) as IAbility;
            if (ability != null) {
                // if we renamed an ability, old save data could load a null.  prevent invalid abilities from loading.
                bool isAutoAttack = false;
                if (ability is AnimatedAbility && (ability as AnimatedAbility).IsAutoAttack) {
                    isAutoAttack = true;
                }
                if (isAutoAttack && autoAttackAbility != null) {
                    // can't learn 2 auto-attacks
                    return;
                }

                string keyName = SystemResourceManager.prepareStringForMatch(abilityName);
                if (!abilityList.ContainsKey(keyName)) {
                    //Debug.Log("PlayerAbilityManager.LoadAbility(" + abilityName + "): found it!");
                    if (ability is AnimatedAbility && (ability as AnimatedAbility).IsAutoAttack == true) {
                        UnLearnDefaultAutoAttackAbility();
                        //Debug.Log(gameObject.name + ".PlayerAbilityManager.LoadAbility(" + abilityName + "): is auto-attack!");
                        autoAttackAbility = ability as BaseAbility;
                    }
                    abilityList[keyName] = ability;
                }
            }
        }


        public override void UpdateAbilityList(int newLevel) {
            //Debug.Log(gameObject.name + ".PlayerAbilitymanager.UpdateAbilityList(). length: " + abilityList.Count);
            base.UpdateAbilityList(newLevel);

            LearnSystemAbilities();

            LearnUnitProfileAbilities();

            if (baseCharacter.CharacterClass != null) {
                LearnClassAbilities(baseCharacter.CharacterClass);
            }

            if (baseCharacter.ClassSpecialization != null) {
                LearnSpecializationAbilities(baseCharacter.ClassSpecialization);
            }

            if (baseCharacter.MyFaction != null) {
                LearnFactionAbilities(baseCharacter.MyFaction);
            }
        }

        public override void PerformAbility(IAbility ability, GameObject target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(gameObject.name + ".PlayerAbilityManager.PerformAbility(" + ability.MyName + ")");
            base.PerformAbility(ability, target, abilityEffectContext);
            // DON'T DO GCD ON CASTS THAT HAVE TIME BECAUSE THEIR CAST TIME WAS ALREADY A TYPE OF GLOBAL COOLDOWN
            OnPerformAbility(ability);
            SystemEventManager.MyInstance.NotifyOnAbilityUsed(ability as BaseAbility);
            (ability as BaseAbility).NotifyOnAbilityUsed();
        }

        public override void NotifyAttemptPerformAbility(IAbility ability) {
            OnAttemptPerformAbility(ability);
        }


        public override void CleanupCoroutines() {
            // called from base.ondisable
            base.CleanupCoroutines();
        }

        public override void StopCasting() {
            //Debug.Log(gameObject.name + ".PlayerAbilityManager.StopCasting()");
            base.StopCasting();
        }

        public override void ProcessCharacterUnitSpawn() {
            if (BaseCharacter != null && BaseCharacter.AnimatedUnit != null) {
                PlayerUnitMovementController movementController = (BaseCharacter.AnimatedUnit as AnimatedPlayerUnit).MyPlayerUnitMovementController;
                //CharacterMotor characterMotor = MyBaseCharacter.MyAnimatedUnit.MyCharacterMotor;
                if (movementController != null) {
                    //Debug.Log("CharacterAbilityManager.OnCharacterUnitSpawn(): movementController is not null");
                    movementController.OnMovement += HandleManualMovement;
                }
            }
            base.ProcessCharacterUnitSpawn();

        }

        public override void HandleCharacterUnitDespawn() {
            if (BaseCharacter != null && BaseCharacter.AnimatedUnit != null) {

                PlayerUnitMovementController movementController = (BaseCharacter.AnimatedUnit as AnimatedPlayerUnit).MyPlayerUnitMovementController;
                if (movementController != null) {
                    movementController.OnMovement -= HandleManualMovement;
                }
            }
            base.HandleCharacterUnitDespawn();
        }

        protected override void BeginAbilityCommon(IAbility ability, GameObject target) {
            //Debug.Log(gameObject.name + ".PlayerAbilityManager.BeginAbilityCommon(" + ability.MyName + ", " + (target == null ? "null" : target.name) + ")");
            base.BeginAbilityCommon(ability, target);
        }

        public override void ProcessAbilityCoolDowns(AnimatedAbility baseAbility, float animationLength, float abilityCoolDown) {
            if (SystemConfigurationManager.MyInstance.MyAllowAutoAttack == false || !baseAbility.IsAutoAttack) {
                base.ProcessAbilityCoolDowns(baseAbility, animationLength, abilityCoolDown);
            }
        }

    }

}