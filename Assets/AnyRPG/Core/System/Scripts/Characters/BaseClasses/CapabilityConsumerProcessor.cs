using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    public class CapabilityConsumerProcessor : ConfiguredClass {

        ICapabilityConsumer capabilityConsumer = null;

        private List<ICapabilityProvider> capabilityProviders = new List<ICapabilityProvider>();

        public CapabilityConsumerProcessor(ICapabilityConsumer capabilityConsumer, SystemGameManager systemGameManager) {
            this.capabilityConsumer = capabilityConsumer;
            Configure(systemGameManager);
        }

        public void UpdateCapabilityProviderList() {
            capabilityProviders = new List<ICapabilityProvider>();
            capabilityProviders.Add(systemConfigurationManager);
            if (capabilityConsumer.UnitProfile != null) {
                capabilityProviders.Add(capabilityConsumer.UnitProfile);
            }
            if (capabilityConsumer.UnitType != null) {
                capabilityProviders.Add(capabilityConsumer.UnitType);
            }
            if (capabilityConsumer.CharacterRace != null) {
                capabilityProviders.Add(capabilityConsumer.CharacterRace);
            }
            if (capabilityConsumer.CharacterClass != null) {
                capabilityProviders.Add(capabilityConsumer.CharacterClass);
            }
            if (capabilityConsumer.ClassSpecialization != null) {
                capabilityProviders.Add(capabilityConsumer.ClassSpecialization);
            }
            if (capabilityConsumer.Faction != null) {
                capabilityProviders.Add(capabilityConsumer.Faction);
            }
        }

        /// <summary>
        /// query a capability consumer's capability providers for a weapon skill
        /// </summary>
        /// <param name="weapon"></param>
        /// <returns></returns>
        public bool IsWeaponSupported(Weapon weapon) {
            if (weapon.WeaponSkill == null || weapon.RequireWeaponSkill == false) {
                return true;
            }
            return IsWeaponSkillSupported(weapon.WeaponSkill);
        }

        public bool IsWeaponSkillSupported(WeaponSkill weaponSkill) {
            foreach (ICapabilityProvider capabilityProvider in capabilityProviders) {
                CapabilityProps capabilityProps = capabilityProvider.GetFilteredCapabilities(capabilityConsumer);
                if (capabilityProps.WeaponSkillList.Contains(weaponSkill)) {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// query a capability consumer's capability providers for an armor class
        /// </summary>
        /// <param name="armor"></param>
        /// <returns></returns>
        public bool IsArmorSupported(Armor armor) {
            foreach (ICapabilityProvider capabilityProvider in capabilityProviders) {
                CapabilityProps capabilityProps = capabilityProvider.GetFilteredCapabilities(capabilityConsumer);
                if (capabilityProps.ArmorClassList.Contains(armor.ArmorClass.ResourceName)) {
                    return true;
                }
            }
            return false;
        }

    }

}