using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {

    /// <summary>
    /// this class stores snapshots of capabilities to determine what to add or remove when a provider is added or changed
    /// </summary>
    public class CapabilityConsumerSnapshot : ConfiguredClass, ICapabilityConsumer {

        private UnitProfile unitProfile = null;
        private UnitType unitType = null;
        private CharacterRace characterRace = null;
        private CharacterClass characterClass = null;
        private ClassSpecialization classSpecialization = null;
        private Faction faction = null;

        private CapabilityConsumerProcessor capabilityConsumerProcessor = null;

        private List<ICapabilityProvider> capabilityProviders = new List<ICapabilityProvider>();

        public UnitProfile UnitProfile { get => unitProfile; set => unitProfile = value; }
        public UnitType UnitType { get => unitType; set => unitType = value; }
        public CharacterRace CharacterRace { get => characterRace; set => characterRace = value; }
        public CharacterClass CharacterClass { get => characterClass; set => characterClass = value; }
        public ClassSpecialization ClassSpecialization { get => classSpecialization; set => classSpecialization = value; }
        public Faction Faction { get => faction; set => faction = value; }
        public CapabilityConsumerProcessor CapabilityConsumerProcessor { get => capabilityConsumerProcessor; }

        public CapabilityConsumerSnapshot(ICapabilityConsumer capabilityConsumer, SystemGameManager systemGameManager) {
            Configure(systemGameManager);
            capabilityProviders.Add(systemConfigurationManager);
            if (capabilityConsumer.UnitProfile != null) {
                unitProfile = capabilityConsumer.UnitProfile;
                capabilityProviders.Add(capabilityConsumer.UnitProfile);
            }
            if (capabilityConsumer.UnitType != null) {
                unitType = capabilityConsumer.UnitType;
                capabilityProviders.Add(capabilityConsumer.UnitType);
            }
            if (capabilityConsumer.CharacterRace != null) {
                characterRace = capabilityConsumer.CharacterRace;
                capabilityProviders.Add(capabilityConsumer.CharacterRace);
            }
            if (capabilityConsumer.CharacterClass != null) {
                characterClass = capabilityConsumer.CharacterClass;
                capabilityProviders.Add(capabilityConsumer.CharacterClass);
            }
            if (capabilityConsumer.ClassSpecialization != null) {
                classSpecialization = capabilityConsumer.ClassSpecialization;
                capabilityProviders.Add(capabilityConsumer.ClassSpecialization);
            }
            if (capabilityConsumer.Faction != null) {
                faction = capabilityConsumer.Faction;
                capabilityProviders.Add(capabilityConsumer.Faction);
            }
        }

        public CapabilityConsumerSnapshot(SystemGameManager systemGameManager) {
            Configure(systemGameManager);
            //capabilityProviders.Add(systemConfigurationManager);
        }

        public List<StatusEffect> GetTraitList() {
            List<StatusEffect> returnList = new List<StatusEffect>();

            foreach (ICapabilityProvider capabilityProvider in capabilityProviders) {
                if (capabilityProvider != null) {
                    returnList.AddRange(capabilityProvider.GetFilteredCapabilities(this).TraitList);
                }
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
            //Debug.Log("CapabilityConsumerSnapshot.GetAbilityList()");
            List<BaseAbility> returnList = new List<BaseAbility>();

            foreach (ICapabilityProvider capabilityProvider in capabilityProviders) {
                //Debug.Log("CapabilityConsumerSnapshot.GetAbilityList() process capabilityProvder: " + capabilityProvider.DisplayName);
                if (capabilityProvider != null) {
                    returnList.AddRange(capabilityProvider.GetFilteredCapabilities(this).AbilityList);
                }
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
        public List<BaseAbility> GetAbilitiesToAdd(CapabilityConsumerSnapshot capabilityConsumerSnapshot, CharacterAbilityManager sourceAbilityManager) {
            List<BaseAbility> returnList = new List<BaseAbility>();

            List<BaseAbility> currentList = GetAbilityList();

            returnList.AddRange(capabilityConsumerSnapshot.GetAbilityList().Except(currentList));
            foreach (BaseAbility baseAbility in GetAbilityList()) {
                if (sourceAbilityManager.RawAbilityList.ContainsValue(baseAbility) == false) {
                    sourceAbilityManager.LearnAbility(baseAbility);
                }
            }

            return returnList;
        }

    }

}