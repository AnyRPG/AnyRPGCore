using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PlayerCharacter : BaseCharacter {

        private CharacterRecipeManager playerRecipeManager;

        public CharacterRecipeManager PlayerRecipeManager { get => playerRecipeManager; set => playerRecipeManager = value; }

        private PlayerCurrencyManager playerCurrencyManager;

        public PlayerCurrencyManager MyPlayerCurrencyManager { get => playerCurrencyManager; set => playerCurrencyManager = value; }

        protected override void Awake() {
            //Debug.Log(gameObject.name + ".PlayerCharcter.Awake()");
            base.Awake();

            playerCurrencyManager = GetComponent<PlayerCurrencyManager>();
            if (playerCurrencyManager == null) {
                Debug.Log(gameObject.name + ".PlayerCharcter.Awake(): playerCurrencyManager is null!");
            }

            playerRecipeManager = GetComponent<CharacterRecipeManager>();
            if (playerRecipeManager == null) {
                Debug.Log(gameObject.name + ".PlayerCharcter.Awake(): playerRecipeManager is null!");
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

        public override void SetCharacterClass(CharacterClass newCharacterClass, bool notify = true, bool resetStats = true) {
            //Debug.Log(gameObject.name + ".PlayerCharacter.SetCharacterFaction(" + newFaction + ")");

            CharacterClass oldCharacterClass = characterClass;
            base.SetCharacterClass(newCharacterClass, notify, resetStats);
            if (newCharacterClass == null) {
                // don't print messages for no reason
                return;
            }
            if (notify) {
                SystemEventManager.MyInstance.NotifyOnClassChange(newCharacterClass, oldCharacterClass);
                MessageFeedManager.MyInstance.WriteMessage("Changed class to " + newCharacterClass.DisplayName);
            }
        }

        public void JoinFaction(Faction newFaction) {
            //Debug.Log(gameObject.name + ".PlayerCharacter.Joinfaction(" + newFaction + ")");
            if (newFaction != null && newFaction != PlayerManager.MyInstance.MyCharacter.Faction) {
                SetCharacterFaction(newFaction);
                characterAbilityManager.LearnFactionAbilities(newFaction);
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

    }

}