using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PlayerCharacter : BaseCharacter {

        private PlayerCurrencyManager playerCurrencyManager;

        public PlayerCurrencyManager MyPlayerCurrencyManager { get => playerCurrencyManager; set => playerCurrencyManager = value; }

        protected override void Awake() {
            //Debug.Log(gameObject.name + ".PlayerCharcter.Awake()");
            base.Awake();

            playerCurrencyManager = GetComponent<PlayerCurrencyManager>();
            if (playerCurrencyManager == null) {
                Debug.Log(gameObject.name + ".PlayerCharcter.Awake(): playerCurrencyManager is null!");
            }

        }

        public override void SetCharacterFaction(Faction newFaction) {
            //Debug.Log(gameObject.name + ".PlayerCharacter.SetCharacterFaction(" + newFaction + ")");
            base.SetCharacterFaction(newFaction);

            if (newFaction != null) {
                faction = newFaction;
                characterFactionManager.SetReputation(newFaction);
            }
        }

        public override void SetCharacterClass(CharacterClass newCharacterClass, bool notify = true) {
            //Debug.Log(gameObject.name + ".PlayerCharacter.SetCharacterFaction(" + newFaction + ")");

            CharacterClass oldCharacterClass = characterClass;
            base.SetCharacterClass(newCharacterClass, notify);
            if (newCharacterClass == null) {
                // don't print messages for no reason
                return;
            }
            if (notify) {
                SystemEventManager.MyInstance.NotifyOnClassChange(newCharacterClass, oldCharacterClass);
                MessageFeedManager.MyInstance.WriteMessage("Changed class to " + newCharacterClass.MyDisplayName);
            }
        }

        public void JoinFaction(Faction newFaction) {
            //Debug.Log(gameObject.name + ".PlayerCharacter.Joinfaction(" + newFaction + ")");
            if (newFaction != null && newFaction != PlayerManager.MyInstance.MyCharacter.MyFaction) {
                SetCharacterFaction(newFaction);
                LearnFactionAbilities(newFaction);
            }
        }

        public void ChangeClassSpecialization(ClassSpecialization newClassSpecialization) {
            //Debug.Log(gameObject.name + ".PlayerCharacter.Joinfaction(" + newFaction + ")");
            if (newClassSpecialization != null && newClassSpecialization != PlayerManager.MyInstance.MyCharacter.CharacterClass) {
                SetClassSpecialization(newClassSpecialization);
            }
        }

        public void ChangeCharacterClass(CharacterClass newCharacterClass) {
            //Debug.Log(gameObject.name + ".PlayerCharacter.Joinfaction(" + newFaction + ")");
            if (newCharacterClass != null && newCharacterClass != PlayerManager.MyInstance.MyCharacter.CharacterClass) {
                SetCharacterClass(newCharacterClass);
            }
        }


        public void LearnFactionAbilities(Faction newFaction) {
            //Debug.Log(gameObject.name + ".PlayerCharacter.LearnFactionAbilities(" + newFaction + ")");
            foreach (BaseAbility baseAbility in newFaction.MyLearnedAbilityList) {
                //Debug.Log(gameObject.name + ".PlayerCharacter.LearnFactionAbilities(" + newFaction + "); ability name: " + abilityName);
                if (baseAbility.MyRequiredLevel <= PlayerManager.MyInstance.MyCharacter.CharacterStats.Level && PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.HasAbility(baseAbility) == false) {
                    //Debug.Log(gameObject.name + ".PlayerCharacter.LearnFactionAbilities(" + newFaction + "); ability name: " + abilityName + " is not learned yet, LEARNING!");
                    PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.LearnAbility(baseAbility);
                } else {
                    //Debug.Log(gameObject.name + ".PlayerCharacter.LearnFactionAbilities(" + newFaction + "); ability name: " + abilityName + "; level: " + SystemAbilityManager.MyInstance.GetResource(abilityName).MyRequiredLevel + "; playerlevel: " + PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel + "; hasability: " + (PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.HasAbility(abilityName)));
                }
            }
        }

       

    }

}