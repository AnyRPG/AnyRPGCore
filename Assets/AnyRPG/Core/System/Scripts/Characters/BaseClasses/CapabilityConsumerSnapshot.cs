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
            // configured class initialization
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
            //Debug.Log("CapabilityConsumerSnapshot()");
            Configure(systemGameManager);
            //capabilityProviders.Add(systemConfigurationManager);
        }

        public List<UnitType> GetValidPetTypeList() {
            List<UnitType> returnList = new List<UnitType>();

            foreach (ICapabilityProvider capabilityProvider in capabilityProviders) {
                if (capabilityProvider != null) {
                    returnList.AddRange(capabilityProvider.GetFilteredCapabilities(this).ValidPetTypeList);
                }
            }

            return returnList;
        }

        public List<UnitProfile> GetStartingPetList() {
            List<UnitProfile> returnList = new List<UnitProfile>();

            foreach (ICapabilityProvider capabilityProvider in capabilityProviders) {
                if (capabilityProvider != null) {
                    returnList.AddRange(capabilityProvider.GetFilteredCapabilities(this).StartingPetList);
                }
            }

            return returnList;
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

        public List<BaseAbilityProperties> GetAbilityList() {
            //Debug.Log("CapabilityConsumerSnapshot.GetAbilityList()");
            List<BaseAbilityProperties> returnList = new List<BaseAbilityProperties>();

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
        /// <param name="newCapabilityConsumerSnapshot"></param>
        /// <returns></returns>
        public List<BaseAbilityProperties> GetAbilitiesToRemove(CapabilityConsumerSnapshot newCapabilityConsumerSnapshot) {
            List<BaseAbilityProperties> returnList = new List<BaseAbilityProperties>();

            List<BaseAbilityProperties> currentList = GetAbilityList();

            returnList.AddRange(currentList.Except(newCapabilityConsumerSnapshot.GetAbilityList()));

            return returnList;
        }

        /// <summary>
        /// return a list of abilities to add when provided with an incoming capability consumer
        /// </summary>
        /// <param name="newCapabilityConsumerSnapshot"></param>
        /// <returns></returns>
        public List<BaseAbilityProperties> GetAbilitiesToAdd(CapabilityConsumerSnapshot newCapabilityConsumerSnapshot, CharacterAbilityManager sourceAbilityManager) {
            List<BaseAbilityProperties> returnList = new List<BaseAbilityProperties>();

            List<BaseAbilityProperties> currentList = GetAbilityList();

            returnList.AddRange(newCapabilityConsumerSnapshot.GetAbilityList().Except(currentList));

            /*
             // commented out because it was causing abilities to be re-learned immediately after unlearning
            // why was it querying the rawability list anyway?  that's only for save manager
            foreach (BaseAbilityProperties baseAbility in GetAbilityList()) {
                if (sourceAbilityManager.RawAbilityList.ContainsValue(baseAbility) == false) {
                    sourceAbilityManager.LearnAbility(baseAbility);
                }
            }
            */

            // add abilities learning from items that won't show up in the main snapshot
            // testing : re-doing the above code to hopefully work better
            // this is not necessary because the abilities that are not from a provider are never unlearned, they just become hidden
            // because the ability list is always filtered
            // the side effect of this is that they become available again and show up back in the spellbook, but must be manually re-added to the bars
            // after a class switch
            /*
            foreach (BaseAbilityProperties baseAbilityProperties in sourceAbilityManager.RawAbilityList.Values) {
                if (returnList.Contains(baseAbilityProperties) == false
                    && baseAbilityProperties.CanLearnAbility(sourceAbilityManager) == true
                    //&& baseAbilityProperties.CharacterClassRequirementIsMet
                    ) {
                    returnList.Add(baseAbilityProperties);
                }
            }
            */

            return returnList;
        }

    }

}