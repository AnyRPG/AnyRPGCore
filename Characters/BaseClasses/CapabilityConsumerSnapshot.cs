using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    public class CapabilityConsumerSnapshot : ICapabilityConsumer {

        private UnitProfile unitProfile = null;
        private UnitType unitType = null;
        private CharacterRace characterRace = null;
        private CharacterClass characterClass = null;
        private ClassSpecialization classSpecialization = null;
        private Faction faction = null;

        private CapabilityConsumerProcessor capabilityConsumerProcessor = null;

        public Faction Faction { get => faction; set => faction = value; }
        public UnitType UnitType { get => unitType; set => unitType = value; }
        public CharacterRace CharacterRace { get => characterRace; set => characterRace = value; }
        public CharacterClass CharacterClass { get => characterClass; set => characterClass = value; }
        public ClassSpecialization ClassSpecialization { get => classSpecialization; set => classSpecialization = value; }
        public UnitProfile UnitProfile { get => unitProfile; set => unitProfile = value; }
        public CapabilityConsumerProcessor CapabilityConsumerProcessor { get => capabilityConsumerProcessor; }

        public CapabilityConsumerSnapshot(ICapabilityConsumer capabilityConsumer) {
            faction = capabilityConsumer.Faction;
            unitType = capabilityConsumer.UnitType;
            characterRace = capabilityConsumer.CharacterRace;
            characterClass = capabilityConsumer.CharacterClass;
            classSpecialization = capabilityConsumer.ClassSpecialization;
        }

        public List<StatusEffect> GetTraitList() {
            List<StatusEffect> returnList = new List<StatusEffect>();

            if (faction != null) {
                returnList.AddRange(faction.GetFilteredCapabilities(this).TraitList);
            }

            if (unitType != null) {
                returnList.AddRange(unitType.GetFilteredCapabilities(this).TraitList);
            }

            if (characterRace != null) {
                returnList.AddRange(characterRace.GetFilteredCapabilities(this).TraitList);
            }

            if (characterClass != null) {
                returnList.AddRange(characterClass.GetFilteredCapabilities(this).TraitList);
            }

            if (classSpecialization != null) {
                returnList.AddRange(classSpecialization.GetFilteredCapabilities(this).TraitList);
            }

            return returnList;
        }

        /// <summary>
        /// return a list of traits to remove when provided with an incoming capability consumer
        /// </summary>
        /// <param name="capabilityConsumerSnapshot"></param>
        /// <returns></returns>
        public List<StatusEffect> GetTraitsToRemove(CapabilityConsumerSnapshot capabilityConsumerSnapshot) {
            List<StatusEffect> returnList = new List<StatusEffect>();

            List<StatusEffect> currentList = GetTraitList();

            returnList.AddRange(currentList.Except(capabilityConsumerSnapshot.GetTraitList()));

            return returnList;
        }

        /// <summary>
        /// return a list of traits to add when provided with an incoming capability consumer
        /// </summary>
        /// <param name="capabilityConsumerSnapshot"></param>
        /// <returns></returns>
        public List<StatusEffect> GetTraitsToAdd(CapabilityConsumerSnapshot capabilityConsumerSnapshot) {
            List<StatusEffect> returnList = new List<StatusEffect>();

            List<StatusEffect> currentList = GetTraitList();

            returnList.AddRange(capabilityConsumerSnapshot.GetTraitList().Except(currentList));

            return returnList;
        }

        public List<BaseAbility> GetAbilityList() {
            List<BaseAbility> returnList = new List<BaseAbility>();

            if (faction != null) {
                returnList.AddRange(faction.GetFilteredCapabilities(this).AbilityList);
            }
            if (unitType != null) {
                returnList.AddRange(unitType.GetFilteredCapabilities(this).AbilityList);
            }
            if (characterRace != null) {
                returnList.AddRange(characterRace.GetFilteredCapabilities(this).AbilityList);
            }
            if (characterClass != null) {
                returnList.AddRange(characterClass.GetFilteredCapabilities(this).AbilityList);
            }
            if (classSpecialization != null) {
                returnList.AddRange(classSpecialization.GetFilteredCapabilities(this).AbilityList);
            }

            return returnList;
        }

        /// <summary>
        /// return a list of abilities to remove when provided with an incoming capability consumer
        /// </summary>
        /// <param name="capabilityConsumerSnapshot"></param>
        /// <returns></returns>
        public List<BaseAbility> GetAbilitiesToRemove(CapabilityConsumerSnapshot capabilityConsumerSnapshot) {
            List<BaseAbility> returnList = new List<BaseAbility>();

            List<BaseAbility> currentList = GetAbilityList();

            returnList.AddRange(currentList.Except(capabilityConsumerSnapshot.GetAbilityList()));

            return returnList;
        }

        /// <summary>
        /// return a list of abilities to add when provided with an incoming capability consumer
        /// </summary>
        /// <param name="capabilityConsumerSnapshot"></param>
        /// <returns></returns>
        public List<BaseAbility> GetAbilitiesToAdd(CapabilityConsumerSnapshot capabilityConsumerSnapshot) {
            List<BaseAbility> returnList = new List<BaseAbility>();

            List<BaseAbility> currentList = GetAbilityList();

            returnList.AddRange(capabilityConsumerSnapshot.GetAbilityList().Except(currentList));

            return returnList;
        }

    }

}