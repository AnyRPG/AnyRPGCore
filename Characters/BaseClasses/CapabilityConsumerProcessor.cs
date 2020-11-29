using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    public class CapabilityConsumerProcessor {

        ICapabilityConsumer capabilityConsumer = null;

        private List<ICapabilityProvider> capabilityProviders = new List<ICapabilityProvider>();

        public CapabilityConsumerProcessor(ICapabilityConsumer capabilityConsumer) {
            this.capabilityConsumer = capabilityConsumer;
        }

        public void UpdateCapabilityProviderList() {
            capabilityProviders = new List<ICapabilityProvider>();
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
            foreach (ICapabilityProvider capabilityProvider in capabilityProviders) {
                CapabilityProps capabilityProps = capabilityProvider.GetFilteredCapabilities(capabilityConsumer);
                if (capabilityProps.WeaponSkillList.Contains(weapon.WeaponSkill)) {
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
                if (capabilityProps.ArmorClassList.Contains(armor.DisplayName)) {
                    return true;
                }
            }
            return false;
        }

    }

}