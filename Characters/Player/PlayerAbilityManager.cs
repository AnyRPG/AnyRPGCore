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
            SystemEventManager.MyInstance.OnEquipmentChanged += HandleEquipmentChanged;
            SystemEventManager.MyInstance.OnPlayerUnitSpawn += HandleCharacterUnitSpawn;
            SystemEventManager.MyInstance.OnPlayerUnitDespawn += HandleCharacterUnitDespawn;
            if (PlayerManager.MyInstance.MyPlayerUnitSpawned) {
                //Debug.Log(gameObject.name + ".PlayerAbilityManager.CreateEventSubscriptions() Player is already spawned");
                HandleCharacterUnitSpawn();
            }
            if (KeyBindManager.MyInstance != null && KeyBindManager.MyInstance.MyKeyBinds != null && KeyBindManager.MyInstance.MyKeyBinds.ContainsKey("CANCEL")) {
                KeyBindManager.MyInstance.MyKeyBinds["CANCEL"].OnKeyPressedHandler += OnEscapeKeyPressedHandler;
            }

        }

        public override void CleanupEventSubscriptions() {
            if (!eventSubscriptionsInitialized) {
                return;
            }
            base.CleanupEventSubscriptions();
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnLevelChanged -= UpdateAbilityList;
                SystemEventManager.MyInstance.OnEquipmentChanged -= HandleEquipmentChanged;
                SystemEventManager.MyInstance.OnPlayerUnitSpawn -= HandleCharacterUnitSpawn;
                SystemEventManager.MyInstance.OnPlayerUnitDespawn -= HandleCharacterUnitDespawn;
            }
            // that next code would have never been necessary because that handler was never set : TEST THAT ESCAPE CANCELS SPELLCASTING - THAT METHOD IS NEVER SET
            if (KeyBindManager.MyInstance != null && KeyBindManager.MyInstance.MyKeyBinds != null && KeyBindManager.MyInstance.MyKeyBinds.ContainsKey("CANCEL")) {
                KeyBindManager.MyInstance.MyKeyBinds["CANCEL"].OnKeyPressedHandler -= OnEscapeKeyPressedHandler;
            }

        }

        /// <summary>
        /// Stop casting if the escape key is pressed
        /// </summary>
        public void OnEscapeKeyPressedHandler() {
            //Debug.Log("Received Escape Key Pressed Handler");
            baseCharacter.MyCharacterAbilityManager.StopCasting();

        }


        public override void OnDisable() {
            base.OnDisable();
            CleanupEventSubscriptions();
        }

        public void AbilityLearnedHandler(BaseAbility newAbility) {
            //Debug.Log("PlayerAbilityManager.AbilityLearnedHandler()");
            if (MessageFeedManager.MyInstance != null) {
                MessageFeedManager.MyInstance.WriteMessage(string.Format("Learned New Ability: {0}", newAbility.MyName));
            }
        }

        public override bool PerformCombatCheck(IAbility ability) {
            bool returnResult = base.PerformCombatCheck(ability);
            if (!returnResult) {
                CombatLogUI.MyInstance.WriteCombatMessage("The ability " + ability.MyName + " can only be cast while out of combat");
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
                CombatLogUI.MyInstance.WriteCombatMessage("You have not learned the ability " + ability.MyName + " yet");
                //Debug.Log("You have not learned the ability " + ability.MyName + " yet");
                //Debug.Log("ability.MyUseableWithoutLearning: " + ability.MyUseableWithoutLearning + "; abilityList.Contains(" + keyName + "): " + abilityList.ContainsKey(keyName));
            }
            return returnResult;
        }

        public override bool PerformManaCheck(IAbility ability) {
            bool returnResult = base.PerformManaCheck(ability);
            if (!returnResult) {
                CombatLogUI.MyInstance.WriteCombatMessage("Not enough mana to perform " + ability.MyName + " at a cost of " + ability.MyAbilityManaCost.ToString());
                //Debug.Log("Not enough mana to perform " + ability.MyName + " at a cost of " + ability.MyAbilityManaCost.ToString());
            }
            return returnResult;
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
            //Debug.Log("PlayerAbilityManager.LoadAbility(" + abilityName + ")");
            IAbility ability = SystemAbilityManager.MyInstance.GetResource(abilityName) as IAbility;
            if (ability != null) {
                // if we renamed an ability, old save data could load a null.  prevent invalid abilities from loading.
                string keyName = SystemResourceManager.prepareStringForMatch(abilityName);
                if (!abilityList.ContainsKey(keyName)) {
                    //Debug.Log("PlayerAbilityManager.LoadAbility(" + abilityName + "): found it!");
                    if (ability is AnimatedAbility && (ability as AnimatedAbility).MyIsAutoAttack == true) {
                        UnLearnDefaultAutoAttackAbility();
                    }

                    abilityList[keyName] = ability;
                }
            }
        }


        public override void UpdateAbilityList(int newLevel) {
            //Debug.Log(gameObject.name + ".PlayerAbilitymanager.UpdateAbilityList(). length: " + abilityList.Count);
            base.UpdateAbilityList(newLevel);
            foreach (BaseAbility ability in SystemAbilityManager.MyInstance.GetResourceList()) {
                if (ability.MyRequiredLevel <= newLevel && ability.MyAutoLearn == true) {
                    if (!HasAbility(ability)) {
                        LearnAbility(ability);
                    } else {
                        //Debug.Log(ability.MyName + " already known, no need to re-learn");
                    }
                }
            }
            if (PlayerManager.MyInstance.MyCharacter.MyFaction != null) {
                PlayerManager.MyInstance.MyCharacter.LearnFactionAbilities(PlayerManager.MyInstance.MyCharacter.MyFaction);
            }
        }

        public override void PerformAbility(IAbility ability, GameObject target, Vector3 groundTarget) {
            //Debug.Log(gameObject.name + ".PlayerAbilityManager.PerformAbility(" + ability.MyName + ")");
            base.PerformAbility(ability, target, groundTarget);
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

        public override void HandleCharacterUnitSpawn() {
            if (MyBaseCharacter != null && MyBaseCharacter.MyAnimatedUnit != null) {
                PlayerUnitMovementController movementController = (MyBaseCharacter.MyAnimatedUnit as AnimatedPlayerUnit).MyPlayerUnitMovementController;
                //CharacterMotor characterMotor = MyBaseCharacter.MyAnimatedUnit.MyCharacterMotor;
                if (movementController != null) {
                    //Debug.Log("CharacterAbilityManager.OnCharacterUnitSpawn(): movementController is not null");
                    movementController.OnMovement += HandleManualMovement;
                }
            }
            base.HandleCharacterUnitSpawn();

        }

        public override void HandleCharacterUnitDespawn() {
            if (MyBaseCharacter != null && MyBaseCharacter.MyAnimatedUnit != null) {

                PlayerUnitMovementController movementController = (MyBaseCharacter.MyAnimatedUnit as AnimatedPlayerUnit).MyPlayerUnitMovementController;
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

    }

}