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

        public override void SetCharacterFaction(string newFaction) {
            //Debug.Log(gameObject.name + ".PlayerCharacter.SetCharacterFaction(" + newFaction + ")");
            base.SetCharacterFaction(newFaction);

            if (newFaction != null && newFaction != string.Empty) {
                factionName = newFaction;
                characterFactionManager.SetReputation(newFaction);
            }
        }

        public override void SetCharacterClass(string newCharacterClass) {
            //Debug.Log(gameObject.name + ".PlayerCharacter.SetCharacterFaction(" + newFaction + ")");

            base.SetCharacterClass(newCharacterClass);
            MessageFeedManager.MyInstance.WriteMessage("Changed class to " + newCharacterClass);
        }

        public void JoinFaction(string newFaction) {
            //Debug.Log(gameObject.name + ".PlayerCharacter.Joinfaction(" + newFaction + ")");
            if (newFaction != null && newFaction != string.Empty && SystemResourceManager.MatchResource(newFaction, PlayerManager.MyInstance.MyCharacter.MyFactionName) == false) {
                SetCharacterFaction(newFaction);
                LearnFactionAbilities(newFaction);
            }
        }

        public void ChangeCharacterClass(string newCharacterClass) {
            //Debug.Log(gameObject.name + ".PlayerCharacter.Joinfaction(" + newFaction + ")");
            if (newCharacterClass != null && newCharacterClass != string.Empty && SystemResourceManager.MatchResource(newCharacterClass, PlayerManager.MyInstance.MyCharacter.MyCharacterClassName) == false) {
                SetCharacterClass(newCharacterClass);
                SystemEventManager.MyInstance.NotifyOnPrerequisiteUpdated();
            }
        }


        public void LearnFactionAbilities(string newFaction) {
            //Debug.Log(gameObject.name + ".PlayerCharacter.LearnFactionAbilities(" + newFaction + ")");
            foreach (string abilityName in SystemFactionManager.MyInstance.GetResource(newFaction).MyLearnedAbilityList) {
                //Debug.Log(gameObject.name + ".PlayerCharacter.LearnFactionAbilities(" + newFaction + "); ability name: " + abilityName);
                if (SystemAbilityManager.MyInstance.GetResource(abilityName).MyRequiredLevel <= PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel && PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.HasAbility(abilityName) == false) {
                    //Debug.Log(gameObject.name + ".PlayerCharacter.LearnFactionAbilities(" + newFaction + "); ability name: " + abilityName + " is not learned yet, LEARNING!");
                    PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.LearnAbility(abilityName);
                } else {
                    //Debug.Log(gameObject.name + ".PlayerCharacter.LearnFactionAbilities(" + newFaction + "); ability name: " + abilityName + "; level: " + SystemAbilityManager.MyInstance.GetResource(abilityName).MyRequiredLevel + "; playerlevel: " + PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel + "; hasability: " + (PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.HasAbility(abilityName)));
                }
            }
        }

       

    }

}