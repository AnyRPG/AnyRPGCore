using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Unit Type", menuName = "AnyRPG/UnitType")]
    [System.Serializable]
    public class UnitType : DescribableResource, IStatProvider, IAbilityProvider {

        [Header("Abilities and Traits")]

        [Tooltip("When no weapons are equippped to learn auto-attack abilities from, this auto-attack ability will be used")]
        [SerializeField]
        private string defaultAutoAttackAbilityName = string.Empty;

        private BaseAbility defaultAutoAttackAbility = null;

        [Tooltip("Abilities this unit will know")]
        [FormerlySerializedAs("learnedAbilityNames")]
        [SerializeField]
        private List<string> abilityNames = new List<string>();

        private List<BaseAbility> abilityList = new List<BaseAbility>();

        [Tooltip("Traits are status effects which are automatically active at all times if the level requirement is met.")]
        [SerializeField]
        private List<string> traitNames = new List<string>();

        private List<StatusEffect> traitList = new List<StatusEffect>();

        [Header("Capabilities")]

        [Tooltip("Weapon skills known by this unit type")]
        [FormerlySerializedAs("weaponSkillList")]
        [SerializeField]
        private List<string> weaponSkills = new List<string>();

        // reference to the actual weapon skills
        private List<WeaponSkill> weaponSkillList = new List<WeaponSkill>();

        [Header("Stats and Scaling")]

        [Tooltip("Stats available to this unit, in addition to the stats defined at the system level that all character use")]
        [FormerlySerializedAs("statScaling")]
        [SerializeField]
        private List<StatScalingNode> primaryStats = new List<StatScalingNode>();

        [Header("Power Resources")]

        [Tooltip("Power Resources used by this unit.  The first resource is considered primary and will show on the unit frame.")]
        [SerializeField]
        private List<string> powerResources = new List<string>();

        // reference to the actual power resources
        private List<PowerResource> powerResourceList = new List<PowerResource>();

        public List<PowerResource> PowerResourceList { get => powerResourceList; set => powerResourceList = value; }
        public List<StatScalingNode> PrimaryStats { get => primaryStats; set => primaryStats = value; }
        public List<WeaponSkill> WeaponSkillList { get => weaponSkillList; set => weaponSkillList = value; }
        public BaseAbility DefaultAutoAttackAbility { get => defaultAutoAttackAbility; set => defaultAutoAttackAbility = value; }
        public List<BaseAbility> AbilityList { get => abilityList; set => abilityList = value; }
        public List<StatusEffect> TraitList { get => traitList; set => traitList = value; }

        public override void SetupScriptableObjects() {

            base.SetupScriptableObjects();

            defaultAutoAttackAbility = null;
            if (defaultAutoAttackAbilityName != null && defaultAutoAttackAbilityName != string.Empty) {
                BaseAbility tmpDefaultAutoAttackAbility = SystemAbilityManager.MyInstance.GetResource(defaultAutoAttackAbilityName);
                if (tmpDefaultAutoAttackAbility != null) {
                    defaultAutoAttackAbility = tmpDefaultAutoAttackAbility;
                } else {
                    Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability : " + defaultAutoAttackAbilityName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

            abilityList = new List<BaseAbility>();
            if (abilityNames != null) {
                foreach (string baseAbilityName in abilityNames) {
                    BaseAbility baseAbility = SystemAbilityManager.MyInstance.GetResource(baseAbilityName);
                    if (baseAbility != null) {
                        if ((baseAbility is AnimatedAbility) && (baseAbility as AnimatedAbility).IsAutoAttack == true && defaultAutoAttackAbility == null) {
                            defaultAutoAttackAbility = baseAbility;
                        }
                        abilityList.Add(baseAbility);
                    } else {
                        Debug.LogError("CharacterClass.SetupScriptableObjects(): Could not find ability : " + baseAbilityName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            traitList = new List<StatusEffect>();
            if (traitNames != null) {
                foreach (string traitName in traitNames) {
                    StatusEffect abilityEffect = SystemAbilityEffectManager.MyInstance.GetResource(traitName) as StatusEffect;
                    if (abilityEffect != null) {
                        traitList.Add(abilityEffect);
                    } else {
                        Debug.LogError("CharacterClass.SetupScriptableObjects(): Could not find ability effect : " + traitName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            weaponSkillList = new List<WeaponSkill>();
            if (weaponSkills != null) {
                foreach (string weaponSkillName in weaponSkills) {
                    WeaponSkill weaponSkill = SystemWeaponSkillManager.MyInstance.GetResource(weaponSkillName);
                    if (weaponSkill != null) {
                        weaponSkillList.Add(weaponSkill);
                    } else {
                        Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find weapon Skill : " + weaponSkillName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            powerResourceList = new List<PowerResource>();
            if (powerResources != null) {
                foreach (string powerResourcename in powerResources) {
                    PowerResource tmpPowerResource = SystemPowerResourceManager.MyInstance.GetResource(powerResourcename);
                    if (tmpPowerResource != null) {
                        powerResourceList.Add(tmpPowerResource);
                    } else {
                        Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find power resource : " + powerResourcename + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            foreach (StatScalingNode statScalingNode in primaryStats) {
                statScalingNode.SetupScriptableObjects();
            }

        }
    }

}