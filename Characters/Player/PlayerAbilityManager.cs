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

        public override bool LearnAbility(BaseAbility newAbility) {
            //Debug.Log(gameObject.name + "PlayerAbilityManager.LearnAbility()");
            bool returnValue = base.LearnAbility(newAbility);
            if (returnValue) {
                //Debug.Log(gameObject.name + "PlayerAbilityManager.LearnAbility() returnvalue is true");
                SystemEventManager.MyInstance.NotifyOnAbilityListChanged(newAbility);
            } else {
                //Debug.Log(gameObject.name + "PlayerAbilityManager.LearnAbility() returnvalue was false");
            }
            return returnValue;
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
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.PerformAbility(" + ability.MyName + ")");
            base.PerformAbility(ability, target, groundTarget);
            // DON'T DO GCD ON CASTS THAT HAVE TIME BECAUSE THEIR CAST TIME WAS ALREADY A TYPE OF GLOBAL COOLDOWN
            OnPerformAbility(ability);
            SystemEventManager.MyInstance.NotifyOnAbilityUsed(ability as BaseAbility);
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

    }

}