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

        public override void SetCharacterClass(CharacterClass newCharacterClass) {
            //Debug.Log(gameObject.name + ".PlayerCharacter.SetCharacterFaction(" + newFaction + ")");

            base.SetCharacterClass(newCharacterClass);
            if (newCharacterClass == null) {
                // don't print messages for no reason
                return;
            }
            MessageFeedManager.MyInstance.WriteMessage("Changed class to " + newCharacterClass.MyName);
        }

        public void JoinFaction(Faction newFaction) {
            //Debug.Log(gameObject.name + ".PlayerCharacter.Joinfaction(" + newFaction + ")");
            if (newFaction != null && newFaction != PlayerManager.MyInstance.MyCharacter.MyFaction) {
                SetCharacterFaction(newFaction);
                LearnFactionAbilities(newFaction);
            }
        }

        public void ChangeCharacterClass(CharacterClass newCharacterClass) {
            //Debug.Log(gameObject.name + ".PlayerCharacter.Joinfaction(" + newFaction + ")");
            if (newCharacterClass != null && newCharacterClass != PlayerManager.MyInstance.MyCharacter.MyCharacterClass) {
                SetCharacterClass(newCharacterClass);
                SystemEventManager.MyInstance.NotifyOnPrerequisiteUpdated();
            }
        }


        public void LearnFactionAbilities(Faction newFaction) {
            //Debug.Log(gameObject.name + ".PlayerCharacter.LearnFactionAbilities(" + newFaction + ")");
            foreach (BaseAbility baseAbility in newFaction.MyLearnedAbilityList) {
                //Debug.Log(gameObject.name + ".PlayerCharacter.LearnFactionAbilities(" + newFaction + "); ability name: " + abilityName);
                if (baseAbility.MyRequiredLevel <= PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel && PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.HasAbility(baseAbility) == false) {
                    //Debug.Log(gameObject.name + ".PlayerCharacter.LearnFactionAbilities(" + newFaction + "); ability name: " + abilityName + " is not learned yet, LEARNING!");
                    PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.LearnAbility(baseAbility);
                } else {
                    //Debug.Log(gameObject.name + ".PlayerCharacter.LearnFactionAbilities(" + newFaction + "); ability name: " + abilityName + "; level: " + SystemAbilityManager.MyInstance.GetResource(abilityName).MyRequiredLevel + "; playerlevel: " + PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel + "; hasability: " + (PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.HasAbility(abilityName)));
                }
            }
        }

       

    }

}