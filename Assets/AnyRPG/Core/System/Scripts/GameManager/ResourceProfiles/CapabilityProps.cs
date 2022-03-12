using AnyRPG;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnyRPG {

    [System.Serializable]
    public class CapabilityProps {

        [Header("Abilities and Traits")]

        [Tooltip("Abilities learned")]
        [FormerlySerializedAs("learnedAbilityNames")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(BaseAbility))]
        protected List<string> abilityNames = new List<string>();

        protected List<BaseAbilityProperties> abilityList = new List<BaseAbilityProperties>();

        [Tooltip("Traits are status effects which are automatically active at all times if the level requirement is met.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(StatusEffect))]
        protected List<string> traitNames = new List<string>();

        protected List<StatusEffect> traitList = new List<StatusEffect>();

        [Header("Equipment")]

        [Tooltip("Armor classes that can be equipped by this class")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(ArmorClass))]
        private List<string> armorClassList = new List<string>();

        [Tooltip("Weapon skills known by this class")]
        [FormerlySerializedAs("weaponSkillList")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(WeaponSkill))]
        private List<string> weaponSkills = new List<string>();

        // reference to the actual weapon skills
        private List<WeaponSkill> weaponSkillList = new List<WeaponSkill>();

        public List<BaseAbilityProperties> AbilityList { get => abilityList; set => abilityList = value; }
        public List<StatusEffect> TraitList { get => traitList; set => traitList = value; }
        public List<string> ArmorClassList { get => armorClassList; set => armorClassList = value; }
        public List<WeaponSkill> WeaponSkillList { get => weaponSkillList; set => weaponSkillList = value; }

        public CapabilityProps Join(CapabilityProps capabilityProps) {
            CapabilityProps returnValue = new CapabilityProps();
            returnValue.AbilityList.AddRange(abilityList);
            returnValue.AbilityList.AddRange(capabilityProps.AbilityList);
            returnValue.TraitList.AddRange(traitList);
            returnValue.TraitList.AddRange(capabilityProps.TraitList);
            returnValue.WeaponSkillList.AddRange(weaponSkillList);
            returnValue.WeaponSkillList.AddRange(capabilityProps.WeaponSkillList);
            returnValue.ArmorClassList.AddRange(armorClassList);
            returnValue.ArmorClassList.AddRange(capabilityProps.ArmorClassList);
            return returnValue;
        }

        public void SetupScriptableObjects(SystemDataFactory systemDataFactory) {

            if (abilityNames != null) {
                foreach (string baseAbilityName in abilityNames) {
                    if (baseAbilityName != null && baseAbilityName != string.Empty) {
                        BaseAbility baseAbility = systemDataFactory.GetResource<BaseAbility>(baseAbilityName);
                        if (baseAbility != null) {
                            abilityList.Add(baseAbility.AbilityProperties);
                        } else {
                            Debug.LogError("CapabilityProps.SetupScriptableObjects(): Could not find ability : " + baseAbilityName + " while inititalizing capabilityProps.  CHECK INSPECTOR");
                        }
                    } else {
                        Debug.LogError("CapabilityProps.SetupScriptableObjects(): null or empty ability found while inititalizing capabilityProps.  CHECK INSPECTOR");
                    }
                }
            }

            if (traitNames != null) {
                foreach (string traitName in traitNames) {
                    if (traitName != null && traitName != string.Empty) {
                        StatusEffect statusEffect = systemDataFactory.GetResource<AbilityEffect>(traitName) as StatusEffect;
                        if (statusEffect != null) {
                            traitList.Add(statusEffect);
                        } else {
                            Debug.LogError("CapabilityProps.SetupScriptableObjects(): Could not find status effect : " + traitName + " while inititalizing capabilityProps.  CHECK INSPECTOR");
                        }
                    } else {
                        Debug.LogError("CapabilityProps.SetupScriptableObjects(): null or empty status effect found while inititalizing capabilityProps.  CHECK INSPECTOR");
                    }
                }
            }

            if (weaponSkills != null) {
                foreach (string weaponSkillName in weaponSkills) {
                    if (weaponSkillName != null && weaponSkillName != string.Empty) {
                        WeaponSkill weaponSkill = systemDataFactory.GetResource<WeaponSkill>(weaponSkillName);
                        if (weaponSkill != null) {
                            weaponSkillList.Add(weaponSkill);
                        } else {
                            Debug.LogError("CapabilityProps.SetupScriptableObjects(): Could not find weapon Skill : " + weaponSkillName + " while inititalizing capabilityProps.  CHECK INSPECTOR");
                        }
                    } else {
                        Debug.LogError("CapabilityProps.SetupScriptableObjects(): null or empty weapon Skill found while inititalizing capabilityProps.  CHECK INSPECTOR");
                    }
                }
            }


        }
    }

  

}